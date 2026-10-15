// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

/// <summary>Giới tính của sinh viên và giới tính quy định của phòng (cùng tập giá trị để so sánh trực tiếp).</summary>
public static class GioiTinh
{
    public const string Nam = "Nam";
    public const string Nu = "Nữ";

    public static readonly string[] TatCa = { Nam, Nu };
}
