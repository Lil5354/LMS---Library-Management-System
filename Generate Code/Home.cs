using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace LMS.Generate_Code
{
    public partial class Home : Form
    {
        public Home(string role, string idstaff)
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 450);
            SetupClock();
        }
        bool menuExpand = false;
        private void menuTransition_Tick(object sender, EventArgs e)
        {
            if(menuExpand == false)
            {
                menuContainer.Height += 10;
                if(menuContainer.Height >= 255)
                {
                    menuTransition.Stop();
                    menuExpand = true;
                }
            }
            else
            {
                menuContainer.Height -= 10;
                if(menuContainer.Height <= 65)
                {
                    menuTransition.Stop();
                    menuExpand = false;
                }
            }
        }
        private void menu_Click(object sender, EventArgs e)
        {
            menuTransition.Start();
        }
        bool slidebarExpand = true;
        private void slidebarTrans_Tick(object sender, EventArgs e)
        {
            if (slidebarExpand)
            {
                slidebar.Width -= 10;
                if(slidebar.Width <= 52)
                {
                    slidebarExpand = false;
                    slidebarTrans.Stop();
                }
            }
            else
            {
                slidebar.Width += 10;
                if (slidebar.Width >= 204)
                {
                    slidebarExpand = true;
                    slidebarTrans.Stop();
                }
            }
        }
        private void btnHam_Click(object sender, EventArgs e)
        {
            slidebarTrans.Start();
        }
        private void borrow_Click(object sender, EventArgs e)
        {
            BorrowTrans.Start();
        }
        private void BorrowTrans_Tick(object sender, EventArgs e)
        {
            if (menuExpand == false)
            {
                BorrowContainer.Height += 10;
                if (BorrowContainer.Height >= 375)
                {
                    BorrowTrans.Stop();
                    menuExpand = true;
                }
            }
            else
            {
                BorrowContainer.Height -= 10;
                if (BorrowContainer.Height <= 65)
                {
                    BorrowTrans.Stop();
                    menuExpand = false;
                }
            }
        }
        private void ManageTrans_Tick(object sender, EventArgs e)
        {
            if (menuExpand == false)
            {
                ManageContainer.Height += 10;
                if (ManageContainer.Height >= 130)
                {
                    ManageTrans.Stop();
                    menuExpand = true;
                }
            }
            else
            {
                ManageContainer.Height -= 10;
                if (ManageContainer.Height <= 65)
                {
                    ManageTrans.Stop();
                    menuExpand = false;
                }
            }
        }
        private void Manage_Click(object sender, EventArgs e)
        {
            ManageTrans.Start();
        }
        private void btnMinSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            // Tùy chỉnh lại vị trí nếu cần (giữ nguyên vị trí hiện tại)
            this.Location = new Point(0, 0);
            btnMinSize.Visible = false;
            btnMaxSize.Visible = true;
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
        private UserControl currentUserControl;
        public void AddUserControl(UserControl userControl)
        {
            if (currentUserControl != null)
            {
                this.Controls.Remove(currentUserControl);
                currentUserControl.Dispose(); // Đảm bảo giải phóng tài nguyên đúng cách
            }
            // Các cải tiến:
            userControl.SuspendLayout(); // Tạm dừng layout để giảm thiểu việc vẽ lại
            this.Controls.Add(userControl);
            userControl.Dock = DockStyle.Fill;
            userControl.BringToFront();
            userControl.ResumeLayout(true); // Khôi phục layout một cách hiệu quả

            currentUserControl = userControl;
        }
        private void btnListBook_Click(object sender, EventArgs e)
        {
            BookList listBookControl = new BookList();
            AddUserControl(listBookControl);
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddBook listBookControl = new AddBook();
            AddUserControl(listBookControl);
        }
        private void btnRes_Click(object sender, EventArgs e)
        {
            Restore listBookControl = new Restore();
            AddUserControl(listBookControl);
        }
        private void btnAddBorrowers_Click(object sender, EventArgs e)
        {
            AddBorrower listBookControl = new AddBorrower();
            AddUserControl(listBookControl);
        }
        private void btnBrHis_Click(object sender, EventArgs e)
        {
            History listBookControl = new History();
            AddUserControl(listBookControl);
        }
        private void btnOverDue_Click(object sender, EventArgs e)
        {
            OverDueList listBookControl = new OverDueList();
            AddUserControl(listBookControl);
        }
        private void btnReturn_Click(object sender, EventArgs e)
        {
            Return listBookControl = new Return();
            AddUserControl(listBookControl);
        }
        private void btnEmployee_Click(object sender, EventArgs e)
        {
            ListEmployee listBookControl = new ListEmployee();
            AddUserControl(listBookControl);
        }
        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login frmLogin = new Login();
            frmLogin.Show();
        }
        private void btnHomeDashBoard_Click(object sender, EventArgs e)
        {
            List<Control> controlsToRemove = new List<Control>();
            foreach (Control control in this.Controls)
            {
                if (control is UserControl)
                {
                    controlsToRemove.Add(control);
                }
            }
            foreach (Control control in controlsToRemove)
            {
                this.Controls.Remove(control);
                control.Dispose();
            }
            currentUserControl = null;
        }

        private void btnStatistic_Click(object sender, EventArgs e)
        {
            Statistic listBookControl = new Statistic();
            AddUserControl(listBookControl);
        }

        private void lblTime_Click(object sender, EventArgs e)
        {

        }
        private void SetupClock()
        {
            // Initialize the timer
            timerClock = new System.Windows.Forms.Timer();
            timerClock.Interval = 1000; // Update every second
            timerClock.Tick += TimerClock_Tick;

            // Start the timer
            timerClock.Start();

            // Update time immediately (don't wait for first tick)
            UpdateTimeDisplay();
        }
        private void UpdateTimeDisplay()
        {
            // Get current time
            DateTime now = DateTime.Now;

            // Update the label with formatted time
            lblTime.Text = now.ToString("HH:mm"); // 24-hour format
                                                  // Or use this for 12-hour format with AM/PM
                                                  // lblTime.Text = now.ToString("hh:mm:ss tt");
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (timerClock != null)
            {
                timerClock.Stop();
                timerClock.Dispose();
            }

            base.OnFormClosed(e);
        }
        private void TimerClock_Tick(object sender, EventArgs e)
        {
            // This will be called every second (1000ms)
            UpdateTimeDisplay();
        }
        private void timerClock_Tick(object sender, EventArgs e)
        {

        }
    }
}
