namespace Microshaoft
{
    using System.Collections.Generic;
    using System.IO;
    public static class DirectoryHelper
    {
        public static void MakeDirectories
                            (
                                IEnumerable<string> directories
                                , bool ignoreRemoteDirectory = true
                            )
        {
            foreach (var directory in directories)
            {
                if
                    (
                        !directory
                                .IsNullOrEmptyOrWhiteSpace()
                    )
                {
                    if (directory.StartsWith("\\"))
                    {
                        if (ignoreRemoteDirectory)
                        {
                            continue;
                        }
                    }
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
            }
        }
    }
}
