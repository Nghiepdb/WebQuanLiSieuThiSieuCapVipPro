using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;

public static class ConsoleUI
{
    // --- THEME MÀU SẮC (CHUẨN HÓA) ---
    public const ConsoleColor MauNenChung = ConsoleColor.Black;
    public const ConsoleColor MauVien = ConsoleColor.Cyan;       // Khung viền
    public const ConsoleColor MauTieuDe = ConsoleColor.Yellow;   // Tiêu đề lớn
    public const ConsoleColor MauChinh = ConsoleColor.White;     // Chữ thường
    public const ConsoleColor MauPhu = ConsoleColor.Gray;        // Chữ phụ / Hint
    public const ConsoleColor MauHighlight = ConsoleColor.Green; // Đang chọn
    public const ConsoleColor MauNenHighlight = ConsoleColor.DarkGray;
    public const ConsoleColor MauLoi = ConsoleColor.Red;
    public const ConsoleColor MauThanhCong = ConsoleColor.Green;

    // --- HÀM VẼ UI ---
    public static void VeTieuDe(string tieuDe)
    {
        Console.Clear();
        int width = Math.Min(Console.WindowWidth - 2, 90); 
        string vienNgang = new string('═', width);
        
        DatMau(MauVien);
        Console.WriteLine($"╔{vienNgang}╗");
        Console.Write("║");
        
        // Căn giữa tiêu đề
        string titleUpper = tieuDe.ToUpper();
        int paddingLeft = (width - titleUpper.Length) / 2;
        int paddingRight = width - titleUpper.Length - paddingLeft;
        
        DatMau(MauTieuDe);
        Console.Write(new string(' ', paddingLeft) + titleUpper + new string(' ', paddingRight));
        
        DatMau(MauVien);
        Console.WriteLine("║");
        Console.WriteLine($"╚{vienNgang}╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void KeVienNgang(int doDai = 90)
    {
        DatMau(ConsoleColor.DarkGray);
        Console.WriteLine(new string('─', doDai));
        Console.ResetColor();
    }

    // --- TÌM KIẾM REAL-TIME SẢN PHẨM ---
    public static SanPham? ChonMotSanPhamVoiBoLocRealTime(List<SanPham> khoHang, string tieuDe = "TÌM KIẾM SẢN PHẨM")
    {
        string tuKhoa = "";
        int indexHienTai = 0;
        bool canVeLai = true;
        int soDongHienThi = 10;
        
        Console.CursorVisible = true;

        while (true)
        {
            var danhSachLoc = khoHang.Where(sp => 
                string.IsNullOrEmpty(tuKhoa) ||
                (sp.MaSP?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                (sp.TenSP?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                sp.GiaBan.ToString().Contains(tuKhoa) ||
                (sp.DanhMuc?.ToLower().Contains(tuKhoa.ToLower()) ?? false)
            ).ToList();

            if (indexHienTai >= danhSachLoc.Count) indexHienTai = Math.Max(0, danhSachLoc.Count - 1);

            if (canVeLai)
            {
                VeTieuDe(tieuDe);
                DatMau(MauPhu);
                Console.WriteLine("Gõ để lọc. [▲/▼] Di chuyển. [Space] XEM NHANH. [Enter] CHỌN. [Esc] Hủy.\n");

                Console.Write("🔍 Tìm kiếm: ");
                DatMau(ConsoleColor.White, ConsoleColor.DarkBlue);
                Console.Write(tuKhoa.PadRight(30)); 
                Console.ResetColor();
                Console.WriteLine($" (Kết quả: {danhSachLoc.Count})");
                
                DatMau(ConsoleColor.DarkCyan);
                Console.WriteLine($"{" Mã",-8} | {" Tên Sản Phẩm",-25} | {" Danh Mục",-20} | {" Giá (VNĐ)",15} | {" Tồn",5}");
                Console.ResetColor();
                KeVienNgang(90);

                int trangStart = Math.Max(0, indexHienTai - soDongHienThi / 2);
                if (trangStart + soDongHienThi > danhSachLoc.Count) 
                    trangStart = Math.Max(0, danhSachLoc.Count - soDongHienThi);

                int trangEnd = Math.Min(trangStart + soDongHienThi, danhSachLoc.Count);

                if (danhSachLoc.Count == 0)
                {
                    DatMau(MauLoi);
                    Console.WriteLine("   (Không tìm thấy sản phẩm nào phù hợp)");
                    Console.ResetColor();
                }
                else
                {
                    for (int i = trangStart; i < trangEnd; i++)
                    {
                        var sp = danhSachLoc[i];
                        bool isHighlight = (i == indexHienTai);

                        if (isHighlight) DatMau(MauHighlight, MauNenHighlight);
                        else DatMau(MauPhu);

                        string ten = (sp.TenSP ?? "").Length > 23 ? sp.TenSP.Substring(0, 20) + "..." : sp.TenSP;
                        string dm = (sp.DanhMuc ?? "").Length > 18 ? sp.DanhMuc.Substring(0, 15) + "..." : sp.DanhMuc;

                        string line = $" {sp.MaSP,-8} | {ten,-25} | {dm,-20} | {sp.GiaBan,15:N0} | {sp.SoLuongTonKho,5}";
                        Console.WriteLine(line.PadRight(90)); 
                        Console.ResetColor();
                    }
                }
                KeVienNgang(90);
                canVeLai = false;
            }

            Console.SetCursorPosition(13 + tuKhoa.Length, 6);

            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (danhSachLoc.Count > 0) {
                    Console.CursorVisible = false;
                    return danhSachLoc[indexHienTai];
                }
                else Console.Beep();
            }
            else if (keyInfo.Key == ConsoleKey.Escape) { Console.CursorVisible = false; return null; }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (danhSachLoc.Count > 0) { indexHienTai = Math.Min(indexHienTai + 1, danhSachLoc.Count - 1); canVeLai = true; }
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (danhSachLoc.Count > 0) { indexHienTai = Math.Max(0, indexHienTai - 1); canVeLai = true; }
            }
            else if (keyInfo.Key == ConsoleKey.PageDown)
            {
                if (danhSachLoc.Count > 0) {
                    indexHienTai = Math.Min(indexHienTai + soDongHienThi, danhSachLoc.Count - 1);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.PageUp)
            {
                if (danhSachLoc.Count > 0) {
                    indexHienTai = Math.Max(0, indexHienTai - soDongHienThi);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                if (danhSachLoc.Count > 0)
                {
                    var spReview = danhSachLoc[indexHienTai];
                    int oldLeft = Console.CursorLeft;
                    int oldTop = Console.CursorTop;

                    int popupTop = Math.Min(Console.WindowHeight - 8, 18);
                    Console.SetCursorPosition(5, popupTop);
                    DatMau(ConsoleColor.White, ConsoleColor.DarkBlue);
                    Console.WriteLine("╔════════════════════════ XEM NHANH (PREVIEW) ══════════════════════╗");
                    Console.SetCursorPosition(5, popupTop + 1);
                    Console.WriteLine($"║ Tên đầy đủ: {spReview.TenSP.PadRight(54)}║");
                    Console.SetCursorPosition(5, popupTop + 2);
                    Console.WriteLine($"║ Danh mục  : {spReview.DanhMuc.PadRight(54)}║");
                    Console.SetCursorPosition(5, popupTop + 3);
                    Console.WriteLine($"║ Giá bán   : {spReview.GiaBan.ToString("N0").PadRight(43)} VNĐ        ║");
                    Console.SetCursorPosition(5, popupTop + 4);
                    Console.WriteLine("╚═══════════════════════════════════════════════════════════════════╝");
                    Console.ResetColor();
                    
                    Console.ReadKey(true);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (tuKhoa.Length > 0) { tuKhoa = tuKhoa.Substring(0, tuKhoa.Length - 1); indexHienTai = 0; canVeLai = true; }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                tuKhoa += keyInfo.KeyChar; indexHienTai = 0; canVeLai = true;
            }
        }
    }

    // --- [MỚI] TÌM KIẾM REAL-TIME NGƯỜI DÙNG ---
    public static NguoiDung? ChonMotNguoiDungVoiBoLocRealTime(List<NguoiDung> dsUser, string tieuDe = "TÌM KIẾM NGƯỜI DÙNG")
    {
        string tuKhoa = "";
        int indexHienTai = 0;
        bool canVeLai = true;
        int soDongHienThi = 10;
        
        Console.CursorVisible = true;

        while (true)
        {
            var danhSachLoc = dsUser.Where(u => 
                string.IsNullOrEmpty(tuKhoa) ||
                (u.Username?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                (u.HoTen?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                (u.Role.ToString().ToLower().Contains(tuKhoa.ToLower()))
            ).ToList();

            if (indexHienTai >= danhSachLoc.Count) indexHienTai = Math.Max(0, danhSachLoc.Count - 1);

            if (canVeLai)
            {
                VeTieuDe(tieuDe);
                DatMau(MauPhu);
                Console.WriteLine("Gõ tên/user để lọc. [▲/▼] Di chuyển. [Enter] CHỌN. [Esc] Hủy.\n");

                Console.Write("🔍 Tìm nhân viên: ");
                DatMau(ConsoleColor.White, ConsoleColor.DarkBlue);
                Console.Write(tuKhoa.PadRight(30)); 
                Console.ResetColor();
                Console.WriteLine($" (Kết quả: {danhSachLoc.Count})");
                
                DatMau(ConsoleColor.DarkCyan);
                Console.WriteLine($"{" Username",-15} | {" Họ và Tên",-30} | {" Vai Trò",-15}");
                Console.ResetColor();
                KeVienNgang(70);

                int trangStart = Math.Max(0, indexHienTai - soDongHienThi / 2);
                if (trangStart + soDongHienThi > danhSachLoc.Count) 
                    trangStart = Math.Max(0, danhSachLoc.Count - soDongHienThi);

                int trangEnd = Math.Min(trangStart + soDongHienThi, danhSachLoc.Count);

                if (danhSachLoc.Count == 0)
                {
                    DatMau(MauLoi);
                    Console.WriteLine("   (Không tìm thấy nhân viên nào phù hợp)");
                    Console.ResetColor();
                }
                else
                {
                    for (int i = trangStart; i < trangEnd; i++)
                    {
                        var u = danhSachLoc[i];
                        bool isHighlight = (i == indexHienTai);

                        if (isHighlight) DatMau(MauHighlight, MauNenHighlight);
                        else DatMau(MauPhu);

                        string ten = (u.HoTen ?? "").Length > 28 ? u.HoTen.Substring(0, 25) + "..." : u.HoTen;
                        string line = $" {u.Username,-15} | {ten,-30} | {u.Role,-15}";
                        Console.WriteLine(line.PadRight(70)); 
                        Console.ResetColor();
                    }
                }
                KeVienNgang(70);
                canVeLai = false;
            }

            Console.SetCursorPosition(18 + tuKhoa.Length, 6);

            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (danhSachLoc.Count > 0) {
                    Console.CursorVisible = false;
                    return danhSachLoc[indexHienTai];
                }
                else Console.Beep();
            }
            else if (keyInfo.Key == ConsoleKey.Escape) { Console.CursorVisible = false; return null; }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (danhSachLoc.Count > 0) { indexHienTai = Math.Min(indexHienTai + 1, danhSachLoc.Count - 1); canVeLai = true; }
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (danhSachLoc.Count > 0) { indexHienTai = Math.Max(0, indexHienTai - 1); canVeLai = true; }
            }
            else if (keyInfo.Key == ConsoleKey.PageDown)
            {
                if (danhSachLoc.Count > 0) {
                    indexHienTai = Math.Min(indexHienTai + soDongHienThi, danhSachLoc.Count - 1);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.PageUp)
            {
                if (danhSachLoc.Count > 0) {
                    indexHienTai = Math.Max(0, indexHienTai - soDongHienThi);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (tuKhoa.Length > 0) { tuKhoa = tuKhoa.Substring(0, tuKhoa.Length - 1); indexHienTai = 0; canVeLai = true; }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                tuKhoa += keyInfo.KeyChar; indexHienTai = 0; canVeLai = true;
            }
        }
    }

    public static List<SanPham>? ChonSanPhamVoiBoLocRealTime(List<SanPham> khoHang)
    {
        string tuKhoa = "";
        HashSet<string> maSpDaChon = new HashSet<string>();
        int indexHienTai = 0;
        bool canVeLai = true;
        int soDongHienThi = 10;
        Console.CursorVisible = true;

        while (true)
        {
            var danhSachLoc = khoHang.Where(sp => 
                string.IsNullOrEmpty(tuKhoa) ||
                (sp.MaSP?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                (sp.TenSP?.ToLower().Contains(tuKhoa.ToLower()) ?? false) ||
                sp.GiaBan.ToString().Contains(tuKhoa) ||
                (sp.DanhMuc?.ToLower().Contains(tuKhoa.ToLower()) ?? false)
            ).ToList();

            if (indexHienTai >= danhSachLoc.Count) indexHienTai = Math.Max(0, danhSachLoc.Count - 1);

            if (canVeLai)
            {
                VeTieuDe("CHỌN NHIỀU SẢN PHẨM");
                DatMau(MauPhu);
                Console.WriteLine("Gõ để lọc. [Enter] Chọn/Bỏ. [Tab] HOÀN TẤT. [Esc] Hủy.\n");

                Console.Write("🔍 Tìm: ");
                DatMau(ConsoleColor.White, ConsoleColor.DarkBlue);
                Console.Write(tuKhoa.PadRight(30));
                Console.ResetColor();
                DatMau(ConsoleColor.Yellow);
                Console.WriteLine($" | GIỎ: {maSpDaChon.Count} món");
                Console.ResetColor();

                DatMau(ConsoleColor.DarkCyan);
                Console.WriteLine($"{"   Mã",-11} | {" Tên Sản Phẩm",-25} | {" Danh Mục",-20} | {" Giá",15}");
                Console.ResetColor();
                KeVienNgang(90);

                int trangStart = Math.Max(0, indexHienTai - soDongHienThi / 2);
                 if (trangStart + soDongHienThi > danhSachLoc.Count) 
                    trangStart = Math.Max(0, danhSachLoc.Count - soDongHienThi);
                int trangEnd = Math.Min(trangStart + soDongHienThi, danhSachLoc.Count);

                if (danhSachLoc.Count == 0) Console.WriteLine("   (Trống)");
                else
                {
                    for (int i = trangStart; i < trangEnd; i++)
                    {
                        var sp = danhSachLoc[i];
                        bool isSelected = maSpDaChon.Contains(sp.MaSP);
                        bool isHighlight = (i == indexHienTai);

                        if (isHighlight) DatMau(MauHighlight, MauNenHighlight);
                        else DatMau(MauPhu);

                        string checkMark = isSelected ? "[x]" : "[ ]";
                        string ten = (sp.TenSP ?? "").Length > 23 ? sp.TenSP.Substring(0, 20) + ".." : sp.TenSP;
                        string dm = (sp.DanhMuc ?? "").Length > 18 ? sp.DanhMuc.Substring(0, 15) + ".." : sp.DanhMuc;

                        string line = $"{checkMark} {sp.MaSP,-8} | {ten,-25} | {dm,-20} | {sp.GiaBan,15:N0}";
                        Console.WriteLine(line.PadRight(90));
                        Console.ResetColor();
                    }
                }
                KeVienNgang(90);
                canVeLai = false;
            }

            Console.SetCursorPosition(8 + tuKhoa.Length, 6);
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Tab)
            {
                if (maSpDaChon.Count == 0) { HienThiThongBao("Chưa chọn món nào!", MauLoi); canVeLai = true; continue; }
                Console.CursorVisible = false;
                return khoHang.Where(sp => maSpDaChon.Contains(sp.MaSP)).ToList();
            }
            else if (keyInfo.Key == ConsoleKey.Escape) { Console.CursorVisible = false; return null; }
            else if (keyInfo.Key == ConsoleKey.DownArrow) { if (danhSachLoc.Count > 0) { indexHienTai = Math.Min(indexHienTai + 1, danhSachLoc.Count - 1); canVeLai = true; } }
            else if (keyInfo.Key == ConsoleKey.UpArrow) { if (danhSachLoc.Count > 0) { indexHienTai = Math.Max(0, indexHienTai - 1); canVeLai = true; } }
            else if (keyInfo.Key == ConsoleKey.PageDown) { if (danhSachLoc.Count > 0) { indexHienTai = Math.Min(indexHienTai + soDongHienThi, danhSachLoc.Count - 1); canVeLai = true; } }
            else if (keyInfo.Key == ConsoleKey.PageUp) { if (danhSachLoc.Count > 0) { indexHienTai = Math.Max(0, indexHienTai - soDongHienThi); canVeLai = true; } }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                if (danhSachLoc.Count > 0)
                {
                    var sp = danhSachLoc[indexHienTai];
                    if (maSpDaChon.Contains(sp.MaSP)) maSpDaChon.Remove(sp.MaSP); else maSpDaChon.Add(sp.MaSP);
                    canVeLai = true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace) { if (tuKhoa.Length > 0) { tuKhoa = tuKhoa.Substring(0, tuKhoa.Length - 1); indexHienTai = 0; canVeLai = true; } }
            else if (!char.IsControl(keyInfo.KeyChar)) { tuKhoa += keyInfo.KeyChar; indexHienTai = 0; canVeLai = true; }
        }
    }

    public static int HienThiMenuChon(string tieuDe, List<string> cacLuaChon)
    {
        int mucChonHienTai = 0;
        ConsoleKeyInfo key;
        Console.CursorVisible = false;
        while (true)
        {
            VeTieuDe(tieuDe);
            for (int i = 0; i < cacLuaChon.Count; i++)
            {
                if (i == mucChonHienTai)
                {
                    DatMau(MauHighlight, MauNenHighlight);
                    Console.WriteLine("  ► " + cacLuaChon[i].PadRight(Math.Min(Console.WindowWidth - 6, 85)));
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("    " + cacLuaChon[i]);
                }
            }
            Console.WriteLine();
            KeVienNgang(Math.Min(Console.WindowWidth - 2, 90));
            DatMau(MauPhu);
            Console.WriteLine(" [▲/▼] Di chuyển | [Enter] Chọn | [Esc] Thoát");
            Console.ResetColor();

            key = Console.ReadKey(true);
            if (char.IsDigit(key.KeyChar))
            {
                int idx = cacLuaChon.FindIndex(s => s.Trim().StartsWith(key.KeyChar.ToString() + "."));
                if (idx != -1) { Console.CursorVisible = true; return idx; }
            }
            switch (key.Key)
            {
                case ConsoleKey.UpArrow: mucChonHienTai = (mucChonHienTai > 0) ? mucChonHienTai - 1 : cacLuaChon.Count - 1; break;
                case ConsoleKey.DownArrow: mucChonHienTai = (mucChonHienTai < cacLuaChon.Count - 1) ? mucChonHienTai + 1 : 0; break;
                case ConsoleKey.Enter: Console.CursorVisible = true; return mucChonHienTai;
                case ConsoleKey.Escape: Console.CursorVisible = true; return -1;
            }
        }
    }

    public static bool XacNhan(string cauHoi)
    {
        Console.WriteLine();
        DatMau(ConsoleColor.Yellow);
        Console.Write($"❓ {cauHoi} (C/K): ");
        Console.ResetColor();
        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.C || k == ConsoleKey.Y) { Console.WriteLine(" Có"); return true; }
            if (k == ConsoleKey.K || k == ConsoleKey.N || k == ConsoleKey.Escape) { Console.WriteLine(" Không"); return false; }
        }
    }

    public static void HienThiThongBao(string thongBao, ConsoleColor mauNen)
    {
        Console.WriteLine();
        ConsoleColor mauChu = (mauNen == ConsoleColor.Yellow || mauNen == ConsoleColor.Green) ? ConsoleColor.Black : ConsoleColor.White;
        string icon = mauNen == ConsoleColor.Red ? "❌" : (mauNen == ConsoleColor.Green ? "✅" : "⚠️");
        DatMau(mauChu, mauNen);
        Console.WriteLine($" {icon} {thongBao} ");
        Console.ResetColor();
        Console.WriteLine(" (Nhấn phím bất kỳ...)");
        Console.ReadKey(true);
    }

    public static string? DocChuoi(string loiNhac, string giaTriMacDinh = "", bool batBuoc = false)
    {
        Console.CursorVisible = true;
        Console.Write(loiNhac);
        var buffer = new StringBuilder();
        
        if (!string.IsNullOrEmpty(giaTriMacDinh))
        {
            buffer.Append(giaTriMacDinh);
            Console.Write(giaTriMacDinh);
        }

        int top = Console.CursorTop;

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                if (buffer.Length == 0 && batBuoc) { 
                    HienThiThongBao("Bắt buộc!", MauLoi); 
                    Console.SetCursorPosition(0, top); Console.Write(new string(' ', Console.WindowWidth)); 
                    Console.SetCursorPosition(0, top); Console.Write(loiNhac); 
                    buffer.Clear(); 
                }
                else { Console.WriteLine(); return buffer.ToString(); }
            }
            else if (key.Key == ConsoleKey.Escape) { Console.WriteLine(); return null; }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    Console.Write("\b \b"); 
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
    }

    public static string? DocChuoiCoGoiY(string loiNhac, List<string> danhSachGoiY)
    {
        Console.CursorVisible = true;
        Console.Write(loiNhac);
        var buffer = new StringBuilder();
        int originLeft = Console.CursorLeft;
        int originTop = Console.CursorTop;

        if (danhSachGoiY.Count > 0)
        {
            Console.WriteLine();
            DatMau(ConsoleColor.DarkGray);
            Console.WriteLine("(Gợi ý: " + string.Join(", ", danhSachGoiY.Take(5)) + "...)");
            Console.ResetColor();
            Console.SetCursorPosition(originLeft, originTop);
        }

        while (true)
        {
            string currentInput = buffer.ToString();
            string suggestion = "";
            if (!string.IsNullOrEmpty(currentInput))
            {
                suggestion = danhSachGoiY.FirstOrDefault(s => s.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase)) ?? "";
            }

            Console.SetCursorPosition(originLeft, originTop);
            Console.Write(new string(' ', 50)); 
            Console.SetCursorPosition(originLeft, originTop);
            Console.Write(currentInput);
            
            if (!string.IsNullOrEmpty(suggestion) && suggestion.Length > currentInput.Length)
            {
                DatMau(ConsoleColor.DarkGray);
                Console.Write(suggestion.Substring(currentInput.Length));
                Console.ResetColor();
            }

            Console.SetCursorPosition(originLeft + currentInput.Length, originTop);

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return buffer.ToString();
            }
            else if (key.Key == ConsoleKey.Tab)
            {
                if (!string.IsNullOrEmpty(suggestion))
                {
                    buffer.Clear();
                    buffer.Append(suggestion);
                }
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return null;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) buffer.Remove(buffer.Length - 1, 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }
    }

    public static string? DocMatKhau(string loiNhac)
    {
        Console.CursorVisible = true;
        Console.Write(loiNhac);
        var pass = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return pass.ToString();
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                Console.WriteLine();
                return null;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (pass.Length > 0)
                {
                    pass.Remove(pass.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pass.Append(key.KeyChar);
                Console.Write("*");
            }
        }
    }

    public static int? DocSoNguyen(string loiNhac, int? giaTriMacDinh = null)
    {
        Console.Write(loiNhac);
        StringBuilder buffer = new StringBuilder();
        if (giaTriMacDinh.HasValue) { buffer.Append(giaTriMacDinh.Value); Console.Write(giaTriMacDinh.Value); }

        while (true)
        {
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter)
            {
                if (buffer.Length == 0) return null;
                Console.WriteLine();
                if(int.TryParse(buffer.ToString(), out int val)) return val;
                return null;
            }
            else if (k.Key == ConsoleKey.Escape) { Console.WriteLine(); return null; }
            else if (k.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) { buffer.Length--; Console.Write("\b \b"); }
            }
            else if (char.IsDigit(k.KeyChar)) 
            {
                buffer.Append(k.KeyChar);
                Console.Write(k.KeyChar);
            }
            else { Console.Beep(); } 
        }
    }

    public static decimal? DocSoThapPhan(string loiNhac, decimal? giaTriMacDinh = null)
    {
        Console.Write(loiNhac);
        StringBuilder buffer = new StringBuilder();
        if (giaTriMacDinh.HasValue) { buffer.Append(giaTriMacDinh.Value.ToString("G0")); Console.Write(giaTriMacDinh.Value.ToString("G0")); }
        
        bool daCoDauPhay = buffer.ToString().Contains(',') || buffer.ToString().Contains('.');

        while (true)
        {
            var k = Console.ReadKey(true);
            char c = k.KeyChar;
            if (k.Key == ConsoleKey.Enter)
            {
                if (buffer.Length == 0) return null;
                Console.WriteLine();
                string s = buffer.ToString().Replace('.', ',');
                if (decimal.TryParse(s, new CultureInfo("vi-VN"), out decimal val)) return val;
                return 0;
            }
            else if (k.Key == ConsoleKey.Escape) { Console.WriteLine(); return null; }
            else if (k.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) 
                {
                    char deleted = buffer[buffer.Length-1];
                    if (deleted == ',' || deleted == '.') daCoDauPhay = false;
                    buffer.Length--; 
                    Console.Write("\b \b"); 
                }
            }
            else if (char.IsDigit(c))
            {
                buffer.Append(c); Console.Write(c);
            }
            else if ((c == '.' || c == ',') && !daCoDauPhay && buffer.Length > 0)
            {
                daCoDauPhay = true;
                buffer.Append(c); Console.Write(c);
            }
            else { Console.Beep(); }
        }
    }

    public static void DatMau(ConsoleColor f, ConsoleColor b = ConsoleColor.Black) { Console.ForegroundColor = f; Console.BackgroundColor = b; }
}