using System;
using GGM.ORM.Attribute;
using GGM.ORM.QueryBuilder.Exception;
using System.Linq;
using System.Reflection;

namespace GGM.ORM.QueryBuilder
{
    public class DefaultQueryBuilder<T> : IQueryBuilder<T>
    {
        public DefaultQueryBuilder()
        {
            var targetType = typeof(T);
            var entityAttribute = targetType.GetCustomAttribute<EntityAttribute>();
            TableName = entityAttribute.Name ?? targetType.Name;
            QueryBuilderException.Check(entityAttribute != null, QueryBuilderError.IsNotEntityClass);

            ColumnInfos = targetType.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));
            QueryBuilderException.Check(primaryKey != null, QueryBuilderError.NotExistPrimaryKey);
            PrimaryKeyName = primaryKey.Name;
        }

        public string TableName { get; }
        public string PrimaryKeyName { get; }
        public ColumnInfo[] ColumnInfos { get; }

        public string Create()
        {
            return $"INSERT INTO {TableName} VALUES(); SELECT LAST_INSERT_ID();";
        }

        public string Create(T data)
        {
            return $"INSERT INTO {TableName} VALUES ({string.Join(" , ", ColumnInfos.Select(info => info.ParameterName))}); SELECT LAST_INSERT_ID();";
        }

        public string Delete(int id)
        {
            return $"DELETE FROM {TableName} WHERE {PrimaryKeyName} = {id}";
        }

        public string DeleteAll(object param)
        {
            var paramInfos = param.GetType().GetProperties().Select(info => new ParameterInfo(info)).ToArray();
            return $"DELETE FROM {TableName} WHERE {string.Join(" AND ", paramInfos.Select(info => info.ParameterExpression))}";

        }

        public string DeleteAll()
        {
            return $"TRUNCATE {TableName}";
        }

        public string Read(int id)
        {
            return $"SELECT {string.Join(",", ColumnInfos.Select(info => info.Name))} FROM {TableName} WHERE {PrimaryKeyName} = {id}";
        }

        public string ReadAll(object param)
        {
            var paramInfos = param.GetType().GetProperties().Select(info => new ParameterInfo(info)).ToArray();
            return $"SELECT {string.Join(",", ColumnInfos.Select(info => info.Name))} FROM {TableName} WHERE {string.Join(" AND ", paramInfos.Select(info => info.ParameterExpression))}";
        }

        public string ReadAll()
        {
            return $"SELECT {string.Join(",", ColumnInfos.Select(info => info.Name))} FROM {TableName}";
        }

        public string Update(int id, T data)
        {
            return $"UPDATE {TableName} SET {string.Join(",", ColumnInfos.Select(info => info.ParameterExpression))}  WHERE {PrimaryKeyName} = {id}";
        }
        
    }
}
