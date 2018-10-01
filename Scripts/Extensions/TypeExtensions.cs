using System;
using System.Linq;
using System.Collections.Generic;


namespace UniJSON
{
    public static class TypeExtensions
    {
        public static bool GetIsGenericList(this Type t)
        {
            return t.IsGenericType 
                && (t.GetGenericTypeDefinition() == typeof(List<>));
        }

        public static bool GetIsGenericDictionary(this Type t)
        {
            return t.IsGenericType 
                && (t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                && t.GetGenericArguments().FirstOrDefault()==typeof(string)
                );
        }
    }
}
