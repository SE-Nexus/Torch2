using System;

namespace Torch2API.Models.Schema
{
    /// <summary>
    /// Represents the metadata linking a game world to a mod list on the panel.
    /// </summary>
    public class WorldModListMetadata
    {
        /// <summary>
        /// The name of the mod list on the panel that this world uses.
        /// </summary>
        public string ModListName { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when this metadata was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
