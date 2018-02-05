using System.Data.Common;
using GGM.Application.Attribute;
using GGM.Context;
using GGM.Context.Attribute;
using MySql.Data.MySqlClient;

namespace GGM.ORM.MySql
{
    [Managed(ManagedType.Singleton)]
    public class MySQLManagerFactory : EntityManagerFactory
    {
        [Config("ORM.Host")] public string Host { get; set; }
        [Config("ORM.DataBase")] public string Database { get; set; }
        [Config("ORM.User")] public string User { get; set; }
        [Config("ORM.Password")] public string Password { get; set; }


        protected override DbConnection CreateDBConnection() =>
            new MySqlConnection($"Server={Host};Database={Database};Uid={User};pwd={Password};SslMode=none");
    }
}