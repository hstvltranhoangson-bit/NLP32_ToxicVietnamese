// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

public class TraPhongViewModel
{
    public int MaPhanPhong { get; set; }
    public string TrangThaiHienTai { get; set; } = string.Empty;
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public DateTime NgayVao { get; set; }

    [Required(ErrorMessage = "Ngày trả phòng là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày trả phòng")]
    public DateTime? NgayRa { get; set; }
}
