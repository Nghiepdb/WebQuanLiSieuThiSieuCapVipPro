using System;
using System.Collections.Generic;
using System.Linq;

public static class QuanLyNguoiDung
{
    public static void HienThiMenu()
    {
        bool dangChay = true;
        while (dangChay)
        {
            var menu = new List<string>
            {
                "1. Danh sách nhân viên",
                "2. Thêm nhân viên mới",
                "3. Sửa thông tin nhân viên (Tên/Quyền)", // [MỚI]
                "4. Xóa nhân viên (Có tìm kiếm)",         // [NÂNG CẤP]
                "5. Reset mật khẩu nhân viên",            // [NÂNG CẤP]
                "0. Quay lại"
            };

            int chon = ConsoleUI.HienThiMenuChon("QUẢN LÝ NGƯỜI DÙNG", menu);
            switch (chon)
            {
                case 0: HienThiDanhSach(); break;
                case 1: ThemNguoiDung(); break;
                case 2: SuaNguoiDung(); break; // [MỚI]
                case 3: XoaNguoiDung(); break;
                case 4: ResetMatKhau(); break;
                case 5: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    private static void HienThiDanhSach()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("DANH SÁCH NHÂN SỰ");
        // Hiển thị danh sách tìm kiếm luôn cho tiện
        ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "DANH SÁCH & TÌM KIẾM NHÂN SỰ");
    }

    private static void ThemNguoiDung()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("THÊM NHÂN VIÊN MỚI");

        string? user = ConsoleUI.DocChuoi("Username (Viết liền, không dấu): ", "", true);
        if (Database.NguoiDungs.Any(x => x.Username == user))
        {
            ConsoleUI.HienThiThongBao("Username đã tồn tại!", ConsoleColor.Red);
            return;
        }

        string? pass = ConsoleUI.DocMatKhau("Mật khẩu khởi tạo: ");
        string? name = ConsoleUI.DocChuoi("Họ và tên hiển thị: ", "", true);
        
        Console.WriteLine("Chọn vai trò:");
        Console.WriteLine("1. Admin (Quản trị)");
        Console.WriteLine("2. ThuNgan (Bán hàng)");
        Console.WriteLine("3. ThuKho (Kho hàng)");
        int? roleOpt = ConsoleUI.DocSoNguyen("Lựa chọn (1-3): ");

        VaiTro role = VaiTro.ThuNgan;
        if (roleOpt == 1) role = VaiTro.Admin;
        else if (roleOpt == 3) role = VaiTro.ThuKho;

        Database.NguoiDungs.Add(new NguoiDung(user, pass, name, role));
        Database.LuuDuLieu();
        ConsoleUI.HienThiThongBao("Thêm thành công!", ConsoleColor.Green);
    }

    // --- [MỚI] SỬA THÔNG TIN NHÂN VIÊN ---
    private static void SuaNguoiDung()
    {
        var u = ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "CHỌN NHÂN VIÊN CẦN SỬA");
        if (u == null) return;

        Console.Clear();
        ConsoleUI.VeTieuDe($"SỬA THÔNG TIN: {u.Username}");
        
        string? nameMoi = ConsoleUI.DocChuoi($"Họ tên mới ({u.HoTen}): ", u.HoTen);
        
        Console.WriteLine($"Vai trò hiện tại: {u.Role}");
        Console.WriteLine("Chọn vai trò mới (Để trống giữ nguyên):");
        Console.WriteLine("1. Admin");
        Console.WriteLine("2. ThuNgan");
        Console.WriteLine("3. ThuKho");
        int? roleOpt = ConsoleUI.DocSoNguyen("Lựa chọn (1-3): ");

        if (ConsoleUI.XacNhan("Lưu thay đổi?"))
        {
            if (!string.IsNullOrEmpty(nameMoi)) u.HoTen = nameMoi;
            if (roleOpt.HasValue)
            {
                if (roleOpt == 1) u.Role = VaiTro.Admin;
                else if (roleOpt == 2) u.Role = VaiTro.ThuNgan;
                else if (roleOpt == 3) u.Role = VaiTro.ThuKho;
            }

            // Update lại trong List (Tham chiếu)
            Database.LuuDuLieu();
            ConsoleUI.HienThiThongBao("Cập nhật thành công!", ConsoleColor.Green);
        }
    }

    private static void XoaNguoiDung()
    {
        // [NÂNG CẤP] Sử dụng tìm kiếm Real-time thay vì gõ tay
        var u = ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "CHỌN NHÂN VIÊN CẦN XÓA");
        Console.Clear();
        
        if (u == null) return;

        // Bảo vệ: Không cho xóa chính mình
        if (u.Username == Program.CurrentUser?.Username)
        {
            ConsoleUI.HienThiThongBao("Không thể tự xóa chính mình!", ConsoleColor.Red);
            return;
        }

        // Bảo vệ: Phải còn ít nhất 1 Admin
        if (u.Role == VaiTro.Admin && Database.NguoiDungs.Count(x => x.Role == VaiTro.Admin) <= 1)
        {
            ConsoleUI.HienThiThongBao("Không thể xóa Admin cuối cùng của hệ thống!", ConsoleColor.Red);
            return;
        }

        if (ConsoleUI.XacNhan($"Bạn CHẮC CHẮN muốn xóa {u.HoTen} ({u.Username})?"))
        {
            Database.NguoiDungs.Remove(u);
            Database.LuuDuLieu();
            ConsoleUI.HienThiThongBao("Đã xóa!", ConsoleColor.Green);
        }
    }

    private static void ResetMatKhau()
    {
        // [NÂNG CẤP] Sử dụng tìm kiếm Real-time
        var u = ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "CHỌN NHÂN VIÊN CẦN RESET PASS");
        
        if (u == null) return;

        Console.Clear();
        ConsoleUI.VeTieuDe("XÁC NHẬN RESET MẬT KHẨU");
        Console.WriteLine($"Tài khoản: {u.Username}");
        Console.WriteLine($"Họ tên   : {u.HoTen}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nLưu ý: Mật khẩu sẽ về mặc định '123456'.");
        Console.ResetColor();

        if (ConsoleUI.XacNhan("Tiến hành Reset?"))
        {
            u.Password = "123456"; 
            Database.LuuDuLieu();
            ConsoleUI.HienThiThongBao("Thành công! Pass mới: 123456", ConsoleColor.Green);
        }
    }
}