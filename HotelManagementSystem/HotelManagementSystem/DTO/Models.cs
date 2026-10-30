
namespace HotelManagementSystem.DTO
{
    public class KhachHangDTO
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; } = "";
        public string CCCD { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
    }

    public class PhongDTO
    {
        public string SoPhong { get; set; } = "";
        public string LoaiPhong { get; set; } = "";
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; } = "";
    }

    public class DichVuDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = "";
        public decimal DonGia { get; set; }
    }
}
