using System;
using Xunit;
using GGM.ORMTest.Entity;
using GGM.ORM.QueryBuilder;
using GGM.ORM;
using System.Linq;
using System.Reflection;
using GGM.ORM.Attribute;

namespace GGM.ORMTest.UnitTest
{
    public class DefaultQueryBuilderTest
    {
        public DefaultQueryBuilderTest()
        {
            queryBuilder = new DefaultQueryBuilder<Person>();
            withID = new Entity.Person();
            withID.ID = 3;
            withID.Name = "withID";
            withID.Address = "Gangnam";
            withID.Email = "yesyes@naver.com";
            withID.Age = 29;

            type = withID.GetType();
            propertyInfos = type.GetProperties();
            columnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
        }

        DefaultQueryBuilder<Person> queryBuilder;
        Person withID;
        ColumnInfo[] columnInfos;
        Type type;
        PropertyInfo[] propertyInfos;
        
        
        [Fact]
        public void ReadQueryBuildeTestr()
        {
            int id = 4;
            var query = queryBuilder.Read(id);
            var result = $"SELECT {string.Join(",", columnInfos.Select(info => info.Name))} FROM person WHERE id = {id}";
            Assert.Equal(query, result);
        }

        [Fact]
        public void ReadAllQueryBuilderTest()
        {
            var query = queryBuilder.ReadAll();
            var result = $"SELECT {string.Join(",", columnInfos.Select(info => info.Name))} FROM person";
           
            Assert.Equal(query, result);
        }

        [Fact]
        public void ReadAllParamQueryBuilderTest()
        {
            var param = new { id = 1, name = "jin", age = 27, address = "Seoul", email = "A@aa.a" };
            var query = queryBuilder.ReadAll(param);
            var result = $"SELECT {string.Join(",", columnInfos.Select(info => info.Name))} FROM person WHERE {string.Join(" AND ", columnInfos.Select(info => info.ParameterExpression))}";
            Assert.Equal(query, result);
        }

        [Fact]
        public void CreateQueryBuilderTest()
        {
            var query = queryBuilder.Create();
            var result = string.Concat("INSERT INTO person VALUES(); SELECT LAST_INSERT_ID();");
            Assert.Equal(query, result);
        }

        [Fact]
        public void CreateDataQueryBuilderTest()
        {
            var query = queryBuilder.Create(withID);
            var result = $"INSERT INTO person VALUES ({string.Join(" , ", columnInfos.Select(info => info.ParameterName))}); SELECT LAST_INSERT_ID();";
            Assert.Equal(query, result);
        }

        [Fact]
        public void UpdateQueryBuilderTest()
        {
            int id = 1;
            var query = queryBuilder.Update(id,withID);
            var result = $"UPDATE person SET {string.Join(",", columnInfos.Select(info => info.ParameterExpression))}  WHERE id = {id}";
            Assert.Equal(query, result);
        }

        [Fact]
        public void DeleteQueryBuilderTest()
        {
            int id = 1;
            var query = queryBuilder.Delete(id);
            var result = string.Concat("DELETE FROM person WHERE id = ", id);
            Assert.Equal(query, result);
        }


        [Fact]
        public void DeleteAllQueryBuilderTest()
        {
            var param = new { id = 1, name = "jin", age = 27, address = "Seoul", email = "A@aa.a" };
            var query =  queryBuilder.DeleteAll(param);
            var result = $"DELETE FROM person WHERE {string.Join(" AND ", columnInfos.Select(info => info.ParameterExpression))}";
            Assert.Equal(query, result);
        }
    }
}