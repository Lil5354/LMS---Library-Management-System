using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LMS.Database;

namespace LMS.Generate_Code
{
    public partial class Restore : UserControl
    {
        string query= "";
        private int selectedBookID = -1;

        public Restore()
        {
            InitializeComponent();
        }

        private void Restore_Load(object sender, EventArgs e)
        {
            query = @"SELECT 
                B.BOOKID AS [ID],
                B.TITLE,
                A.FULLNAME AS AUTHOR,
                P.NAME AS PUBLISHER,
                C.NAME AS CATEGORY,
                B.PUBLICATIONYEAR AS [YEAR],
                B.BORROWEDCOUNT AS [NO.BORROWED],
                B.DATEADDB AS [DATE ADD],
                CASE 
                    WHEN B.STATUS = 0 THEN 'Hidden'
                    WHEN B.STATUS = 1 THEN 'Available'
                END AS [STATUS]
            FROM 
                BOOKS B
                INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
            ORDER BY 
                B.STATUS;";
            DTLibrary.Instance.LoadList(query, dtgv);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Restore_Load(sender, e);
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (selectedBookID == -1)
            {
                MessageBox.Show("Please select a book to restore.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra trạng thái của sách
            string checkStatusQuery = $"SELECT STATUS FROM BOOKS WHERE BOOKID = {selectedBookID};";
            int status = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(checkStatusQuery));

            if (status == 1)
            {
                MessageBox.Show("The book is already available.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Cập nhật STATUS về 1 để khôi phục sách
            string updateStatusQuery = $"UPDATE BOOKS SET STATUS = 1 WHERE BOOKID = {selectedBookID};";
            GetDatabase.Instance.ExecuteQuery(updateStatusQuery);

            // Thông báo thành công
            MessageBox.Show("Book restored successfully!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Làm mới dữ liệu trong DataGridView
            Restore_Load(sender, e);
        }

        private void dtgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                DataGridViewRow row = dtgv.Rows[e.RowIndex];
                selectedBookID = Convert.ToInt32(row.Cells["ID"].Value); 
            }
        }
    }
}
