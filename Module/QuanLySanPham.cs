using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text; // Cần cho Export CSV
using System.IO;

public static class QuanLySanPham
{
    public static Dictionary<string, SanPham> dsTheoMa = new Dictionary<string, SanPham>();
    public static Dictionary<string, List<string>> dsTheoTen = new Dictionary<string, List<string>>();
    public static Dictionary<string, List<string>> dsTheoDanhMuc = new Dictionary<string, List<string>>();

    // --- LOGIC TỰ ĐỘNG SINH MÃ ---
    private static string TaoMaSPTuDong()
    {
        if (dsTheoMa.Count == 0) return "SP001";
        int maxId = 0;
        foreach (var ma in dsTheoMa.Keys)
        {
            var match = Regex.Match(ma, @"\d+");
            if (match.Success)
            {
                if (int.TryParse(match.Value, out int id))
                {
                    if (id > maxId) maxId = id;
                }
            }
        }
        return $"SP{(maxId + 1):D3}";
    }

    // --- CÁC HÀM LOGIC CRUD (ĐÃ CẬP NHẬT SOFT DELETE) ---
    public static bool ThemSanPham(SanPham sp)
    {
        if (dsTheoMa.ContainsKey(sp.MaSP)) return false;
        dsTheoMa.Add(sp.MaSP, sp);
        CapNhatIndexPhu(sp);
        return true;
    }

    // Thay vì xóa hẳn, ta chỉ đánh dấu IsDeleted = true
    public static bool XoaSanPham(string maSP)
    {
        if (!dsTheoMa.TryGetValue(maSP, out SanPham sp)) return false;
        
        sp.IsDeleted = true; // Soft Delete
        dsTheoMa[maSP] = sp; // Cập nhật lại vào Dictionary
        
        // Không cần xóa index phụ, vì khi tìm kiếm ta sẽ lọc IsDeleted
        return true;
    }

    public static bool KhoiPhucSanPham(string maSP)
    {
        if (!dsTheoMa.TryGetValue(maSP, out SanPham sp)) return false;
        sp.IsDeleted = false; // Khôi phục
        dsTheoMa[maSP] = sp;
        return true;
    }

    public static bool SuaSanPham(SanPham spMoi)
    {
        if (!dsTheoMa.TryGetValue(spMoi.MaSP, out SanPham spCu)) return false;
        dsTheoMa[spMoi.MaSP] = spMoi;

        // Cập nhật Index phụ (Tên & Danh mục)
        string tenCu = spCu.TenSP?.ToLowerInvariant() ?? "";
        string tenMoi = spMoi.TenSP?.ToLowerInvariant() ?? "";
        if (tenCu != tenMoi)
        {
            if (dsTheoTen.ContainsKey(tenCu))
            {
                dsTheoTen[tenCu].Remove(spMoi.MaSP);
                if (dsTheoTen[tenCu].Count == 0) dsTheoTen.Remove(tenCu);
            }
            if (!dsTheoTen.ContainsKey(tenMoi)) dsTheoTen.Add(tenMoi, new List<string>());
            dsTheoTen[tenMoi].Add(spMoi.MaSP);
        }

        string dmCu = spCu.DanhMuc ?? "Chưa phân loại";
        string dmMoi = spMoi.DanhMuc ?? "Chưa phân loại";

        if (dmCu != dmMoi)
        {
            if (dsTheoDanhMuc.ContainsKey(dmCu))
            {
                dsTheoDanhMuc[dmCu].Remove(spMoi.MaSP);
                if (dsTheoDanhMuc[dmCu].Count == 0) dsTheoDanhMuc.Remove(dmCu);
            }
            if (!dsTheoDanhMuc.ContainsKey(dmMoi)) dsTheoDanhMuc.Add(dmMoi, new List<string>());
            dsTheoDanhMuc[dmMoi].Add(spMoi.MaSP);
        }
        return true;
    }

    private static void CapNhatIndexPhu(SanPham sp)
    {
        string tenKey = sp.TenSP?.ToLowerInvariant() ?? "";
        if (!dsTheoTen.ContainsKey(tenKey)) dsTheoTen.Add(tenKey, new List<string>());
        dsTheoTen[tenKey].Add(sp.MaSP);

        string danhMuc = string.IsNullOrEmpty(sp.DanhMuc) ? "Chưa phân loại" : sp.DanhMuc;
        if (!dsTheoDanhMuc.ContainsKey(danhMuc)) dsTheoDanhMuc.Add(danhMuc, new List<string>());
        dsTheoDanhMuc[danhMuc].Add(sp.MaSP);
    }

    // Tìm kiếm: Chỉ trả về SP chưa bị xóa
    public static bool TimTheoMa(string ma, out SanPham sp) 
    {
        bool found = dsTheoMa.TryGetValue(ma, out sp);
        return found && !sp.IsDeleted;
    }

    public static List<SanPham> TimTheoTen(string tenSP)
    {
        List<SanPham> ketQua = new List<SanPham>();
        string tenKey = tenSP.ToLowerInvariant();
        if (dsTheoTen.TryGetValue(tenKey, out List<string> danhSachMaSP))
        {
            foreach (string maSP in danhSachMaSP)
            {
                if (dsTheoMa.TryGetValue(maSP, out SanPham sp) && !sp.IsDeleted) ketQua.Add(sp);
            }
        }
        return ketQua;
    }

    public static List<SanPham> TimTheoDanhMuc(string dm)
    {
        var kq = new List<SanPham>();
        if (dsTheoDanhMuc.TryGetValue(dm, out List<string> dsMa))
            foreach (var ma in dsMa) 
                if (dsTheoMa.TryGetValue(ma, out SanPham sp) && !sp.IsDeleted) kq.Add(sp);
        return kq;
    }

    // --- GIAO DIỆN CONSOLE ---
    public static void HienThiMenu()
    {
        while (true)
        {
            var menu = new List<string> 
            { 
                "1. Xem toàn bộ & Tìm kiếm (Real-time)", 
                "2. Thêm sản phẩm (Tự động mã + Gợi ý)", 
                "3. Sửa sản phẩm", 
                "4. Xóa sản phẩm (Đưa vào thùng rác)", 
                "5. Xem theo Danh mục", 
                "6. Thùng rác (Khôi phục)",
                "7. Xuất Báo cáo Excel (.csv)",
                "0. Quay lại" 
            };
            int chon = ConsoleUI.HienThiMenuChon("== QUẢN LÝ SẢN PHẨM ==", menu);
            switch (chon)
            {
                case 0: GiaoDienTimKiemVaXem(); break;
                case 1: GiaoDienThem(); break;
                case 2: GiaoDienSua(); break;
                case 3: GiaoDienXoa(); break;
                case 4: GiaoDienXemDanhMuc(); break;
                case 5: GiaoDienThungRac(); break;
                case 6: GiaoDienXuatBaoCao(); break;
                case 7: return;
                case -1: return;
            }
        }
    }

    private static void GiaoDienTimKiemVaXem()
    {
        // Chỉ lấy SP chưa xóa
        var listSP = dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
        if (listSP.Count == 0) { ConsoleUI.HienThiThongBao("Kho trống.", ConsoleColor.Red); return; }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "TÌM KIẾM & XEM CHI TIẾT");
        
        if (spChon.HasValue)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("CHI TIẾT SẢN PHẨM");
            HienThiChiTiet(spChon.Value);
            Console.WriteLine("\nNhấn phím bất kỳ để quay lại...");
            Console.ReadKey(true);
        }
    }

    private static void GiaoDienSua()
    {
        var listSP = dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
        if (listSP.Count == 0) { ConsoleUI.HienThiThongBao("Kho trống.", ConsoleColor.Red); return; }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "CHỌN SẢN PHẨM CẦN SỬA");
        
        if (spChon.HasValue)
        {
            SanPham spCu = spChon.Value;
            Console.Clear();
            ConsoleUI.VeTieuDe("SỬA SẢN PHẨM");
            HienThiChiTiet(spCu);
            ConsoleUI.KeVienNgang();
            
            string? ten = ConsoleUI.DocChuoi($"1. Tên ({spCu.TenSP}): ", spCu.TenSP);
            
            // Dùng gợi ý cho danh mục
            var suggestions = dsTheoDanhMuc.Keys.ToList();
            string dm = ConsoleUI.DocChuoiCoGoiY($"2. Danh mục ({spCu.DanhMuc}): ", suggestions) ?? spCu.DanhMuc;
            if (string.IsNullOrEmpty(dm)) dm = spCu.DanhMuc;

            string? dvt = ConsoleUI.DocChuoi($"3. ĐVT ({spCu.DonViTinh}): ", spCu.DonViTinh);
            decimal? gia = ConsoleUI.DocSoThapPhan($"4. Giá ({spCu.GiaBan:N0}): ", spCu.GiaBan);
            int? ton = ConsoleUI.DocSoNguyen($"5. Tồn ({spCu.SoLuongTonKho}): ", spCu.SoLuongTonKho);

            if (ten == null || dvt == null || gia == null || ton == null) return;

            var spMoi = new SanPham(spCu.MaSP, ten, dvt, gia.Value, ton.Value, dm);
            // Giữ lại trạng thái IsDeleted của cái cũ (thường là false)
            spMoi.IsDeleted = spCu.IsDeleted;
            
            if (SuaSanPham(spMoi)) ConsoleUI.HienThiThongBao("Cập nhật thành công!", ConsoleColor.Green);
        }
    }

    private static void GiaoDienXoa()
    {
        var listSP = dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
        if (listSP.Count == 0) { ConsoleUI.HienThiThongBao("Kho trống.", ConsoleColor.Red); return; }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "CHỌN SẢN PHẨM CẦN XÓA");

        if (spChon.HasValue)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("XÓA SẢN PHẨM");
            HienThiChiTiet(spChon.Value);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️ Lưu ý: Sản phẩm sẽ được chuyển vào THÙNG RÁC (Có thể khôi phục).");
            Console.ResetColor();

            if (ConsoleUI.XacNhan("Bạn CHẮC CHẮN muốn xóa sản phẩm này?"))
            {
                XoaSanPham(spChon.Value.MaSP);
                ConsoleUI.HienThiThongBao("Đã chuyển vào thùng rác.", ConsoleColor.Green);
            }
        }
    }

    private static void GiaoDienThungRac()
    {
        // Lấy danh sách đã xóa
        var listDaXoa = dsTheoMa.Values.Where(x => x.IsDeleted).ToList();
        
        if (listDaXoa.Count == 0) 
        {
            ConsoleUI.HienThiThongBao("Thùng rác trống.", ConsoleColor.Green);
            return;
        }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listDaXoa, "THÙNG RÁC - CHỌN ĐỂ KHÔI PHỤC");

        if (spChon.HasValue)
        {
            if (ConsoleUI.XacNhan($"Khôi phục sản phẩm {spChon.Value.TenSP}?"))
            {
                KhoiPhucSanPham(spChon.Value.MaSP);
                ConsoleUI.HienThiThongBao("Đã khôi phục thành công!", ConsoleColor.Green);
            }
        }
    }

    private static void GiaoDienThem()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("THÊM SẢN PHẨM MỚI");
        
        string maTuDong = TaoMaSPTuDong();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[*] Mã sản phẩm (Tự động): {maTuDong}");
        Console.ResetColor();

        string? ten = ConsoleUI.DocChuoi("1. Tên SP: ", "", true);
        if (ten == null) return;
        
        // --- NÂNG CẤP: GỢI Ý DANH MỤC ---
        var dsGoiY = dsTheoDanhMuc.Keys.ToList();
        string? dm = ConsoleUI.DocChuoiCoGoiY("2. Danh mục (Gõ để hiện gợi ý + Tab): ", dsGoiY);
        if (string.IsNullOrEmpty(dm)) dm = "Chưa phân loại";

        string? dvt = ConsoleUI.DocChuoi("3. ĐVT: ", "", true);
        decimal? gia = ConsoleUI.DocSoThapPhan("4. Giá bán: ");
        int? ton = ConsoleUI.DocSoNguyen("5. Tồn kho: ");

        if (dvt == null || gia == null || ton == null) return;

        var sp = new SanPham(maTuDong, ten, dvt, gia.Value, ton.Value, dm);
        ThemSanPham(sp);
        ConsoleUI.HienThiThongBao($"Thêm thành công {maTuDong}!", ConsoleColor.Green);
    }

    private static void GiaoDienXemDanhMuc()
    {
        var listDM = dsTheoDanhMuc.Keys.OrderBy(x => x).ToList();
        if (listDM.Count == 0) { ConsoleUI.HienThiThongBao("Chưa có danh mục.", ConsoleColor.Yellow); return; }
        
        int chon = ConsoleUI.HienThiMenuChon("CHỌN DANH MỤC:", listDM);
        if (chon != -1)
        {
            var listSP = TimTheoDanhMuc(listDM[chon]);
            ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, $"DANH MỤC: {listDM[chon].ToUpper()}");
        }
    }
    
    // --- TÍNH NĂNG MỚI: XUẤT EXCEL ---
    private static void GiaoDienXuatBaoCao()
    {
        try 
        {
            ConsoleUI.VeTieuDe("XUẤT BÁO CÁO EXCEL");
            string fileName = $"BaoCao_SanPham_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            sb.AppendLine("MaSP,TenSP,DanhMuc,DonViTinh,GiaBan,TonKho,TrangThai");
            
            foreach (var sp in dsTheoMa.Values)
            {
                string status = sp.IsDeleted ? "Da Xoa" : "Hien Thi";
                // Xử lý dấu phẩy trong tên để tránh vỡ CSV
                string cleanTen = sp.TenSP.Replace(",", " ");
                string cleanDM = sp.DanhMuc.Replace(",", " ");
                
                sb.AppendLine($"{sp.MaSP},{cleanTen},{cleanDM},{sp.DonViTinh},{sp.GiaBan},{sp.SoLuongTonKho},{status}");
            }

            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
            
            // Lấy đường dẫn tuyệt đối để in ra cho oách
            string fullPath = Path.GetFullPath(fileName);
            Console.WriteLine($"\nĐã xuất {dsTheoMa.Count} dòng dữ liệu.");
            Console.WriteLine($"File lưu tại: {fullPath}");
            ConsoleUI.HienThiThongBao("Xuất báo cáo thành công!", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            ConsoleUI.HienThiThongBao($"Lỗi xuất file: {ex.Message}", ConsoleColor.Red);
        }
    }

    private static void HienThiChiTiet(SanPham sp)
    {
        Console.WriteLine($" Mã SP   : {sp.MaSP}");
        Console.WriteLine($" Tên SP  : {sp.TenSP}");
        Console.WriteLine($" Danh mục: {sp.DanhMuc}");
        Console.WriteLine($" ĐVT     : {sp.DonViTinh}");
        Console.WriteLine($" Giá bán : {sp.GiaBan:N0} VNĐ");
        Console.WriteLine($" Tồn kho : {sp.SoLuongTonKho}");
        if (sp.IsDeleted) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" TRẠNG THÁI: ĐÃ XÓA (TRONG THÙNG RÁC)");
            Console.ResetColor();
        }
    }
}