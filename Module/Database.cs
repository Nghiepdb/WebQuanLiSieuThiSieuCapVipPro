using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

public static class Database
{
    // --- ĐƯỜNG DẪN FILE ---
    private const string FILE_SANPHAM = "Data/sanpham.json";
    private const string FILE_NGUOIDUNG = "Data/users.json";
    private const string FILE_KHACHHANG = "Data/customers.json";
    private const string FILE_LOHANG = "Data/batches.json";
    private const string FILE_HOADON = "Data/invoices.json";
    private const string FILE_CA = "Data/shifts.json";
    private const string FILE_TRAHANG = "Data/returns.json"; // [MỚI] File phiếu trả

    // --- DỮ LIỆU TẬP TRUNG (RAM) ---
    public static List<NguoiDung> NguoiDungs = new List<NguoiDung>();
    public static List<KhachHang> KhachHangs = new List<KhachHang>();
    public static List<LoHang> LoHangs = new List<LoHang>();
    public static List<HoaDon> HoaDons = new List<HoaDon>();
    public static List<PhienLamViec> LichSuCa = new List<PhienLamViec>();
    public static List<PhieuTraHang> PhieuTraHangs = new List<PhieuTraHang>(); // [MỚI] List RAM
    
    private static JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static void LoadDuLieu()
    {
        Console.WriteLine("Đang khởi động hệ thống cơ sở dữ liệu...");
        Directory.CreateDirectory("Data"); 

        LoadSanPham();
        LoadGeneric(FILE_NGUOIDUNG, ref NguoiDungs);
        EnsureDefaultAdmin(); 

        LoadGeneric(FILE_KHACHHANG, ref KhachHangs);
        LoadGeneric(FILE_LOHANG, ref LoHangs);
        LoadGeneric(FILE_HOADON, ref HoaDons);
        LoadGeneric(FILE_CA, ref LichSuCa);
        LoadGeneric(FILE_TRAHANG, ref PhieuTraHangs); // [MỚI] Load phiếu trả

        Console.WriteLine("Tải dữ liệu hoàn tất.");
    }

    public static void LuuDuLieu()
    {
        Console.WriteLine("Đang lưu dữ liệu...");
        SaveSanPham();
        SaveGeneric(FILE_NGUOIDUNG, NguoiDungs);
        SaveGeneric(FILE_KHACHHANG, KhachHangs);
        SaveGeneric(FILE_LOHANG, LoHangs);
        SaveGeneric(FILE_HOADON, HoaDons);
        SaveGeneric(FILE_CA, LichSuCa);
        SaveGeneric(FILE_TRAHANG, PhieuTraHangs); // [MỚI] Lưu phiếu trả
        Console.WriteLine("Lưu thành công!");
    }

    private static void LoadGeneric<T>(string path, ref List<T> list)
    {
        if (!File.Exists(path)) return;
        try
        {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<List<T>>(json);
            if (data != null) list = data;
        }
        catch { Console.WriteLine($"Lỗi đọc file {path}"); }
    }

    private static void SaveGeneric<T>(string path, List<T> list)
    {
        try
        {
            string json = JsonSerializer.Serialize(list, _options);
            File.WriteAllText(path, json);
        }
        catch (Exception ex) { Console.WriteLine($"Lỗi lưu file {path}: {ex.Message}"); }
    }

    private static void LoadSanPham()
    {
        if (!File.Exists(FILE_SANPHAM)) return;
        try
        {
            string json = File.ReadAllText(FILE_SANPHAM);
            var list = JsonSerializer.Deserialize<List<SanPham>>(json);
            
            QuanLySanPham.dsTheoMa.Clear();
            QuanLySanPham.dsTheoTen.Clear();
            QuanLySanPham.dsTheoDanhMuc.Clear();

            if (list != null)
            {
                foreach (var sp in list)
                {
                    if (!QuanLySanPham.dsTheoMa.ContainsKey(sp.MaSP))
                        QuanLySanPham.dsTheoMa.Add(sp.MaSP, sp);

                    string tenKey = sp.TenSP.ToLowerInvariant();
                    if (!QuanLySanPham.dsTheoTen.ContainsKey(tenKey))
                        QuanLySanPham.dsTheoTen.Add(tenKey, new List<string>());
                    QuanLySanPham.dsTheoTen[tenKey].Add(sp.MaSP);

                    string dmKey = sp.DanhMuc ?? "Chưa phân loại";
                    if (!QuanLySanPham.dsTheoDanhMuc.ContainsKey(dmKey))
                        QuanLySanPham.dsTheoDanhMuc.Add(dmKey, new List<string>());
                    QuanLySanPham.dsTheoDanhMuc[dmKey].Add(sp.MaSP);
                }
            }
        }
        catch { Console.WriteLine("Lỗi đọc sản phẩm."); }
    }

    private static void SaveSanPham()
    {
        var list = QuanLySanPham.dsTheoMa.Values.ToList();
        SaveGeneric(FILE_SANPHAM, list);
    }

    private static void EnsureDefaultAdmin()
    {
        if (NguoiDungs.Count == 0)
        {
            NguoiDungs.Add(new NguoiDung("admin", "123456", "Quản Trị Viên", VaiTro.Admin));
            NguoiDungs.Add(new NguoiDung("thungan", "1", "Nhân Viên Thu Ngân", VaiTro.ThuNgan));
            NguoiDungs.Add(new NguoiDung("thukho", "1", "Nhân Viên Kho", VaiTro.ThuKho));
            SaveGeneric(FILE_NGUOIDUNG, NguoiDungs);
            Console.WriteLine("Đã tạo tài khoản mặc định: admin / 123456");
        }
    }
}