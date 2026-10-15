// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;

/// <summary>
/// Phân quyền tại Controller/Action (không chỉ ẩn menu trên View).
/// - Chưa đăng nhập -> chuyển về trang đăng nhập.
/// - Tài khoản đã bị xóa/khóa -> xóa Session cũ và yêu cầu đăng nhập lại.
/// - Sai vai trò -> trang từ chối truy cập (403).
/// Vai trò được lấy từ DATABASE theo tài khoản đang đăng nhập, không lấy từ dữ liệu client gửi lên.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class YeuCauDangNhapAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string ItemVaiTro = "VaiTroHienTai";
    public const string ItemMaTaiKhoan = "MaTaiKhoanHienTai";

    private readonly string[] _vaiTroChoPhep;

    public YeuCauDangNhapAttribute(params string[] vaiTroChoPhep)
    {
        _vaiTroChoPhep = vaiTroChoPhep;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var maTaiKhoan = http.Session.LayMaTaiKhoan();

        if (maTaiKhoan == null)
        {
            context.Result = ChuyenDenDangNhap(http);
            return;
        }

        var db = http.RequestServices.GetRequiredService<AppDbContext>();
        var tk = await db.TaiKhoans.AsNoTracking()
            .Where(t => t.MaTaiKhoan == maTaiKhoan.Value)
            .Select(t => new { t.VaiTro, t.TrangThai })
            .FirstOrDefaultAsync();

        if (tk == null || tk.TrangThai != TrangThaiChung.HoatDong)
        {
            http.Session.Clear();
            context.Result = ChuyenDenDangNhap(http);
            return;
        }

        http.Items[ItemVaiTro] = tk.VaiTro;
        http.Items[ItemMaTaiKhoan] = maTaiKhoan.Value;

        if (_vaiTroChoPhep.Length > 0 && !_vaiTroChoPhep.Contains(tk.VaiTro))
        {
            context.Result = new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", null);
        }
    }

    private static IActionResult ChuyenDenDangNhap(HttpContext http)
    {
        object? route = HttpMethods.IsGet(http.Request.Method)
            ? new { returnUrl = http.Request.Path + http.Request.QueryString }
            : null;
        return new RedirectToActionResult("DangNhap", "TaiKhoan", route);
    }
}
