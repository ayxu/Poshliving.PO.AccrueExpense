using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeKingdeeApi
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlQuery = "select userid, name, kduserid from Poshliving.dbo.Crm2KDUserMap where userid = 516091";
            Console.WriteLine(Exec(sqlQuery));
        }

        private static string Exec(string sqlQuery)
        {
            SqlConnection conn = new SqlConnection("Data Source=kingdee;Initial Catalog=Poshliving;" +
                "Persist Security Info=True;User ID=reportonly;Password=!ttsh$");
            SqlCommand command = new SqlCommand(sqlQuery, conn);
            conn.Open();

            SqlDataReader reader = command.ExecuteReader();

            JObject jobj = new JObject();
            jobj.Add("Success", "false");
            string jsonResult = jobj.ToString();

            try
            {
                if (reader.HasRows)
                {
                    var r = Serialize(reader);
                    jsonResult = JsonConvert.SerializeObject(r, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                jobj.Add("Error", ex.Message);
                jsonResult = jobj.ToString();
            }

            return jsonResult;
        }

        private static IEnumerable<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        private static Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                        SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }
    }
}
