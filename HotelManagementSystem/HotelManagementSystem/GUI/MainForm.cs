
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelManagementSystem.BUS;

namespace HotelManagementSystem.GUI
{
    public class MainForm : Form
    {
        private readonly HotelBUS bus = new HotelBUS();

        private DataTable dtPhong = new DataTable();
        private DataGridView dgvPhong = new DataGridView();
        private DataGridView dgvPhieu = new DataGridView();

        private ComboBox cboPhong = new ComboBox();
        private ComboBox cboDichVu = new ComboBox();
        private ComboBox cboLoai = new ComboBox();
        private ComboBox cboTrangThai = new ComboBox();

        private TextBox txtHoTen = new TextBox();
        private TextBox txtCCCD = new TextBox();
        private TextBox txtSDT = new TextBox();
        private TextBox txtMaPhieuDV = new TextBox();
        private TextBox txtSoLuong = new TextBox();
        private TextBox txtMaPhieuTT = new TextBox();

        public MainForm()
        {
            Text = "Hotel Management System";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 700);
            MinimumSize = new Size(950, 600);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var tabPhong = new TabPage("Quản lý phòng");
            var tabNhan = new TabPage("Nhận phòng");
            var tabDichVu = new TabPage("Dịch vụ và thanh toán");

            tabs.TabPages.AddRange(new[] { tabPhong, tabNhan, tabDichVu });
            Controls.Add(tabs);

            TaoTabPhong(tabPhong);
            TaoTabNhanPhong(tabNhan);
            TaoTabDichVu(tabDichVu);

            Load += (s, e) => TaiDuLieu();
        }

        private Label TaoLabel(
            string text, int x, int y, Control parent, int width = 90)
        {
            var label = new Label
            {
                Text = text,
                Left = x,
                Top = y + 3,
                Width = width,
                Height = 24,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            parent.Controls.Add(label);
            return label;
        }

        private TextBox TaoTextBox(
            int x, int y, Control parent, int width = 200)
        {
            var txt = new TextBox
            {
                Left = x,
                Top = y,
                Width = width,
                Height = 27,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            parent.Controls.Add(txt);
            return txt;
        }

        private Button TaoButton(
            string text, int x, int y, Control parent,
            Action action, int width = 150)
        {
            var btn = new Button
            {
                Text = text,
                Left = x,
                Top = y,
                Width = width,
                Height = 32
            };

            btn.Click += (s, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };

            parent.Controls.Add(btn);
            return btn;
        }

        private DataGridView TaoGrid(Control parent, int top)
        {
            var dgv = new DataGridView
            {
                Left = 15,
                Top = top,
                Width = parent.ClientSize.Width - 30,
                Height = Math.Max(200, parent.ClientSize.Height - top - 20),
                Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left |
                         AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ScrollBars = ScrollBars.Both
            };

            parent.Controls.Add(dgv);
            return dgv;
        }

        private ComboBox TaoCombo(
            int x, int y, Control parent, int width = 200)
        {
            var cbo = new ComboBox
            {
                Left = x,
                Top = y,
                Width = width,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            parent.Controls.Add(cbo);
            return cbo;
        }

        private void TaoTabPhong(TabPage tab)
        {
            TaoLabel("Loại phòng:", 15, 15, tab, 100);
            cboLoai = TaoCombo(120, 15, tab, 180);
            cboLoai.Items.AddRange(
                new object[] { "Tất cả", "Đơn", "Đôi", "VIP" });
            cboLoai.SelectedIndex = 0;

            TaoLabel("Trạng thái:", 330, 15, tab, 100);
            cboTrangThai = TaoCombo(435, 15, tab, 180);
            cboTrangThai.Items.AddRange(
                new object[]
                {
                    "Tất cả", "Trống", "Đang ở", "Đang dọn dẹp"
                });
            cboTrangThai.SelectedIndex = 0;

            TaoButton("Lọc phòng", 640, 12, tab, LocPhong, 120);
            TaoButton("Tải lại", 775, 12, tab, TaiDuLieu, 120);

            dgvPhong = TaoGrid(tab, 60);

            TaoButton("Đánh dấu phòng đã dọn", 15, 475, tab, () =>
            {
                if (dgvPhong.CurrentRow == null)
                    throw new Exception("Hãy chọn một phòng.");

                string soPhong = dgvPhong.CurrentRow
                    .Cells["SoPhong"].Value!.ToString()!;

                bus.CapNhatTrangThaiPhong(soPhong, "Trống");
                TaiDuLieu();

                MessageBox.Show("Đã cập nhật phòng về trạng thái Trống.");
            }, 220);
        }

        private void TaoTabNhanPhong(TabPage tab)
        {
            TaoLabel("Chọn phòng:", 20, 25, tab, 120);
            cboPhong = TaoCombo(150, 22, tab, 250);

            TaoLabel("Họ tên khách:", 20, 75, tab, 120);
            txtHoTen = TaoTextBox(150, 72, tab, 250);

            TaoLabel("CCCD:", 20, 125, tab, 120);
            txtCCCD = TaoTextBox(150, 122, tab, 250);

            TaoLabel("Số điện thoại:", 20, 175, tab, 120);
            txtSDT = TaoTextBox(150, 172, tab, 250);

            TaoButton("Xác nhận nhận phòng", 150, 220, tab, () =>
            {
                if (cboPhong.SelectedValue == null)
                    throw new Exception("Không có phòng trống để chọn.");

                string soPhong = cboPhong.SelectedValue.ToString()!;

                bus.NhanPhong(
                    soPhong,
                    txtHoTen.Text,
                    txtCCCD.Text,
                    txtSDT.Text);

                MessageBox.Show("Nhận phòng thành công.");

                txtHoTen.Clear();
                txtCCCD.Clear();
                txtSDT.Clear();

                TaiDuLieu();
            }, 200);

            dgvPhieu = TaoGrid(tab, 280);
        }

        private void TaoTabDichVu(TabPage tab)
        {
            tab.AutoScroll = true;

            var grpDichVu = new GroupBox
            {
                Text = "Thêm dịch vụ",
                Left = 15,
                Top = 15,
                Width = 600,
                Height = 145,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            tab.Controls.Add(grpDichVu);

            TaoLabel("Mã phiếu:", 15, 30, grpDichVu, 90);
            txtMaPhieuDV = TaoTextBox(115, 29, grpDichVu, 120);

            TaoLabel("Dịch vụ:", 250, 30, grpDichVu, 80);
            cboDichVu = TaoCombo(340, 29, grpDichVu, 230);
            cboDichVu.DropDownWidth = 230;

            TaoLabel("Số lượng:", 15, 80, grpDichVu, 90);
            txtSoLuong = TaoTextBox(115, 79, grpDichVu, 120);
            txtSoLuong.Text = "1";

            TaoButton("Thêm dịch vụ", 340, 76, grpDichVu, () =>
            {
                if (!int.TryParse(txtMaPhieuDV.Text, out int maPhieu))
                    throw new Exception("Mã phiếu phải là số hợp lệ.");

                if (cboDichVu.SelectedValue == null)
                    throw new Exception("Vui lòng chọn dịch vụ.");

                if (!int.TryParse(txtSoLuong.Text, out int soLuong))
                    throw new Exception("Số lượng phải là số nguyên.");

                bus.ThemDichVu(
                    maPhieu,
                    Convert.ToInt32(cboDichVu.SelectedValue),
                    soLuong);

                MessageBox.Show("Thêm dịch vụ thành công.");
            }, 150);

            var grpThanhToan = new GroupBox
            {
                Text = "Thanh toán",
                Left = 635,
                Top = 15,
                Width = 390,
                Height = 145,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            tab.Controls.Add(grpThanhToan);

            TaoLabel("Mã phiếu:", 15, 35, grpThanhToan, 90);
            txtMaPhieuTT = TaoTextBox(115, 32, grpThanhToan, 120);

            TaoButton("Thanh toán", 115, 80, grpThanhToan, () =>
            {
                if (!int.TryParse(txtMaPhieuTT.Text, out int maPhieu))
                    throw new Exception("Mã phiếu thanh toán không hợp lệ.");

                decimal tong = bus.ThanhToan(maPhieu);

                MessageBox.Show(
                    "Thanh toán thành công!\nTổng tiền: " +
                    tong.ToString("N0") + " VNĐ");

                TaiDuLieu();
            }, 150);

            var dgv = TaoGrid(tab, 180);
            dgv.Anchor = AnchorStyles.Top |
                         AnchorStyles.Bottom |
                         AnchorStyles.Left |
                         AnchorStyles.Right;

            void ChinhKichThuocBang()
            {
                dgv.Left = 15;
                dgv.Top = 180;
                dgv.Width = Math.Max(400, tab.ClientSize.Width - 45);
                dgv.Height = Math.Max(180, tab.ClientSize.Height - 205);
            }

            tab.Resize += (s, e) => ChinhKichThuocBang();
            ChinhKichThuocBang();

            dgv.DataSource = bus.LayPhieuDangO();
        }

        private void TaiDuLieu()
        {
            dtPhong = bus.LayDanhSachPhong();
            dgvPhong.DataSource = dtPhong;

            DataTable phongTrong = dtPhong.Clone();

            foreach (DataRow row in dtPhong.Rows)
            {
                if (row["TrangThai"].ToString() == "Trống")
                    phongTrong.ImportRow(row);
            }

            cboPhong.DataSource = null;
            cboPhong.DisplayMember = "SoPhong";
            cboPhong.ValueMember = "SoPhong";
            cboPhong.DataSource = phongTrong;

            cboDichVu.DataSource = bus.LayDanhSachDichVu();
            cboDichVu.DisplayMember = "TenDV";
            cboDichVu.ValueMember = "MaDV";

            dgvPhieu.DataSource = bus.LayPhieuDangO();

            LocPhong();
        }

        private void LocPhong()
        {
            if (dtPhong.Columns.Count == 0)
                return;

            dgvPhong.DataSource = bus.LocPhong(
                dtPhong,
                cboLoai.Text,
                cboTrangThai.Text);
        }
    }
}
