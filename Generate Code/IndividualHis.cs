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
using Microsoft.Office.Interop.Excel;

namespace LMS.Generate_Code
{
    public partial class IndividualHis : UserControl
    {
        private string readerId;
        private string readerName;
        public IndividualHis(string readerId, string readerName)
        {
            InitializeComponent();
            this.readerName = readerName;
            this.readerId = readerId;
            grbIndividual.Text = $"List of Borrowed Book by: {readerName}";
        }

        private void IndividualHis_Load(object sender, EventArgs e)
        {
            string query = $@"
            SELECT 
                B.TITLE,
                A.FULLNAME AS [AUTHOR],
                C.NAME AS [CATEGORY],
                BT.BORROWDATE AS [BORROW DATE],
                BT.DUEDATE AS [DUE DATE],
                BT.RETURNDATE AS [RETURN DATE],
                CASE 
                    WHEN BT.[STATUS] = 'Returned' THEN 'Restored'
                    WHEN BT.[STATUS] = 'Borrowing' THEN 'Borrowing'
                    ELSE BT.[STATUS]
                END AS 'STATUS',
                RT.FINEAMOUNT AS 'FINE AMOUNT'
            FROM 
                BORROWINGTICKETS BT
                JOIN BOOKS B ON BT.BOOKID = B.BOOKID
                JOIN CATEGORIES C ON B.CATEGORYID = C.CATEGORYID
                JOIN AUTHORS A ON B.AUTHORID = A.AUTHORID
                LEFT JOIN RETURNTICKETS RT ON BT.TICKETID = RT.TICKETID
            WHERE 
                BT.READERID = '{readerId}'
            ORDER BY 
                BT.BORROWDATE DESC";
            DTLibrary.Instance.LoadList(query, dtgvIndividualHis);


            // Định dạng cột ngày tháng
            dtgvIndividualHis.Columns["BORROW DATE"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dtgvIndividualHis.Columns["DUE DATE"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dtgvIndividualHis.Columns["RETURN DATE"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dtgvIndividualHis.Columns["FINE AMOUNT"].DefaultCellStyle.Format = "N0";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dtgvIndividualHis.Rows.Count == 0)
            {
                MessageBox.Show("Don't have proposed data.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tạo SaveFileDialog để chọn nơi lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Save file Excel",
                FileName = $"ListofBorrowBook of {readerName}"+ DateTime.Now.ToString(" dd-MM-yyyy") + ".xlsx"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Tạo ứng dụng Excel
                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excelApp.Visible = false;
                    Workbook workbook = excelApp.Workbooks.Add();
                    Worksheet worksheet = (Worksheet)workbook.Sheets[1];

                    // Xuất tiêu đề cột
                    for (int i = 0; i < dtgvIndividualHis.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1] = dtgvIndividualHis.Columns[i].HeaderText;
                    }

                    // Xuất dữ liệu từ DataGridView
                    for (int i = 0; i < dtgvIndividualHis.Rows.Count; i++)
                    {
                        for (int j = 0; j < dtgvIndividualHis.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1] = dtgvIndividualHis.Rows[i].Cells[j].Value?.ToString();
                        }
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns.AutoFit();

                    // Lưu file Excel
                    workbook.SaveAs(saveFileDialog.FileName);
                    workbook.Close();
                    excelApp.Quit();

                    // Giải phóng tài nguyên
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                    MessageBox.Show("Data exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
