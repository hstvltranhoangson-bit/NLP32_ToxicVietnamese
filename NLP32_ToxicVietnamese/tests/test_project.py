"""pytest tests/ - kiểm thử dữ liệu, logic mô hình và đầu ra."""
import pandas as pd
import pytest
from src.preprocessing import clean_text
from src.metrics import scores, choose_threshold
from src.baseline import train_baseline, BaselinePredictor
from src.data_loader import load_split


def test_clean_text_preserves_signal():
    assert clean_text('  Sao\n vậy 😕   ') == 'Sao vậy 😕'
    assert clean_text('') == ''


def test_scores_and_threshold():
    r = scores([0, 1, 0, 1], [0.1, 0.8, 0.2, 0.9])
    assert r['confusion_matrix'] == [[2, 0], [0, 2]]
    assert 0.2 <= choose_threshold([0, 1], [0.1, 0.9]) <= 0.8


def test_loader_validation(tmp_path):
    pd.DataFrame({'Comment': ['Tốt quá', 'Tệ thật'], 'Toxicity': [0, 1]}).to_csv(tmp_path/'train.csv', index=False)
    assert list(load_split(tmp_path, 'train')['label']) == [0, 1]
    pd.DataFrame({'Comment': [''], 'Toxicity': [1]}).to_csv(tmp_path/'test.csv', index=False)
    with pytest.raises(ValueError): load_split(tmp_path, 'test')


def test_end_to_end_baseline(tmp_path):
    train = pd.DataFrame({'text': ['Cảm ơn bạn', 'Bạn rất tốt', 'Bài rất hay', 'Giỏi quá',
                                  'Đừng xúc phạm người khác', 'Bạn thật đáng ghét', 'Câu nói quá thô lỗ', 'Đồ kém cỏi'],
                          'label': [0, 0, 0, 0, 1, 1, 1, 1]})
    valid = pd.DataFrame({'text': ['Rất hay', 'Thô lỗ quá', 'Cảm ơn', 'Đáng ghét'], 'label': [0, 1, 0, 1]})
    train_baseline(train, valid, tmp_path)
    result = BaselinePredictor(tmp_path/'model.joblib').predict('Tôi rất vui')
    assert result['label'] in (0, 1)
    assert 0 <= result['toxicity_probability'] <= 1
