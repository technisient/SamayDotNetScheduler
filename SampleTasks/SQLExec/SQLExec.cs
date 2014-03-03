using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Technisient;

namespace JobsNS
{
    class SQLExec : TaskBase
    {
        public override object Run(object input)
        {
            SqlConnection myConnection = new SqlConnection();
            try
            {
                myConnection = new SqlConnection("user id=username;" +
                                           "password=password;server=serverurl;" +
                                           "Trusted_Connection=yes;" +
                                           "database=database; " +
                                           "connection timeout=30");
                myConnection.Open();

                SqlCommand myCommand = new SqlCommand("INSERT INTO table (Column1, Column2) " +
                                         "Values ('string', 1)", myConnection);

                return myCommand.ExecuteNonQuery();
            }
            finally
            {
                myConnection.Close();
            }
        }
    }
}
