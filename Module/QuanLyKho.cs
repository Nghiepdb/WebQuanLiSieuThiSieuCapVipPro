using System;
using System.Collections.Generic;
using System.Linq;

public static class QuanLyKho
{
    public static void HienThiMenu()
    {
        bool dangChay = true;
        var cacLuaChon = new List<string>
        {
            "1. Cảnh báo hàng sắp hết (Tồn < 10)",
            "2. Thống kê tổng giá trị kho hàng",
            "3. Nhập thêm hàng (Tìm kiếm Real-time)",
            "4. Đổi mật khẩu cá nhân", // [MỚI]
            "0. Quay lại Menu chính"
        };

        while (dangChay)
        {
            int luaChon = ConsoleUI.HienThiMenuChon("== QUẢN LÝ KHO & THỐNG KÊ ==", cacLuaChon);
            switch (luaChon)
            {
                case 0: CanhBaoHangSapHet(); break;
                case 1: ThongKeTongGiaTri(); break;
                case 2: NhapHang(); break;
                case 3: Program.DoiMatKhau(); break; // [MỚI] Gọi hàm từ Program
                case 4: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    private static void CanhBaoHangSapHet()
    {
        // Chỉ lọc sản phẩm chưa xóa
        var listSP = QuanLySanPham.dsTheoMa.Values
            .Where(p => !p.IsDeleted && p.SoLuongTonKho < 10)
            .OrderBy(p => p.SoLuongTonKho)
            .ToList();

        if (listSP.Count == 0)
        {
            ConsoleUI.HienThiThongBao("Kho hàng ổn định. Không có hàng sắp hết.", ConsoleColor.Green);
            return;
        }
        ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "DANH SÁCH HÀNG SẮP HẾT (CẦN NHẬP)");
    }

    private static void ThongKeTongGiaTri()
    {
        Console.Clear();
        // Chỉ tính tổng giá trị hàng chưa xóa
        var dsHieuLuc = QuanLySanPham.dsTheoMa.Values.Where(p => !p.IsDeleted).ToList();
        
        decimal tong = dsHieuLuc.Sum(sp => sp.GiaBan * sp.SoLuongTonKho);
        
        Console.WriteLine($"Tổng mặt hàng (Khả dụng): {dsHieuLuc.Count:N0}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"TỔNG GIÁ TRỊ KHO: {tong:N0} VNĐ");
        Console.ResetColor();
        ConsoleUI.HienThiThongBao("Thống kê xong.", ConsoleColor.Yellow);
    }

    private static void NhapHang()
    {
        // Chỉ cho phép nhập hàng chưa xóa
        var listSP = QuanLySanPham.dsTheoMa.Values.Where(p => !p.IsDeleted).ToList();
        
        if (listSP.Count == 0) { ConsoleUI.HienThiThongBao("Kho trống.", ConsoleColor.Red); return; }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "CHỌN SẢN PHẨM ĐỂ NHẬP KHO");

        if (spChon.HasValue)
        {
            SanPham sp = spChon.Value;
            Console.Clear();
            Console.WriteLine($"Đang nhập hàng cho: {sp.TenSP}");
            Console.WriteLine($"Tồn hiện tại: {sp.SoLuongTonKho}");
            
            int? slThem = ConsoleUI.DocSoNguyen("Nhập số lượng muốn thêm: ");
            if (slThem != null && slThem > 0)
            {
                sp.SoLuongTonKho += slThem.Value;
                QuanLySanPham.dsTheoMa[sp.MaSP] = sp;
                ConsoleUI.HienThiThongBao($"Đã nhập thêm {slThem}. Tồn mới: {sp.SoLuongTonKho}", ConsoleColor.Green);
            }
        }
        Database.LuuDuLieu();
    }
}