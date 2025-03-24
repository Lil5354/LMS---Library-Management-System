using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LMS.Proccess;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static LMS.Login;

namespace LMS.Generate_Code
{
    public partial class AddBorrower : UserControl
    {
        private string idStaff;
        private int selectedBookID = -1;
        string query = "", q = "";
        private List<string> allReaders = new List<string>();
        public AddBorrower()
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
        private void InitializeComboBox()
        {
            // Query to get all reader names from the database
            string query = "SELECT FULLNAME FROM READERS;";
            var readers = GetDatabase.Instance.LoadData(query);

            // Clear existing items in the ComboBox
            cbbReader.Items.Clear();

            // Add reader names to the ComboBox and the allReaders list
            foreach (var reader in readers)
            {
                string readerName = reader["FULLNAME"].ToString();
                cbbReader.Items.Add(readerName);
                allReaders.Add(readerName);
            }

            // Set the DropDownStyle to DropDown
            cbbReader.DropDownStyle = ComboBoxStyle.DropDown;
            cbbReader.AutoCompleteSource = AutoCompleteSource.ListItems;
            // Attach the TextChanged event handler
            cbbReader.TextChanged += cbbReader_TextChanged;
        }
        private void VBrwTicket_Click(object sender, EventArgs e)
        {
            AddBorrower2 listBookControl = new AddBorrower2();
            AddUserControl(listBookControl);
        }
        private void ClearBookListBox()
        {
            cbbAuthorSearch.SelectedIndex = -1;
            cbbCateSearch.SelectedIndex = -1;
            cbbYofPSearch.SelectedIndex = -1;
        }
        private void LoadBorrowedBooks(string readerName)
        {
            // Query to get currently borrowed books by the reader
            string query = $@"
        SELECT B.TITLE, BT.BORROWDATE, BT.DUEDATE
        FROM BORROWINGTICKETS BT
        INNER JOIN BOOKS B ON BT.BOOKID = B.BOOKID
        INNER JOIN READERS R ON BT.READERID = R.READERID
        WHERE R.FULLNAME = N'{readerName.Replace("'", "''")}' AND BT.[STATUS] = 'Borrowing';";
            var borrowedBooks = GetDatabase.Instance.ExecuteQuery(query);

            // Bind the data to the DataGridView
            dtgvBBorrow.DataSource = borrowedBooks;
        }
        private void AddBorrower_Load(object sender, EventArgs e)
        {
            q = "SELECT [NAME] FROM CATEGORIES;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbCateSearch);
            q = "SELECT \r\n    FULLNAME AS AUTHOR_NAME\r\nFROM \r\n    AUTHORS;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbAuthorSearch);
            q = "SELECT DISTINCT\r\n    PUBLICATIONYEAR AS YEAR_OF_PUBLICATION\r\nFROM \r\n    BOOKS B";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbYofPSearch);
            query = @"SELECT 
                B.BOOKID AS [ID],
                B.TITLE,
                A.FULLNAME AS AUTHOR,
                P.NAME AS PUBLISHER,
                C.NAME AS CATEGORY,
                B.PUBLICATIONYEAR AS [YEAR],
                B.BORROWEDCOUNT AS [NO.BORROWED],
                B.DATEADDB AS [DATE ADD]
            FROM 
                BOOKS B
                INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
            WHERE 
                B.STATUS = 1 
                AND NOT EXISTS (
                    SELECT 1 
                    FROM BORROWINGTICKETS BT 
                    WHERE BT.BOOKID = B.BOOKID AND  (BT.[STATUS] = N'Borrowing' OR BT.[APPROVAL_STATUS] = N'Waiting'));";
            DTLibrary.Instance.LoadList(query, dtgv);
            InitializeComboBox();
            ClearBookListBox();
        }
        private void cbbReader_TextChanged(object sender, EventArgs e)
        {
            string searchText = cbbReader.Text.ToLower();

            // Clear the current items in the ComboBox
            cbbReader.Items.Clear();

            // Filter the allReaders list based on the search text
            var filteredReaders = allReaders.Where(reader => reader.ToLower().Contains(searchText)).ToList();

            // Add the filtered items back to the ComboBox
            foreach (var reader in filteredReaders)
            {
                cbbReader.Items.Add(reader);
            }

            // Show the dropdown list
            cbbReader.DroppedDown = true;

            // Set the cursor position to the end of the text
            cbbReader.SelectionStart = cbbReader.Text.Length;
        }
        private void cbbReader_SelectedIndexChanged(object sender, EventArgs e)
        {
            string readerName = cbbReader.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(readerName))
            {
                // Query to get the reader's email
                string getEmailQuery = $"SELECT EMAIL FROM READERS WHERE FULLNAME = N'{readerName.Replace("'", "''")}';";
                string email = GetDatabase.Instance.ExecuteScalar(getEmailQuery).ToString();

                // Set the email in the TextBox
                txtEmail.Text = email;

                // Load currently borrowed books for the selected reader
                LoadBorrowedBooks(readerName);
            }
        }
        private void btnSearchAdv_Click(object sender, EventArgs e)
        {
            string categoryName = string.IsNullOrEmpty(cbbCateSearch.Text) ? "NULL" : $"N'{cbbCateSearch.Text.Replace("'", "''")}'";
            string authorName = string.IsNullOrEmpty(cbbAuthorSearch.Text) ? "NULL" : $"N'{cbbAuthorSearch.Text.Replace("'", "''")}'";
            string publicationYear = string.IsNullOrEmpty(cbbYofPSearch.Text) ? "NULL" : cbbYofPSearch.Text;
            string query = $@"
                DECLARE @CategoryName NVARCHAR(100) = {categoryName};
                DECLARE @AuthorName NVARCHAR(100) = {authorName};
                DECLARE @PublicationYear INT = {publicationYear};

                SELECT 
                B.BOOKID AS [ID],
                B.TITLE,
                A.FULLNAME AS AUTHOR,
                P.NAME AS PUBLISHER,
                C.NAME AS CATEGORY,
                B.PUBLICATIONYEAR AS [YEAR],
                B.BORROWEDCOUNT AS [NO.BORROWED],
                B.DATEADDB AS [DATE ADD]
            FROM 
                BOOKS B
                INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
            WHERE 
                B.STATUS = 1 
                AND (@CategoryName IS NULL OR C.NAME = @CategoryName)
                    AND (@AuthorName IS NULL OR A.FULLNAME = @AuthorName)
                    AND (@PublicationYear IS NULL OR B.PUBLICATIONYEAR = @PublicationYear)
                    AND NOT EXISTS (
                    SELECT 1 
                    FROM BORROWINGTICKETS BT 
                    WHERE BT.BOOKID = B.BOOKID AND  (BT.[STATUS] = N'Borrowing' OR BT.[APPROVAL_STATUS] = N'Waiting'));";       
            DTLibrary.Instance.LoadList(query, dtgv);
            ClearBookListBox();
        }
        private void dtgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Đảm bảo người dùng click vào dòng dữ liệu, không phải header
            {
                DataGridViewRow row = dtgv.Rows[e.RowIndex];
                selectedBookID = Convert.ToInt32(row.Cells["ID"].Value); // Lấy BOOKID từ cột "ID"
            }
        }
        private void txtSearchBook_KeyUp(object sender, KeyEventArgs e)
        {
            string searchText = txtSearchBook.Text.Trim(); // Lấy giá trị từ ô tìm kiếm và loại bỏ khoảng trắng thừa

            string query = $@"
                SELECT 
                B.BOOKID AS [ID],
                B.TITLE,
                A.FULLNAME AS AUTHOR,
                P.NAME AS PUBLISHER,
                C.NAME AS CATEGORY,
                B.PUBLICATIONYEAR AS [YEAR],
                B.BORROWEDCOUNT AS [NO.BORROWED],
                B.DATEADDB AS [DATE ADD]
            FROM 
                BOOKS B
                INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
            WHERE 
                B.STATUS = 1 
               
                AND NOT EXISTS (
                    SELECT 1 
                    FROM BORROWINGTICKETS BT 
                    WHERE BT.BOOKID = B.BOOKID AND (BT.[STATUS] = N'Borrowing' OR BT.[APPROVAL_STATUS] = N'Waiting'))
                    AND B.TITLE LIKE N'%{searchText.Replace("'", "''")}%';";
            DTLibrary.Instance.LoadList(query, dtgv);
        }
        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (selectedBookID == -1 || string.IsNullOrEmpty(cbbReader.Text.Trim()))
            {
                MessageBox.Show("Please select a book and enter a valid reader name.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy thông tin độc giả
            string readerName = cbbReader.Text.Trim();
            string getReaderIDQuery = $"SELECT READERID FROM READERS WHERE FULLNAME = N'{readerName.Replace("'", "''")}';";
            int readerID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getReaderIDQuery));

            // Kiểm tra số lượng sách đang mượn
            string checkBorrowedCountQuery = $@"
            SELECT COUNT(*) 
            FROM BORROWINGTICKETS 
            WHERE READERID = {readerID} AND [STATUS] = 'Borrowing';";
            int borrowedCount = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(checkBorrowedCountQuery));

            if (borrowedCount >= 5)
            {
                MessageBox.Show("The reader has already borrowed 5 books. Cannot borrow more.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy thông tin thủ thư từ biến idStaff trong form Home
            string librarianID = LoginInfo.StaffID;

            // Tính Due Date (7 ngày sau ngày hiện tại)
            DateTime dueDate = DateTime.Now.AddDays(7);

            // Thêm Borrowing Ticket vào cơ sở dữ liệu
            string insertBorrowingTicketQuery = $@"
        INSERT INTO BORROWINGTICKETS (READERID, LIBRARIANID, BOOKID, BORROWDATE, DUEDATE, [STATUS])
        VALUES (
            {readerID},
            '{librarianID}',
            {selectedBookID},
            GETDATE(),
            '{dueDate.ToString("yyyy-MM-dd")}',
            'Borrowing'
        );";
            GetDatabase.Instance.ExecuteQuery(insertBorrowingTicketQuery);

            // Thông báo thành công
            MessageBox.Show("Borrowing ticket added successfully!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Làm mới dữ liệu
            LoadBorrowedBooks(readerName);
        }
    }
}
