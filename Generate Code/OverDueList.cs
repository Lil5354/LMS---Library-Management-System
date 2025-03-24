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
    public partial class OverDueList : UserControl
    {
        public OverDueList()
        {
            InitializeComponent();
        }

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value;

            // Query để lấy danh sách các cuốn sách đã bị mượn quá hạn
            string query = $@"
        SELECT 
            B.TITLE AS [TITLE],
            R.FULLNAME AS [READER NAME],
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
            BT.[STATUS] = 'Borrowing' AND BT.DUEDATE < GETDATE() AND BT.DUEDATE >= '{startDate.ToString("yyyy-MM-dd")}';";

            // Load dữ liệu vào DataGridView
            DTLibrary.Instance.LoadList(query, dtgvLateReturns);
            var data = GetDatabase.Instance.LoadData(query);
            decimal totalFineAmount = data.AsEnumerable().Sum(row => Convert.ToDecimal(row["FINE AMOUNT"]));
            lblTotalFineAmount.Text = $"{totalFineAmount:N0}VND";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dtgvLateReturns.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tạo SaveFileDialog để chọn nơi lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Save an Excel File",
                FileName = "LateReturns.xlsx"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Tạo một ứng dụng Excel
                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excelApp.Visible = false;
                    Workbook workbook = excelApp.Workbooks.Add();
                    Worksheet worksheet = (Worksheet)workbook.Sheets[1];

                    // Xuất tiêu đề cột
                    for (int i = 1; i <= dtgvLateReturns.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i] = dtgvLateReturns.Columns[i - 1].HeaderText;
                    }

                    // Xuất dữ liệu từ DataGridView
                    for (int i = 0; i < dtgvLateReturns.Rows.Count; i++)
                    {
                        for (int j = 0; j < dtgvLateReturns.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1] = dtgvLateReturns.Rows[i].Cells[j].Value?.ToString();
                        }
                    }

                    // Lưu file Excel
                    workbook.SaveAs(saveFileDialog.FileName);
                    workbook.Close();
                    excelApp.Quit();

                    MessageBox.Show("Data exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
