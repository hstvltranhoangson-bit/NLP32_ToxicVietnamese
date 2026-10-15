// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

public class PhongDetailsViewModel
{
    public PhongItemViewModel Phong { get; set; } = new();
    public List<NguoiDangOViewModel> NguoiDangO { get; set; } = new();
    public bool LaAdmin { get; set; }
}
