// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Dòng hiển thị danh sách loại phòng kèm số phòng thuộc loại.</summary>
public class LoaiPhongDongViewModel
{
    public LoaiPhong Loai { get; set; } = new();
    public int SoPhong { get; set; }
}
