using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.Shared.Tmx
{
    public class TmxTilesetCollection : Collection<TmxTileset>
    {
        /// <summary>
        ///     <list type="bullet">
        ///         <item><description>Returns the tileset with the given name, or null if not found.</description></item>
        ///         <item><description>You should probably cache this as it iterates through all of the tilesets until it finds one with a matching name.</description></item>
        ///         <item><description>It is case sensitive.</description></item>
        ///     </list>
        /// </summary>
        public TmxTileset this[string name]
        {
            get
            {
                return this.FirstOrDefault(tileset => tileset.Name == name);
            }
        }

        internal TmxTilesetCollection()
        {
            
        }
    }
}