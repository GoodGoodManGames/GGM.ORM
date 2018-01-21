using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.Exception
{
    public enum ParameterError
    {
        NotExistType,
    }

    public class ParameterException : System.Exception
    {
        public ParameterException(ParameterError parameterError)
            : base($"Fail to get parameter. {nameof(ParameterError)} : {parameterError}")
        {
            ParameterError = parameterError;
        }

        public ParameterError ParameterError { get; }

        public static void Check(bool condition, ParameterError parameterError)
        {
            if (condition != true)
                throw new ParameterException(parameterError);
        }
    }
}

