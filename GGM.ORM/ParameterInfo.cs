using System.Reflection;

namespace GGM.ORM
{
    public class ParameterInfo
    {
        public ParameterInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        
        public string Name => PropertyInfo.Name;

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