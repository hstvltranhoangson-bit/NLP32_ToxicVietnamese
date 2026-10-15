// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>CRUD loại phòng - chỉ Admin (kiểm tra quyền bằng filter trước khi chạy Action).</summary>
[YeuCauDangNhap(VaiTro.Admin)]
public class LoaiPhongController : BaseController
{
    private readonly AppDbContext _db;

    public LoaiPhongController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var ds = await _db.LoaiPhongs.AsNoTracking()
            .Select(l => new LoaiPhongDongViewModel { Loai = l, SoPhong = l.PhongKTXs.Count })
            .OrderBy(x => x.Loai.SoNguoiToiDa).ThenBy(x => x.Loai.TenLoaiPhong)
            .ToListAsync();
        return View(ds);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _db.LoaiPhongs.AsNoTracking().Include(l => l.PhongKTXs).FirstOrDefaultAsync(l => l.MaLoaiPhong == id);
        return loai == null ? NotFound() : View(loai);
    }

    public IActionResult Create() => View(new LoaiPhong());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenLoaiPhong,SoNguoiToiDa,DonGiaThang,MoTa,TrangThai")] LoaiPhong model)
    {
        model.TenLoaiPhong = (model.TenLoaiPhong ?? string.Empty).Trim();
        if (await _db.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == model.TenLoaiPhong))
            ModelState.AddModelError(nameof(model.TenLoaiPhong), "Tên loại phòng đã tồn tại.");
        if (!TrangThaiChung.TatCa.Contains(model.TrangThai))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái không hợp lệ.");

        if (!ModelState.IsValid) return View(model);

        _db.LoaiPhongs.Add(model);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã thêm loại phòng mới.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _db.LoaiPhongs.FindAsync(id);
        return loai == null ? NotFound() : View(loai);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaLoaiPhong,TenLoaiPhong,SoNguoiToiDa,DonGiaThang,MoTa,TrangThai")] LoaiPhong model)
    {
        if (id != model.MaLoaiPhong) return NotFound();
        var entity = await _db.LoaiPhongs.FindAsync(id);
        if (entity == null) return NotFound();

        model.TenLoaiPhong = (model.TenLoaiPhong ?? string.Empty).Trim();
        if (await _db.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == model.TenLoaiPhong && l.MaLoaiPhong != id))
            ModelState.AddModelError(nameof(model.TenLoaiPhong), "Tên loại phòng đã tồn tại.");
        if (!TrangThaiChung.TatCa.Contains(model.TrangThai))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái không hợp lệ.");

        // Không cho giảm sức chứa xuống thấp hơn số người đang ở thực tế của các phòng thuộc loại này
        var soDangO = await _db.PhongKTXs.Where(p => p.MaLoaiPhong == id)
            .Select(p => p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)).ToListAsync();
        var maxDangO = soDangO.Count == 0 ? 0 : soDangO.Max();
        if (model.SoNguoiToiDa < maxDangO)
            ModelState.AddModelError(nameof(model.SoNguoiToiDa), $"Hiện có phòng đang có {maxDangO} người ở, số người tối đa không được nhỏ hơn giá trị này.");

        if (!ModelState.IsValid) return View(model);

        entity.TenLoaiPhong = model.TenLoaiPhong;
        entity.SoNguoiToiDa = model.SoNguoiToiDa;
        entity.DonGiaThang = model.DonGiaThang;
        entity.MoTa = model.MoTa;
        entity.TrangThai = model.TrangThai;
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã cập nhật loại phòng.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _db.LoaiPhongs.AsNoTracking().Include(l => l.PhongKTXs).FirstOrDefaultAsync(l => l.MaLoaiPhong == id);
        return loai == null ? NotFound() : View(loai);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var loai = await _db.LoaiPhongs.FindAsync(id);
        if (loai == null) return NotFound();

        var coPhong = await _db.PhongKTXs.AnyAsync(p => p.MaLoaiPhong == id);
        var coNguyenVong = await _db.DangKyKTXs.AnyAsync(d => d.NguyenVongLoaiPhong == id);
        if (coPhong || coNguyenVong)
        {
            ThongBaoLoi("Không thể xóa: loại phòng đang được phòng hoặc đăng ký sử dụng. Hãy chuyển trạng thái sang Khóa thay vì xóa.");
            return RedirectToAction(nameof(Index));
        }

        _db.LoaiPhongs.Remove(loai);
        await _db.SaveChangesAsync();
        ThongBaoThanhCong("Đã xóa loại phòng.");
        return RedirectToAction(nameof(Index));
    }
}
