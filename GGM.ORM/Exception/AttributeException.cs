using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.Exception
{
    public enum AttributeError
    {
        NotExistColumnAttribute
    }

    public class AttributeException : System.Exception
    {
        public AttributeException(AttributeError attributeError)
            : base($"Fail to get attribute. {nameof(AttributeError)} : {attributeError}")
        {
            AttributeError = attributeError;
        }

        public AttributeError AttributeError { get; }

        public static void Check(bool condition, AttributeError attributeError)
        {
            if (condition != true)
                throw new AttributeException(attributeError);
        }
    }
}
