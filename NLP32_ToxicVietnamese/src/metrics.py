"""Metrics và chọn ngưỡng CHỈ trên validation."""
import numpy as np
from sklearn.metrics import (accuracy_score, f1_score, precision_score, recall_score,
                             average_precision_score, confusion_matrix)


def scores(y_true, prob, threshold=0.5):
    y_true = np.asarray(y_true, dtype=int)
    prob = np.asarray(prob, dtype=float)
    pred = (prob >= threshold).astype(int)
    cm = confusion_matrix(y_true, pred, labels=[0, 1])
    result = {
        'n': int(len(y_true)), 'threshold': float(threshold),
        'macro_f1': float(f1_score(y_true, pred, average='macro', zero_division=0)),
        'accuracy': float(accuracy_score(y_true, pred)),
        'precision_toxic': float(precision_score(y_true, pred, pos_label=1, zero_division=0)),
        'recall_toxic': float(recall_score(y_true, pred, pos_label=1, zero_division=0)),
        'confusion_matrix': cm.tolist(),
    }
    result['pr_auc_toxic'] = float(average_precision_score(y_true, prob)) if len(np.unique(y_true)) == 2 else None
    return result


def choose_threshold(y_true, prob):
    """Ngưỡng tối đa Macro-F1 trên validation; hòa điểm lấy ngưỡng nhỏ nhất."""
    candidates = np.linspace(0.2, 0.8, 61)
    candidates = np.unique(np.append(candidates, 0.5))
    options = [(scores(y_true, prob, float(t))['macro_f1'], -float(t), float(t)) for t in candidates]
    return max(options)[2]
