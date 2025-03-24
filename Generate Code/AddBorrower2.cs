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
    public partial class AddBorrower2 : UserControl
    {
        public AddBorrower2()
        {
            InitializeComponent();
        }

        private void AddBorrower2_Load(object sender, EventArgs e)
        {
            string query = @"
        SELECT 
            BT.TICKETID AS [ID],
			R.FULLNAME AS [READER NAME],
			B.TITLE AS [TITLE],
			R.PHONE AS [PHONE],
			R.EMAIL AS [EMAIL],
            BT.BORROWDATE AS [BORROW DATE],
            BT.DUEDATE AS [DUE DATE]
			
        FROM 
            BORROWINGTICKETS BT
            INNER JOIN BOOKS B ON BT.BOOKID = B.BOOKID
            INNER JOIN READERS R ON BT.READERID = R.READERID
        WHERE 
            BT.[APPROVAL_STATUS] = 'Waiting';";

            // Load dữ liệu vào DataGridView
            DTLibrary.Instance.LoadList(query, dtgvLBT);
        }

        private void btnClose_Click(object sender, EventArgs e)
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

        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (dtgvLBT.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to approve.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int ticketID = Convert.ToInt32(dtgvLBT.SelectedRows[0].Cells["ID"].Value);
            DialogResult result = MessageBox.Show("Are you sure you want to approve this ticket?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string idstaff = LoginInfo.StaffID;
                string updateQuery = $@"
                UPDATE BORROWINGTICKETS 
                SET [APPROVAL_STATUS] = 'Approve', LIBRARIANID = '{idstaff}'
                WHERE TICKETID = {ticketID};";

                int rowsAffected = GetDatabase.Instance.ExecuteNonQuery(updateQuery);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Ticket approved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AddBorrower2_Load(sender, e);
                }
                else
                {
                    MessageBox.Show("Failed to approve the ticket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnDecline_Click(object sender, EventArgs e)
        {
            if (dtgvLBT.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to decline.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int ticketID = Convert.ToInt32(dtgvLBT.SelectedRows[0].Cells["ID"].Value);
            DialogResult result = MessageBox.Show("Are you sure you want to decline this ticket?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string idstaff = LoginInfo.StaffID;
                string updateQuery = $@"
            UPDATE BORROWINGTICKETS 
            SET [APPROVAL_STATUS] = 'Decline', LIBRARIANID = '{idstaff}'
            WHERE TICKETID = {ticketID};";

                int rowsAffected = GetDatabase.Instance.ExecuteNonQuery(updateQuery);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Ticket declined successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AddBorrower2_Load(sender, e); 
                }
                else
                {
                    MessageBox.Show("Failed to decline the ticket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
