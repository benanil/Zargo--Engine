using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ZargoEngine.Rendering
{
    using Editor;
    using Helper;
    using System.Runtime.CompilerServices;
    using SysVec3 = System.Numerics.Vector3;
    using SysVec4 = System.Numerics.Vector4;

    public class Material : IDrawable
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
        // private const string FindFloats = @"\b/[+-]?([0-9]*[.])?[0-9]+/g\b";
        /// <summary>finds floats and integers</summary>
        private const string FindNumbers = @"\b(\d+(?:\.\d+)?)\b";

        private readonly struct Attributes
        {
            internal const string DontUse = "dontUse";
            internal const string Black = "black";
            internal const string Enum = "enum";
            internal const string UniformEnd = "UniformEnd";
        }

        private readonly struct UniformType
        {
            internal const byte
                _float = 0, _vec4 = 4,
                _Int = 1, _mat4 = 5,
                _vec2 = 2, _color3 = 6,
                _vec3 = 3, _color4 = 7;
        }
        #endregion

        public string name;
        public string path;

        /// <summary>set shader via void</summary>
        public Shader shader { get; private set; }
        private readonly Dictionary<MeshBase, List<MeshRenderer>> meshList = new Dictionary<MeshBase, List<MeshRenderer>>();

        private PropHolder<float>[] floats; private PropHolder<Vector4>[] vector4s;
        private PropHolder<int>[] integers; private PropHolder<SysVec3>[] color3s;
        private PropHolder<Vector2>[] vector2s; private PropHolder<SysVec4>[] color4s;
        private PropHolder<Vector3>[] vector3s; private PropHolder<Matrix4>[] mat4s;

        private EnumData[] enumDatas;
        private TextureHolder[] textures;

        // this is for only editor, with these we are controlling shader changed or not
        private string vertexPath, fragmentPath;

        #region DataTypes
        // Rebind means: when we change shader in editor the locations are changing so we want to change locations
        internal struct TextureHolder
        {
            /// <summary> uniform location </summary>
            internal int textureLocation;
            /// <summary> texture pointer </summary>
            internal int textureID;
            internal string path;
            internal readonly string name;

            public void Rebind(Shader shader) {
                textureLocation = shader.GetUniformLocation(name);
            }

            public TextureHolder(Shader shader, string texturePath, in string name)
            {
                textureLocation = shader.GetUniformLocation(name);
                textureID = default;
                Texture texture = AssetManager.GetTexture(texturePath);

                if (texture != null) {
                    textureID = texture.texID;
                }

                this.name = name;
                this.path = texture.path;
            }
        }

        /// <summary>property holder</summary> 
        internal struct PropHolder<T> where T : struct
        {
            internal readonly string Name; internal int Location; internal T Value;
            internal void Rebind(Shader shader) {
                Location = shader.GetUniformLocation(Name);
            }

            public PropHolder(string name, int location, T value) {
                this.Name = name; this.Location = location; this.Value = value;
            }
        }

        internal struct EnumData
        {
            public int value;
            public int location;
            public readonly string name;
            public readonly string[] names;
            internal void Rebind(Shader shader) {
                location = shader.GetUniformLocation(name);
            }

            internal EnumData(in int value, Shader shader, in string name, string[] names)
            {
                this.names = names; this.value = value; this.name = name;
                location = shader.GetUniformLocation(name);
            }
        }

        #endregion

        /// <summary> Autention! empty</summary>
        public Material() { AssetManager.AddMaterial(this); }

        public Material(Shader shader) : base()
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
                    TextureHolder textureHolder = textures[t];
                    shader.SetIntLocation(textureHolder.textureLocation, t);
                    GL.ActiveTexture(TextureUnit.Texture0 + t);
                    GL.BindTexture(TextureTarget.Texture2D, textureHolder.textureID);
                }
            }

            if (shader.SupportsShadows)
            {
                shader.SetInt("shadowMap", textures.Length);
                GL.ActiveTexture(TextureUnit.Texture0 + textures.Length);
                GL.BindTexture(TextureTarget.Texture2D, Shadow.GetShadowTexture());
            }
            
            SetProperties();

            RenderMeshes();

            // unbind
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        // for depth texture and feature usages
        internal void RenderMeshes() => RenderMeshes(shader.ModelMatrixLoc);

        internal void RenderMeshes(in int modelLoc, bool shadowPass = false)
        {
            // drawing meshes bind only one mesh once and change model matrices
            foreach (var (mesh, renderers) in meshList) // key = meshBase value = MeshBase
            {
                mesh.Prepare();

                foreach (var render in renderers)
                {
                    if ((!shadowPass || !render.supportsShadows) && shadowPass) continue;
                    // model matrix working fine
                    GL.UniformMatrix4(modelLoc, true, ref render.transform.Translation);

                    // for example skinned mesh renderer does set matrixes here
                    render.Render();

                    mesh.Draw();
                }
                mesh.End();
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
                    GUI.TextureField(textures[i].name, ref textures[i].textureID, ref textures[i].path);
                    if (sameLine && i != textures.Length - 1) ImGui.SameLine(); // second condition is for not writing last texture sameline
                    sameLine = !sameLine;
                }
            }

            for (i = 0; i < vector2s.Length; i++) GUI.Vector2Field(ref vector2s[i].Value, vector2s[i].Name, SetAndDetach);
            for (i = 0; i < vector3s.Length; i++) GUI.Vector3Field(ref vector3s[i].Value, vector3s[i].Name, SetAndDetach);
            for (i = 0; i < vector4s.Length; i++) GUI.Vector4Field(ref vector4s[i].Value, vector4s[i].Name, SetAndDetach);
            for (i = 0; i < color3s.Length; i++) GUI.ColorEdit3(ref color3s[i].Value, color3s[i].Name, SetAndDetach);
            for (i = 0; i < color4s.Length; i++) GUI.ColorEdit4(ref color4s[i].Value, color4s[i].Name, SetAndDetach);
            for (i = 0; i < floats.Length; i++) GUI.FloatField(ref floats[i].Value, floats[i].Name, SetAndDetach);
            for (i = 0; i < integers.Length; i++)
            {
                GUI.IntField(ref integers[i].Value, integers[i].Name, SetAndDetach);
            }

            for (i = 0; i < enumDatas.Length; i++) {
                GUI.EnumFieldInt(ref enumDatas[i].value, enumDatas[i].names, enumDatas[i].name, SetAndDetach);
            }

            // 2 line for vertex and fragment
            ImGui.BeginGroup();
            {
                GUI.ImageButton((IntPtr)EditorResources.instance.ShaderIcon.texID, GUI._FileSize);
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
                GUI.ImageButton((IntPtr)EditorResources.instance.ShaderIcon.texID, GUI._FileSize);
                GUI.DropUIElementString(EditorResources.SHADER, (fragPath) =>
                {
                    fragmentPath = fragPath;
                });
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(fragmentPath) ?? "fragment shader name null");
            }
            ImGui.EndGroup();

            // shader buttons
            if (ImGui.Button("Compile shader")) {
                shader.Recompile();
            }
            ImGui.SameLine();
            if (ImGui.Button("Change Shader")) {
                if (vertexPath != shader.vertexPath || fragmentPath != shader.fragmentPath) {

                    Shader AfterShader = AssetManager.GetShader(vertexPath, fragmentPath);
                    Renderer3D.OnShaderChanged(this, shader, AfterShader);
                }
            }
        }

        private void SetProperties()
        {
            byte i = 0;
            for (; i < floats.Length; i++) GL.Uniform1(floats[i].Location, floats[i].Value);
            for (i = 0; i < integers.Length; i++) GL.Uniform1(integers[i].Location, integers[i].Value);
            for (i = 0; i < vector2s.Length; i++) GL.Uniform2(vector2s[i].Location, vector2s[i].Value);
            for (i = 0; i < vector3s.Length; i++) GL.Uniform3(vector3s[i].Location, vector3s[i].Value);
            for (i = 0; i < vector4s.Length; i++) GL.Uniform4(vector4s[i].Location, vector4s[i].Value);
            for (i = 0; i < color3s.Length; i++) GL.Uniform3(color3s[i].Location, color3s[i].Value.X, color3s[i].Value.Y, color3s[i].Value.Z);
            for (i = 0; i < color4s.Length; i++) GL.Uniform4(color4s[i].Location, color4s[i].Value.X, color4s[i].Value.Y, color4s[i].Value.Z, color4s[i].Value.W);
            for (i = 0; i < enumDatas.Length; i++) GL.Uniform1(enumDatas[i].location, enumDatas[i].value);
        }

        private void SetAndDetach()
        {
            shader.Use();
            SetProperties();
            shader.Detach();
        }

        #region registering
        public void ChangeShader(Shader afterShader)
        {
            if (shader != afterShader) // changed
            {
                FindOrCreate(); // we changed shader we must find other material
            }
        }

        internal void FindOrCreate()
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

        internal void TryAddRenderer(MeshRenderer renderer)
        {
            if (renderer.mesh == null) return;

            if (meshList.ContainsKey(renderer.mesh)) {

                if (!meshList[renderer.mesh].Contains(renderer))
                {
                    meshList[renderer.mesh].Add(renderer);
                }
            }
            else {
                meshList.Add(renderer.mesh, new List<MeshRenderer> { renderer });
            }
        }

        internal void TryRemoveRenderer(MeshBase mesh, MeshRenderer rendererBase)
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
            name = Path.GetFileNameWithoutExtension(shader.fragmentPath);

            this.shader = shader;
            shader.OnShaderChanged += OnShaderChanged;

            var textureList = new List<TextureHolder>();

            // total 14 byte data better than creating lists for each property, list contains 2 integer
            byte floatCount = 0, intCount = 0, vec2Count = 0, vec3Count = 0,
                 floatIndex = 0, intIndex = 0, vec2Index = 0, vec3Index = 0,
                 vec4Count = 0, color3Count = 0, color4Count = 0, mat4Count = 0,
                 vec4Index = 0, color3Index = 0, color4Index = 0, mat4Index = 0;

            StreamReader reader = File.OpenText(shader.fragmentPath);
            string line = reader.ReadLine();

            for (; string.IsNullOrEmpty(line); line = reader.ReadLine())
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

                    // checking dont use attribute this allows us not displaying shadow texture in inspector etc
                if (splits[0] == "uniform" && !(splits.Length >= 4 && splits[3] == Attributes.DontUse))
                {
                    if (splits[1] == "sampler2D")
                    {   // add texture slot, check is texture have black attribute if so, use black texture
                        Texture texture = !string.IsNullOrEmpty(splits[3]) && splits[3] == Attributes.Black ? AssetManager.DarkTexture : AssetManager.DefaultTexture;
                        textureList.Add(new TextureHolder(shader, texture.path, splits[2]));
                    }
                    else if (splits[1].Contains(uniforms) && !string.IsNullOrWhiteSpace(splits[2]) && !splits[2].Contains(defaultProperties))
                    {
                        // yandere dev
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
            reader.Dispose();

            textures = textureList.ToArray();

            floats   = new PropHolder<float>[floatCount];  vector4s = new PropHolder<Vector4>[vec4Count];
            integers = new PropHolder<int>[intCount];      color3s = new PropHolder<SysVec3>[color3Count];
            vector2s = new PropHolder<Vector2>[vec2Count]; color4s = new PropHolder<SysVec4>[color4Count];
            vector3s = new PropHolder<Vector3>[vec3Count]; mat4s = new PropHolder<Matrix4>[mat4Count];
            
            var enumDatas = new List<EnumData>();

            reader = File.OpenText(shader.fragmentPath);
            line = reader.ReadLine();

            for (; !string.IsNullOrEmpty(line); line = reader.ReadLine())
            {
                Debug.Log(line);
                Match match = Regex.Match(line, @"\b\w+\b"); // splits: uniform valueType valueName value
                string[] splits = new string[4];

                for (byte j = 0; j < 4 && match.Success; j++)
                {
                    splits[j] = match.Groups[0].Value;
                    match = match.NextMatch();
                }

                // biggest if ever, checks if we need to parse this line.
                if (splits[0] == Attributes.UniformEnd) break;
                if (!(splits.Length >= 4 && splits[3] == Attributes.DontUse)) continue;
                if (splits[0] != "uniform" || !splits[1].Contains(uniforms) || string.IsNullOrWhiteSpace(splits[2]) || splits.Length < 3) continue;
                { 
                    if (splits[1] == uniforms[0]) // float
                    {
                        if (string.IsNullOrEmpty(splits[3]))
                            floats[floatIndex] = new PropHolder<float>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), 0f);
                        else if (float.TryParse(splits[3], out float value))
                            floats[floatIndex] = new PropHolder<float>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), value);

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
                            enumDatas.Add(new EnumData(0, shader, splits[2], names.ToArray()));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(splits[3]) && int.TryParse(splits[3], out int value))
                                integers[intIndex] = new PropHolder<int>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), value);
                            else
                                integers[intIndex] = new PropHolder<int>(splits[2], GL.GetUniformLocation(shader.program, splits[2]), 0);

                            intIndex++;
                        }
                    }
                    else
                    {
                        Match numberMatch = Regex.Match(line, FindNumbers); // finds x,y,z first value is 2,3 or 4
                        string[] numbers = new string[5];

                        for (byte j = 0; j < 5 && numberMatch.Success; j++)
                        {
                            splits[j] = match.Groups[0].Value;
                            match = match.NextMatch();
                        }

                        bool condition2 = !string.IsNullOrEmpty(numbers[1]) & !string.IsNullOrEmpty(numbers[2]);
                        bool condition3 = condition2 & !string.IsNullOrEmpty(numbers[3]);
                        int location = GL.GetUniformLocation(shader.program, splits[2]);

                        if (splits[1] == uniforms[2]) { // vec2
                            vector2s[vec2Index] = new PropHolder<Vector2>(splits[2], location, condition2 ?
                                                  new (ParseF(numbers[1]), ParseF(numbers[2])) : Vector2.Zero);
                            vec2Index++;
                        }
                        else if (splits[1] == uniforms[3] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase)) 
                        { // color3
                            color3s[color3Index] = new PropHolder<SysVec3>(splits[2], location, condition3 ?
                                                   new (ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3])) : SysVec3.One);
                            color3Index++;
                        }
                        else if (splits[1] == uniforms[4] && splits[2].Contains("color", StringComparison.OrdinalIgnoreCase)) 
                        { // color4
                            color4s[color4Index] = new PropHolder<SysVec4>(splits[2], location, condition3 & !string.IsNullOrEmpty(numbers[4]) ? 
                                                   new (ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3]), ParseF(numbers[4])) : SysVec4.One);
                            color4Index++;
                        }
                        else if (splits[1] == uniforms[3]) { // vec3
                            vector3s[vec3Index] = new PropHolder<Vector3>(splits[2], location, condition3 ? 
                                                  new (ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3])) : Vector3.One);
                            vec3Index++;
                        }
                        else if (splits[1] == uniforms[4]) { // vec4
                            vector4s[vec4Index] = new PropHolder<Vector4>(splits[2], location, condition3 &
                           !string.IsNullOrEmpty(numbers[4]) ? new (ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3]), ParseF(numbers[4])) : Vector4.One); vec4Index++;
                        }
                        else if (splits[1] == uniforms[5]) { // matrix4
                            mat4s[mat4Index] = new PropHolder<Matrix4>(splits[2], location, Matrix4.Identity); mat4Index++;
                        }
                    }
                }
            }
            this.enumDatas = enumDatas.ToArray();
            reader.Dispose();
            Renderer3D.AssignMaterial(this);
        }

        private void OnShaderChanged()
        {
            // Rebind means: when we change shader in editor the locations are changing so we want to change locations
            byte i;
            for (i = 0; i < floats  .Length; i++) floats  [i].Rebind(shader);
            for (i = 0; i < integers.Length; i++) integers[i].Rebind(shader);
            for (i = 0; i < vector2s.Length; i++) vector2s[i].Rebind(shader);
            for (i = 0; i < vector3s.Length; i++) vector3s[i].Rebind(shader);
            for (i = 0; i < vector4s.Length; i++) vector4s[i].Rebind(shader);
            for (i = 0; i < color3s .Length; i++) color3s [i].Rebind(shader);
            for (i = 0; i < color4s .Length; i++) color4s [i].Rebind(shader);
            for (i = 0; i < enumDatas.Length; i++) enumDatas[i].Rebind(shader);
            for (i = 0; i < textures.Length ; i++) textures[i].Rebind(shader);
        }

        internal void SaveToFile()
        {
            path = AssetManager.GetRelativePath(path);

            if (File.Exists(path)) File.Delete(path);

            using StreamWriter writer = File.CreateText(path);
            writer.WriteLine(name);
            writer.WriteLine(AssetManager.GetRelativePath(shader.vertexPath));
            writer.WriteLine(AssetManager.GetRelativePath(shader.fragmentPath));

            byte i;
            for (i = 0; i < floats.Length; i++) writer.WriteLine($"{uniforms[UniformType._float]} {floats[i].Name} {floats[i].Value}");
            for (i = 0; i < integers.Length; i++) writer.WriteLine($"{uniforms[UniformType._Int]} {integers[i].Name} {integers[i].Value}");
            for (i = 0; i < vector2s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec2]} {vector2s[i].Name} {vector2s[i].Value}");
            for (i = 0; i < vector3s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec3]} {vector3s[i].Name} {vector3s[i].Value}");
            for (i = 0; i < vector4s.Length; i++) writer.WriteLine($"{uniforms[UniformType._vec4]} {vector4s[i].Name} {vector4s[i].Value}");
            for (i = 0; i < color3s .Length; i++) writer.WriteLine($"{uniforms[UniformType._color3]} {color3s[i].Name} {color3s[i].Value}");
            for (i = 0; i < color4s .Length; i++) writer.WriteLine($"{uniforms[UniformType._color4]} {color4s[i].Name} {color4s[i].Value}");

            for (i = 0; i < enumDatas.Length; i++) // attribute for enums
            {
                writer.Write($"enum {enumDatas[i].name} {enumDatas[i].value}");
                for (byte j = 0; j < enumDatas[i].names.Length; j++) {
                    writer.Write($" {enumDatas[i].names[j]}");
                }
                writer.Write('\n');
            }

            if (textures == null) return;
            foreach (var texture in textures) writer.WriteLine($"texture {texture.name} {AssetManager.GetRelativePath(texture.path)}");
        }

        // this function created because of readibility
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float ParseF(string value) => float.Parse(value);

        // if material doesnt exist in asset manager load from here 
        static internal Material LoadFromFile(in string path)
        {
            if (!File.Exists(path)) return default;

            using StreamReader reader = File.OpenText(path);
            Material material = new Material() { name = reader.ReadLine() };

            string vertexPath = reader.ReadLine(), fragmentPath = reader.ReadLine();

            Shader shader = material.shader = AssetManager.GetShader(vertexPath, fragmentPath);
            shader.OnShaderChanged += material.OnShaderChanged;

            const int maxProperty = 15; // this means each uniform can contain maximum amount of 15 
            var floats = new List<PropHolder<float>>(maxProperty);
            var integers = new List<PropHolder<int>>(maxProperty);
            var vector2s = new List<PropHolder<Vector2>>(maxProperty);
            var vector3s = new List<PropHolder<Vector3>>(maxProperty);
            var vector4s = new List<PropHolder<Vector4>>(maxProperty);
            var color3s = new List<PropHolder<SysVec3>>(maxProperty);
            var color4s = new List<PropHolder<SysVec4>>(maxProperty);
            var textures = new List<TextureHolder>(maxProperty);
            var enumData = new List<EnumData>(maxProperty);

            string line = reader.ReadLine();

            for(; !string.IsNullOrEmpty(line); line = reader.ReadLine())
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

                if      (type == uniforms[0]) floats.Add(new PropHolder<float>(name, location, ParseF(numbers[0])));// floatCount++;
                else if (type == uniforms[1]) integers.Add(new PropHolder<int>(name, location, int.Parse(numbers[0])));
                else if (type == uniforms[2]) vector2s.Add(new PropHolder<Vector2>(name, location, new (ParseF(numbers[0]), ParseF(numbers[1]))));
                else if (type == uniforms[3]) vector3s.Add(new PropHolder<Vector3>(name, location, new (ParseF(numbers[0]), ParseF(numbers[1]), ParseF(numbers[2]))));
                else if (type == uniforms[4]) vector4s.Add(new PropHolder<Vector4>(name, location, new (ParseF(numbers[0]), ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3]))));
      /*color3*/else if (type == uniforms[6]) color3s.Add(new PropHolder<SysVec3>(name , location, new (ParseF(numbers[0]), ParseF(numbers[1]), ParseF(numbers[2]))));
                else if (type == uniforms[7]) color4s.Add(new PropHolder<SysVec4>(name , location, new (ParseF(numbers[0]), ParseF(numbers[1]), ParseF(numbers[2]), ParseF(numbers[3]))));
                else if (type == "texture") textures.Add(new TextureHolder(shader, line[(name.Length + type.Length + 2)..], name)); // add 2 because of spaces between type and name to path
                else if (type == "enum")
                {
                    string uniformName = (words = words.NextMatch()).Groups[0].Value;
                    name = (words = words.NextMatch()).Groups[0].Value;

                    List<string> names = new List<string>();

                    while (!string.IsNullOrEmpty(name)) {
                        names.Add(name);
                        name = (words = words.NextMatch()).Groups[0].Value;
                    }

                    enumData.Add(new EnumData(int.Parse(numbers[0]), shader, uniformName, names.ToArray()));
                }
            }
            material.enumDatas = enumData.ToArray();
            material.floats = floats.ToArray(); material.integers = integers.ToArray();
            material.vector2s = vector2s.ToArray(); material.vector3s = vector3s.ToArray();
            material.vector4s = vector4s.ToArray(); material.color3s = color3s.ToArray();
            material.color4s = color4s.ToArray(); material.textures = textures.ToArray();
            material.SetProperties();
            material.vertexPath = vertexPath; material.fragmentPath = fragmentPath;
            material.path = AssetManager.GetRelativePath(path);
            Renderer3D.AssignMaterial(material);

            return material;
        }
        internal void LoadFromOtherMaterial(Material other)
        {
            textures   = other.textures;   vector4s = other.vector4s;
            floats     = other.floats;     color3s = other.color3s;
            integers   = other.integers;   color4s = other.color4s;
            vector2s   = other.vector2s;   mat4s = other.mat4s;
            vector3s   = other.vector3s;   shader = other.shader;
            enumDatas  = other.enumDatas;  
            vertexPath = other.vertexPath; fragmentPath = other.fragmentPath;
            shader.OnShaderChanged += OnShaderChanged;
        }
        #endregion // save load

        #region Setters
        public void SetTexture(int slot, Texture texture)
        {
            textures[slot] = new TextureHolder(shader, texture.path, textures[slot].name);    
        }
        public void SetEnum(in string name, in int value)
        {
            for (byte i = 0; i < enumDatas.Length; i++)
            {
                if (name != enumDatas[i].name) continue;
                enumDatas[i].value = value; break;
            }
        }

        public void SetFloat(in string name, in float value) {
            for (byte i = 0; i < floats.Length; i++) if (name == floats[i].Name) { floats[i].Value = value; break; }
        }
        public void SetInt(in string name, in int value) {
            for (byte i = 0; i < integers.Length; i++) if (name == integers[i].Name) { integers[i].Value = value; break; }
        }
        public void SetVector2(in string name, in Vector2 value) {
            for (byte i = 0; i < vector2s.Length; i++) if (name == vector2s[i].Name) { vector2s[i].Value = value; break; }
        }
        public void SetVector3(in string name, in Vector3 value) {
            for (byte i = 0; i < vector3s.Length; i++) if (name == vector3s[i].Name) { vector3s[i].Value = value; break; }
        }
        public void SetVector4(in string name, in Vector4 value) {
            for (byte i = 0; i < vector4s.Length; i++) if (name == vector4s[i].Name) { vector4s[i].Value = value; break; }
        }
        public void SetColor3(in string name, in SysVec3 value) {
            for (byte i = 0; i < color3s.Length; i++) if (name == color3s[i].Name) { color3s[i].Value = value; break; }
        }
        public void SetColor4(in string name, in SysVec4 value) {
            for (byte i = 0; i < color4s.Length; i++) if (name == color4s[i].Name) { color4s[i].Value = value; break; }
        }
        public void SetMatrix(in string name, in Matrix4 value) {
            for (byte i = 0; i < mat4s.Length; i++) if (name == mat4s[i].Name) { mat4s[i].Value = value; break; }
        }
        #endregion

        public void Dispose()
        {
            Renderer3D.RemoveMaterial(this);
            GC.SuppressFinalize(this);
        }
    }
}
