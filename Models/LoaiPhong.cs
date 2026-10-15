// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>Loại phòng: quyết định sức chứa tối đa và đơn giá thuê.</summary>
public class LoaiPhong
{
    [Key]
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Tên loại phòng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên loại phòng tối đa 100 ký tự")]
    [Display(Name = "Tên loại phòng")]
    public string TenLoaiPhong { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Số người tối đa phải lớn hơn 0 (tối đa 20)")]
    [Display(Name = "Số người tối đa")]
    public int SoNguoiToiDa { get; set; } = 4;

    [Range(typeof(decimal), "1", "100000000", ErrorMessage = "Đơn giá tháng phải lớn hơn 0")]
    [DataType(DataType.Currency)]
    [Display(Name = "Đơn giá/tháng (VNĐ)")]
    public decimal DonGiaThang { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiChung.HoatDong;

    public ICollection<PhongKTX> PhongKTXs { get; set; } = new List<PhongKTX>();
}
