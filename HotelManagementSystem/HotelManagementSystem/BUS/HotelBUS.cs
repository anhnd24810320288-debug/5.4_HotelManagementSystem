
using System;
using System.Data;
using HotelManagementSystem.DAL;

namespace HotelManagementSystem.BUS
{
    public class HotelBUS
    {
        private readonly HotelDAL dal = new HotelDAL();

        public DataTable LayDanhSachPhong()
        {
            return dal.LayDanhSachPhong();
        }

        public DataTable LayDanhSachDichVu()
        {
            return dal.LayDanhSachDichVu();
        }

        public DataTable LayPhieuDangO()
        {
            return dal.LayPhieuDangO();
        }

        public DataView LocPhong(DataTable dt, string loai, string trangThai)
        {
            DataView view = new DataView(dt);
            string filter = "";

            if (!string.IsNullOrWhiteSpace(loai) && loai != "Tất cả")
                filter += "LoaiPhong = '" + loai.Replace("'", "''") + "'";

            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
            {
                if (filter.Length > 0) filter += " AND ";
                filter += "TrangThai = '" + trangThai.Replace("'", "''") + "'";
            }

            view.RowFilter = filter;
            return view;
        }

        public void NhanPhong(
            string soPhong, string hoTen, string cccd, string sdt)
        {
            if (string.IsNullOrWhiteSpace(soPhong) ||
                string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(cccd))
                throw new Exception("Vui lòng nhập phòng, họ tên và CCCD.");

            dal.NhanPhong(soPhong, hoTen.Trim(), cccd.Trim(), sdt.Trim());
        }

        public void ThemDichVu(int maPhieu, int maDV, int soLuong)
        {
            if (maPhieu <= 0 || maDV <= 0 || soLuong <= 0)
                throw new Exception("Mã phiếu, dịch vụ và số lượng không hợp lệ.");

            dal.ThemDichVu(maPhieu, maDV, soLuong);
        }

        public decimal ThanhToan(int maPhieu)
        {
            if (maPhieu <= 0)
                throw new Exception("Mã phiếu không hợp lệ.");

            return dal.ThanhToan(maPhieu);
        }

        public void CapNhatTrangThaiPhong(string soPhong, string trangThai)
        {
            dal.CapNhatTrangThaiPhong(soPhong, trangThai);
        }
    }
}
