using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poshliving.Quotation.ChangeLineHeight
{
    public class ChangeRowHeigth : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            this.View.GetControl<EntryGrid>("FQUOTATIONENTRY").SetRowHeight(80);
            base.AfterBindData(e);
        }
    }
}
