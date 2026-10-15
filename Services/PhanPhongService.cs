// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

/// <summary>
/// Nghiệp vụ phân phòng / chuyển phòng / trả phòng.
/// Mỗi thao tác chạy trong một Transaction (Serializable) và KIỂM TRA LẠI sức chứa ngay trước khi SaveChanges
/// để tránh dữ liệu đã thay đổi giữa lúc mở form và lúc xác nhận.
/// </summary>
public class PhanPhongService : IPhanPhongService
{
    private readonly AppDbContext _db;
    private readonly IPhongService _phongService;

    public PhanPhongService(AppDbContext db, IPhongService phongService)
    {
        _db = db;
        _phongService = phongService;
    }

    // ---------------- Truy vấn hỗ trợ ----------------

    /// <summary>Phòng có thể chọn: loại bỏ phòng Tạm ngừng và phòng đã đủ sức chứa.</summary>
    public Task<List<PhongItemViewModel>> LayPhongCoTheChonAsync(int? loaiTruMaPhong = null)
    {
        return _phongService.TruyVanPhong()
            .Where(p => p.TrangThai == TrangThaiPhong.DangSuDung
                        && p.SoChoCon > 0
                        && (!loaiTruMaPhong.HasValue || p.MaPhong != loaiTruMaPhong.Value))
            .OrderBy(p => p.ToaNha).ThenBy(p => p.TenPhong)
            .ToListAsync();
    }

    public Task<List<DangKyKTX>> LayDangKyChoDuyetAsync()
    {
        return _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .Where(d => d.TrangThai == TrangThaiDangKy.ChoDuyet)
            .OrderBy(d => d.NgayDangKy).ThenBy(d => d.MaDangKy)
            .ToListAsync();
    }

    private IQueryable<PhanPhong> TruyVanPhanPhong()
    {
        return _db.PhanPhongs.AsNoTracking()
            .Include(p => p.Phong).ThenInclude(ph => ph!.LoaiPhong)
            .Include(p => p.DangKy).ThenInclude(d => d!.SinhVien);
    }

    private static IQueryable<PhanPhong> TimTheoTuKhoa(IQueryable<PhanPhong> q, string? tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa)) return q;
        var k = tuKhoa.Trim();
        return q.Where(p => p.DangKy!.MaSinhVien.Contains(k)
                         || p.DangKy!.SinhVien!.HoTen.Contains(k)
                         || p.Phong!.TenPhong.Contains(k));
    }

    public Task<PagedList<PhanPhong>> LayDangOAsync(LocDanhSachViewModel loc)
    {
        var q = TimTheoTuKhoa(TruyVanPhanPhong().Where(p => p.TrangThai == TrangThaiPhanPhong.DangO), loc.TuKhoa);
        return q.OrderBy(p => p.Phong!.TenPhong).ThenBy(p => p.NgayVao)
                .ToPagedListAsync(loc.Trang, CauHinhPhanTrang.DanhSachMoiTrang);
    }

    public Task<PagedList<PhanPhong>> LayLichSuAsync(LocDanhSachViewModel loc)
    {
        var q = TruyVanPhanPhong();
        if (!string.IsNullOrWhiteSpace(loc.TrangThai)) q = q.Where(p => p.TrangThai == loc.TrangThai);
        q = TimTheoTuKhoa(q, loc.TuKhoa);
        return q.OrderByDescending(p => p.NgayVao).ThenByDescending(p => p.MaPhanPhong)
                .ToPagedListAsync(loc.Trang, CauHinhPhanTrang.DanhSachMoiTrang);
    }

    public Task<PhanPhong?> LayPhanPhongAsync(int maPhanPhong)
        => TruyVanPhanPhong().FirstOrDefaultAsync(p => p.MaPhanPhong == maPhanPhong);

    // ---------------- Kiểm tra dùng chung cho phân/chuyển phòng ----------------

    /// <returns>Thông báo lỗi, hoặc null nếu phòng hợp lệ để nhận sinh viên.</returns>
    private async Task<string?> KiemTraPhongNhanAsync(int maPhong, string gioiTinhSinhVien)
    {
        var phong = await _db.PhongKTXs.Include(p => p.LoaiPhong).FirstOrDefaultAsync(p => p.MaPhong == maPhong);
        if (phong == null) return "Phòng được chọn không tồn tại.";
        if (phong.TrangThai != TrangThaiPhong.DangSuDung) return $"Phòng {phong.TenPhong} đang tạm ngừng, không thể phân sinh viên vào.";
        if (phong.GioiTinhPhong != gioiTinhSinhVien)
            return $"Phòng {phong.TenPhong} là phòng {phong.GioiTinhPhong}, không phù hợp với sinh viên {gioiTinhSinhVien}.";

        // Số người đang ở xác định bằng LINQ từ PhanPhong có trạng thái Đang ở
        var soDangO = await _db.PhanPhongs.CountAsync(p => p.MaPhong == maPhong && p.TrangThai == TrangThaiPhanPhong.DangO);
        var sucChua = phong.LoaiPhong!.SoNguoiToiDa;
        if (soDangO >= sucChua) return $"Phòng {phong.TenPhong} đã đủ người ({soDangO}/{sucChua}), không thể phân thêm.";
        return null;
    }

    // ---------------- Phân phòng ----------------

    public async Task<KetQuaXuLy> PhanPhongAsync(int maDangKy, int maPhong, DateTime ngayVao)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var dk = await _db.DangKyKTXs.Include(d => d.SinhVien).FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
            if (dk == null || dk.SinhVien == null) return KetQuaXuLy.NotFound("Không tìm thấy đăng ký.");
            if (dk.TrangThai != TrangThaiDangKy.ChoDuyet)
                return KetQuaXuLy.Loi($"Đăng ký không ở trạng thái Chờ duyệt (hiện tại: {dk.TrangThai}).");
            if (dk.SinhVien.TrangThai != TrangThaiChung.HoatDong)
                return KetQuaXuLy.Loi("Sinh viên đang bị khóa, không thể phân phòng.");
            if (ngayVao.Date < dk.NgayDangKy.Date)
                return KetQuaXuLy.Loi("Ngày vào không được trước ngày đăng ký.");

            var loi = await KiemTraPhongNhanAsync(maPhong, dk.SinhVien.GioiTinh);
            if (loi != null) return KetQuaXuLy.Loi(loi);

            // Một sinh viên chỉ có tối đa một PhanPhong "Đang ở"
            var daCoPhong = await _db.PhanPhongs.AnyAsync(p => p.TrangThai == TrangThaiPhanPhong.DangO && p.DangKy!.MaSinhVien == dk.MaSinhVien);
            if (daCoPhong) return KetQuaXuLy.Loi("Sinh viên đang ở một phòng khác, không thể phân thêm phòng.");

            _db.PhanPhongs.Add(new PhanPhong
            {
                MaDangKy = dk.MaDangKy,
                MaPhong = maPhong,
                NgayVao = ngayVao.Date,
                NgayRa = null,
                TrangThai = TrangThaiPhanPhong.DangO
            });
            dk.TrangThai = TrangThaiDangKy.DaPhanPhong;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return KetQuaXuLy.Ok($"Đã phân phòng cho sinh viên {dk.SinhVien.HoTen}.");
        }
        catch (Exception)
        {
            return KetQuaXuLy.Loi("Không thể hoàn tất phân phòng do dữ liệu vừa thay đổi. Vui lòng thử lại.");
        }
    }

    // ---------------- Chuyển phòng ----------------

    public async Task<KetQuaXuLy> ChuyenPhongAsync(int maPhanPhong, int maPhongMoi, DateTime ngayChuyen)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var cu = await _db.PhanPhongs.Include(p => p.DangKy).ThenInclude(d => d!.SinhVien)
                .FirstOrDefaultAsync(p => p.MaPhanPhong == maPhanPhong);
            if (cu == null || cu.DangKy?.SinhVien == null) return KetQuaXuLy.NotFound("Không tìm thấy bản ghi phân phòng.");
            if (cu.TrangThai != TrangThaiPhanPhong.DangO)
                return KetQuaXuLy.Loi("Sinh viên hiện không ở phòng này (đã chuyển hoặc đã trả phòng).");
            if (maPhongMoi == cu.MaPhong)
                return KetQuaXuLy.Loi("Phòng mới phải khác phòng hiện tại của sinh viên.");
            if (ngayChuyen.Date < cu.NgayVao.Date)
                return KetQuaXuLy.Loi("Ngày chuyển không được trước ngày vào phòng hiện tại.");

            var loi = await KiemTraPhongNhanAsync(maPhongMoi, cu.DangKy.SinhVien.GioiTinh);
            if (loi != null) return KetQuaXuLy.Loi(loi);

            // Bước 1: đóng bản ghi cũ (giữ lịch sử, không sửa MaPhong, không xóa)
            cu.TrangThai = TrangThaiPhanPhong.DaChuyen;
            cu.NgayRa = ngayChuyen.Date;
            await _db.SaveChangesAsync();

            // Bước 2: tạo bản ghi mới cho phòng mới (tách 2 lần lưu để không vi phạm chỉ mục "1 Đang ở / đăng ký")
            _db.PhanPhongs.Add(new PhanPhong
            {
                MaDangKy = cu.MaDangKy,
                MaPhong = maPhongMoi,
                NgayVao = ngayChuyen.Date,
                TrangThai = TrangThaiPhanPhong.DangO
            });
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return KetQuaXuLy.Ok($"Đã chuyển phòng cho sinh viên {cu.DangKy.SinhVien.HoTen}.");
        }
        catch (Exception)
        {
            return KetQuaXuLy.Loi("Không thể hoàn tất chuyển phòng do dữ liệu vừa thay đổi. Vui lòng thử lại.");
        }
    }

    // ---------------- Trả phòng ----------------

    public async Task<KetQuaXuLy> TraPhongAsync(int maPhanPhong, DateTime ngayRa)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var pp = await _db.PhanPhongs.Include(p => p.DangKy).ThenInclude(d => d!.SinhVien)
                .FirstOrDefaultAsync(p => p.MaPhanPhong == maPhanPhong);
            if (pp == null || pp.DangKy == null) return KetQuaXuLy.NotFound("Không tìm thấy bản ghi phân phòng.");
            if (pp.TrangThai != TrangThaiPhanPhong.DangO)
                return KetQuaXuLy.Loi("Sinh viên không còn ở phòng này (đã trả phòng hoặc đã chuyển). Không thể trả phòng lần nữa.");
            if (ngayRa.Date < pp.NgayVao.Date)
                return KetQuaXuLy.Loi("Ngày trả phòng không được trước ngày vào phòng.");

            pp.TrangThai = TrangThaiPhanPhong.DaTraPhong;
            pp.NgayRa = ngayRa.Date;
            pp.DangKy.TrangThai = TrangThaiDangKy.DaTraPhong;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return KetQuaXuLy.Ok($"Sinh viên {pp.DangKy.SinhVien?.HoTen} đã trả phòng. Chỗ trống đã được giải phóng.");
        }
        catch (Exception)
        {
            return KetQuaXuLy.Loi("Không thể hoàn tất trả phòng. Vui lòng thử lại.");
        }
    }
}
