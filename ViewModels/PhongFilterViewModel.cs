// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Toàn bộ điều kiện truy vấn danh sách phòng: tìm kiếm + lọc + sắp xếp + phân trang.</summary>
public class PhongFilterViewModel
{
    // Tìm kiếm
    public string? TuKhoa { get; set; }

    // Lọc
    public int? MaLoaiPhong { get; set; }
    public string? ToaNha { get; set; }
    public int? Tang { get; set; }
    public string? GioiTinhPhong { get; set; }
    public string? TrangThai { get; set; }
    /// <summary>"con" = còn chỗ, "het" = hết chỗ.</summary>
    public string? ConCho { get; set; }

    // Sắp xếp: ten_asc | ten_desc | tang_asc | tang_desc | cho_asc | cho_desc
    public string? SapXep { get; set; } = "ten_asc";

    // Phân trang
    public int Trang { get; set; } = 1;

    /// <summary>Dựng route values để các liên kết phân trang giữ nguyên điều kiện truy vấn.</summary>
    public Dictionary<string, string> ToRouteValues()
    {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(TuKhoa)) d["TuKhoa"] = TuKhoa.Trim();
        if (MaLoaiPhong.HasValue) d["MaLoaiPhong"] = MaLoaiPhong.Value.ToString();
        if (!string.IsNullOrWhiteSpace(ToaNha)) d["ToaNha"] = ToaNha;
        if (Tang.HasValue) d["Tang"] = Tang.Value.ToString();
        if (!string.IsNullOrWhiteSpace(GioiTinhPhong)) d["GioiTinhPhong"] = GioiTinhPhong;
        if (!string.IsNullOrWhiteSpace(TrangThai)) d["TrangThai"] = TrangThai;
        if (!string.IsNullOrWhiteSpace(ConCho)) d["ConCho"] = ConCho;
        if (!string.IsNullOrWhiteSpace(SapXep)) d["SapXep"] = SapXep;
        return d;
    }
}
