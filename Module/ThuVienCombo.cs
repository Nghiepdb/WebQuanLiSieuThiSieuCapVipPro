using System;
using System.Collections.Generic;
using System.Linq;

public static class ThuVienCombo
{
    struct LeHoi
    {
        public string TenLeHoi;
        public string MoTa;
        public LeHoi(string ten, string moTa) { TenLeHoi = ten; MoTa = moTa; }
    }

    private static readonly List<LeHoi> DsLeHoi = new List<LeHoi>
    {
        new LeHoi("Tết Nguyên Đán", "Combo quà Tết Sum Vầy"),
        new LeHoi("Lễ Tình Nhân", "Combo Cặp đôi"),
        new LeHoi("Quốc Tế Phụ Nữ", "Quà tặng Phái Đẹp"),
        new LeHoi("Giáng Sinh", "Combo Đêm Đông"),
        new LeHoi("Quốc Khánh", "Mừng Đại Lễ"),
        new LeHoi("Black Friday", "Săn sale giá hời")
    };

    // --- MENU CHÍNH CỦA MODULE COMBO ---
    public static void HienThiMenu()
    {
        bool dangChay = true;
        while (dangChay)
        {
            var luaChon = new List<string> 
            { 
                "1. Xem mẫu Combo ngày lễ có sẵn (Tự động gợi ý)", 
                "2. Tạo Combo thủ công (Tự chọn sản phẩm)",
                "3. Tạo Combo Giải Cứu (Hàng cận date - FEFO)", // [MỚI]
                "0. Quay lại" 
            };

            int chon = ConsoleUI.HienThiMenuChon("TẠO COMBO QUÀ TẶNG", luaChon);

            switch (chon)
            {
                case 0: HienThiMenuNgayLe(); break;
                case 1: ChayCheDoThuCong(); break;
                case 2: XuLyComboHangCanDate(); break; // [MỚI]
                case 3: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    // --- CHỨC NĂNG MỚI: COMBO GIẢI CỨU HÀNG CẬN DATE ---
    private static void XuLyComboHangCanDate()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("COMBO GIẢI CỨU (HÀNG CẬN DATE)");

        // Cho phép nhập số ngày cảnh báo, mặc định là 30
        int songay = ConsoleUI.DocSoNguyen("Nhập giới hạn số ngày hết hạn (Mặc định 30 ngày): ") ?? 30;
        DateTime homNay = DateTime.Now;
        DateTime nguongCanhBao = homNay.AddDays(songay);

        Console.WriteLine($"\nĐang quét kho tìm hàng hết hạn trước: {nguongCanhBao:dd/MM/yyyy}...");

        // 1. Lọc các Mã SP từ Lô Hàng có HSD <= ngưỡng và còn Tồn > 0
        // GroupBy để loại bỏ mã trùng lặp nếu 1 sản phẩm có nhiều lô cận date
        var dsMaSPCanDate = Database.LoHangs
            .Where(l => l.SoLuong > 0 && l.HanSuDung <= nguongCanhBao && l.HanSuDung >= homNay)
            .Select(l => l.MaSP)
            .Distinct()
            .ToList();

        if (dsMaSPCanDate.Count == 0)
        {
            ConsoleUI.HienThiThongBao($"Tuyệt vời! Không có lô hàng nào hết hạn trong {songay} ngày tới.", ConsoleColor.Green);
            return;
        }

        // 2. Lấy thông tin chi tiết Sản Phẩm từ Dictionary
        var dsNguyenLieu = new List<SanPham>();
        foreach (var ma in dsMaSPCanDate)
        {
            if (QuanLySanPham.dsTheoMa.TryGetValue(ma, out SanPham sp) && !sp.IsDeleted)
            {
                dsNguyenLieu.Add(sp);
            }
        }

        if (dsNguyenLieu.Count < 2)
        {
            ConsoleUI.HienThiThongBao("Số lượng hàng cận date quá ít để tạo Combo.", ConsoleColor.Yellow);
            return;
        }

        Console.WriteLine($"Tìm thấy {dsNguyenLieu.Count} mặt hàng cần đẩy gấp!");
        
        // 3. Chạy thuật toán gợi ý combo
        ChayThuatToanVaHienThi(dsNguyenLieu, $"COMBO GIẢI CỨU (DATE < {songay} NGÀY)");
    }

    // --- CHẾ ĐỘ 1: MẪU CÓ SẴN (PRESET) ---
    private static void HienThiMenuNgayLe()
    {
        while (true)
        {
            var menuItems = new List<string>();
            for (int i = 0; i < DsLeHoi.Count; i++)
            {
                menuItems.Add($"{i + 1}. {DsLeHoi[i].TenLeHoi} - {DsLeHoi[i].MoTa}");
            }
            menuItems.Add("0. Quay lại");

            int chon = ConsoleUI.HienThiMenuChon("CHỌN DỊP LỄ", menuItems);
            if (chon == -1 || chon == menuItems.Count - 1) break;

            LeHoi leHoiChon = DsLeHoi[chon];
            XuLyTuDongChoLeHoi(leHoiChon);
        }
    }

    private static void XuLyTuDongChoLeHoi(LeHoi leHoi)
    {
        Console.Clear();
        ConsoleUI.VeTieuDe($"GỢI Ý: {leHoi.TenLeHoi}");

        Random rng = new Random();
        var dsNguyenLieu = QuanLySanPham.dsTheoMa.Values
            .Where(x => !x.IsDeleted)
            .OrderBy(x => rng.Next()) // Random danh sách
            .Take(20)                 // Lấy 20 món bất kỳ
            .ToList();
        
        if (dsNguyenLieu.Count < 2)
        {
            ConsoleUI.HienThiThongBao("Kho hàng không đủ sản phẩm.", ConsoleColor.Red);
            return;
        }

        ChayThuatToanVaHienThi(dsNguyenLieu, $"Gợi ý cho {leHoi.TenLeHoi}");
    }

    // --- CHẾ ĐỘ 2: THỦ CÔNG (MANUAL) ---
    private static void ChayCheDoThuCong()
    {
        Console.Clear();
        var toanBoKho = QuanLySanPham.dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
        
        if (toanBoKho.Count == 0)
        {
            ConsoleUI.HienThiThongBao("Kho hàng đang trống.", ConsoleColor.Red);
            return;
        }

        // 1. Chọn nguyên liệu
        var dsDaChon = ConsoleUI.ChonSanPhamVoiBoLocRealTime(toanBoKho);

        if (dsDaChon == null || dsDaChon.Count == 0) return;

        Console.Clear(); 
        ConsoleUI.VeTieuDe("CẤU HÌNH COMBO");
        Console.WriteLine($"Đã chọn {dsDaChon.Count} sản phẩm làm nguyên liệu.");
        ConsoleUI.KeVienNgang();

        // 2. Chạy thuật toán
        ChayThuatToanVaHienThi(dsDaChon, "Combo Thủ Công");
    }

    // --- HÀM CHUNG: CHẠY BACKTRACKING & HIỂN THỊ ---
    private static void ChayThuatToanVaHienThi(List<SanPham> dsDauVao, string tieuDe)
    {
        int? m = ConsoleUI.DocSoNguyen($"Mỗi combo gồm bao nhiêu món? (Tối đa {dsDauVao.Count}): ");
        if (m == null || m <= 0 || m > dsDauVao.Count) return;

        int? topK = ConsoleUI.DocSoNguyen("Bạn muốn xem bao nhiêu phương án tốt nhất? (VD: 5): ");
        if (topK == null || topK <= 0) return;

        Console.WriteLine("\nĐang chạy thuật toán Backtracking...");
        var tatCaCombos = LietKeCombos(dsDauVao, m.Value);

        if (tatCaCombos.Count == 0)
        {
            ConsoleUI.HienThiThongBao("Không tìm thấy tổ hợp nào.", ConsoleColor.Yellow);
            return;
        }

        // Sắp xếp theo tổng giá trị giảm dần
        var topCombos = tatCaCombos
            .Select(cb => new { DanhSach = cb, TongGia = cb.Sum(sp => sp.GiaBan) })
            .OrderByDescending(x => x.TongGia)
            .Take(topK.Value)
            .ToList();

        // Hiển thị phân trang
        int trang = 0;
        int size = 3;
        int tongSoTrang = (int)Math.Ceiling((double)topCombos.Count / size);

        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"KẾT QUẢ: {tieuDe.ToUpper()}");
            Console.WriteLine($"Tìm thấy {tatCaCombos.Count} tổ hợp. Hiển thị Top {topK} giá trị cao nhất.");
            ConsoleUI.KeVienNgang();

            var pageData = topCombos.Skip(trang * size).Take(size).ToList();
            int startIndex = trang * size + 1;

            for (int i = 0; i < pageData.Count; i++)
            {
                var item = pageData[i];
                ConsoleUI.DatMau(ConsoleColor.Cyan);
                Console.WriteLine($" #{startIndex + i} - TỔNG GIÁ TRỊ: {item.TongGia:N0} VNĐ");
                Console.ResetColor();
                
                foreach (var sp in item.DanhSach)
                {
                    Console.WriteLine($"   + {sp.TenSP,-30} | {sp.GiaBan:N0}");
                }
                ConsoleUI.KeVienNgang(50);
            }

            Console.WriteLine($"\nTrang {trang + 1}/{tongSoTrang}. [◄/►] Chuyển trang. [Enter] Chọn mua. [Esc] Thoát.");
            var k = Console.ReadKey(true).Key;

            if (k == ConsoleKey.RightArrow && trang < tongSoTrang - 1) trang++;
            else if (k == ConsoleKey.LeftArrow && trang > 0) trang--;
            else if (k == ConsoleKey.Escape) break;
            else if (k == ConsoleKey.Enter) ConsoleUI.HienThiThongBao("Đã thêm Combo vào đơn hàng (Demo)", ConsoleColor.Green);
        }
    }

    // --- LOGIC BACKTRACKING ---
    public static List<List<SanPham>> LietKeCombos(List<SanPham> list, int m)
    {
        var res = new List<List<SanPham>>();
        Backtrack(list, m, 0, new List<SanPham>(), res);
        return res;
    }

    private static void Backtrack(List<SanPham> list, int m, int start, List<SanPham> curr, List<List<SanPham>> res)
    {
        if (curr.Count == m) { res.Add(new List<SanPham>(curr)); return; }
        for (int i = start; i < list.Count; i++)
        {
            curr.Add(list[i]);
            Backtrack(list, m, i + 1, curr, res);
            curr.RemoveAt(curr.Count - 1);
        }
    }
}