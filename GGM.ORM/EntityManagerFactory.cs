using GGM.ORM.Exception;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace GGM.ORM
{
    /// <summary>
    ///     EntityManger를 생성하는 Factory입니다. Thread-Safe하게 구현되며 각 Vendor에 맞게 구현되어야합니다.
    /// </summary>
    public abstract class EntityManagerFactory
    {
        protected abstract DbConnection CreateDBConnection();

        public EntityManager CreateEntityManager()
        {
            return new EntityManager(CreateDBConnection());
        }
    }
}
