
using System.IO;
using System.Windows.Forms;
using ZargoEngine.Editor;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    using static EngineConstsants;

    public static class AssetImporter
    {
        public static void ShowImportWindow()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "chose file",
                Filter = "All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Import(openFileDialog.FileName);
            }
        }

        public static void Import(in string path)
        {
            if (!File.Exists(path)) return;

            string extension  = Path.GetExtension(path);
            string newFile = Path.GetFullPath(EditorResources.currentDirectory + "\\" + Path.GetFileName(path));
            
            if (extension == obj || extension == fbx)
            {
                var go = AssimpImporter.ImportAssimpScene(path);

                Inspector.currentObject = go;

                // import edilen objeyi assets pathina kopyalar
                File.Copy(path, newFile, true);
                // yeni yaratılan objeyi sahne kamerasının 1 mt önüne spawnlar
                go.transform.position = Camera.SceneCamera.Position + Camera.SceneCamera.Front;
            }
            else if (extension == jpg || extension == png || extension == tga || extension == ".PNG")
            {
                File.Copy(path, newFile, true);
                AssetManager.GetTexture(path);
            }
            // todo add sound
            else
            {
                MessageBox.Show("File Type is Not supported", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            EditorResources.RefreshCurrentDirectory();
        }

        public static void LoadFileToScene(string path)
        {
            string extension = Path.GetExtension(path);

            if (extension.Contains(obj, fbx, dae, blend))
            {
                var go = AssimpImporter.ImportAssimpScene(path);
                Inspector.currentObject = go;
                // yeni yaratılan objeyi sahne kamerasının 1 mt önüne spawnlar
                go.transform.position = Camera.SceneCamera.Position + Camera.SceneCamera.Front;
            }
        }
    }
}
