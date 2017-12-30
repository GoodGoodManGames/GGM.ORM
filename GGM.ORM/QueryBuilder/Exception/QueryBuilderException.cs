using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.QueryBuilder.Exception
{
    public enum QueryBuilderError
    {
        IsNotEntityClass, NotExistPrimaryKey
    }

    public class QueryBuilderException : System.Exception
    {
        public QueryBuilderException(QueryBuilderError queryBuilderError)
            : base($"Fail to create QueryBuilder. {nameof(QueryBuilderError)} : {queryBuilderError}")
        {
            QueryBuilderError = queryBuilderError;
        }

        public QueryBuilderError QueryBuilderError { get; }

        public static void Check(bool condition, QueryBuilderError queryBuilderError)
        {
            if (condition != true)
                throw new QueryBuilderException(queryBuilderError);
        }
    }
}
