using System;
using System.IO;

namespace dVRC
{
    public static class IOTools
    {
        // https://stackoverflow.com/a/7710620/12968919
        public static bool IsChildDirectory(string parentPath, string childPath)
        {
            var parentUri = new Uri(parentPath);
            var childUri = new DirectoryInfo(childPath);
            while (childUri != null)
            {
                if(new Uri(childUri.FullName) == parentUri)
                {
                    return true;
                }
                childUri = childUri.Parent;
            }
            return false;
        }
    }
}