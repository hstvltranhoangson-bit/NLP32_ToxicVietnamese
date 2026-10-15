// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

/// <summary>Dữ liệu cho partial _PhanTrang: RouteValues chứa toàn bộ điều kiện tìm kiếm/lọc/sắp xếp cần giữ khi chuyển trang.</summary>
public class PhanTrangViewModel
{
    public int Trang { get; set; }
    public int TongTrang { get; set; }
    public int TongSo { get; set; }
    public Dictionary<string, string> RouteValues { get; set; } = new();

    public static PhanTrangViewModel Tao<T>(PagedList<T> ds, Dictionary<string, string> routeValues) => new()
    {
        Trang = ds.Trang,
        TongTrang = ds.TongTrang,
        TongSo = ds.TongSo,
        RouteValues = routeValues
    };
}
