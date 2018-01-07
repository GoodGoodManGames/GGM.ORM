using System;
using Xunit;
using GGM.ORMTest.Entity;
using GGM.ORM.QueryBuilder;

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
        }

        DefaultQueryBuilder<Person> queryBuilder;
        Person withID;

        [Fact]
        public string ReadQueryBuildeTestr()
        {
            return queryBuilder.Read(4);
        }

        [Fact]
        public string ReadAllQueryBuilderTest()
        {
            return queryBuilder.ReadAll();
        }

        [Fact]
        public string ReadAllParamQueryBuilderTest()
        {
            return queryBuilder.ReadAll(new { id = 1, name = "jin", age = 27, address = "Seoul", email = "A@aa.a" });
        }

        [Fact]
        public string CreateQueryBuilderTest()
        {
            return queryBuilder.Create();
        }

        [Fact]
        public string CreateDataQueryBuilderTest()
        {
            return queryBuilder.Create(withID);
        }

        [Fact]
        public string UpdateQueryBuilderTest()
        {
            return queryBuilder.Update(1,withID);
        }

        [Fact]
        public string DeleteQueryBuilderTest()
        {
            return queryBuilder.Delete(1);
        }


        [Fact]
        public string DeleteAllQueryBuilderTest()
        {
            return queryBuilder.DeleteAll(new { id = 1, name = "jin", age = 27, address = "Seoul", email = "A@aa.a" });
        }
    }
}