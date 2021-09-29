
using OpenTK.Windowing.Common;
using ZargoEngine.Rendering;
using ZargoEngine.SaveLoad;

namespace ZargoEngine.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private VSyncMode Vsync;

        private float fov = Camera.SceneCamera.Fov;

        const string CameraRotateSpeed = nameof(CameraRotateSpeed);
        const string CameraMoveSpeed   = nameof(CameraMoveSpeed);

        public SettingsWindow()
        {
            LoadPrefs();
            Program.MainGame.Unload += SavePrefs;
            title = "settings";
        }

        protected override void OnGUI()
        {
            if (SceneManager.currentScene == null) return;

            GUI.HeaderIn("Performance", .85f);
            GUI.EnumField(ref Vsync,nameof(Vsync), onSellect: () => Program.MainGame.VSync = Vsync);
            
            GUI.HeaderIn(nameof(Camera),.85f);
            GUI.FloatField(ref SceneManager.currentScene.cameraRotateSpeed, CameraRotateSpeed, null, .5f);
            GUI.FloatField(ref SceneManager.currentScene.cameraMoveSpeed  , CameraMoveSpeed,   null, .5f);
            GUI.FloatField(ref fov, "Fov", OnFovChanged, 1);
        }

        private void OnFovChanged()
        {
            if (SceneManager.currentScene == null) return;
            Camera.SceneCamera.Fov = fov;
            Camera.SceneCamera.UpdateVectors();
        }

        private void SavePrefs()
        {
            if (SceneManager.currentScene == null) return;
            PlayerPrefs.SetInt(nameof(Vsync), (int)Vsync);
            PlayerPrefs.SetFloat(nameof(fov), fov);
            PlayerPrefs.SetFloat(CameraRotateSpeed, SceneManager.currentScene.cameraRotateSpeed);
            PlayerPrefs.SetFloat(CameraMoveSpeed  , SceneManager.currentScene.cameraMoveSpeed  );
        }

        private void LoadPrefs()
        {
            if (SceneManager.currentScene == null) return;

            if (PlayerPrefs.TryGetFloat(CameraRotateSpeed, out float rotateSpeed))
            {
                Vsync = (VSyncMode)PlayerPrefs.GetInt(nameof(Vsync));
                SceneManager.currentScene.cameraRotateSpeed = rotateSpeed;
                SceneManager.currentScene.cameraMoveSpeed   = PlayerPrefs.GetFloat(CameraMoveSpeed );
                Camera.SceneCamera.Fov = PlayerPrefs.GetFloat(nameof(fov));
            }
        }
    }
}
