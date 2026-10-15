// Họ và tên: [Họ tên Sinh viên 2]
// Mã sinh viên: [MSSV 2]
// Nội dung thực hiện: Module 2 - Quản lý phòng KTX, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyKyTucXa_UNETI_DHTI17A2ND.Helpers;

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.ViewModels;

public class PhongListViewModel
{
    public PagedList<PhongItemViewModel> KetQua { get; set; } = new(new List<PhongItemViewModel>(), 0, 1, 1);
    public PhongFilterViewModel Filter { get; set; } = new();
    public bool LaAdmin { get; set; }

    public List<SelectListItem> LoaiPhongs { get; set; } = new();
    public List<SelectListItem> ToaNhas { get; set; } = new();
    public List<SelectListItem> Tangs { get; set; } = new();
}
