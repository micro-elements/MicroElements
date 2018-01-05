using System.ComponentModel;

namespace MicroElements.Bootstrap
{
    /// <summary>
    /// Metadata for startable component.
    /// </summary>
    public interface IStartableMetadata
    {
        /// <summary>
        /// Order of start. Can be used when you need deterministic ordered start.
        /// </summary>
        [DefaultValue(0)]
        int StartOrder { get; }
    }
}