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
        /// <summary>
        ///     EntityManagerFactory를 생성합니다.
        /// </summary>
        /// <param name="assemblyName">대상 EntityManager가 위치하는 Assembly의 이름</param>
        /// <param name="classPath">대상 EntityManager의 Class의 풀네임</param>
        /// <param name="connectionString">DbConnection의 생성자에 들어갈 문자열</param>
        /// <returns></returns>
        public static EntityManagerFactory CreateFactory(string assemblyName, string classPath, string connectionString)
        {
            if (factories.ContainsKey(classPath))
                return factories[classPath];
            var targetAssembly = Assembly.Load(assemblyName);
            CreateEntityManagerException.Check(targetAssembly != null, CreateEntityManagerError.NotExistAssembly);
            var targetType = targetAssembly.GetType(classPath);
            CreateEntityManagerException.Check(targetType != null, CreateEntityManagerError.NotExistFactoryClass);
            var factory = Activator.CreateInstance(targetType, connectionString) as EntityManagerFactory;
            factories[classPath] = factory;
            return factory;
        }

        private static Dictionary<string, EntityManagerFactory> factories = new Dictionary<string, EntityManagerFactory>();

        public EntityManagerFactory(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        protected abstract DbConnection CreateDBConnection();

        public EntityManager CreateEntityManager()
        {
            return new EntityManager(CreateDBConnection());
        }
    }
}
