using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.WebApi.ServicesStub;

namespace CustomizeKingdeeApi
{
    class Program
    {
        private static string connectionString = "Data Source=kingdee;Initial Catalog=Poshliving;" +
                "Persist Security Info=True;User ID=reportonly;Password=!ttsh$";

        static void Main(string[] args)
        {
            //string sqlQuery = "select MTL_L.FSPECIFICATION product, MTL_L.FNAME productName, stk_L.FNAME warehouseName, inv.FBASEQTY qty" +
            //            " from AIS20190516233807.dbo.T_STK_INVENTORY inv" +
            //            " left outer join AIS20190516233807.dbo.T_BD_STOCK stk on inv.FSTOCKID = stk.FSTOCKID" +
            //            " left outer join AIS20190516233807.dbo.T_BD_STOCK_L stk_L on stk.FSTOCKID = stk_L.FSTOCKID and stk_L.FLOCALEID = 2052" +
            //            " left outer join AIS20190516233807.dbo.T_BD_MATERIAL MTL on inv.FMATERIALID = MTL.FMATERIALID" +
            //            " left outer join AIS20190516233807.dbo.T_BD_MATERIAL_L MTL_L on MTL.FMATERIALID = MTL_L.FMATERIALID and MTL_L.FLOCALEID = 2052" +
            //            " where inv.FBASEQTY <> 0" +
            //            " and stk.FNUMBER like '01%'" +
            //            " order by MTL_L.FSPECIFICATION,  stk_L.FNAME";
            //Console.WriteLine(Exec1(sqlQuery));
            
            string strJson = @"[
                   {
                      'DetailId': 118051,
                      'Status': '5ff429e759f8d6',
                      'ChangeDate': '2021-01-11T16:00:00.000Z',
                      'OC': 'OC21001',
                      'Remarks': 'excel update'
                   },
                   {
                      'DetailId': 118063,
                      'Status': '5ff429bf59f8d2',
                      'ChangeDate': '2021-01-05T16:00:00.000Z',
                      'OC': 'OC21002',
                      'Remarks': 'excel update'
                   }
                ]";
            //测试方法 UpdateLogisticsStatus，需要先 static 修饰            
            Console.WriteLine(Poshliving.CustomizeAPI.SqlQuery.UpdateLogisticsStatus(strJson));
            //Console.WriteLine(AddWarehouseToAccountingRange("384117"));
        }

        private static string AddWarehouseToAccountingRange(string stockId)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            string sql = "select top 1 [FACCTGRANGEID], [FENTRYID], [FSTOCKID], [FSTOCKORGID], [FOWNERID], [FSEQ], [FOwnerTypeId] " +
                " from PoshTest200514.dbo.T_HS_ACCTGRANGEENTRY" +
                " where FACCTGRANGEID = 113933 order by FSEQ desc";

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

                    sql = "insert into PoshTest200514.dbo.T_HS_ACCTGRANGEENTRY " +
                        "([FACCTGRANGEID], [FENTRYID], [FSTOCKID], [FSTOCKORGID], [FOWNERID], [FSEQ], [FOwnerTypeId]) values " +
                        "(113933, "
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
        private class QueryResult
        {
            public Boolean success { get; set; }
            [JsonProperty("count")]
            public int RecordCount { get; set; }
            public IEnumerable<Dictionary<string, object>> records { get; set; }
        }
        private static string Exec1(string sqlQuery)
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

            jobj.Add("success", "false");
            jobj.Add("count", 0);
            return jobj.ToString();
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
