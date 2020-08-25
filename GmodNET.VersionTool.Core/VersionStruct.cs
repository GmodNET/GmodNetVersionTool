using System;
using System.Collections.Generic;
using System.Text;

namespace GmodNET.VersionTool.Core
{
    /// <summary>
    /// Represents a structure of JSON version file
    /// </summary>
    public struct VersionStruct
    {
        /// <summary>
        /// Semantic Versioning 2.0.0 compatible version string
        /// </summary>
        public string Version {get; set; }
        /// <summary>
        /// Codename for a version.
        /// <remarks>
        /// Can be dropped.
        /// If specified, must be a single alphanumerical word ([0-9A-Za-z]).
        /// </remarks>
        /// </summary>
        public string Codename {get; set; }
    }
}
