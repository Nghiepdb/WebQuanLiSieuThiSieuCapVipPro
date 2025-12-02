using System;

/// <summary>
/// Định nghĩa cấu trúc dữ liệu cho Mặt hàng (Sản phẩm).
/// Sản phẩm đóng vai trò là danh mục gốc. 
/// </summary>
public struct SanPham
{
    public string MaSP { get; set; }
    public string TenSP { get; set; }
    public string DonViTinh { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTonKho { get; set; }
    public string DanhMuc { get; set; }
    
    // [MỚI] Hạn sử dụng (Date) để hiển thị trực quan
    public DateTime HanSuDung { get; set; }

    public bool IsDeleted { get; set; }

    public SanPham(string maSP, string tenSP, string donViTinh, decimal giaBan, int soLuongTonKho, string danhMuc, DateTime hsd)
    {
        MaSP = maSP;
        TenSP = tenSP;
        DonViTinh = donViTinh;
        GiaBan = giaBan;
        SoLuongTonKho = soLuongTonKho;
        DanhMuc = danhMuc;
        HanSuDung = hsd;
        IsDeleted = false; 
    }
}