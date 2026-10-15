// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Dòng danh sách sinh viên kèm cờ "đang ở KTX".</summary>
public class SinhVienDongViewModel
{
    public SinhVien SinhVien { get; set; } = new();
    public bool DangO { get; set; }
}
