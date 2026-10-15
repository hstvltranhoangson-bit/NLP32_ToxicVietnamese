// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>
/// Yêu cầu xin ở KTX của sinh viên. Đăng ký chưa đồng nghĩa đã được xếp phòng:
/// việc xếp phòng được lưu ở bảng PhanPhong.
/// </summary>
public class DangKyKTX
{
    [Key]
    public int MaDangKy { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc dự kiến")]
    public DateTime NgayKetThucDuKien { get; set; }

    /// <summary>Nguyện vọng chỉ mang tính tham khảo (có thể để trống).</summary>
    [Display(Name = "Nguyện vọng loại phòng")]
    public int? NguyenVongLoaiPhong { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

    public SinhVien? SinhVien { get; set; }
    public LoaiPhong? LoaiPhongNguyenVong { get; set; }
    public ICollection<PhanPhong> PhanPhongs { get; set; } = new List<PhanPhong>();
}
