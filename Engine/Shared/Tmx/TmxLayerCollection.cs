using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.Shared.Tmx
{
    public class TmxLayerCollection : Collection<TmxLayer>
    {
        /// <summary>
        ///     <list type="bullet">
        ///         <item><description>Returns the layer with the given name, or null if not found.</description></item>
        ///         <item><description>You should probably cache this as it iterates through all of the layers until it finds one with a matching name.</description></item>
        ///         <item><description>It is case sensitive.</description></item>
        ///     </list>
        /// </summary>
        public TmxLayer this[string name]
        {
            get
            {
                return this.FirstOrDefault(layer => layer.Name == name);
            }
        }

        internal TmxLayerCollection()
        {
            
        }
    }
}