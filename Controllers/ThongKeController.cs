// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Filters;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Controllers;

[YeuCauDangNhap(VaiTro.Admin)]
public class ThongKeController : BaseController
{
    private readonly IThongKeService _service;

    public ThongKeController(IThongKeService service) => _service = service;

    public async Task<IActionResult> Dashboard() => View(await _service.LayDashboardAsync());

    public async Task<IActionResult> Index() => View(await _service.LayThongKeAsync());
}
