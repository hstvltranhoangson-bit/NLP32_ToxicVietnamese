// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public interface IThongKeService
{
    Task<DashboardViewModel> LayDashboardAsync();
    Task<ThongKeViewModel> LayThongKeAsync();
}
