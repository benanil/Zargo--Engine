using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#nullable disable warnings

namespace ZargoEngine.Rendering {
    using ZargoEngine.Editor;
    using ZargoEngine.Helper;
    using SysVec3 = System.Numerics.Vector3;
    using SysVec4 = System.Numerics.Vector4;

    public class Material : IDisposable , IDrawable
    {

        #region Constants
        private static readonly string[] defaultProperties =
        {
            "ambientStrength", "sunAngle" ,
            "sunIntensity"   , "viewpos"  ,
            "ambientColor"   , "sunColor" ,
            "model"
        };

        private static readonly string[] uniforms = { "float", "int", "vec2", "vec3", "vec4", "mat4", "color3", "color4" };

        /// <summary>finded from stack overflow</summary>
        private const string FindFloats = @"\b/[+-]?([0-9]*[.])?[0-9]+/g\b"; 
        /// <summary>finds floats and integers</summary>
        private const string FindNumbers = @"\b(\d+(?:\.\d+)?)\b";

        private readonly struct Attributes
        {
            internal const string DontUse = "dontUse";
            internal const string Black   = "black";
            internal const string Enum    = "enum";
            internal const string UniformEnd = "UniformEnd";
            internal const string ShadowMapName = "shadowMap";
        }

        private readonly struct UniformType
        {
            internal const byte 
                _float  = 0,  _vec4   = 4,
                _Int    = 1,  _mat4   = 5,
                _vec2   = 2,  _color3 = 6,
                _vec3   = 3,  _color4 = 7;
        }
        #endregion

        public string name;
        public string path;

        /// <summary>set shader via void</summary>
        public  Shader shader { get; private set; }
        private readonly Dictionary<MeshBase, List<RendererBase>> meshList = new Dictionary<MeshBase, List<RendererBase>>();

        private TextureHolder[]           textures;
        private PropertyHolder<float>  [] floats  ;  private PropertyHolder<Vector4>[] vector4s; 
        private PropertyHolder<int>    [] integers;  private PropertyHolder<SysVec3>[] color3s ;
        private PropertyHolder<Vector2>[] vector2s;  private PropertyHolder<SysVec4>[] color4s ;
        private PropertyHolder<Vector3>[] vector3s;  private PropertyHolder<Matrix4>[] mat4s   ;

        private List<EnumData> enumDatas;

        // this is for only editor, with these we are controlling shader changed or not
        private string vertexPath, fragmentPath;

        #region DataTypes
        internal struct TextureHolder
        {
            /// <summary> uniform location </summary>
            internal int textureLocation;
            /// <summary> texture pointer </summary>
            internal int textureID;
            internal readonly string name;
            internal readonly string path;

            public TextureHolder(Shader shader, Texture texture, in string name)
            {
                textureLocation = shader.GetUniformLocation(name);
                textureID = texture.texID;
                this.name = name;
                this.path = texture.path;
            }
        }

        internal struct PropertyHolder<T> where T : struct
        {
            internal readonly string name;  internal readonly int location; internal T value;
            public PropertyHolder(string name, int location, T value) {
                this.name = name;   this.location = location; this.value = value;
            }
        }

        internal readonly struct EnumData
        {
            public readonly int index; 
            public readonly string[] names;

            internal EnumData(int index, string[] names)
            {
                this.index = index; this.names = names;
            }
        }

        #endregion

        private Material() { }

        public Material(Shader shader)
        {
            LoadFromShader(shader);
        }

        // renders all of the assigned meshes
        internal void Render()
        {
            //set and bind textures
            if (textures != null)
            {
                for (byte t = 0; t < textures.Length; t++)
                {
                    TextureHolder textureHolder= textures[t];
                    shader.SetIntLocation(textureHolder.textureLocation, t);
                    GL.ActiveTexture(TextureUnit.Texture0 + t);
                    GL.BindTexture(TextureTarget.Texture2D, textureHolder.textureID);
                }
            }

            if (shader.SupportsShadows)
            {
                shader.SetInt("shadowMap", textures.Length);
                GL.ActiveTexture(TextureUnit.Texture0 + textures.Length);
                GL.BindTexture(TextureTarget.Texture2D, Renderer3D.GetShadowTexture());
            }

            RenderMeshes();

            // unbind
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // for depth texture and fature usages
        internal void RenderMeshes() => RenderMeshes(shader.ModelMatrixLoc);

        internal void RenderMeshes(in int modelLoc)
        {
            // drawing meshes bind only one mesh once and change model matrices
            foreach (var meshPair in meshList) // key = meshBase value = MeshBase
            {
                meshPair.Key.Prepare();

                for (int i = 0; i < meshPair.Value.Count; i++)
                {
                    // model matrix working fine
                    GL.UniformMatrix4(modelLoc, true, ref meshPair.Value[i].transform.Translation);

                    // for example skinned mesh renderer does set matrixes here
                    meshPair.Value[i].Render();

                    meshPair.Key.Draw();
                }
                meshPair.Key.End();
            }
        }

        public void DrawWindow()
        {
            GUI.HeaderIn(nameof(Material) + $" {name}");

            byte i = 0;
            if (textures != null)
            {
                bool sameLine = true;
                for (; i < textures.Length; i++)
                {
                    GUI.TextureField(textures[i].name, ref textures[i].textureID);
                    if (sameLine && i != textures.Length-1) ImGui.SameLine(); // second condition is for not writing last texture sameline
                    sameLine = !sameLine;
                }
            }

            for (i = 0; i < vector2s.Length; i++)  GUI.Vector2Field(ref vector2s[i].value, vector2s[i].name, SetProperties);
            for (i = 0; i < vector3s.Length; i++)  GUI.Vector3Field(ref vector3s[i].value, vector3s[i].name, SetProperties);
            for (i = 0; i < vector4s.Length; i++)  GUI.Vector4Field(ref vector4s[i].value, vector4s[i].name, SetProperties);
            for (i = 0; i < color3s .Length; i++)  GUI.ColorEdit3(ref color3s [i].value,   color3s [i].name, SetProperties);
            for (i = 0; i < color4s .Length; i++)  GUI.ColorEdit4(ref color4s [i].value,   color4s [i].name, SetProperties);
            for (i = 0; i < floats  .Length; i++)  GUI.FloatField(ref floats  [i].value,   floats  [i].name, SetProperties);
            for (i = 0; i < integers.Length; i++)
            {
                int index = enumDatas.FindIndex(@enum => @enum.index == i);

                if (index != -1)
                     GUI.EnumField(ref integers[i].value, enumDatas[index].names, integers[i].name, SetProperties);
                else GUI.IntField  (ref integers[i].value, integers[i].name, SetProperties);
            }

            // 2 line for vertex and fragment
            ImGui.BeginGroup();
            { 
                GUI.ImageButton((IntPtr)EditorResources.instance.shaderIcon.texID, GUI._FileSize);
                GUI.DropUIElementString(EditorResources.SHADER, (vertPath) =>
                {
                    vertexPath = vertPath;
                });
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(vertexPath) ?? "vertex shader name null");
            }
            ImGui.EndGroup();
                ImGui.SameLine();
            ImGui.BeginGroup();
            { 
                GUI.ImageButton((IntPtr)EditorResources.instance.shaderIcon.texID, GUI._FileSize);
                GUI.DropUIElementString(EditorResources.SHADER, (fragPath) =>
                {
                    fragmentPath = fragPath;
                });
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(fragmentPath) ?? "fragment shader name null");
            }
            ImGui.EndGroup();

            // shader buttons
            {
                if (ImGui.Button("Compile shader")) {
                    shader.Recompile();
                }
                ImGui.SameLine();
                if (ImGui.Button("Change Shader")) {
                    if (vertexPath != shader.vertexPath || fragmentPath != shader.fragmentPath) {

                        Shader beforeShader = shader, AfterShader = AssetManager.GetShader(vertexPath, fragmentPath);
                        Renderer3D.OnShaderChanged(this, shader, AfterShader);
                    }
                }
            }
        }

        private void SetProperties()
        {

            shader.Use();
            byte i = 0;
            for (; i < floats.Length; i++)        GL.Uniform1(floats[i].location, floats[i].value);
            for (i = 0; i < integers.Length; i++) GL.Uniform1(integers[i].location, integers[i].value);
            for (i = 0; i < vector2s.Length; i++) GL.Uniform2(vector2s[i].location, vector2s[i].value);
            for (i = 0; i < vector3s.Length; i++) GL.Uniform3(vector3s[i].location, vector3s[i].value);
            for (i = 0; i < vector4s.Length; i++) GL.Uniform4(vector4s[i].location, vector4s[i].value);
            for (i = 0; i < color3s.Length; i++)  GL.Uniform3(color3s[i].location, color3s[i].value.X, color3s[i].value.Y, color3s[i].value.Z);
            for (i = 0; i < color4s.Length; i++)  GL.Uniform4(color4s[i].location, color4s[i].value.X, color4s[i].value.Y, color4s[i].value.Z, color4s[i].value.W);
            Shader.DetachShader();
        }
        
        #region registering
        
        public void ChangeShader(Shader AfterShader)
        {
            if (shader != AfterShader) // changed
            {
                OnShaderChanged();
            }
        }

        private void OnShaderChanged()
        {
            Material containingMaterial = AssetManager.materials.Find(mat => mat.vertexPath == vertexPath &&
                                                                             mat.fragmentPath == fragmentPath);
            if (containingMaterial != null) {
                LoadFromOtherMaterial(containingMaterial);
            }
            else {
                LoadFromShader(shader);
            }
        }

        internal void TryAddRenderer(RendererBase renderer)
        {
            if (renderer.mesh == null) return;

            if (meshList.ContainsKey(renderer.mesh)) {

                if (!meshList[renderer.mesh].Contains(renderer))
                {
                    meshList[renderer.mesh].Add(renderer);
                }
            }
            else {
                meshList.Add(renderer.mesh, new List<RendererBase> { renderer });
            }
        }

        internal void TryRemoveRenderer(MeshBase mesh, RendererBase rendererBase)
        {
            if (mesh == null) return;

            if (meshList.ContainsKey(mesh))
            {
                if (meshList[mesh].Count == 1) { // remove last mesh renderer standing
                    meshList.Remove(mesh);
                }
                else {
                    meshList[mesh].Remove(rendererBase);
                }
            }
        }
        #endregion

        #region SaveLoad&Parsing
        internal void LoadFromShader(Shader shader)
        {
            vertexPath = shader.vertexPath; fragmentPath = shader.fragmentPath;
            this.shader = shader; this.enumDatas = new List<EnumData>();

            var textureList = new List<TextureHolder>();

            // total 14 byte data better than creating lists for each property, list contains 2 integer
            byte floatCount = 0, intCount = 0, vec2Count = 0, vec3Count = 0,
                 floatIndex = 0, intIndex = 0, vec2Index = 0, vec3Index = 0,
                 vec4Count = 0, color3Count = 0, color4Count = 0, mat4Count = 0,
                 vec4Index = 0, color3Index = 0, color4Index = 0, mat4Index = 0;

            StreamReader reader = File.OpenText(shader.fragmentPath);
            string line = reader.ReadLine();

            while (line != null)
            {
                Match match = Regex.Match(line, @"\b\w+\b"); // splits: uniform valueType valueName value
                string[] splits = new string[]
                {
                    match.Groups[0].Value,
                    (match = match.NextMatch()).Groups[0].Value,
                    (match = match.NextMatch()).Groups[0].Value,
                    match.NextMatch().Groups[0].Value
                };

                if (splits[0] == Attributes.UniformEnd) break;

                if (splits[0] == "uniform")
                {
                    // checking dont use attribute this allows us not displaying shadow texture in inspector etc
                    if (!(splits[3] != null && splits[3] == Attributes.DontUse))
                    {
                        if (splits[1] == "sampler2D")
                        {   // add texture slot, check is texture have black attribute if so, use black texture
                            Texture texture = !string.IsNullOrEmpty(splits[3]) && splits[3] == Attributes.Black ? AssetManager.DarkTexture : AssetManager.DefaultTexture;
                            textureList.Add(new TextureHolder(shader, texture, splits[2]));
                        }
                        else if (splits[1].Contains(uniforms) && splits.Length > 1 &&
                               !string.IsNullOrWhiteSpace(splits[2]) && !splits[2].Contains(defaultProperties))
                        {
                            if (splits[1] == uniforms[0]) floatCount++;
                            else if (splits[1] == uniforms[1]) intCount++;
                            else if (splits[1] == uniforms[2]) vec2Count++;
                            else if (splits[1] == uniforms[3] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase)) color3Count++;
                            else if (splits[1] == uniforms[4] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase)) color4Count++;
                            else if (splits[1] == uniforms[3]) vec3Count++;
                            else if (splits[1] == uniforms[4]) vec4Count++;
                            else if (splits[1] == uniforms[5]) mat4Count++;
                        }
                    }
                }
                line = reader.ReadLine();
            }
            reader.Dispose();

            textures = textureList.ToArray();

            floats   = new PropertyHolder<float>[floatCount];  integers = new PropertyHolder<int>[intCount];
            vector2s = new PropertyHolder<Vector2>[vec2Count]; vector3s = new PropertyHolder<Vector3>[vec3Count];
            vector4s = new PropertyHolder<Vector4>[vec4Count];  color3s = new PropertyHolder<SysVec3>[color3Count];
            color4s  = new PropertyHolder<SysVec4>[color4Count];  mat4s = new PropertyHolder<Matrix4>[mat4Count];

            reader = File.OpenText(shader.fragmentPath);
            line = reader.ReadLine();

            for (; line != null; line = reader.ReadLine())
            {
                Debug.Log(line);
                Match match = Regex.Match(line, @"\b\w+\b"); // splits: uniform valueType valueName value
                string[] splits = new string[4];

                for (byte j = 0; j < 4 && match.Success; j++)
                {
                    splits[j] = match.Groups[0].Value;
                    match = match.NextMatch();
                }

                if (splits[0] == Attributes.UniformEnd) break;

                if (splits.Length < 3) continue;

                if (splits[0] == "uniform" && splits[1].Contains(uniforms) && splits.Length > 1 &&
                   !string.IsNullOrWhiteSpace(splits[2]))
                {
                    if (splits[1] == uniforms[0]) // float
                    {
                        if (string.IsNullOrEmpty(splits[3]))
                            floats[floatIndex] = new PropertyHolder<float>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), 0f);
                        else if (float.TryParse(splits[3], out float value))
                            floats[floatIndex] = new PropertyHolder<float>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), value);

                        floatIndex++;
                    }
                    else if (splits[1] == uniforms[1]) // int
                    {
                        string attribute = splits[3];

                        // attribute for enum field
                        if (!string.IsNullOrEmpty(attribute) && attribute == Attributes.Enum)
                        {
                            string name = match.Groups[0].Value;
                            List<string> names = new List<string>();

                            while (!string.IsNullOrEmpty(name))
                            {
                                names.Add(name);
                                name = (match = match.NextMatch()).Groups[0].Value;
                            }
                            enumDatas.Add(new EnumData(intIndex, names.ToArray()));
                        }

                        if (!string.IsNullOrEmpty(splits[3]) && int.TryParse(splits[3], out int value))
                            integers[intIndex] = new PropertyHolder<int>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), value);
                        else
                            integers[intIndex] = new PropertyHolder<int>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), 0);

                        intIndex++;
                    }
                    else
                    {
                        Match numberMatch = Regex.Match(line, FindFloats); // finds x,y,z first value is 2,3 or 4
                        string[] numbers = new string[5];

                        for (byte j = 0; j < 5 && numberMatch.Success; j++)
                        {
                            splits[j] = match.Groups[0].Value;
                            match = match.NextMatch();
                        }

                        bool condition2 = !string.IsNullOrEmpty(numbers[1]) & !string.IsNullOrEmpty(numbers[2]);
                        bool condition3 = condition2 & !string.IsNullOrEmpty(numbers[3]);
                        int location = GL.GetUniformLocation(shader.program, splits[2]);

                        if (splits[1] == uniforms[2])
                        { // vec2
                            vector2s[vec2Index] = new PropertyHolder<Vector2>(splits[2], location, condition2 ? new Vector2(float.Parse(numbers[1]), float.Parse(numbers[2])) : Vector2.Zero);
                            vec2Index++;
                        }
                        else if (splits[1] == uniforms[3] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase))
                        { // color3
                            color3s[color3Index] = new PropertyHolder<SysVec3>(splits[2], location, condition3 ? new SysVec3(float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3])) : SysVec3.One);
                            color3Index++;
                        }
                        else if (splits[1] == uniforms[4] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase))
                        { // color4
                            color4s[vec3Index] = new PropertyHolder<SysVec4>(splits[2], location, condition3 & !string.IsNullOrEmpty(numbers[4]) ? new SysVec4(float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]), float.Parse(numbers[4])) : SysVec4.One);
                            color4Index++;
                        }
                        else if (splits[1] == uniforms[3])
                        { // vec3
                            vector3s[vec3Index] = new PropertyHolder<Vector3>(splits[2], location, condition3 ? new Vector3(float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3])) : Vector3.One);
                            vec3Index++;
                        }
                        else if (splits[1] == uniforms[4])
                        { // vec4
                            vector4s[vec4Index] = new PropertyHolder<Vector4>(splits[2], location, condition3 & !string.IsNullOrEmpty(numbers[4]) ? new Vector4(float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]), float.Parse(numbers[4])) : Vector4.One); vec4Index++;
                        }
                        else if (splits[1] == uniforms[5])
                        { // matrix4
                            mat4s[mat4Index] = new PropertyHolder<Matrix4>(splits[2], location, Matrix4.Identity); mat4Index++;
                        }
                    }
                }
            }
            reader.Dispose();
            SetProperties();
            Renderer3D.AssignMaterial(this);
        }

        internal static readonly string directory = "Materials/";

        internal void SaveToFile()
        {
            path = AssetManager.GetRelativePath(AssetManager.AssetsPath + directory + name + ".mat");

            if (!Directory.Exists(AssetManager.AssetsPath + directory)) Directory.CreateDirectory(directory);
            if (File.Exists(path)) File.Delete(path);

            using StreamWriter writer = File.CreateText(path);
            writer.WriteLine(name);
            writer.WriteLine(AssetManager.GetRelativePath(shader.vertexPath));
            writer.WriteLine(AssetManager.GetRelativePath(shader.fragmentPath));

            byte i;
            for (i = 0; i < floats.Length; i++)   writer.WriteLine($"{uniforms[UniformType._float ]} {floats[i].name} {floats[i].value}");
            for (i = 0; i < integers.Length; i++) writer.WriteLine($"{uniforms[UniformType._Int   ]} {integers[i].name} {integers[i].value}");
            for (i = 0; i < vector2s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec2  ]} {vector2s[i].name} {vector2s[i].value}");
            for (i = 0; i < vector3s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec3  ]} {vector3s[i].name} {vector3s[i].value}" );
            for (i = 0; i < vector4s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec4  ]} {vector4s[i].name} {vector4s[i].value}");
            for (i = 0; i < color3s.Length; i++)  writer.WriteLine($"{uniforms[UniformType._color3]} {color3s[i].name} {color3s[i].value}");
            for (i = 0; i < color4s.Length; i++)  writer.WriteLine($"{uniforms[UniformType._color4]} {color4s[i].name} {color4s[i].value}");
            
            for (i = 0; i < enumDatas.Count; i++) // attribute for enums
            {
                writer.Write($"enum {enumDatas[i].index}");
                for (byte j = 0; j < enumDatas[i].names.Length; j++) {
                    writer.Write($" {enumDatas[i].names[j]}");
                }
                writer.Write('\n');
            }
            
            if (textures != null)
                foreach (var item in textures) writer.WriteLine($"texture {item.name} {AssetManager.GetRelativePath(item.name)}");
        }

        // if material doesnt exist in asset manager load from here 
        static internal Material LoadFromFile(in string path)
        {
            if (!File.Exists(path)) return default;

            using StreamReader reader = File.OpenText(path);
            Material material = new Material() { name = reader.ReadLine() , enumDatas = new List<EnumData>() };

            string vertexPath = reader.ReadLine(), fragmentPath = reader.ReadLine();

            Shader shader = material.shader = AssetManager.GetShader(vertexPath, fragmentPath);

            const int maxProperty = 15; // this means each uniform can contain maximum amount of 15 
            List<PropertyHolder<float>  >   floats  = new List<PropertyHolder<float>>(maxProperty);   List<PropertyHolder<int>    >   integers = new List<PropertyHolder<int>    >(maxProperty);
            List<PropertyHolder<Vector2>> vector2s  = new List<PropertyHolder<Vector2>>(maxProperty); List<PropertyHolder<Vector3>>   vector3s = new List<PropertyHolder<Vector3>>(maxProperty);
            List<PropertyHolder<Vector4>> vector4s  = new List<PropertyHolder<Vector4>>(maxProperty); List<PropertyHolder<SysVec3>>   color3s  = new List<PropertyHolder<SysVec3>>(maxProperty);
            List<PropertyHolder<SysVec4>> color4s   = new List<PropertyHolder<SysVec4>>(maxProperty); List<TextureHolder>  textures = new List<TextureHolder>();

            string line = reader.ReadLine();

            while (!string.IsNullOrEmpty(line))
            {
                Match numberMatch = Regex.Match(line, FindNumbers); // finds x,y,z first value is 2,3 or 4
                string[] numbers = new string[5];

                for (byte j = 0; j < 5 && numberMatch.Success; j++)
                {
                    numbers[j] = numberMatch.Groups[0].Value;
                    numberMatch = numberMatch.NextMatch();
                }

                Match words = Regex.Match(line, @"\b\w+\b");
                string type = words.Groups[0].Value;
                string name = (words = words.NextMatch()).Groups[0].Value;
                int location = GL.GetUniformLocation(shader.program, name);

                     if (type == uniforms[UniformType._float ]) floats.Add(new PropertyHolder<float>  (name, location, float.Parse(numbers[0])));// floatCount++;
                else if (type == uniforms[UniformType._Int   ]) integers.Add(new PropertyHolder<int>    (name, location, int.Parse(numbers[0])));
                else if (type == uniforms[UniformType._vec2  ]) vector2s.Add(new PropertyHolder<Vector2>(name, location, new Vector2(float.Parse(numbers[0]), float.Parse(numbers[1]))));
                else if (type == uniforms[UniformType._vec3  ]) vector3s.Add(new PropertyHolder<Vector3>(name, location, new Vector3(float.Parse(numbers[0]), float.Parse(numbers[1]), float.Parse(numbers[2]))));
                else if (type == uniforms[UniformType._vec4  ]) vector4s.Add(new PropertyHolder<Vector4>(name, location, new Vector4(float.Parse(numbers[0]), float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]))));
                else if (type == uniforms[UniformType._color3]) color3s.Add(new PropertyHolder<SysVec3>(name, location, new SysVec3(float.Parse(numbers[0]), float.Parse(numbers[1]), float.Parse(numbers[2]))));
                else if (type == uniforms[UniformType._color4]) color4s.Add(new PropertyHolder<SysVec4>(name, location, new SysVec4(float.Parse(numbers[0]), float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]))));
                else if (type == "texture") textures.Add(new TextureHolder(shader, AssetManager.GetTexture(line[(name.Length + type.Length + 2)..]), name)); // add 2 because of spaces between type and name to path
                else if (type == "enum")
                {
                    name = (words = words.NextMatch()).Groups[0].Value;
                    List<string> names = new List<string>() { name };
                    
                    while (!string.IsNullOrEmpty(name)) {
                        names.Add(name);
                        name = (words = words.NextMatch()).Groups[0].Value;
                    }
                    material.enumDatas.Add(new EnumData(int.Parse(numbers[0]), names.ToArray()));
                }

                line = reader.ReadLine();
            }
            material.floats   = floats.ToArray();   material.integers = integers.ToArray();
            material.vector2s = vector2s.ToArray(); material.vector3s = vector3s.ToArray();
            material.vector4s = vector4s.ToArray(); material.color3s  = color3s.ToArray();
            material.color4s  = color4s.ToArray();  material.textures = textures.ToArray();
            material.SetProperties();
            material.vertexPath = vertexPath; material.fragmentPath = fragmentPath;
            material.path = AssetManager.GetRelativePath(path);
            Renderer3D.AssignMaterial(material);
            return material;
        }
        private void LoadFromOtherMaterial(Material other)
        {
            textures = other.textures; vector4s = other.vector4s;
            floats   = other.floats  ; color3s  = other.color3s;
            integers = other.integers; color4s  = other.color4s;
            vector2s = other.vector2s; mat4s    = other.mat4s;
            vector3s = other.vector3s;
        }
        #endregion // save load

        #region Setters
        public void SetFloat(in string name, in float value) {
            for (byte i = 0; i < floats.Length; i++)  if (name == floats[i].name) { floats[i].value = value; break; }
            shader.SetFloat(name, value);
        }
        public void Setint(in string name, in int value) {
            for (byte i = 0; i < integers.Length; i++)  if (name == integers[i].name) { integers[i].value = value; break; }
            shader.SetInt(name, value);
        }
        public void SetVector2(in string name, in Vector2 value) {
            for (byte i = 0; i < vector2s.Length ; i++) if (name == vector2s[i].name) { vector2s[i].value = value; break; }
            shader.SetVector2(name, value);
        }
        public void SetVector3(in string name, in Vector3 value) {
            for (byte i = 0; i < vector3s.Length ; i++) if (name == vector3s[i].name) { vector3s[i].value = value; break; }
            shader.SetVector3(name, value);
        }
        public void SetVector4(in string name, in Vector4 value) {
            for (byte i = 0; i < vector4s.Length; i++) if (name == vector4s[i].name) { vector4s[i].value = value; break; }
            shader.SetVector4(name, value);
        }
        public void SetColor3(in string name, in SysVec3 value) {
            for (byte i = 0; i < color3s.Length; i++) if (name == color3s[i].name) { color3s[i].value = value; break; }
            shader.SetVector3Sys(name, value);
        }
        public void SetColor4(in string name, in SysVec4 value) {
            for (byte i = 0; i < color4s.Length; i++) if (name == color4s[i].name) { color4s[i].value = value; break; }
            shader.SetVector4Sys(name, value);
        }
        public void SetMatrix(in string name, in Matrix4 value) {
            for (byte i = 0; i < mat4s.Length; i++) if (name == mat4s[i].name) { mat4s[i].value = value; break; }
            shader.SetMatrix4(name, value);
        }
        #endregion

        public void Dispose()
        {
            Renderer3D.RemoveMaterial(this);
            GC.SuppressFinalize(this);
        }
    }
}
