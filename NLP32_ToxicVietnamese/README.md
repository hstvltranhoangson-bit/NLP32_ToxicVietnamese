# NLP-32 — Phát hiện nội dung độc hại trong văn bản tiếng Việt

**Solution:** `UNETI_DHTI17A2ND` · **Project:** `NLP32_ToxicVietnamese`  
**Học phần:** Xử lý ngôn ngữ tự nhiên (NLP) · **Giảng viên:** Bùi Văn Tân  
**Lớp:** DHTI17A2ND — Trường Đại học Kinh tế – Kỹ thuật Công nghiệp (UNETI)

| Thành viên | Mã sinh viên |
|---|---|
| Nguyễn Văn Cường | 23203100019 |
| Trần Hoàng Sơn | 23203100010 |
| Trần Công Quang Đăng | 23203100040 |
| Hà Việt Anh | 23203100046 |

> **Bản hướng dẫn cập nhật cho Visual Studio 2026 + Developer PowerShell.** Các lệnh ở mục 3 bám sát máy đã kiểm tra có **Python 3.14.3** và **`.venv` được tạo tại thư mục Solution**. Đối với PhoBERT, khuyến nghị Python 3.11 hoặc 3.12 vì khả năng tương thích của một số thư viện trên 3.14 còn phụ thuộc phiên bản gói. Cài đặt thành công cần được xác nhận trên máy sử dụng.

## 1. Tổng quan và cấu trúc thư mục

Hệ thống nhận bình luận tiếng Việt và dự đoán `Không độc hại` (0) hoặc `Độc hại` (1). Project gồm baseline **TF–IDF + Logistic Regression**, tùy chọn mô hình **PhoBERT**, chức năng đánh giá kết quả và giao diện web Streamlit.

```text
UNETI_DHTI17A2ND_NLP32_Solution/
└── UNETI_DHTI17A2ND/                 ← THƯ MỤC SOLUTION
    ├── UNETI_DHTI17A2ND.sln
    ├── .venv/                         ← MÔI TRƯỜNG ẢO (tạo khi cài, không nằm trong ZIP)
    └── NLP32_ToxicVietnamese/        ← THƯ MỤC PROJECT; chạy lệnh tại đây
        ├── NLP32_ToxicVietnamese.pyproj
        ├── README.md
        ├── requirements.txt
        ├── requirements-dev.txt
        ├── requirements-phobert.txt
        ├── app.py                     ← Giao diện Streamlit
        ├── src/
        │   ├── main.py               ← Lệnh inspect/train/evaluate/predict/compare
        │   ├── data_loader.py        ← Đọc dữ liệu
        │   ├── preprocessing.py      ← Chuẩn hóa văn bản
        │   ├── baseline.py           ← TF–IDF + Logistic Regression
        │   ├── phobert.py            ← PhoBERT
        │   ├── metrics.py            ← Các chỉ số đánh giá
        │   ├── evaluation.py         ← Xuất kết quả đánh giá
        │   ├── comparison.py         ← So sánh hai mô hình
        │   └── config.py             ← Cấu hình chung
        ├── data/
        │   ├── example/              ← Dữ liệu GIẢ LẬP để chạy thử
        │   └── raw/                  ← Đặt dữ liệu UIT-ViCTSD thật ở đây
        ├── notebooks/01_eda.ipynb
        ├── tests/test_project.py
        └── outputs/                   ← File mô hình và kết quả sau khi chạy
```

**Phân biệt quan trọng:** `UNETI_DHTI17A2ND` chứa `.sln` và `.venv`; `NLP32_ToxicVietnamese` chứa `requirements.txt`, `app.py` và `src/`. Nếu PowerShell đang ở thư mục project rồi thì **không `cd NLP32_ToxicVietnamese` lần nữa**.

## 2. Mở project trong Visual Studio 2026

1. Giải nén ZIP, mở Visual Studio 2026 và chọn **File → Open → Project/Solution**, sau đó chọn `UNETI_DHTI17A2ND.sln`.
2. Mở **View → Terminal** (hoặc Developer PowerShell) trong Visual Studio.
3. Nếu Visual Studio không tải được project `.pyproj`, kiểm tra trong **Visual Studio Installer → Modify** xem bản đang dùng có hỗ trợ thành phần **Python development** hay không. Việc chạy lệnh trong Terminal vẫn cần một bản Python được cài riêng.
4. Phải chạy các lệnh ở **thư mục project**. Bấm `F5` với file `app.py` không thay thế cho lệnh khởi động Streamlit ở mục 5.

Ví dụ đường dẫn của máy đã dùng khi kiểm tra:

```text
F:\Sora\source\UNETI_DHTI17A2ND_NLP32_Solution\UNETI_DHTI17A2ND
```

Đường dẫn này chỉ là ví dụ; không cần chuyển project sang ổ `F:` nếu máy đang lưu ở ổ khác.

## 3. CHẠY NHANH — ĐÚNG VỚI MÁY VISUAL STUDIO 2026 HIỆN TẠI

**Điều kiện đã xác nhận:** Máy nhận Python bằng đường dẫn trực tiếp sau và trả về `Python 3.14.3`:

```powershell
& "$env:LOCALAPPDATA\Python\pythoncore-3.14-64\python.exe" --version
```

### Bước 1. Từ thư mục Solution, kiểm tra `.venv`

Nếu Terminal đang hiển thị dạng `...\UNETI_DHTI17A2ND>` (chưa có `\NLP32_ToxicVietnamese>`), dùng:

```powershell
Get-Location
Test-Path .\.venv\Scripts\python.exe
```

- Nếu hiện `True`: môi trường `.venv` đã được tạo, **không cần tạo lại**.
- Nếu hiện `False`: tạo môi trường từ Python 3.14.3 bằng lệnh dưới đây:

```powershell
& "$env:LOCALAPPDATA\Python\pythoncore-3.14-64\python.exe" -m venv .venv
```

Nếu đang ở thư mục project, có thể trở về Solution bằng `cd ..` **một lần**, rồi thực hiện thao tác kiểm tra hoặc tạo `.venv` ở trên.

### Bước 2. Chuyển vào project và kiểm tra file yêu cầu

```powershell
cd .\NLP32_ToxicVietnamese
Get-Location
Test-Path .\requirements.txt
```

`Test-Path` phải trả về `True`. Nếu đã ở `...\NLP32_ToxicVietnamese>` thì **bỏ qua lệnh `cd`** và chỉ kiểm tra `Test-Path`.

### Bước 3. Cài thư viện bằng Python của `.venv` ở Solution

**Lưu ý có `..\` ở đầu đường dẫn Python**, vì lúc này Terminal đang ở project còn `.venv` nằm ở thư mục cha:

```powershell
.venv\Scripts\python.exe --version
.venv\Scripts\python.exe -m pip install --upgrade pip
.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Thông báo `A new release of pip is available` chỉ là thông báo nâng cấp, **không phải lỗi**. Nếu quá trình cài thư viện báo lỗi tương thích với Python 3.14, xem mục 4.

### Bước 4. Huấn luyện và kiểm tra bằng dữ liệu mẫu

Chạy lần lượt khi Terminal vẫn đang ở `NLP32_ToxicVietnamese`:

```powershell
.venv\Scripts\python.exe -m src.main inspect --data example
.venv\Scripts\python.exe -m src.main train --data example --model baseline
.venv\Scripts\python.exe -m src.main evaluate --data example --model baseline
.venv\Scripts\python.exe -m src.main predict --model baseline --text "Tôi rất thích bài viết này"
```

Sau khi huấn luyện, các file mô hình và đánh giá được lưu ở `outputs/baseline/`. **Dữ liệu `example` là dữ liệu nhân tạo chỉ để thử chức năng; không sử dụng điểm số này để kết luận chất lượng trên dữ liệu thật.**

### Bước 5. Khởi động giao diện web

```powershell
.venv\Scripts\python.exe -m streamlit run app.py
```

Khi khởi động thành công, Streamlit thường hiển thị `Local URL: http://localhost:8501`. Giữ Terminal đang chạy và mở đường dẫn được Streamlit in ra trong trình duyệt. Chọn baseline để thử phân tích. Muốn kết thúc, nhấn `Ctrl + C` trong Terminal.

> **Có thể copy nhanh theo thứ tự (khi đang ở thư mục Solution và `.venv` đã tồn tại):**
>
> ```powershell
> cd .\NLP32_ToxicVietnamese
> .venv\Scripts\python.exe -m pip install -r requirements.txt
> .venv\Scripts\python.exe -m src.main inspect --data example
> .venv\Scripts\python.exe -m src.main train --data example --model baseline
> .venv\Scripts\python.exe -m src.main evaluate --data example --model baseline
> .venv\Scripts\python.exe -m streamlit run app.py
> ```

## 4. Nếu Python 3.14 gặp lỗi cài thư viện: dùng Python 3.12

Python 3.14.3 hiện đã chạy được lệnh `--version` và tạo `.venv` trên máy, nhưng **điều đó không tự đảm bảo** tất cả thư viện NLP, đặc biệt là PhoBERT/`underthesea`, tương thích. Khi cài đặt thất bại, khuyên dùng Python **3.12 (64-bit)**, rồi tạo một môi trường mới.

Cài Python 3.12 nếu máy chưa có:

```powershell
winget install -e --id Python.Python.3.12
```

Đóng và mở lại Terminal của Visual Studio. Kiểm tra bằng `python --version`, `py -0p` **nếu có**, hoặc gọi đúng đường dẫn `python.exe` của bản 3.12. Nếu cài theo đường dẫn người dùng thường gặp, có thể kiểm tra:

```powershell
& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" --version
```

Lưu ý đây chỉ là **đường dẫn tham khảo**, có thể khác tùy nơi cài. Tại thư mục Solution, tạo môi trường 3.12 tách biệt, **không ghi đè `.venv` 3.14**:

```powershell
& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m venv .venv312
cd .\NLP32_ToxicVietnamese
..\.venv312\Scripts\python.exe -m pip install --upgrade pip
..\.venv312\Scripts\python.exe -m pip install -r requirements.txt
..\.venv312\Scripts\python.exe -m src.main train --data example --model baseline
..\.venv312\Scripts\python.exe -m streamlit run app.py
```

Chỉ dùng các lệnh trên **sau khi đã xác nhận đường dẫn Python 3.12 hợp lệ**. Nếu đang đứng trong Project từ đầu, không chạy lại `cd .\NLP32_ToxicVietnamese`. Trong các lệnh sau, thay `..\.venv\Scripts\python.exe` thành `..\.venv312\Scripts\python.exe` nếu quyết định dùng môi trường 3.12.

## 5. Các lệnh chương trình và kiểm thử

Các lệnh dưới đây được minh họa với `.venv` **nằm tại Solution**, còn Terminal **đang đứng trong project**.

| Mục đích | Lệnh |
|---|---|
| Kiểm tra dữ liệu mẫu | `..\.venv\Scripts\python.exe -m src.main inspect --data example` |
| Huấn luyện baseline dữ liệu mẫu | `..\.venv\Scripts\python.exe -m src.main train --data example --model baseline` |
| Đánh giá baseline dữ liệu mẫu | `..\.venv\Scripts\python.exe -m src.main evaluate --data example --model baseline` |
| Dự đoán một bình luận | `..\.venv\Scripts\python.exe -m src.main predict --model baseline --text "Bình luận mẫu"` |
| Mở giao diện web | `..\.venv\Scripts\python.exe -m streamlit run app.py` |
| Cài bộ công cụ kiểm thử | `..\.venv\Scripts\python.exe -m pip install -r requirements-dev.txt` |
| Chạy kiểm thử tự động | `..\.venv\Scripts\python.exe -m pytest -q` |

**Lưu ý:** `train` phải chạy thành công **trước** `evaluate` hoặc `predict`; giao diện cũng yêu cầu mô hình đã được huấn luyện. Các lệnh batch `run_demo.bat` và `train_baseline.bat` trong bản project gốc gọi `python` qua PATH, nên trên máy hiện tại dễ gặp lỗi; hãy ưu tiên các lệnh sử dụng `.venv` như trên.

## 6. Làm việc với dữ liệu UIT-ViCTSD thật

Sau khi tự kiểm tra nguồn và điều kiện sử dụng dữ liệu tại <https://github.com/tarudesu/ViCTSD>, có thể:

```powershell
..\.venv\Scripts\python.exe -m src.main download-data --confirm-rights
```

Hoặc đặt ba file CSV đúng tên `train.csv`, `valid.csv`, `test.csv` vào `data/raw/`. Mã đọc hai cột `Comment` và `Toxicity`; giữ nguyên chia tập train/validation/test và không đưa dữ liệu test vào bước chọn mô hình.

```powershell
..\.venv\Scripts\python.exe -m src.main inspect --data real
..\.venv\Scripts\python.exe -m src.main train --data real --model baseline
..\.venv\Scripts\python.exe -m src.main evaluate --data real --model baseline
```

Sau khi chuyển từ dữ liệu mẫu sang dữ liệu thật, **huấn luyện lại** bằng `--data real`. Chương trình kiểm tra thông tin nguồn dữ liệu đã dùng để train và không cho `evaluate` chéo giữa `example` với `real`.

## 7. Tùy chọn PhoBERT (mô hình chính)

Ưu tiên **Python 3.11/3.12** và GPU; trên CPU quá trình huấn luyện có thể rất lâu. Nếu sử dụng `.venv312` ở Solution, chạy trong thư mục project:

```powershell
.venv312\Scripts\python.exe -m pip install -r requirements-phobert.txt
.venv312\Scripts\python.exe -m src.main train --data real --model phobert
.venv312\Scripts\python.exe -m src.main evaluate --data real --model phobert
.venv312\Scripts\python.exe -m src.main compare
.venv312\Scripts\python.exe -m src.main predict --model phobert --text "Bình luận mẫu"
```

Lệnh `compare` có ý nghĩa **sau khi cả baseline và PhoBERT đều được đánh giá trên cùng bộ test dữ liệu thật**. Việc tải checkpoint `vinai/phobert-base` lần đầu cần kết nối Internet. Nếu chưa có Python 3.12 hoặc GPU, có thể hoàn tất baseline trước rồi làm phần PhoBERT sau. Không báo cáo số liệu PhoBERT nếu nhóm chưa huấn luyện và đánh giá thật.

## 8. Các lỗi thường gặp trên Visual Studio Developer PowerShell

### Lỗi 1 — `Python was not found` hoặc `py : The term 'py' is not recognized`

**Nguyên nhân:** Terminal không tìm thấy `python` hoặc `py` qua PATH; Visual Studio không tự đồng nghĩa với việc Python đã được khai báo trong PATH.  
**Khắc phục:** Kiểm tra bản Python đã cài bằng đường dẫn đầy đủ:

```powershell
& "$env:LOCALAPPDATA\Python\pythoncore-3.14-64\python.exe" --version
```

Sau khi tạo `.venv`, gọi `..\.venv\Scripts\python.exe` từ project; **không bắt buộc dùng lệnh `py`**.

### Lỗi 2 — `cd : Cannot find path ...\NLP32_ToxicVietnamese\NLP32_ToxicVietnamese`

**Nguyên nhân:** Đang ở trong `NLP32_ToxicVietnamese` nhưng lại `cd` tiếp vào chính nó.  
**Khắc phục:** Chạy `Get-Location`. Nếu đường dẫn đã kết thúc bằng `\NLP32_ToxicVietnamese`, **không cần `cd` nữa**.

### Lỗi 3 — `Could not open requirements file: ... 'requirements.txt'`

**Nguyên nhân:** Bạn đang ở **Solution**, trong khi `requirements.txt` nằm trong **Project**.  
**Khắc phục:** Từ Solution chạy:

```powershell
cd .\NLP32_ToxicVietnamese
..\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Hoặc giữ nguyên ở Solution và chỉ định đường dẫn file: `-r .\NLP32_ToxicVietnamese\requirements.txt`.

### Lỗi 4 — `.venv\Scripts\activate : The module '.venv' could not be loaded`

**Nguyên nhân:** Dùng sai cú pháp PowerShell hoặc chưa tạo được `.venv`.  
**Khắc phục:** Không cần `activate` nếu gọi trực tiếp `..\.venv\Scripts\python.exe`. Nếu muốn kích hoạt khi đang ở project và `.venv` nằm tại Solution, dùng:

```powershell
..\.venv\Scripts\Activate.ps1
```

Nếu PowerShell chặn script, **không cần thay đổi ExecutionPolicy**: quay về cách gọi trực tiếp `..\.venv\Scripts\python.exe`.

### Lỗi 5 — `No module named streamlit` / `No module named src`

- `No module named streamlit`: cài `requirements.txt` bằng **đúng Python trong `.venv`** rồi chạy Streamlit bằng Python đó.
- `No module named src`: chạy `Get-Location`, chuyển vào **thư mục project** có `src/`, `app.py` trước khi gọi `python -m src.main`.

### Lỗi 6 — Không có mô hình / không có dữ liệu thật

- Giao diện báo chưa có baseline: chạy `train --data example --model baseline` trước.
- `evaluate` báo thiếu `source.json`: cần chạy `train` qua CLI trước khi đánh giá.
- `--data real` báo thiếu CSV: chuẩn bị `data/raw/train.csv`, `valid.csv`, `test.csv`, hoặc tải theo đúng điều kiện nguồn.
- `A new release of pip is available`: chỉ là thông báo nâng cấp, không phải lỗi.

## 9. Kết quả và giới hạn sử dụng

- `outputs/baseline/`: model baseline, cấu hình chọn bằng validation, metric/test prediction sau thực nghiệm.
- `outputs/phobert/`: checkpoint và kết quả PhoBERT sau khi **tự huấn luyện**.
- Dữ liệu `example` chỉ dùng thử khả năng chạy đầu-cuối, **không chứng minh** chất lượng phát hiện độc hại ngoài thực tế.
- Kết quả dự đoán nhằm **hỗ trợ người kiểm duyệt**, không tự động xóa hay xử phạt bình luận. Những trường hợp mỉa mai, trích dẫn, tiếng lóng hoặc thiếu ngữ cảnh có thể bị dự đoán sai.

## 10. Tài liệu tham khảo

1. Bùi Văn Tân (2026), *Danh sách và hướng dẫn thực hiện đề tài bài tập nhóm NLP*, UNETI, mục 4.32 (NLP-32).
2. UIT-ViCTSD: <https://github.com/tarudesu/ViCTSD>.
3. VinAI PhoBERT: <https://github.com/VinAIResearch/PhoBERT>.

---

**Ghi chú cập nhật:** README này tổng hợp những lỗi phát sinh khi chạy project bằng **Visual Studio 2026 Developer PowerShell**. Chỉ các bước `--version` và tạo `.venv` bằng Python 3.14.3 đã được người dùng xác nhận thành công trong buổi cấu hình; cài đặt đầy đủ thư viện và huấn luyện trên máy đó vẫn cần thực hiện và kiểm tra đầu ra.
