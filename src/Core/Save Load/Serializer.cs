
#nullable disable warnings
namespace ZargoEngine 
{
    using OpenTK.Mathematics;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Rendering;
    using SaveLoad;

    public static class Serializer
    {
        public static void GetWriter(string path, out BinaryWriter writer, out FileStream stream)
        {
            stream = new FileStream(AssetManager.AssetsPath + path + ".bin", FileMode.Create);
            writer = new BinaryWriter(stream);
        }

        public static void GetReader(string path, out BinaryReader writer, out FileStream stream)
        {
            stream = new FileStream(AssetManager.AssetsPath + path + ".bin", FileMode.Open);
            writer = new BinaryReader(stream);
        }

        /// <summary> saves as a binary to target path \n doesn't touch arrays</summary>
        public static void SaveData<T>(BinaryWriter writer, T obj)
        {
            FieldInfo[] fields = typeof(T).GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var type = fields[i].FieldType;
                var value = fields[i].GetValue(obj);

                if      (type == typeof(bool))   writer.Write(value != null && (bool)value);
                else if (type == typeof(float))  writer.Write((float)value);
                else if (type == typeof(int))    writer.Write((int)value);
                else if (type == typeof(short))  writer.Write((short)value);
                else if (type == typeof(byte))   writer.Write((byte)value);
                else if (type == typeof(string)) writer.Write((string)value);
                else if (type == typeof(Array))  continue;
                else if (type.IsValueType)
                {
                    WriteMethodInfo.MakeGenericMethod(type).Invoke(null, new object[] { writer, value });
                }
            }
        }

        /// <summary>loads object from path doesn't touch arrays</summary>
        public static T LoadData<T>(BinaryReader reader)
        {
            FieldInfo[] fields = typeof(T).GetFields();
            T instance = Activator.CreateInstance<T>();

            for (int i = 0; i < fields.Length; i++)
            {
                Type type = fields[i].FieldType;

                if      (type == typeof(bool))   fields[i].SetValue(instance, reader.ReadBoolean());
                else if (type == typeof(float))  fields[i].SetValue(instance, reader.ReadSingle());
                else if (type == typeof(int))    fields[i].SetValue(instance, reader.ReadInt32());
                else if (type == typeof(short))  fields[i].SetValue(instance, reader.ReadInt16());
                else if (type == typeof(byte))   fields[i].SetValue(instance, reader.ReadByte());
                else if (type == typeof(string)) fields[i].SetValue(instance, reader.ReadString());
                else if (type == typeof(Array))  continue;
                else if (type.IsValueType)
                {
                    object value = ReadMethodInfo.MakeGenericMethod(type).Invoke(null, new object[] { reader });
                    fields[i].SetValue(instance, value);
                }
            }
            return instance;
        }

        #region For_Scene
        internal static void SerializeScene(SceneSaveData sceneSave, string path)
        {
            using FileStream stream = new FileStream(path, FileMode.Create);
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(sceneSave.Name ?? "scene");
            writer.Write(sceneSave.savedGOs.Length);

            WriteStruct(writer, sceneSave.cameraPos);
            WriteStruct(writer, sceneSave.cameraUp);
            WriteStruct(writer, sceneSave.cameraForward);
            SaveData(writer, sceneSave.RenderSaveData);

            for (int i = 0; i < sceneSave.savedGOs.Length; i++)
            {
                GameObjectSaveData savedGO = sceneSave.savedGOs[i];
                SaveData(writer, savedGO);
                writer.Write(savedGO.companentDatas.Length);

                for (int j = 0; j < savedGO.companentDatas.Length; j++)
                {
                    CompanentData companentData = savedGO.companentDatas[j];
                    SaveData(writer, companentData);
                    SaveFields(writer, companentData.fieldDatas);
                }
            }
            WriteStruct(writer, Shadow.Settings);
        }

        internal static SceneSaveData DeserializeScene(string path)
        {
            SceneSaveData sceneSave = new SceneSaveData();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                using BinaryReader reader = new BinaryReader(stream);
                sceneSave.Name = reader.ReadString();
                sceneSave.savedGOs = new GameObjectSaveData[reader.ReadInt32()];

                sceneSave.cameraPos = ReadStruct<Vector3>(reader);
                sceneSave.cameraUp = ReadStruct<Vector3>(reader);
                sceneSave.cameraForward = ReadStruct<Vector3>(reader);

                sceneSave.RenderSaveData = LoadData<RenderSaveData>(reader);

                for (int i = 0; i < sceneSave.savedGOs.Length; i++)
                {
                    GameObjectSaveData savedGO = LoadData<GameObjectSaveData>(reader);

                    savedGO.companentDatas = new CompanentData[reader.ReadInt32()];
                    for (int j = 0; j < savedGO.companentDatas.Length; j++)
                    {
                        CompanentData companentData = LoadData<CompanentData>(reader);
                        companentData.fieldDatas = LoadFields(reader);
                        savedGO.companentDatas[j] = companentData;
                    }
                    sceneSave.savedGOs[i] = savedGO;
                }
                
                ShadowSettings settings = default;
                try
                {
                    settings = ReadStruct<ShadowSettings>(reader);
                }
                catch { settings = ShadowSettings.Default; }
                finally
                {
                    Shadow.Set(settings);
                }
            }

            return sceneSave;
        }
        
        public static void SaveFields(BinaryWriter writer, FieldData[] array) 
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i].value);
                writer.Write(array[i].assemblyQualified);
                writer.Write(array[i].fieldName);
            }
        }

        public static FieldData[] LoadFields(BinaryReader reader) 
        {
            FieldData[] array = new FieldData[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new FieldData(reader.ReadString(), reader.ReadString(), reader.ReadString());
            }
            return array;
        }
        #endregion

        public static void SaveStrings<T>(BinaryWriter writer, string[] array) 
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public static string[] LoadStrings(BinaryReader reader) 
        {
            string[] array = new string[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = reader.ReadString();
            }
            return array;
        }

        public static void SaveArray<T>(BinaryWriter writer, T[] array) where T : struct
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                WriteStruct(writer, array[i]);
            }
        }

        public static T[] LoadArray<T>(BinaryReader reader) where T : struct
        {
            T[] array = new T[reader.ReadInt32()];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = ReadStruct<T>(reader);
            }
            return array;
        }

        public static void LoadArray<T>(BinaryReader reader, out T[] result) where T : struct
        {
            result = new T[reader.ReadInt32()];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ReadStruct<T>(reader);
            }
        }

        static readonly MethodInfo ReadMethodInfo = typeof(Serializer).GetMethod("ReadStruct", BindingFlags.Static | BindingFlags.NonPublic);
        private static T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] bytes = reader.ReadBytes(size);
            Marshal.Copy(bytes, 0, ptr, size);
            T value = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return value;
        }

        static readonly MethodInfo WriteMethodInfo = typeof(Serializer).GetMethod("WriteStruct", BindingFlags.Static | BindingFlags.NonPublic);
        private static void WriteStruct<T>(BinaryWriter writer, T value) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            writer.Write(bytes);
        }
    }
}
