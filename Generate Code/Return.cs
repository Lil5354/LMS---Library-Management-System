using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LMS.Proccess;

namespace LMS.Generate_Code
{
    public partial class Return : UserControl
    {
        public Return()
        {
            InitializeComponent();
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
        private void Return_Load(object sender, EventArgs e)
        {
            string query = @"
            SELECT 
                R.READERID,
                R.FULLNAME AS [Reader Name],
                R.EMAIL,
                R.PHONE,
                R.[ADDRESS],
                R.DATEOFBIRTH,
                R.REGISTRATIONDATE,
                COUNT(BT.TICKETID) AS [Number of Borrowed Books]
            FROM 
                READERS R
                INNER JOIN BORROWINGTICKETS BT ON R.READERID = BT.READERID AND BT.[STATUS] = 'Borrowing'
            GROUP BY 
                R.READERID, R.FULLNAME, R.EMAIL, R.PHONE, R.[ADDRESS], R.DATEOFBIRTH, R.REGISTRATIONDATE;";
            DTLibrary.Instance.LoadList(query, dtgvReaders);
        }
        private void btnReload_Click(object sender, EventArgs e)
        {
            Return_Load(sender, e);
        }

        private void txtSearchReader_KeyUp(object sender, KeyEventArgs e)
        {
            string searchText = txtSearchReader.Text.Trim(); // Lấy giá trị từ ô tìm kiếm và loại bỏ khoảng trắng thừa

            string query = $@"
                SELECT 
                R.READERID AS ID,
                R.FULLNAME AS [READER NAME],
                R.EMAIL,
                R.PHONE,
                R.[ADDRESS],
                R.DATEOFBIRTH,
                R.REGISTRATIONDATE [REGISTER DATE],
                COUNT(BT.TICKETID) AS [Number of Borrowed Books]
            FROM 
                READERS R
                INNER JOIN BORROWINGTICKETS BT ON R.READERID = BT.READERID AND BT.[STATUS] = 'Borrowing'
            WHERE R.FULLNAME LIKE N'%{searchText.Replace("'", "''")}%'
            GROUP BY 
                R.READERID, R.FULLNAME, R.EMAIL, R.PHONE, R.[ADDRESS], R.DATEOFBIRTH, R.REGISTRATIONDATE";
            DTLibrary.Instance.LoadList(query, dtgvReaders);
        }

        private void dtgvReaders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Lấy thông tin của độc giả được chọn
                DataGridViewRow row = dtgvReaders.Rows[e.RowIndex];
                string readerID = row.Cells[0].Value.ToString();
                string readerName = row.Cells[1].Value.ToString();

                // Mở form Return2 với thông tin của độc giả
                Return2 listBookControl = new Return2(readerID,readerName);
                AddUserControl(listBookControl);
            }
        }

        private void txtSearchReader_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
