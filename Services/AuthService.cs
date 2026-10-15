// Họ và tên: [Họ tên Sinh viên 1]
// Mã sinh viên: [MSSV 1]
// Nội dung thực hiện: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db) => _db = db;

    public async Task<KetQuaDangNhap> DangNhapAsync(string tenDangNhap, string matKhau)
    {
        var ten = tenDangNhap.Trim();
        var tk = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == ten);

        if (tk == null)
            return new KetQuaDangNhap { Truong = "TenDangNhap", ThongBao = "Tên đăng nhập không tồn tại." };

        if (!MatKhauHelper.KiemTra(matKhau, tk.MatKhau))
            return new KetQuaDangNhap { Truong = "MatKhau", ThongBao = "Sai mật khẩu." };

        if (tk.TrangThai != TrangThaiChung.HoatDong)
            return new KetQuaDangNhap { ThongBao = "Tài khoản đã bị khóa. Vui lòng liên hệ Ban quản lý KTX." };

        return new KetQuaDangNhap { TaiKhoan = tk };
    }

    public async Task<KetQuaXuLy> DoiMatKhauAsync(int maTaiKhoan, string matKhauCu, string matKhauMoi)
    {
        var tk = await _db.TaiKhoans.FindAsync(maTaiKhoan);
        if (tk == null) return KetQuaXuLy.NotFound("Không tìm thấy tài khoản.");
        if (!MatKhauHelper.KiemTra(matKhauCu, tk.MatKhau)) return KetQuaXuLy.Loi("Mật khẩu hiện tại không đúng.");

        tk.MatKhau = MatKhauHelper.BamMatKhau(matKhauMoi);
        await _db.SaveChangesAsync();
        return KetQuaXuLy.Ok("Đổi mật khẩu thành công.");
    }

    /// <summary>
    /// Khóa/mở khóa tài khoản. Chỉ đổi trạng thái, KHÔNG xóa dữ liệu: lịch sử đăng ký và phân phòng vẫn được giữ lại.
    /// </summary>
    public async Task<KetQuaXuLy> DoiTrangThaiKhoaAsync(int maTaiKhoanCanDoi, int maTaiKhoanThucHien)
    {
        if (maTaiKhoanCanDoi == maTaiKhoanThucHien)
            return KetQuaXuLy.Loi("Không thể tự khóa tài khoản đang đăng nhập.");

        var tk = await _db.TaiKhoans.Include(t => t.SinhVien).FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoanCanDoi);
        if (tk == null) return KetQuaXuLy.NotFound("Không tìm thấy tài khoản.");

        tk.TrangThai = tk.TrangThai == TrangThaiChung.HoatDong ? TrangThaiChung.Khoa : TrangThaiChung.HoatDong;
        if (tk.SinhVien != null) tk.SinhVien.TrangThai = tk.TrangThai;   // đồng bộ trạng thái hồ sơ

        await _db.SaveChangesAsync();
        return KetQuaXuLy.Ok(tk.TrangThai == TrangThaiChung.Khoa
            ? $"Đã khóa tài khoản {tk.TenDangNhap}."
            : $"Đã mở khóa tài khoản {tk.TenDangNhap}.");
    }
}
