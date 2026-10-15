// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

/// <summary>Duyệt đăng ký, phân phòng, chuyển phòng, trả phòng, xem lịch sử - chỉ Admin.</summary>
[YeuCauDangNhap(VaiTro.Admin)]
public class PhanPhongController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IPhanPhongService _service;

    public PhanPhongController(AppDbContext db, IPhanPhongService service)
    {
        _db = db;
        _service = service;
    }

    /// <summary>Danh sách sinh viên đang ở (có nút Chuyển phòng / Trả phòng).</summary>
    public async Task<IActionResult> Index([FromQuery] LocDanhSachViewModel loc)
    {
        var ds = await _service.LayDangOAsync(loc);
        ViewBag.Loc = loc;
        ViewBag.PhanTrang = PhanTrangViewModel.Tao(ds, loc.ToRouteValues());
        return View(ds);
    }

    public async Task<IActionResult> ChoDuyet() => View(await _service.LayDangKyChoDuyetAsync());

    public async Task<IActionResult> LichSu([FromQuery] LocDanhSachViewModel loc)
    {
        var ds = await _service.LayLichSuAsync(loc);
        ViewBag.Loc = loc;
        ViewBag.PhanTrang = PhanTrangViewModel.Tao(ds, loc.ToRouteValues());
        return View(ds);
    }

    // ---------------- Phân phòng ----------------

    public async Task<IActionResult> Phan(int? id)
    {
        if (id == null) return NotFound();
        var vm = await TaoPhanPhongVmAsync(id.Value);
        if (vm == null) return NotFound();
        if (vm.TrangThaiDangKy != TrangThaiDangKy.ChoDuyet)
        {
            ThongBaoLoi("Chỉ phân phòng cho đăng ký ở trạng thái Chờ duyệt.");
            return RedirectToAction(nameof(ChoDuyet));
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Phan(int id, PhanPhongViewModel model)
    {
        if (id != model.MaDangKy) return NotFound();

        if (ModelState.IsValid)
        {
            var kq = await _service.PhanPhongAsync(id, model.MaPhong, model.NgayVao!.Value);
            if (kq.KhongTimThay) return NotFound();
            if (kq.ThanhCong)
            {
                ThongBaoThanhCong(kq.ThongBao);
                return RedirectToAction(nameof(ChoDuyet));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);   // báo rõ nguyên nhân
        }

        var nap = await TaoPhanPhongVmAsync(id);
        if (nap == null) return NotFound();
        nap.MaPhong = model.MaPhong;
        nap.NgayVao = model.NgayVao;
        return View(nap);
    }

    // ---------------- Chuyển phòng ----------------

    public async Task<IActionResult> Chuyen(int? id)
    {
        if (id == null) return NotFound();
        var vm = await TaoChuyenPhongVmAsync(id.Value);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Chuyen(int id, ChuyenPhongViewModel model)
    {
        if (id != model.MaPhanPhong) return NotFound();

        if (ModelState.IsValid)
        {
            var kq = await _service.ChuyenPhongAsync(id, model.MaPhongMoi, model.NgayChuyen!.Value);
            if (kq.KhongTimThay) return NotFound();
            if (kq.ThanhCong)
            {
                ThongBaoThanhCong(kq.ThongBao);
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        var nap = await TaoChuyenPhongVmAsync(id);
        if (nap == null) return NotFound();
        nap.MaPhongMoi = model.MaPhongMoi;
        nap.NgayChuyen = model.NgayChuyen;
        return View(nap);
    }

    // ---------------- Trả phòng ----------------

    public async Task<IActionResult> Tra(int? id)
    {
        if (id == null) return NotFound();
        var pp = await _service.LayPhanPhongAsync(id.Value);
        if (pp == null) return NotFound();
        return View(TaoTraPhongVm(pp));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Tra(int id, TraPhongViewModel model)
    {
        if (id != model.MaPhanPhong) return NotFound();

        if (ModelState.IsValid)
        {
            var kq = await _service.TraPhongAsync(id, model.NgayRa!.Value);
            if (kq.KhongTimThay) return NotFound();
            if (kq.ThanhCong)
            {
                ThongBaoThanhCong(kq.ThongBao);
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, kq.ThongBao);
        }

        var pp = await _service.LayPhanPhongAsync(id);
        if (pp == null) return NotFound();
        var nap = TaoTraPhongVm(pp);
        nap.NgayRa = model.NgayRa;
        return View(nap);
    }

    // ---------------- Dựng ViewModel (nạp lại danh sách phòng mỗi lần hiển thị) ----------------

    private async Task<PhanPhongViewModel?> TaoPhanPhongVmAsync(int maDangKy)
    {
        var dk = await _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.SinhVien).Include(d => d.LoaiPhongNguyenVong)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
        if (dk?.SinhVien == null) return null;

        return new PhanPhongViewModel
        {
            MaDangKy = dk.MaDangKy,
            TrangThaiDangKy = dk.TrangThai,
            MaSinhVien = dk.MaSinhVien,
            HoTen = dk.SinhVien.HoTen,
            GioiTinh = dk.SinhVien.GioiTinh,
            NguyenVong = dk.LoaiPhongNguyenVong?.TenLoaiPhong,
            NgayBatDau = dk.NgayBatDau,
            NgayKetThucDuKien = dk.NgayKetThucDuKien,
            NgayVao = dk.NgayBatDau < DateTime.Today ? DateTime.Today : dk.NgayBatDau,
            PhongCoTheChon = await _service.LayPhongCoTheChonAsync()
        };
    }

    private async Task<ChuyenPhongViewModel?> TaoChuyenPhongVmAsync(int maPhanPhong)
    {
        var pp = await _service.LayPhanPhongAsync(maPhanPhong);
        if (pp?.DangKy?.SinhVien == null || pp.Phong == null) return null;

        return new ChuyenPhongViewModel
        {
            MaPhanPhong = pp.MaPhanPhong,
            TrangThaiHienTai = pp.TrangThai,
            MaSinhVien = pp.DangKy.MaSinhVien,
            HoTen = pp.DangKy.SinhVien.HoTen,
            GioiTinh = pp.DangKy.SinhVien.GioiTinh,
            MaPhongHienTai = pp.MaPhong,
            TenPhongHienTai = pp.Phong.TenPhong,
            NgayVaoHienTai = pp.NgayVao,
            NgayChuyen = DateTime.Today,
            PhongCoTheChon = await _service.LayPhongCoTheChonAsync(pp.MaPhong)
        };
    }

    private static TraPhongViewModel TaoTraPhongVm(Models.PhanPhong pp) => new()
    {
        MaPhanPhong = pp.MaPhanPhong,
        TrangThaiHienTai = pp.TrangThai,
        MaSinhVien = pp.DangKy?.MaSinhVien ?? string.Empty,
        HoTen = pp.DangKy?.SinhVien?.HoTen ?? string.Empty,
        TenPhong = pp.Phong?.TenPhong ?? string.Empty,
        NgayVao = pp.NgayVao,
        NgayRa = DateTime.Today
    };
}
