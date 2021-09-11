
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZargoEngine.Rendering;

#nullable disable warnings

namespace ZargoEngine
{
    public static class AssetManager
    {
        public static readonly string AssetsPath = Directory.GetCurrentDirectory() + @"..\..\..\..\Assets\";           // this paths must change when game builded
        public static readonly string AssetsPathBackSlash = Directory.GetCurrentDirectory() + @"..\..\..\..\Assets\";  // this paths must change when game builded
        private static readonly string materialDirectory = AssetsPath + "Materials";

        public readonly static List<MeshBase> meshes = new();
        public readonly static List<Texture> textures = new();
        public readonly static List<Shader> shaders = new();
        public readonly static List<Material> materials = new();
        public static Texture DefaultTexture => textures.First();
        public static Material DefaultMaterial => materials.First();
        public static Texture DarkTexture => textures[1];
        
        // creates default assets of the engine
        internal static void LoadDefaults()
        {
            GetTexture("Images/default texture.png");
            GetTexture("Images/dark_texture.png");

            GetShader("Shaders/Basic.vert", "Shaders/litShader.frag");
            GetShader("Shaders/Basic.vert", "Shaders/Basic.frag");
            GetShader("Shaders/pbr/pbr.vert", "Shaders/pbr/pbr.frag");

            if (!Directory.Exists(materialDirectory)) Directory.CreateDirectory(materialDirectory);

            AddMaterial("pbr", shaders[2]);
            AddMaterial("Lit", shaders[0]);
            AddMaterial("Basic", shaders[1]);
        }

        public static string GetRelativePath(in string file)
        {
            return Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
        }

        public static void AddMaterial(string name, Shader shader)
        { 
            if (File.Exists(AssetsPath + $"Materials/{name}.mat"))
                materials.Add(Material.LoadFromFile(AssetsPath + $"Materials/{name}.mat"));
            else
            {
                Material material = new Material(shader);
                material.name = name;
                materials.Add(material);
                material.SaveToFile();
            }
        }

        public static Shader GetShader(string vertexPath, string fragmentPath)
        {
            ProceedPath(ref vertexPath);
            ProceedPath(ref fragmentPath);

            Shader shader = shaders.Find(shader => shader.vertexPath == vertexPath &&
                                                   shader.fragmentPath == fragmentPath);
            if (shader != null) return shader;

            shader = new Shader(vertexPath, fragmentPath);

            shaders.Add(shader);

            return shader;
        }

        public static Texture GetTexture(string path, PixelFormat pixelFormat = PixelFormat.Rgba, bool generateMipMap = true)
        {
            ProceedPath(ref path);

            Texture texture = textures.Find(tex => tex.path == path);
            
            if (texture != null) return texture;

            texture = new Texture(path, pixelFormat, generateMipMap);

            textures.Add(texture);

            return texture;
        }

        public static Mesh GetMesh(in string path) => GetMeshFullPath<Mesh>(ProceedPathFunc(path));
        public static SkinnedMesh GetSkinnedMesh(in string path) => GetMeshFullPath<SkinnedMesh>(ProceedPathFunc(path));

        public static T GetMeshFullPath<T>(string path) where T : MeshBase
        {
            ProceedPath(ref path);

            if (!File.Exists(path)) {
                Debug.Log($"mesh path is not exist type:{typeof(T)} path:{path}");
                return default;
            }

            MeshBase meshBase = meshes.Find(mesh => mesh.path == path); 
            if (meshBase != null) return meshBase as T;

            if (typeof(T) == typeof(SkinnedMesh))
                meshBase = null;//AssimpImporter.LoadSkinnedMeshJava();
            else meshBase = new Mesh(GetRelativePath(path));

            meshes.Add(meshBase);

            return meshBase as T;
        }

        internal static Material GetMaterial(string materialPath)
        {
            ProceedPath(ref materialPath);

            Material material = materials.Find(mat => mat.path == materialPath);

            if (material != null) return material;

            material = Material.LoadFromFile(materialPath);
            materials.Add(material);
            return material;
        }


        private static void ProceedPath(ref string path)
        {
            if (Path.IsPathFullyQualified(path)) { // this returns true if value starts with "C:/" etc
                path = GetRelativePath(path);
            }
            else if(!path.StartsWith('.')) { // file is not alredy relative
                path = GetRelativePath(AssetsPath + path);
            }
        }

        private static string ProceedPathFunc(in string path)
        {
            // first conditions returns if path starts with "C:/"etc or not
            if (Path.IsPathFullyQualified(path) ) { // file is full path
                return GetRelativePath(path);
            }
            else if(!path.StartsWith('.')) { // file is not alredy relative
                return GetRelativePath(AssetsPath + path);
            }
            return path; // doesnt need to edit
        }

        public static string GetFileLocation(string path)
        {
            ProceedPath(ref path);

            if (!File.Exists(path)) {
                Debug.LogError("file is not exist:");
                Debug.LogError(path);
                return null;
            }
            return Path.GetFullPath(path);
        }

        public static bool TryGetFileLocation(string path, out string outPath)
        {
            ProceedPath(ref path);
            outPath = path;
            return File.Exists(path);
        }

        internal static void ClearAllAssets()
        { 
            foreach (var mesh    in meshes)      mesh.Dispose();
            foreach (var texture in textures) texture.Dispose();
            foreach (var shader  in shaders)   shader.Dispose();
        }
    }
}
