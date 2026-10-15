// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>Controller cơ sở: cung cấp thông tin người dùng hiện tại (đã được filter phân quyền xác thực từ DB).</summary>
public abstract class BaseController : Controller
{
    protected string VaiTroHienTai => HttpContext.Items[YeuCauDangNhapAttribute.ItemVaiTro] as string ?? string.Empty;
    protected int MaTaiKhoanHienTai => HttpContext.Items[YeuCauDangNhapAttribute.ItemMaTaiKhoan] is int ma ? ma : 0;
    protected bool LaAdmin => VaiTroHienTai == VaiTro.Admin;

    protected void ThongBaoThanhCong(string noiDung) => TempData["ThanhCong"] = noiDung;
    protected void ThongBaoLoi(string noiDung) => TempData["Loi"] = noiDung;
}
