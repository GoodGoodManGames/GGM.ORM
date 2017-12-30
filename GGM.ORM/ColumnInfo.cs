using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GGM.ORM.Attribute;

namespace GGM.ORM
{
    public class ColumnInfo
    {
        public ColumnInfo(PropertyInfo propertyInfo, ColumnAttribute columnAttribute)
        {
            PropertyInfo = propertyInfo;
            ColumnAttribute = columnAttribute;
        }

        public PropertyInfo PropertyInfo { get; }
        public ColumnAttribute ColumnAttribute { get; }
        private string mName = string.Empty;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(mName))
                    mName = ColumnAttribute.Name ?? PropertyInfo.Name;
                return mName;
            }
        }

        private string mParameterName = string.Empty;
        public string ParameterName
        {
            get
            {
                if (string.IsNullOrEmpty(mParameterName))
                    mParameterName = string.Concat("@", Name);
                return mParameterName;
            }
        }

        private string mParameterExpression = string.Empty;
        public string ParameterExpression
        {
            get
            {
                if (string.IsNullOrEmpty(mParameterExpression))
                    mParameterExpression = string.Format("{0} = @{0}", Name);
                return mParameterExpression;
            }
        }
    }
}
