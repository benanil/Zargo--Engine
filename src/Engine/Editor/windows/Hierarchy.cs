
using ImGuiNET;
using ZargoEngine.Rendering;

namespace ZargoEngine.Editor
{
    public sealed class Hierarchy : EditorWindow
    {
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

        private void DrawEntityRec(GameObject entity)
        {
            var flags = (Inspector.currentObject != entity) ? ImGuiTreeNodeFlags.OpenOnArrow : 0 | ImGuiTreeNodeFlags.Selected;

            if (entity.name != null && ImGui.TreeNodeEx(entity.name ?? string.Empty, flags))
            {
                // todo add drag and drop
                ImGui.TreeNodeEx(entity.name ?? string.Empty);

                for (int i = 0; i < entity.transform.ChildCount; i++)
                {
                    DrawEntityRec(entity.transform.GetChild(i).gameObject);
                }
                
                ImGui.TreePop();
            }

            if (ImGui.IsItemClicked()) {
                Inspector.currentObject = entity;
            }

            if (Inspector.currentObject == entity)
            {
                if (ImGui.BeginPopupContextWindow())
                {
                    if (ImGui.MenuItem("Delete")){
                        deletedObject = entity;
                    }
                    ImGui.EndPopup();
                }
            }

        }
    }
}
