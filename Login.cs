using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LMS.Database;
using LMS.Generate_Code;

namespace LMS
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            int x = (this.ClientSize.Width - panelLogin.Width) / 2;
            int y = (this.ClientSize.Height - panelLogin.Height) / 2;
            panelLogin.Location = new Point(x, y);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 450);
        }
        public static class LoginInfo
        {
            public static DateTime LoginTime { get; set; }
            public static string EmployeeName { get; set; }
            public static string EmployeeID { get; set; }
            public static SqlDbType StaffID { get; internal set; }
        }
        private void btnMaxSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnMaxSize.Visible = false;
            btnMinSize.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Do you really want to exit?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Exit cancelled. Continue your activity ❤️.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Focus();
            }
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            if (txtPass.PasswordChar == '\u25CF')
            {
                txtPass.PasswordChar = '\0';
                btnHide.Visible = false;
                btnEye.Visible = true;
            }
        }

        private void btnEye_Click(object sender, EventArgs e)
        {
            if (txtPass.PasswordChar == '\0')
            {
                txtPass.PasswordChar = '\u25CF';
                btnEye.Visible = false;
                btnHide.Visible = true;
            }
        }

        private void btnMinSize_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            // Tùy chỉnh lại vị trí nếu cần (giữ nguyên vị trí hiện tại)
            this.Location = new Point(0, 0);
            btnMinSize.Visible = false;
            btnMaxSize.Visible = true;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtUsername.Text;
            string password = txtPass.Text;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string role = LginLgout.Instance.LgManage(email, password);
                if (role != null)
                {
                    // Lưu thời gian đăng nhập
                    LoginInfo.LoginTime = DateTime.Now;

                    // Lấy thông tin nhân viên từ database sử dụng GetDatabase
                    string query = "SELECT IDSTAFF, FULLNAME FROM LIBRARIANS WHERE EMAIL = @email AND PASSWORD = @password";
                    object[] parameters = new object[] { email, password };
                    DataTable result = GetDatabase.Instance.ExecuteQuery(query, parameters);

                    if (result.Rows.Count > 0)
                    {
                        DataRow row = result.Rows[0];
                        LoginInfo.EmployeeID = row["IDSTAFF"].ToString();
                        LoginInfo.EmployeeName = row["FULLNAME"].ToString();
                    }
                    query = "SELECT IDSTAFF FROM LIBRARIANS where EMAIL = '" + email + "'";
                    string idstaff = "";
                    DataTable data = GetDatabase.Instance.ExecuteQuery(query);
                    foreach (DataRow row in data.Rows)
                    {
                        idstaff = row["IDSTAFF"].ToString();
                        break;
                    }
                    Home f = new Home(role, idstaff);
                    this.Hide();
                    f.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please enter a valid email or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter a valid email or password." + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
