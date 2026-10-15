// Họ và tên: [Họ tên Sinh viên 4]
// Mã sinh viên: [MSSV 4]
// Nội dung thực hiện: Module 4 - Phân phòng, chuyển phòng, trả phòng, Dashboard, thống kê
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

public class ChuyenPhongViewModel
{
    public int MaPhanPhong { get; set; }
    public string TrangThaiHienTai { get; set; } = string.Empty;
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public int MaPhongHienTai { get; set; }
    public string TenPhongHienTai { get; set; } = string.Empty;
    public DateTime NgayVaoHienTai { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng mới")]
    [Display(Name = "Phòng mới")]
    public int MaPhongMoi { get; set; }

    [Required(ErrorMessage = "Ngày chuyển là bắt buộc")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày chuyển")]
    public DateTime? NgayChuyen { get; set; }

    public List<PhongItemViewModel> PhongCoTheChon { get; set; } = new();
}
