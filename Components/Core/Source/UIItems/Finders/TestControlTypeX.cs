using System;
using System.Linq;
using White.Core.UIItems.Custom;

namespace White.Core.UIItems.Finders
{
    public static class TestControlTypeX
    {
        public static bool IsCustomType(this Type type)
        {            
            var isAnnotatedWithTypeMappingAttr =
                type.GetCustomAttributes(typeof (ControlTypeMappingAttribute), true).Length > 0;
            
            return isAnnotatedWithTypeMappingAttr || typeof(CustomUIItem).IsAssignableFrom(type);
        }
    }
}