using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace GGM.ORM
{
    /// <summary>
    ///     엔티티를 관리하는 클래스입니다. Thread-safe하지 않습니다.
    /// </summary>
    //TODO: 추후 영속성 관리 기능이 추가될 예정
    public class EntityManager
    {
        public EntityManager(DbConnection connection)
        {
            Connection = connection;
        }
        public DbConnection Connection { get; }
    }
}
