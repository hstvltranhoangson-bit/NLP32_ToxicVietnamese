"""Đọc UIT-ViCTSD theo split gốc, kiểm tra schema và rò rỉ dữ liệu."""
from pathlib import Path
import pandas as pd
from .preprocessing import clean_text

FILE_NAMES = {'train': 'train.csv', 'valid': 'valid.csv', 'test': 'test.csv'}


def load_split(folder: Path, split: str) -> pd.DataFrame:
    if split not in FILE_NAMES:
        raise ValueError(f"Split không hợp lệ: {split}")
    path = Path(folder) / FILE_NAMES[split]
    if not path.is_file():
        raise FileNotFoundError(f"Không tìm thấy {path}. Xem README để chuẩn bị dữ liệu.")
    df = pd.read_csv(path, encoding='utf-8-sig')
    # Schema gốc gồm Comment, Toxicity; các trường khác không dùng làm đầu vào.
    if {'Comment', 'Toxicity'}.issubset(df.columns):
        id_series = df['id'] if 'id' in df.columns else (df['Unnamed: 0'] if 'Unnamed: 0' in df.columns else df.index)
        df = pd.DataFrame({'id': id_series, 'text': df['Comment'], 'label': df['Toxicity']})
    elif {'text', 'label'}.issubset(df.columns):
        df = df.copy()
        if 'id' not in df.columns:
            df.insert(0, 'id', [f'{split}-{i}' for i in range(len(df))])
    else:
        raise ValueError(f"{path.name} cần cột Comment/Toxicity hoặc text/label")
    if df['text'].isna().any() or df['label'].isna().any():
        raise ValueError(f"{path.name} có text/label rỗng: phải kiểm tra nguồn, không âm thầm xóa.")
    df['text'] = df['text'].map(clean_text)
    if (df['text'].str.len() == 0).any():
        raise ValueError(f"{path.name} chứa bình luận trống sau tiền xử lý")
    df['label'] = pd.to_numeric(df['label'], errors='raise').astype(int)
    if not set(df['label'].unique()).issubset({0, 1}):
        raise ValueError('Nhãn phải là 0 hoặc 1')
    df['id'] = df['id'].astype(str)
    if df['id'].duplicated().any():
        raise ValueError(f"{path.name}: ID trùng trong cùng split")
    return df[['id', 'text', 'label']].reset_index(drop=True)


def leakage_report(folder: Path) -> dict:
    frames = {s: load_split(folder, s) for s in FILE_NAMES}
    overlap = {}
    splits = list(frames)
    for i, first in enumerate(splits):
        for second in splits[i+1:]:
            left = set(frames[first]['text'].str.casefold())
            right = set(frames[second]['text'].str.casefold())
            overlap[f'{first}/{second}'] = len(left & right)
    return {'counts': {s: len(f) for s, f in frames.items()}, 'same_text_cross_split': overlap,
            'label_counts': {s: f['label'].value_counts().to_dict() for s, f in frames.items()}}
