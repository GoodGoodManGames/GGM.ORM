using System;
using System.Collections.Generic;
using System.Text;
using GGM.ORM;
using GGM.ORM.QueryBuilder;

namespace GGM.ORMTest.Entity
{
    public class PersonRepository : Repository<Person>
    {
        public PersonRepository(string assemblyName, string entityManagerPath, string dbOptions, IQueryBuilder<Person> queryBuilder) : base(assemblyName, entityManagerPath, dbOptions, null)
        {
        }
    }
}
