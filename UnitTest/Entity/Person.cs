using System;
using System.Collections.Generic;
using System.Text;
using GGM.ORM.Attribute;

namespace GGM.ORMTest.Entity
{
    [Entity("person")]
    public class Person
    {
        public Person() { }
        public Person(int iD, string name, int age, string address, string email)
        {
            ID = iD;
            Name = name;
            Age = age;
            Address = address;
            Email = email;
        }

        [PrimaryKey]
        [Column("id")]
        public int ID { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("age")]
        public int Age { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("email")]
        public string Email { get; set; }
    }
}
