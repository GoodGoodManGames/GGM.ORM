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
        private string _name = string.Empty;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = ColumnAttribute.Name ?? PropertyInfo.Name;
                return _name;
            }
        }

        private string _parameterName = string.Empty;

        public string ParameterName
        {
            get
            {
                if (string.IsNullOrEmpty(_parameterName))
                    _parameterName = string.Concat("@", Name);
                return _parameterName;
            }
        }

        private string _parameterExpression = string.Empty;

        public string ParameterExpression
        {
            get
            {
                if (string.IsNullOrEmpty(_parameterExpression))
                    _parameterExpression = string.Format("{0} = @{0}", Name);
                return _parameterExpression;
            }
        }
    }
}