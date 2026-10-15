// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

/// <summary>Kết quả phân trang: dữ liệu của 1 trang + thông tin tổng.</summary>
public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Trang { get; }
    public int KichThuocTrang { get; }
    public int TongSo { get; }
    public int TongTrang => KichThuocTrang <= 0 ? 0 : (int)Math.Ceiling(TongSo / (double)KichThuocTrang);

    public PagedList(IReadOnlyList<T> items, int tongSo, int trang, int kichThuocTrang)
    {
        Items = items;
        TongSo = tongSo;
        Trang = trang;
        KichThuocTrang = kichThuocTrang;
    }

    /// <summary>Chuẩn hóa số trang về khoảng hợp lệ [1, tổng trang].</summary>
    public static int ChuanHoaTrang(int trang, int tongSo, int kichThuocTrang)
    {
        var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)kichThuocTrang));
        return Math.Min(Math.Max(trang, 1), tongTrang);
    }
}
