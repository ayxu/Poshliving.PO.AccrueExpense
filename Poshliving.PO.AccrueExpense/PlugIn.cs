using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poshliving.PO.AccrueExpense
{
    [Description("计算采购费用暂估")]
    public class PlugIn : AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            //价税合计发生变化
            if (e.Field.FieldName.Equals("FBILLALLAMOUNT", StringComparison.InvariantCultureIgnoreCase))
            {
                //关税，增值税
                double goodsamt = 0, freight = 0, custome = 0, vat = 0, 
                       clearance = 0, agencyfee = 0, bankfee = 0;
                
                //获取单据体实体元数据描述信息
                Entity entity = this.View.BusinessInfo.GetEntity("FPOOrderEntry");
                DynamicObjectCollection collecions =
                        this.View.Model.GetEntityDataObject(entity);
                foreach (DynamicObject obj in collecions)
                {
                    //货款
                    goodsamt = Convert.ToDouble(obj["ALLAMOUNT"]);
                    //运费 = 货款 * 0.08
                    freight = freight + goodsamt * 0.08;
                    //关税 = （货款+运费）* 货品税率
                    custome = custome + goodsamt * 1.08 * 
                        GetMultiple(obj["MaterialId_id"].ToString()); //此处输入实体属性名，不是字段名
                    //增值税 = （货款 + 运费 + 关税）*13%
                    vat = vat + (goodsamt +
                                 goodsamt * 0.08 +
                                 goodsamt * 1.08 * GetMultiple(obj["MaterialId_id"].ToString())
                                ) * 0.13;
                    clearance = clearance + goodsamt * 0.05;
                    agencyfee = agencyfee + goodsamt * 0.01;
                    bankfee = bankfee + goodsamt * 0.005;
                }

                this.View.Model.SetValue("F_ZCUSTOME", custome);
                this.View.Model.SetValue("F_ZTAX", vat);
                this.View.Model.SetValue("F_ZCLEARANCE", clearance);
                this.View.Model.SetValue("F_ZAGENCYFEE", agencyfee);
                this.View.Model.SetValue("F_ZBANKFEE", bankfee);
                this.View.Model.SetValue("F_ZOCEANFREIGHT", freight);
            }
        }

        private double GetMultiple(string matid)
        {
            string connetionString = @"Data Source=localhost;Initial Catalog=AIS20190516233807;User ID=reportonly;Password=!ttsh$";
            using (DbContext ctx = new DbContext(connetionString))
            {
                //dc.Connection.Open();
                string multiple = ctx.Database.SqlQuery<string>(@"select mgl.FDESCRIPTION from T_BD_MATERIAL ma 
                       left outer join T_BD_MATERIALGROUP mg on mg.FID = ma.FMATERIALGROUP 
                       left outer join T_BD_MATERIALGROUP_L mgl on mg.FID = mgl.FID and mgl.FLOCALEID = 2052 
                       WHERE ma.FMATERIALID = {0}", matid).FirstOrDefault();

                return string.IsNullOrEmpty(multiple) ? Convert.ToDouble(0.09) : Convert.ToDouble(multiple);
            }            
        }
    }
}
