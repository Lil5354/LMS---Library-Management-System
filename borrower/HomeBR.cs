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
using Guna.UI2.WinForms;
using System.Data;
using System.Data.SqlClient;
using LMS.Proccess;
using System.Collections;

namespace LMS.borrower
{
    public partial class HomeBR : Form
    {
        public HomeBR()
        {
            InitializeComponent();
            panelBook.Visible = false;
            FlowBooks.Visible = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
            SetDoubleBuffered(panelBook, true);
            SetDoubleBuffered(FlowBooks, true);
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 450);
        }
        private void SetDoubleBuffered(Control control, bool value)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            property?.SetValue(control, value, null);
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

        private void btnMaxSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnMaxSize.Visible = false;
            btnMinSize.Visible = true;
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login frmLogin = new Login();
            frmLogin.Show();
        }
        //private void LoadBooks(string query)
        //{
          
        //}

        // Xử lý sự kiện khi người dùng nhấp vào bìa sách
        private void BookCover_Click(int bookId, string bookTitle)
        {
            // Xử lý khi người dùng nhấp vào ảnh bìa sách
            ShowBookDetails(bookId, bookTitle);
        }

        // Xử lý sự kiện khi người dùng nhấp vào panel sách
        private void BookPanel_Click(int bookId, string bookTitle)
        {
            // Xử lý khi người dùng nhấp vào panel sách
            ShowBookDetails(bookId, bookTitle);
        }

        // Hiển thị chi tiết sách
        private void ShowBookDetails(int bookId, string bookTitle)
        {
            try
            {
                string query = $"SELECT B.BOOKID, B.TITLE, A.FULLNAME AS AUTHOR, P.NAME AS PUBLISHER, " +
                             $"C.NAME AS CATEGORY, B.PUBLICATIONYEAR AS PUBLISHED_DATE, B.DESCRIPTION, " +
                             $"C.NAME AS TYPE " +
                             $"FROM BOOKS B " +
                             $"INNER JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID " +
                             $"INNER JOIN PUBLISHERS P ON B.PUBLISHERID = P.PUBLISHERID " +
                             $"INNER JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID " +
                             $"WHERE B.BOOKID = {bookId}";

                DataTable bookData = GetDatabase.Instance.ExecuteQuery(query);

                if (bookData.Rows.Count > 0)
                {
                    DataRow book = bookData.Rows[0];

                    // Clear the panel
                    FlowBooks.Visible = false;

                    // Create the layout for book details
                    Guna2PictureBox coverImage = new Guna2PictureBox
                    {
                        Width = 180,
                        Height = 220,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(45, 45),
                        BorderRadius = 10
                    };

                    // Load book cover image
                    try
                    {
                        string imagePath = $@"D:\FILE CỦA THẢO\Software PJ\LMS\Image Item/{bookTitle}.jpg";
                        if (File.Exists(imagePath))
                        {
                            coverImage.Image = Image.FromFile(imagePath);
                        }
                        else
                        {
                            coverImage.Image = Properties.Resources.default_book_cover;
                        }
                    }
                    catch
                    {
                        coverImage.Image = Properties.Resources.default_book_cover;
                    }

                    // Book title label - now positioned beside the book cover
                    Label lblTitle = new Label
                    {
                        Text = bookTitle,
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        AutoSize = false,
                        Width = panelBook.Width - 250,
                        Height = 40,
                        Location = new Point(250, 45)
                    };

                    // Create a rounded description panel using Guna2Panel
                    Guna2Panel descriptionPanel = new Guna2Panel
                    {
                        Width = panelBook.Width - 300,
                        Height = 170,
                        Location = new Point(250, 95),
                        BorderRadius = 15,
                        BorderColor = Color.FromArgb(220, 220, 220),
                        BorderThickness = 1,
                        FillColor = Color.White,
                        ShadowDecoration = {
                    Enabled = true,
                    Depth = 1,
                    Color = Color.FromArgb(30, 0, 0, 0)
                }
                    };

                    // Update RichTextBox for description with proper padding
                    RichTextBox rtbDescription = new RichTextBox
                    {
                        Text = book["DESCRIPTION"].ToString(),
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = Color.White,
                        Location = new Point(15, 15),
                        Width = descriptionPanel.Width - 30,
                        Height = descriptionPanel.Height - 30,
                        Font = new Font("Segoe UI", 10)
                    };

                    // Add description to the panel
                    descriptionPanel.Controls.Add(rtbDescription);

                    // Update these labels for book details
                    Label lblAuthor = new Label
                    {
                        Text = $"Author: {book["AUTHOR"]}",
                        Font = new Font("Segoe UI", 11),
                        Location = new Point(250, 280),
                        AutoSize = true
                    };

                    Label lblPublished = new Label
                    {
                        Text = $"Date published: {book["PUBLISHED_DATE"]}",
                        Font = new Font("Segoe UI", 11),
                        Location = new Point(250, 310),
                        AutoSize = true
                    };

                    Label lblType = new Label
                    {
                        Text = $"Type: {book["TYPE"]}",
                        Font = new Font("Segoe UI", 11),
                        Location = new Point(250, 340),
                        AutoSize = true
                    };

                    // Create Borrow and Return buttons
                    Guna2Button btnBorrow = new Guna2Button
                    {
                        Text = "Borrow",
                        FillColor = Color.FromArgb(128, 128, 128),
                        Size = new Size(180, 40),
                        BorderRadius = 5,
                        Location = new Point(45, 280),
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.White
                    };

                    Guna2Button btnReturn = new Guna2Button
                    {
                        Text = "Return",
                        FillColor = Color.FromArgb(76, 175, 80),
                        Size = new Size(180, 40),
                        BorderRadius = 5,
                        Location = new Point(45, 330),
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.White
                    };

                    // Add event handlers
                    btnBorrow.Click += (sender, e) => BorrowBook(bookId);
                    btnReturn.Click += (sender, e) => ReturnBook(bookId);

                    // Add all controls to main panel
                    panelBook.Controls.Add(coverImage);
                    panelBook.Controls.Add(lblTitle);
                    panelBook.Controls.Add(descriptionPanel);
                    panelBook.Controls.Add(lblAuthor);
                    panelBook.Controls.Add(lblPublished);
                    panelBook.Controls.Add(lblType);
                    panelBook.Controls.Add(btnBorrow);
                    panelBook.Controls.Add(btnReturn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading book details: " + ex.Message);
            }
        }

        // Thêm các phương thức để xử lý mượn và trả sách
        private void BorrowBook(int bookId)
        {
            // Xử lý mượn sách
            MessageBox.Show($"Bạn đã mượn sách có ID: {bookId}");
            // Thêm code để cập nhật trạng thái mượn sách trong database
        }

        private void ReturnBook(int bookId)
        {
            // Xử lý trả sách
            MessageBox.Show($"Bạn đã trả sách có ID: {bookId}");
            // Thêm code để cập nhật trạng thái trả sách trong database
        }

        private void btnCatalog_Click(object sender, EventArgs e)
        {
            panelBook.Visible = true;
            FlowBooks.Visible = true;

            try
            {
                // Change the initial query to match your database schema
                string query = "SELECT * FROM Books";
                DataTable data = GetDatabase.Instance.ExecuteQuery(query);
                FlowBooks.Controls.Clear();

                foreach (DataRow row in data.Rows)
                {
                    // Change ID to BOOKID
                    int bookId = Convert.ToInt32(row["BOOKID"]);
                    string bookTitle = row["TITLE"].ToString();

                    // Create the main container using Guna2Panel for rounded corners
                    Guna2Panel bookPanel = new Guna2Panel
                    {
                        Width = 150,
                        Height = 180,
                        Margin = new Padding(10),
                        FillColor = Color.FromArgb(255, 233, 214, 181),
                        BorderRadius = 15,
                        ShadowDecoration = {
                    Enabled = true,
                    Depth = 2,
                    Color = Color.FromArgb(20, 0, 0, 0)
                },
                        Cursor = Cursors.Hand
                    };

                    // Create and configure the image using Guna2PictureBox
                    Guna2PictureBox bookCover = new Guna2PictureBox
                    {
                        Width = 110,
                        Height = 110,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(20, 10),
                        BorderRadius = 5
                    };

                    // Load book cover image
                    try
                    {
                        string imagePath = $@"D:\FILE CỦA THẢO\Software PJ\LMS\Image Item/{bookTitle}.jpg";
                        if (File.Exists(imagePath))
                        {
                            bookCover.Image = Image.FromFile(imagePath);
                        }
                        else
                        {
                            // Default book cover if specific cover not found
                            bookCover.Image = Properties.Resources.default_book_cover;
                        }
                    }
                    catch
                    {
                        bookCover.Image = Properties.Resources.default_book_cover;
                    }

                    bookCover.Click += (s, ev) => BookCover_Click(bookId, bookTitle);

                    // Create and configure the title label
                    Label titleLabel = new Label
                    {
                        Text = bookTitle,
                        AutoSize = false,
                        Width = 130,
                        Height = 40,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Times New Roman", 9, FontStyle.Bold),
                        Location = new Point(10, 130),
                        BackColor = Color.Transparent
                    };

                    // Add hover effect using Guna properties
                    bookPanel.MouseEnter += (s, ev) =>
                    {
                        bookPanel.FillColor = Color.FromArgb(240, 225, 200);
                        bookPanel.Cursor = Cursors.Hand;
                    };
                    bookPanel.MouseLeave += (s, ev) =>
                    {
                        bookPanel.FillColor = Color.FromArgb(255, 233, 214, 181);
                    };

                    // Add click event
                    bookPanel.Click += (s, ev) => BookPanel_Click(bookId, bookTitle);

                    // Add controls to the panel
                    bookPanel.Controls.Add(bookCover);
                    bookPanel.Controls.Add(titleLabel);

                    // Add panel to the flow layout
                    FlowBooks.Controls.Add(bookPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading books: " + ex.Message);
            }
        }
    }
}
