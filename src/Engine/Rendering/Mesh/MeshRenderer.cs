#nullable disable warnings

using Coroutine;
using ImGuiNET;
using ZargoEngine.Editor;

namespace ZargoEngine.Rendering
{
    public class MeshRenderer : Companent
    {
        private MeshBase _mesh;
        public MeshBase mesh
        {
            get => _mesh;
            set
            {
                if (value == null) {
                    Debug.Log("you try to add null material");
                    if (Material != null)
                        _material.TryRemoveRenderer(_mesh, this);
                    _mesh = value;
                }
                else {
                    if (mesh != null && Material != null)
                        _material.TryRemoveRenderer(_mesh, this);
                    
                    _mesh = value;
                    if (_material == null) {
                        Debug.Log("changed material is null please insert other material or fix code");
                        return;
                    }
                    _material.TryAddRenderer(this);
                }
            }
        }
        
        private Material _material;
        public Material Material
        {
            get => _material;
            set
            {
                if (value == null)
                {
                    Debug.Log("material setted to null");
                    _material.TryRemoveRenderer(_mesh, this);
                    _material = value;
                }
                else if (value != _material) {
                    if (Material != null)
                        _material.TryRemoveRenderer(mesh, this); // remove this renderer from old material
                    _material = value;
                    _material.TryAddRenderer(this); // add this to new material
                }
            }
        }

        public int TrianglesCount => mesh.Positions.Length / 3;

       /// <summary> used for serializing </summary>
        public string MaterialPath;
        /// <summary> used for serializing </summary>
        public string MeshPath;

        // this is most likely used for moving objects because
        // we need to add projection shadow soon
        public bool supportsShadows = true;

        public new virtual void Render() { }

        public MeshRenderer(MeshBase mesh, GameObject gameObject, Material material) : base(gameObject)
        {
            name = "Mesh Renderer";

            _mesh = mesh; 
            this.Material = material;  
            
            CoroutineHandler.InvokeLater(new Wait(.31f), () =>
            {
                transform.UpdateTranslation(); // this will also calculates shadows
            });
            transform.OnTransformChanged += delegate(ref OpenTK.Mathematics.Matrix4 mat) 
            { 
                // we dont want to calculate shadow for moving objects
                if (supportsShadows) {
                    Shadow.UpdateShadows();
                }
            };
        }

        public override void DrawWindow()
        {
            if (ImGui.CollapsingHeader(name, ImGuiTreeNodeFlags.CollapsingHeader))
            { 
                mesh = GUI.ModelField(mesh);
            
                Material?.DrawWindow();
                Material = GUI.MaterialField(Material);

                ImGui.Text("triangles: " + TrianglesCount.ToString());
            }
        }

        public override void Dispose()
        {
            _material?.TryRemoveRenderer(_mesh, this);
            if (supportsShadows) Shadow.UpdateShadows();

            base.Dispose();
        }
    }
}

