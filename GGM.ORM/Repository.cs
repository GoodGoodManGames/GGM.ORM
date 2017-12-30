using GGM.ORM.Attribute;
using GGM.ORM.QueryBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using GGM.ORM.Exception;
using System.Linq;

namespace GGM.ORM
{
    public abstract class Repository<T> where T : new()
    {
        public Repository(string assemblyName, string classPath, string dbOptions, IQueryBuilder<T> queryBuilder)
        {
            EntityManager = EntityManagerFactory.CreateFactory(assemblyName, classPath, dbOptions).CreateEntityManager();
            QueryBuilder = queryBuilder;
            if (QueryBuilder == null)
                QueryBuilder = new DefaultQueryBuilder<T>();
        }

        public EntityManager EntityManager { get; }
        public IQueryBuilder<T> QueryBuilder { get; }

        public delegate void FillParamInfoGenerator(IDbCommand command, object parameter);
        public delegate T ConstructInstance(IDataReader reader);
        public delegate List<T> ConstructInstances(IDataReader reader);

        public Dictionary<Type, FillParamInfoGenerator> ParameterGeneratorCache = new Dictionary<Type, FillParamInfoGenerator>();
        public Dictionary<Type, ConstructInstance> ConstructInstanceCache = new Dictionary<Type, ConstructInstance>();
        public Dictionary<Type, ConstructInstances> ConstructInstancesCache = new Dictionary<Type, ConstructInstances>();


        public T Read(int id)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Read(id);
            command.CommandType = CommandType.Text;
            EntityManager.Connection.Open();
            DbDataReader reader = command.ExecuteReader();

            if (!ConstructInstanceCache.ContainsKey(reader.GetType()))
                ConstructInstanceCache.Add(reader.GetType(), CreateConstructInstance(reader));
            var constructingInstance = ConstructInstanceCache[reader.GetType()];
            var result = constructingInstance(reader);

            EntityManager.Connection.Close();
            return result;
        }

        public IEnumerable<T> ReadAll(object param)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.ReadAll(param);
            command.CommandType = CommandType.Text;
            if (!ParameterGeneratorCache.ContainsKey(param.GetType()))
                ParameterGeneratorCache.Add(param.GetType(), CreateParamInfoGenerator(param));
            var fillParameterGenerator = ParameterGeneratorCache[param.GetType()];
            fillParameterGenerator(command, param);
            EntityManager.Connection.Open();
            DbDataReader reader = command.ExecuteReader();
            if (!ConstructInstancesCache.ContainsKey(reader.GetType()))
                ConstructInstancesCache.Add(reader.GetType(), CreateConstructInstances(reader));
            var constructingInstances = ConstructInstancesCache[reader.GetType()];
            var result = constructingInstances(reader);
            EntityManager.Connection.Close();
            return result;
        }

        public IEnumerable<T> ReadAll()
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.ReadAll();
            command.CommandType = CommandType.Text;
            EntityManager.Connection.Open();

            DbDataReader reader = command.ExecuteReader();
            if (!ConstructInstancesCache.ContainsKey(reader.GetType()))
                ConstructInstancesCache.Add(reader.GetType(), CreateConstructInstances(reader));
            var constructingInstances = ConstructInstancesCache[reader.GetType()];
            var result = constructingInstances(reader);
            EntityManager.Connection.Close();
            return result;
        }
        public string TableName { get; }
        public string PrimaryKeyName { get; }
        public ColumnInfo[] ColumnInfos { get; set; }

        public T Create()
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Create();
            command.CommandType = CommandType.Text;

            EntityManager.Connection.Open();
            var id = Convert.ToInt32(command.ExecuteScalar());
            EntityManager.Connection.Close();

            var result = new T();
            var type = typeof(T);
            ColumnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));
            primaryKey.PropertyInfo.SetValue(result, id);
            return result;

        }

        public T Create(T data)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Create(data);
            command.CommandType = CommandType.Text;
            if (!ParameterGeneratorCache.ContainsKey(data.GetType()))
                ParameterGeneratorCache.Add(data.GetType(), CreateParamInfoGenerator(data));
            var fillParameterGenerator = ParameterGeneratorCache[data.GetType()];
            fillParameterGenerator(command, data);

            EntityManager.Connection.Open();
            var id = Convert.ToInt32(command.ExecuteScalar());
            EntityManager.Connection.Close();

            var dataType = data.GetType();
            ColumnInfos = dataType.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var dataPrimaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));
            var dataId = dataPrimaryKey.PropertyInfo.GetValue(data);
            var result = data;

            if ((int)dataId == 0)
            {
                var type = typeof(T);
                ColumnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
                var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));
                primaryKey.PropertyInfo.SetValue(result, id);
            }

            return result;
        }

        public void Update(int id, T data)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Update(id, data);
            command.CommandType = CommandType.Text;
            var type = typeof(T);
            PropertyInfo[] propertyInfos = type.GetProperties();
            foreach (var property in propertyInfos)
            {
                DbParameter parameter = command.CreateParameter();
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                AttributeException.Check(columnAttribute != null, AttributeError.NotExistColumnAttribute);
                var columnName = columnAttribute.Name ?? property.Name;
                if (columnName.Equals("id"))
                    parameter.Value = id;
                else
                    parameter.Value = property.GetValue(data);
                parameter.ParameterName = property.Name;
                parameter.DbType = LookupDbType(property.GetType());
                command.Parameters.Add(parameter);
            }
            EntityManager.Connection.Open();
            command.ExecuteNonQuery();
            EntityManager.Connection.Close();
        }

        public void Delete(int id)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Delete(id);
            command.CommandType = CommandType.Text;
            EntityManager.Connection.Open();
            command.ExecuteNonQuery();
            EntityManager.Connection.Close();
        }

        public void DeleteAll(object param)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.DeleteAll(param);
            command.CommandType = CommandType.Text;
            if (!ParameterGeneratorCache.ContainsKey(param.GetType()))
                ParameterGeneratorCache.Add(param.GetType(), CreateParamInfoGenerator(param));
            var fillParameterGenerator = ParameterGeneratorCache[param.GetType()];
            fillParameterGenerator(command, param);
            EntityManager.Connection.Open();
            command.ExecuteNonQuery();
            EntityManager.Connection.Close();
        }

        private FillParamInfoGenerator CreateParamInfoGenerator(object param)
        {

            var parameterType = param.GetType();
            var propertyInfos = parameterType.GetProperties();
            var dynamicMethod = new DynamicMethod("CreateParameterGenerator", null, new[] { typeof(IDbCommand), typeof(object) });
            var il = dynamicMethod.GetILGenerator();
            var commandIL = il.DeclareLocal(typeof(IDbCommand));
            il.Emit(OpCodes.Ldarg_0); //[DbCommand]
            il.Emit(OpCodes.Stloc, commandIL); //Empty
            foreach (var property in propertyInfos)
            {
                il.Emit(OpCodes.Ldloc, commandIL); //[DbCommand]
                il.Emit(OpCodes.Callvirt, typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters)).GetGetMethod()); //[Parameters]
                il.Emit(OpCodes.Ldloc, commandIL); //[Parameters][Dbcommand]
                il.Emit(OpCodes.Callvirt, typeof(IDbCommand).GetMethod(nameof(IDbCommand.CreateParameter))); //[Parameters] [DbParameter]

                // SetName
                il.Emit(OpCodes.Dup); //[Parameters][DbParameter][DbParameter]
                il.Emit(OpCodes.Ldstr, property.Name); //[Parameters][DbParameter][DbParameter][Name]           
                il.Emit(OpCodes.Callvirt, typeof(IDataParameter).GetProperty(nameof(IDbDataParameter.ParameterName)).GetSetMethod()); //[Parameters][DbParameter]

                // SetDbType
                DbType dbType = LookupDbType(property.PropertyType);
                il.Emit(OpCodes.Dup); //[Parameters][DbParameter][DbParameter]
                il.Emit(OpCodes.Ldc_I4, (int)dbType); //[Parameters][DbParameter][DbParameter][dbType-num]
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty(nameof(IDataParameter.DbType)).GetSetMethod(), null);//[Parameters][DbParameter]

                // SetValue
                il.Emit(OpCodes.Dup); //[Parameters][DbParameter][DbParameter]
                il.Emit(OpCodes.Ldarg_1); ////[Parameters][DbParameter][DbParameter][object]
                il.Emit(OpCodes.Call, property.GetGetMethod()); //[Parameters][DbParameter][DbParameter][Value]

                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, property.PropertyType);//[Parameters][DbParameter][DbParameter][boxed-Value]

                il.Emit(OpCodes.Callvirt, typeof(IDataParameter).GetProperty(nameof(IDataParameter.Value)).GetSetMethod()); //[Parameters][DbParameter]
                il.Emit(OpCodes.Callvirt, typeof(IList).GetMethod(nameof(IList.Add))); //[int]
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ret);

            return (FillParamInfoGenerator)dynamicMethod.CreateDelegate(typeof(FillParamInfoGenerator));
        }

        /// <summary>
        ///     DataReader로부터 한개의 row를 읽어 인스턴스를 만든다.
        /// </summary>
        /// <param name="reader">DB로 부터 가져온 데이터의 시작 위치</param>
        /// <returns></returns>
        /// 
        private ConstructInstance CreateConstructInstance(IDataReader reader)
        {
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("ConstructInstance", type, new[] { typeof(IDataReader) }, type);
            var il = dynamicMethod.GetILGenerator();
            Label startLoop = il.DefineLabel();
            Label endLoop = il.DefineLabel();

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructor); //[instance]
            il.MarkLabel(startLoop);

            il.Emit(OpCodes.Ldarg_0); //[instance][IDataReader]
            il.Emit(OpCodes.Call, typeof(IDataReader).GetMethod(nameof(IDataReader.Read))); //[instance][Bool:CheckNext]
            il.Emit(OpCodes.Brfalse, endLoop); //[instance]

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                Label startValueTypeNullLoop = il.DefineLabel();
                Label endValueTypeNullLoop = il.DefineLabel();
                Label jumpToEnd = il.DefineLabel();

                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                AttributeException.Check(columnAttribute != null, AttributeError.NotExistColumnAttribute);
                var columnName = columnAttribute.Name ?? property.Name;
                //SetValue
                il.Emit(OpCodes.Dup); //[instance][instance]
                il.Emit(OpCodes.Ldarg_0); //[instance][instance][IDataReader]
                il.Emit(OpCodes.Ldstr, columnName); //[instance][instance][IDataReader][Name]
                il.Emit(OpCodes.Call, typeof(IDataReaderHelper).GetMethod(nameof(IDataReaderHelper.GetValue)));//[instance][instance][value]
                il.Emit(OpCodes.Dup);//[instance][instance][value][value]
                il.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField(nameof(DBNull.Value))); //[instance][instance][value][value][DBNullValue]
                il.Emit(OpCodes.Ceq);// [instance][instance][value][result]
                il.Emit(OpCodes.Brtrue, startValueTypeNullLoop);//[instance][instance]
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, property.PropertyType);//[instance][instance][unbox_value]
                il.MarkLabel(endValueTypeNullLoop);
                il.Emit(OpCodes.Call, property.GetSetMethod());//[instance]
                il.Emit(OpCodes.Br, jumpToEnd);

                il.MarkLabel(startValueTypeNullLoop);
                if (property.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Pop);//[instance][instance]
                    il.Emit(OpCodes.Ldc_I4_0);//[instance][instance][0]
                }
                il.Emit(OpCodes.Br, endValueTypeNullLoop);
                il.MarkLabel(jumpToEnd);
            }
            il.MarkLabel(endLoop);
            il.Emit(OpCodes.Ret);
            return (ConstructInstance)dynamicMethod.CreateDelegate(typeof(ConstructInstance));
        }


        /// <summary>
        ///     DataReader로부터 여러개의 row를 읽어 인스턴스들을 만든 후, 리스트에 저장한다. 
        /// </summary>
        /// <param name="reader">DB로 부터 가져온 데이터의 시작 위치</param>
        /// <returns></returns>
        /// 
        private ConstructInstances CreateConstructInstances(IDataReader reader)
        {
            var listType = typeof(List<T>);
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("ConstructInstances", typeof(List<T>), new[] { typeof(IDataReader) });
            var il = dynamicMethod.GetILGenerator();
            Label startLoop = il.DefineLabel();
            Label endLoop = il.DefineLabel();

            ConstructorInfo constructorList = listType.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructorList); //[listInstance]
            il.MarkLabel(startLoop);

            //Check Follow
            il.Emit(OpCodes.Dup);//[listInstance][listInstance]
            il.Emit(OpCodes.Ldarg_0); //[listInstance][listInstance][IDataReader]
            il.Emit(OpCodes.Call, typeof(IDataReader).GetMethod(nameof(IDataReader.Read))); //[listInstance][listInstance][CheckNext]
            il.Emit(OpCodes.Brfalse, endLoop); //[listInstance][listInstance]

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructor); //[listInstance][listInstance][instance]


            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                Label startValueTypeNullLoop = il.DefineLabel();
                Label endValueTypeNullLoop = il.DefineLabel();
                Label jumpToEnd = il.DefineLabel();

                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                AttributeException.Check(columnAttribute != null, AttributeError.NotExistColumnAttribute);
                var columnName = columnAttribute.Name ?? property.Name;
                //SetValue
                il.Emit(OpCodes.Dup); //[instance][instance]
                il.Emit(OpCodes.Ldarg_0); //[instance][instance][IDataReader]
                il.Emit(OpCodes.Ldstr, columnName); //[instance][instance][IDataReader][Name]
                il.Emit(OpCodes.Call, typeof(IDataReaderHelper).GetMethod(nameof(IDataReaderHelper.GetValue)));//[instance][instance][value]
                il.Emit(OpCodes.Dup);//[instance][instance][value][value]
                il.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField(nameof(DBNull.Value))); //[instance][instance][value][value][DBNullValue]
                il.Emit(OpCodes.Ceq);// [instance][instance][value][result]
                il.Emit(OpCodes.Brtrue, startValueTypeNullLoop);//[instance][instance]
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, property.PropertyType);//[instance][instance][unbox_value]
                il.MarkLabel(endValueTypeNullLoop);
                il.Emit(OpCodes.Call, property.GetSetMethod());//[instance]
                il.Emit(OpCodes.Br, jumpToEnd);

                il.MarkLabel(startValueTypeNullLoop);
                if (property.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Pop);//[instance][instance]
                    il.Emit(OpCodes.Ldc_I4_0);//[instance][instance][0]
                }
                il.Emit(OpCodes.Br, endValueTypeNullLoop);
                il.MarkLabel(jumpToEnd);
            }
            il.Emit(OpCodes.Call, typeof(List<T>).GetMethod(nameof(List<T>.Add))); //[listInstance] 
            il.Emit(OpCodes.Br, startLoop);//[listInstance]
            il.MarkLabel(endLoop); //[listInstance][listInstance]
            il.Emit(OpCodes.Pop);//[listINstance]
            il.Emit(OpCodes.Ret);

            return (ConstructInstances)dynamicMethod.CreateDelegate(typeof(ConstructInstances));
        }

        private static DbType LookupDbType(Type type)
        {
            if (type == typeof(int))
                return DbType.Int32;
            if (type == typeof(string))
                return DbType.String;
            return DbType.Object;
        }
    }

    internal static class IDataReaderHelper
    {
        public static object GetValue(IDataReader dataRecord, string key) => dataRecord[key];
    }
}
