
using ImGuiNET;
using System;
using ZargoEngine.Helper;
using ZargoEngine.Physics;
using ZargoEngine.Rendering;
using ZargoEngine.UI;

#nullable disable warnings
namespace ZargoEngine.Editor
{
    public sealed class Inspector :  IDrawable
    {
        private static bool Hovered;
        private static bool Focused;

        private bool windowActive = false;
        public string title = "Editor Window";

        public static IDrawable currentObject;
        public static Inspector instance;

        public Inspector()
        {
            instance = this;
            title = "Inspector";
        }


        readonly TitleAndAction[] inspectorRightClicks =
        {
            new TitleAndAction("Mesh Renderer", () =>
            { 
                if (currentObject is GameObject go)
                {
                    if (!go.HasComponent<MeshRenderer>())
                    {
                        var tex = AssetManager.DefaultTexture;
                        new MeshRenderer(MeshCreator.CreateCube(), go, AssetManager.DefaultMaterial);
                    }
                }
            }),
            new TitleAndAction("Text Renderer", () =>
            {
                if (currentObject is GameObject go)
                {
                    new FontRenderer(go);
                    currentObject = go;
                }
            }),
            new TitleAndAction("Collider", () =>
            {
                if (currentObject is GameObject go)
                {
                    new BepuCollider(go, 5, BepuPhysics.Collidables.CollidableMobility.Dynamic);
                    currentObject = go;
                }
            }),
            new TitleAndAction("Static Collider", () =>
            {
                if (currentObject is GameObject go)
                {
                    new BepuCollider(go, 0);
                    currentObject = go;
                }
            }),
            new TitleAndAction("Light", () =>
            {
                if (currentObject is GameObject go)
                {
                    new Light(go);
                    go.transform.scale = new OpenTK.Mathematics.Vector3(300, 300, 300);
                }
            }),
        };

        public void DrawWindow()
        {
            if (ImGui.Begin(title, ref windowActive, ImGuiWindowFlags.None))
            {
                Hovered = ImGui.IsWindowHovered();
                Focused = ImGui.IsWindowFocused();

                GUI.RightClickPopUp("inspector-rightclick", inspectorRightClicks);
         
                currentObject?.DrawWindow();
            }
            ImGui.End();
        }

        internal static void TryDelete()
        {
            if (Focused)
            {
                currentObject?.Dispose();
            }
        }

        internal static void Duplicate()
        {
            if (currentObject is GameObject old)
            {
                var newGo = new GameObject(old.name + "1");
                newGo.transform.SetMatrix(Extensions.TRS(old.transform));

                if (old.TryGetComponent(out MeshRenderer oldRenderer))
                {
                    new MeshRenderer(oldRenderer.mesh, newGo, AssetManager.DefaultMaterial);
                }
                if (newGo.TryGetComponent(out BepuCollider oldCollider))
                {
                    var collider = new BepuCollider(newGo, oldCollider.mass, oldCollider.mobility);
                    collider.UpdatePhysics = false; // for possible bugs
                }

                for (int i = 0; i < old.components.Count; i++)
                {
                    if (old.components[i] is not (MeshRenderer or BepuCollider))
                    {
                        Activator.CreateInstance(old.components[i].GetType(), newGo);
                    }
                }
            }
        }

        public void Dispose() {}
    }
}
