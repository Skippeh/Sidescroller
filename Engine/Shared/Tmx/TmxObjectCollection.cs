using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.Shared.Tmx
{
    public class TmxObjectCollection : Collection<TmxObject>
    {
        /// <summary>
        ///     <list type="bullet">
        ///         <item><description>Returns the objects with a matching name.</description></item>
        ///         <item><description>You should probably cache this as it iterates through all of the objects looking for the ones with a matching name.</description></item>
        ///         <item><description>It is case sensitive.</description></item>
        ///     </list>
        /// </summary>
        public TmxObject[] this[string name]
        {
            get
            {
                return this.Where(tileset => tileset.Name == name).ToArray();
            }
        }
    }
}