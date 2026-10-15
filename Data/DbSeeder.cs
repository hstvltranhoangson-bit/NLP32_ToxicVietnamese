// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;

/// <summary>
/// Dữ liệu mẫu: 4 loại phòng, 20 phòng, 30 sinh viên (+ tài khoản), 2 Admin, 27 đăng ký
/// và dữ liệu phân phòng đủ các tình huống kiểm thử (phòng đầy, còn 1 chỗ, tạm ngừng,
/// đã chuyển, đã trả, chờ duyệt, đã hủy).
/// </summary>
public static class DbSeeder
{
    public const string MatKhauAdmin = "Admin@123";
    public const string MatKhauSinhVien = "Sv@123456";

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.TaiKhoans.AnyAsync()) return;

        var homNay = DateTime.Today;

        // ---------- Tài khoản Admin ----------
        db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "admin", MatKhau = MatKhauHelper.BamMatKhau(MatKhauAdmin), HoTen = "Quản trị viên KTX", Email = "admin@uneti.edu.vn", VaiTro = VaiTro.Admin });
        db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "admin2", MatKhau = MatKhauHelper.BamMatKhau(MatKhauAdmin), HoTen = "Cán bộ quản lý KTX", Email = "admin2@uneti.edu.vn", VaiTro = VaiTro.Admin });

        // ---------- 4 loại phòng ----------
        var l2 = new LoaiPhong { TenLoaiPhong = "Phòng 2 người (dịch vụ)", SoNguoiToiDa = 2, DonGiaThang = 900000, MoTa = "Phòng khép kín, điều hòa, nóng lạnh." };
        var l4 = new LoaiPhong { TenLoaiPhong = "Phòng 4 người", SoNguoiToiDa = 4, DonGiaThang = 600000, MoTa = "Phòng khép kín, quạt trần, giường tầng." };
        var l6 = new LoaiPhong { TenLoaiPhong = "Phòng 6 người", SoNguoiToiDa = 6, DonGiaThang = 450000, MoTa = "Phòng tiêu chuẩn, nhà vệ sinh chung theo tầng." };
        var l8 = new LoaiPhong { TenLoaiPhong = "Phòng 8 người", SoNguoiToiDa = 8, DonGiaThang = 350000, MoTa = "Phòng tập thể, tiết kiệm chi phí." };
        db.LoaiPhongs.AddRange(l2, l4, l6, l8);

        // ---------- 20 phòng ----------
        PhongKTX P(string ten, LoaiPhong loai, string toa, int tang, string gt, string tt = TrangThaiPhong.DangSuDung, string? moTa = null)
            => new() { TenPhong = ten, LoaiPhong = loai, ToaNha = toa, Tang = tang, GioiTinhPhong = gt, TrangThai = tt, MoTa = moTa };

        var phongs = new List<PhongKTX>
        {
            P("A101", l4, "Tòa A", 1, GioiTinh.Nam), P("A102", l4, "Tòa A", 1, GioiTinh.Nam),
            P("A201", l6, "Tòa A", 2, GioiTinh.Nam), P("A202", l6, "Tòa A", 2, GioiTinh.Nam),
            P("A301", l2, "Tòa A", 3, GioiTinh.Nam), P("A302", l8, "Tòa A", 3, GioiTinh.Nam),

            P("B101", l4, "Tòa B", 1, GioiTinh.Nu), P("B102", l4, "Tòa B", 1, GioiTinh.Nu),
            P("B201", l6, "Tòa B", 2, GioiTinh.Nu), P("B202", l6, "Tòa B", 2, GioiTinh.Nu),
            P("B301", l2, "Tòa B", 3, GioiTinh.Nu),
            P("B302", l2, "Tòa B", 3, GioiTinh.Nu, TrangThaiPhong.TamNgung, "Đang sửa chữa hệ thống điện."),

            P("C101", l8, "Tòa C", 1, GioiTinh.Nam), P("C102", l8, "Tòa C", 1, GioiTinh.Nam),
            P("C201", l4, "Tòa C", 2, GioiTinh.Nam), P("C202", l4, "Tòa C", 2, GioiTinh.Nam),

            P("D101", l6, "Tòa D", 1, GioiTinh.Nu), P("D102", l4, "Tòa D", 1, GioiTinh.Nu),
            P("D201", l2, "Tòa D", 2, GioiTinh.Nu), P("D202", l8, "Tòa D", 2, GioiTinh.Nu),
        };
        db.PhongKTXs.AddRange(phongs);
        PhongKTX Phong(string ten) => phongs.First(p => p.TenPhong == ten);

        // ---------- 30 sinh viên (18 nam + 12 nữ) + tài khoản ----------
        var tenNam = new[]
        {
            "Nguyễn Văn An", "Trần Minh Quân", "Lê Hoàng Nam", "Phạm Quốc Huy", "Vũ Đức Anh", "Đặng Văn Hùng",
            "Bùi Thành Long", "Hoàng Gia Bảo", "Ngô Tuấn Kiệt", "Đỗ Mạnh Cường", "Dương Văn Khoa", "Lý Hữu Phước",
            "Phan Trọng Nghĩa", "Trịnh Công Sơn", "Đinh Xuân Trường", "Mai Quang Vinh", "Cao Duy Khánh", "Lâm Hải Đăng"
        };
        var tenNu = new[]
        {
            "Nguyễn Thị Lan", "Trần Thị Mai", "Lê Thu Hà", "Phạm Ngọc Anh", "Vũ Thùy Linh", "Đặng Hồng Nhung",
            "Bùi Phương Thảo", "Hoàng Khánh Linh", "Ngô Bảo Châu", "Đỗ Thanh Tâm", "Dương Mỹ Duyên", "Lý Diệu Hương"
        };
        var lops = new[] { "TI17A1HN", "TI17A2HN", "DI17A1HN", "KT17A1HN" };
        var khoas = new[] { "Công nghệ thông tin", "Điện - Điện tử", "Cơ khí", "Kinh tế" };

        var svs = new List<SinhVien>();
        for (int i = 0; i < 30; i++)
        {
            bool nam = i < 18;
            string ma = (22103100001L + i).ToString();
            string hoTen = nam ? tenNam[i] : tenNu[i - 18];
            var tk = new TaiKhoan
            {
                TenDangNhap = ma,
                MatKhau = MatKhauHelper.BamMatKhau(MatKhauSinhVien),
                HoTen = hoTen,
                Email = $"{ma}@sv.uneti.edu.vn",
                VaiTro = VaiTro.SinhVien,
                TrangThai = TrangThaiChung.HoatDong
            };
            svs.Add(new SinhVien
            {
                MaSinhVien = ma,
                TaiKhoan = tk,
                HoTen = hoTen,
                NgaySinh = new DateTime(2004 + i % 3, 1 + i % 12, 1 + (i * 3) % 27),
                GioiTinh = nam ? GioiTinh.Nam : GioiTinh.Nu,
                Lop = lops[i % lops.Length],
                Khoa = khoas[i % khoas.Length],
                SoDienThoai = "0912000" + (i + 1).ToString("000"),
                Email = tk.Email,
                DiaChi = nam ? "Hà Nam" : "Nam Định",
                TrangThai = TrangThaiChung.HoatDong
            });
        }
        db.SinhViens.AddRange(svs);
        SinhVien SV(int stt) => svs[stt - 1];   // stt từ 1 đến 30

        // ---------- Đăng ký + phân phòng ----------
        DangKyKTX DK(SinhVien sv, string tt, DateTime batDau, LoaiPhong? nguyenVong, string? ghiChu = null)
        {
            var dk = new DangKyKTX
            {
                SinhVien = sv,
                NgayDangKy = tt == TrangThaiDangKy.ChoDuyet || tt == TrangThaiDangKy.DaHuy ? homNay.AddDays(-3) : batDau.AddDays(-5),
                NgayBatDau = batDau,
                NgayKetThucDuKien = batDau.AddMonths(6),
                LoaiPhongNguyenVong = nguyenVong,
                GhiChu = ghiChu,
                TrangThai = tt
            };
            db.DangKyKTXs.Add(dk);
            return dk;
        }
        void PP(DangKyKTX dk, string phong, DateTime vao, string tt, DateTime? ra = null)
            => db.PhanPhongs.Add(new PhanPhong { DangKy = dk, Phong = Phong(phong), NgayVao = vao, NgayRa = ra, TrangThai = tt });

        void DangO(int stt, string phong, int soThangTruoc, LoaiPhong? nv)
        {
            var vao = homNay.AddMonths(-soThangTruoc).AddDays(-stt % 9);
            var dk = DK(SV(stt), TrangThaiDangKy.DaPhanPhong, vao, nv);
            PP(dk, phong, vao, TrangThaiPhanPhong.DangO);
        }

        // Phòng ĐẦY: A301 (2/2)
        DangO(1, "A301", 8, l2); DangO(2, "A301", 8, l2);
        // Phòng CÒN 1 CHỖ: A101 (3/4)
        DangO(3, "A101", 7, l4); DangO(4, "A101", 6, l4); DangO(5, "A101", 6, l4);
        // Phòng còn nhiều chỗ
        DangO(6, "A102", 5, l4);
        DangO(7, "C101", 4, l8); DangO(8, "C101", 3, l8);
        DangO(9, "A201", 2, l6);

        // SV10: đã CHUYỂN phòng A102 -> C201 (2 bản ghi lịch sử)
        {
            var vao = homNay.AddMonths(-5);
            var dk = DK(SV(10), TrangThaiDangKy.DaPhanPhong, vao, l4, "Xin chuyển gần khoa.");
            var ngayChuyen = homNay.AddMonths(-2);
            PP(dk, "A102", vao, TrangThaiPhanPhong.DaChuyen, ngayChuyen);
            PP(dk, "C201", ngayChuyen, TrangThaiPhanPhong.DangO);
        }
        // SV11, SV12: ĐÃ TRẢ phòng
        {
            var vao = homNay.AddMonths(-9);
            var dk = DK(SV(11), TrangThaiDangKy.DaTraPhong, vao, l6);
            PP(dk, "A202", vao, TrangThaiPhanPhong.DaTraPhong, homNay.AddMonths(-3));
            var vao2 = homNay.AddMonths(-8);
            var dk2 = DK(SV(12), TrangThaiDangKy.DaTraPhong, vao2, l8);
            PP(dk2, "C102", vao2, TrangThaiPhanPhong.DaTraPhong, homNay.AddMonths(-1));
        }
        // Chờ duyệt (nam): SV13, SV14, SV15 ; Đã hủy: SV16
        DK(SV(13), TrangThaiDangKy.ChoDuyet, homNay.AddDays(10), l4, "Mong được ở cùng lớp.");
        DK(SV(14), TrangThaiDangKy.ChoDuyet, homNay.AddDays(12), l6);
        DK(SV(15), TrangThaiDangKy.ChoDuyet, homNay.AddDays(15), l8, "Hoàn cảnh khó khăn, xin giá thấp.");
        DK(SV(16), TrangThaiDangKy.DaHuy, homNay.AddDays(20), l2, "Sinh viên tự hủy.");
        // SV17, SV18 chưa có đăng ký (dùng để kiểm thử đăng ký mới)

        // ----- Nữ -----
        // Phòng CÒN 1 CHỖ: B101 (3/4)
        DangO(19, "B101", 6, l4); DangO(20, "B101", 5, l4); DangO(21, "B101", 5, l4);
        // Phòng ĐẦY: D201 (2/2)
        DangO(22, "D201", 4, l2); DangO(23, "D201", 4, l2);
        DangO(24, "B201", 3, l6);
        // SV25: chuyển phòng B102 -> D102
        {
            var vao = homNay.AddMonths(-4);
            var dk = DK(SV(25), TrangThaiDangKy.DaPhanPhong, vao, l4);
            var ngayChuyen = homNay.AddMonths(-1);
            PP(dk, "B102", vao, TrangThaiPhanPhong.DaChuyen, ngayChuyen);
            PP(dk, "D102", ngayChuyen, TrangThaiPhanPhong.DangO);
        }
        // SV26: đã trả phòng
        {
            var vao = homNay.AddMonths(-7);
            var dk = DK(SV(26), TrangThaiDangKy.DaTraPhong, vao, l6);
            PP(dk, "B201", vao, TrangThaiPhanPhong.DaTraPhong, homNay.AddMonths(-2));
        }
        DK(SV(27), TrangThaiDangKy.ChoDuyet, homNay.AddDays(9), l4);
        DK(SV(28), TrangThaiDangKy.ChoDuyet, homNay.AddDays(14), l2, "Cần phòng yên tĩnh để học.");
        DK(SV(29), TrangThaiDangKy.DaHuy, homNay.AddDays(18), l6, "Đổi kế hoạch.");
        // SV30: chưa có đăng ký

        await db.SaveChangesAsync();
    }
}
