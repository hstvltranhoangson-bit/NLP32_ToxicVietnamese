// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

/// <summary>
/// Lịch sử ở thực tế. Chuyển phòng / trả phòng KHÔNG sửa MaPhong hay xóa bản ghi,
/// mà đổi trạng thái bản ghi cũ và (khi chuyển) tạo bản ghi mới.
/// </summary>
public class PhanPhong
{
    [Key]
    public int MaPhanPhong { get; set; }

    public int MaDangKy { get; set; }
    public int MaPhong { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào")]
    public DateTime NgayVao { get; set; }

    /// <summary>Nullable: để trống khi sinh viên còn đang ở.</summary>
    [DataType(DataType.Date)]
    [Display(Name = "Ngày ra")]
    public DateTime? NgayRa { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = TrangThaiPhanPhong.DangO;

    public DangKyKTX? DangKy { get; set; }
    public PhongKTX? Phong { get; set; }
}
