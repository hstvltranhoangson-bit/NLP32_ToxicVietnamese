// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Form sinh viên gửi đăng ký ở KTX. MaSinhVien và NgayDangKy KHÔNG nằm trong form: do hệ thống xác định.</summary>
public class DangKyKtxCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu ở")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime? NgayBatDau { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc dự kiến")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc dự kiến")]
    public DateTime? NgayKetThucDuKien { get; set; } = DateTime.Today.AddMonths(6);

    [Display(Name = "Nguyện vọng loại phòng")]
    public int? NguyenVongLoaiPhong { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public List<SelectListItem> LoaiPhongs { get; set; } = new();
}
