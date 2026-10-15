// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>Dashboard và thống kê: mọi số liệu được truy vấn LINQ trực tiếp từ DB, không lưu tổng cố định.</summary>
public class ThongKeService : IThongKeService
{
    private readonly AppDbContext _db;
    private readonly IPhongService _phongService;

    public ThongKeService(AppDbContext db, IPhongService phongService)
    {
        _db = db;
        _phongService = phongService;
    }

    private IQueryable<Models.PhanPhong> DangO() => _db.PhanPhongs.Where(p => p.TrangThai == TrangThaiPhanPhong.DangO);

    private async Task<(int tongCho, int dangDung, double tyLe)> TinhSucChuaAsync()
    {
        // Tổng số chỗ = tổng SoNguoiToiDa của các phòng đang hoạt động
        var tongCho = await _db.PhongKTXs.Where(p => p.TrangThai == TrangThaiPhong.DangSuDung)
                                         .SumAsync(p => p.LoaiPhong!.SoNguoiToiDa);
        // Số chỗ đang sử dụng = số PhanPhong có trạng thái Đang ở
        var dangDung = await DangO().CountAsync();
        var tyLe = tongCho == 0 ? 0 : Math.Round(dangDung * 100.0 / tongCho, 1);
        return (tongCho, dangDung, tyLe);
    }

    public async Task<DashboardViewModel> LayDashboardAsync()
    {
        var (tongCho, dangDung, tyLe) = await TinhSucChuaAsync();

        var d = new DashboardViewModel
        {
            TongSoPhong = await _db.PhongKTXs.CountAsync(),
            TongSinhVien = await _db.SinhViens.CountAsync(),
            TongDangKy = await _db.DangKyKTXs.CountAsync(),
            DangKyChoDuyet = await _db.DangKyKTXs.CountAsync(x => x.TrangThai == TrangThaiDangKy.ChoDuyet),
            SinhVienDangO = await DangO().Select(p => p.DangKy!.MaSinhVien).Distinct().CountAsync(),
            TongSoCho = tongCho,
            ChoDangSuDung = dangDung,
            ChoConTrong = tongCho - dangDung,
            TyLeSuDung = tyLe,
            SoPhongDay = await _db.PhongKTXs.CountAsync(p => p.TrangThai == TrangThaiPhong.DangSuDung
                && p.PhanPhongs.Count(pp => pp.TrangThai == TrangThaiPhanPhong.DangO) >= p.LoaiPhong!.SoNguoiToiDa)
        };

        d.SinhVienTheoToa = await SinhVienTheoToaAsync();
        d.SinhVaoTheoThang = await SinhVienVaoTheoThangAsync();
        return d;
    }

    private Task<List<ThongKeItem>> SinhVienTheoToaAsync() =>
        DangO().GroupBy(p => p.Phong!.ToaNha)
               .Select(g => new ThongKeItem { Nhan = g.Key, GiaTri = g.Count() })
               .OrderBy(x => x.Nhan)
               .ToListAsync();

    private async Task<List<ThongKeItem>> SinhVienVaoTheoThangAsync()
    {
        // Ngày vào KTX lần đầu của mỗi đăng ký (chuyển phòng không tính là vào KTX thêm lần nữa)
        var ngayVaoDauTien = await _db.PhanPhongs.GroupBy(p => p.MaDangKy)
                                                 .Select(g => g.Min(x => x.NgayVao))
                                                 .ToListAsync();
        return ngayVaoDauTien
            .GroupBy(n => new { n.Year, n.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ThongKeItem { Nhan = $"{g.Key.Month:00}/{g.Key.Year}", GiaTri = g.Count() })
            .ToList();
    }

    public async Task<ThongKeViewModel> LayThongKeAsync()
    {
        var (tongCho, dangDung, tyLe) = await TinhSucChuaAsync();
        var vm = new ThongKeViewModel { TongSoCho = tongCho, ChoDangSuDung = dangDung, TyLeSuDung = tyLe };

        // Số phòng theo từng loại
        vm.PhongTheoLoai = await _db.PhongKTXs.GroupBy(p => p.LoaiPhong!.TenLoaiPhong)
            .Select(g => new ThongKeItem { Nhan = g.Key, GiaTri = g.Count() })
            .OrderByDescending(x => x.GiaTri).ToListAsync();

        // Số sinh viên đang ở + số chỗ còn lại của từng phòng
        vm.SinhVienTheoPhong = await _phongService.TruyVanPhong().OrderBy(p => p.TenPhong).ToListAsync();

        // Phòng có nhiều sinh viên đang ở nhất
        vm.PhongDongNhat = await _phongService.TruyVanPhong()
            .OrderByDescending(p => p.SoDangO).ThenBy(p => p.TenPhong).Take(5).ToListAsync();

        vm.SinhVienTheoToa = await SinhVienTheoToaAsync();

        vm.SinhVienTheoLoaiPhong = await DangO().GroupBy(p => p.Phong!.LoaiPhong!.TenLoaiPhong)
            .Select(g => new ThongKeItem { Nhan = g.Key, GiaTri = g.Count() })
            .OrderByDescending(x => x.GiaTri).ToListAsync();

        // Số đăng ký theo trạng thái
        vm.DangKyTheoTrangThai = await _db.DangKyKTXs.GroupBy(d => d.TrangThai)
            .Select(g => new ThongKeItem { Nhan = g.Key, GiaTri = g.Count() })
            .OrderByDescending(x => x.GiaTri).ToListAsync();

        vm.SinhVienVaoTheoThang = await SinhVienVaoTheoThangAsync();
        return vm;
    }
}
