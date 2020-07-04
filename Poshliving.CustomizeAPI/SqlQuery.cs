using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Poshliving.CustomizeAPI
{
    public class SqlQuery : AbstractWebApiBusinessService
    {
        public SqlQuery(Kingdee.BOS.ServiceFacade.KDServiceFx.KDServiceContext context)
            : base(context)
        {

        }

        public string Exec(string sqlQuery)
        {
            SqlConnection conn = new SqlConnection("Data Source=kingdee;Initial Catalog=Poshliving;" +
                "Persist Security Info=True;User ID=reportonly;Password=!ttsh$");
            SqlCommand command = new SqlCommand(sqlQuery, conn);
            conn.Open();

            SqlDataReader reader = command.ExecuteReader();
            
            JObject jobj = new JObject();

            try
            {
                if (reader.HasRows)
                {                    
                    var r = Serialize(reader);
                    string jsonResult = JsonConvert.SerializeObject(r, Formatting.Indented);
                    jobj.Add("success", "true");
                    jobj.Add("count", r.Count());
                    jobj.Add("records", jsonResult);

                    return jobj.ToString();
                }
            }
            catch (Exception ex)
            {
                jobj.Add("Error", ex.Message);
            }

            jobj.Add("success", "false");
            jobj.Add("count", 0);
            return jobj.ToString();
        }

        private IEnumerable<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                                        SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }
    }
}
