"""PhoBERT classifier, tách từ trước khi tokenize, train/valid/test riêng biệt."""
import json
import random
from pathlib import Path
import numpy as np
from .config import TrainConfig
from .metrics import scores, choose_threshold
from .preprocessing import segment_for_phobert


def _load_dependencies():
    try:
        import torch
        from torch.utils.data import DataLoader, Dataset
        from transformers import AutoTokenizer, AutoModelForSequenceClassification
    except ImportError as exc:
        raise RuntimeError('Cài requirements-phobert.txt (PyTorch, Transformers, underthesea).') from exc
    return torch, DataLoader, Dataset, AutoTokenizer, AutoModelForSequenceClassification


def _dataloader(df, tokenizer, max_length, batch_size, shuffle, torch, DataLoader):
    texts = [segment_for_phobert(t) for t in df['text'].tolist()]
    enc = tokenizer(texts, padding='max_length', truncation=True, max_length=max_length, return_tensors='pt')
    labels = torch.tensor(df['label'].to_numpy(), dtype=torch.long)
    class EncodedDataset(torch.utils.data.Dataset):
        def __len__(self): return len(labels)
        def __getitem__(self, ix):
            return {**{key: val[ix] for key, val in enc.items()}, 'labels': labels[ix]}
    return DataLoader(EncodedDataset(), batch_size=batch_size, shuffle=shuffle)


def _probabilities(model, loader, torch, device):
    model.eval()
    predicted = []
    with torch.no_grad():
        for batch in loader:
            batch = {k: v.to(device) for k, v in batch.items()}
            logits = model(**batch).logits
            predicted.extend(torch.softmax(logits, dim=-1)[:, 1].cpu().tolist())
    return np.asarray(predicted)


def train_phobert(train, valid, outdir: Path, cfg=None):
    cfg = cfg or TrainConfig()
    torch, DataLoader, _, AutoTokenizer, AutoModel = _load_dependencies()
    torch.manual_seed(cfg.seed); np.random.seed(cfg.seed); random.seed(cfg.seed)
    if torch.cuda.is_available(): torch.cuda.manual_seed_all(cfg.seed)
    device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    outdir.mkdir(parents=True, exist_ok=True)
    tokenizer = AutoTokenizer.from_pretrained(cfg.model_name, use_fast=False)
    model = AutoModel.from_pretrained(cfg.model_name, num_labels=2,
                                     id2label={0:'NON_TOXIC', 1:'TOXIC'},
                                     label2id={'NON_TOXIC':0, 'TOXIC':1}).to(device)
    train_loader = _dataloader(train, tokenizer, cfg.max_length, cfg.batch_size, True, torch, DataLoader)
    val_loader = _dataloader(valid, tokenizer, cfg.max_length, cfg.batch_size, False, torch, DataLoader)
    optimizer = torch.optim.AdamW(model.parameters(), lr=cfg.learning_rate)
    best_f1 = -1.0
    history = []
    for epoch in range(1, cfg.epochs + 1):
        model.train()
        total_loss = 0.0
        for batch in train_loader:
            batch = {k: v.to(device) for k, v in batch.items()}
            optimizer.zero_grad()
            loss = model(**batch).loss
            loss.backward()
            torch.nn.utils.clip_grad_norm_(model.parameters(), 1.0)
            optimizer.step()
            total_loss += float(loss.item())
        prob = _probabilities(model, val_loader, torch, device)
        threshold = choose_threshold(valid['label'], prob)
        report = scores(valid['label'], prob, threshold)
        record = {'epoch': epoch, 'train_loss_sum': total_loss, 'validation': report}
        history.append(record)
        print(f"Epoch {epoch}: validation macro-F1={report['macro_f1']:.4f}")
        if report['macro_f1'] > best_f1:
            best_f1 = report['macro_f1']
            model.save_pretrained(outdir)
            tokenizer.save_pretrained(outdir)
            (outdir / 'metadata.json').write_text(json.dumps({
                'threshold': threshold, 'max_length': cfg.max_length,
                'seed': cfg.seed, 'base_checkpoint': cfg.model_name, 'best_epoch': epoch,
                'validation_metrics': report, 'segmentation': 'underthesea.word_tokenize(format=text)',
                'device_for_training': str(device)
            }, ensure_ascii=False, indent=2), encoding='utf-8')
    (outdir / 'history.json').write_text(json.dumps(history, ensure_ascii=False, indent=2), encoding='utf-8')
    return history


class PhoBERTPredictor:
    def __init__(self, path: Path):
        torch, _, _, AutoTokenizer, AutoModel = _load_dependencies()
        self.torch = torch
        self.path = Path(path)
        meta = json.loads((self.path / 'metadata.json').read_text(encoding='utf-8'))
        self.threshold = float(meta['threshold'])
        self.max_length = int(meta['max_length'])
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.tokenizer = AutoTokenizer.from_pretrained(str(self.path), use_fast=False)
        self.model = AutoModel.from_pretrained(str(self.path)).to(self.device).eval()

    def predict(self, text):
        segmented = segment_for_phobert(text)
        inputs = self.tokenizer(segmented, return_tensors='pt', truncation=True,
                                padding='max_length', max_length=self.max_length)
        with self.torch.no_grad():
            inputs = {k: v.to(self.device) for k, v in inputs.items()}
            logits = self.model(**inputs).logits
            prob = float(self.torch.softmax(logits, dim=-1)[0, 1].item())
        return {'label': int(prob >= self.threshold), 'toxicity_probability': prob,
                'threshold': self.threshold, 'model': 'PhoBERT fine-tuned'}
