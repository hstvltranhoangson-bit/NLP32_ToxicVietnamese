// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>
/// Một dòng phòng kèm các giá trị TÍNH TOÁN từ dữ liệu thực tế:
/// SucChua (từ loại phòng), SoDangO (đếm PhanPhong Đang ở), SoChoCon = SucChua - SoDangO.
/// </summary>
public class PhongItemViewModel
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public int MaLoaiPhong { get; set; }
    public string TenLoaiPhong { get; set; } = string.Empty;
    public decimal DonGiaThang { get; set; }
    public string ToaNha { get; set; } = string.Empty;
    public int Tang { get; set; }
    public string GioiTinhPhong { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public int SucChua { get; set; }
    public int SoDangO { get; set; }
    public int SoChoCon { get; set; }

    public bool ConCho => SoChoCon > 0;
    public double TyLeSuDung => SucChua <= 0 ? 0 : Math.Round(SoDangO * 100.0 / SucChua, 1);
    public string NhanChon => $"{TenPhong} - {TenLoaiPhong} - {ToaNha}, tầng {Tang} - Phòng {GioiTinhPhong} - còn {SoChoCon}/{SucChua} chỗ";
}
