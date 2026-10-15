// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

/// <summary>Trạng thái đăng ký KTX: Chờ duyệt -> Đã phân phòng -> Đã trả phòng (hoặc Đã hủy).</summary>
public static class TrangThaiDangKy
{
    public const string ChoDuyet = "Chờ duyệt";
    public const string DaPhanPhong = "Đã phân phòng";
    public const string DaTraPhong = "Đã trả phòng";
    public const string DaHuy = "Đã hủy";

    public static readonly string[] TatCa = { ChoDuyet, DaPhanPhong, DaTraPhong, DaHuy };
}
