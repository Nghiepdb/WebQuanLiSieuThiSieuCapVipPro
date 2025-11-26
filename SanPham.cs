using System;

/// <summary>
/// Định nghĩa cấu trúc dữ liệu cho Mặt hàng (Sản phẩm).
/// Sản phẩm đóng vai trò là danh mục gốc. 
/// Số lượng tồn kho tổng thể sẽ được tính tổng từ các Lô Hàng (LoHang) trong Models.cs.
/// </summary>
public struct SanPham
{
    public string MaSP { get; set; }
    public string TenSP { get; set; }
    public string DonViTinh { get; set; }
    public decimal GiaBan { get; set; }
    
    // Số lượng tồn kho hiển thị (Cache). 
    // Trong logic mới, giá trị này nên được cập nhật mỗi khi nhập/xuất Lô hàng.
    public int SoLuongTonKho { get; set; }
    
    public string DanhMuc { get; set; }

    // [NÂNG CẤP] Cờ đánh dấu đã xóa (Soft Delete)
    // false: Đang hiển thị, true: Đã vào thùng rác
    public bool IsDeleted { get; set; }

    public SanPham(string maSP, string tenSP, string donViTinh, decimal giaBan, int soLuongTonKho, string danhMuc = "Chưa phân loại")
    {
        MaSP = maSP;
        TenSP = tenSP;
        DonViTinh = donViTinh;
        GiaBan = giaBan;
        SoLuongTonKho = soLuongTonKho;
        DanhMuc = danhMuc;
        IsDeleted = false; // Mặc định là chưa xóa
    }
}