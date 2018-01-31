using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace GGM.ORMTest.EntityManagerFactory
{
    public class MysqlManagerFactory : GGM.ORM.EntityManagerFactory
    {
        protected override DbConnection CreateDBConnection()
        {
            return new MySqlConnection("Server=203.253.76.178;Database=practice;Uid=ggm_black;pwd=songji710;");
        }
    }
}
