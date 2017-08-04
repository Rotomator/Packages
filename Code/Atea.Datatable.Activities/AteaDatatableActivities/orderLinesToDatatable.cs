using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.ComponentModel;

namespace AteaDatatableActivities
{
    public class orderLinesToDatatable : CodeActivity
    {
        [Category("InputString")]
        [RequiredArgument]
        public InArgument<String> InputString { get; set; }

        [Category("Delimiter")]
        [RequiredArgument]
        public InArgument<String> Delimiter { get; set; }

        [Category("Output")]
        public OutArgument<System.Data.DataTable> dtOutput { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var inputString = InputString.Get(context);
            var delimiter = Delimiter.Get(context);

            var strArray = inputString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Declare DataTable object.
            var dt = new System.Data.DataTable();

            // Add columns to the DataTable
            dt.Columns.Add("LINENUMBER", typeof(String));
            dt.Columns.Add("PARTNUMBER", typeof(String));
            dt.Columns.Add("DESCRIPTION", typeof(String));
            dt.Columns.Add("ORDERQTY", typeof(String));
            dt.Columns.Add("UNITPRICE", typeof(String));

            // Add strings from string array to datatable
            foreach (string str in strArray)
            {
                var drow = dt.NewRow();   // Here you will get an actual instance of a DataRow

                var strArrayTemp = str.Split(new string[] { delimiter }, StringSplitOptions.None);

                drow[0] = strArrayTemp[0];
                drow[1] = strArrayTemp[1];
                drow[2] = strArrayTemp[2];
                drow[3] = strArrayTemp[3];
                drow[4] = strArrayTemp[4];

                dt.Rows.Add(drow);             // Don't forget to add the row to the DataTable.             
            }

            dtOutput.Set(context, dt);
        }
    }
}
