"""Tổng hợp kết quả test của hai mô hình cùng nguồn dữ liệu; không tính lại metric."""
import csv
import json
from pathlib import Path
from .config import BASELINE_DIR, PHOBERT_DIR, OUTPUT


def compare_test_results():
    rows = []
    sources = set()
    for name, folder in [('Baseline TF-IDF', BASELINE_DIR), ('PhoBERT fine-tuned', PHOBERT_DIR)]:
        metrics_file = folder / 'test_metrics.json'
        source_file = folder / 'source.json'
        if not metrics_file.exists():
            print(f'Bỏ qua {name}: chưa chạy evaluate trên test.')
            continue
        if not source_file.exists():
            raise RuntimeError(f'{name}: thiếu thông tin nguồn dữ liệu. Hãy train lại bằng CLI.')
        source = json.loads(source_file.read_text(encoding='utf-8'))['training_dataset']
        sources.add(source)
        metrics = json.loads(metrics_file.read_text(encoding='utf-8'))
        rows.append({'model': name, 'dataset': source, 'n_test': metrics['n'],
                     'macro_f1': metrics['macro_f1'],
                     'precision_toxic': metrics['precision_toxic'],
                     'recall_toxic': metrics['recall_toxic'],
                     'pr_auc_toxic': metrics['pr_auc_toxic'],
                     'threshold': metrics['threshold']})
    if len(sources) > 1:
        raise RuntimeError('Không so sánh model từ hai dataset khác nhau: train lại trên cùng real/example.')
    if not rows:
        raise RuntimeError('Chưa có kết quả test. Hãy evaluate ít nhất một mô hình.')
    if len({r['n_test'] for r in rows}) > 1:
        raise RuntimeError('Số mẫu test không bằng nhau. Kiểm tra nguồn và split.')
    dest = OUTPUT / 'comparison_test.csv'
    with dest.open('w', encoding='utf-8-sig', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=rows[0].keys())
        writer.writeheader()
        writer.writerows(rows)
    return dest, rows
