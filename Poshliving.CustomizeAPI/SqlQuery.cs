using Kingdee.BOS.Core.Metadata.Util;
using Kingdee.BOS.JSON;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Poshliving.CustomizeAPI
{
    public class SqlQuery : AbstractWebApiBusinessService
    {
        private class QueryResult
        {
            public Boolean success { get; set; }
            [JsonProperty("count")]
            public int RecordCount { get; set; }
            public IEnumerable<Dictionary<string, object>> records { get; set; }
        }

        public SqlQuery(Kingdee.BOS.ServiceFacade.KDServiceFx.KDServiceContext context)
            : base(context)
        {

        }
        private static string connectionString = "Data Source=kingdee;Initial Catalog=Poshliving;" +
                "Persist Security Info=True;User ID=reportonly;Password=!ttsh$";
        public string Exec(string sqlQuery)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sqlQuery, conn);
            conn.Open();

            SqlDataReader reader = command.ExecuteReader();
            
            JObject jobj = new JObject();

            try
            {
                if (reader.HasRows)
                {                    
                    var r = Serialize(reader);

                    QueryResult qresult = new QueryResult();
                    qresult.success = true;
                    qresult.RecordCount = r.Count();
                    qresult.records = r;

                    return JsonConvert.SerializeObject(qresult, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                jobj.Add("Error", ex.Message);
            }

            JArray ja = new JArray();
            jobj.Add("success", "false");
            jobj.Add("count", 0);
            jobj.Add("records", ja);
            return jobj.ToString();
        }
        public string AddWarehouseToAccountingRange(string dbname, string acctRangeId, string stockId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            string sql = "select top 1 [FACCTGRANGEID], [FENTRYID], [FSTOCKID], [FSTOCKORGID], [FOWNERID], [FSEQ], [FOwnerTypeId] " +
                " from " + dbname + ".dbo.T_HS_ACCTGRANGEENTRY" +
                " where FACCTGRANGEID = " + acctRangeId + " order by FSEQ desc";

            SqlCommand command = new SqlCommand(sql, conn);
            conn.Open();
            SqlDataReader reader = command.ExecuteReader();

            JObject jobj = new JObject();
            long fentryid = 0;
            long fseq = 0;

            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        fentryid = long.Parse(reader["FENTRYID"].ToString()) + 1;
                        fseq = long.Parse(reader["FSEQ"].ToString()) + 1;
                    }

                    sql = "insert into " + dbname + ".dbo.T_HS_ACCTGRANGEENTRY " +
                        "([FACCTGRANGEID], [FENTRYID], [FSTOCKID], [FSTOCKORGID], [FOWNERID], [FSEQ], [FOwnerTypeId]) values " +
                        "(" + acctRangeId + ", " 
                        + fentryid.ToString() + ", "
                        + stockId + ",100006, 100006," 
                        + fseq.ToString() + ", " 
                        + "'BD_OwnerOrg')";
                    command.CommandText = sql;
                    reader.Close();

                    if (command.ExecuteNonQuery() == 1)
                    {
                        jobj.Add("success", "true");                        
                    }
                    else
                    {
                        jobj.Add("success", "false");
                        jobj.Add("message", "unknow error.");
                    }
                }
            }
            catch (Exception ex)
            {
                jobj.Add("success", "false");
                jobj.Add("message", ex.Message);
            }

            return jobj.ToString();
        }
        public string UpdateLogisticsStatus(string updateJson)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            JObject jobjReturn = new JObject();
            // JArray jArray = JArray.Parse(updateJson);
            // string sql = "";

            try
            {
                //foreach (JObject jObj in jArray)
                //{
                //    sql = sql + String.Format("update PoshDev1014.dbo.t_PUR_POENTRYDELIPLAN set " +
                //          "F_ZBAOSHI_LOGISTICSTATUS = '{0}', " +
                //          "F_ZBAOSH_LOGSTCHANGEDATE = '{1}'," +
                //          "F_ZBAOSHI_FACTORYOC = '{2}'," +
                //          "F_ZBAOSHI_REMARKS = '{3}'" +
                //          " where FDETAILID = {4}",
                //          jObj["Status"].ToString(),
                //          jObj["ChangeDate"].ToString(),
                //          jObj["OC"].ToString(),
                //          jObj["Remarks"].ToString(),
                //          jObj["DetailId"].ToString()) + ";";
                //}

                //command.CommandText = sql;
                SqlCommand command = new SqlCommand(updateJson, conn);
                if (command.ExecuteNonQuery() >= 1)
                {
                    jobjReturn.Add("success", "true");
                }
                else
                {
                    jobjReturn.Add("success", "false");
                    jobjReturn.Add("message", "unknow error.");
                }
            }
            catch (Exception ex)
            {
                jobjReturn.Add("success", "false");
                jobjReturn.Add("message", ex.Message);
            }

            return jobjReturn.ToString();
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
