using System.Collections.Generic;

namespace Engine.Shared.Tmx
{
    public class TmxProperties : Dictionary<string, object>
    {
        /// <summary>Returns the value of the given property name. Returns null if it doesn't exist, as opposed to throwing an exception in any other case.</summary>
        public new object this[string key]
        {
            get { return ContainsKey(key) ? base[key] : null; }
            internal set { base[key] = value; }
        }

        public T GetValue<T>(string key)
        {
            object val = this[key];

            if (val != null)
                return (T)val;

            return default(T);
        }

        internal TmxProperties()
        {
            
        }
    }
}