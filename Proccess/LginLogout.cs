using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS.Proccess;

namespace LMS.Database
{
    public class LginLgout
    {
        private static LginLgout instance;
        public static LginLgout Instance
        {
            get { if (instance == null) instance = new LginLgout(); return instance; }
            private set { instance = value; }
        }
        public string LgManage(string email, string password)
        {
            string query = "SELECT * FROM LIBRARIANS WHERE Email = N'" + email + "' AND [Password] =N'" + password + "' ";
            DataTable result = GetDatabase.Instance.ExecuteQuery(query);
            if (result.Rows.Count > 0)
            {
                // Return the role of the logged-in user
                return result.Rows[0]["Role"].ToString();
            }
            // Return null if login fails
            return null;
        }
    }
}

