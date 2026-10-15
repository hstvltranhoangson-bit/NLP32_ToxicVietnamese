// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public class DangKyService : IDangKyService
{
    private readonly AppDbContext _db;

    public DangKyService(AppDbContext db) => _db = db;

    public async Task<KetQuaXuLy> TaoDangKyAsync(string maSinhVien, DangKyKtxCreateViewModel vm)
    {
        var sv = await _db.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == maSinhVien);
        if (sv == null) return KetQuaXuLy.Loi("Không tìm thấy hồ sơ sinh viên.");
        if (sv.TrangThai != TrangThaiChung.HoatDong || sv.TaiKhoan?.TrangThai != TrangThaiChung.HoatDong)
            return KetQuaXuLy.Loi("Hồ sơ sinh viên không ở trạng thái hoạt động nên không thể đăng ký.");

        if (vm.NgayBatDau == null || vm.NgayKetThucDuKien == null)
            return KetQuaXuLy.Loi("Vui lòng nhập đầy đủ ngày bắt đầu và ngày kết thúc dự kiến.");

        var batDau = vm.NgayBatDau.Value.Date;
        var ketThuc = vm.NgayKetThucDuKien.Value.Date;

        if (batDau < DateTime.Today)
            return KetQuaXuLy.Loi("Ngày bắt đầu không được nằm trong quá khứ.");
        if (ketThuc <= batDau)
            return KetQuaXuLy.Loi("Ngày kết thúc dự kiến phải sau ngày bắt đầu.");

        if (await _db.DangKyKTXs.AnyAsync(d => d.MaSinhVien == maSinhVien && d.TrangThai == TrangThaiDangKy.DaPhanPhong))
            return KetQuaXuLy.Loi("Bạn đang có đăng ký ở trạng thái Đã phân phòng nên không thể đăng ký thêm.");

        if (await _db.DangKyKTXs.AnyAsync(d => d.MaSinhVien == maSinhVien && d.TrangThai == TrangThaiDangKy.ChoDuyet))
            return KetQuaXuLy.Loi("Bạn đã có một đăng ký đang Chờ duyệt. Hãy hủy đăng ký cũ trước khi gửi đăng ký mới.");

        if (vm.NguyenVongLoaiPhong.HasValue &&
            !await _db.LoaiPhongs.AnyAsync(l => l.MaLoaiPhong == vm.NguyenVongLoaiPhong.Value && l.TrangThai == TrangThaiChung.HoatDong))
            return KetQuaXuLy.Loi("Loại phòng nguyện vọng không hợp lệ.");

        _db.DangKyKTXs.Add(new DangKyKTX
        {
            MaSinhVien = maSinhVien,
            NgayDangKy = DateTime.Today,              // do hệ thống xác định
            NgayBatDau = batDau,
            NgayKetThucDuKien = ketThuc,
            NguyenVongLoaiPhong = vm.NguyenVongLoaiPhong,
            GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim(),
            TrangThai = TrangThaiDangKy.ChoDuyet
        });
        await _db.SaveChangesAsync();
        return KetQuaXuLy.Ok("Gửi đăng ký thành công. Vui lòng chờ Ban quản lý KTX duyệt và phân phòng.");
    }

    public async Task<KetQuaXuLy> HuyDangKyAsync(int maDangKy, string? maSinhVien)
    {
        var dk = await _db.DangKyKTXs.FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);

        // Sinh viên chỉ được thao tác trên đăng ký của CHÍNH MÌNH (không tin mã trên URL)
        if (dk == null || (maSinhVien != null && dk.MaSinhVien != maSinhVien))
            return KetQuaXuLy.NotFound("Không tìm thấy đăng ký.");

        if (dk.TrangThai != TrangThaiDangKy.ChoDuyet)
            return KetQuaXuLy.Loi($"Chỉ được hủy đăng ký ở trạng thái Chờ duyệt (hiện tại: {dk.TrangThai}).");

        dk.TrangThai = TrangThaiDangKy.DaHuy;
        await _db.SaveChangesAsync();
        return KetQuaXuLy.Ok("Đã hủy đăng ký.");
    }

    public Task<PagedList<DangKyKTX>> TimKiemAsync(LocDanhSachViewModel loc)
    {
        var query = _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
        {
            var k = loc.TuKhoa.Trim();
            query = query.Where(d => d.MaSinhVien.Contains(k) || d.SinhVien!.HoTen.Contains(k));
        }
        if (!string.IsNullOrWhiteSpace(loc.TrangThai))
            query = query.Where(d => d.TrangThai == loc.TrangThai);

        return query.OrderByDescending(d => d.NgayDangKy).ThenByDescending(d => d.MaDangKy)
                    .ToPagedListAsync(loc.Trang, CauHinhPhanTrang.DanhSachMoiTrang);
    }

    public Task<DangKyKTX?> LayChiTietAsync(int maDangKy)
    {
        return _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.SinhVien)
            .Include(d => d.LoaiPhongNguyenVong)
            .Include(d => d.PhanPhongs).ThenInclude(p => p.Phong).ThenInclude(ph => ph!.LoaiPhong)
            .FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
    }

    /// <summary>Mọi truy vấn đều ràng buộc theo MaTaiKhoan đang đăng nhập -> không thể xem chéo dữ liệu.</summary>
    public async Task<SinhVienCaNhanViewModel?> LayThongTinCaNhanAsync(int maTaiKhoan)
    {
        var sv = await _db.SinhViens.AsNoTracking().Include(s => s.TaiKhoan)
            .FirstOrDefaultAsync(s => s.MaTaiKhoan == maTaiKhoan);
        if (sv == null || sv.TaiKhoan == null) return null;

        var dangKys = await _db.DangKyKTXs.AsNoTracking()
            .Include(d => d.LoaiPhongNguyenVong)
            .Where(d => d.MaSinhVien == sv.MaSinhVien)
            .OrderByDescending(d => d.NgayDangKy).ThenByDescending(d => d.MaDangKy)
            .ToListAsync();

        var lichSu = await _db.PhanPhongs.AsNoTracking()
            .Include(p => p.Phong).ThenInclude(ph => ph!.LoaiPhong)
            .Include(p => p.DangKy)
            .Where(p => p.DangKy!.MaSinhVien == sv.MaSinhVien)
            .OrderByDescending(p => p.NgayVao).ThenByDescending(p => p.MaPhanPhong)
            .ToListAsync();

        return new SinhVienCaNhanViewModel
        {
            SinhVien = sv,
            TaiKhoan = sv.TaiKhoan,
            DanhSachDangKy = dangKys,
            DangKyChoDuyet = dangKys.FirstOrDefault(d => d.TrangThai == TrangThaiDangKy.ChoDuyet),
            PhongHienTai = lichSu.FirstOrDefault(p => p.TrangThai == TrangThaiPhanPhong.DangO),
            LichSuO = lichSu
        };
    }
}
