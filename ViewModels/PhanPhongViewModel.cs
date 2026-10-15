// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Form Admin phân phòng cho một đăng ký Chờ duyệt.</summary>
public class PhanPhongViewModel
{
    public int MaDangKy { get; set; }
    public string TrangThaiDangKy { get; set; } = string.Empty;
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public string? NguyenVong { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThucDuKien { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng")]
    [Display(Name = "Phòng")]
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Ngày vào là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào")]
    public DateTime? NgayVao { get; set; }

    public List<PhongItemViewModel> PhongCoTheChon { get; set; } = new();
}
