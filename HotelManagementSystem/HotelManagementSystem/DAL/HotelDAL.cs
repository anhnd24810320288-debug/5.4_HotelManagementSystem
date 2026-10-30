
using System;
using System.Data;
using MySql.Data.MySqlClient;
using HotelManagementSystem.DTO;

namespace HotelManagementSystem.DAL
{
    public class HotelDAL
    {
        public DataTable LayDanhSachPhong()
        {
            using var conn = Database.GetConnection();
            using var da = new MySqlDataAdapter(
                "SELECT * FROM Phong ORDER BY SoPhong", conn);

            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable LayDanhSachDichVu()
        {
            using var conn = Database.GetConnection();
            using var da = new MySqlDataAdapter(
                "SELECT * FROM DichVu ORDER BY TenDV", conn);

            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable LayPhieuDangO()
        {
            string sql = @"
                SELECT p.MaPhieu, p.SoPhong, k.HoTen, k.CCCD,
                       p.NgayNhanPhong
                FROM PhieuDatPhong p
                JOIN KhachHang k ON p.MaKH = k.MaKH
                WHERE p.NgayTraPhong IS NULL
                ORDER BY p.NgayNhanPhong DESC";

            using var conn = Database.GetConnection();
            using var da = new MySqlDataAdapter(sql, conn);

            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public void NhanPhong(
            string soPhong, string hoTen, string cccd, string sdt)
        {
            using var conn = Database.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                string trangThai;

                using (var cmd = new MySqlCommand(
                    "SELECT TrangThai FROM Phong WHERE SoPhong=@p FOR UPDATE",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", soPhong);
                    object? result = cmd.ExecuteScalar();

                    if (result == null)
                        throw new Exception("Không tìm thấy phòng.");

                    trangThai = result.ToString()!;
                }

                if (trangThai != "Trống")
                    throw new Exception("Phòng này hiện không trống.");

                int maKH;

                using (var cmd = new MySqlCommand(
                    "SELECT MaKH FROM KhachHang WHERE CCCD=@cccd",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@cccd", cccd);
                    object? result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        using var insertKH = new MySqlCommand(
                            @"INSERT INTO KhachHang(HoTen, CCCD, SoDienThoai)
                              VALUES(@ten,@cccd,@sdt);
                              SELECT LAST_INSERT_ID();", conn, tx);

                        insertKH.Parameters.AddWithValue("@ten", hoTen);
                        insertKH.Parameters.AddWithValue("@cccd", cccd);
                        insertKH.Parameters.AddWithValue("@sdt", sdt);
                        maKH = Convert.ToInt32(insertKH.ExecuteScalar());
                    }
                    else
                    {
                        maKH = Convert.ToInt32(result);

                        using var updateKH = new MySqlCommand(
                            @"UPDATE KhachHang
                              SET HoTen=@ten, SoDienThoai=@sdt
                              WHERE MaKH=@ma", conn, tx);

                        updateKH.Parameters.AddWithValue("@ten", hoTen);
                        updateKH.Parameters.AddWithValue("@sdt", sdt);
                        updateKH.Parameters.AddWithValue("@ma", maKH);
                        updateKH.ExecuteNonQuery();
                    }
                }

                using (var cmd = new MySqlCommand(
                    @"INSERT INTO PhieuDatPhong
                      (SoPhong, MaKH, NgayNhanPhong, TongTienThanhToan)
                      VALUES(@p,@kh,NOW(),0)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", soPhong);
                    cmd.Parameters.AddWithValue("@kh", maKH);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand(
                    "UPDATE Phong SET TrangThai='Đang ở' WHERE SoPhong=@p",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", soPhong);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void ThemDichVu(int maPhieu, int maDV, int soLuong)
        {
            if (soLuong <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            using var conn = Database.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                @"INSERT INTO SuDungDichVu(MaPhieu, MaDV, SoLuong, ThoiGian)
                  SELECT p.MaPhieu, @dv, @sl, NOW()
                  FROM PhieuDatPhong p
                  WHERE p.MaPhieu=@phieu AND p.NgayTraPhong IS NULL",
                conn);

            cmd.Parameters.AddWithValue("@dv", maDV);
            cmd.Parameters.AddWithValue("@sl", soLuong);
            cmd.Parameters.AddWithValue("@phieu", maPhieu);

            if (cmd.ExecuteNonQuery() == 0)
                throw new Exception("Phiếu không tồn tại hoặc đã thanh toán.");
        }

        public decimal ThanhToan(int maPhieu)
        {
            using var conn = Database.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                string soPhong;
                DateTime ngayNhan;
                decimal giaGio;

                using (var cmd = new MySqlCommand(
                    @"SELECT p.SoPhong, p.NgayNhanPhong, r.GiaTheoGio
                      FROM PhieuDatPhong p
                      JOIN Phong r ON p.SoPhong=r.SoPhong
                      WHERE p.MaPhieu=@ma AND p.NgayTraPhong IS NULL
                      FOR UPDATE", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@ma", maPhieu);
                    using var reader = cmd.ExecuteReader();

                    if (!reader.Read())
                        throw new Exception("Phiếu không tồn tại hoặc đã thanh toán.");

                    soPhong = reader.GetString("SoPhong");
                    ngayNhan = reader.GetDateTime("NgayNhanPhong");
                    giaGio = reader.GetDecimal("GiaTheoGio");
                }

                decimal tienPhong = (decimal)Math.Ceiling(
                    Math.Max(1, (DateTime.Now - ngayNhan).TotalHours)) * giaGio;

                decimal tienDichVu;

                using (var cmd = new MySqlCommand(
                    @"SELECT COALESCE(SUM(sd.SoLuong * dv.DonGia),0)
                      FROM SuDungDichVu sd
                      JOIN DichVu dv ON sd.MaDV=dv.MaDV
                      WHERE sd.MaPhieu=@ma", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@ma", maPhieu);
                    tienDichVu = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                decimal tongTien = tienPhong + tienDichVu;

                using (var cmd = new MySqlCommand(
                    @"UPDATE PhieuDatPhong
                      SET NgayTraPhong=NOW(), TongTienThanhToan=@tong
                      WHERE MaPhieu=@ma AND NgayTraPhong IS NULL",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@tong", tongTien);
                    cmd.Parameters.AddWithValue("@ma", maPhieu);

                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception("Phiếu đã được thanh toán.");
                }

                using (var cmd = new MySqlCommand(
                    "UPDATE Phong SET TrangThai='Đang dọn dẹp' WHERE SoPhong=@p",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", soPhong);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return tongTien;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void CapNhatTrangThaiPhong(string soPhong, string trangThai)
        {
            using var conn = Database.GetConnection();
            using var cmd = new MySqlCommand(
                "UPDATE Phong SET TrangThai=@tt WHERE SoPhong=@p", conn);

            cmd.Parameters.AddWithValue("@tt", trangThai);
            cmd.Parameters.AddWithValue("@p", soPhong);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
