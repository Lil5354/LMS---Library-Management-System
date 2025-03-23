using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LMS.borrower;
using LMS.Database;
using LMS.Generate_Code;
using LMS.Proccess;

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
            public static string ReaderID { get; set; }
            public static string ReaderName { get; set; }
            public static string UserType { get; set; } // "Librarian" or "Reader"
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
                // First, try to authenticate as librarian
                string librarianRole = LginLgout.Instance.LgManage(email, password);

                if (librarianRole != null)
                {
                    // Librarian login successful
                    // Save login time
                    LoginInfo.LoginTime = DateTime.Now;

                    // Get librarian information from database
                    string query = "SELECT IDSTAFF, FULLNAME FROM LIBRARIANS WHERE EMAIL = @email AND PASSWORD = @password";
                    object[] parameters = new object[] { email, password };
                    DataTable result = GetDatabase.Instance.ExecuteQuery(query, parameters);

                    if (result.Rows.Count > 0)
                    {
                        DataRow row = result.Rows[0];
                        LoginInfo.EmployeeID = row["IDSTAFF"].ToString();
                        LoginInfo.EmployeeName = row["FULLNAME"].ToString();
                        LoginInfo.UserType = "Librarian";
                    }

                    string idstaff = "";
                    query = "SELECT IDSTAFF FROM LIBRARIANS where EMAIL = '" + email + "'";
                    DataTable data = GetDatabase.Instance.ExecuteQuery(query);
                    foreach (DataRow row in data.Rows)
                    {
                        idstaff = row["IDSTAFF"].ToString();
                        break;
                    }

                    Home f = new Home(librarianRole, idstaff);
                    this.Hide();
                    f.ShowDialog();
                    this.Show();
                }
                else
                {
                    // If not a librarian, try reader authentication
                    string query = "SELECT READERID, FULLNAME FROM READERS WHERE EMAIL = @email AND PASSWORD = @password";
                    object[] parameters = new object[] { email, password };
                    DataTable result = GetDatabase.Instance.ExecuteQuery(query, parameters);

                    if (result.Rows.Count > 0)
                    {
                        // Reader login successful
                        DataRow row = result.Rows[0];
                        LoginInfo.ReaderID = row["READERID"].ToString();
                        LoginInfo.ReaderName = row["FULLNAME"].ToString();
                        LoginInfo.UserType = "Reader";
                        LoginInfo.LoginTime = DateTime.Now;

                        // Open reader dashboard
                        HomeBR readerDashboard = new HomeBR(LoginInfo.ReaderID);
                        this.Hide();
                        readerDashboard.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // You can implement reader registration functionality here
            try
            {
                ReaderRegistration registrationForm = new ReaderRegistration();
                registrationForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening registration form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Sample class for Reader Registration (you'll need to implement this)
    public class ReaderRegistration : Form
    {
        // Registration form implementation
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private DateTimePicker dtpDateOfBirth;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;

        public ReaderRegistration()
        {
            InitializeComponent();
            this.Text = "Reader Registration";
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(400, 500);
        }

        private void InitializeComponent()
        {
            // Initialize form components
            this.txtFullName = new TextBox();
            this.txtEmail = new TextBox();
            this.txtPhone = new TextBox();
            this.txtAddress = new TextBox();
            this.dtpDateOfBirth = new DateTimePicker();
            this.txtPassword = new TextBox();
            this.txtConfirmPassword = new TextBox();
            this.btnRegister = new Button();
            this.btnCancel = new Button();

            // Set up layout and properties
            // (Layout code would go here)

            // Set up event handlers
            this.btnRegister.Click += new EventHandler(btnRegister_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    
        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check if email already exists
                string query = "SELECT COUNT(*) FROM READERS WHERE EMAIL = @email";
                object[] parameters = new object[] { txtEmail.Text };
                int count = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(query, parameters));

                if (count > 0)
                {
                    MessageBox.Show("This email is already registered. Please use a different email.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Insert new reader
                query = @"INSERT INTO READERS (FULLNAME, EMAIL, PHONE, ADDRESS, DATEOFBIRTH, PASSWORD)
                          VALUES (@fullname, @email, @phone, @address, @dob, @password)";

                parameters = new object[] {
                    txtFullName.Text,
                    txtEmail.Text,
                    txtPhone.Text,
                    txtAddress.Text,
                    dtpDateOfBirth.Value.Date,
                    txtPassword.Text
                };

                int result = GetDatabase.Instance.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    MessageBox.Show("Registration successful! You can now login with your email and password.", "Registration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
