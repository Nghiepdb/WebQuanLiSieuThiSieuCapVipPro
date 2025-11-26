using System;
using System.Collections.Generic;
using System.Linq; 

/// <summary>
/// Module "đối thủ" sử dụng List (O(N)) để so sánh hiệu năng.
/// Đã được nâng cấp để thực hiện logic chặt chẽ giống Dictionary.
/// </summary>
public static class QuanLySanPham_List
{
    public static List<SanPham> dsSanPham = new List<SanPham>();

    public static void XoaTatCa()
    {
        dsSanPham.Clear();
    }

    // --- CRUD VỚI ĐỘ PHỨC TẠP O(N) ---

    /// <summary>
    /// Thêm sản phẩm. 
    /// List phải duyệt toàn bộ (O(N)) để kiểm tra trùng mã trước khi thêm.
    /// </summary>
    public static bool ThemSanPham(SanPham sp)
    {
        // Phải quét cả danh sách để xem mã đã có chưa (O(N))
        // Nếu bỏ dòng này, List thêm rất nhanh (O(1)) nhưng dữ liệu sẽ bị trùng -> Sai logic nghiệp vụ
        if (dsSanPham.Any(p => p.MaSP == sp.MaSP)) 
        {
            return false;
        }
        
        dsSanPham.Add(sp);
        return true;
    }

    /// <summary>
    /// Tìm theo Mã. Phải duyệt tuần tự (O(N)).
    /// </summary>
    public static bool TimTheoMa(string maSP, out SanPham timThay)
    {
        // Duyệt từ đầu đến cuối để tìm
        foreach (var sp in dsSanPham)
        {
            if (sp.MaSP == maSP)
            {
                timThay = sp;
                return true;
            }
        }
        
        timThay = default;
        return false;
    }

    /// <summary>
    /// Tìm theo Tên. Phải duyệt tuần tự (O(N)).
    /// </summary>
    public static List<SanPham> TimTheoTen(string tenSP)
    {
        string tenKey = tenSP.ToLowerInvariant();
        return dsSanPham.Where(sp => sp.TenSP.ToLowerInvariant() == tenKey).ToList();
    }

    /// <summary>
    /// Cập nhật sản phẩm. Phải tìm vị trí (O(N)) rồi mới sửa.
    /// </summary>
    public static bool SuaSanPham(SanPham spMoi)
    {
        // FindIndex mất O(N)
        int index = dsSanPham.FindIndex(sp => sp.MaSP == spMoi.MaSP);
        
        if (index != -1)
        {
            dsSanPham[index] = spMoi;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Xóa sản phẩm. Phải tìm (O(N)) và dời chỗ các phần tử sau đó (O(N)).
    /// </summary>
    public static bool XoaSanPham(string maSP)
    {
        // RemoveAll duyệt và xóa, độ phức tạp O(N)
        int soLuongXoa = dsSanPham.RemoveAll(sp => sp.MaSP == maSP);
        return soLuongXoa > 0;
    }
}