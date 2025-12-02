using System;
using System.Collections.Generic;

// --- QUẢN LÝ NGƯỜI DÙNG (AUTH) ---
public enum VaiTro
{
    Admin,      // Quản trị viên: Full quyền
    ThuNgan,    // Cashier: Chỉ bán hàng
    ThuKho      // Warehouse: Chỉ quản lý kho
}

public class NguoiDung
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = ""; 
    public string HoTen { get; set; } = "";
    public VaiTro Role { get; set; }

    public NguoiDung() { }

    public NguoiDung(string user, string pass, string name, VaiTro role)
    {
        Username = user;
        Password = pass;
        HoTen = name;
        Role = role;
    }
}

// --- QUẢN LÝ KHÁCH HÀNG (CRM) ---
public enum HangKhachHang
{
    ThanThiet, // Mặc định
    VIP,       // Mua > 10tr: Giảm 5%
    SVIP       // Mua > 50tr: Giảm 10%
}

public class KhachHang
{
    public string SoDienThoai { get; set; } = "";
    public string TenKhach { get; set; } = "";
    public decimal TongChiTieu { get; set; }
    public HangKhachHang Hang { get; set; } = HangKhachHang.ThanThiet;

    public void CapNhatHang()
    {
        if (TongChiTieu >= 50000000) Hang = HangKhachHang.SVIP;
        else if (TongChiTieu >= 10000000) Hang = HangKhachHang.VIP;
        else Hang = HangKhachHang.ThanThiet;
    }

    public decimal LayPhanTramGiamGia()
    {
        switch (Hang)
        {
            case HangKhachHang.VIP: return 0.05m; // 5%
            case HangKhachHang.SVIP: return 0.10m; // 10%
            default: return 0;
        }
    }
}

// --- QUẢN LÝ KHO (BATCH MANAGEMENT) ---
public class LoHang
{
    public string MaLo { get; set; } = "";      
    public string MaSP { get; set; } = "";
    public int SoLuong { get; set; }      
    public DateTime NgayNhap { get; set; }
    public DateTime HanSuDung { get; set; }

    public LoHang() { }

    public LoHang(string maLo, string maSP, int soLuong, DateTime hsd)
    {
        MaLo = maLo;
        MaSP = maSP;
        SoLuong = soLuong;
        NgayNhap = DateTime.Now;
        HanSuDung = hsd;
    }
}

// --- QUẢN LÝ CA LÀM VIỆC (SHIFT) ---
public class PhienLamViec
{
    public string MaPhien { get; set; } = "";       
    public string UsernameNhanVien { get; set; } = "";
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime? ThoiGianKetThuc { get; set; }
    
    public decimal TienDauCa { get; set; }
    public decimal TienKetCaHeThong { get; set; } 
    public decimal TienKetCaThucTe { get; set; }  
    
    public decimal ChenhLech => TienKetCaThucTe - TienKetCaHeThong;

    public PhienLamViec() { }
}

// --- HÓA ĐƠN & CHI TIẾT ---
public class HoaDon
{
    public string MaHoaDon { get; set; } = "";
    public string NguoiTao { get; set; } = ""; 
    public string? SdtKhachHang { get; set; } 
    public DateTime NgayLap { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal GiamGia { get; set; }
    public decimal ThanhTien => TongTienHang - GiamGia;
    public List<ChiTietHoaDon> ChiTiet { get; set; } = new List<ChiTietHoaDon>();
}

public class ChiTietHoaDon
{
    public string MaSP { get; set; } = "";
    public string TenSP { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}

// --- PHIẾU TRẢ HÀNG ---
public class PhieuTraHang
{
    public string MaPhieu { get; set; } = "";
    public string MaHoaDonGoc { get; set; } = "";
    public string NguoiXuLy { get; set; } = "";
    public DateTime NgayTra { get; set; }
    
    public string MaSP { get; set; } = "";
    public string TenSP { get; set; } = "";
    public int SoLuongTra { get; set; }
    public decimal SoTienHoan { get; set; }
    public string LyDo { get; set; } = "";

    public PhieuTraHang() { }
}