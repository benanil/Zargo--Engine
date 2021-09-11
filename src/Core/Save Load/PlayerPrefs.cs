#pragma warning disable CS8605 // Unboxing a possibly null value.
#pragma warning disable CA1416 // Validate platform compatibility

using System;
using System.Linq;
using Microsoft.Win32;
using ZargoEngine.Core;

namespace ZargoEngine.SaveLoad
{
    public static class PlayerPrefs
    {
        public const string MiddleGamesFolder = @"SOFTWARE\MiddleGames";

        static readonly RegistryKey projectKey;

        static PlayerPrefs()
        { 
            CheckOS();
            var middleGamesKey = Registry.CurrentUser.OpenSubKey(MiddleGamesFolder) ?? Registry.CurrentUser.CreateSubKey(MiddleGamesFolder);
            projectKey = middleGamesKey.OpenSubKey(ProjectSettings.ProjectName, true) ?? Registry.CurrentUser.CreateSubKey(MiddleGamesFolder + '\\' + ProjectSettings.ProjectName, true);
        }

        public static void ClearAllPlayerPrefs(){

            string[] keys = projectKey.GetValueNames();

            for (int k = 0; k < keys.Length; k++){
                projectKey.DeleteValue(keys[k]);
            }
            Debug.Log("Player Prefs cleared");
        }

        #region getters
        public static int GetInt(string name){
            TryGetInt(name, out int value);
            return value;
        }

        public static float GetFloat(string name) { TryGetFloat(name, out float value); return value; }
        public static bool GetBool(string name) { TryGetInt(name, out int value); return value == 1;}
        public static string GetString(string name) { TryGetStringValue(name, out string value); return value; }
        public static bool TryGetInt(string name,out int value) { return TryGetNumericValue(name, out value); }

        public static bool TryGetFloat(string name,out float value){
            bool canConvert = projectKey.GetSubKeyNames().Contains(name);
            if (canConvert){
                TryGetStringValue(name, out string strValue);
                value = float.Parse(strValue);
            }
            else value = default;
            return canConvert;
        }

        public static bool TryGetBool(string name,out bool value){
            bool canConvert = projectKey.GetSubKeyNames().Contains(name);
            if (canConvert) {
                TryGetNumericValue(name, out int intValue);
                value = intValue == 1;
            }
            else value = default;
            return canConvert;
        }

        public static bool TryGetStringValue(string name, out string value){
            bool canConvert = projectKey.GetValueNames().Contains(name);
            if (canConvert) value = (string)projectKey.GetValue(name);
            else value = default;
            return canConvert;
        }

        public static bool TryGetNumericValue(string name, out int value){
            bool canConvert = projectKey.GetValueNames().Contains(name);
            
            if (canConvert) value = (int)projectKey.GetValue(name);
            else value = default;
            
            return canConvert;
        }

        private static void CheckOS()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        }
        #endregion

        #region setters
        public static void SetInt(string name, int value){
            projectKey.SetValue(name, value, RegistryValueKind.DWord);
        }

        public static void SetFloat(string name, float value){
            projectKey.SetValue(name, value.ToString(), RegistryValueKind.String);
        }

        public static void SetBool(string name, bool value){
            projectKey.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
        }

        public static void SetString(string name, string value){
            projectKey.SetValue(name, value, RegistryValueKind.String);
        }
        #endregion
    }
}
