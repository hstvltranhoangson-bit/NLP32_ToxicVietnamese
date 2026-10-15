// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public interface IPhanPhongService
{
    Task<List<PhongItemViewModel>> LayPhongCoTheChonAsync(int? loaiTruMaPhong = null);
    Task<List<DangKyKTX>> LayDangKyChoDuyetAsync();
    Task<PagedList<PhanPhong>> LayDangOAsync(LocDanhSachViewModel loc);
    Task<PagedList<PhanPhong>> LayLichSuAsync(LocDanhSachViewModel loc);
    Task<PhanPhong?> LayPhanPhongAsync(int maPhanPhong);

    Task<KetQuaXuLy> PhanPhongAsync(int maDangKy, int maPhong, DateTime ngayVao);
    Task<KetQuaXuLy> ChuyenPhongAsync(int maPhanPhong, int maPhongMoi, DateTime ngayChuyen);
    Task<KetQuaXuLy> TraPhongAsync(int maPhanPhong, DateTime ngayRa);
}
