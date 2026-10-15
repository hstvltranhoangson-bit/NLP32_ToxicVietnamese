// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

/// <summary>Các hàm tiện ích đọc/ghi thông tin đăng nhập trong Session.</summary>
public static class PhienDangNhap
{
    public const string KeyMaTaiKhoan = "MaTaiKhoan";
    public const string KeyHoTen = "HoTen";
    public const string KeyVaiTro = "VaiTro";

    public static void LuuDangNhap(this ISession session, TaiKhoan taiKhoan)
    {
        session.SetInt32(KeyMaTaiKhoan, taiKhoan.MaTaiKhoan);
        session.SetString(KeyHoTen, taiKhoan.HoTen);
        session.SetString(KeyVaiTro, taiKhoan.VaiTro);
    }

    public static int? LayMaTaiKhoan(this ISession session) => session.GetInt32(KeyMaTaiKhoan);
    public static string? LayHoTen(this ISession session) => session.GetString(KeyHoTen);
    public static string? LayVaiTro(this ISession session) => session.GetString(KeyVaiTro);
}
