"""Chuẩn hóa nhẹ: KHÔNG xóa từ lóng, emoji, dấu câu hoặc lời trích dẫn."""
import re
import unicodedata


def clean_text(text: str) -> str:
    if not isinstance(text, str):
        return ""
    text = unicodedata.normalize("NFC", text)
    text = text.replace("\u200b", "").replace("\ufeff", "")
    text = re.sub(r"\s+", " ", text).strip()
    return text


def segment_for_phobert(text: str) -> str:
    """PhoBERT cần đầu vào được tách từ (vd: sinh_viên); không lặng lẽ bỏ qua."""
    cleaned = clean_text(text)
    if not cleaned:
        return ""
    try:
        from underthesea import word_tokenize
    except ImportError as exc:
        raise RuntimeError("Thiếu underthesea. Cài requirements-phobert.txt") from exc
    return word_tokenize(cleaned, format="text")
