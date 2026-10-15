// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

public class ThongKeViewModel
{
    public List<ThongKeItem> PhongTheoLoai { get; set; } = new();
    public List<PhongItemViewModel> SinhVienTheoPhong { get; set; } = new();
    public List<ThongKeItem> SinhVienTheoToa { get; set; } = new();
    public List<ThongKeItem> SinhVienTheoLoaiPhong { get; set; } = new();
    public List<ThongKeItem> DangKyTheoTrangThai { get; set; } = new();
    public List<PhongItemViewModel> PhongDongNhat { get; set; } = new();
    public List<ThongKeItem> SinhVienVaoTheoThang { get; set; } = new();
    public int TongSoCho { get; set; }
    public int ChoDangSuDung { get; set; }
    public double TyLeSuDung { get; set; }
}
