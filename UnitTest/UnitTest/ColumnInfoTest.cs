using System;
using Xunit;
using GGM.ORMTest.Entity;
using System.Reflection;
using GGM.ORM.Attribute;
using GGM.ORM;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace GGM.ORMTest.UnitTest
{
    public class ColumnInfoTest
    {
        public ColumnInfoTest()
        {
            type = typeof(Person);
            property = type.GetProperty("ID");
            columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            columnInfo = new ColumnInfo(property, columnAttribute);
        }

        Type type;
        PropertyInfo property;
        ColumnAttribute columnAttribute;
        ColumnInfo columnInfo;

       
        [Fact]
        public void GetParamExpressionTest()
        {
            var expression = columnInfo.ParameterExpression;
            var resultExpression = string.Concat(property.Name.ToLower(), " = @", property.Name.ToLower());
            Assert.Matches(expression, resultExpression);
        }
    }
}