using System;
using System.Collections.Generic;
using System.Text;
using GGM.ORM;
using GGM.ORM.QueryBuilder;

namespace GGM.ORMTest.Entity
{
    public class PersonRepository : Repository<Person>
    {
        public PersonRepository(ORM.EntityManagerFactory entityManagerFactory, IQueryBuilder<Person> queryBuilder) : base(entityManagerFactory, null)
        {
        }
    }
}
