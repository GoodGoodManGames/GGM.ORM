using System;
using Xunit;
using GGM.ORMTest.Entity;

namespace GGM.ORMTest
{
    public class ORMTest
    {
        [Fact]
        public void TestMethod1()
        {
            var repo = new PersonRepository("UnitTest", "GGM.ORMTest.EntityManagerFactory.MysqlManagerFactory", "Server=203.253.76.178;Database=practice;Uid=ggm_black;pwd=songji710;", null);

            var noId = new Person();
            noId.Name = "jin";
            noId.Age = 26;
            noId.Email = "jinwoo710@naver.com";
            noId.Address = "Seoul";


            var yesId = new Person();
            yesId.Name = "jin";
            yesId.Age = 26;
            yesId.Email = "jinwoo710@naver.com";
            yesId.Address = "Seoul";
            yesId.ID = 2;

            repo.Update(24453, noId);
            repo.Update(24451, yesId);
        }
    }
}
