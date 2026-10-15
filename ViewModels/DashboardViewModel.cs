// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Các chỉ số tổng quan - tất cả được truy vấn LINQ trực tiếp từ DB tại thời điểm xem.</summary>
public class DashboardViewModel
{
    public int TongSoPhong { get; set; }
    public int TongSinhVien { get; set; }
    public int TongDangKy { get; set; }
    public int DangKyChoDuyet { get; set; }
    public int SinhVienDangO { get; set; }
    public int TongSoCho { get; set; }
    public int ChoDangSuDung { get; set; }
    public int ChoConTrong { get; set; }
    public int SoPhongDay { get; set; }
    /// <summary>Tỷ lệ sử dụng (%) = Chỗ đang sử dụng / Tổng số chỗ x 100 (0 nếu tổng chỗ = 0).</summary>
    public double TyLeSuDung { get; set; }

    public List<ThongKeItem> SinhVienTheoToa { get; set; } = new();
    public List<ThongKeItem> SinhVaoTheoThang { get; set; } = new();
}
