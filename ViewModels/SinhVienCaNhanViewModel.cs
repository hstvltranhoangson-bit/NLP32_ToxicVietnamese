// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Tổng hợp dữ liệu cá nhân của sinh viên đang đăng nhập (tài khoản, hồ sơ, đăng ký, phòng, lịch sử).</summary>
public class SinhVienCaNhanViewModel
{
    public TaiKhoan TaiKhoan { get; set; } = new();
    public SinhVien SinhVien { get; set; } = new();
    public DangKyKTX? DangKyChoDuyet { get; set; }
    public PhanPhong? PhongHienTai { get; set; }
    public List<PhanPhong> LichSuO { get; set; } = new();
    public List<DangKyKTX> DanhSachDangKy { get; set; } = new();
}
