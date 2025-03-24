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
    public partial class ListEmployee : UserControl
    {
        private int index = -1;
        private string query = "";

        public ListEmployee()
        {
            InitializeComponent();
            LoadLibrarians();
        }
        private void LoadLibrarians()
        {
            query = @"SELECT 
                        IDSTAFF AS ID, 
                        FULLNAME AS [FULLNAME], 
                        CONVERT(VARCHAR(10), DOB, 103) AS [D.O.B], 
                        EMAIL, 
                        PHONE, 
                        [ROLE], 
                        CASE WHEN STATUS = 1 THEN 'ON' ELSE 'OFF' END AS STATUS 
                      FROM LIBRARIANS 
                      ORDER BY [STATUS] DESC";

            DTLibrary.Instance.LoadList(query, dtgvListE);
            ApplyStatusColors();
        }
        private void ApplyStatusColors()
        {
            foreach (DataGridViewRow row in dtgvListE.Rows)
            {
                if (row.Cells["STATUS"].Value != null)
                {
                    string status = row.Cells["STATUS"].Value.ToString();
                    DataGridViewButtonCell statusCell = row.Cells["STATUS"] as DataGridViewButtonCell;

                    if (statusCell != null)
                    {
                        if (status == "ON")
                        {
                            statusCell.Style.BackColor = Color.Green;
                            statusCell.Style.ForeColor = Color.White;
                        }
                        else
                        {
                            statusCell.Style.BackColor = Color.Red;
                            statusCell.Style.ForeColor = Color.White;
                        }
                    }
                }
            }
        }
        private void dtgvListE_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                index = e.RowIndex;
                DataGridViewRow row = dtgvListE.Rows[e.RowIndex];
                txtFullName.Text = row.Cells["FULLNAME"].Value?.ToString();
                dtpDOB.Text = row.Cells["DATEOFBIRTH"].Value?.ToString();
                txtEmail.Text = row.Cells["EMAIL"].Value?.ToString();
                txtPhone.Text = row.Cells["PHONE"].Value?.ToString();
                cbRole.Text = row.Cells["ROLE"].Value?.ToString();
            }
        }

        private void dtgvListE_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dtgvListE.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                DialogResult dialog = MessageBox.Show("Do you really want to change this librarian's status?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialog == DialogResult.Yes)
                {
                    DataGridViewRow selectedRow = dtgvListE.Rows[e.RowIndex];
                    string currentStatus = selectedRow.Cells["STATUS"].Value.ToString();
                    string id = selectedRow.Cells["ID"].Value.ToString();

                    if (currentStatus == "OFF")
                    {
                        selectedRow.Cells["STATUS"].Value = "ON";
                        selectedRow.Cells["STATUS"].Style.BackColor = Color.Green;
                        selectedRow.Cells["STATUS"].Style.ForeColor = Color.White;

                        if (DTLibrary.Instance.UpdateLibrarianStatus(id, 1) > 0)
                        {
                            MessageBox.Show("Librarian activated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else if (currentStatus == "ON")
                    {
                        selectedRow.Cells["STATUS"].Value = "OFF";
                        selectedRow.Cells["STATUS"].Style.BackColor = Color.Red;
                        selectedRow.Cells["STATUS"].Style.ForeColor = Color.White;

                        if (DTLibrary.Instance.UpdateLibrarianStatus(id, 0) > 0)
                        {
                            MessageBox.Show("Librarian deactivated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(cbRole.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate email format
            try
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                if (addr.Address != txtEmail.Text)
                {
                    throw new FormatException();
                }
            }
            catch
            {
                MessageBox.Show("Please enter a valid email address.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate phone number
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtPhone.Text, @"^[0-9]{10,15}$"))
            {
                MessageBox.Show("Please enter a valid phone number (10-15 digits).", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validate date of birth (at least 18 years old)
            if (dtpDOB.Value > DateTime.Now.AddYears(-18))
            {
                MessageBox.Show("Librarian must be at least 18 years old.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            dtpDOB.Value = DateTime.Now.AddYears(-20); // Default to 20 years old
            cbRole.SelectedIndex = -1;
        }
        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadLibrarians();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                try
                {
                    string fullName = txtFullName.Text;
                    string email = txtEmail.Text;
                    string phone = txtPhone.Text;
                    string role = cbRole.Text;
                    DateTime dob = dtpDOB.Value;
                    string dobString = dob.ToString("yyyy-MM-dd");

                    if (DTLibrary.Instance.InsertLibrarian(fullName, email, phone, dobString, role) > 0)
                    {
                        MessageBox.Show("Librarian added successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                        LoadLibrarians();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add librarian.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding librarian: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (index == -1)
            {
                MessageBox.Show("Please select a librarian to update.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ValidateInputs())
            {
                try
                {
                    DataGridViewRow row = dtgvListE.Rows[index];
                    string id = row.Cells["ID"].Value.ToString();
                    string fullName = txtFullName.Text;
                    string email = txtEmail.Text;
                    string phone = txtPhone.Text;
                    string role = cbRole.Text;
                    DateTime dob = dtpDOB.Value;
                    string dobString = dob.ToString("yyyy-MM-dd");

                    if (DTLibrary.Instance.UpdateLibrarian(id, fullName, email, phone, dobString, role) > 0)
                    {
                        MessageBox.Show("Librarian updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                        LoadLibrarians();
                        index = -1;
                    }
                    else
                    {
                        MessageBox.Show("Failed to update librarian.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating librarian: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
