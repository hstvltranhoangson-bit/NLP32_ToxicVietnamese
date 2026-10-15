// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public interface IAuthService
{
    Task<KetQuaDangNhap> DangNhapAsync(string tenDangNhap, string matKhau);
    Task<KetQuaXuLy> DoiMatKhauAsync(int maTaiKhoan, string matKhauCu, string matKhauMoi);
    Task<KetQuaXuLy> DoiTrangThaiKhoaAsync(int maTaiKhoanCanDoi, int maTaiKhoanThucHien);
}
