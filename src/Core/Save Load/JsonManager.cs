#pragma warning disable CS8603 // Possible null reference return.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZargoEngine
{
    public static partial class JsonManager
    {
        public const string json = ".json";

        [Serializable]
        public struct ArrayHolder<T>
        {
            public T[] Data;

            public ArrayHolder(T[] data){
                Data = data;
            }
        }

        public static string EnsurePath(string suredPath, string desiredPath, char seperator = '/')
        {
            Queue<string> filePaths = new Queue<string>(desiredPath.Split(seperator));
            string currentFile = Path.Combine(suredPath, filePaths.Dequeue());
            
            while (!Directory.Exists(Path.Combine(suredPath + currentFile)))
            {
                Directory.CreateDirectory(currentFile);
                var newPath = filePaths.Dequeue();
                if (filePaths.Count == 1 || newPath == string.Empty) break;
                currentFile = Path.Combine(currentFile, newPath);
            }

            return Path.Combine(suredPath + desiredPath);
        }

        public static bool Save<T>(this T obj,string directory, string file)
        {
            if (!Directory.Exists(directory)){
                Debug.LogError("path directory doesnt exist");
                return false;
            }

            directory += '\\'; file += json;

            string realFile = directory + file;
            string jsonTxt = JsonConvert.SerializeObject(obj);

            if (!File.Exists(realFile)){
                using StreamWriter streamWriter = File.CreateText(realFile);
                streamWriter.Write(jsonTxt);
                return true;
            }

            using StreamWriter writer = new StreamWriter(directory + file);
            writer.Write(jsonTxt);
            return true;
        }

        public static bool SaveStream<T>(this T obj, StreamWriter writer)
        {
            string jsonTxt = JsonConvert.SerializeObject(obj);
            using (writer) writer.Write(jsonTxt);
            return true;
        }

        public static bool SaveArray<T>(this T[] obj,string directory, string file)
        {
            if (!Directory.Exists(directory)){
                Debug.LogError("path directory doesnt exist");
                return false;
            }
            
            directory += '\\'; file += json;

            var convertedObj = new ArrayHolder<T>(obj);
            string realFile = directory + file;
            string jsonText = JsonConvert.SerializeObject(convertedObj,Formatting.None,
                              new JsonSerializerSettings()
                              { 
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                              });

            if (!File.Exists(realFile)){
                using StreamWriter streamWriter = File.CreateText(realFile);
                streamWriter.Write(jsonText);
                return true;
            }

            using StreamWriter writer = new StreamWriter(directory + file);
            writer.Write(jsonText);
            
            return true;
        }

        public static bool SaveArrayStream<T>(this T[] obj, StreamWriter writer)
        {
            var convertedObj = new ArrayHolder<T>(obj);
            string jsonText  = JsonConvert.SerializeObject(convertedObj, Formatting.None,
                              new JsonSerializerSettings()
                              {
                                  ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                  MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                              });
            
            using (writer) writer.Write(jsonText);

            return true;
        }

        public static T Load<T>(string file) 
        { 
            if (!File.Exists(file))
            {
                Debug.LogError("path directory doesnt exist");
                return default;
            }

            using StreamReader reader = new StreamReader(file);
            string convertedTxt = reader.ReadToEnd();

            var serializedObject = JsonConvert.DeserializeObject<T>(convertedTxt);

            if (serializedObject == null)
                Debug.LogError("file load failed");

            return serializedObject;
        }

        public static T[] LoadArray<T>(string file)
        {
            if (!File.Exists(file)){
                Debug.LogError("path directory doesnt exist");
                return null;
            }

            using StreamReader reader = new StreamReader(file);
            string convertedTxt = reader.ReadToEnd();

            var serializedObject = JsonConvert.DeserializeObject<ArrayHolder<T>>(convertedTxt).Data;

            if (serializedObject == null)
                Debug.LogError("file load failed");

            return serializedObject;
        }
    }
}
