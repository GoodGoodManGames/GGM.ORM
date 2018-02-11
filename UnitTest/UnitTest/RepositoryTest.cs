using System;
using Xunit;
using GGM.ORMTest.Entity;
using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using GGM.ORM;
using GGM.ORMTest.EntityManagerFactory;
using Xunit.Sdk;

namespace GGM.ORMTest.UnitTest
{
    public class RepositoryTest : IDisposable
    {
        public RepositoryTest()
        {
            repository = new PersonRepository(new MysqlManagerFactory(), null);

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

            var nullCommand = repository.EntityManager.Connection.CreateCommand();
            nullCommand.CommandText = "INSERT INTO person VALUES();";
            nullCommand.CommandType = CommandType.Text;

            var withoutIDCommand = repository.EntityManager.Connection.CreateCommand();
            withoutIDCommand.CommandText =
                "INSERT INTO person (name,address,email,age) VALUES (\"withoutID\",\"Busan\",\"withOutID@naver.com\",25)";
            withoutIDCommand.CommandType = CommandType.Text;

            repository.EntityManager.Connection.Open();
            for (var i = 0; i < 3; i++)
            {
                nullCommand.ExecuteNonQuery();
                withoutIDCommand.ExecuteNonQuery();
            }

            repository.EntityManager.Connection.Close();
        }

        public void Dispose()
        {
            var command = repository.EntityManager.Connection.CreateCommand();
            command.CommandText = "TRUNCATE person;";
            command.CommandType = CommandType.Text;
            if (repository.EntityManager.Connection.State == ConnectionState.Closed)
                repository.EntityManager.Connection.Open();
            command.ExecuteNonQuery();
            repository.EntityManager.Connection.Close();
        }

        PersonRepository repository;
        Person withoutID;
        Person withID;

        [Fact]
        public void CreateTest()
        {
            var createInstance = repository.Create();
            Assert.NotNull(createInstance);
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
        public void ReadAllParamTest()
        {
            repository.Create(withID);
            var data = repository.ReadAll(new
            {
                age = 25
            });
            var dataCount = data.Count();
            Assert.Equal(3,dataCount);
        }

        [Fact]
        public void ReadSingleTest()
        {
            var readSingleAgain = repository.Read(2);
            Assert.NotNull(readSingleAgain);
        }

        [Fact]
        public void ReadAllTest()
        {
            var result = repository.ReadAll();
            Assert.NotEqual(0, result.Count());
        }

        [Fact]
        public void DeleteAllTest()
        {
            repository.DeleteAll();
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 0);
        }

        [Fact]
        public void DeleteTest()
        {
            int id = 1;
            repository.Delete(id);
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 5);
        }

        [Fact]
        public void DeleteAllParamTest()
        {
            var param = new {name = "withoutID"};
            repository.DeleteAll(param);
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 3);
        }

        [Fact]
        public void UpdateWithDataTest()
        {
            repository.Update(5, withoutID);

            var result = repository.Read(5);
            var copyWithID = withoutID;
            copyWithID.ID = 5;

            PropertyInfo[] propertyinfos = typeof(Person).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in propertyinfos)
            {
                Assert.True(property.GetValue(copyWithID).Equals(property.GetValue(result)));
            }
        }

        [Fact]
        public async Task CreateAsyncTest()
        {
            var createInstance = await repository.CreateAsync();
            Assert.NotNull(createInstance);
        }

        [Fact]
        public async Task CreateWithDataAsyncTest()
        {
            var createWithID = await repository.CreateAsync(withID);
            var createWithoutID = await repository.CreateAsync(withoutID);

            Assert.NotNull(createWithID);
            Assert.NotNull(createWithoutID);
        }

        [Fact]
        public async Task ReadAsyncTest()
        {
            var firstData = await repository.ReadAsync(1).ConfigureAwait(false);
            var result = firstData.ID;
            Assert.Equal(result, 1);
        }

        [Fact]
        public async Task ReadAllAsyncTest()
        {
            var data = await repository.ReadAllAsync().ConfigureAwait(false);
            var dataResult = data.ToArray();
            Assert.Equal(dataResult.Count(), 6);
        }

        [Fact]
        public async Task ReadAllParamAsyncTest()
        {
            var param = new { age = 25 };
            var data = await repository.ReadAllAsync(param).ConfigureAwait(false);
            var dataResult = data.ToArray();
            Assert.Equal(dataResult.Count(), 3);
        }

        [Fact]
        public async Task DeleteAsyncTest()
        {
            int id = 1;
            await repository.DeleteAsync(id);
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 5);
        }

        [Fact]
        public async Task DeleteAllAsyncTest()
        {
            await repository.DeleteAllAsync();
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 0);
        }

        [Fact]
        public async Task DeleteAllParamAsyncTest()
        {
            var param = new { email = "withOutID@naver.com" };
            await repository.DeleteAllAsync(param);
            var result = repository.ReadAll();
            Assert.Equal(result.Count(), 3);
        }

        [Fact]
        public async Task UpdateWithDataAsyncTest()
        {
            await repository.UpdateAsync(5, withoutID);

            var result = repository.Read(5);
            var copyWithID = withoutID;
            copyWithID.ID = 5;

            PropertyInfo[] propertyinfos = typeof(Person).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in propertyinfos)
            {
                Assert.True(property.GetValue(copyWithID).Equals(property.GetValue(result)));
            }
        }
    }
}