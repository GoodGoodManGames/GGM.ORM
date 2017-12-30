using System;
using System.Collections.Generic;
using System.Text;

namespace GGM.ORM.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : System.Attribute
    {
        public EntityAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
