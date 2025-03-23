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
        private string _currentReaderID;
        public HomeBR(string readerID)
        {
            InitializeComponent();
            panelBook.Visible = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
            SetDoubleBuffered(panelBook, true);
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 450);
            SetupClock();
            _currentReaderID = readerID;
            if (int.TryParse(readerID, out int readerId))
            {
                UpdateCurrentUser(readerId: readerId);
            }
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

        private void BookCover_Click(int bookId, string bookTitle)
        {
            lblTime.Visible = false;
            panelBook.Controls.Clear();
            panelBook.Visible = true;


            // Xử lý khi người dùng nhấp vào ảnh bìa sách
            ShowBookDetails(bookId, bookTitle);
        }

        // Xử lý sự kiện khi người dùng nhấp vào panel sách
        private void BookPanel_Click(int bookId, string bookTitle)
        {
            lblTime.Visible = false;
            panelBook.Controls.Clear();
            panelBook.Visible = true;
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
        // Thêm định nghĩa lớp CurrentUser
        public class CurrentUser
        {
            public int ReaderId { get; set; }
            public string LibrarianId { get; set; }

            // Các thuộc tính khác nếu cần
        }

        // Tạo biến toàn cục để lưu thông tin người dùng hiện tại
        private static CurrentUser _currentUser;

        // Thuộc tính để truy cập CurrentUser từ bất kỳ đâu trong lớp
        public static CurrentUser CurrentUsers
        {
            get
            {
                // Nếu chưa được khởi tạo, tạo mới với giá trị mặc định
                if (_currentUser == null)
                {
                    _currentUser = new CurrentUser
                    {
                        ReaderId = -1,          // Giá trị mặc định -1 (chưa đăng nhập)
                        LibrarianId = null // Gán cứng LibrarianId = "LB001"
                    };
                }
                return _currentUser;
            }
        }
        public static void UpdateCurrentUser(int readerId = -1, string librarianId = null)
        {
            if (_currentUser == null)
            {
                _currentUser = new CurrentUser();
            }

            _currentUser.ReaderId = readerId;
            _currentUser.LibrarianId = librarianId;
        }
        // Thêm các phương thức để xử lý mượn và trả sách
        private void BorrowBook(int bookId)
        {
            try
            {
                // Get the current reader ID
                int currentReaderId = CurrentUsers.ReaderId;

                // Check if the current user ID is valid
                if (currentReaderId <= 0)
                {
                    MessageBox.Show("Invalid Reader ID. Please log in again.",
                                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if the book is already borrowed by this user
                string checkQuery = $"SELECT COUNT(*) FROM BORROWINGTICKETS " +
                                  $"WHERE BOOKID = {bookId} AND READERID = {currentReaderId} " +
                                  $"AND (STATUS = 'Đang mượn' OR STATUS = 'Đang chờ duyệt')";
                int existingBorrows = Convert.ToInt32(GetDatabase.Instance.ExecuteScalar(checkQuery));

                if (existingBorrows > 0)
                {
                    MessageBox.Show("Bạn đã mượn hoặc đặt lịch mượn cuốn sách này và chưa trả.",
                                   "Đã mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Find a valid librarian ID for this operation
                // Either:
                // 1. Get the librarian ID from the LIBRARIANS table (the first valid one)
                // 2. Or use the current user's librarian ID if they're a librarian
                string librarianQuery = "SELECT TOP 1 IDSTAFF FROM LIBRARIANS";
                object librarianResult = GetDatabase.Instance.ExecuteScalar(librarianQuery);

                string librarianId;
                if (librarianResult != null && librarianResult != DBNull.Value)
                {
                    librarianId = librarianResult.ToString();
                }
                else if (!string.IsNullOrEmpty(CurrentUsers.LibrarianId))
                {
                    librarianId = CurrentUsers.LibrarianId;
                }
                else
                {
                    MessageBox.Show("Không có thủ thư khả dụng để xử lý giao dịch này.",
                                   "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Set due date to 14 days from now
                DateTime borrowDate = DateTime.Now;
                DateTime dueDate = borrowDate.AddDays(14);

                // Create insert query for BORROWINGTICKETS table - ensure librarian ID exists in LIBRARIANS table
                string insertQuery = $"INSERT INTO BORROWINGTICKETS (READERID, LIBRARIANID, BOOKID, BORROWDATE, DUEDATE, STATUS, APPROVAL_STATUS) " +
                                   $"VALUES ({currentReaderId}, '{librarianId}', {bookId}, " +
                                   $"'{borrowDate.ToString("yyyy-MM-dd")}', '{dueDate.ToString("yyyy-MM-dd")}', 'Đang chờ duyệt', 'Chờ duyệt')";

                // Execute the insert query
                int result = GetDatabase.Instance.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    MessageBox.Show("Yêu cầu mượn sách của bạn đã được gửi và đang chờ thủ thư phê duyệt. " +
                                    "Ngày hẹn trả: " + dueDate.ToString("yyyy-MM-dd"),
                                   "Ngày hẹn trả sẽ được xác nhận sau khi phê duyệt",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the book details to update any UI changes
                    string currentTitle = GetBookTitle(bookId);
                    ShowBookDetails(bookId, currentTitle);
                }
                else
                {
                    MessageBox.Show("Không thể đăng ký mượn sách. Vui lòng thử lại.",
                                   "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình mượn sách: " + ex.Message,
                               "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReturnBook(int bookId)
        {
            try
            {
                // Get the current reader ID
                int currentReaderId = CurrentUsers.ReaderId;

                // Check if the current user ID is valid
                if (currentReaderId <= 0)
                {
                    MessageBox.Show("ID độc giả không hợp lệ. Vui lòng đăng nhập lại.",
                                   "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if the book is borrowed by this user
                string checkQuery = $@"SELECT bt.TICKETID, bt.DUEDATE, bt.BORROWDATE, bt.STATUS, bt.APPROVAL_STATUS, 
                              b.TITLE, b.AUTHORID, b.PUBLISHERID, b.PUBLICATIONYEAR
                              FROM BORROWINGTICKETS bt
                              JOIN BOOKS b ON bt.BOOKID = b.BOOKID
                              WHERE bt.BOOKID = {bookId} AND bt.READERID = {currentReaderId} 
                              AND (bt.STATUS = 'Đang mượn' OR bt.STATUS = 'Đang chờ duyệt')";

                DataTable borrowedData = GetDatabase.Instance.ExecuteQuery(checkQuery);

                if (borrowedData.Rows.Count == 0)
                {
                    MessageBox.Show("Bạn chưa mượn cuốn sách này hoặc đã trả rồi.",
                                   "Không mượn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the ticket information and book details
                int ticketId = Convert.ToInt32(borrowedData.Rows[0]["TICKETID"]);
                DateTime dueDate = Convert.ToDateTime(borrowedData.Rows[0]["DUEDATE"]);
                DateTime borrowDate = Convert.ToDateTime(borrowedData.Rows[0]["BORROWDATE"]);
                string status = borrowedData.Rows[0]["STATUS"].ToString();
                string approvalStatus = borrowedData.Rows[0]["APPROVAL_STATUS"].ToString();
                string title = borrowedData.Rows[0]["TITLE"].ToString();
                int authorId = Convert.ToInt32(borrowedData.Rows[0]["AUTHORID"]);
                int publisherId = Convert.ToInt32(borrowedData.Rows[0]["PUBLISHERID"]);
                string publishYear = borrowedData.Rows[0]["PUBLICATIONYEAR"].ToString();

                // Get author name and publisher name from their respective IDs
                string authorQuery = $"SELECT FULLNAME FROM AUTHORS WHERE AUTHORID = {authorId}";
                string publisherQuery = $"SELECT NAME FROM PUBLISHERS WHERE PUBLISHERID = {publisherId}";

                string authorName = GetDatabase.Instance.ExecuteScalar(authorQuery)?.ToString() ?? "Unknown";
                string publisherName = GetDatabase.Instance.ExecuteScalar(publisherQuery)?.ToString() ?? "Unknown";

                DateTime currentDate = DateTime.Now;

                // Calculate fine if the book would be returned late (10,000 VND per day)
                decimal fineAmount = 0;
                int daysLate = 0;
                if (currentDate > dueDate)
                {
                    daysLate = (int)(currentDate - dueDate).TotalDays;
                    fineAmount = daysLate * 10000; // 10,000 VND per day
                }

                // Build information message
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"Thông tin sách:");
                messageBuilder.AppendLine($"Tên sách: {title}");
                messageBuilder.AppendLine($"Tác giả: {authorName}");
                messageBuilder.AppendLine($"Nhà xuất bản: {publisherName}");
                messageBuilder.AppendLine($"Năm xuất bản: {publishYear}");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine($"Ngày mượn: {borrowDate.ToString("dd/MM/yyyy")}");
                messageBuilder.AppendLine($"Hạn trả: {dueDate.ToString("dd/MM/yyyy")}");

                if (status == "Đang chờ duyệt")
                {
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Trạng thái: Đang chờ duyệt");
                    messageBuilder.AppendLine("Yêu cầu mượn sách của bạn đang chờ thủ thư phê duyệt.");
                }
                else if (status == "Đang mượn")
                {
                    messageBuilder.AppendLine();
                    if (daysLate > 0)
                    {
                        messageBuilder.AppendLine($"Sách đã quá hạn {daysLate} ngày!");
                        messageBuilder.AppendLine($"Phí phạt quá hạn: {fineAmount.ToString("#,##0")} VND");
                    }
                    else
                    {
                        int daysRemaining = (int)(dueDate - currentDate).TotalDays;
                        messageBuilder.AppendLine($"Còn {daysRemaining} ngày đến hạn trả sách.");
                    }
                }

                MessageBox.Show(messageBuilder.ToString(), "Thông tin mượn sách", MessageBoxButtons.OK,
                              daysLate > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                // Refresh the book details to update any UI changes
                string currentTitle = GetBookTitle(bookId);
                ShowBookDetails(bookId, currentTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hiển thị thông tin sách: " + ex.Message,
                               "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetBookTitle(int bookId)
        {
            try
            {
                string query = $"SELECT TITLE FROM BOOKS WHERE BOOKID = {bookId}";
                return GetDatabase.Instance.ExecuteScalar(query).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        private FlowLayoutPanel FlowBooks;
        private void btnCatalog_Click(object sender, EventArgs e)
        {
            lblTime.Visible = false;
            panelBook.Controls.Clear();
            panelBook.Visible = true;
            // Tạo FlowLayoutPanel để hiển thị sách
            FlowBooks = new FlowLayoutPanel();
            SetDoubleBuffered(FlowBooks, true);

            // Thiết lập thuộc tính cho FlowBooks
            FlowBooks.Dock = DockStyle.Fill;  // Đảm bảo lấp đầy panelBook
            FlowBooks.AutoScroll = true;      // Cho phép cuộn nếu có nhiều sách

            // Thêm FlowBooks vào panelBook
            panelBook.Controls.Add(FlowBooks);

            try
            {
                // Query và thêm sách vào FlowBooks
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
        private Guna2DataGridView dgvBorrowedBooks;
        private void btnBorrow_Click(object sender, EventArgs e)
        {
            lblTime.Visible = false;
            panelBook.Visible = true;
            panelBook.Controls.Clear();
            SetupBorrowingDataGridView();

            // Load the borrowing data
            LoadBorrowingData();
        }
        private void SetupBorrowingDataGridView()
        {
            try
            {
                panelBook.Controls.Clear();

                // Tạo panel container chính với các góc bo tròn
                Guna2Panel containerPanel = new Guna2Panel();
                containerPanel.BorderRadius = 15;
                containerPanel.FillColor = Color.White;
                containerPanel.Dock = DockStyle.Fill;
                containerPanel.Margin = new Padding(10);
                containerPanel.Padding = new Padding(20);
                containerPanel.Size = new Size(panelBook.Width - 40, panelBook.Height - 40);
                containerPanel.Location = new Point(20, 20);
                containerPanel.ShadowDecoration.Enabled = true;
                containerPanel.ShadowDecoration.Depth = 5;
                containerPanel.ShadowDecoration.Color = Color.FromArgb(200, 200, 200);

                // Tạo tiêu đề cho panel
                Label lblHistory = new Label();
                lblHistory.Text = "Borrowing Books";
                lblHistory.Font = new Font("VNI-Vari", 18F);
                lblHistory.AutoSize = true;
                lblHistory.Location = new Point(40, 20);
                lblHistory.ForeColor = Color.FromArgb(101, 70, 49);
                containerPanel.Controls.Add(lblHistory);

                // Tạo nhãn timestamp
                Label lblUpdated = new Label();
                lblUpdated.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                lblUpdated.Font = new Font("Segoe UI", 9F);
                lblUpdated.AutoSize = true;
                lblUpdated.ForeColor = Color.Gray;
                lblUpdated.Location = new Point(containerPanel.Width - 130, 40);
                containerPanel.Controls.Add(lblUpdated);

                // Tạo GroupBox chứa DataGridView
                Guna2GroupBox borrowBookGroupBox = new Guna2GroupBox();
                borrowBookGroupBox.Text = "Borrowed Books";
                borrowBookGroupBox.Font = new Font("VNI-Vari", 16F, FontStyle.Regular);
                borrowBookGroupBox.ForeColor = Color.FromArgb(101, 70, 49);
                borrowBookGroupBox.BorderRadius = 15;
                borrowBookGroupBox.CustomBorderColor = Color.FromArgb(101, 70, 49);
                borrowBookGroupBox.BorderColor = Color.FromArgb(220, 220, 220);
                borrowBookGroupBox.FillColor = Color.White;
                borrowBookGroupBox.Location = new Point(40, 80);
                borrowBookGroupBox.Size = new Size(containerPanel.Width - 40, containerPanel.Height - 80);
                borrowBookGroupBox.ShadowDecoration.Enabled = true;
                borrowBookGroupBox.ShadowDecoration.Depth = 5;
                borrowBookGroupBox.ShadowDecoration.Color = Color.FromArgb(200, 200, 200);
                borrowBookGroupBox.Padding = new Padding(10, 40, 10, 10);
                containerPanel.Controls.Add(borrowBookGroupBox);

                // Tạo DataGridView và gán vào biến thành viên
                dgvBorrowedBooks = new Guna2DataGridView();
                dgvBorrowedBooks.Name = "dgvBorrowedBooks";
                dgvBorrowedBooks.Location = new Point(0, 15);
                dgvBorrowedBooks.Size = new Size(borrowBookGroupBox.Width, borrowBookGroupBox.Height);
                dgvBorrowedBooks.AllowUserToAddRows = false;
                dgvBorrowedBooks.AllowUserToDeleteRows = false;
                dgvBorrowedBooks.AllowUserToResizeRows = false;
                dgvBorrowedBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvBorrowedBooks.BackgroundColor = Color.White;
                dgvBorrowedBooks.BorderStyle = BorderStyle.None;
                dgvBorrowedBooks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvBorrowedBooks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvBorrowedBooks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvBorrowedBooks.ColumnHeadersHeight = 40;
                dgvBorrowedBooks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(107, 78, 55);
                dgvBorrowedBooks.EnableHeadersVisualStyles = false;
                dgvBorrowedBooks.GridColor = Color.FromArgb(231, 229, 255);
                dgvBorrowedBooks.ReadOnly = true;
                dgvBorrowedBooks.RowHeadersVisible = false;
                dgvBorrowedBooks.RowTemplate.Height = 40;
                dgvBorrowedBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvBorrowedBooks.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
                dgvBorrowedBooks.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(101, 70, 49);
                dgvBorrowedBooks.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvBorrowedBooks.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                dgvBorrowedBooks.ThemeStyle.HeaderStyle.ForeColor = Color.White;
                dgvBorrowedBooks.ThemeStyle.HeaderStyle.Height = 40;
                dgvBorrowedBooks.ThemeStyle.RowsStyle.BackColor = Color.White;
                dgvBorrowedBooks.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvBorrowedBooks.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 10F);
                dgvBorrowedBooks.ThemeStyle.RowsStyle.ForeColor = Color.Black;
                dgvBorrowedBooks.ThemeStyle.RowsStyle.Height = 40;
                dgvBorrowedBooks.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
                dgvBorrowedBooks.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
                dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvBorrowedBooks.ThemeStyle.HeaderStyle.BackColor;
                dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

                // Make sure to apply this after all other styling
                dgvBorrowedBooks.Columns.Cast<DataGridViewColumn>().ToList().ForEach(c =>
                {
                    c.HeaderCell.Style.BackColor = Color.FromArgb(101, 70, 49);
                    c.HeaderCell.Style.ForeColor = Color.White;
                    c.HeaderCell.Style.SelectionBackColor = Color.FromArgb(101, 70, 49);
                    c.HeaderCell.Style.SelectionForeColor = Color.White;
                });
                // Canh giữa nội dung trong tất cả các ô
                dgvBorrowedBooks.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Thiết lập Dock để DataGridView lấp đầy GroupBox


                // Thêm DataGridView vào GroupBox trước khi thêm cột và dữ liệu
                borrowBookGroupBox.Controls.Add(dgvBorrowedBooks);
                // Thêm các cột vào DataGridView
                dgvBorrowedBooks.Columns.Add("BookName", "Book Title");
                dgvBorrowedBooks.Columns.Add("Author", "Author");
                dgvBorrowedBooks.Columns.Add("Category", "Category");
                dgvBorrowedBooks.Columns.Add("CheckIn", "Check-in Date");
                dgvBorrowedBooks.Columns.Add("Duration", "Duration");
                dgvBorrowedBooks.Columns.Add("Status", "Status");

                // Thêm container panel vào panel chính
                panelBook.Controls.Add(containerPanel);

                // Xử lý việc cột không hiển thị đúng - refresh grid để cập nhật giao diện
                dgvBorrowedBooks.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up DataGridView: " + ex.Message, "Setup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadBorrowingData()
        {
            try
            {
                // Check if DataGridView is initialized
                if (dgvBorrowedBooks == null || dgvBorrowedBooks.IsDisposed)
                {
                    MessageBox.Show("DataGridView is not properly initialized.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy ReaderId từ người dùng hiện tại đang đăng nhập
                int currentReaderId = CurrentUsers.ReaderId;

                // Nếu không có CurrentUsers hoặc ReaderId không hợp lệ, thử dùng _currentReaderID
                if (currentReaderId <= 0 && !string.IsNullOrEmpty(_currentReaderID))
                {
                    if (!int.TryParse(_currentReaderID, out currentReaderId))
                    {
                        // Nếu không thể chuyển đổi, hiển thị lỗi
                        MessageBox.Show("Invalid Reader ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Use LIKE operator and eliminate all whitespace and case sensitivity issues
                // Thêm điều kiện lọc theo ReaderId của người dùng đang đăng nhập
                string query = @"
SELECT 
    B.TITLE AS BookName,
    A.FULLNAME AS Author,
    C.NAME AS Category,
    BT.BORROWDATE AS CheckInDate,
    BT.DUEDATE AS DueDate,
    CASE 
        WHEN GETDATE() > BT.DUEDATE 
        THEN CAST(DATEDIFF(day, BT.DUEDATE, GETDATE()) AS VARCHAR) + ' days overdue'
        ELSE CAST(DATEDIFF(day, GETDATE(), BT.DUEDATE) AS VARCHAR) + ' days left'
    END AS Duration,
    CASE 
        WHEN GETDATE() > BT.DUEDATE THEN 'Overdue'
        ELSE 'Borrowing'
    END AS Status
FROM 
    BORROWINGTICKETS BT
    JOIN READERS R ON BT.READERID = R.READERID 
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    REPLACE(LTRIM(RTRIM(BT.STATUS)), ' ', '') LIKE N'%Đangmượn%'
    AND BT.RETURNDATE IS NULL
    AND BT.READERID = " + currentReaderId + @"
ORDER BY 
    BT.BORROWDATE DESC";

                // Alternative query that doesn't rely on the status text at all
                string alternativeQuery = @"
SELECT 
    B.TITLE AS BookName,
    A.FULLNAME AS Author,
    C.NAME AS Category,
    BT.BORROWDATE AS CheckInDate,
    BT.DUEDATE AS DueDate,
    CASE 
        WHEN GETDATE() > BT.DUEDATE 
        THEN CAST(DATEDIFF(day, BT.DUEDATE, GETDATE()) AS VARCHAR) + ' days overdue'
        ELSE CAST(DATEDIFF(day, GETDATE(), BT.DUEDATE) AS VARCHAR) + ' days left'
    END AS Duration,
    CASE 
        WHEN GETDATE() > BT.DUEDATE THEN 'Overdue'
        ELSE 'Borrowing'
    END AS Status
FROM 
    BORROWINGTICKETS BT
    JOIN READERS R ON BT.READERID = R.READERID 
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    BT.RETURNDATE IS NULL
    AND BT.READERID = " + currentReaderId + @"
ORDER BY 
    BT.BORROWDATE DESC";

                // Còn lại giữ nguyên
                // Try the first query
                DataTable dataTable = GetDatabase.Instance.ExecuteQuery(query);

                // If no results, try the alternative query that ignores STATUS
                if (dataTable.Rows.Count == 0)
                {
                    dataTable = GetDatabase.Instance.ExecuteQuery(alternativeQuery);
                }

                // Clear existing rows
                dgvBorrowedBooks.Rows.Clear();

                // Check if we got any data back
                if (dataTable.Rows.Count == 0)
                {
                    // Let's do a direct check on the actual values in the database
                    string debugQuery = @"
        SELECT TOP 10 
            TICKETID, 
            READERID,
            STATUS, 
            LEN(STATUS) AS StatusLength, 
            RETURNDATE 
        FROM 
            BORROWINGTICKETS 
        WHERE
            READERID = " + currentReaderId + @"
        ORDER BY 
            TICKETID";

                    DataTable debugTable = GetDatabase.Instance.ExecuteQuery(debugQuery);

                    StringBuilder statusDetails = new StringBuilder();
                    foreach (DataRow row in debugTable.Rows)
                    {
                        string returnDateInfo = row["RETURNDATE"] == DBNull.Value ? "NULL" : row["RETURNDATE"].ToString();
                        statusDetails.AppendLine($"Ticket {row["TICKETID"]}: '{row["STATUS"]}' (Length: {row["StatusLength"]}, ReturnDate: {returnDateInfo})");
                    }

                    MessageBox.Show($"No borrowing data found for reader ID {currentReaderId}. Details from database:\n{statusDetails}",
                        "Debugging Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Process the data and populate the DataGridView
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        string bookName = row["BookName"].ToString();
                        string author = row["Author"].ToString();
                        string category = row["Category"].ToString();
                        DateTime checkInDate = Convert.ToDateTime(row["CheckInDate"]);
                        string duration = row["Duration"].ToString();
                        string status = row["Status"].ToString();

                        // Add the row to the DataGridView
                        int rowIndex = dgvBorrowedBooks.Rows.Add(bookName, author, category,
                            checkInDate.ToString("MM/dd/yyyy"), duration, status);
                    }
                    catch (Exception ex)
                    {
                        // Just log this error but continue processing other rows
                        Console.WriteLine($"Error processing row: {ex.Message}");
                    }
                }

                // Update the timestamp label
                foreach (Control control in panelBook.Controls)
                {
                    if (control is Guna2Panel containerPanel)
                    {
                        foreach (Control panelControl in containerPanel.Controls)
                        {
                            if (panelControl is Label && panelControl.Text.StartsWith("Updated at"))
                            {
                                panelControl.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                                break;
                            }
                        }
                    }
                }

                // Force a refresh
                dgvBorrowedBooks.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrowing data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the complete exception for debugging
                Console.WriteLine(ex.ToString());
            }
        }
        private void StyleStatusCell(int rowIndex, string status)
        {
            if (rowIndex < 0 || rowIndex >= dgvBorrowedBooks.Rows.Count)
                return;

            DataGridViewCell statusCell = dgvBorrowedBooks.Rows[rowIndex].Cells["Status"];

            // Apply pill-shaped background similar to Image 1
            statusCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            statusCell.Style.Padding = new Padding(5);

            if (status == "Returned")
            {
                statusCell.Style.BackColor = Color.LightGreen;
                statusCell.Style.ForeColor = Color.DarkGreen;
            }
            else if (status == "Overdue")
            {
                statusCell.Style.BackColor = Color.LightCoral;
                statusCell.Style.ForeColor = Color.DarkRed;
            }
            else // Borrowing
            {
                statusCell.Style.BackColor = Color.Khaki;
                statusCell.Style.ForeColor = Color.DarkOrange;
            }
        }
        private void btnHome_Click(object sender, EventArgs e)
        {
            lblTime.Visible = true;
            panelBook.Visible = false;
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
        private void TimerClock_Tick(object sender, EventArgs e)
        {
            // This will be called every second (1000ms)
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
        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (FlowBooks != null)
            {
                foreach (Control control in FlowBooks.Controls)
                {
                    if (control is Guna2Panel panel)
                    {
                        // Get the name label from the panel
                        Label nameLabel = panel.Controls.OfType<Label>().FirstOrDefault(
                            label => label.Location.Y == 130);  // Updated Y position to match where labels are created

                        if (nameLabel != null)
                        {
                            string itemName = nameLabel.Text;
                            panel.Visible = itemName.ToLower().Contains(txtSearch.Text.Trim().ToLower());
                        }
                    }
                }
            }
        }
        private void LoadReturnData()
        {
            try
            {
                // Check if DataGridView is initialized
                if (dgvReturnBooks == null || dgvReturnBooks.IsDisposed)
                {
                    MessageBox.Show("DataGridView is not properly initialized.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy ReaderId từ người dùng hiện tại đang đăng nhập
                int currentReaderId = CurrentUsers.ReaderId;

                // Nếu không có CurrentUsers hoặc ReaderId không hợp lệ, thử dùng _currentReaderID
                if (currentReaderId <= 0 && !string.IsNullOrEmpty(_currentReaderID))
                {
                    if (!int.TryParse(_currentReaderID, out currentReaderId))
                    {
                        // Nếu không thể chuyển đổi, hiển thị lỗi
                        MessageBox.Show("Invalid Reader ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Query to get all borrowed books (both overdue and currently borrowed)
                string query = @"
SELECT 
    BT.TICKETID,
    B.TITLE AS Name,
    A.FULLNAME AS Author,
    C.NAME AS Category,
    BT.BORROWDATE AS CheckInDate,
    BT.DUEDATE AS DueDate,
    CASE 
        WHEN GETDATE() > BT.DUEDATE 
        THEN CAST(DATEDIFF(day, BT.DUEDATE, GETDATE()) AS VARCHAR) + ' days overdue'
        ELSE CAST(DATEDIFF(day, GETDATE(), BT.DUEDATE) AS VARCHAR) + ' days left'
    END AS Duration,
    CASE 
        WHEN GETDATE() > BT.DUEDATE THEN 'Overdue'
        ELSE 'Borrowing'
    END AS Status
FROM 
    BORROWINGTICKETS BT
    JOIN READERS R ON BT.READERID = R.READERID 
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    BT.RETURNDATE IS NULL
    AND BT.READERID = " + currentReaderId + @"
ORDER BY 
    CASE WHEN GETDATE() > BT.DUEDATE THEN 0 ELSE 1 END, -- Show overdue first
    DATEDIFF(day, BT.DUEDATE, GETDATE()) DESC";

                // Execute query
                DataTable dataTable = GetDatabase.Instance.ExecuteQuery(query);

                // Clear existing rows
                dgvReturnBooks.Rows.Clear();

                // Check if we got any data back
                if (dataTable.Rows.Count == 0)
                {
                    // No data found
                    MessageBox.Show("No borrowed books found for reader ID " + currentReaderId, "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Process the data and populate the DataGridView
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        int ticketId = Convert.ToInt32(row["TICKETID"]);
                        string bookName = row["Name"].ToString();
                        string author = row["Author"].ToString();
                        string category = row["Category"].ToString(); // This is "Type" in the DataGridView
                        string status = row["Status"].ToString();

                        // Add the row to the DataGridView - match column order with the DataGridView setup
                        int rowIndex = dgvReturnBooks.Rows.Add(ticketId, bookName, author, category, status);

                        // Set custom formatting based on status (overdue vs borrowing)
                        if (status == "Overdue")
                        {
                            dgvReturnBooks.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Just log this error but continue processing other rows
                        Console.WriteLine($"Error processing row: {ex.Message}");
                    }
                }

                // Update the timestamp label
                foreach (Control control in panelBook.Controls)
                {
                    if (control is Guna2Panel containerPanel)
                    {
                        foreach (Control panelControl in containerPanel.Controls)
                        {
                            if (panelControl is Label && panelControl.Text.StartsWith("Updated at"))
                            {
                                panelControl.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                                break;
                            }
                        }
                    }
                }

                // Force a refresh
                dgvReturnBooks.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading return data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the complete exception for debugging
                Console.WriteLine(ex.ToString());
            }
        }        // Add a flag to track if we've already set up the event handler
        private bool _returnButtonHandlerSetup = false;

        // Handle button click events
        private void DgvReturnBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if the clicked cell is in the Action column and not the header row
                if (e.RowIndex >= 0 && e.ColumnIndex == dgvReturnBooks.Columns["Action"].Index)
                {
                    // Get the TICKETID from the first cell of the clicked row
                    int ticketId = Convert.ToInt32(dgvReturnBooks.Rows[e.RowIndex].Cells["TicketID"].Value);
                    string bookTitle = dgvReturnBooks.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                    string status = dgvReturnBooks.Rows[e.RowIndex].Cells["Status"].Value.ToString();

                    // Query to get the ticket details by TICKETID
                    string ticketQuery = $@"
            SELECT 
                BT.TICKETID,
                BT.BORROWDATE,
                BT.DUEDATE,
                BT.STATUS,
                BT.APPROVAL_STATUS,
                B.BOOKID,
                B.TITLE,
                B.AUTHORID,
                B.PUBLISHERID,
                B.PUBLICATIONYEAR,
                B.CATEGORYID
            FROM 
                BORROWINGTICKETS BT
                JOIN BOOKS B ON BT.BOOKID = B.BOOKID
            WHERE 
                BT.TICKETID = {ticketId}";

                    DataTable ticketData = GetDatabase.Instance.ExecuteQuery(ticketQuery);

                    if (ticketData.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy thông tin phiếu mượn.",
                                       "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Get ticket information and book details
                    int bookId = Convert.ToInt32(ticketData.Rows[0]["BOOKID"]);
                    DateTime dueDate = Convert.ToDateTime(ticketData.Rows[0]["DUEDATE"]);
                    DateTime borrowDate = Convert.ToDateTime(ticketData.Rows[0]["BORROWDATE"]);
                    string borrowStatus = ticketData.Rows[0]["STATUS"].ToString();
                    string approvalStatus = ticketData.Rows[0]["APPROVAL_STATUS"].ToString();
                    string title = ticketData.Rows[0]["TITLE"].ToString();
                    int authorId = Convert.ToInt32(ticketData.Rows[0]["AUTHORID"]);
                    int publisherId = Convert.ToInt32(ticketData.Rows[0]["PUBLISHERID"]);
                    string publishYear = ticketData.Rows[0]["PUBLICATIONYEAR"].ToString();

                    // Get author name and publisher name from their respective IDs
                    string authorQuery = $"SELECT FULLNAME FROM AUTHORS WHERE AUTHORID = {authorId}";
                    string publisherQuery = $"SELECT NAME FROM PUBLISHERS WHERE PUBLISHERID = {publisherId}";

                    string authorName = GetDatabase.Instance.ExecuteScalar(authorQuery)?.ToString() ?? "Unknown";
                    string publisherName = GetDatabase.Instance.ExecuteScalar(publisherQuery)?.ToString() ?? "Unknown";

                    DateTime currentDate = DateTime.Now;

                    // Calculate fine if the book would be returned late (10,000 VND per day)
                    decimal fineAmount = 0;
                    int daysLate = 0;
                    if (currentDate > dueDate)
                    {
                        daysLate = (int)(currentDate - dueDate).TotalDays;
                        fineAmount = daysLate * 10000; // 10,000 VND per day
                    }

                    // Build information message
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine($"Thông tin sách:");
                    messageBuilder.AppendLine($"Tên sách: {title}");
                    messageBuilder.AppendLine($"Tác giả: {authorName}");
                    messageBuilder.AppendLine($"Nhà xuất bản: {publisherName}");
                    messageBuilder.AppendLine($"Năm xuất bản: {publishYear}");
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine($"Ngày mượn: {borrowDate.ToString("dd/MM/yyyy")}");
                    messageBuilder.AppendLine($"Hạn trả: {dueDate.ToString("dd/MM/yyyy")}");

                    if (borrowStatus == "Đang chờ duyệt")
                    {
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine("Trạng thái: Đang chờ duyệt");
                        messageBuilder.AppendLine("Yêu cầu mượn sách của bạn đang chờ thủ thư phê duyệt.");

                        MessageBox.Show(messageBuilder.ToString(), "Thông tin mượn sách", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (borrowStatus == "Đang mượn" || status == "Overdue" || status == "Borrowing")
                    {
                        messageBuilder.AppendLine();
                        if (daysLate > 0)
                        {
                            messageBuilder.AppendLine($"Sách đã quá hạn {daysLate} ngày!");
                            messageBuilder.AppendLine($"Phí phạt quá hạn: {fineAmount.ToString("#,##0")} VND");
                            messageBuilder.AppendLine("Vui lòng liên hệ thủ thư để trả sách.");
                        }
                        else
                        {
                            int daysRemaining = (int)(dueDate - currentDate).TotalDays;
                            messageBuilder.AppendLine($"Còn {daysRemaining} ngày đến hạn trả sách.");
                            messageBuilder.AppendLine("Vui lòng liên hệ thủ thư khi muốn trả sách.");
                        }

                        MessageBox.Show(messageBuilder.ToString(), "Thông tin mượn sách", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xem thông tin sách: " + ex.Message,
                               "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Guna2DataGridView dgvReturnBooks;
        private void btnReturn_Click(object sender, EventArgs e)
        {
            lblTime.Visible = false;
            panelBook.Visible = true;
            panelBook.Controls.Clear();
            SetupReturnDataGridView();
            LoadReturnData();
        }
        private void SetupReturnDataGridView()
        {
            try
            {
                panelBook.Controls.Clear();

                // Main container panel with rounded corners
                Guna2Panel containerPanel = new Guna2Panel
                {
                    BorderRadius = 15,
                    FillColor = Color.White,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10),
                    Padding = new Padding(20),
                    Size = new Size(panelBook.Width - 40, panelBook.Height - 40),
                    Location = new Point(20, 20),
                    ShadowDecoration = {
                Enabled = true,
                Depth = 5,
                Color = Color.FromArgb(200, 200, 200)
            }
                };

                // Title label
                Label lblReturn = new Label
                {
                    Text = "Return Book Information",
                    Font = new Font("VNI-Vari", 18F),
                    AutoSize = true,
                    Location = new Point(40, 20),
                    ForeColor = Color.FromArgb(101, 70, 49)
                };
                containerPanel.Controls.Add(lblReturn);

                // Last updated timestamp
                Label lblUpdated = new Label
                {
                    Text = "Updated at " + DateTime.Now.ToString("h:mm tt"),
                    Font = new Font("Segoe UI", 9F),
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Location = new Point(containerPanel.Width - 130, 40)
                };
                containerPanel.Controls.Add(lblUpdated);

                // Group box to contain the DataGridView
                Guna2GroupBox returnBookGroupBox = new Guna2GroupBox
                {
                    Text = "Borrowed Books",
                    Font = new Font("VNI-Vari", 16F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(101, 70, 49),
                    BorderRadius = 15,
                    CustomBorderColor = Color.FromArgb(101, 70, 49),
                    BorderColor = Color.FromArgb(220, 220, 220),
                    FillColor = Color.White,
                    Location = new Point(40, 80),
                    Size = new Size(containerPanel.Width - 80, containerPanel.Height - 100),
                    ShadowDecoration = {
                Enabled = true,
                Depth = 5,
                Color = Color.FromArgb(200, 200, 200)
            },
                    Padding = new Padding(10, 40, 10, 10)
                };
                containerPanel.Controls.Add(returnBookGroupBox);

                // Create DataGridView for returned books
                dgvReturnBooks = new Guna2DataGridView
                {
                    Name = "dgvReturnBooks",
                    Size = new Size(returnBookGroupBox.Width, returnBookGroupBox.Height),
                    Location = new Point(0, 15),
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                    ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                    ColumnHeadersHeight = 40,
                    DefaultCellStyle = {
                SelectionBackColor = Color.FromArgb(107, 78, 55),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
                    EnableHeadersVisualStyles = false,
                    GridColor = Color.FromArgb(231, 229, 255),
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    RowTemplate = { Height = 40 },
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default,
                    ThemeStyle = {
                HeaderStyle = {
                    BackColor = Color.FromArgb(101, 70, 49),
                    BorderStyle = DataGridViewHeaderBorderStyle.None,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Height = 40
                },
                RowsStyle = {
                    BackColor = Color.White,
                    BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.Black,
                    Height = 40,
                    SelectionBackColor = Color.FromArgb(231, 229, 255),
                    SelectionForeColor = Color.FromArgb(71, 69, 94)
                }
            }
                };

                // Create columns with consistent names that match what LoadReturnData() will use
                DataGridViewTextBoxColumn ticketIdColumn = new DataGridViewTextBoxColumn();
                ticketIdColumn.Name = "TicketID";
                ticketIdColumn.HeaderText = "Mã phiếu";
                dgvReturnBooks.Columns.Add(ticketIdColumn);

                dgvReturnBooks.Columns.Add("Name", "Name");
                dgvReturnBooks.Columns.Add("Author", "Author");
                dgvReturnBooks.Columns.Add("Type", "Type");
                dgvReturnBooks.Columns.Add("Status", "Status");

                // Create Action column with button
                DataGridViewButtonColumn actionColumn = new DataGridViewButtonColumn();
                actionColumn.Name = "Action";
                actionColumn.HeaderText = "Action";
                actionColumn.Text = "Return";
                actionColumn.UseColumnTextForButtonValue = true;
                actionColumn.FlatStyle = FlatStyle.Flat;
                actionColumn.DefaultCellStyle.BackColor = Color.FromArgb(101, 70, 49);
                actionColumn.DefaultCellStyle.ForeColor = Color.White;
                actionColumn.DefaultCellStyle.SelectionBackColor = Color.FromArgb(120, 90, 70);
                actionColumn.DefaultCellStyle.SelectionForeColor = Color.White;
                dgvReturnBooks.Columns.Add(actionColumn);
                actionColumn.DefaultCellStyle.BackColor = Color.FromArgb(101, 70, 49);
                actionColumn.DefaultCellStyle.ForeColor = Color.White;
                actionColumn.DefaultCellStyle.SelectionBackColor = Color.FromArgb(101, 70, 49); // Same as regular color
                actionColumn.DefaultCellStyle.SelectionForeColor = Color.White;

                // Add this after adding all columns but before the event handler:
                // Prevent header color change on hover
                dgvReturnBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(101, 70, 49);
                dgvReturnBooks.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

                // Modify the cell format event to ensure consistent button styling
                dgvReturnBooks.CellFormatting += (sender, e) => {
                    if (e.ColumnIndex == dgvReturnBooks.Columns["Action"].Index)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(101, 70, 49);
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.SelectionBackColor = Color.FromArgb(101, 70, 49);
                        e.CellStyle.SelectionForeColor = Color.White;
                    }
                };
                // Make all headers and cells center-aligned
                dgvReturnBooks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvReturnBooks.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Add the DataGridView to the GroupBox
                returnBookGroupBox.Controls.Add(dgvReturnBooks);

                // Add the main container to the panel
                panelBook.Controls.Add(containerPanel);

                // Add event handler for cell click - only attach it once here
                dgvReturnBooks.CellClick += DgvReturnBooks_CellClick;

                // Refresh to update the UI
                dgvReturnBooks.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up return DataGridView: " + ex.Message,
                    "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Guna2DataGridView dgvOverdueBooks;
        private void btnOverdue_Click(object sender, EventArgs e)
        {
            lblTime.Visible = false;
            panelBook.Visible = true;
            panelBook.Controls.Clear();
            SetupOverdueDataGridView();

            // Load the overdue data
            LoadOverdueData();
        }
        private void SetupOverdueDataGridView()
        {
            try
            {
                panelBook.Controls.Clear();

                // Create main container panel with rounded corners
                Guna2Panel containerPanel = new Guna2Panel();
                containerPanel.BorderRadius = 15;
                containerPanel.FillColor = Color.White;
                containerPanel.Dock = DockStyle.Fill;
                containerPanel.Margin = new Padding(10);
                containerPanel.Padding = new Padding(20);
                containerPanel.Size = new Size(panelBook.Width - 40, panelBook.Height - 40);
                containerPanel.Location = new Point(20, 20);
                containerPanel.ShadowDecoration.Enabled = true;
                containerPanel.ShadowDecoration.Depth = 5;
                containerPanel.ShadowDecoration.Color = Color.FromArgb(200, 200, 200);

                // Create title for panel
                Label lblOverdue = new Label();
                lblOverdue.Text = "Overdue List";
                lblOverdue.Font = new Font("VNI-Vari", 18F);
                lblOverdue.AutoSize = true;
                lblOverdue.Location = new Point(40, 20);
                lblOverdue.ForeColor = Color.FromArgb(101, 70, 49);
                containerPanel.Controls.Add(lblOverdue);

                // Create timestamp label
                Label lblUpdated = new Label();
                lblUpdated.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                lblUpdated.Font = new Font("Segoe UI", 9F);
                lblUpdated.AutoSize = true;
                lblUpdated.ForeColor = Color.Gray;
                lblUpdated.Location = new Point(containerPanel.Width - 130, 40);
                containerPanel.Controls.Add(lblUpdated);

                // Create GroupBox to contain DataGridView
                Guna2GroupBox overdueGroupBox = new Guna2GroupBox();
                overdueGroupBox.Text = "Overdue Books";
                overdueGroupBox.Font = new Font("VNI-Vari", 16F, FontStyle.Regular);
                overdueGroupBox.ForeColor = Color.FromArgb(101, 70, 49);
                overdueGroupBox.BorderRadius = 15;
                overdueGroupBox.CustomBorderColor = Color.FromArgb(101, 70, 49);
                overdueGroupBox.BorderColor = Color.FromArgb(220, 220, 220);
                overdueGroupBox.FillColor = Color.White;
                overdueGroupBox.Location = new Point(40, 80);
                overdueGroupBox.Size = new Size(containerPanel.Width - 40, containerPanel.Height - 80);
                overdueGroupBox.ShadowDecoration.Enabled = true;
                overdueGroupBox.ShadowDecoration.Depth = 5;
                overdueGroupBox.ShadowDecoration.Color = Color.FromArgb(200, 200, 200);
                overdueGroupBox.Padding = new Padding(10, 40, 10, 10);
                containerPanel.Controls.Add(overdueGroupBox);

                // Create and configure DataGridView
                dgvOverdueBooks = new Guna2DataGridView();
                dgvOverdueBooks.Name = "dgvOverdueBooks";
                dgvOverdueBooks.Location = new Point(0, 15);
                dgvOverdueBooks.Size = new Size(overdueGroupBox.Width, overdueGroupBox.Height);
                dgvOverdueBooks.AllowUserToAddRows = false;
                dgvOverdueBooks.AllowUserToDeleteRows = false;
                dgvOverdueBooks.AllowUserToResizeRows = false;
                dgvOverdueBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvOverdueBooks.BackgroundColor = Color.White;
                dgvOverdueBooks.BorderStyle = BorderStyle.None;
                dgvOverdueBooks.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvOverdueBooks.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvOverdueBooks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dgvOverdueBooks.ColumnHeadersHeight = 40;
                dgvOverdueBooks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(107, 78, 55);
                dgvOverdueBooks.EnableHeadersVisualStyles = false;
                dgvOverdueBooks.GridColor = Color.FromArgb(231, 229, 255);
                dgvOverdueBooks.ReadOnly = true;
                dgvOverdueBooks.RowHeadersVisible = false;
                dgvOverdueBooks.RowTemplate.Height = 40;
                dgvOverdueBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvOverdueBooks.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
                dgvOverdueBooks.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(101, 70, 49);
                dgvOverdueBooks.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
                dgvOverdueBooks.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                dgvOverdueBooks.ThemeStyle.HeaderStyle.ForeColor = Color.White;
                dgvOverdueBooks.ThemeStyle.HeaderStyle.Height = 40;
                dgvOverdueBooks.ThemeStyle.RowsStyle.BackColor = Color.White;
                dgvOverdueBooks.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvOverdueBooks.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 10F);
                dgvOverdueBooks.ThemeStyle.RowsStyle.ForeColor = Color.Black;
                dgvOverdueBooks.ThemeStyle.RowsStyle.Height = 40;
                dgvOverdueBooks.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
                dgvOverdueBooks.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
                dgvOverdueBooks.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgvOverdueBooks.ThemeStyle.HeaderStyle.BackColor;
                dgvOverdueBooks.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

                // Make sure to apply styling after all other settings
                dgvOverdueBooks.Columns.Cast<DataGridViewColumn>().ToList().ForEach(c =>
                {
                    c.HeaderCell.Style.BackColor = Color.FromArgb(101, 70, 49);
                    c.HeaderCell.Style.ForeColor = Color.White;
                    c.HeaderCell.Style.SelectionBackColor = Color.FromArgb(101, 70, 49);
                    c.HeaderCell.Style.SelectionForeColor = Color.White;
                });

                // Center content in all cells
                dgvOverdueBooks.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvOverdueBooks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Add DataGridView to GroupBox before adding columns and data
                overdueGroupBox.Controls.Add(dgvOverdueBooks);

                // Add columns to DataGridView matching the mockup image
                dgvOverdueBooks.Columns.Add("BookName", "Name");
                dgvOverdueBooks.Columns.Add("Author", "Author");
                dgvOverdueBooks.Columns.Add("Category", "Type");
                dgvOverdueBooks.Columns.Add("CheckIn", "Check-in");
                dgvOverdueBooks.Columns.Add("Duration", "Duration");
                dgvOverdueBooks.Columns.Add("Fee", "Fee");

                // Add container panel to main panel
                panelBook.Controls.Add(containerPanel);
                

                // Refresh grid to update UI
                dgvOverdueBooks.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up DataGridView: " + ex.Message, "Setup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadOverdueData()
        {
            try
            {
                // Check if DataGridView is initialized
                if (dgvOverdueBooks == null || dgvOverdueBooks.IsDisposed)
                {
                    MessageBox.Show("DataGridView is not properly initialized.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy ReaderId từ người dùng hiện tại đang đăng nhập
                int currentReaderId = CurrentUsers.ReaderId;

                // Nếu không có CurrentUsers hoặc ReaderId không hợp lệ, thử dùng _currentReaderID
                if (currentReaderId <= 0 && !string.IsNullOrEmpty(_currentReaderID))
                {
                    if (!int.TryParse(_currentReaderID, out currentReaderId))
                    {
                        // Nếu không thể chuyển đổi, hiển thị lỗi
                        MessageBox.Show("Invalid Reader ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Query to get overdue books
                // Thêm điều kiện lọc theo ReaderId của người dùng đang đăng nhập
                string query = @"
SELECT 
    B.TITLE AS BookName,
    A.FULLNAME AS Author,
    C.NAME AS Category,
    BT.BORROWDATE AS CheckInDate,
    CAST(DATEDIFF(day, BT.DUEDATE, GETDATE()) AS VARCHAR) + ' days overdue' AS Duration,
    DATEDIFF(day, BT.DUEDATE, GETDATE()) * 3000 AS Fee
FROM 
    BORROWINGTICKETS BT
    JOIN READERS R ON BT.READERID = R.READERID 
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    GETDATE() > BT.DUEDATE
    AND BT.RETURNDATE IS NULL
    AND BT.READERID = " + currentReaderId + @"
ORDER BY 
    DATEDIFF(day, BT.DUEDATE, GETDATE()) DESC";

                // Execute query
                DataTable dataTable = GetDatabase.Instance.ExecuteQuery(query);

                // Clear existing rows
                dgvOverdueBooks.Rows.Clear();

                // Check if we got any data back
                if (dataTable.Rows.Count == 0)
                {
                    // No data found
                    MessageBox.Show("No overdue books found for reader ID " + currentReaderId, "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Process the data and populate the DataGridView
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        string bookName = row["BookName"].ToString();
                        string author = row["Author"].ToString();
                        string category = row["Category"].ToString();
                        DateTime checkInDate = Convert.ToDateTime(row["CheckInDate"]);
                        string duration = row["Duration"].ToString();
                        decimal fee = Convert.ToDecimal(row["Fee"]);

                        // Add the row to the DataGridView
                        int rowIndex = dgvOverdueBooks.Rows.Add(bookName, author, category,
                            checkInDate.ToString("MM/dd/yyyy"), duration, fee.ToString("N0"));
                    }
                    catch (Exception ex)
                    {
                        // Just log this error but continue processing other rows
                        Console.WriteLine($"Error processing row: {ex.Message}");
                    }
                }

                // Update the timestamp label
                foreach (Control control in panelBook.Controls)
                {
                    if (control is Guna2Panel containerPanel)
                    {
                        foreach (Control panelControl in containerPanel.Controls)
                        {
                            if (panelControl is Label && panelControl.Text.StartsWith("Updated at"))
                            {
                                panelControl.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                                break;
                            }
                        }
                    }
                }

                // Force a refresh
                dgvOverdueBooks.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading overdue data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the complete exception for debugging
                Console.WriteLine(ex.ToString());
            }
        }
        private void SetupHistoryDataGridView()
        {
            try
            {
                panelBook.Controls.Clear();

                // Main container panel with rounded corners
                Guna2Panel containerPanel = new Guna2Panel
                {
                    BorderRadius = 15,
                    FillColor = Color.White,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10),
                    Padding = new Padding(20),
                    Size = new Size(panelBook.Width - 40, panelBook.Height - 40),
                    Location = new Point(20, 20),
                    ShadowDecoration = {
                Enabled = true,
                Depth = 5,
                Color = Color.FromArgb(200, 200, 200)
            }
                };

                // Title label
                Label lblHistory = new Label
                {
                    Text = "Borrowing History",
                    Font = new Font("VNI-Vari", 18F),
                    AutoSize = true,
                    Location = new Point(40, 20),
                    ForeColor = Color.FromArgb(101, 70, 49)
                };
                containerPanel.Controls.Add(lblHistory);

                // Last updated timestamp
                Label lblUpdated = new Label
                {
                    Text = "Updated at " + DateTime.Now.ToString("h:mm tt"),
                    Font = new Font("Segoe UI", 9F),
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Location = new Point(containerPanel.Width - 130, 40)
                };
                containerPanel.Controls.Add(lblUpdated);

                // Group box to contain the DataGridView
                Guna2GroupBox historyGroupBox = new Guna2GroupBox
                {
                    Text = "Borrowing History",
                    Font = new Font("VNI-Vari", 16F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(101, 70, 49),
                    BorderRadius = 15,
                    CustomBorderColor = Color.FromArgb(101, 70, 49),
                    BorderColor = Color.FromArgb(220, 220, 220),
                    FillColor = Color.White,
                    Location = new Point(40, 80),
                    Size = new Size(containerPanel.Width - 80, containerPanel.Height - 100),
                    ShadowDecoration = {
                Enabled = true,
                Depth = 5,
                Color = Color.FromArgb(200, 200, 200)
            },
                    Padding = new Padding(10, 40, 10, 10)
                };
                containerPanel.Controls.Add(historyGroupBox);

                // Create DataGridView for history
                dgvHistory = new Guna2DataGridView
                {
                    Name = "dgvHistory",
                    Size = new Size(historyGroupBox.Width, historyGroupBox.Height),
                    Location = new Point(0, 15),
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                    ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                    ColumnHeadersHeight = 40,
                    DefaultCellStyle = {
                SelectionBackColor = Color.FromArgb(107, 78, 55),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
                    EnableHeadersVisualStyles = false,
                    GridColor = Color.FromArgb(231, 229, 255),
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    RowTemplate = { Height = 40 },
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default,
                    ThemeStyle = {
                HeaderStyle = {
                    BackColor = Color.FromArgb(101, 70, 49),
                    BorderStyle = DataGridViewHeaderBorderStyle.None,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Height = 40
                },
                RowsStyle = {
                    BackColor = Color.White,
                    BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.Black,
                    Height = 40,
                    SelectionBackColor = Color.FromArgb(231, 229, 255),
                    SelectionForeColor = Color.FromArgb(71, 69, 94)
                }
            }
                };

                // Create columns
                dgvHistory.Columns.Add("TicketID", "Mã phiếu");
                dgvHistory.Columns.Add("Name", "Name");
                dgvHistory.Columns.Add("Author", "Author");
                dgvHistory.Columns.Add("Type", "Type");
                dgvHistory.Columns.Add("CheckInDate", "Check-in");
                dgvHistory.Columns.Add("Duration", "Duration");
                dgvHistory.Columns.Add("Status", "Status");

                // Make all headers and cells center-aligned
                dgvHistory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvHistory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // Add the DataGridView to the GroupBox
                historyGroupBox.Controls.Add(dgvHistory);

                // Add the main container to the panel
                panelBook.Controls.Add(containerPanel);

                // Refresh to update the UI
                dgvHistory.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up history DataGridView: " + ex.Message,
                    "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Guna2DataGridView dgvHistory;
        private void LoadHistoryData()
        {
            try
            {
                // Check if DataGridView is initialized
                if (dgvHistory == null || dgvHistory.IsDisposed)
                {
                    MessageBox.Show("DataGridView is not properly initialized.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get ReaderId from currently logged in user
                int currentReaderId = CurrentUsers.ReaderId;

                // If CurrentUsers is null or ReaderId is invalid, try using _currentReaderID
                if (currentReaderId <= 0 && !string.IsNullOrEmpty(_currentReaderID))
                {
                    if (!int.TryParse(_currentReaderID, out currentReaderId))
                    {
                        // If conversion fails, display error
                        MessageBox.Show("Invalid Reader ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Query to get all borrowing history for the user
                string query = @"
SELECT 
    BT.TICKETID,
    B.TITLE AS Name,
    A.FULLNAME AS Author,
    C.NAME AS Category,
    BT.BORROWDATE AS CheckInDate,
    BT.DUEDATE AS DueDate,
    BT.RETURNDATE,
    CASE 
        -- If returned, show return date
        WHEN BT.RETURNDATE IS NOT NULL THEN 'Returned on ' + CONVERT(VARCHAR(10), BT.RETURNDATE, 103)
        -- If overdue, show days overdue
        WHEN GETDATE() > BT.DUEDATE THEN 'Overdue - ' + CAST(DATEDIFF(day, BT.DUEDATE, GETDATE()) AS VARCHAR) + ' days'
        -- If still borrowing and not overdue, show days left
        ELSE CAST(DATEDIFF(day, GETDATE(), BT.DUEDATE) AS VARCHAR) + ' days left'
    END AS Duration,
    CASE 
        -- If returned, show 'Returned' regardless of whether it was overdue
        WHEN BT.RETURNDATE IS NOT NULL THEN 'Returned'
        -- If not returned and overdue, show 'Overdue'
        WHEN GETDATE() > BT.DUEDATE THEN 'Overdue'
        -- If approval status is still 'Chờ duyệt', show 'Pending Approval'
        WHEN BT.APPROVAL_STATUS = 'Chờ duyệt' THEN 'Pending Approval'
        -- Otherwise, if within due date, show 'Borrowing'
        ELSE 'Borrowing'
    END AS Status
FROM 
    BORROWINGTICKETS BT
    JOIN READERS R ON BT.READERID = R.READERID 
    JOIN BOOKS B ON BT.BOOKID = B.BOOKID
    JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
    JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
WHERE 
    BT.READERID = " + currentReaderId + @"
ORDER BY 
    BT.BORROWDATE DESC";

                // Execute query
                DataTable dataTable = GetDatabase.Instance.ExecuteQuery(query);

                // Clear existing rows
                dgvHistory.Rows.Clear();

                // Check if we got any data back
                if (dataTable.Rows.Count == 0)
                {
                    // No data found
                    MessageBox.Show("No borrowing history found for reader ID " + currentReaderId, "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Process the data and populate the DataGridView
                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        int ticketId = Convert.ToInt32(row["TICKETID"]);
                        string bookName = row["Name"].ToString();
                        string author = row["Author"].ToString();
                        string category = row["Category"].ToString();
                        DateTime checkInDate = Convert.ToDateTime(row["CheckInDate"]);
                        string duration = row["Duration"].ToString();
                        string status = row["Status"].ToString();

                        // Add the row to the DataGridView
                        int rowIndex = dgvHistory.Rows.Add(
                            ticketId,
                            bookName,
                            author,
                            category,
                            checkInDate.ToString("dd/MM/yyyy"),
                            duration,
                            status
                        );

                        // Set custom formatting based on status
                        switch (status)
                        {
                            case "Overdue":
                                dgvHistory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                                break;
                            case "Returned":
                                dgvHistory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Green;
                                break;
                            case "Pending Approval":
                                dgvHistory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Orange;
                                break;
                            case "Borrowing":
                                dgvHistory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Blue;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log this error but continue processing other rows
                        Console.WriteLine($"Error processing row: {ex.Message}");
                    }
                }

                // Update the timestamp label
                foreach (Control control in panelBook.Controls)
                {
                    if (control is Guna2Panel containerPanel)
                    {
                        foreach (Control panelControl in containerPanel.Controls)
                        {
                            if (panelControl is Label && panelControl.Text.StartsWith("Updated at"))
                            {
                                panelControl.Text = "Updated at " + DateTime.Now.ToString("h:mm tt");
                                break;
                            }
                        }
                    }
                }

                // Force a refresh
                dgvHistory.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading history data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the complete exception for debugging
                Console.WriteLine(ex.ToString());
            }
        }
        private void HomeBR_Load(object sender, EventArgs e)
        {

        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            lblTime.Visible = false;
            panelBook.Visible = true;
            panelBook.Controls.Clear();
            SetupHistoryDataGridView();
            LoadHistoryData();
        }

        // Helper method to style status cells with appropriate colors


    }
}
