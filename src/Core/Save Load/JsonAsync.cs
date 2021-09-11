
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZargoEngine
{
    public static partial class JsonManager
    {
        public static async Task<bool> SaveAsync<T>(T obj, string directory,string file)
        { 
            //if (!Directory.Exists(directory))
            //{
            //    Debug.LogError("path directory doesnt exist");
            //    return false;
            //}

            //directory += '\\'; file += json;

            //string realFile = directory + file;
            //string jsonTxt = JsonSerializer.(obj, Formatting.None,
            //                  new JsonSerializerSettings()
            //                  {
            //                      ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //                  });

            //if (!File.Exists(realFile))
            //{
            //    using StreamWriter streamWriter = File.CreateText(realFile);
            //    streamWriter.Write(jsonTxt);
            //    return true;
            //}

            //using StreamWriter writer = new StreamWriter(directory + file);
            //writer.Write(jsonTxt);
            return true;
        }

    }
}
