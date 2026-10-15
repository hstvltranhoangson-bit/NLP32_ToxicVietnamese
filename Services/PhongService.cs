// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public class PhongService : IPhongService
{
    private readonly AppDbContext _db;

    public PhongService(AppDbContext db) => _db = db;

    /// <summary>
    /// Số chỗ còn KHÔNG lưu trong DB mà được tính:
    ///   SoChoCon = LoaiPhong.SoNguoiToiDa - COUNT(PhanPhong có TrangThai = Đang ở của phòng đó)
    /// </summary>
    public IQueryable<PhongItemViewModel> TruyVanPhong()
    {
        return _db.PhongKTXs.AsNoTracking().Select(p => new PhongItemViewModel
        {
            MaPhong = p.MaPhong,
            TenPhong = p.TenPhong,
            MaLoaiPhong = p.MaLoaiPhong,
            TenLoaiPhong = p.LoaiPhong!.TenLoaiPhong,
            DonGiaThang = p.LoaiPhong!.DonGiaThang,
            ToaNha = p.ToaNha,
            Tang = p.Tang,
            GioiTinhPhong = p.GioiTinhPhong,
            TrangThai = p.TrangThai,
            MoTa = p.MoTa,
            SucChua = p.LoaiPhong!.SoNguoiToiDa,
            SoDangO = p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO),
            SoChoCon = p.LoaiPhong!.SoNguoiToiDa - p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO)
        });
    }

    /// <summary>Thứ tự xử lý: Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Phân trang -> Hiển thị.</summary>
    public async Task<PhongListViewModel> TimKiemAsync(PhongFilterViewModel f)
    {
        // 1. Truy vấn gốc
        var query = TruyVanPhong();

        // 2. Tìm kiếm theo tên phòng hoặc tòa nhà
        if (!string.IsNullOrWhiteSpace(f.TuKhoa))
        {
            var tuKhoa = f.TuKhoa.Trim();
            query = query.Where(p => p.TenPhong.Contains(tuKhoa) || p.ToaNha.Contains(tuKhoa));
        }

        // 3. Lọc kết hợp nhiều điều kiện
        if (f.MaLoaiPhong.HasValue) query = query.Where(p => p.MaLoaiPhong == f.MaLoaiPhong.Value);
        if (!string.IsNullOrWhiteSpace(f.ToaNha)) query = query.Where(p => p.ToaNha == f.ToaNha);
        if (f.Tang.HasValue) query = query.Where(p => p.Tang == f.Tang.Value);
        if (!string.IsNullOrWhiteSpace(f.GioiTinhPhong)) query = query.Where(p => p.GioiTinhPhong == f.GioiTinhPhong);
        if (!string.IsNullOrWhiteSpace(f.TrangThai)) query = query.Where(p => p.TrangThai == f.TrangThai);
        if (f.ConCho == "con") query = query.Where(p => p.SoChoCon > 0);       // dựa trên số SV Đang ở
        else if (f.ConCho == "het") query = query.Where(p => p.SoChoCon <= 0);

        // 4. Sắp xếp (kể cả theo số chỗ còn - là giá trị tính toán)
        IOrderedQueryable<PhongItemViewModel> sapXep = f.SapXep switch
        {
            "ten_desc" => query.OrderByDescending(p => p.TenPhong),
            "tang_asc" => query.OrderBy(p => p.Tang).ThenBy(p => p.TenPhong),
            "tang_desc" => query.OrderByDescending(p => p.Tang).ThenBy(p => p.TenPhong),
            "cho_asc" => query.OrderBy(p => p.SoChoCon).ThenBy(p => p.TenPhong),
            "cho_desc" => query.OrderByDescending(p => p.SoChoCon).ThenBy(p => p.TenPhong),
            _ => query.OrderBy(p => p.TenPhong)
        };

        // 5. Phân trang ngay trên truy vấn EF Core (Skip/Take)
        var ketQua = await sapXep.ToPagedListAsync(f.Trang, CauHinhPhanTrang.PhongMoiTrang);
        f.Trang = ketQua.Trang;

        var vm = new PhongListViewModel { KetQua = ketQua, Filter = f };
        await NapDanhMucAsync(vm);
        return vm;
    }

    private async Task NapDanhMucAsync(PhongListViewModel vm)
    {
        var f = vm.Filter;

        vm.LoaiPhongs = await _db.LoaiPhongs.AsNoTracking().OrderBy(l => l.TenLoaiPhong)
            .Select(l => new SelectListItem { Value = l.MaLoaiPhong.ToString(), Text = l.TenLoaiPhong, Selected = f.MaLoaiPhong == l.MaLoaiPhong })
            .ToListAsync();

        var toaNhas = await _db.PhongKTXs.AsNoTracking().Select(p => p.ToaNha).Distinct().OrderBy(t => t).ToListAsync();
        vm.ToaNhas = toaNhas.Select(t => new SelectListItem { Value = t, Text = t, Selected = f.ToaNha == t }).ToList();

        var tangs = await _db.PhongKTXs.AsNoTracking().Select(p => p.Tang).Distinct().OrderBy(t => t).ToListAsync();
        vm.Tangs = tangs.Select(t => new SelectListItem { Value = t.ToString(), Text = "Tầng " + t, Selected = f.Tang == t }).ToList();
    }

    public Task<PhongItemViewModel?> LayChiTietAsync(int maPhong)
        => TruyVanPhong().FirstOrDefaultAsync(p => p.MaPhong == maPhong);

    public Task<List<NguoiDangOViewModel>> LayNguoiDangOAsync(int maPhong)
    {
        return _db.PhanPhongs.AsNoTracking()
            .Where(pp => pp.MaPhong == maPhong && pp.TrangThai == TrangThaiPhanPhong.DangO)
            .OrderBy(pp => pp.NgayVao)
            .Select(pp => new NguoiDangOViewModel
            {
                MaSinhVien = pp.DangKy!.MaSinhVien,
                HoTen = pp.DangKy!.SinhVien!.HoTen,
                NgayVao = pp.NgayVao,
                NgayKetThucDuKien = pp.DangKy!.NgayKetThucDuKien
            })
            .ToListAsync();
    }
}
