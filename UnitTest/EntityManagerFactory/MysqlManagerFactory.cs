using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace GGM.ORMTest.EntityManagerFactory
{
    public class MysqlManagerFactory : GGM.ORM.EntityManagerFactory
    {
        public MysqlManagerFactory(string dbOptions) : base(dbOptions)
        {
        }

        protected override DbConnection CreateDBConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
