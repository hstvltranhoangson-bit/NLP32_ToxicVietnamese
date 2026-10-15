// Họ và tên: [Cả nhóm - điền họ tên các thành viên]
// Mã sinh viên: [MSSV các thành viên]
// Nội dung thực hiện: Thiết kế CSDL, Entity, DbContext, Migration, cấu hình chung
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Constants;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Models;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Data;

/// <summary>DbContext của hệ thống: khai báo DbSet và cấu hình quan hệ, ràng buộc (Fluent API).</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<PhongKTX> PhongKTXs => Set<PhongKTX>();
    public DbSet<SinhVien> SinhViens => Set<SinhVien>();
    public DbSet<DangKyKTX> DangKyKTXs => Set<DangKyKTX>();
    public DbSet<PhanPhong> PhanPhongs => Set<PhanPhong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TaiKhoan (1) - (1) SinhVien
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.HasIndex(x => x.TenDangNhap).IsUnique();
            e.HasOne(x => x.SinhVien)
             .WithOne(s => s.TaiKhoan)
             .HasForeignKey<SinhVien>(s => s.MaTaiKhoan)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SinhVien>(e =>
        {
            e.HasIndex(x => x.MaTaiKhoan).IsUnique();
        });

        modelBuilder.Entity<LoaiPhong>(e =>
        {
            e.HasIndex(x => x.TenLoaiPhong).IsUnique();
            e.Property(x => x.DonGiaThang).HasColumnType("decimal(18,2)");
        });

        // LoaiPhong (1) - (n) PhongKTX
        modelBuilder.Entity<PhongKTX>(e =>
        {
            e.HasIndex(x => x.TenPhong).IsUnique();
            e.HasOne(x => x.LoaiPhong)
             .WithMany(l => l.PhongKTXs)
             .HasForeignKey(x => x.MaLoaiPhong)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // SinhVien (1) - (n) DangKyKTX
        modelBuilder.Entity<DangKyKTX>(e =>
        {
            e.HasOne(x => x.SinhVien)
             .WithMany(s => s.DangKyKTXs)
             .HasForeignKey(x => x.MaSinhVien)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LoaiPhongNguyenVong)
             .WithMany()
             .HasForeignKey(x => x.NguyenVongLoaiPhong)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // DangKyKTX (1) - (n) PhanPhong ; PhongKTX (1) - (n) PhanPhong
        modelBuilder.Entity<PhanPhong>(e =>
        {
            e.HasOne(x => x.DangKy)
             .WithMany(d => d.PhanPhongs)
             .HasForeignKey(x => x.MaDangKy)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Phong)
             .WithMany(p => p.PhanPhongs)
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.Restrict);

            // Một đăng ký tại một thời điểm chỉ có tối đa 1 bản ghi "Đang ở".
            e.HasIndex(x => x.MaDangKy)
             .IsUnique()
             .HasFilter($"[TrangThai] = N'{TrangThaiPhanPhong.DangO}'");
        });
    }
}
