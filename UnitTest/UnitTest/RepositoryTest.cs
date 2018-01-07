using System;
using Xunit;
using GGM.ORMTest.Entity;
using System.Data.Common;
using System.Data;
using System.Reflection;

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
        Person withID;
        
        

        [Fact]
        public void ReadAllTest()
        {
            var readAll =repository.ReadAll();
            Assert.NotNull(readAll);
        }

        [Fact]
        public void ReadAllParamTest()
        {
            var readAllParam = repository.ReadAll(new { id = 1, name = "jinwoo", age = 26, address = "Seoul", email = "jinwoo710@naver.com" });
            Assert.NotNull(readAllParam);
        }

        [Fact]
        public void ReadSingleTest()
        {
            var readSingle = repository.Read(1);
            Assert.NotNull(readSingle);
        }

        [Fact]
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
        public void DeleteTest()
        {
            int id = 1151;
            repository.Delete(id);
            var result = repository.Read(id);
            Assert.Equal(0,result.ID);           
        }
        
        [Fact]
        public void DeleteAllTest()
        {
            
            var param = new { id = 151, name = "withoutID", age = 28, address = "Busan", email = "withOutID@naver.com" };
            repository.DeleteAll(param);

            var result = repository.ReadAll(param).GetEnumerator();
            result.MoveNext();
            Assert.Null(result.Current);
        }
        
        [Fact]
        public void UpdateWithDataTest()
        {
            int id = 152;
            repository.Update(id, withID);

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