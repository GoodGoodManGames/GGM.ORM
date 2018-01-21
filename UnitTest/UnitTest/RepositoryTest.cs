using System;
using Xunit;
using GGM.ORMTest.Entity;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Linq;


namespace GGM.ORMTest.UnitTest
{
    public class RepositoryTest
    {
        public RepositoryTest()
        {
            repository = new PersonRepository("UnitTest", "GGM.ORMTest.EntityManagerFactory.MysqlManagerFactory", "Server=203.253.76.178;Database=practice;Uid=ggm_black;pwd=songji710;", null);

            withoutID = new Person();
            withoutID.Name = "withoutID";
            withoutID.Address = "Busan";
            withoutID.Email = "withOutID@naver.com";
            withoutID.Age = 28;

            withID = new Person();
            withID.ID = 25;
            withID.Name = "withID";
            withID.Address = "Gangnam";
            withID.Email = "yesyes@naver.com";
            withID.Age = 29;
        }

        PersonRepository repository;
        Person withoutID;
        Person withID;[Fact]
        public void CreateNullTest()
        {
            var createNullInstance = repository.Create();
            Assert.NotNull(createNullInstance);

        }

        [Fact]
        public void CreateWithDataTest()
        {

            var createWithID = repository.Create(withID);
            var createWithoutID = repository.Create(withoutID);

            Assert.NotNull(createWithID);
            Assert.NotNull(createWithoutID);
        }
        
        [Fact]
        public void DeleteAllTest()
        {
            repository.DeleteAll();
            var result = repository.ReadAll().GetEnumerator();
            result.MoveNext();
            Assert.Null(result.Current);
        }

        [Fact]
        public void DeleteTest()
        {
            repository.DeleteAll();
            repository.Create(withID);
            int id = 25;
            repository.Delete(id);
            var result = repository.Read(id);
            Assert.Equal(0,result.ID);           
        }
        
        [Fact]
        public void DeleteAllParamTest()
        {
            repository.DeleteAll();
            repository.Create(withID);
            var param = new { id = 25, name = "withID", age = 29, address = "Gangnam", email = "yesyes@naver.com" };
            repository.DeleteAll(param);

            var result = repository.ReadAll(param).GetEnumerator();
            result.MoveNext();
            Assert.Null(result.Current);
        }

        [Fact]
        public void ReadAllTest()
        {
            repository.DeleteAll();
            repository.Create();
            repository.Create();
            repository.Create();
            repository.Create(withoutID);
            repository.Create(withoutID);
            repository.Create(withoutID);
            repository.Create(withID);
            var data = repository.ReadAll();
            var dataCount = data.Count();
            Assert.NotEqual(0, dataCount);
        }

        [Fact]
        public void ReadAllParamTest()
        {
            repository.DeleteAll();
            repository.Create();
            repository.Create();
            repository.Create();
            repository.Create(withID);
            var data = repository.ReadAll(new { id = 25, name = "withID", age = 29, address = "Gangnam", email = "yesyes@naver.com" });
            var dataCount = data.Count();
            Assert.NotEqual(0, dataCount);
        }

        //[Fact]
        public void ReadSingleTest()
        {
            repository.DeleteAll();
            repository.Create();
            var readSingle = repository.Read(1);
            Assert.NotNull(readSingle);
            repository.DeleteAll();
            repository.Create(withoutID);
            var readSingleAgain = repository.Read(1);
            Assert.NotNull(readSingleAgain);
        }

        //[Fact]
        public void UpdateWithDataTest()
        {
            repository.DeleteAll();
            repository.Create(withID);
            int id = 25;
            repository.Update(id, withoutID);

            var result = repository.Read(id);
            var copyWithID = withID;
            copyWithID.ID = id;

            PropertyInfo[] propertyinfos = typeof(Person).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in propertyinfos)
            {
                Assert.True(property.GetValue(copyWithID).Equals( property.GetValue(result)));
            }
        }  
    }
}