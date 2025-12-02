using System;
using System.Collections.Generic;
using System.Linq;

public static class QuanLyKhachHang
{
    public static void XemDanhSachKhachHang()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("DANH SÁCH KHÁCH HÀNG THÂN THIẾT (ADMIN ONLY)");

        var dsKhach = Database.KhachHangs.OrderByDescending(k => k.TongChiTieu).ToList();

        if (dsKhach.Count == 0)
        {
            ConsoleUI.HienThiThongBao("Chưa có dữ liệu khách hàng.", ConsoleColor.Yellow);
            return;
        }

        Console.WriteLine($"Tổng số: {dsKhach.Count} khách hàng.");
        ConsoleUI.KeVienNgang();
        Console.WriteLine($"{"Họ và Tên",-25} | {"Số Điện Thoại",-15} | {"Tổng Chi Tiêu",15} | {"Hạng",-10}");
        ConsoleUI.KeVienNgang();

        // Phân trang
        int trang = 0;
        int size = 10;
        int maxTrang = (int)Math.Ceiling((double)dsKhach.Count / size);

        while (true)
        {
            int top = Console.CursorTop;
            var view = dsKhach.Skip(trang * size).Take(size).ToList();

            foreach (var kh in view)
            {
                ConsoleColor mauHang = kh.Hang == HangKhachHang.SVIP ? ConsoleColor.Red : 
                                      (kh.Hang == HangKhachHang.VIP ? ConsoleColor.Yellow : ConsoleColor.White);
                
                Console.Write($" {kh.TenKhach,-24} | {kh.SoDienThoai,-15} | {kh.TongChiTieu,15:N0} | ");
                ConsoleUI.DatMau(mauHang);
                Console.WriteLine($"{kh.Hang,-10}");
                Console.ResetColor();
            }

            // Xóa dòng thừa nếu trang cuối ít hơn size
            for (int i = view.Count; i < size; i++) Console.WriteLine(new string(' ', 80));

            ConsoleUI.KeVienNgang();
            Console.WriteLine($"Trang {trang + 1}/{maxTrang}. [◄/►] Sang trang. [Esc] Thoát.");

            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.LeftArrow && trang > 0) trang--;
            else if (k == ConsoleKey.RightArrow && trang < maxTrang - 1) trang++;
            else if (k == ConsoleKey.Escape) break;

            // Reset cursor để vẽ lại bảng mượt mà hơn
            Console.SetCursorPosition(0, top);
        }
    }
}