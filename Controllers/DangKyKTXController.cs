// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

[YeuCauDangNhap]
public class DangKyKTXController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IDangKyService _service;

    public DangKyKTXController(AppDbContext db, IDangKyService service)
    {
        _db = db;
        _service = service;
    }

    /// <summary>Mã sinh viên được xác định từ TÀI KHOẢN đăng nhập, không lấy từ URL/Form.</summary>
    private Task<string?> LayMaSinhVienHienTaiAsync() =>
        _db.SinhViens.AsNoTracking().Where(s => s.MaTaiKhoan == MaTaiKhoanHienTai)
            .Select(s => (string?)s.MaSinhVien).FirstOrDefaultAsync();

    // ---------- Sinh viên ----------

    [YeuCauDangNhap(VaiTro.SinhVien)]
    public async Task<IActionResult> Create()
    {
        var vm = new DangKyKtxCreateViewModel();
        await NapLoaiPhongAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.SinhVien)]
    public async Task<IActionResult> Create(DangKyKtxCreateViewModel model)
    {
        var maSv = await LayMaSinhVienHienTaiAsync();
        if (maSv == null) return View("~/Views/SinhVien/ChuaCoHoSo.cshtml");

        if (ModelState.IsValid)
        {
            var kq = await _service.TaoDangKyAsync(maSv, model);
            if (kq.ThanhCong)
            {
                ThongBaoThanhCong(kq.ThongBao);
                return RedirectToAction(nameof(CuaToi));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        await NapLoaiPhongAsync(model);
        return View(model);
    }

    [YeuCauDangNhap(VaiTro.SinhVien)]
    public async Task<IActionResult> CuaToi()
    {
        var vm = await _service.LayThongTinCaNhanAsync(MaTaiKhoanHienTai);
        return vm == null ? View("~/Views/SinhVien/ChuaCoHoSo.cshtml") : View(vm);
    }

    // ---------- Dùng chung (có kiểm tra quyền sở hữu) ----------

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var dk = await _service.LayChiTietAsync(id.Value);
        if (dk == null) return NotFound();

        // Sinh viên đổi ID trên URL để xem đăng ký của người khác -> NotFound
        if (!LaAdmin && dk.MaSinhVien != await LayMaSinhVienHienTaiAsync()) return NotFound();
        return View(dk);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        string? maSv = null;
        if (!LaAdmin)
        {
            maSv = await LayMaSinhVienHienTaiAsync();
            if (maSv == null) return NotFound();
        }

        var kq = await _service.HuyDangKyAsync(id, maSv);
        if (kq.KhongTimThay) return NotFound();
        if (kq.ThanhCong) ThongBaoThanhCong(kq.ThongBao); else ThongBaoLoi(kq.ThongBao);
        return LaAdmin ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(CuaToi));
    }

    // ---------- Admin ----------

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Index([FromQuery] LocDanhSachViewModel loc)
    {
        var ds = await _service.TimKiemAsync(loc);
        ViewBag.Loc = loc;
        ViewBag.PhanTrang = PhanTrangViewModel.Tao(ds, loc.ToRouteValues());
        return View(ds);
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var dk = await _service.LayChiTietAsync(id.Value);
        return dk == null ? NotFound() : View(dk);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dk = await _db.DangKyKTXs.FindAsync(id);
        if (dk == null) return NotFound();

        // Đăng ký đã phân phòng (hoặc đã có lịch sử PhanPhong) không được xóa
        if (dk.TrangThai == TrangThaiDangKy.DaPhanPhong || await _db.PhanPhongs.AnyAsync(p => p.MaDangKy == id))
        {
            ThongBaoLoi("Không thể xóa: đăng ký đã được phân phòng/đã có lịch sử ở. Dữ liệu này cần giữ lại.");
            return RedirectToAction(nameof(Index));
        }

        _db.DangKyKTXs.Remove(dk);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã xóa đăng ký.");
        return RedirectToAction(nameof(Index));
    }

    private async Task NapLoaiPhongAsync(DangKyKtxCreateViewModel vm)
    {
        vm.LoaiPhongs = await _db.LoaiPhongs.AsNoTracking()
            .Where(l => l.TrangThai == TrangThaiChung.HoatDong)
            .OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new SelectListItem
            {
                Value = l.MaLoaiPhong.ToString(),
                Text = l.TenLoaiPhong + " - " + l.SoNguoiToiDa + " người",
                Selected = vm.NguyenVongLoaiPhong == l.MaLoaiPhong
            }).ToListAsync();
    }
}
