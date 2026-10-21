"""CLI chính: python -m src.main {inspect,train,evaluate,predict,download-data}."""
import argparse
import json
from pathlib import Path
from urllib.request import urlretrieve
from .config import RAW, EXAMPLE, BASELINE_DIR, PHOBERT_DIR, TrainConfig
from .data_loader import load_split, leakage_report
from .baseline import BaselinePredictor, train_baseline
from .preprocessing import clean_text

BASE_URL = 'https://raw.githubusercontent.com/tarudesu/ViCTSD/main/'


def download_dataset(folder: Path, confirm: bool):
    if not confirm:
        raise RuntimeError('Cần --confirm-rights: tự kiểm tra điều kiện sử dụng UIT-ViCTSD trước khi tải.')
    folder.mkdir(parents=True, exist_ok=True)
    for split, origin in [('train', 'ViCTSD_train.csv'), ('valid', 'ViCTSD_valid.csv'), ('test', 'ViCTSD_test.csv')]:
        dest = folder / f'{split}.csv'
        if dest.exists():
            print(f'Đã có {dest}; không ghi đè')
        else:
            urlretrieve(BASE_URL + origin, dest)
            print(f'Đã tải {origin} -> {dest}')


def get_predictor(model):
    if model == 'baseline':
        return BaselinePredictor(BASELINE_DIR / 'model.joblib')
    from .phobert import PhoBERTPredictor
    return PhoBERTPredictor(PHOBERT_DIR)


def main():
    parser = argparse.ArgumentParser(description='UNETI NLP-32 - phát hiện độc hại tiếng Việt')
    sub = parser.add_subparsers(dest='command', required=True)
    for command in ('inspect', 'train', 'evaluate'):
        p = sub.add_parser(command)
        p.add_argument('--data', choices=['real', 'example'], default='real')
        if command != 'inspect':
            p.add_argument('--model', choices=['baseline', 'phobert'], default='baseline')
    pred = sub.add_parser('predict')
    pred.add_argument('--text', required=True)
    pred.add_argument('--model', choices=['baseline', 'phobert'], default='baseline')
    sub.add_parser('compare')
    get_data = sub.add_parser('download-data')
    get_data.add_argument('--confirm-rights', action='store_true')
    args = parser.parse_args()
    if args.command == 'compare':
        from .comparison import compare_test_results
        path, rows = compare_test_results()
        print(json.dumps({'saved': str(path), 'comparison': rows}, ensure_ascii=False, indent=2))
        return
    if args.command == 'download-data':
        download_dataset(RAW, args.confirm_rights)
        return
    if args.command == 'predict':
        t = clean_text(args.text)
        if not t: raise ValueError('Không được nhập bình luận trống')
        print(json.dumps(get_predictor(args.model).predict(t), ensure_ascii=False, indent=2))
        return
    folder = RAW if args.data == 'real' else EXAMPLE
    if args.command == 'inspect':
        print(json.dumps(leakage_report(folder), ensure_ascii=False, indent=2))
        return
    train = load_split(folder, 'train') if args.command == 'train' else None
    valid = load_split(folder, 'valid') if args.command == 'train' else None
    outdir = (BASELINE_DIR if args.model == 'baseline' else PHOBERT_DIR)
    if args.command == 'train':
        if args.model == 'baseline':
            result = train_baseline(train, valid, outdir)
            print('Đã chọn mô hình bằng VALID:', result['selected']['name'])
        else:
            from .phobert import train_phobert
            train_phobert(train, valid, outdir, TrainConfig())
            print('Đã lưu PhoBERT theo validation tại:', outdir)
        (outdir / 'source.json').write_text(json.dumps({'training_dataset': args.data, 'train_split': 'train', 'selected_on': 'valid'}, ensure_ascii=False, indent=2), encoding='utf-8')
    else:
        source = outdir / 'source.json'
        if not source.exists():
            raise RuntimeError('Không thấy source.json: hãy huấn luyện bằng CLI trước khi evaluate.')
        recorded = json.loads(source.read_text(encoding='utf-8'))['training_dataset']
        if args.data != recorded:
            raise RuntimeError(f'Mô hình đã học trên bộ {recorded}, không được đánh giá lẫn với --data {args.data}.')
        from .evaluation import evaluate
        test = load_split(folder, 'test')
        predictor = get_predictor(args.model)
        report = evaluate(predictor, test, outdir, 'test')
        print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
