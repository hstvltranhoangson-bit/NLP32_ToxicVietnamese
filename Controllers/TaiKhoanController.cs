// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

public class TaiKhoanController : BaseController
{
    private readonly IAuthService _auth;
    private readonly AppDbContext _db;

    public TaiKhoanController(IAuthService auth, AppDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (HttpContext.Session.LayMaTaiKhoan() != null) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var kq = await _auth.DangNhapAsync(model.TenDangNhap, model.MatKhau);
        if (!kq.ThanhCong || kq.TaiKhoan == null)
        {
            ModelState.AddModelError(kq.Truong, kq.ThongBao);
            return View(model);
        }

        HttpContext.Session.Clear();
        HttpContext.Session.LuuDangNhap(kq.TaiKhoan);   // MaTaiKhoan, HoTen, VaiTro

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return kq.TaiKhoan.VaiTro == VaiTro.Admin
            ? RedirectToAction("Dashboard", "ThongKe")
            : RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();   // xóa Session: không thể dùng lại Session cũ
        return RedirectToAction(nameof(DangNhap));
    }

    public IActionResult TuChoiTruyCap()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    // ----- Quản lý tài khoản (Admin) -----

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Index([FromQuery] LocDanhSachViewModel loc)
    {
        var query = _db.TaiKhoans.AsNoTracking().Include(t => t.SinhVien).AsQueryable();
        if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
        {
            var k = loc.TuKhoa.Trim();
            query = query.Where(t => t.TenDangNhap.Contains(k) || t.HoTen.Contains(k) || t.Email.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(loc.TrangThai)) query = query.Where(t => t.TrangThai == loc.TrangThai);

        var ds = await query.OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap)
                            .ToPagedListAsync(loc.Trang, CauHinhPhanTrang.DanhSachMoiTrang);
        ViewBag.Loc = loc;
        ViewBag.PhanTrang = PhanTrangViewModel.Tao(ds, loc.ToRouteValues());
        return View(ds);
    }

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> DoiTrangThaiKhoa(int id)
    {
        var kq = await _auth.DoiTrangThaiKhoaAsync(id, MaTaiKhoanHienTai);
        if (kq.KhongTimThay) return NotFound();
        if (kq.ThanhCong) ThongBaoThanhCong(kq.ThongBao); else ThongBaoLoi(kq.ThongBao);
        return RedirectToAction(nameof(Index));
    }

    // ----- Đổi mật khẩu (mọi người dùng đã đăng nhập) -----

    [YeuCauDangNhap]
    public IActionResult DoiMatKhau() => View(new DoiMatKhauViewModel());

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var kq = await _auth.DoiMatKhauAsync(MaTaiKhoanHienTai, model.MatKhauCu, model.MatKhauMoi);
        if (!kq.ThanhCong)
        {
            ModelState.AddModelError(nameof(model.MatKhauCu), kq.ThongBao);
            return View(model);
        }
        ThongBaoThanhCong(kq.ThongBao);
        return RedirectToAction("Index", "Home");
    }
}
