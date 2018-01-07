using System;
using Xunit;
using GGM.ORMTest.Entity;

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
            withID.ID = 120;
            withID.Name = "withID";
            withID.Address = "Gangnam";
            withID.Email = "yesyes@naver.com";
            withID.Age = 29;
        }

        PersonRepository repository;
        Person withoutID;
        Person withID;

        [Fact]
        public void CreateNullTest()
        {
            repository.Create();
            repository.Create();
        }

        [Fact]
        public void CreateWithDataTest()
        {

            repository.Create(withID);
            repository.Create(withoutID);
        }

        [Fact]
        public void ReadAllTest()
        {
            repository.ReadAll();

        }

        [Fact]
        public void ReadAllParamTest()
        {
            repository.ReadAll(new { id = 1, name = "jinwoo", age = 26, address = "Seoul", email = "jinwoo710@naver.com" });
        }

        [Fact]
        public void ReadSingleTest()
        {
            repository.Read(1);
            repository.Read(4);
        }

        [Fact]
        public void DeleteTest()
        {
            repository.Delete(24522);
        }

        [Fact]
        public void DeleteAllTest()
        {
            repository.DeleteAll(new { id = 24521, name = "noID", age = 28, address = "Busan", email = "withOutID@naver.com" });
        }

        [Fact]
        public void UpdateWithDataTest()
        {
            repository.Update(7, withID);
            repository.Update(8, withoutID);
        }
    }
}