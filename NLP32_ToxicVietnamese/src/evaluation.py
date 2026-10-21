"""Kiểm thử với ngưỡng khóa từ validation, lưu lỗi/ma trận nhầm lẫn."""
import csv
import json
import numpy as np
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from sklearn.metrics import ConfusionMatrixDisplay
from .metrics import scores


def evaluate(predictor, df, outdir: Path, split='test'):
    outdir.mkdir(parents=True, exist_ok=True)
    outputs = [predictor.predict(text) for text in df['text']]
    prob = [r['toxicity_probability'] for r in outputs]
    report = scores(df['label'].to_numpy(), prob, predictor.threshold)
    report.update({'split': split, 'model': outputs[0]['model'], 'threshold_source': 'valid'})
    (outdir / f'{split}_metrics.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    with (outdir / f'{split}_predictions.csv').open('w', newline='', encoding='utf-8-sig') as f:
        writer = csv.writer(f)
        writer.writerow(['id', 'text', 'actual', 'predicted', 'prob_toxic', 'correct', 'analysis_note'])
        for row, pred in zip(df.itertuples(index=False), outputs):
            writer.writerow([row.id, row.text, row.label, pred['label'],
                             round(pred['toxicity_probability'], 6), int(row.label == pred['label']), ''])
    fig, ax = plt.subplots(figsize=(5.5, 4.4))
    ConfusionMatrixDisplay(confusion_matrix=np.asarray(report['confusion_matrix']), display_labels=['Không độc hại', 'Độc hại']).plot(ax=ax, cmap='Blues', colorbar=False)
    ax.set_title(f'Ma trận nhầm lẫn - {split} - {len(df)} mẫu')
    fig.tight_layout()
    fig.savefig(outdir / f'{split}_confusion_matrix.png', dpi=160)
    plt.close(fig)
    return report
