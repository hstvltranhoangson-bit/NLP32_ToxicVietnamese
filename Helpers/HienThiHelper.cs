// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

/// <summary>Hàm hỗ trợ hiển thị trên View (màu nhãn trạng thái).</summary>
public static class HienThiHelper
{
    public static string LopBadge(string? trangThai) => trangThai switch
    {
        TrangThaiChung.HoatDong or TrangThaiPhong.DangSuDung or TrangThaiPhanPhong.DangO or TrangThaiDangKy.DaPhanPhong => "bg-success",
        TrangThaiDangKy.ChoDuyet => "bg-warning text-dark",
        TrangThaiChung.Khoa or TrangThaiPhong.TamNgung or TrangThaiDangKy.DaHuy => "bg-danger",
        TrangThaiPhanPhong.DaChuyen => "bg-info text-dark",
        _ => "bg-secondary"
    };
}
