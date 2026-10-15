// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>Danh sách/chi tiết phòng: mọi người dùng đăng nhập. Thêm/sửa/xóa/đổi trạng thái: chỉ Admin.</summary>
[YeuCauDangNhap]
public class PhongKTXController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IPhongService _phongService;

    public PhongKTXController(AppDbContext db, IPhongService phongService)
    {
        _db = db;
        _phongService = phongService;
    }

    // GET: /PhongKTX?TuKhoa=A&ToaNha=Tòa A&SapXep=cho_desc&Trang=2
    public async Task<IActionResult> Index([FromQuery] PhongFilterViewModel filter)
    {
        if (!LaAdmin)
        {
            // Sinh viên chỉ xem các phòng còn khả năng tiếp nhận (không phụ thuộc tham số URL)
            filter.TrangThai = TrangThaiPhong.DangSuDung;
            filter.ConCho = "con";
        }

        var vm = await _phongService.TimKiemAsync(filter);
        vm.LaAdmin = LaAdmin;
        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var phong = await _phongService.LayChiTietAsync(id.Value);
        if (phong == null) return NotFound();

        var vm = new PhongDetailsViewModel { Phong = phong, LaAdmin = LaAdmin };
        if (LaAdmin) vm.NguoiDangO = await _phongService.LayNguoiDangOAsync(id.Value);   // SV không xem danh tính người ở
        return View(vm);
    }

    // ---------- Chức năng quản trị ----------

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Create()
    {
        await NapLoaiPhongAsync();
        return View(new PhongKTX());
    }

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Create([Bind("TenPhong,MaLoaiPhong,ToaNha,Tang,GioiTinhPhong,TrangThai,MoTa")] PhongKTX model)
    {
        model.TenPhong = (model.TenPhong ?? string.Empty).Trim();
        await KiemTraNghiepVuAsync(model, null);

        if (!ModelState.IsValid)
        {
            await NapLoaiPhongAsync(model.MaLoaiPhong);   // nạp lại select khi Validation lỗi
            return View(model);
        }

        _db.PhongKTXs.Add(model);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã thêm phòng mới.");
        return RedirectToAction(nameof(Index));
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var phong = await _db.PhongKTXs.FindAsync(id);
        if (phong == null) return NotFound();
        await NapLoaiPhongAsync(phong.MaLoaiPhong);
        return View(phong);
    }

    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Edit(int id, [Bind("MaPhong,TenPhong,MaLoaiPhong,ToaNha,Tang,GioiTinhPhong,TrangThai,MoTa")] PhongKTX model)
    {
        if (id != model.MaPhong) return NotFound();
        var entity = await _db.PhongKTXs.FindAsync(id);
        if (entity == null) return NotFound();

        model.TenPhong = (model.TenPhong ?? string.Empty).Trim();
        await KiemTraNghiepVuAsync(model, id);

        if (!ModelState.IsValid)
        {
            await NapLoaiPhongAsync(model.MaLoaiPhong);
            return View(model);
        }

        entity.TenPhong = model.TenPhong;
        entity.MaLoaiPhong = model.MaLoaiPhong;
        entity.ToaNha = model.ToaNha.Trim();
        entity.Tang = model.Tang;
        entity.GioiTinhPhong = model.GioiTinhPhong;
        entity.TrangThai = model.TrangThai;
        entity.MoTa = model.MoTa;
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã cập nhật phòng.");
        return RedirectToAction(nameof(Index));
    }

    [YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var phong = await _phongService.LayChiTietAsync(id.Value);
        return phong == null ? NotFound() : View(phong);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var phong = await _db.PhongKTXs.FindAsync(id);
        if (phong == null) return NotFound();

        // Không xóa phòng đã phát sinh lịch sử phân phòng
        if (await _db.PhanPhongs.AnyAsync(p => p.MaPhong == id))
        {
            ThongBaoLoi("Không thể xóa: phòng đã phát sinh lịch sử phân phòng. Hãy chuyển trạng thái sang Tạm ngừng.");
            return RedirectToAction(nameof(Index));
        }

        _db.PhongKTXs.Remove(phong);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã xóa phòng.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Cập nhật nhanh trạng thái phòng: Đang sử dụng <-> Tạm ngừng.</summary>
    [HttpPost, ValidateAntiForgeryToken, YeuCauDangNhap(VaiTro.Admin)]
    public async Task<IActionResult> DoiTrangThai(int id, [FromQuery] string? quayLai = null)
    {
        var phong = await _db.PhongKTXs.FindAsync(id);
        if (phong == null) return NotFound();

        phong.TrangThai = phong.TrangThai == TrangThaiPhong.DangSuDung ? TrangThaiPhong.TamNgung : TrangThaiPhong.DangSuDung;
        await _db.SaveChangesAsync();
        ThongBaoThanhCong($"Phòng {phong.TenPhong} chuyển sang trạng thái {phong.TrangThai}.");

        if (!string.IsNullOrEmpty(quayLai) && Url.IsLocalUrl(quayLai)) return LocalRedirect(quayLai);
        return RedirectToAction(nameof(Index));
    }

    // ---------- Hỗ trợ ----------

    private async Task NapLoaiPhongAsync(int? maDangChon = null)
    {
        var ds = await _db.LoaiPhongs.AsNoTracking()
            .Where(l => l.TrangThai == TrangThaiChung.HoatDong || l.MaLoaiPhong == maDangChon)
            .OrderBy(l => l.SoNguoiToiDa)
            .Select(l => new SelectListItem
            {
                Value = l.MaLoaiPhong.ToString(),
                Text = l.TenLoaiPhong + " (" + l.SoNguoiToiDa + " người)",
                Selected = l.MaLoaiPhong == maDangChon
            }).ToListAsync();
        ViewBag.LoaiPhongs = ds;
    }

    /// <summary>Các quy tắc phụ thuộc dữ liệu: kiểm tra bằng LINQ tại Controller.</summary>
    private async Task KiemTraNghiepVuAsync(PhongKTX model, int? maDangSua)
    {
        if (await _db.PhongKTXs.AnyAsync(p => p.TenPhong == model.TenPhong && p.MaPhong != (maDangSua ?? 0)))
            ModelState.AddModelError(nameof(model.TenPhong), "Tên phòng đã tồn tại.");

        var loai = await _db.LoaiPhongs.AsNoTracking().FirstOrDefaultAsync(l => l.MaLoaiPhong == model.MaLoaiPhong);
        if (loai == null)
            ModelState.AddModelError(nameof(model.MaLoaiPhong), "Loại phòng không tồn tại.");

        if (!GioiTinh.TatCa.Contains(model.GioiTinhPhong))
            ModelState.AddModelError(nameof(model.GioiTinhPhong), "Giới tính phòng không hợp lệ.");
        if (!TrangThaiPhong.TatCa.Contains(model.TrangThai))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái phòng không hợp lệ.");

        if (maDangSua.HasValue && loai != null)
        {
            var dangO = await _db.PhanPhongs.CountAsync(p => p.MaPhong == maDangSua.Value && p.TrangThai == TrangThaiPhanPhong.DangO);
            if (dangO > loai.SoNguoiToiDa)
                ModelState.AddModelError(nameof(model.MaLoaiPhong), $"Phòng đang có {dangO} người ở, vượt sức chứa của loại phòng mới ({loai.SoNguoiToiDa}).");

            var gtCu = await _db.PhongKTXs.AsNoTracking().Where(p => p.MaPhong == maDangSua.Value).Select(p => p.GioiTinhPhong).FirstOrDefaultAsync();
            if (dangO > 0 && gtCu != null && gtCu != model.GioiTinhPhong)
                ModelState.AddModelError(nameof(model.GioiTinhPhong), "Phòng đang có sinh viên ở nên không thể đổi giới tính phòng.");
        }
    }
}
