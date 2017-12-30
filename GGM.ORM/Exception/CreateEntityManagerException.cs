using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.Exception
{
    public enum CreateEntityManagerError
    {
        NotExistAssembly, NotExistFactoryClass
    }

    public class CreateEntityManagerException : System.Exception
    {
        public CreateEntityManagerException(CreateEntityManagerError createEntityManagerError)
            : base($"Fail to create EntityManager. {nameof(CreateEntityManagerError)} : {createEntityManagerError}")
        {
            CreateEntityManagerError = createEntityManagerError;
        }

        public CreateEntityManagerError CreateEntityManagerError { get; }

        public static void Check(bool condition, CreateEntityManagerError queryBuilderError)
        {
            if (condition != true)
                throw new CreateEntityManagerException(queryBuilderError);
        }
    }
}
