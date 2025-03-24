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
using static LMS.Login;

namespace LMS.Generate_Code
{
    public partial class Return2 : UserControl
    {
        private string readerID;
        private string readerName;
        public Return2(string readerID, string readerName)
        {
            InitializeComponent();
            this.readerName = readerName; 
            this.readerID = readerID;
            lblReaderName.Text = readerName;
            LoadBorrowedBooks();
        }
        private void LoadBorrowedBooks()
        {
            // Query để lấy danh sách sách đang mượn của độc giả
            string query = $@"
           SELECT 
            B.TITLE AS [TITLE],
            R.FULLNAME AS [READER NAME],
            BT.BORROWDATE AS [BORROW DATE],
            BT.DUEDATE AS [DUE DATE],
            DATEDIFF(day, BT.DUEDATE, GETDATE()) AS [DAY OVERDUE],
            CASE 
                WHEN DATEDIFF(day, BT.DUEDATE, GETDATE()) > 0 
                THEN DATEDIFF(day, BT.DUEDATE, GETDATE()) * 5000 
                ELSE 0 
            END AS [FINE AMOUNT]
        FROM 
            BORROWINGTICKETS BT
            INNER JOIN BOOKS B ON BT.BOOKID = B.BOOKID
            INNER JOIN READERS R ON BT.READERID = R.READERID
        WHERE 
                BT.READERID = '{readerID}' AND BT.[STATUS] = 'Borrowing';";
            DTLibrary.Instance.LoadList(query, dtgv);
        }

        private void Return2_Load(object sender, EventArgs e)
        {
            LoadBorrowedBooks();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (dtgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select at least one book to return.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tính tổng số tiền phí
            decimal totalFineAmount = 0;
            foreach (DataGridViewRow row in dtgv.SelectedRows)
            {
                DateTime dueDate = Convert.ToDateTime(row.Cells["DUE DATE"].Value);
                int daysOverdue = (DateTime.Now - dueDate).Days;
                decimal fineAmount = daysOverdue > 0 ? daysOverdue * 5000 : 0; // Giả sử phạt 5000 mỗi ngày quá hạn
                totalFineAmount += fineAmount;
            }

            // Hiển thị thông báo số tiền phí cần trả
            DialogResult result = MessageBox.Show($"Total fine amount to pay: {totalFineAmount:N0}VND\nDo you want to proceed with the return?", "Confirm Return", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Lặp qua các dòng được chọn trong DataGridView
                foreach (DataGridViewRow row in dtgv.SelectedRows)
                {
                    // Lấy thông tin từ dòng được chọn
                    string bookTitle = row.Cells["TITLE"].Value.ToString();
                    DateTime borrowDate = Convert.ToDateTime(row.Cells["BORROW DATE"].Value);
                    DateTime dueDate = Convert.ToDateTime(row.Cells["DUE DATE"].Value);

                    // Tính số ngày quá hạn và số tiền phạt (nếu có)
                    int daysOverdue = (DateTime.Now - dueDate).Days;
                    decimal fineAmount = daysOverdue > 0 ? daysOverdue * 5000 : 0; // Giả sử phạt 5000 mỗi ngày quá hạn

                    // Thêm dữ liệu vào bảng RETURNTICKETS
                    string insertQuery = $@"
                INSERT INTO RETURNTICKETS (TICKETID, LIBRARIANID, RETURNDATE, FINEAMOUNT)
                VALUES (
                    (SELECT TICKETID FROM BORROWINGTICKETS WHERE READERID = '{readerID}' AND BOOKID = (SELECT BOOKID FROM BOOKS WHERE TITLE = N'{bookTitle}')),
                    '{LoginInfo.StaffID}',
                    GETDATE(),
                    {fineAmount}
                );";

                    int rowsAffected = GetDatabase.Instance.ExecuteNonQuery(insertQuery);

                    if (rowsAffected > 0)
                    {
                        // Cập nhật trạng thái của Borrowing Ticket thành "Returned"
                        string updateQuery = $@"
                    UPDATE BORROWINGTICKETS 
                    SET [STATUS] = 'Restored'
                    WHERE READERID = '{readerID}' AND BOOKID = (SELECT BOOKID FROM BOOKS WHERE TITLE = N'{bookTitle}');";

                        GetDatabase.Instance.ExecuteNonQuery(updateQuery);
                    }
                }

                // Thông báo thành công
                MessageBox.Show("Books returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm mới danh sách sách đang mượn
                LoadBorrowedBooks();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Do you really want to exit?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                this.Hide();
            }
            else
            {
                MessageBox.Show("Exit cancelled. Continue your activity ❤️.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Focus();
            }
        }
    }
}
