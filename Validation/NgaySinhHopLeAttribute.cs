// Họ và tên: [Họ tên Sinh viên 3]
// Mã sinh viên: [MSSV 3]
// Nội dung thực hiện: Module 3 - Quản lý sinh viên, đăng ký ở KTX, xem thông tin/lịch sử ở
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Validation;

/// <summary>Kiểm tra ngày sinh không ở tương lai và độ tuổi hợp lý (15 - 60).</summary>
[AttributeUsage(AttributeTargets.Property)]
public class NgaySinhHopLeAttribute : ValidationAttribute
{
    public NgaySinhHopLeAttribute() : base("Ngày sinh không hợp lệ (tuổi phải từ 15 đến 60 và không ở tương lai)") { }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime ngaySinh) return false;
        var homNay = DateTime.Today;
        if (ngaySinh.Date > homNay) return false;
        var tuoi = homNay.Year - ngaySinh.Year;
        if (ngaySinh.Date > homNay.AddYears(-tuoi)) tuoi--;
        return tuoi is >= 15 and <= 60;
    }
}
