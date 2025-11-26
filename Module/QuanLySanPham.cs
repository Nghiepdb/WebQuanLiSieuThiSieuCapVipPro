using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text; 
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

    // Xóa mềm (Soft Delete) -> Đưa vào thùng rác
    public static bool XoaSanPham(string maSP)
    {
        if (!dsTheoMa.TryGetValue(maSP, out SanPham sp)) return false;
        
        sp.IsDeleted = true; 
        dsTheoMa[maSP] = sp; 
        
        return true;
    }

    // [MỚI] Xóa vĩnh viễn (Hard Delete) -> Bay màu khỏi RAM
    public static bool XoaVinhVien(string maSP)
    {
        if (!dsTheoMa.TryGetValue(maSP, out SanPham sp)) return false;

        // 1. Xóa khỏi Dictionary chính
        dsTheoMa.Remove(maSP);

        // 2. Xóa khỏi Index theo Tên
        string tenKey = sp.TenSP?.ToLowerInvariant() ?? "";
        if (dsTheoTen.ContainsKey(tenKey))
        {
            dsTheoTen[tenKey].Remove(maSP);
            if (dsTheoTen[tenKey].Count == 0) dsTheoTen.Remove(tenKey);
        }

        // 3. Xóa khỏi Index theo Danh mục
        string dmKey = sp.DanhMuc ?? "Chưa phân loại";
        if (dsTheoDanhMuc.ContainsKey(dmKey))
        {
            dsTheoDanhMuc[dmKey].Remove(maSP);
            if (dsTheoDanhMuc[dmKey].Count == 0) dsTheoDanhMuc.Remove(dmKey);
        }

        return true;
    }

    // [MỚI] Dọn sạch thùng rác
    public static void DonSachThungRac()
    {
        // Lấy danh sách ID cần xóa để tránh lỗi khi đang duyệt Dictionary
        var listCanXoa = dsTheoMa.Values.Where(x => x.IsDeleted).Select(x => x.MaSP).ToList();
        foreach (var ma in listCanXoa)
        {
            XoaVinhVien(ma);
        }
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
                "6. Thùng rác (Khôi phục & Dọn dẹp)", // Cập nhật tiêu đề
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

    // --- [REFACTOR] THÊM SẢN PHẨM THEO BƯỚC ---
    private static void GiaoDienThem()
    {
        string maTuDong = TaoMaSPTuDong();
        string? ten = "";
        string dm = "Chưa phân loại";
        string? dvt = "";
        decimal? gia = 0;
        int? ton = 0;

        // --- BƯỚC 1: TÊN SẢN PHẨM ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"THÊM SẢN PHẨM MỚI ({maTuDong}) - BƯỚC 1/5");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[*] Mã sản phẩm: {maTuDong}");
            Console.ResetColor();
            ConsoleUI.KeVienNgang();

            ten = ConsoleUI.DocChuoi("1. Tên SP: ", "", true);
            if (ten == null) return; // ESC
            
            if (string.IsNullOrWhiteSpace(ten))
            {
                ConsoleUI.HienThiThongBao("Tên không được để trống!", ConsoleColor.Red);
                continue;
            }
            break;
        }

        // --- BƯỚC 2: DANH MỤC (GỢI Ý) ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"THÊM SẢN PHẨM MỚI ({maTuDong}) - BƯỚC 2/5");
            Console.WriteLine($"Tên SP: {ten}");
            ConsoleUI.KeVienNgang();

            var dsGoiY = dsTheoDanhMuc.Keys.ToList();
            string? dmInput = ConsoleUI.DocChuoiCoGoiY("2. Danh mục (Gõ để hiện gợi ý + Tab): ", dsGoiY);
            
            if (dmInput == null) return; // ESC

            dm = string.IsNullOrEmpty(dmInput) ? "Chưa phân loại" : dmInput;
            break;
        }

        // --- BƯỚC 3: ĐƠN VỊ TÍNH ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"THÊM SẢN PHẨM MỚI ({maTuDong}) - BƯỚC 3/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"Danh mục: {dm}");
            ConsoleUI.KeVienNgang();

            dvt = ConsoleUI.DocChuoi("3. ĐVT (VD: Cái, Hộp...): ", "", true);
            if (dvt == null) return; // ESC

            if (string.IsNullOrWhiteSpace(dvt))
            {
                ConsoleUI.HienThiThongBao("ĐVT không được để trống!", ConsoleColor.Red);
                continue;
            }
            break;
        }

        // --- BƯỚC 4: GIÁ BÁN ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"THÊM SẢN PHẨM MỚI ({maTuDong}) - BƯỚC 4/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"ĐVT     : {dvt}");
            ConsoleUI.KeVienNgang();

            gia = ConsoleUI.DocSoThapPhan("4. Giá bán (VNĐ): ");
            if (gia == null) return; // ESC
            break;
        }

        // --- BƯỚC 5: TỒN KHO ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"THÊM SẢN PHẨM MỚI ({maTuDong}) - BƯỚC 5/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"Giá bán : {gia:N0}");
            ConsoleUI.KeVienNgang();

            ton = ConsoleUI.DocSoNguyen("5. Tồn kho ban đầu: ");
            if (ton == null) return; // ESC
            break;
        }

        // --- LƯU ---
        var sp = new SanPham(maTuDong, ten, dvt, gia.Value, ton.Value, dm);
        ThemSanPham(sp);
        Console.Clear();
        ConsoleUI.HienThiThongBao($"Thêm thành công {maTuDong}!", ConsoleColor.Green);
    }

    // --- [REFACTOR] SỬA SẢN PHẨM THEO BƯỚC ---
    private static void GiaoDienSua()
    {
        var listSP = dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
        if (listSP.Count == 0) { ConsoleUI.HienThiThongBao("Kho trống.", ConsoleColor.Red); return; }

        SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "CHỌN SẢN PHẨM CẦN SỬA");
        if (!spChon.HasValue) return;

        SanPham spCu = spChon.Value;
        
        string? ten = "", dvt = "";
        string dm = spCu.DanhMuc;
        decimal? gia = 0;
        int? ton = 0;

        // --- BƯỚC 1: SỬA TÊN ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"SỬA SẢN PHẨM: {spCu.MaSP} - BƯỚC 1/5");
            HienThiChiTiet(spCu);
            ConsoleUI.KeVienNgang();
            
            ten = ConsoleUI.DocChuoi($"Tên mới ({spCu.TenSP}): ", spCu.TenSP);
            if (ten == null) return; // ESC
            break;
        }

        // --- BƯỚC 2: SỬA DANH MỤC ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"SỬA SẢN PHẨM: {spCu.MaSP} - BƯỚC 2/5");
            Console.WriteLine($"Tên SP: {ten}");
            ConsoleUI.KeVienNgang();

            var suggestions = dsTheoDanhMuc.Keys.ToList();
            string? dmInput = ConsoleUI.DocChuoiCoGoiY($"Danh mục ({spCu.DanhMuc}): ", suggestions);
            
            if (dmInput == null) return; // ESC
            
            dm = string.IsNullOrEmpty(dmInput) ? spCu.DanhMuc : dmInput;
            break;
        }

        // --- BƯỚC 3: SỬA ĐVT ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"SỬA SẢN PHẨM: {spCu.MaSP} - BƯỚC 3/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"Danh mục: {dm}");
            ConsoleUI.KeVienNgang();

            dvt = ConsoleUI.DocChuoi($"ĐVT ({spCu.DonViTinh}): ", spCu.DonViTinh);
            if (dvt == null) return;
            break;
        }

        // --- BƯỚC 4: SỬA GIÁ ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"SỬA SẢN PHẨM: {spCu.MaSP} - BƯỚC 4/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"ĐVT     : {dvt}");
            ConsoleUI.KeVienNgang();

            gia = ConsoleUI.DocSoThapPhan($"Giá bán ({spCu.GiaBan:N0}): ", spCu.GiaBan);
            if (gia == null) return;
            break;
        }

        // --- BƯỚC 5: SỬA TỒN ---
        while (true)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe($"SỬA SẢN PHẨM: {spCu.MaSP} - BƯỚC 5/5");
            Console.WriteLine($"Tên SP  : {ten}");
            Console.WriteLine($"Giá bán : {gia:N0}");
            ConsoleUI.KeVienNgang();

            ton = ConsoleUI.DocSoNguyen($"Tồn kho ({spCu.SoLuongTonKho}): ", spCu.SoLuongTonKho);
            if (ton == null) return;
            break;
        }

        // --- LƯU ---
        var spMoi = new SanPham(spCu.MaSP, ten, dvt, gia.Value, ton.Value, dm);
        spMoi.IsDeleted = spCu.IsDeleted;
        
        if (SuaSanPham(spMoi)) 
        {
            Console.Clear();
            ConsoleUI.HienThiThongBao("Cập nhật thành công!", ConsoleColor.Green);
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

    // --- [NÂNG CẤP] GIAO DIỆN THÙNG RÁC (CÓ DỌN DẸP) ---
    private static void GiaoDienThungRac()
    {
        while (true)
        {
            Console.Clear(); // Fix lỗi đè chữ
            int soLuongRac = dsTheoMa.Values.Count(x => x.IsDeleted);
            
            var menu = new List<string>
            {
                $"1. Xem & Khôi phục/Xóa lẻ (Đang có {soLuongRac} món)",
                "2. Dọn sạch thùng rác (Xóa vĩnh viễn tất cả)",
                "0. Quay lại"
            };

            int chon = ConsoleUI.HienThiMenuChon("QUẢN LÝ THÙNG RÁC", menu);
            
            if (chon == 0)
            {
                // Xem & Xử lý từng món
                var listDaXoa = dsTheoMa.Values.Where(x => x.IsDeleted).ToList();
                if (listDaXoa.Count == 0) 
                {
                    ConsoleUI.HienThiThongBao("Thùng rác trống.", ConsoleColor.Green);
                    continue;
                }

                SanPham? spChon = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listDaXoa, "THÙNG RÁC - CHỌN ĐỂ XỬ LÝ");

                if (spChon.HasValue)
                {
                    Console.Clear();
                    ConsoleUI.VeTieuDe("TÙY CHỌN XỬ LÝ");
                    HienThiChiTiet(spChon.Value);
                    ConsoleUI.KeVienNgang();
                    Console.WriteLine("1. Khôi phục (Đưa lại kho hàng)");
                    Console.WriteLine("2. Xóa vĩnh viễn (Không thể khôi phục)");
                    Console.WriteLine("0. Hủy");
                    
                    int? opt = ConsoleUI.DocSoNguyen("Lựa chọn: ");
                    if (opt == 1)
                    {
                        KhoiPhucSanPham(spChon.Value.MaSP);
                        ConsoleUI.HienThiThongBao("Đã khôi phục!", ConsoleColor.Green);
                    }
                    else if (opt == 2)
                    {
                        if (ConsoleUI.XacNhan("CẢNH BÁO: Xóa vĩnh viễn?"))
                        {
                            XoaVinhVien(spChon.Value.MaSP);
                            ConsoleUI.HienThiThongBao("Đã xóa vĩnh viễn!", ConsoleColor.Green);
                        }
                    }
                }
            }
            else if (chon == 1)
            {
                // Dọn sạch
                if (soLuongRac == 0)
                {
                    ConsoleUI.HienThiThongBao("Thùng rác đã trống sẵn.", ConsoleColor.Green);
                    continue;
                }

                Console.Clear();
                ConsoleUI.VeTieuDe("DỌN DẸP THÙNG RÁC");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"CẢNH BÁO: Bạn sắp xóa vĩnh viễn {soLuongRac} sản phẩm.");
                Console.WriteLine("Hành động này KHÔNG THỂ hoàn tác!");
                Console.ResetColor();

                if (ConsoleUI.XacNhan("Xác nhận dọn sạch thùng rác?"))
                {
                    DonSachThungRac();
                    ConsoleUI.HienThiThongBao("Thùng rác đã được dọn sạch!", ConsoleColor.Green);
                }
            }
            else if (chon == 2 || chon == -1)
            {
                return; // Thoát
            }
        }
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
    
    private static void GiaoDienXuatBaoCao()
    {
        try 
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("XUẤT BÁO CÁO EXCEL");
            string fileName = $"BaoCao_SanPham_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            sb.AppendLine("MaSP,TenSP,DanhMuc,DonViTinh,GiaBan,TonKho,TrangThai");
            
            foreach (var sp in dsTheoMa.Values)
            {
                string status = sp.IsDeleted ? "Da Xoa" : "Hien Thi";
                string cleanTen = sp.TenSP.Replace(",", " ");
                string cleanDM = sp.DanhMuc.Replace(",", " ");
                
                sb.AppendLine($"{sp.MaSP},{cleanTen},{cleanDM},{sp.DonViTinh},{sp.GiaBan},{sp.SoLuongTonKho},{status}");
            }

            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
            
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