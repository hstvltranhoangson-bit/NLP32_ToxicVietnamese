// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public interface IDangKyService
{
    Task<KetQuaXuLy> TaoDangKyAsync(string maSinhVien, DangKyKtxCreateViewModel vm);
    /// <param name="maSinhVien">Mã SV của người thực hiện; null nếu là Admin hủy.</param>
    Task<KetQuaXuLy> HuyDangKyAsync(int maDangKy, string? maSinhVien);
    Task<PagedList<DangKyKTX>> TimKiemAsync(LocDanhSachViewModel loc);
    Task<DangKyKTX?> LayChiTietAsync(int maDangKy);
    Task<SinhVienCaNhanViewModel?> LayThongTinCaNhanAsync(int maTaiKhoan);
}
