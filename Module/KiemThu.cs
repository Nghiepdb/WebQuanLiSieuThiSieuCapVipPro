using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class KiemThu
{
    public static void HienThiMenu()
    {
        bool dangChay = true;
        var cacLuaChon = new List<string>
        {
            "1. So sánh hiệu năng (Dictionary vs List)",
            "2. Kiểm thử Combo (Backtracking)",
            "3. Tạo 100 dữ liệu mẫu thực tế (Có tạo Lô hàng)",
            "0. Quay lại"
        };

        while (dangChay)
        {
            int luaChonIndex = ConsoleUI.HienThiMenuChon("KIỂM THỬ & DATA", cacLuaChon);
            switch (luaChonIndex)
            {
                case 0: KiemThuHieuNang_CTDL_SoSanh(); break;
                case 1: KiemThuHieuNang_Combo(); break;
                case 2: TaoVaLuuDuLieuMau(); break;
                case 3: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    private static void TaoVaLuuDuLieuMau()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("TẠO DATA MẪU");
        if (!ConsoleUI.XacNhan("CẢNH BÁO: Dữ liệu cũ (Sản phẩm & Lô hàng) sẽ bị XÓA. Tiếp tục?")) return;

        // Xóa sạch dữ liệu cũ
        QuanLySanPham.dsTheoMa.Clear();
        QuanLySanPham.dsTheoTen.Clear();
        QuanLySanPham.dsTheoDanhMuc.Clear();
        QuanLySanPham_List.XoaTatCa();
        Database.LoHangs.Clear(); // Xóa cả lô hàng để đồng bộ

        var duLieuMau = new List<SanPham>();
        var dataMau = new Dictionary<string, List<string>>()
        {
            { "Đồ gia dụng", new List<string> { "Chảo chống dính", "Nồi cơm điện", "Bát sứ", "Đũa gỗ", "Ly thủy tinh" } },
            { "Bánh kẹo", new List<string> { "Snack Oishi", "Bánh quy Cosy", "Kẹo dẻo", "Chocolate KitKat", "Bánh Gạo" } },
            { "Văn phòng phẩm", new List<string> { "Bút bi", "Vở học sinh", "Giấy A4", "Bút chì màu", "Thước kẻ" } },
            { "Đồ uống", new List<string> { "Trà xanh", "Nước tăng lực", "Sữa đậu nành", "Bia Tiger", "Pepsi", "Coca Cola" } },
            { "Thực phẩm", new List<string> { "Mì gói", "Xúc xích", "Đồ hộp", "Cá hộp", "Cháo gói", "Miến dong" } }
        };

        string[] hauTo = { "Cao cấp", "Nhập khẩu", "Đặc biệt", "Khuyến mãi", "Nội địa", "Loại 1" };
        Random r = new Random();
        var keys = dataMau.Keys.ToList();

        for (int i = 1; i <= 100; i++)
        {
            string cat = keys[r.Next(keys.Count)];
            var names = dataMau[cat];
            string name = names[r.Next(names.Count)];
            string suffix = hauTo[r.Next(hauTo.Length)];
            string finalName = $"{name} {suffix}"; 
            string maSP = $"SP{i:D3}";
            int soLuong = r.Next(10, 200);

            // 1. Tạo Sản Phẩm
            duLieuMau.Add(new SanPham(maSP, finalName, "Cái", r.Next(10, 500) * 1000, soLuong, cat));

            // 2. [FIX] Tạo Lô Hàng tương ứng (Để bán được)
            Database.LoHangs.Add(new LoHang {
                MaLo = $"L{i:D3}_{DateTime.Now.Ticks % 10000}",
                MaSP = maSP,
                SoLuong = soLuong,
                NgayNhap = DateTime.Now.AddDays(-r.Next(1, 30)),
                HanSuDung = DateTime.Now.AddMonths(r.Next(1, 12)) // Hạn sử dụng ngẫu nhiên
            });
        }

        foreach (var sp in duLieuMau) QuanLySanPham.ThemSanPham(sp);
        
        Database.LuuDuLieu(); // Lưu cả Sản phẩm và Lô hàng
        ConsoleUI.HienThiThongBao($"Đã tạo 100 Sản phẩm & 100 Lô hàng tương ứng.", ConsoleUI.MauThanhCong);
    }
    
     private static void KiemThuHieuNang_CTDL_SoSanh()
    {
         Console.Clear();
        ConsoleUI.VeTieuDe("SO SÁNH HIỆU NĂNG (STRESS TEST)");
        Console.WriteLine("(*) Mẹo: Nhập N >= 50.000 để thấy rõ sự khác biệt.");
        int? soLuongTest = ConsoleUI.DocSoNguyen("Nhập số lượng phần tử (N) (VD: 50000): ");
        if (soLuongTest == null || soLuongTest <= 0) return;

        int N = soLuongTest.Value;
        int loopCount = 10000; 

        Console.WriteLine($"\n[1] Đang khởi tạo {N:N0} dữ liệu mẫu vào RAM...");

        List<SanPham> duLieuTest = new List<SanPham>();
        for (int i = 0; i < N; i++) 
        {
            duLieuTest.Add(new SanPham($"TEST{i}", $"Sản phẩm Test {i}", "Cái", 100, 10, "Test"));
        }
        
        string maGiua = $"TEST{N / 2}";
        string maKhongTonTai = "KHONG_CO_DAU_TIM_CHI_MET";
        
        QuanLySanPham.dsTheoMa.Clear();
        QuanLySanPham.dsTheoTen.Clear();
        QuanLySanPham.dsTheoDanhMuc.Clear();
        QuanLySanPham_List.XoaTatCa();

        Stopwatch sw = new Stopwatch();
        var ketQua = new List<Tuple<string, string, string>>();

        Console.WriteLine("\n[2] Benchmark: THÊM DỮ LIỆU (INSERT)...");
        sw.Restart(); 
        foreach (var sp in duLieuTest) QuanLySanPham.ThemSanPham(sp); 
        sw.Stop();
        string tDict_Them = $"{sw.ElapsedMilliseconds} ms";

        sw.Restart(); 
        foreach (var sp in duLieuTest) QuanLySanPham_List.ThemSanPham(sp); 
        sw.Stop();
        string tList_Them = $"{sw.ElapsedMilliseconds} ms";
        ketQua.Add(Tuple.Create($"Thêm {N:N0} SP", tDict_Them, tList_Them));

        Console.WriteLine($"\n[3] Benchmark: TÌM THEO MÃ ({loopCount:N0} lần liên tục)...");
        sw.Restart();
        for(int i=0; i<loopCount; i++) QuanLySanPham.TimTheoMa(maGiua, out _);
        sw.Stop();
        string tDict_Tim = $"{sw.ElapsedMilliseconds} ms"; 

        sw.Restart();
        for (int i = 0; i < loopCount; i++) QuanLySanPham_List.TimTheoMa(maGiua, out _);
        sw.Stop();
        string tList_Tim = $"{sw.ElapsedMilliseconds} ms";
        ketQua.Add(Tuple.Create($"Tìm Mã (Giữa)", tDict_Tim, tList_Tim));

        Console.WriteLine($"\n[4] Benchmark: TÌM MÃ KHÔNG TỒN TẠI ({loopCount:N0} lần)...");
        sw.Restart();
        for(int i=0; i<loopCount; i++) QuanLySanPham.TimTheoMa(maKhongTonTai, out _);
        sw.Stop();
        string tDict_NotFind = $"{sw.ElapsedMilliseconds} ms";

        sw.Restart();
        for (int i = 0; i < loopCount; i++) QuanLySanPham_List.TimTheoMa(maKhongTonTai, out _);
        sw.Stop();
        string tList_NotFind = $"{sw.ElapsedMilliseconds} ms";
        ketQua.Add(Tuple.Create("Tìm Không Có", tDict_NotFind, tList_NotFind));

        int soLuongXoa = Math.Min(N, 5000); 
        Console.WriteLine($"\n[5] Benchmark: XÓA {soLuongXoa:N0} SẢN PHẨM...");

        sw.Restart();
        for(int i=0; i<soLuongXoa; i++) QuanLySanPham.XoaSanPham($"TEST{i}");
        sw.Stop();
        string tDict_Xoa = $"{sw.ElapsedMilliseconds} ms";

        sw.Restart();
        for (int i = 0; i < soLuongXoa; i++) QuanLySanPham_List.XoaSanPham($"TEST{i}");
        sw.Stop();
        string tList_Xoa = $"{sw.ElapsedMilliseconds} ms";
        ketQua.Add(Tuple.Create($"Xóa {soLuongXoa:N0} SP", tDict_Xoa, tList_Xoa));
        
        Console.Clear();
        ConsoleUI.VeTieuDe($"KẾT QUẢ (N={N:N0}, Loop={loopCount:N0})");
        Console.WriteLine($"| {"PHÉP TOÁN",-20} | {"DICTIONARY (Hash)",-20} | {"LIST (Linear)",-20} | {"NHẬN XÉT", -20}");
        ConsoleUI.KeVienNgang(85);
        
        foreach (var row in ketQua) 
        {
            Console.Write($"| {row.Item1,-20} | ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{row.Item2,-20}");
            Console.ResetColor();
            Console.Write(" | ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{row.Item3,-20}");
            Console.ResetColor();
            Console.WriteLine("|");
        }
        ConsoleUI.KeVienNgang(85);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n[KẾT LUẬN]");
        Console.WriteLine("1. Dictionary tìm kiếm SIÊU NHANH (gần như tức thì).");
        Console.WriteLine("2. List càng nhiều dữ liệu càng chậm.");
        Console.ResetColor();
        ConsoleUI.HienThiThongBao("Kiểm thử hoàn tất.", ConsoleColor.Green);
    }

     private static void KiemThuHieuNang_Combo()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("KIỂM THỬ THUẬT TOÁN COMBO");
        int? n = ConsoleUI.DocSoNguyen("Nhập n (VD: 20): ");
        if (n == null) return;
        int? m = ConsoleUI.DocSoNguyen("Nhập m: ");
        if (m == null) return;

        List<SanPham> ds = new List<SanPham>();
        for (int i = 0; i < n.Value; i++) ds.Add(new SanPham($"T{i}", $"SP {i}", "Cái", 1, 1));

        Console.WriteLine("Đang tính toán...");
        Stopwatch sw = Stopwatch.StartNew();
        var kq = ThuVienCombo.LietKeCombos(ds, m.Value);
        sw.Stop();
        Console.WriteLine($"Kết quả: {kq.Count:N0} combos. Thời gian: {sw.ElapsedMilliseconds} ms");
        ConsoleUI.HienThiThongBao("Xong.", ConsoleColor.Green);
    }
}