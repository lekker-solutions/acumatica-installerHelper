#region #Copyright

// ----------------------------------------------------------------------------------
//   COPYRIGHT (c) 2025 CONTOU CONSULTING
//   ALL RIGHTS RESERVED
//   AUTHOR: Kyle Vanderstoep
//   CREATED DATE: 2025/09/29
// ----------------------------------------------------------------------------------

#endregion

namespace AcumaticaInstallerHelper.Models
{
    public class VersionFormatException : Exception
    {
        public VersionFormatException(string version)
            : base($"Version '{version}' is invalid. Expected format is ##.###.####")
        {
        }
    }
}