using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public static class Program
{
    public static NguoiDung? CurrentUser;

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.Title = "SUPERMARKET MANAGEMENT - ZTEAM (PRO VERSION)";

        Database.LoadDuLieu(); 

        while (true)
        {
            if (HienThiManHinhDangNhap())
            {
                ChuyenHuongMenu();
            }
            else
            {
                break;
            }
        }

        Database.LuuDuLieu();
        Console.WriteLine("Đã lưu dữ liệu. Tạm biệt!");
    }

    private static bool HienThiManHinhDangNhap()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ______  _                          
 |___  / | |                         
    / /  | |_   ___   __ _  _ __ ___ 
   / /   | __| / _ \ / _` || '_ ` _ \
  / /__  | |_ |  __/| (_| || | | | | |
 /_____|  \__| \___| \__,_||_| |_| |_|
                                      
");
            Console.WriteLine("=== ĐĂNG NHẬP HỆ THỐNG ===");
            Console.ResetColor();

            string? user = ConsoleUI.DocChuoi("Tài khoản (để trống để thoát): ");
            if (string.IsNullOrEmpty(user)) return false;

            string? pass = ConsoleUI.DocMatKhau("Mật khẩu: ");

            var u = Database.NguoiDungs.FirstOrDefault(x => x.Username == user && x.Password == pass);
            
            if (u != null)
            {
                CurrentUser = u;
                ConsoleUI.HienThiThongBao($"Xin chào {u.HoTen} ({u.Role})!", ConsoleColor.Green);
                return true;
            }
            else
            {
                ConsoleUI.HienThiThongBao("Sai tài khoản hoặc mật khẩu!", ConsoleColor.Red);
            }
        }
    }

    private static void ChuyenHuongMenu()
    {
        if (CurrentUser == null) return;

        switch (CurrentUser.Role)
        {
            case VaiTro.Admin:
                MenuAdmin();
                break;
            case VaiTro.ThuNgan:
                QuanLyBanHang.HienThiMenu(); 
                CurrentUser = null;
                break;
            case VaiTro.ThuKho:
                QuanLyKho.HienThiMenu();
                CurrentUser = null;
                break;
        }
    }

    private static void MenuAdmin()
    {
        bool dangChay = true;
        var menu = new List<string>
        {
            "1. Quản lý Người dùng (Reset Pass/Cấp quyền)",
            "2. Quản lý Sản phẩm (Full CRUD)",
            "3. Quản lý Kho (Nhập/Xuất/Báo cáo)",
            "4. Tra cứu Hóa đơn & Lịch sử GD (NEW)",
            "5. Vào Quầy Thu Ngân (POS)",
            "6. Thư viện Combo (Backtracking)",
            "7. Kiểm thử Hiệu năng & Data",
            "8. Đổi mật khẩu cá nhân", // [MỚI]
            "0. Đăng xuất"
        };

        while (dangChay)
        {
            int chon = ConsoleUI.HienThiMenuChon($"ADMIN CONTROL PANEL - {CurrentUser?.HoTen}", menu);
            switch (chon)
            {
                case 0: QuanLyNguoiDung.HienThiMenu(); break;
                case 1: QuanLySanPham.HienThiMenu(); break;
                case 2: QuanLyKho.HienThiMenu(); break;
                case 3: QuanLyHoaDon.HienThiMenu(); break; 
                case 4: QuanLyBanHang.HienThiMenu(); break;
                case 5: ThuVienCombo.HienThiMenu(); break;
                case 6: KiemThu.HienThiMenu(); break;
                case 7: DoiMatKhau(); break; // [MỚI]
                case 8: dangChay = false; CurrentUser = null; break;
                case -1: dangChay = false; CurrentUser = null; break;
            }
        }
    }

    // --- [MỚI] CHỨC NĂNG ĐỔI MẬT KHẨU (DÙNG CHUNG) ---
    public static void DoiMatKhau()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("ĐỔI MẬT KHẨU CÁ NHÂN");

        if (CurrentUser == null) return;

        // 1. Xác thực mật khẩu cũ
        string? passCu = ConsoleUI.DocMatKhau("Nhập mật khẩu cũ: ");
        if (passCu != CurrentUser.Password)
        {
            ConsoleUI.HienThiThongBao("Mật khẩu cũ không đúng!", ConsoleColor.Red);
            return;
        }

        // 2. Nhập mật khẩu mới
        string? passMoi = ConsoleUI.DocMatKhau("Nhập mật khẩu mới: ");
        if (string.IsNullOrEmpty(passMoi) || passMoi.Length < 1)
        {
            ConsoleUI.HienThiThongBao("Mật khẩu không được để trống!", ConsoleColor.Red);
            return;
        }
        
        if (passMoi == passCu)
        {
            ConsoleUI.HienThiThongBao("Mật khẩu mới không được trùng mật khẩu cũ!", ConsoleColor.Red);
            return;
        }

        string? xacNhan = ConsoleUI.DocMatKhau("Nhập lại mật khẩu mới: ");
        if (passMoi != xacNhan)
        {
            ConsoleUI.HienThiThongBao("Mật khẩu xác nhận không khớp!", ConsoleColor.Red);
            return;
        }

        // 3. Cập nhật và Lưu
        CurrentUser.Password = passMoi;
        
        // Cập nhật vào danh sách gốc trong Database (do CurrentUser là tham chiếu nhưng tìm lại cho chắc chắn)
        var userInDb = Database.NguoiDungs.FirstOrDefault(u => u.Username == CurrentUser.Username);
        if (userInDb != null) userInDb.Password = passMoi;
        
        Database.LuuDuLieu();
        ConsoleUI.HienThiThongBao("Đổi mật khẩu thành công!", ConsoleColor.Green);
    }
}