using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LMS.Proccess
{
    public class DTLibrary
    {
        private static DTLibrary instance;
        public static DTLibrary Instance
        {
            get { if (instance == null) instance = new DTLibrary(); return instance; }
            private set { instance = value; }
        }
        private DTLibrary() { }
        public void AutoBindColumns(DataGridView dgv, DataTable dataTable)
        {
            foreach (DataGridViewColumn dgvColumn in dgv.Columns)
            {
                if (dataTable.Columns.Contains(dgvColumn.HeaderText))
                {
                    dgvColumn.DataPropertyName = dgvColumn.HeaderText;
                }
            }
        }
        public void LoadList(string query, DataGridView dtgv)
        {
            try
            {
                DataTable data = GetDatabase.Instance.ExecuteQuery(query);
                AutoBindColumns(dtgv, data);
                dtgv.DataSource = data;
                dtgv.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account list: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int InsertLibrarian(string fullName, string email, string phone, string dob, string role)
        {

            string query = "INSERT INTO LIBRARIANS " +
        "( FULLNAME, EMAIL, PHONE, DOB, [ROLE]) VALUES  (N'" + fullName + "','" + email + "','" + phone + "','" + dob + "','" + role + "' )";
            int result = GetDatabase.Instance.ExecuteNonQuery(query);
            if (result > 0)
            {
                return result;
            }
            // Return null if function false
            return 0;
        }

        // Phương thức cập nhật trạng thái thủ thư
        public int UpdateLibrarianStatus(string id, int isActive)
        {
            string query = "UPDATE LIBRARIANS SET STATUS = "+isActive+" WHERE IDSTAFF = '"+id+"'"; 
            int result = GetDatabase.Instance.ExecuteNonQuery(query);
            if (result > 0)
            {
                return result;
            }
            return 0;
        }

        // Phương thức cập nhật thông tin thủ thư
        public int UpdateLibrarian(string id, string fullName, string email, string phone, string dob, string role)
        {
            string query = "UPDATE LIBRARIANS SET FULLNAME = N'" + fullName + "', Email = '" + email + "',PHONE = '" + phone + "',DOB = '" + dob + "',[Role] = '" + role + "'WHERE IDSTAFF = N'" + id + "' ";
            int result = GetDatabase.Instance.ExecuteNonQuery(query);
            if (result > 0)
            {
                return result;
            }
            // Return null if function false
            return 0;


        }
    }
}
