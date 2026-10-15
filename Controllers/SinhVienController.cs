// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

public class SinhVienController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IDangKyService _dangKyService;

    public SinhVienController(AppDbContext db, IDangKyService dangKyService)
    {
        _db = db;
        _dangKyService = dangKyService;
    }

    // ================= Phía sinh viên: chỉ xem dữ liệu CỦA CHÍNH MÌNH =================
    // Không nhận mã sinh viên từ URL: mã được xác định từ tài khoản đang đăng nhập.

    [YeuCauDangNhap(VaiTro.SinhVien)]
    public async Task<IActionResult> CaNhan()
    {
        var vm = await _dangKyService.LayThongTinCaNhanAsync(MaTaiKhoanHienTai);
        return vm == null ? View("ChuaCoHoSo") : View(vm);
    }

    [YeuCauDangNhap(VaiTro.SinhVien)]
    public async Task<IActionResult> LichSu()
    {
        var vm = await _dangKyService.LayThongTinCaNhanAsync(MaTaiKhoanHienTai);
        return vm == null ? View("ChuaCoHoSo") : View(vm);
    }

    // ================= Phía Admin: quản lý sinh viên =================

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Index([FromQuery] LocDanhSachViewModel loc)
    {
        var query = _db.SinhViens.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
        {
            var k = loc.TuKhoa.Trim();
            query = query.Where(s => s.MaSinhVien.Contains(k) || s.HoTen.Contains(k) || s.Lop.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(loc.TrangThai)) query = query.Where(s => s.TrangThai == loc.TrangThai);

        var trang = await query.OrderBy(s => s.MaSinhVien).ToPagedListAsync(loc.Trang, CauHinhPhanTrang.DanhSachMoiTrang);

        // Xác định những sinh viên của trang hiện tại đang có phòng "Đang ở"
        var maTrang = trang.Items.Select(s => s.MaSinhVien).ToList();
        var dangO = await _db.PhanPhongs
            .Where(p => p.TrangThai == TrangThaiPhanPhong.DangO && maTrang.Contains(p.DangKy!.MaSinhVien))
            .Select(p => p.DangKy!.MaSinhVien)
            .ToListAsync();
        var tapDangO = dangO.ToHashSet();

        var ds = new PagedList<SinhVienDongViewModel>(
            trang.Items.Select(s => new SinhVienDongViewModel { SinhVien = s, DangO = tapDangO.Contains(s.MaSinhVien) }).ToList(),
            trang.TongSo, trang.Trang, trang.KichThuocTrang);

        ViewBag.Loc = loc;
        ViewBag.PhanTrang = PhanTrangViewModel.Tao(ds, loc.ToRouteValues());
        return View(ds);
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Details(string? id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        var vm = await _dangKyService.LayThongTinCaNhanAsync(sv.MaTaiKhoan);
        return vm == null ? NotFound() : View(vm);
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public IActionResult Create() => View(new SinhVienFormViewModel());

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Create(SinhVienFormViewModel model)
    {
        model.LaSua = false;
        model.MaSinhVien = (model.MaSinhVien ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(model.MatKhau))
            ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu ban đầu cho tài khoản sinh viên.");
        if (await _db.SinhViens.AnyAsync(s => s.MaSinhVien == model.MaSinhVien))
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên đã tồn tại.");
        if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.MaSinhVien))
            ModelState.AddModelError(nameof(model.MaSinhVien), "Tên đăng nhập trùng với mã sinh viên đã có tài khoản.");
        KiemTraGiaTriChon(model);

        if (!ModelState.IsValid) return View(model);

        var taiKhoan = new TaiKhoan
        {
            TenDangNhap = model.MaSinhVien,
            MatKhau = MatKhauHelper.BamMatKhau(model.MatKhau!),
            HoTen = model.HoTen.Trim(),
            Email = model.Email.Trim(),
            VaiTro = VaiTro.SinhVien,
            TrangThai = model.TrangThai
        };
        _db.SinhViens.Add(new SinhVien
        {
            MaSinhVien = model.MaSinhVien,
            TaiKhoan = taiKhoan,
            HoTen = model.HoTen.Trim(),
            NgaySinh = model.NgaySinh,
            GioiTinh = model.GioiTinh,
            Lop = model.Lop.Trim(),
            Khoa = model.Khoa.Trim(),
            SoDienThoai = model.SoDienThoai,
            Email = model.Email.Trim(),
            DiaChi = model.DiaChi,
            TrangThai = model.TrangThai
        });
        await _db.SaveChangesAsync();
        ThongBaoThanhCong($"Đã thêm sinh viên {model.HoTen} và tạo tài khoản đăng nhập (tên đăng nhập = mã sinh viên).");
        return RedirectToAction(nameof(Index));
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Edit(string? id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().FirstOrDefaultAsync(s => s.MaSinhVien == id);
        return sv == null ? NotFound() : View(SinhVienFormViewModel.TuEntity(sv));
    }

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Edit(string id, SinhVienFormViewModel model)
    {
        if (id != model.MaSinhVien) return NotFound();
        model.LaSua = true;
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null || sv.TaiKhoan == null) return NotFound();
        KiemTraGiaTriChon(model);

        if (!ModelState.IsValid) return View(model);

        sv.HoTen = model.HoTen.Trim();
        sv.NgaySinh = model.NgaySinh;
        sv.GioiTinh = model.GioiTinh;
        sv.Lop = model.Lop.Trim();
        sv.Khoa = model.Khoa.Trim();
        sv.SoDienThoai = model.SoDienThoai;
        sv.Email = model.Email.Trim();
        sv.DiaChi = model.DiaChi;
        sv.TrangThai = model.TrangThai;

        // Đồng bộ tài khoản; khóa tài khoản không xóa lịch sử đăng ký/phân phòng
        sv.TaiKhoan.HoTen = sv.HoTen;
        sv.TaiKhoan.Email = sv.Email;
        sv.TaiKhoan.TrangThai = model.TrangThai;
        if (!string.IsNullOrWhiteSpace(model.MatKhau)) sv.TaiKhoan.MatKhau = MatKhauHelper.BamMatKhau(model.MatKhau);

        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã cập nhật thông tin sinh viên.");
        return RedirectToAction(nameof(Index));
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();
        var sv = await _db.SinhViens.AsNoTracking().Include(s => s.DangKyKTXs).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        return sv == null ? NotFound() : View(sv);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
        if (sv == null) return NotFound();

        if (await _db.DangKyKTXs.AnyAsync(d => d.MaSinhVien == id))
        {
            ThongBaoLoi("Không thể xóa: sinh viên đã có đăng ký KTX (cần giữ lịch sử). Hãy khóa hồ sơ thay vì xóa.");
            return RedirectToAction(nameof(Index));
        }

        var taiKhoan = sv.TaiKhoan;
        _db.SinhViens.Remove(sv);
        if (taiKhoan != null) _db.TaiKhoans.Remove(taiKhoan);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã xóa sinh viên và tài khoản liên quan.");
        return RedirectToAction(nameof(Index));
    }

    private void KiemTraGiaTriChon(SinhVienFormViewModel model)
    {
        if (!GioiTinh.TatCa.Contains(model.GioiTinh))
            ModelState.AddModelError(nameof(model.GioiTinh), "Giới tính không hợp lệ.");
        if (!TrangThaiChung.TatCa.Contains(model.TrangThai))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái không hợp lệ.");
    }
}
