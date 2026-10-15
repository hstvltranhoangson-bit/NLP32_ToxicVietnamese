// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

/// <summary>Trạng thái bản ghi lịch sử phân phòng.</summary>
public static class TrangThaiPhanPhong
{
    public const string DangO = "Đang ở";
    public const string DaChuyen = "Đã chuyển";
    public const string DaTraPhong = "Đã trả phòng";

    public static readonly string[] TatCa = { DangO, DaChuyen, DaTraPhong };
}
