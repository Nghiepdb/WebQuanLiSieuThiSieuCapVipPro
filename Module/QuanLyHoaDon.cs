using System;
using System.Collections.Generic;
using System.Linq;

public static class QuanLyHoaDon
{
    public static void HienThiMenu()
    {
        bool dangChay = true;
        while (dangChay)
        {
            var menu = new List<string>
            {
                "1. Xem toàn bộ lịch sử giao dịch",
                "2. Tìm kiếm Hóa Đơn (Theo Mã/SĐT)",
                "3. Báo cáo doanh thu theo Nhân viên",
                "0. Quay lại"
            };

            int chon = ConsoleUI.HienThiMenuChon("TRA CỨU LỊCH SỬ & ĐƠN HÀNG", menu);
            switch (chon)
            {
                case 0: XemToanBo(); break;
                case 1: TimKiemHoaDon(); break;
                case 2: BaoCaoDoanhThuNhanVien(); break;
                case 3: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    private static void XemToanBo()
    {
        // Sắp xếp mới nhất lên đầu
        var ds = Database.HoaDons.OrderByDescending(x => x.NgayLap).ToList();
        HienThiDanhSachHoaDon(ds, "TOÀN BỘ GIAO DỊCH");
    }

    private static void TimKiemHoaDon()
    {
        string? tuKhoa = ConsoleUI.DocChuoi("Nhập Mã HĐ hoặc SĐT Khách hàng: ");
        if (string.IsNullOrEmpty(tuKhoa)) return;

        var ds = Database.HoaDons
            .Where(x => x.MaHoaDon.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) 
                     || (x.SdtKhachHang != null && x.SdtKhachHang.Contains(tuKhoa)))
            .OrderByDescending(x => x.NgayLap)
            .ToList();

        HienThiDanhSachHoaDon(ds, $"KẾT QUẢ TÌM: {tuKhoa}");
    }

    private static void HienThiDanhSachHoaDon(List<HoaDon> ds, string tieuDe)
    {
        if (ds.Count == 0)
        {
            ConsoleUI.HienThiThongBao("Không tìm thấy dữ liệu.", ConsoleColor.Red);
            return;
        }

        // Phân trang đơn giản
        int trang = 0;
        int size = 5;
        int maxTrang = (int)Math.Ceiling((double)ds.Count / size);

        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe(tieuDe);
            Console.WriteLine($"Tổng số: {ds.Count} hóa đơn.");
            ConsoleUI.KeVienNgang();
            Console.WriteLine($"{"Mã HĐ",-18} | {"Ngày Lập",-16} | {"Người Bán",-10} | {"Khách Hàng",-12} | {"Tổng Tiền",12}");
            ConsoleUI.KeVienNgang();

            var view = ds.Skip(trang * size).Take(size).ToList();
            foreach (var hd in view)
            {
                string khach = string.IsNullOrEmpty(hd.SdtKhachHang) ? "Vãng lai" : hd.SdtKhachHang;
                Console.WriteLine($"{hd.MaHoaDon,-18} | {hd.NgayLap:dd/MM HH:mm} | {hd.NguoiTao,-10} | {khach,-12} | {hd.ThanhTien,12:N0}");
            }
            ConsoleUI.KeVienNgang();
            Console.WriteLine($"Trang {trang + 1}/{maxTrang}. [Enter] Xem chi tiết HĐ đầu tiên. [Trái/Phải] Chuyển trang. [Esc] Thoát.");

            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.LeftArrow && trang > 0) trang--;
            else if (k == ConsoleKey.RightArrow && trang < maxTrang - 1) trang++;
            else if (k == ConsoleKey.Escape) break;
            else if (k == ConsoleKey.Enter)
            {
                // Chọn xem chi tiết cái đầu tiên trong trang view (Demo UI đơn giản)
                // Trong thực tế sẽ làm chọn dòng, nhưng ở đây nhập mã để xem cho nhanh
                string? maChon = ConsoleUI.DocChuoi("Nhập lại Mã HĐ cần xem chi tiết: ");
                var hdChon = ds.FirstOrDefault(x => x.MaHoaDon == maChon);
                if (hdChon != null) XemChiTiet(hdChon);
            }
        }
    }

    private static void XemChiTiet(HoaDon hd)
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("CHI TIẾT HÓA ĐƠN");
        Console.WriteLine($"Mã HĐ    : {hd.MaHoaDon}");
        Console.WriteLine($"Ngày lập : {hd.NgayLap:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"Nhân viên: {hd.NguoiTao}"); // Trả lời câu hỏi: Ai bán?
        Console.WriteLine($"Khách    : {hd.SdtKhachHang ?? "Khách lẻ"}");
        ConsoleUI.KeVienNgang();
        
        Console.WriteLine($"{"Tên Sản Phẩm",-35} | {"SL",-5} | {"Đơn Giá",-12} | {"Thành Tiền",-12}");
        foreach (var item in hd.ChiTiet)
        {
            Console.WriteLine($"{item.TenSP,-35} | {item.SoLuong,-5} | {item.DonGia,12:N0} | {item.ThanhTien,12:N0}");
        }
        ConsoleUI.KeVienNgang();
        Console.WriteLine($"TỔNG TIỀN HÀNG: {hd.TongTienHang:N0}");
        Console.WriteLine($"GIẢM GIÁ      : -{hd.GiamGia:N0}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"THỰC THU      : {hd.ThanhTien:N0} VNĐ");
        Console.ResetColor();

        Console.WriteLine("\n[1] In lại hóa đơn (Giả lập) | [0] Quay lại");
        Console.ReadKey();
    }

    private static void BaoCaoDoanhThuNhanVien()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("DOANH SỐ THEO NHÂN VIÊN");
        
        var thongKe = Database.HoaDons
            .GroupBy(x => x.NguoiTao)
            .Select(g => new { NhanVien = g.Key, DoanhThu = g.Sum(x => x.ThanhTien), DonHang = g.Count() })
            .OrderByDescending(x => x.DoanhThu)
            .ToList();

        Console.WriteLine($"{"Nhân Viên",-20} | {"Số Đơn",-10} | {"Doanh Thu",15}");
        ConsoleUI.KeVienNgang(50);
        foreach (var tk in thongKe)
        {
            Console.WriteLine($"{tk.NhanVien,-20} | {tk.DonHang,-10} | {tk.DoanhThu,15:N0}");
        }
        Console.ReadKey();
    }
}