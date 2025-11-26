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
                "3. Sửa thông tin nhân viên (Tên/Quyền)", 
                "4. Xóa nhân viên (Có tìm kiếm)",         
                "5. Reset mật khẩu nhân viên",            
                "0. Quay lại"
            };

            int chon = ConsoleUI.HienThiMenuChon("QUẢN LÝ NGƯỜI DÙNG", menu);
            switch (chon)
            {
                case 0: HienThiDanhSach(); break;
                case 1: ThemNguoiDung(); break;
                case 2: SuaNguoiDung(); break; 
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
        ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "DANH SÁCH & TÌM KIẾM NHÂN SỰ");
    }

    private static void ThemNguoiDung()
    {
        string? user = "";
        string? pass = "";
        string? name = "";
        VaiTro role = VaiTro.ThuNgan;

        // --- BƯỚC 1: NHẬP USERNAME ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("THÊM NHÂN VIÊN MỚI - BƯỚC 1/4");
            user = ConsoleUI.DocChuoi("Username (Viết liền, không dấu): ", "", true);
            
            if (user == null) return; // ESC -> Thoát

            if (string.IsNullOrWhiteSpace(user))
            {
                ConsoleUI.HienThiThongBao("Username không được để trống!", ConsoleColor.Red);
                continue;
            }

            if (Database.NguoiDungs.Any(x => x.Username == user))
            {
                ConsoleUI.HienThiThongBao("Username đã tồn tại! Vui lòng chọn tên khác.", ConsoleColor.Red);
                continue;
            }
            break; 
        }

        // --- BƯỚC 2: NHẬP MẬT KHẨU ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("THÊM NHÂN VIÊN MỚI - BƯỚC 2/4");
            Console.WriteLine($"Username: {user}");
            ConsoleUI.KeVienNgang();

            pass = ConsoleUI.DocMatKhau("Mật khẩu khởi tạo: ");
            if (pass == null) return; // ESC

            if (string.IsNullOrWhiteSpace(pass))
            {
                ConsoleUI.HienThiThongBao("Mật khẩu không được để trống!", ConsoleColor.Red);
                continue;
            }
            break;
        }

        // --- BƯỚC 3: NHẬP HỌ TÊN ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("THÊM NHÂN VIÊN MỚI - BƯỚC 3/4");
            Console.WriteLine($"Username: {user}");
            Console.WriteLine($"Mật khẩu : ******");
            ConsoleUI.KeVienNgang();

            name = ConsoleUI.DocChuoi("Họ và tên hiển thị: ", "", true);
            if (name == null) return;

            if (string.IsNullOrWhiteSpace(name))
            {
                ConsoleUI.HienThiThongBao("Họ tên không được để trống!", ConsoleColor.Red);
                continue;
            }
            break;
        }

        // --- BƯỚC 4: CHỌN VAI TRÒ (FIX LỖI ENTER) ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("THÊM NHÂN VIÊN MỚI - BƯỚC 4/4");
            Console.WriteLine($"Username: {user}");
            Console.WriteLine($"Họ tên   : {name}");
            ConsoleUI.KeVienNgang();
            
            Console.WriteLine("Chọn vai trò:");
            Console.WriteLine("1. Admin (Quản trị)");
            Console.WriteLine("2. ThuNgan (Bán hàng)");
            Console.WriteLine("3. ThuKho (Kho hàng)");
            
            // Dùng DocChuoi để bắt được Enter rỗng
            string? input = ConsoleUI.DocChuoi("Lựa chọn (1-3): ");
            
            if (input == null) return; // ESC -> Thoát

            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleUI.HienThiThongBao("Vui lòng nhập lựa chọn (1, 2 hoặc 3)!", ConsoleColor.Red);
                continue;
            }

            if (input == "1") { role = VaiTro.Admin; break; }
            if (input == "2") { role = VaiTro.ThuNgan; break; }
            if (input == "3") { role = VaiTro.ThuKho; break; }

            ConsoleUI.HienThiThongBao("Lựa chọn không hợp lệ!", ConsoleColor.Red);
        }

        // --- LƯU DỮ LIỆU ---
        Database.NguoiDungs.Add(new NguoiDung(user, pass, name, role));
        Database.LuuDuLieu();
        ConsoleUI.HienThiThongBao($"Thêm thành công nhân viên {user}!", ConsoleColor.Green);
    }

    // --- SỬA THÔNG TIN NHÂN VIÊN ---
    private static void SuaNguoiDung()
    {
        var u = ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "CHỌN NHÂN VIÊN CẦN SỬA");
        if (u == null) return;

        Console.Clear();
        ConsoleUI.VeTieuDe($"SỬA THÔNG TIN: {u.Username}");
        
        string? nameMoi = ConsoleUI.DocChuoi($"Họ tên mới ({u.HoTen}): ", u.HoTen);
        if (nameMoi == null) return; // ESC
        
        Console.WriteLine($"Vai trò hiện tại: {u.Role}");
        Console.WriteLine("Chọn vai trò mới (Để trống giữ nguyên):");
        Console.WriteLine("1. Admin");
        Console.WriteLine("2. ThuNgan");
        Console.WriteLine("3. ThuKho");
        
        int? roleOpt = ConsoleUI.DocSoNguyen("Lựa chọn (1-3): ");
        // Enter -> null -> Giữ nguyên (Logic sửa)
        // ESC -> null -> Giữ nguyên (Tạm chấp nhận ở bước Sửa vì không quan trọng bằng Thêm)

        if (ConsoleUI.XacNhan("Lưu thay đổi?"))
        {
            if (!string.IsNullOrEmpty(nameMoi)) u.HoTen = nameMoi;
            if (roleOpt.HasValue)
            {
                if (roleOpt == 1) u.Role = VaiTro.Admin;
                else if (roleOpt == 2) u.Role = VaiTro.ThuNgan;
                else if (roleOpt == 3) u.Role = VaiTro.ThuKho;
            }

            Database.LuuDuLieu();
            ConsoleUI.HienThiThongBao("Cập nhật thành công!", ConsoleColor.Green);
        }
    }

    private static void XoaNguoiDung()
    {
        var u = ConsoleUI.ChonMotNguoiDungVoiBoLocRealTime(Database.NguoiDungs, "CHỌN NHÂN VIÊN CẦN XÓA");
        Console.Clear();
        
        if (u == null) return;

        if (u.Username == Program.CurrentUser?.Username)
        {
            ConsoleUI.HienThiThongBao("Không thể tự xóa chính mình!", ConsoleColor.Red);
            return;
        }

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