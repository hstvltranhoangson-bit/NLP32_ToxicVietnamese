// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>Kết quả xử lý nghiệp vụ: thành công hay thất bại kèm thông báo rõ nguyên nhân.</summary>
public class KetQuaXuLy
{
    public bool ThanhCong { get; private set; }
    public string ThongBao { get; private set; } = string.Empty;
    /// <summary>True nếu bản ghi không tồn tại (hoặc không thuộc về người dùng) - Controller trả NotFound.</summary>
    public bool KhongTimThay { get; private set; }

    public static KetQuaXuLy Ok(string thongBao) => new() { ThanhCong = true, ThongBao = thongBao };
    public static KetQuaXuLy Loi(string thongBao) => new() { ThanhCong = false, ThongBao = thongBao };
    public static KetQuaXuLy NotFound(string thongBao = "Không tìm thấy dữ liệu.") => new() { ThanhCong = false, KhongTimThay = true, ThongBao = thongBao };
}
