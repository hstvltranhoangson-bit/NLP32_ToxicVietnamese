"""Giao diện Streamlit phân tích một bình luận tiếng Việt."""
import json
from pathlib import Path
import streamlit as st
from src.config import BASELINE_DIR, PHOBERT_DIR, LABEL_NAMES
from src.preprocessing import clean_text
from src.baseline import BaselinePredictor

st.set_page_config(page_title='UNETI | NLP-32', page_icon='🛡️', layout='centered')
st.markdown('''<style>
.block-container {padding-top:2.2rem;max-width:900px}
.hero{padding:23px 28px;background:linear-gradient(120deg,#112f66,#2464a4);color:white;border-radius:18px;margin-bottom:22px}
.hero h1{font-size:1.85rem;margin:0;color:white}.hero p{margin:8px 0 0;color:#cde6ff}
</style>''', unsafe_allow_html=True)
st.markdown('<div class="hero"><h1>🛡️ NLP-32 | Kiểm duyệt bình luận tiếng Việt</h1><p>UNETI · DHTI17A2ND · Hỗ trợ người kiểm duyệt, không tự động xử phạt</p></div>', unsafe_allow_html=True)
model_kind = st.selectbox('Mô hình', ['Baseline TF–IDF + Logistic Regression', 'PhoBERT (đã fine-tune)'])
text = st.text_area('Nhập bình luận tiếng Việt', height=145, placeholder='Ví dụ: Mình không đồng ý với quan điểm này vì...')
if st.button('🔎 Phân tích bình luận', use_container_width=True, type='primary'):
    cleaned = clean_text(text)
    if not cleaned:
        st.warning('Hãy nhập bình luận có nội dung.')
    else:
        try:
            if model_kind.startswith('Baseline'):
                path = BASELINE_DIR / 'model.joblib'
                if not path.exists(): raise FileNotFoundError('Chưa có baseline. Chạy lệnh train theo README.')
                predictor = BaselinePredictor(path)
            else:
                path = PHOBERT_DIR / 'metadata.json'
                if not path.exists(): raise FileNotFoundError('Chưa có PhoBERT fine-tune. Chạy train --model phobert.')
                from src.phobert import PhoBERTPredictor
                predictor = PhoBERTPredictor(PHOBERT_DIR)
            src = BASELINE_DIR if model_kind.startswith('Baseline') else PHOBERT_DIR
            data_note = src / 'source.json'
            if data_note.exists():
                kind = json.loads(data_note.read_text(encoding='utf-8')).get('training_dataset')
                if kind == 'example': st.warning('MÔ HÌNH CHỈ HỌC DỮ LIỆU MẪU NHÂN TẠO, KHÔNG DÙNG ĐỂ KIỂM DUYỆT THỰC TẾ.')
            result = predictor.predict(cleaned)
            st.metric('Dự đoán', LABEL_NAMES[result['label']])
            st.progress(result['toxicity_probability'], text=f"Điểm xác suất lớp độc hại: {result['toxicity_probability']:.1%}")
            st.caption(f"Mô hình: {result['model']} · Ngưỡng từ validation: {result['threshold']:.2f}")
            if result['label'] == 1: st.warning('Đưa vào hàng chờ nhân viên xem xét, KHÔNG tự động xóa.')
            else: st.info('Mô hình chưa phát hiện độc hại theo ngưỡng đã chọn; vẫn có thể sai.')
        except Exception as exc:
            st.error(str(exc))
st.divider()
st.caption('Lưu ý: Lời trích dẫn, mỉa mai, ngữ cảnh thiếu hoặc từ lóng có thể làm dự đoán sai. Kết quả chỉ hỗ trợ sàng lọc.')
