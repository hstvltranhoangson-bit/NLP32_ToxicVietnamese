// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>
/// Phòng ký túc xá. KHÔNG có cột SoChoCon: số chỗ còn luôn được tính từ
/// SoNguoiToiDa của loại phòng trừ số PhanPhong đang ở.
/// </summary>
public class PhongKTX
{
    [Key]
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Tên phòng là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên phòng tối đa 50 ký tự")]
    [Display(Name = "Tên phòng")]
    public string TenPhong { get; set; } = string.Empty;

    [Display(Name = "Loại phòng")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại phòng")]
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Tòa nhà là bắt buộc")]
    [StringLength(50)]
    [Display(Name = "Tòa nhà")]
    public string ToaNha { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Tầng phải từ 1 đến 20")]
    [Display(Name = "Tầng")]
    public int Tang { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn giới tính phòng")]
    [StringLength(10)]
    [Display(Name = "Giới tính phòng")]
    public string GioiTinhPhong { get; set; } = GioiTinh.Nam;

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiPhong.DangSuDung;

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    public LoaiPhong? LoaiPhong { get; set; }
    public ICollection<PhanPhong> PhanPhongs { get; set; } = new List<PhanPhong>();
}
