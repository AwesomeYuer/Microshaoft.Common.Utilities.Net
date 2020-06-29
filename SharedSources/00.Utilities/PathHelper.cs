namespace Microshaoft
{
    using System;
    using System.IO;
    public static class PathHelper
    {
        // for linux
        // https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs
        // https://blog.lindexi.com/post/C-dotnet-%E8%A7%A3%E5%86%B3-Path-%E8%8E%B7%E5%8F%96%E6%96%87%E4%BB%B6%E5%90%8D%E8%B7%AF%E5%BE%84%E5%9C%A8-Windows-%E6%9E%84%E5%BB%BA%E5%9C%A8-Linux-%E6%89%A7%E8%A1%8C%E9%97%AE%E9%A2%98.html

        public static string GetFileNameWithoutExtension(string path)
        {
            return
                GetFileNameWithoutExtension(path.AsSpan()).ToString();
        }
        public static ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
        {
            ReadOnlySpan<char> fileName = GetFileName(path);
            int lastPeriod = fileName.LastIndexOf('.');
            return lastPeriod == -1 ?
                fileName : // No extension was found
                fileName.Slice(0, lastPeriod);
        }
        public static string GetFileName(string path)
        {
            return
                GetFileName(path.AsSpan()).ToString();
        }
        public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
        {
            int root = Path.GetPathRoot(path).Length;
            
            // We don't want to cut off "C:\file.txt:stream" (i.e. should be "file.txt:stream")
            // but we *do* want "C:Foo" => "Foo". This necessitates checking for the root.

            for (int i = path.Length; --i >= 0;)
            {
                if (i < root || path[i] == '/' || path[i] == '\\')
                    return path.Slice(i + 1, path.Length - i - 1);
            }

            return path;
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }
            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // 不是同一种路径，无法转换成相对路径。
                return toPath;
            }

            if (fromUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)
                && !fromPath.EndsWith("/") && !fromPath.EndsWith("\\"))
            {
                // 如果是文件系统，则视来源路径为文件夹。
                fromUri = new Uri(fromPath + Path.DirectorySeparatorChar);
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
