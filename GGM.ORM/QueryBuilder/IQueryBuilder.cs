using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.QueryBuilder
{
    public interface IQueryBuilder<T>
    {
        string Read(int id);
        string ReadAll(object param);
        string ReadAll();
        string Create();
        string Create(T data);
        string Update(int id, T data);
        string Delete(int id);
        string DeleteAll(object param);
    }
}
