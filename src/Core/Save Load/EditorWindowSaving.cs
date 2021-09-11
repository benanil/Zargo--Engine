using OpenTK.Mathematics;
using System;
using ZargoEngine.Core;

namespace ZargoEngine.Editor
{
    public static class EditorWindowSaving
    {

        public static void Save()
        {
            /*
            JsonManager.Save(new WindowData(Program.MainGame.ClientRectangle, Screen.FullScreen), 
                JsonManager.EnsurePath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"/Middle Games/{ProjectSettings.ProjectName}/")
                , $"{nameof(WindowData)}");
            */
        }

        public static void Load()
        {
            /*

            JsonManager.Load<WindowData>(
                JsonManager.EnsurePath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"/Middle Games/{ProjectSettings.ProjectName}/")
                + $"{nameof(WindowData)}").Apply();
            */
       }
        
        [Serializable]
        public struct WindowData
        {
            public Box2i rectangle;
            public bool isFullScreen;

            public void Apply()
            {
                if (rectangle != default && rectangle.Size != Vector2i.Zero)
                {
                    Program.MainGame.ClientRectangle = rectangle;
                }

                Program.MainGame.CenterWindow();
            }

            public WindowData(Box2i rectangle, bool isFullScreen) 
            {
                this.rectangle = rectangle;
                this.isFullScreen = isFullScreen;
            }
        }
    }
}
