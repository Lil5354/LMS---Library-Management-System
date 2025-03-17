using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMS.Generate_Code
{
    public partial class AddBorrower : UserControl
    {
        public AddBorrower()
        {
            InitializeComponent();
        }

        private void guna2ComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
        private void btnRegister_Click(object sender, EventArgs e)
        {
            AddBorrower2 listBookControl = new AddBorrower2();
            AddUserControl(listBookControl);
        }
    }
}
