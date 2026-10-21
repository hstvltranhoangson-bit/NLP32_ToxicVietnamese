"""TF-IDF + Logistic Regression: so word/char n-gram và class_weight."""
import json
from pathlib import Path
import joblib
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline
from .metrics import scores, choose_threshold


def make_model(analyzer='word', class_weight=None, seed=42):
    if analyzer == 'char':
        vectorizer = TfidfVectorizer(analyzer='char', ngram_range=(2, 5), min_df=1,
                                     max_features=50000, sublinear_tf=True)
    else:
        # Token tiếng Việt theo khoảng trắng; so sánh đặc trưng từ/cụm từ.
        vectorizer = TfidfVectorizer(analyzer='word', ngram_range=(1, 2),
                                     token_pattern=r'(?u)\b\w+\b', min_df=1,
                                     max_features=50000, sublinear_tf=True)
    return Pipeline([
        ('tfidf', vectorizer),
        ('logreg', LogisticRegression(max_iter=1500, class_weight=class_weight,
                                      solver='liblinear', random_state=seed))
    ])


def train_baseline(train, valid, outdir: Path, seed=42):
    if train['label'].nunique() < 2:
        raise ValueError('Train phải có đủ 2 nhãn')
    outdir.mkdir(parents=True, exist_ok=True)
    experiments = []
    best = None
    for analyzer in ('word', 'char'):
        for weight in (None, 'balanced'):
            model = make_model(analyzer, weight, seed)
            model.fit(train['text'], train['label'])
            prob = model.predict_proba(valid['text'])[:, 1]
            threshold = choose_threshold(valid['label'], prob)
            report = scores(valid['label'], prob, threshold)
            name = f"tfidf_{analyzer}_{weight or 'none'}"
            record = {'name': name, 'analyzer': analyzer, 'class_weight': weight, **report}
            experiments.append(record)
            if best is None or report['macro_f1'] > best[0]:
                best = (report['macro_f1'], model, threshold, record)
    _, model, threshold, selected = best
    joblib.dump({'pipeline': model, 'threshold': threshold}, outdir / 'model.joblib')
    manifest = {'data_split_for_selection': 'valid', 'selected': selected,
                'validation_experiments': experiments, 'seed': seed,
                'note': 'Không báo cáo các con số valid như kết quả test.'}
    (outdir / 'validation.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
    return manifest


class BaselinePredictor:
    def __init__(self, path: Path):
        data = joblib.load(path)
        self.pipeline = data['pipeline']
        self.threshold = float(data['threshold'])

    def predict(self, text):
        prob = float(self.pipeline.predict_proba([text])[0, 1])
        return {'label': int(prob >= self.threshold), 'toxicity_probability': prob,
                'threshold': self.threshold, 'model': 'TF-IDF + Logistic Regression'}
