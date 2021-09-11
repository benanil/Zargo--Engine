
using OpenTK.Windowing.Common;
using ZargoEngine.Rendering;
using ZargoEngine.SaveLoad;

namespace ZargoEngine.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private VSyncMode Vsync;

        private float fov = Camera.main.Fov;

        const string CameraRotateSpeed = nameof(CameraRotateSpeed);
        const string CameraMoveSpeed   = nameof(CameraMoveSpeed);

        public SettingsWindow()
        {
            LoadPrefs();
            SceneManager.currentScene.cameraRotateSpeed = 30;
            Program.MainGame.Unload += SavePrefs;
            title = "settings";
        }

        protected override void OnGUI()
        {
            GUI.HeaderIn("Performance", .85f);
            GUI.EnumField(ref Vsync,nameof(Vsync), onSellect: () => Program.MainGame.VSync = Vsync);
            
            GUI.HeaderIn(nameof(Camera),.85f);
            GUI.FloatField(ref SceneManager.currentScene.cameraRotateSpeed, CameraRotateSpeed, null, .5f);
            GUI.FloatField(ref SceneManager.currentScene.cameraMoveSpeed  , CameraMoveSpeed,   null, .5f);
            GUI.FloatField(ref fov, "Fov", OnFovChanged, 1);
        }

        private void OnFovChanged()
        {
            Camera.main.Fov = fov;
            Camera.main.UpdateVectors();
        }

        private void SavePrefs()
        {
            PlayerPrefs.SetInt(nameof(Vsync), (int)Vsync);
            PlayerPrefs.SetFloat(nameof(fov), fov);
            PlayerPrefs.SetFloat(CameraRotateSpeed, SceneManager.currentScene.cameraRotateSpeed);
            PlayerPrefs.SetFloat(CameraMoveSpeed  , SceneManager.currentScene.cameraMoveSpeed  );
        }

        private void LoadPrefs()
        {
            if (PlayerPrefs.TryGetFloat(CameraRotateSpeed, out float rotateSpeed))
            {
                Vsync = (VSyncMode)PlayerPrefs.GetInt(nameof(Vsync));
                SceneManager.currentScene.cameraRotateSpeed = rotateSpeed;
                SceneManager.currentScene.cameraMoveSpeed   = PlayerPrefs.GetFloat(CameraMoveSpeed );
                Camera.main.Fov = PlayerPrefs.GetFloat(nameof(fov));
            }
        }
    }
}
