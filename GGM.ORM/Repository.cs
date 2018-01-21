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
            ParameterGeneratorCache = new Dictionary<Type, FillParamInfoGenerator>();
            DataParamGeneratorCache = new Dictionary<Type, FillDataInfoGenerator>();
            ConstructInstanceCache = new Dictionary<Type, ConstructInstance>();
            ConstructNullInstanceCache = new Dictionary<Type, ConstructNullInstance>();
            ConstructDataInstanceCache = new Dictionary<Type, ConstructDataInstance>();

            EntityManager = EntityManagerFactory.CreateFactory(assemblyName, classPath, dbOptions).CreateEntityManager();
            QueryBuilder = queryBuilder;
            if (QueryBuilder == null)
                QueryBuilder = new DefaultQueryBuilder<T>();
        }

        public delegate void FillParamInfoGenerator(IDbCommand command, object parameter);
        public delegate void FillDataInfoGenerator(IDbCommand command, T data, int id);
        public delegate T ConstructNullInstance(int id);
        public delegate T ConstructInstance(IDataReader reader);
        public delegate T ConstructDataInstance(T data, int id);
        
        public Dictionary<Type, FillParamInfoGenerator> ParameterGeneratorCache { get; set; }
        public Dictionary<Type, FillDataInfoGenerator> DataParamGeneratorCache { get; set; }
        public Dictionary<Type, ConstructInstance> ConstructInstanceCache { get; set; }
        public Dictionary<Type, ConstructNullInstance> ConstructNullInstanceCache { get; set; }
        public Dictionary<Type, ConstructDataInstance> ConstructDataInstanceCache { get; set; }      

        public EntityManager EntityManager { get; }
        public IQueryBuilder<T> QueryBuilder { get; }
        public ColumnInfo[] ColumnInfos { get; set; }

        public T Read(int id)
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Read(id);
            command.CommandType = CommandType.Text;
            EntityManager.Connection.Open();
            
            T result = default(T);

            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                if (!ConstructInstanceCache.ContainsKey(reader.GetType()))
                    ConstructInstanceCache.Add(reader.GetType(), CreateConstructInstance(reader));
                var constructingInstance = ConstructInstanceCache[reader.GetType()];
                result = constructingInstance(reader);
            }


            EntityManager.Connection.Close();
            
            return result;
        }

        public IEnumerable<T> ReadAll(object param)
        {
            ParameterException.Check(param != null, ParameterError.NotExistParameter);
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.ReadAll(param);
            command.CommandType = CommandType.Text;
            if (!ParameterGeneratorCache.ContainsKey(param.GetType()))
                ParameterGeneratorCache.Add(param.GetType(), CreateParamInfoGenerator(param));
            var fillParameterGenerator = ParameterGeneratorCache[param.GetType()];
            fillParameterGenerator(command, param);

            EntityManager.Connection.Open();

            using (var reader = command.ExecuteReader())
            {

                if (!ConstructInstanceCache.ContainsKey(reader.GetType()))
                    ConstructInstanceCache.Add(reader.GetType(), CreateConstructInstance(reader));
                var constructingInstance = ConstructInstanceCache[reader.GetType()];

                while (reader.Read())
                {
                    object row = constructingInstance(reader);
                    yield return (T) row;
                }
            }

            EntityManager.Connection.Close();
        }

        public IEnumerable<T> ReadAll()
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.ReadAll();
            command.CommandType = CommandType.Text;

            EntityManager.Connection.Open();
            
            using (var reader = command.ExecuteReader())
            {
                if (!ConstructInstanceCache.ContainsKey(reader.GetType()))
                    ConstructInstanceCache.Add(reader.GetType(), CreateConstructInstance(reader));
                var constructingInstance = ConstructInstanceCache[reader.GetType()];

                while (reader.Read())
                {
                    object row = constructingInstance(reader);
                    yield return (T) row;
                }
            }

            EntityManager.Connection.Close();
        }
        


        public T Create()
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Create();
            command.CommandType = CommandType.Text;

            EntityManager.Connection.Open();
            var id = Convert.ToInt32(command.ExecuteScalar());
            EntityManager.Connection.Close();

            if (!ConstructNullInstanceCache.ContainsKey(id.GetType()))
                ConstructNullInstanceCache.Add(id.GetType(), CreateConstructNullInstance());
            var constructingInstance = ConstructNullInstanceCache[id.GetType()];
            var result = constructingInstance(id);
            return result;

        }

        public T Create(T data)
        {
            ParameterException.Check(data != null, ParameterError.NotExistData);
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

            if (!ConstructDataInstanceCache.ContainsKey(data.GetType()))
                ConstructDataInstanceCache.Add(data.GetType(), CreateConstructDataInstance());
            var constructingInstance = ConstructDataInstanceCache[data.GetType()];
            var result = constructingInstance(data, id);
            return result;
        }

        public void Update(int id, T data)
        {
            ParameterException.Check(data != null, ParameterError.NotExistData);
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.Update(id, data);
            command.CommandType = CommandType.Text;

            if (!DataParamGeneratorCache.ContainsKey(data.GetType()))
                DataParamGeneratorCache.Add(data.GetType(), CreateDataInfoGenerator(data, id));
            var fillParameterGenerator = DataParamGeneratorCache[data.GetType()];
            fillParameterGenerator(command, data, id);

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

        public void DeleteAll()
        {
            DbCommand command = EntityManager.Connection.CreateCommand();
            command.CommandText = QueryBuilder.DeleteAll();
            command.CommandType = CommandType.Text;
            EntityManager.Connection.Open();
            command.ExecuteNonQuery();
            EntityManager.Connection.Close();
        }

        public void DeleteAll(object param)
        {
            ParameterException.Check(param != null, ParameterError.NotExistParameter);
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

        //param으로부터 값들을 받아 Parameter를 채운다
        private FillParamInfoGenerator CreateParamInfoGenerator(object param)
        {
            ParameterException.Check(param != null, ParameterError.NotExistParameter);
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

        //Parameter의 @id 부분은 id로, 나머지는 data의 값을 채워넣는다
        private FillDataInfoGenerator CreateDataInfoGenerator(Object data, int id)
        {
            ParameterException.Check(data != null, ParameterError.NotExistData);
            var type = data.GetType();
            var propertyInfos = type.GetProperties();
            ColumnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));

            var dynamicMethod = new DynamicMethod("CreateDataInfoGenerator", null, new[] { typeof(IDbCommand), typeof(object), typeof(int) });
            var il = dynamicMethod.GetILGenerator();
            var commandIL = il.DeclareLocal(typeof(IDbCommand));
            il.Emit(OpCodes.Ldarg_0); //[DbCommand]
            il.Emit(OpCodes.Stloc, commandIL); //Empty
            foreach (var property in propertyInfos)
            {
                Label isIDProperty = il.DefineLabel();
                Label endSetValue = il.DefineLabel();

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


                if(property.Name.ToLower() == primaryKey.Name.ToLower())
                {
                    //SetIDValue
                    il.Emit(OpCodes.Dup);//[Parameters][DbParameter][DbParameter]
                    il.Emit(OpCodes.Ldarg_2);//[Parameters][DbParameter][DbParameter][Value]
                }
                else
                {
                    // SetValue
                    il.Emit(OpCodes.Dup); //[Parameters][DbParameter][DbParameter]
                    il.Emit(OpCodes.Ldarg_1); ////[Parameters][DbParameter][DbParameter][object]
                    il.Emit(OpCodes.Call, property.GetGetMethod()); //[Parameters][DbParameter][DbParameter][Value]
                }
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, property.PropertyType);//[Parameters][DbParameter][DbParameter][boxed-Value]
                il.Emit(OpCodes.Callvirt, typeof(IDataParameter).GetProperty(nameof(IDataParameter.Value)).GetSetMethod()); //[Parameters][DbParameter]
                il.Emit(OpCodes.Callvirt, typeof(IList).GetMethod(nameof(IList.Add))); //[int]
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ret);

            return (FillDataInfoGenerator)dynamicMethod.CreateDelegate(typeof(FillDataInfoGenerator));
        }

        //DataReader로 부터 값들을 읽어 인스턴스를 만든다
        private ConstructInstance CreateConstructInstance(IDataReader reader)
        {
            ParameterException.Check(reader != null, ParameterError.NotExistReader);
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("ConstructInstance", type, new[] { typeof(IDataReader) }, type);
            var il = dynamicMethod.GetILGenerator();
            Label startLoop = il.DefineLabel();
            Label endLoop = il.DefineLabel();

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructor); //[instance]
            il.MarkLabel(startLoop);

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

        //id만 존재하는 인스턴스를 만든다
        private ConstructNullInstance CreateConstructNullInstance()
        {
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("ConstructNullInstance", type, new[] { typeof(Int32) }, type);
            var il = dynamicMethod.GetILGenerator();
            ColumnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructor); //[instance]
            il.Emit(OpCodes.Dup); //[instance][instance]
            il.Emit(OpCodes.Ldarg_0); //[instance][instance][id]
            il.Emit(OpCodes.Call, primaryKey.PropertyInfo.GetSetMethod());//[instance]
            il.Emit(OpCodes.Ret);

            return (ConstructNullInstance)dynamicMethod.CreateDelegate(typeof(ConstructNullInstance));
        }

        //id값과 데이터의 값을 받아 인스턴스를 만든다
        private ConstructDataInstance CreateConstructDataInstance()
        {
            var type = typeof(T);
            var dynamicMethod = new DynamicMethod("ConstructDataInstance", type, new[] { typeof(T), typeof(Int32) }, type);
            var il = dynamicMethod.GetILGenerator();
            ColumnInfos = type.GetProperties().Select(info => new ColumnInfo(info, info.GetCustomAttribute<ColumnAttribute>())).Where(info => info.ColumnAttribute != null).ToArray();
            var primaryKey = ColumnInfos.FirstOrDefault(info => info.PropertyInfo.IsDefined(typeof(PrimaryKeyAttribute)));

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, constructor); //[instance]

            PropertyInfo[] propertyinfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in propertyinfos)
            {
                Label notAutoIncrementCase = il.DefineLabel();
                Label jumpEnd = il.DefineLabel();
                
                if (property.Name.ToLower() == primaryKey.Name.ToLower())
                {
                    //check the id value
                    il.Emit(OpCodes.Ldarg_0);//[instance][T]
                    il.Emit(OpCodes.Call, property.GetGetMethod());//[instance][value]
                    il.Emit(OpCodes.Ldc_I4_0);//[instance][value][0]
                    il.Emit(OpCodes.Ceq);//[instance][result]
                    il.Emit(OpCodes.Brfalse, notAutoIncrementCase);//[instance]

                    il.Emit(OpCodes.Dup);//[instance][instance]
                    il.Emit(OpCodes.Ldarg_1); //[instance][instance][id]
                    il.Emit(OpCodes.Call, primaryKey.PropertyInfo.GetSetMethod());//[instance]
                    il.Emit(OpCodes.Br, jumpEnd);
                }

                il.MarkLabel(notAutoIncrementCase);
                il.Emit(OpCodes.Dup); //[instance][instance]
                il.Emit(OpCodes.Ldarg_0); //[instance][instance][T]
                il.Emit(OpCodes.Call, typeof(T).GetProperty(property.Name).GetGetMethod());//[instance][instance][value]
                il.Emit(OpCodes.Call, property.GetSetMethod());//[instance]
                il.MarkLabel(jumpEnd);

            }
            il.Emit(OpCodes.Ret);

            return (ConstructDataInstance)dynamicMethod.CreateDelegate(typeof(ConstructDataInstance));
        }

        private static DbType LookupDbType(Type type)
        {
            if (type == typeof(int))
                return DbType.Int32;
            else if (type == typeof(string))
                return DbType.String;
            else
            {
                throw new ParameterException(ParameterError.NotExistType);
            }
        }
    }

    internal static class IDataReaderHelper
    {
        public static object GetValue(IDataReader dataRecord, string key) => dataRecord[key];
    }
}