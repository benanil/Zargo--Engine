
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
                    DrawEntity(x);
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
                    go.transform.SetPosition(Camera.main.Position + (Camera.main.Front * 2), true);
                }
                if (ImGui.MenuItem("Create Sphere"))
                {
                    var go = new GameObject("Sphere");
                    new MeshRenderer(MeshCreator.CreateSphere(), go, AssetManager.DefaultMaterial);
                    go.transform.SetPosition(Camera.main.Position + (Camera.main.Front * 2), true);
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

        private void DrawEntity(GameObject entity)
        {
            var flags = (Inspector.currentObject != entity) ? ImGuiTreeNodeFlags.OpenOnArrow : 0 | ImGuiTreeNodeFlags.Selected;
            
            if (ImGui.TreeNodeEx(entity.name, flags))
            {
                // todo make tree great again!
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
           // slowDebugger.LogSlow("drawing entity: " + entity.name);
        }
    }
}
