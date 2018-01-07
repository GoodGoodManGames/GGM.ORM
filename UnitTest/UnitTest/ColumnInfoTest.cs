using System;
using Xunit;
using GGM.ORMTest.Entity;
using System.Reflection;
using GGM.ORM.Attribute;
using GGM.ORM;

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
        public string GetNameTest()
        {
            return columnInfo.Name;
        }

        [Fact]
        public string GetParamNameTest()
        {
            return columnInfo.ParameterName;
        }

        [Fact]
        public string GetParamExpressionTest()
        {
            return columnInfo.ParameterExpression;
        }
    }
}