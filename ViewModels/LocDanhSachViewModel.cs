// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Điều kiện lọc đơn giản dùng chung cho các danh sách (tìm theo từ khóa, lọc trạng thái, phân trang).</summary>
public class LocDanhSachViewModel
{
    public string? TuKhoa { get; set; }
    public string? TrangThai { get; set; }
    public int Trang { get; set; } = 1;

    public Dictionary<string, string> ToRouteValues()
    {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(TuKhoa)) d["TuKhoa"] = TuKhoa.Trim();
        if (!string.IsNullOrWhiteSpace(TrangThai)) d["TrangThai"] = TrangThai;
        return d;
    }
}
