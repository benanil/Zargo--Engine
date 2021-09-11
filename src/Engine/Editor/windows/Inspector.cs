
using ZargoEngine.Helper;
using ZargoEngine.Physics;
using ZargoEngine.Rendering;
using ZargoEngine.UI;

namespace ZargoEngine.Editor
{
    public sealed class Inspector : EditorWindow
    {
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

        protected override void OnGUI()
        {
            GUI.RightClickPopUp("inspector-rightclick", inspectorRightClicks);
            
            currentObject?.DrawWindow();
        }

        internal static void Duplicate()
        {
            if (currentObject is GameObject old)
            {
                var go = new GameObject(old.name + "1");
                go.transform.SetMatrix(Extensions.TRS(old.transform));

                if (go.TryGetComponent(out MeshRenderer oldRenderer))
                {
                    go.AddComponent(new MeshRenderer((Mesh)oldRenderer.mesh, oldRenderer.gameObject, AssetManager.DefaultMaterial));
                }

                if (go.TryGetComponent(out BepuCollider oldCollider))
                {
                    var collider = new BepuCollider(go, oldCollider.mass, oldCollider.mobility);
                    collider.UpdatePhysics = false; // for possible bugs
                    go.AddComponent(collider);
                }
            }
        }
    }
}
