
using ImGuiNET;
using ZargoEngine.Rendering;

namespace ZargoEngine.Editor
{
    public sealed class Hierarchy : EditorWindow
    {
        static int PushID;

        public Hierarchy()
        {
            title = "Hierarchy";
        }

        protected override void OnGUI()
        {
            SceneManager.currentScene.gameObjects.ForEach(x =>
            {
                if (x.transform.parent == null)
                {
                    DrawEntityRec(x);
                }
            });

            PushID = 0;

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Create new"))
                {
                    new GameObject("new obj");
                }
                if (ImGui.MenuItem("Create Cube"))
                {
                    var go = new GameObject("Cube");
                    new MeshRenderer(MeshCreator.CreateCube(), go, AssetManager.DefaultMaterial);
                    go.transform.SetPosition(Camera.SceneCamera.Position + (Camera.SceneCamera.Front * 2), true);
                }
                if (ImGui.MenuItem("Create Sphere"))
                {
                    var go = new GameObject("Sphere");
                    new MeshRenderer(MeshCreator.CreateSphere(), go, AssetManager.DefaultMaterial);
                    go.transform.SetPosition(Camera.SceneCamera.Position + (Camera.SceneCamera.Front * 2), true);
                }
                if (CurrentObj != null)
                {
                    if (ImGui.MenuItem("Delete"))
                    {
                        deletedObject = CurrentObj;
                    }
                }
                ImGui.EndPopup();
            }

            if (deletedObject != null)
            {
                SceneManager.currentScene.gameObjects.Remove(deletedObject);
                deletedObject?.Dispose();
                Inspector.currentObject = null;
                deletedObject = null;
            }
        }

        GameObject deletedObject;
        GameObject CurrentObj;

        private void DrawEntityRec(GameObject entity)
        {
            var flags = (Inspector.currentObject != entity) ? ImGuiTreeNodeFlags.OpenOnArrow : 0 | ImGuiTreeNodeFlags.Selected;

            ImGui.PushID(PushID++);

            if (ImGui.TreeNodeEx(entity.name ?? string.Empty, flags))
            {
                // todo add drag and drop
                for (int i = 0; i < entity.transform.ChildCount; i++)
                {
                    DrawEntityRec(entity.transform.GetChild(i).gameObject);
                }
                ImGui.TreePop();
            }

            if (ImGui.IsItemClicked()) {
                Inspector.currentObject = entity;
                CurrentObj = entity;
            }

            ImGui.PopID();
        }
    }
}
