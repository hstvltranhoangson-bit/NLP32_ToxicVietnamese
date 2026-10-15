// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

public static class PagedQueryExtensions
{
    /// <summary>
    /// Phân trang NGAY TRÊN truy vấn EF Core: CountAsync + Skip().Take() -> chỉ lấy đúng các dòng của trang.
    /// </summary>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, int trang, int kichThuocTrang)
    {
        var tongSo = await query.CountAsync();
        trang = PagedList<T>.ChuanHoaTrang(trang, tongSo, kichThuocTrang);
        var items = await query.Skip((trang - 1) * kichThuocTrang).Take(kichThuocTrang).ToListAsync();
        return new PagedList<T>(items, tongSo, trang, kichThuocTrang);
    }
}
