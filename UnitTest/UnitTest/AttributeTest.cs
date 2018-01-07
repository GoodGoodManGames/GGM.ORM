using System;
using Xunit;
using GGM.ORMTest.Entity;
using GGM.ORM.QueryBuilder;
using GGM.ORM.Attribute;

namespace GGM.ORMTest.UnitTest
{
    public class AttributeTest
    {
        public AttributeTest()
        {
            columnAttribute = new ColumnAttribute("TestColumnAttribute");
            entityAttribute = new EntityAttribute("TestEntityAttribute");
            primaryKeyAttribute = new PrimaryKeyAttribute();

        }
        ColumnAttribute columnAttribute;
        EntityAttribute entityAttribute;
        PrimaryKeyAttribute primaryKeyAttribute;

        [Fact]
        public string GgetNameOfColumnAttributeTest()
        {
            return columnAttribute.Name;
        }

        [Fact]
        public string GetNameOfentityAttributeTest()
        {
            return entityAttribute.Name;
        }
    }
}