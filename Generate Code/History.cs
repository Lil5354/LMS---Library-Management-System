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
    public partial class History : UserControl
    {
        string query = "";
        public History()
        {
            InitializeComponent();
            LoadReadersWithBorrowedBooks();
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
        private void LoadReadersWithBorrowedBooks()
        {
            // Query lấy danh sách độc giả và tổng số sách đã mượn
            string query = @"
            SELECT 
                R.READERID AS [ID],
                R.FULLNAME AS [READER NAME],
                R.EMAIL,
                R.PHONE,
                R.[ADDRESS],
                R.DATEOFBIRTH,
                R.REGISTRATIONDATE,
                COUNT(BT.TICKETID) AS [TOTAL OF BORROWED BOOKS]
            FROM 
                READERS R
                LEFT JOIN BORROWINGTICKETS BT ON R.READERID = BT.READERID
            GROUP BY 
                R.READERID, R.FULLNAME, R.EMAIL, R.PHONE, R.[ADDRESS], R.DATEOFBIRTH, R.REGISTRATIONDATE
            ORDER BY 
                COUNT(BT.TICKETID) DESC";

            DTLibrary.Instance.LoadList(query,dtgvReaders);
        }

        private void dtgvReaders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string readerId = dtgvReaders.Rows[e.RowIndex].Cells[0].Value.ToString();
                string readerName = dtgvReaders.Rows[e.RowIndex].Cells[1].Value.ToString();

                // Mở form chi tiết
                IndividualHis listBookControl = new IndividualHis(readerId, readerName);
                AddUserControl(listBookControl);
            }
        }

        private void txtSearchReader_KeyUp(object sender, KeyEventArgs e)
        {
            string searchText = txtSearchReader.Text.Trim(); // Lấy giá trị từ ô tìm kiếm và loại bỏ khoảng trắng thừa

            string query = $@"
                SELECT 
                R.READERID AS [ID],
                R.FULLNAME AS [READER NAME],
                R.EMAIL,
                R.PHONE,
                R.[ADDRESS],
                R.DATEOFBIRTH,
                R.REGISTRATIONDATE,
                COUNT(BT.TICKETID) AS [TOTAL OF BORROWED BOOKS]
            FROM 
                READERS R
                LEFT JOIN BORROWINGTICKETS BT ON R.READERID = BT.READERID
            WHERE 
                R.FULLNAME LIKE N'%{searchText.Replace("'", "''")}%'
            GROUP BY 
                R.READERID, R.FULLNAME, R.EMAIL, R.PHONE, R.[ADDRESS], R.DATEOFBIRTH, R.REGISTRATIONDATE
            ORDER BY 
                COUNT(BT.TICKETID) DESC";
     
            DTLibrary.Instance.LoadList(query, dtgvReaders);
        }
    }
}
