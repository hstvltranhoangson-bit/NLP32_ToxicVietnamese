# Hệ thống quản lý ký túc xá và phân phòng sinh viên (ASP.NET Core 10 MVC)

Solution: `De_tai_XÂY DỰNG HỆ THỐNG QUẢN LÝ KÝ TÚC XÁ VÀ PHÂN PHÒNG SINH VIÊN.sln`
Project : `QuanLyKyTucXa_UNETI_DHTI17A2ND` 

## Yêu cầu
- .NET 10 SDK, SQL Server (hoặc LocalDB đi kèm Visual Studio)
- Công cụ EF: `dotnet tool install --global dotnet-ef`

## Cài đặt và chạy
1. Mở solution bằng Visual Studio 2022/2026 hoặc VS Code.
2. Sửa Connection String trong `appsettings.json` (`DefaultConnection`) cho đúng máy của bạn.
3. Tạo Migration (lần đầu, lưu vào Git cùng source code):
   ```
   cd QuanLyKyTucXa_UNETI_DHTI17A2ND
   dotnet ef migrations add KhoiTaoCoSoDuLieu
   dotnet ef database update
   ```
   Nếu bỏ qua bước 3 thì khi chạy, chương trình tự tạo CSDL từ Model (EnsureCreated). Khi đã dùng cách này
   mà muốn chuyển sang Migration, hãy xóa database trước (`dotnet ef database drop`) rồi làm lại bước 3.
4. Chạy: `dotnet run` (hoặc F5). Lần chạy đầu tự nạp dữ liệu mẫu (DbSeeder).

## Tài khoản kiểm thử
| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin` / `admin2` | `Admin@123` |
| Sinh viên | `22103100001` ... `22103100030` | `Sv@123456` |

Gợi ý kịch bản: SV `22103100013`-`15`, `27`, `28` có đăng ký Chờ duyệt (Admin phân phòng);
SV `22103100017`, `18`, `30` chưa có đăng ký (đăng ký mới); `22103100001` ở phòng A301 (phòng đã đầy);
phòng A101 và B101 còn đúng 1 chỗ; phòng B302 Tạm ngừng; SV `22103100010` và `25` đã từng chuyển phòng.

## Cấu trúc thư mục
`Constants/` hằng số trạng thái · `Models/` Entity · `Data/` DbContext + dữ liệu mẫu · `Validation/` validation tùy biến ·
`ViewModels/` · `Services/` nghiệp vụ (LINQ) · `Filters/` phân quyền · `Controllers/` · `Views/` · `Helpers/`
