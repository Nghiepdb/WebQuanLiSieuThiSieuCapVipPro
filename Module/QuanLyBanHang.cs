using System;
using System.Collections.Generic;
using System.Linq;

public static class QuanLyBanHang
{
    private static PhienLamViec? CaHienTai;
    private static List<ChiTietHoaDon> GioHang = new List<ChiTietHoaDon>();
    private static KhachHang? KhachHienTai;

    public static void HienThiMenu()
    {
        bool dangChay = true;
        while (dangChay)
        {
            Console.Clear();
            ConsoleUI.VeTieuDe("QUẦY THU NGÂN (POS) - FEFO ENABLED");
            
            if (CaHienTai == null)
            {
                ConsoleUI.HienThiThongBao("CHƯA MỞ CA LÀM VIỆC!", ConsoleColor.Red);
                if (ConsoleUI.XacNhan("Bạn có muốn mở ca mới ngay bây giờ?")) MoCa();
                else return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[CA: {CaHienTai?.MaPhien}] - NV: {Program.CurrentUser?.HoTen}");
            Console.WriteLine($"Bắt đầu lúc: {CaHienTai?.ThoiGianBatDau:dd/MM/yyyy HH:mm:ss}");
            Console.ResetColor();

            var menu = new List<string> 
            { 
                "1. Bắt đầu giao dịch mới (Scan -> Tích điểm -> FEFO)", 
                "2. Xem lịch sử hóa đơn trong ca", 
                "3. Kết ca (Bàn giao tiền & Thoát)", 
                "4. Xử lý Trả hàng (Đổi/Trả & Hoàn tiền)",
                "5. Đổi mật khẩu cá nhân", // [MỚI]
                "0. Thoát ra" 
            };

            int chon = ConsoleUI.HienThiMenuChon("CHỨC NĂNG", menu);
            switch (chon)
            {
                case 0: GiaoDichMoi(); break;
                case 1: XemLichSuCa(); break;
                case 2: KetCa(); dangChay = false; break;
                case 3: XuLyTraHang(); break;
                case 4: Program.DoiMatKhau(); break; // [MỚI] Gọi hàm từ Program
                case 5: dangChay = false; break;
                case -1: dangChay = false; break;
            }
        }
    }

    // --- LOGIC TRẢ HÀNG MỚI ---
    private static void XuLyTraHang()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("XỬ LÝ TRẢ HÀNG & HOÀN TIỀN");

        // 1. Tìm hóa đơn
        string? maHD = ConsoleUI.DocChuoi(">> Nhập Mã Hóa Đơn gốc: ");
        if (string.IsNullOrEmpty(maHD)) return;

        var hd = Database.HoaDons.FirstOrDefault(x => x.MaHoaDon.Equals(maHD, StringComparison.OrdinalIgnoreCase));
        if (hd == null)
        {
            ConsoleUI.HienThiThongBao("Không tìm thấy hóa đơn này!", ConsoleColor.Red);
            return;
        }

        // Hiển thị chi tiết để chọn
        Console.WriteLine($"Hóa đơn: {hd.MaHoaDon} | Ngày: {hd.NgayLap:dd/MM/yyyy}");
        Console.WriteLine("Danh sách sản phẩm đã mua:");
        for (int i = 0; i < hd.ChiTiet.Count; i++)
        {
            var item = hd.ChiTiet[i];
            Console.WriteLine($"  {i + 1}. {item.TenSP} (SL: {item.SoLuong}) - Giá: {item.DonGia:N0}");
        }
        ConsoleUI.KeVienNgang();

        // 2. Chọn sản phẩm trả
        int? stt = ConsoleUI.DocSoNguyen(">> Chọn STT sản phẩm muốn trả: ");
        if (stt == null || stt < 1 || stt > hd.ChiTiet.Count) return;

        var spTra = hd.ChiTiet[stt.Value - 1];
        
        // 3. Nhập số lượng và Lý do
        int? slTra = ConsoleUI.DocSoNguyen($">> Nhập số lượng trả (Tối đa {spTra.SoLuong}): ");
        if (slTra == null || slTra <= 0 || slTra > spTra.SoLuong)
        {
            ConsoleUI.HienThiThongBao("Số lượng không hợp lệ!", ConsoleColor.Red);
            return;
        }

        string? lyDo = ConsoleUI.DocChuoi(">> Lý do trả hàng (VD: Lỗi NSX, Hư hỏng): ", "", true);

        // 4. Xác nhận
        decimal tienHoan = slTra.Value * spTra.DonGia;
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"XÁC NHẬN TRẢ: {slTra} x {spTra.TenSP}");
        Console.WriteLine($"TIỀN CẦN HOÀN LẠI KHÁCH: {tienHoan:N0} VNĐ");
        Console.ResetColor();

        if (ConsoleUI.XacNhan("Đồng ý lập phiếu trả hàng?"))
        {
            // Tạo phiếu trả
            var phieu = new PhieuTraHang
            {
                MaPhieu = "TH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                MaHoaDonGoc = hd.MaHoaDon,
                NguoiXuLy = Program.CurrentUser?.Username ?? "Unknown",
                NgayTra = DateTime.Now,
                MaSP = spTra.MaSP,
                TenSP = spTra.TenSP,
                SoLuongTra = slTra.Value,
                SoTienHoan = tienHoan,
                LyDo = lyDo ?? "Khác"
            };

            Database.PhieuTraHangs.Add(phieu);
            
            // Trừ tiền trong ca hiện tại (Vì phải lấy tiền két trả khách)
            if (CaHienTai != null) CaHienTai.TienKetCaHeThong -= tienHoan;

            Database.LuuDuLieu();
            ConsoleUI.HienThiThongBao($"Đã lập phiếu {phieu.MaPhieu}. Hãy hoàn tiền cho khách.", ConsoleUI.MauThanhCong);
        }
    }

    private static void MoCa()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("MỞ CA LÀM VIỆC");
        decimal? tienDauCa = ConsoleUI.DocSoThapPhan("Nhập số tiền đầu ca (VNĐ): ");
        
        CaHienTai = new PhienLamViec
        {
            MaPhien = DateTime.Now.ToString("yyyyMMddHHmmss"),
            UsernameNhanVien = Program.CurrentUser?.Username ?? "Unknown",
            ThoiGianBatDau = DateTime.Now,
            TienDauCa = tienDauCa ?? 0,
            TienKetCaHeThong = tienDauCa ?? 0
        };
        
        // Lưu lịch sử ca
        Database.LichSuCa.Add(CaHienTai);
        Database.LuuDuLieu();
        ConsoleUI.HienThiThongBao("Đã mở ca thành công! Sẵn sàng bán hàng.", ConsoleUI.MauThanhCong);
    }

    private static void GiaoDichMoi()
    {
        GioHang.Clear();
        KhachHienTai = null;

        // --- BƯỚC 1: QUÉT SẢN PHẨM ---
        while (true)
        {
            // Chỉ lấy sản phẩm chưa bị xóa
            var listSP = QuanLySanPham.dsTheoMa.Values.Where(x => !x.IsDeleted).ToList();
            
            if (listSP.Count == 0)
            {
                ConsoleUI.HienThiThongBao("Kho hàng trống hoặc chưa có sản phẩm nào khả dụng.", ConsoleColor.Red);
                return;
            }

            SanPham? sp = ConsoleUI.ChonMotSanPhamVoiBoLocRealTime(listSP, "TÌM & THÊM SẢN PHẨM (ESC ĐỂ CHỐT ĐƠN)");
            
            if (sp == null) 
            {
                if (GioHang.Count == 0) return; 
                break; 
            }

            Console.Clear();
            ConsoleUI.VeTieuDe("NHẬP SỐ LƯỢNG");
            
            // --- LOGIC AUTO-FIX (TỰ CHỮA LÀNH DỮ LIỆU) ---
            var cacLoKhaDung = Database.LoHangs
                .Where(l => l.MaSP == sp.Value.MaSP && l.HanSuDung > DateTime.Now)
                .ToList();
            
            int tonKhoThucTe = cacLoKhaDung.Sum(l => l.SoLuong);

            if (tonKhoThucTe == 0 && sp.Value.SoLuongTonKho > 0)
            {
                var loTuDong = new LoHang 
                {
                    MaLo = $"AUTO_{DateTime.Now.Ticks}",
                    MaSP = sp.Value.MaSP,
                    SoLuong = sp.Value.SoLuongTonKho,
                    NgayNhap = DateTime.Now,
                    HanSuDung = DateTime.Now.AddMonths(12)
                };
                Database.LoHangs.Add(loTuDong);
                cacLoKhaDung.Add(loTuDong);
                tonKhoThucTe = loTuDong.SoLuong;
                Database.LuuDuLieu(); 
            }
            // ------------------------------------------------

            Console.WriteLine($"Sản phẩm: {sp.Value.TenSP}");
            Console.WriteLine($"Giá bán : {sp.Value.GiaBan:N0} VNĐ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Tồn kho khả dụng (Tổng các lô): {tonKhoThucTe} {sp.Value.DonViTinh}");
            Console.ResetColor();
            ConsoleUI.KeVienNgang();

            if (GioHang.Count > 0)
            {
                Console.WriteLine($"--- Giỏ hàng hiện tại ({GioHang.Count} món) ---");
                Console.WriteLine($"Tổng tiền tạm tính: {GioHang.Sum(x=>x.ThanhTien):N0} VNĐ");
                ConsoleUI.KeVienNgang();
            }

            int? sl = ConsoleUI.DocSoNguyen(">> Nhập số lượng khách mua (Mặc định 1): ", 1);
            if (!sl.HasValue) sl = 1;

            if (sl.Value > 0)
            {
                int daCoTrongGio = GioHang.Where(x => x.MaSP == sp.Value.MaSP).Sum(x => x.SoLuong);
                
                if (sl.Value + daCoTrongGio > tonKhoThucTe)
                {
                    ConsoleUI.HienThiThongBao($"Không đủ hàng! Tồn kho: {tonKhoThucTe}. Đã có trong giỏ: {daCoTrongGio}", ConsoleUI.MauLoi);
                }
                else
                {
                    var itemTonTai = GioHang.FirstOrDefault(x => x.MaSP == sp.Value.MaSP);
                    if (itemTonTai != null)
                    {
                        itemTonTai.SoLuong += sl.Value;
                        ConsoleUI.HienThiThongBao($"Cập nhật: {itemTonTai.TenSP} thành {itemTonTai.SoLuong} {sp.Value.DonViTinh}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        GioHang.Add(new ChiTietHoaDon 
                        { 
                            MaSP = sp.Value.MaSP, 
                            TenSP = sp.Value.TenSP, 
                            SoLuong = sl.Value, 
                            DonGia = sp.Value.GiaBan 
                        });
                        ConsoleUI.HienThiThongBao("Đã thêm vào giỏ.", ConsoleColor.Green);
                    }
                }
            }
        }

        // --- BƯỚC 2: XÁC ĐỊNH KHÁCH HÀNG ---
        if (GioHang.Count > 0)
        {
            XuLyKhachHang();
            ThanhToan();
        }
    }

    private static void XuLyKhachHang()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("BƯỚC 2: THÔNG TIN KHÁCH HÀNG");
        HienThiGioHang();
        Console.WriteLine();

        while (true)
        {
            string? sdt = ConsoleUI.DocChuoi(">> Nhập SĐT Khách hàng (Enter để bỏ qua): ");
            
            if (string.IsNullOrEmpty(sdt))
            {
                KhachHienTai = null;
                Console.WriteLine("=> Khách vãng lai.");
                break;
            }

            KhachHienTai = Database.KhachHangs.FirstOrDefault(k => k.SoDienThoai == sdt);
            
            if (KhachHienTai != null)
            {
                KhachHienTai.CapNhatHang();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n✅ {KhachHienTai.TenKhach} | Hạng: {KhachHienTai.Hang} | Điểm: {KhachHienTai.TongChiTieu:N0}");
                Console.ResetColor();
                break;
            }
            else
            {
                ConsoleUI.HienThiThongBao("SĐT mới!", ConsoleColor.Yellow);
                if (ConsoleUI.XacNhan("Tạo thành viên mới?"))
                {
                    string? ten = ConsoleUI.DocChuoi(">> Tên khách hàng: ", "", true);
                    if (ten != null)
                    {
                        KhachHienTai = new KhachHang { SoDienThoai = sdt, TenKhach = ten, TongChiTieu = 0 };
                        Database.KhachHangs.Add(KhachHienTai);
                        Database.LuuDuLieu(); 
                        ConsoleUI.HienThiThongBao($"Đã tạo hồ sơ cho {ten}.", ConsoleUI.MauThanhCong);
                        break;
                    }
                }
                else
                {
                    KhachHienTai = null;
                    break;
                }
            }
        }
        Console.WriteLine("\nNhấn phím bất kỳ để thanh toán...");
        Console.ReadKey();
    }

    private static void HienThiGioHang()
    {
        Console.WriteLine($"{"Tên SP",-30} | {"SL",-5} | {"Đơn Giá",-15} | {"Thành Tiền",-15}");
        ConsoleUI.KeVienNgang();
        foreach (var item in GioHang)
        {
            Console.WriteLine($"{item.TenSP,-30} | {item.SoLuong,-5} | {item.DonGia,15:N0} | {item.ThanhTien,15:N0}");
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nTỔNG TIỀN HÀNG: {GioHang.Sum(x => x.ThanhTien):N0} VNĐ");
        Console.ResetColor();
    }

    private static void ThanhToan()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("BƯỚC 3: THANH TOÁN & TRỪ KHO (FEFO)");
        
        decimal tongTienHang = GioHang.Sum(x => x.ThanhTien);
        decimal giamGia = 0;

        if (KhachHienTai != null)
            giamGia = tongTienHang * KhachHienTai.LayPhanTramGiamGia();

        decimal phaiTra = tongTienHang - giamGia;

        HienThiGioHang();
        ConsoleUI.KeVienNgang();
        Console.WriteLine($"Tổng tiền hàng : {tongTienHang:N0}");
        if (giamGia > 0) 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Giảm giá ({KhachHienTai?.Hang}): -{giamGia:N0}");
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"KHÁCH PHẢI TRẢ: {phaiTra:N0} VNĐ");
        Console.ResetColor();

        decimal khachDua = 0;
        while (true)
        {
            decimal? nhap = ConsoleUI.DocSoThapPhan("\nKhách đưa (0: Trả Thẻ): ");
            if (nhap != null)
            {
                khachDua = nhap.Value;
                if (khachDua == 0) { khachDua = phaiTra; Console.WriteLine("=> Đã thanh toán Thẻ."); break; }
                if (khachDua >= phaiTra) { Console.WriteLine($"=> Tiền thừa: {khachDua - phaiTra:N0} VNĐ"); break; }
                ConsoleUI.HienThiThongBao("Tiền thiếu!", ConsoleUI.MauLoi);
            }
        }

        var hd = new HoaDon
        {
            MaHoaDon = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            NguoiTao = Program.CurrentUser?.Username ?? "Unknown",
            SdtKhachHang = KhachHienTai?.SoDienThoai,
            NgayLap = DateTime.Now,
            TongTienHang = tongTienHang,
            GiamGia = giamGia,
            ChiTiet = new List<ChiTietHoaDon>(GioHang)
        };
        Database.HoaDons.Add(hd);

        if (KhachHienTai != null)
        {
            KhachHienTai.TongChiTieu += phaiTra;
            KhachHienTai.CapNhatHang();
        }

        if (CaHienTai != null) CaHienTai.TienKetCaHeThong += phaiTra;

        Console.WriteLine("\nĐang trừ kho theo lô hạn sử dụng gần nhất...");
        foreach (var item in GioHang)
        {
            int soLuongCanTru = item.SoLuong;
            
            var cacLoHang = Database.LoHangs
                .Where(l => l.MaSP == item.MaSP && l.SoLuong > 0 && l.HanSuDung >= DateTime.Today)
                .OrderBy(l => l.HanSuDung)
                .ToList();

            foreach (var lo in cacLoHang)
            {
                if (soLuongCanTru <= 0) break;
                int layRa = Math.Min(soLuongCanTru, lo.SoLuong); 
                lo.SoLuong -= layRa;
                soLuongCanTru -= layRa;
                Console.WriteLine($" -> Trừ {layRa} {item.TenSP} từ lô {lo.MaLo} (HSD: {lo.HanSuDung:dd/MM/yyyy})");
            }

            if (soLuongCanTru > 0)
            {
                ConsoleUI.HienThiThongBao($"Cảnh báo: Sản phẩm {item.TenSP} bị lệch kho! Còn thiếu {soLuongCanTru} chưa trừ được.", ConsoleColor.Red);
            }

            if (QuanLySanPham.dsTheoMa.TryGetValue(item.MaSP, out SanPham sp))
            {
                int tonKhoMoi = Database.LoHangs.Where(l => l.MaSP == item.MaSP).Sum(l => l.SoLuong);
                sp.SoLuongTonKho = tonKhoMoi;
                QuanLySanPham.dsTheoMa[item.MaSP] = sp;
            }
        }

        Database.LuuDuLieu();
        ConsoleUI.HienThiThongBao("Giao dịch thành công!", ConsoleUI.MauThanhCong);
    }

    private static void XemLichSuCa()
    {
        Console.Clear();
        ConsoleUI.VeTieuDe("LỊCH SỬ CA HIỆN TẠI");
        
        var ds = Database.HoaDons
            .Where(h => h.NgayLap >= CaHienTai?.ThoiGianBatDau && h.NguoiTao == CaHienTai?.UsernameNhanVien)
            .OrderByDescending(h => h.NgayLap)
            .ToList();

        if (ds.Count == 0) Console.WriteLine("(Trống)");
        else
        {
            Console.WriteLine($"{"Mã HĐ",-20} | {"Thời gian",-10} | {"Khách",-15} | {"Thành Tiền",15}");
            ConsoleUI.KeVienNgang();
            foreach (var hd in ds)
            {
                string khach = "Khách lẻ";
                if (hd.SdtKhachHang != null)
                {
                    var k = Database.KhachHangs.FirstOrDefault(x => x.SoDienThoai == hd.SdtKhachHang);
                    if (k != null) khach = k.TenKhach;
                }
                Console.WriteLine($"{hd.MaHoaDon,-20} | {hd.NgayLap:HH:mm} | {khach,-15} | {hd.ThanhTien,15:N0}");
            }
        }
        Console.ReadKey();
    }

    private static void KetCa()
    {
        if (CaHienTai == null) return;
        Console.Clear();
        ConsoleUI.VeTieuDe("KẾT CA LÀM VIỆC");
        
        CaHienTai.ThoiGianKetThuc = DateTime.Now;
        
        Console.WriteLine($"Bắt đầu: {CaHienTai.ThoiGianBatDau:dd/MM HH:mm}");
        Console.WriteLine($"Kết thúc: {CaHienTai.ThoiGianKetThuc:dd/MM HH:mm}");
        ConsoleUI.KeVienNgang();
        Console.WriteLine($"Tiền đầu ca       : {CaHienTai.TienDauCa:N0}");
        Console.WriteLine($"Doanh thu hệ thống: {CaHienTai.TienKetCaHeThong - CaHienTai.TienDauCa:N0}");
        Console.WriteLine($"TỔNG TIỀN KÉT     : {CaHienTai.TienKetCaHeThong:N0} VNĐ");
        
        decimal? thucTe = ConsoleUI.DocSoThapPhan("\n>> Tiền thực tế đếm được: ");
        CaHienTai.TienKetCaThucTe = thucTe ?? 0;

        decimal chenhLech = CaHienTai.ChenhLech;
        
        Console.WriteLine();
        if (chenhLech == 0) 
            ConsoleUI.HienThiThongBao("✅ Khớp tiền.", ConsoleUI.MauThanhCong);
        else if (chenhLech > 0) 
            ConsoleUI.HienThiThongBao($"⚠️ DƯ: {chenhLech:N0}", ConsoleColor.Yellow);
        else 
            ConsoleUI.HienThiThongBao($"❌ THIẾU: {Math.Abs(chenhLech):N0}", ConsoleUI.MauLoi);

        Database.LuuDuLieu();
        CaHienTai = null;
    }
}