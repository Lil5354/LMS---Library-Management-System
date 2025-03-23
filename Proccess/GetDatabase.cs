using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMS.Proccess
{
    public class GetDatabase
    {
        private static GetDatabase instance;
        private string connectionString = "Data Source=LAPTOP-T5G4R7PV\\SQLEXPRESS01;Initial Catalog=LIBRARYM;Integrated Security=True";

        public static GetDatabase Instance
        {
            get { if (instance == null) instance = new GetDatabase(); return GetDatabase.instance; }
            private set { GetDatabase.instance = value; }
        }

        private GetDatabase() { }

        //Lấy dữ liệu xuống
        public DataTable ExecuteQuery(string query, object[] parameter = null)
        {
            DataTable data = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(data);
                connection.Close();
            }
            return data;
        }
        //Không lấy thông tin, chỉ lấy giá trị dòng thành công (Insert, Update)
        public int ExecuteNonQuery(string query, object[] parameter = null)
        {
            int data = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }
                data = command.ExecuteNonQuery();
                connection.Close();
            }
            return data;
        }
        //Lấy giá trị trả về (Khi dùng Select count *)
        public object ExecuteScalar(string query, object[] parameter = null)
        {
            object data = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }
                data = command.ExecuteScalar();
                connection.Close();
            }
            return data;
        }
        public void LoadDataToComboBox(string query, ComboBox comboBox)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader[0].ToString()); // Add the first column value
                        }
                    }
                }
                connection.Close();
            }
        }
        public void LoadDataToTextBox(string query, Guna.UI2.WinForms.Guna2TextBox textBox, object[] parameter = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameter != null)
                    {
                        string[] listPara = query.Split(' ');
                        int i = 0;
                        foreach (string item in listPara)
                        {
                            if (item.Contains('@'))
                            {
                                command.Parameters.AddWithValue(item, parameter[i]);
                                i++;
                            }
                        }
                    }
                    // Thực thi truy vấn và lấy kết quả
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        // Gán giá trị vào Guna2TextBox
                        textBox.Text = result.ToString();
                    }
                    else
                    {
                        // Nếu không có kết quả, xóa nội dung Guna2TextBox
                        textBox.Text = string.Empty;
                    }
                }
                connection.Close();
            }
        }
        public List<KeyValuePair<string, double>> GetChartData(string query, object[] parameter = null)
        {
            var chartData = new List<KeyValuePair<string, double>>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameter[i]);
                            i++;
                        }
                    }
                }
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = reader[0].ToString(); // Cột đầu tiên: X (danh mục hoặc tên)
                        double value = Convert.ToDouble(reader[1]); // Cột thứ hai: Y (giá trị)
                        chartData.Add(new KeyValuePair<string, double>(key, value));
                    }
                }
                connection.Close();
            }
            return chartData;
        }

    }
}
