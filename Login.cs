using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
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
            public static string StaffID { get; set; }
           
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
                        LoginInfo.StaffID = row["IDSTAFF"].ToString();
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
        // Modern UI components using Guna UI
        private Guna2TextBox txtFullName;
        private Guna2TextBox txtEmail;
        private Guna2TextBox txtPhone;
        private Guna2TextBox txtAddress;
        private Guna2DateTimePicker dtpDateOfBirth;
        private Guna2TextBox txtPassword;
        private Guna2TextBox txtConfirmPassword;
        private Guna2Button btnRegister;
        private Guna2Button btnCancel;
        private Guna2Panel mainPanel;
        private Label lblValidationError;
        private Guna2Panel titlePanel;
        private Guna2ControlBox btnClose;
        private PictureBox pictureProfile;
        private Guna2ComboBox comboJoinDate;
        private Label lblTitle;

        // Labels for form fields
        private Label lblFullName;
        private Label lblEmail;
        private Label lblPhone;
        private Label lblAddress;
        private Label lblDateOfBirth;
        private Label lblJoinDate;
        private Label lblPassword;
        private Label lblConfirmPassword;

        public ReaderRegistration()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Reset validation message
                lblValidationError.Text = "";
                lblValidationError.Visible = false;

                // Validate input fields
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    ShowValidationError("Full name cannot be left blank.");
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
                {
                    ShowValidationError("Please enter a valid email address.");
                    txtEmail.Focus();
                    return;
                }

                // Check if email already exists - use proper parameters
                string checkEmailQuery = "SELECT COUNT(*) FROM READERS WHERE EMAIL = @email";
                SqlParameter emailParam = new SqlParameter("@email", txtEmail.Text);
                int emailCount = Convert.ToInt32(ExecuteScalarWithParams(checkEmailQuery, new SqlParameter[] { emailParam }));

                if (emailCount > 0)
                {
                    ShowValidationError("This email has already been registered. Please use a different email.");
                    txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    ShowValidationError("Phone number cannot be left blank.");
                    txtPhone.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    ShowValidationError("Address cannot be left blank.");
                    txtAddress.Focus();
                    return;
                }

                // Check if user is at least 6 years old
                DateTime minBirthDate = DateTime.Now.AddYears(-6);
                if (dtpDateOfBirth.Value > minBirthDate)
                {
                    ShowValidationError("The reader must be at least 6 years old.");
                    dtpDateOfBirth.Focus();
                    return;
                }

                // Validate password
                if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 6)
                {
                    ShowValidationError("Password must be at least 6 characters long.");
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    ShowValidationError("Passwords do not match.");
                    txtConfirmPassword.Focus();
                    return;
                }

                // All validations passed, insert new reader with proper SQL parameters
                string insertQuery = @"INSERT INTO READERS (FULLNAME, EMAIL, PHONE, [ADDRESS], DATEOFBIRTH, REGISTRATIONDATE, [PASSWORD]) 
                              VALUES (@fullname, @email, @phone, @address, @dateOfBirth, @registrationDate, @password)";

                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Add parameters properly to prevent SQL injection and syntax errors
                        command.Parameters.AddWithValue("@fullname", txtFullName.Text);
                        command.Parameters.AddWithValue("@email", txtEmail.Text);
                        command.Parameters.AddWithValue("@phone", txtPhone.Text);
                        command.Parameters.AddWithValue("@address", txtAddress.Text);
                        command.Parameters.AddWithValue("@dateOfBirth", dtpDateOfBirth.Value);
                        command.Parameters.AddWithValue("@registrationDate", DateTime.Now);
                        command.Parameters.AddWithValue("@password", txtPassword.Text);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Registration successful! You can log in using your email and password.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            ShowValidationError("Registration failed. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowValidationError("Error: " + ex.Message);
            }
        }
        private object ExecuteScalarWithParams(string query, SqlParameter[] parameters)
        {
            object result = null;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    result = command.ExecuteScalar();
                }
            }
            return result;
        }

        // Helper method to get connection string
        private string GetConnectionString()
        {
            return "Data Source=LAPTOP-T5G4R7PV\\SQLEXPRESS01;Initial Catalog=LIBRARYM;Integrated Security=True";
        }
        // Show validation error within the form instead of using MessageBox
        private void ShowValidationError(string message)
        {
            lblValidationError.Text = message;
            lblValidationError.Visible = true;
        }

        // Helper method to generate a reader ID card
        private void GenerateReaderID()
        {
            try
            {
                // Get the newly registered reader's information
                string query = "SELECT TOP 1 READERID, FULLNAME FROM READERS WHERE EMAIL = @email ORDER BY READERID DESC";
                object[] parameters = new object[] { txtEmail.Text };
                DataTable result = GetDatabase.Instance.ExecuteQuery(query, parameters);

                if (result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    string readerId = row["READERID"].ToString();
                    string readerName = row["FULLNAME"].ToString();

                    // You could implement a function here to generate and print a reader ID card
                    // For example:
                    // GenerateCodeHelper.GenerateReaderCard(readerId, readerName, txtEmail.Text);

                    // For now, just show a confirmation
                    MessageBox.Show($"Reader ID {readerId} has been created for {readerName}.\nPlease remember this ID for future reference.",
    "Reader ID Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while creating reader ID: " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {
            this.mainPanel = new Guna2Panel();
            this.titlePanel = new Guna2Panel();
            this.btnClose = new Guna2ControlBox();
            this.lblTitle = new Label();
            this.txtFullName = new Guna2TextBox();
            this.txtEmail = new Guna2TextBox();
            this.txtPhone = new Guna2TextBox();
            this.txtAddress = new Guna2TextBox();
            this.dtpDateOfBirth = new Guna2DateTimePicker();
            this.txtPassword = new Guna2TextBox();
            this.txtConfirmPassword = new Guna2TextBox();
            this.btnRegister = new Guna2Button();
            this.btnCancel = new Guna2Button();
            this.lblValidationError = new Label();
            this.pictureProfile = new PictureBox();
            this.comboJoinDate = new Guna2ComboBox();

            // Initialize labels
            this.lblFullName = new Label();
            this.lblEmail = new Label();
            this.lblPhone = new Label();
            this.lblAddress = new Label();
            this.lblDateOfBirth = new Label();
            this.lblJoinDate = new Label();
            this.lblPassword = new Label();
            this.lblConfirmPassword = new Label();

            // Main form setup
            this.Size = new Size(700, 620);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Register New Reader";

            // Main panel with rounded corners
            this.mainPanel.BorderRadius = 20;
            this.mainPanel.FillColor = Color.White;
            this.mainPanel.Size = new Size(700, 620);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.ShadowDecoration.Enabled = true;
            this.mainPanel.ShadowDecoration.Depth = 5;
            this.mainPanel.ShadowDecoration.Color = Color.Gray;
            this.mainPanel.Padding = new Padding(15);

            // Title panel
            this.titlePanel.Dock = DockStyle.Top;
            this.titlePanel.Height = 40;
            this.titlePanel.BackColor = Color.Transparent;
            this.titlePanel.FillColor = Color.Transparent;

            // Close button
            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.FillColor = Color.Transparent;
            this.btnClose.IconColor = Color.DimGray;
            this.btnClose.Size = new Size(30, 30);
            this.btnClose.Location = new Point(mainPanel.Width - 40, 10);
            this.btnClose.Click += (s, e) => this.Close();

            // Title
            this.lblTitle = new Label();
            this.lblTitle.Text = "Register New Reader";
            this.lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitle.Dock = DockStyle.Fill;

            // Create circular mask for profile picture if needed
            if (pictureProfile != null)
            {
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, pictureProfile.Width, pictureProfile.Height);
                pictureProfile.Region = new Region(path);
            }

            // Set fonts for all labels
            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font textBoxFont = new Font("Segoe UI", 11, FontStyle.Regular);

            // Define starting Y position
            int startY = 80;
            int fieldHeight = 40;
            int labelHeight = 20;
            int verticalGap = 15;
            int leftColX = 50;
            int rightColX = 350;
            int fieldWidth = 280;
            int fullWidthField = 580;

            // Full Name label and textbox
            this.lblFullName.Text = "Full name*";
            this.lblFullName.Font = labelFont;
            this.lblFullName.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblFullName.Location = new Point(leftColX, startY);
            this.lblFullName.Size = new Size(fieldWidth, labelHeight);

            this.txtFullName.Font = textBoxFont;
            this.txtFullName.BorderRadius = 10;
            this.txtFullName.Location = new Point(leftColX, startY + labelHeight);
            this.txtFullName.Size = new Size(fieldWidth, fieldHeight);
            this.txtFullName.BorderColor = Color.LightGray;
            this.txtFullName.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Email label and textbox
            this.lblEmail.Text = "Email*";
            this.lblEmail.Font = labelFont;
            this.lblEmail.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblEmail.Location = new Point(rightColX, startY);
            this.lblEmail.Size = new Size(fieldWidth, labelHeight);

            this.txtEmail.Font = textBoxFont;
            this.txtEmail.BorderRadius = 10;
            this.txtEmail.Location = new Point(rightColX, startY + labelHeight);
            this.txtEmail.Size = new Size(fieldWidth, fieldHeight);
            this.txtEmail.BorderColor = Color.LightGray;
            this.txtEmail.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Next row - Phone (moved to left column since Position was removed)
            int row2Y = startY + labelHeight + fieldHeight + verticalGap;

            // Phone label and textbox
            this.lblPhone.Text = "Phone number*";
            this.lblPhone.Font = labelFont;
            this.lblPhone.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblPhone.Location = new Point(leftColX, row2Y);
            this.lblPhone.Size = new Size(fieldWidth, labelHeight);

            this.txtPhone.Font = textBoxFont;
            this.txtPhone.BorderRadius = 10;
            this.txtPhone.Location = new Point(leftColX, row2Y + labelHeight);
            this.txtPhone.Size = new Size(fieldWidth, fieldHeight);
            this.txtPhone.BorderColor = Color.LightGray;
            this.txtPhone.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Join Date label and combo (moved to right column)
            this.lblJoinDate.Text = "Date join";
            this.lblJoinDate.Font = labelFont;
            this.lblJoinDate.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblJoinDate.Location = new Point(rightColX, row2Y);
            this.lblJoinDate.Size = new Size(fieldWidth, labelHeight);

            this.comboJoinDate.Font = textBoxFont;
            this.comboJoinDate.BorderRadius = 10;
            this.comboJoinDate.Location = new Point(rightColX, row2Y + labelHeight);
            this.comboJoinDate.Size = new Size(fieldWidth, fieldHeight);
            this.comboJoinDate.BorderColor = Color.LightGray;
            this.comboJoinDate.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Add current and past months to the combo box
            DateTime now = DateTime.Now;
            for (int i = 0; i < 12; i++)
            {
                DateTime date = now.AddMonths(-i);
                // Convert month name to English
                string monthName = GetMonthName(date.Month);
                comboJoinDate.Items.Add($"{monthName} {date.Year}");
            }
            comboJoinDate.SelectedIndex = 0;

            // Next row - Address (full width)
            int row3Y = row2Y + labelHeight + fieldHeight + verticalGap;

            // Address label and textbox
            this.lblAddress.Text = "Address*";
            this.lblAddress.Font = labelFont;
            this.lblAddress.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblAddress.Location = new Point(leftColX, row3Y);
            this.lblAddress.Size = new Size(fullWidthField, labelHeight);

            this.txtAddress.Font = textBoxFont;
            this.txtAddress.BorderRadius = 10;
            this.txtAddress.Location = new Point(leftColX, row3Y + labelHeight);
            this.txtAddress.Size = new Size(fullWidthField, fieldHeight);
            this.txtAddress.BorderColor = Color.LightGray;
            this.txtAddress.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Next row - Date of Birth and Password
            int row4Y = row3Y + labelHeight + fieldHeight + verticalGap;

            // Date of Birth label and picker
            this.lblDateOfBirth.Text = "Date of birth*";
            this.lblDateOfBirth.Font = labelFont;
            this.lblDateOfBirth.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblDateOfBirth.Location = new Point(leftColX, row4Y);
            this.lblDateOfBirth.Size = new Size(fieldWidth, labelHeight);

            this.dtpDateOfBirth.Font = textBoxFont;
            this.dtpDateOfBirth.BorderRadius = 10;
            this.dtpDateOfBirth.Location = new Point(leftColX, row4Y + labelHeight);
            this.dtpDateOfBirth.Size = new Size(fieldWidth, fieldHeight);
            this.dtpDateOfBirth.BorderColor = Color.LightGray;
            this.dtpDateOfBirth.Format = DateTimePickerFormat.Short;
            this.dtpDateOfBirth.Value = DateTime.Now.AddYears(-18);
            this.dtpDateOfBirth.FillColor = Color.White;

            // Password label and textbox
            this.lblPassword.Text = "Password* (At least 6 characters)";
            this.lblPassword.Font = labelFont;
            this.lblPassword.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblPassword.Location = new Point(rightColX, row4Y);
            this.lblPassword.Size = new Size(fieldWidth, labelHeight);

            this.txtPassword.Font = textBoxFont;
            this.txtPassword.BorderRadius = 10;
            this.txtPassword.Location = new Point(rightColX, row4Y + labelHeight);
            this.txtPassword.Size = new Size(fieldWidth, fieldHeight);
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.BorderColor = Color.LightGray;
            this.txtPassword.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Next row - Confirm Password
            int row5Y = row4Y + labelHeight + fieldHeight + verticalGap;

            // Confirm Password label and textbox (moved to left column)
            this.lblConfirmPassword.Text = "Confirm password*";
            this.lblConfirmPassword.Font = labelFont;
            this.lblConfirmPassword.ForeColor = Color.FromArgb(80, 53, 36);
            this.lblConfirmPassword.Location = new Point(leftColX, row5Y);
            this.lblConfirmPassword.Size = new Size(fieldWidth, labelHeight);

            this.txtConfirmPassword.Font = textBoxFont;
            this.txtConfirmPassword.BorderRadius = 10;
            this.txtConfirmPassword.Location = new Point(leftColX, row5Y + labelHeight);
            this.txtConfirmPassword.Size = new Size(fieldWidth, fieldHeight);
            this.txtConfirmPassword.PasswordChar = '•';
            this.txtConfirmPassword.BorderColor = Color.LightGray;
            this.txtConfirmPassword.FocusedState.BorderColor = Color.FromArgb(80, 53, 36);

            // Validation error label
            this.lblValidationError.AutoSize = false;
            this.lblValidationError.Size = new Size(fullWidthField, 40);
            this.lblValidationError.Location = new Point(leftColX, row5Y + labelHeight + fieldHeight + 10);
            this.lblValidationError.ForeColor = Color.Red;
            this.lblValidationError.TextAlign = ContentAlignment.MiddleCenter;
            this.lblValidationError.Visible = false;

            // Register and Cancel buttons
            int buttonsY = row5Y + labelHeight + fieldHeight + 50;

            // Register button
            this.btnRegister.Text = "Register";
            this.btnRegister.BorderRadius = 10;
            this.btnRegister.Size = new Size(fieldWidth, 45);
            this.btnRegister.Location = new Point(leftColX, buttonsY);
            this.btnRegister.FillColor = Color.FromArgb(80, 53, 36);
            this.btnRegister.ForeColor = Color.White;
            this.btnRegister.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnRegister.Click += new EventHandler(btnRegister_Click);

            // Cancel button
            this.btnCancel.Text = "Cancel";
            this.btnCancel.BorderRadius = 10;
            this.btnCancel.Size = new Size(fieldWidth, 45);
            this.btnCancel.Location = new Point(rightColX, buttonsY);
            this.btnCancel.FillColor = Color.Gray;
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            // Add controls to panels
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Controls.Add(this.btnClose);

            this.mainPanel.Controls.Add(this.titlePanel);
            if (pictureProfile != null) this.mainPanel.Controls.Add(this.pictureProfile);

            // Add labels first (so they're behind the textboxes)
            this.mainPanel.Controls.Add(this.lblFullName);
            this.mainPanel.Controls.Add(this.lblEmail);
            this.mainPanel.Controls.Add(this.lblPhone);
            this.mainPanel.Controls.Add(this.lblAddress);
            this.mainPanel.Controls.Add(this.lblDateOfBirth);
            this.mainPanel.Controls.Add(this.lblJoinDate);
            this.mainPanel.Controls.Add(this.lblPassword);
            this.mainPanel.Controls.Add(this.lblConfirmPassword);

            // Add input controls
            this.mainPanel.Controls.Add(this.txtFullName);
            this.mainPanel.Controls.Add(this.txtEmail);
            this.mainPanel.Controls.Add(this.txtPhone);
            this.mainPanel.Controls.Add(this.txtAddress);
            this.mainPanel.Controls.Add(this.dtpDateOfBirth);
            this.mainPanel.Controls.Add(this.comboJoinDate);
            this.mainPanel.Controls.Add(this.txtPassword);
            this.mainPanel.Controls.Add(this.txtConfirmPassword);
            this.mainPanel.Controls.Add(this.lblValidationError);
            this.mainPanel.Controls.Add(this.btnRegister);
            this.mainPanel.Controls.Add(this.btnCancel);

            // Add panel to form
            this.Controls.Add(this.mainPanel);

            // Make form draggable
            this.titlePanel.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(this.Handle, 0xA1, 0x2, 0);
                }
            };
        }

        // Helper for English month names
        private string GetMonthName(int month)
        {
            switch (month)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
                default: return $"Month {month}";
            }
        }

        // Helper class for form dragging
        private static class NativeMethods
        {
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HT_CAPTION = 0x2;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
        }
    }
}
