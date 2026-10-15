// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Validation;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>Hồ sơ sinh viên. Khóa chính là mã sinh viên (chuỗi, không trùng).</summary>
public class SinhVien
{
    [Key]
    [StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = string.Empty;

    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [NgaySinhHopLe]
    [Display(Name = "Ngày sinh")]
    public DateTime NgaySinh { get; set; } = new DateTime(2005, 1, 1);

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [StringLength(10)]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = Constants.GioiTinh.Nam;

    [Required(ErrorMessage = "Lớp là bắt buộc")]
    [StringLength(30)]
    [Display(Name = "Lớp")]
    public string Lop { get; set; } = string.Empty;

    [Required(ErrorMessage = "Khoa là bắt buộc")]
    [StringLength(100)]
    [Display(Name = "Khoa")]
    public string Khoa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;

    public TaiKhoan? TaiKhoan { get; set; }
    public ICollection<DangKyKTX> DangKyKTXs { get; set; } = new List<DangKyKTX>();
}
