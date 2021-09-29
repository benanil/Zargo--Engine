
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Helper
{
    public static class Helper
    {
        public static void OpenFolder(in string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        // for dictionaries
        public static void AddOrCreate<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Value value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
