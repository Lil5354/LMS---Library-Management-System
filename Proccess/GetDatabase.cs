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
                        double value = reader[1] == DBNull.Value ? 0 : Convert.ToDouble(reader[1]);

                        chartData.Add(new KeyValuePair<string, double>(key, value));
                    }
                }
                connection.Close();
            }
            return chartData;
        }
        public DataTable GetDataTable(string query, object[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                // Xử lý tham số giống như trong GetChartData
                if (parameters != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            command.Parameters.AddWithValue(item, parameters[i]);
                            i++;
                        }
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }

                connection.Close();
            }

            return dataTable;
        }
        public List<Dictionary<string, object>> LoadData(string query)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Mở kết nối
                    connection.Open();

                    // Tạo và thực thi lệnh SQL
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Đọc dữ liệu từ SqlDataReader
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();

                                // Lặp qua các cột và thêm vào Dictionary
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object value = reader.GetValue(i);
                                    row[columnName] = value;
                                }

                                // Thêm dòng dữ liệu vào danh sách kết quả
                                result.Add(row);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    Console.WriteLine("Error loading data: " + ex.Message);
                }
                finally
                {
                    // Đảm bảo kết nối được đóng
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }

            return result;
        }
   
    }
}
