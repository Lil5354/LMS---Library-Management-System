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
    public partial class AddBook : UserControl
    {
        int index = -1;
        string query = "", q = "";
        private int selectedBookID = -1;
        public AddBook()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void AddBook_Load(object sender, EventArgs e)
        {
            q = "SELECT [NAME] FROM CATEGORIES;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbCategory);
            q = "SELECT [NAME] FROM PUBLISHERS;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbPublic);
            q = "SELECT \r\n    FULLNAME AS AUTHOR_NAME\r\nFROM \r\n    AUTHORS;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbAuthor);
            q = "SELECT DISTINCT\r\n    PUBLICATIONYEAR AS YEAR_OF_PUBLICATION\r\nFROM \r\n    BOOKS B";
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
                    WHEN EXISTS (
                        SELECT 1 
                        FROM BORROWINGTICKETS BT 
                        WHERE BT.TICKETID = B.BOOKID AND BT.[STATUS] = N'Borrowing'
                    ) THEN 'Borrowing'
                    ELSE 'Available'
                END AS [BORROW STATUS]
            FROM 
                BOOKS B
                INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
            WHERE B.STATUS = 1;";
            DTLibrary.Instance.LoadList(query, dtgvAddBook);
            ClearAddBookBox();
        }

        private void cbbAuthor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbAuthor.SelectedItem.ToString() == "Other")
            {
                grbAddAuthor.Visible = true;
            }
            else
            {
                grbAddAuthor.Visible = false;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string authorName = cbbAuthor.SelectedItem?.ToString();
            string publisherName = cbbPublic.SelectedItem?.ToString();
            string categoryName = cbbCategory.SelectedItem?.ToString();
            string publicationYearText = txtYearPublic.Text.Trim();

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(authorName) ||
                string.IsNullOrEmpty(publisherName) || string.IsNullOrEmpty(categoryName) ||
                string.IsNullOrEmpty(publicationYearText))
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra năm xuất bản có phải là số hợp lệ không
            if (!int.TryParse(publicationYearText, out int publicationYear))
            {
                MessageBox.Show("Publication year must be a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Lấy AUTHORID từ tên tác giả
            string getAuthorIDQuery = $"SELECT AUTHORID FROM AUTHORS WHERE FULLNAME = N'{authorName.Replace("'", "''")}';";
            int authorID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getAuthorIDQuery));

            // Lấy PUBLISHERID từ tên nhà xuất bản
            string getPublisherIDQuery = $"SELECT PUBLISHERID FROM PUBLISHERS WHERE NAME = N'{publisherName.Replace("'", "''")}';";
            int publisherID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getPublisherIDQuery));

            // Lấy CATEGORYID từ tên danh mục
            string getCategoryIDQuery = $"SELECT CATEGORYID FROM CATEGORIES WHERE NAME = N'{categoryName.Replace("'", "''")}';";
            int categoryID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getCategoryIDQuery));

            // Thêm sách mới vào bảng BOOKS
            string insertBookQuery = $@"
            INSERT INTO BOOKS (TITLE, AUTHORID, PUBLISHERID, CATEGORYID, PUBLICATIONYEAR, DATEADDB)
            VALUES (
                N'{title.Replace("'", "''")}',
                {authorID},
                {publisherID},
                {categoryID},
                {publicationYear},
                GETDATE()
            );";
            GetDatabase.Instance.ExecuteQuery(insertBookQuery);

            // Thông báo thành công
            MessageBox.Show("Book added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddBook_Load(sender, e);
            ClearAddBookBox();
        }

        private void btnAddAuthor_Click(object sender, EventArgs e)
        {
            string authorName = txtAuthorName.Text.Trim();
            string authorBio = txtAuthorBio.Text.Trim();

            // Check if the author's name is entered
            if (string.IsNullOrEmpty(authorName))
            {
                MessageBox.Show("Please enter the author's name.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the author already exists in the database
            string checkAuthorQuery = $"SELECT COUNT(*) FROM AUTHORS WHERE FULLNAME = N'{authorName.Replace("'", "''")}';";
            int authorCount = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(checkAuthorQuery));

            if (authorCount > 0)
            {
                MessageBox.Show("Author already exists in the system.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add the new author to the AUTHORS table
            string insertAuthorQuery = $@"
        INSERT INTO AUTHORS (FULLNAME, BIOGRAPHY)
        VALUES (N'{authorName.Replace("'", "''")}', N'{authorBio.Replace("'", "''")}');";
            GetDatabase.Instance.ExecuteQuery(insertAuthorQuery);

            // Show success notification
            MessageBox.Show("Author added successfully!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Clear the input controls
            q = "SELECT \r\n    FULLNAME AS AUTHOR_NAME\r\nFROM \r\n    AUTHORS;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbAuthor);
            txtAuthorName.Clear();
            txtAuthorBio.Clear();
            grbAddAuthor.Visible = false;
            cbbAuthor.SelectedItem = authorName;
        }

        private void dtgvAddBook_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                index = e.RowIndex;
                // Lấy hàng hiện tại
                DataGridViewRow row = dtgvAddBook.Rows[e.RowIndex];
                // Gán dữ liệu từ các cột vào TextBox
                txtTitle.Text = row.Cells[1].Value?.ToString();
                cbbAuthor.Text = row.Cells[2].Value?.ToString();
                cbbAuthor.Text = row.Cells[3].Value?.ToString();
                cbbCategory.Text = row.Cells[4].Value?.ToString();
                txtYearPublic.Text = row.Cells[5].Value?.ToString();
                dtpDateAdd.Text = row.Cells[7].Value?.ToString();
                string bookId = row.Cells[0].Value?.ToString(); // ID của sách bạn muốn truy vấn
                string query = "SELECT DESCRIPTION FROM BOOKS WHERE BOOKID = @BookId";
                object[] parameters = { bookId };
                GetDatabase.Instance.LoadDataToTextBox(query, txtDescrip, parameters);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            AddBook_Load(sender, e);
            ClearAddBookBox();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

            if (selectedBookID == -1)
            {
                MessageBox.Show("Please select a book to update.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy giá trị từ các điều khiển
            string title = txtTitle.Text.Trim();
            string authorName = cbbAuthor.SelectedItem?.ToString();
            string publisherName = cbbPublic.SelectedItem?.ToString();
            string categoryName = cbbCategory.SelectedItem?.ToString();
            string publicationYearText = txtYearPublic.Text.Trim();

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(authorName) ||
                string.IsNullOrEmpty(publisherName) || string.IsNullOrEmpty(categoryName) ||
                string.IsNullOrEmpty(publicationYearText))
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra năm xuất bản có phải là số hợp lệ không
            if (!int.TryParse(publicationYearText, out int publicationYear))
            {
                MessageBox.Show("Publication year must be a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy AUTHORID từ tên tác giả
            string getAuthorIDQuery = $"SELECT AUTHORID FROM AUTHORS WHERE FULLNAME = N'{authorName.Replace("'", "''")}';";
            int authorID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getAuthorIDQuery));

            // Lấy PUBLISHERID từ tên nhà xuất bản
            string getPublisherIDQuery = $"SELECT PUBLISHERID FROM PUBLISHERS WHERE NAME = N'{publisherName.Replace("'", "''")}';";
            int publisherID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getPublisherIDQuery));

            // Lấy CATEGORYID từ tên danh mục
            string getCategoryIDQuery = $"SELECT CATEGORYID FROM CATEGORIES WHERE NAME = N'{categoryName.Replace("'", "''")}';";
            int categoryID = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(getCategoryIDQuery));

            // Cập nhật thông tin sách trong bảng BOOKS
            string updateBookQuery = $@"
                    UPDATE BOOKS
                    SET 
                        TITLE = N'{title.Replace("'", "''")}',
                        AUTHORID = {authorID},
                        PUBLISHERID = {publisherID},
                        CATEGORYID = {categoryID},
                        PUBLICATIONYEAR = {publicationYear}
                    WHERE 
                        BOOKID = {selectedBookID};";
            GetDatabase.Instance.ExecuteQuery(updateBookQuery);
            MessageBox.Show("Book updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddBook_Load(sender, e);
            ClearAddBookBox();
        }

        private void ClearAddBookBox()
        {
            cbbAuthor.SelectedIndex = -1;
            cbbPublic.SelectedIndex = -1;
            cbbCategory.SelectedIndex = -1;
            txtDescrip.Text = txtTitle.Text = txtYearPublic.Text = txtAuthorName.Text = txtAuthorBio.Text = "";
            dtpDateAdd.Value = DateTime.Now;
        }

    }
}
