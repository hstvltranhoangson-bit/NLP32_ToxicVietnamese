// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public class KetQuaDangNhap
{
    public TaiKhoan? TaiKhoan { get; set; }
    public string ThongBao { get; set; } = string.Empty;
    /// <summary>Tên trường gắn thông báo lỗi (rỗng = lỗi chung).</summary>
    public string Truong { get; set; } = string.Empty;
    public bool ThanhCong => TaiKhoan != null;
}
