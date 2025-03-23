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
    public partial class BookList : UserControl
    {
        int index = -1;
        string query = "", q = "";
        private int selectedBookID = -1;
        public BookList()
        {
            InitializeComponent();
        }

        private void dtgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                index = e.RowIndex;
                // Lấy hàng hiện tại
                DataGridViewRow row = dtgv.Rows[e.RowIndex];
                // Gán dữ liệu từ các cột vào TextBox
                txtBTitle.Text = row.Cells[1].Value?.ToString();
                txtAuthor.Text = row.Cells[2].Value?.ToString();
                txtPublisher.Text = row.Cells[3].Value?.ToString();
                cbbCategory.Text = row.Cells[4].Value?.ToString();
                txtYearPublic.Text = row.Cells[5].Value?.ToString();
                dtpDA.Text = row.Cells[7].Value?.ToString();
                string bookId = row.Cells[0].Value?.ToString(); // ID của sách bạn muốn truy vấn
                string query = "SELECT DESCRIPTION FROM BOOKS WHERE BOOKID = @BookId";
                object[] parameters = { bookId };
                GetDatabase.Instance.LoadDataToTextBox(query, txtDescrip, parameters);
                selectedBookID = Convert.ToInt32(row.Cells[0].Value);

            }
        }

        private void txtDiscription_TextChanged(object sender, EventArgs e)
        {

        }

        private void BookList_Load(object sender, EventArgs e)
        {

            q = "SELECT [NAME] FROM CATEGORIES;";
            GetDatabase.Instance.LoadDataToComboBox(q, cbbCategory);
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
            DTLibrary.Instance.LoadList(query, dtgv);
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
                    B.DATEADDB AS [DATE ADD],
                    CASE 
                        WHEN EXISTS (
                            SELECT 1 
                            FROM BORROWINGTICKETS BT 
                            WHERE BT.BOOKID = B.BOOKID AND BT.[STATUS] = N'Borrowing'
                        ) THEN 'Borrowing'
                        ELSE 'Available'
                    END AS [BORROW STATUS]
                FROM 
                    BOOKS B
                    INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                    INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                    INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                WHERE 
                    B.TITLE LIKE N'%{searchText.Replace("'", "''")}%';";
            DTLibrary.Instance.LoadList(query, dtgv);
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
                    B.BOOKID AS ID, 
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
                            WHERE BT.BOOKID = B.BOOKID AND BT.[STATUS] = N'Borrowing'
                        ) THEN 'Borrowing' 
                        ELSE 'Available' 
                    END AS [BORROW STATUS]
                FROM 
                    BOOKS B
                    INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                    INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID
                    INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                WHERE 
                    (@CategoryName IS NULL OR C.NAME = @CategoryName)
                    AND (@AuthorName IS NULL OR A.FULLNAME = @AuthorName)
                    AND (@PublicationYear IS NULL OR B.PUBLICATIONYEAR = @PublicationYear)
                    AND B.STATUS = 1;";
            DTLibrary.Instance.LoadList(query, dtgv);
            ClearBookListBox();
        }

    
        private void btnLoad_Click_1(object sender, EventArgs e)
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
            DTLibrary.Instance.LoadList(query, dtgv);
            ClearBookListBox();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {

        }

        private void dtpDA_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ClearBookListBox()
        {
            cbbAuthorSearch.SelectedIndex = -1;
            cbbCateSearch.SelectedIndex = -1;
            cbbCategory.SelectedIndex = -1;
            cbbYofPSearch.SelectedIndex = -1;
            txtSearchBook.Text = txtAuthor.Text = txtDescrip.Text = txtBTitle.Text = txtPublisher.Text = txtYearPublic.Text = "";
            dtpDA.Value = DateTime.Now;
        }
    }
}
