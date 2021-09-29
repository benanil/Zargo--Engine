#pragma warning disable CA2211 // Non-constant fields should not be visible

using ImGuiNET;
using Microsoft.VisualBasic.FileIO;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine.Editor
{
    using Vector2 = System.Numerics.Vector2;
    using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
    using static EngineConstsants;

    public class EditorResources : EditorWindow
    {
        public static EditorResources instance;

        public readonly Texture
            FolderTexture, FileTexture,
            ThreeDTexture, FileBack,
            TextureIcon, ShaderIcon;

        public static string currentDirectory;

        private string[] files, folders;

        private readonly Stack<string> oldPaths = new Stack<string>();

        private const float fileSize = 35;

        private readonly string startPath;

        ///<summary>const args that assigned in ımguı drag drop</summary>
        public const string
            MODEL = nameof(MODEL), SOUND = nameof(SOUND),
            CODE = nameof(CODE), TEXTURE = nameof(TEXTURE),
            SHADER = nameof(SHADER), MATERIAL = nameof(MATERIAL);

        const string Dots = "..";
        const byte MaxFileNameLenght = 10;
        static readonly Vector2 backButtonSize = new Vector2(12, 12);

        public override void DrawWindow()
        {
            if (ImGui.Begin(title, ref windowActive, ImGuiWindowFlags.None | ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {

                if (ImGui.BeginTabBar("Creation"))
                {
                    if (ImGui.TabItemButton("Create"))
                    {
                        CreateMenu();
                    }
                }
                ImGui.EndTabBar();

                Hovered = ImGui.IsWindowHovered();
                Focused = ImGui.IsWindowFocused();

                // back folder button
                if (GUI.ImageButton((IntPtr)FileBack.texID, backButtonSize))
                {
                    if (oldPaths.Count != 0) {
                        currentDirectory = oldPaths.Pop();
                        RefreshCurrentDirectory();
                        ImGui.End();
                        return;
                    }
                    else {
                        currentDirectory = startPath;
                        RefreshCurrentDirectory();
                        ImGui.End();
                        return;
                    }
                }

                ImGui.SameLine();

                ImGui.SameLine();
                ImGui.Text(currentDirectory);

                // this helps for multiple columns 
                float width;
                string fileName;

                void GenerateFileName(string file)
                {
                    fileName = Path.GetFileName(file);
                    fileName = fileName.Length > MaxFileNameLenght ? fileName.Substring(0, MaxFileNameLenght) + Dots : fileName;
                }

                // folders
                for (int i = 0; i < folders.Length; i++)
                {
                    ImGui.BeginGroup();

                    void click()
                    {
                        currentDirectory = folders[i];
                        oldPaths.Push(currentDirectory);
                        RefreshCurrentDirectory();
                        ImGui.EndGroup();
                        ImGui.End();
                    }

                    if (GUI.ImageButton((IntPtr)FolderTexture.texID))
                    {
                        click();
                        return;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(Path.GetFileName(folders[i]));
                        ImGui.EndTooltip();
                        if (Input.MouseButtonUp(MouseButton.Left)){
                            click();
                            return;
                        }
                    }

                    GenerateFileName(folders[i]);
                    ImGui.Text(fileName);
                    width = ImGui.GetColumnWidth();

                    ImGui.AlignTextToFramePadding();
                    ImGui.EndGroup();

                    RightClickPopUp(folders[i]);
                    // this helps for multiple columns 
                    if (width > fileSize * 2)
                        ImGui.SameLine();
                }

                // files
                for (int i = 0; i < files.Length; i++)
                {
                    ImGui.BeginGroup();

                    IconAndElementType fileAndIcon = ChoseIconAndElementType(files[i]);
                    GUI.ImageButton(fileAndIcon.icon);

                    if (ImGui.IsItemHovered())
                    {
                        // finaly working
                        GUI.DragUIElementString(ref files[i], fileAndIcon.elementType, fileAndIcon.icon);
                        ImGui.BeginTooltip();
                        {
                            ImGui.Text(Path.GetFileName(files[i]));
                        }
                        ImGui.EndTooltip();
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            if (fileAndIcon.elementType == MATERIAL)
                            { 
                                Inspector.currentObject = AssetManager.GetMaterial(files[i]);
                            }
                            if (fileAndIcon.elementType == MODEL) // model clicked
                            {
                                GenerateMeshViewer(files[i]);
                            }
                        }
                    }

                    GenerateFileName(files[i]);
                    ImGui.TextWrapped(fileName);
                    width = ImGui.GetColumnWidth();

                    ImGui.AlignTextToFramePadding();
                    ImGui.EndGroup();

                    RightClickPopUp(files[i]);

                    // this helps for multiple columns 
                    if (width > fileSize * 2 && i < files.Length - 1)
                        ImGui.SameLine();
                }

            }
            ImGui.End();
        }

        public static void GenerateMeshViewer(string path, Action<MeshBase> action = null)
        {
            List<MeshBase> meshes = AssetManager.meshes.FindAll(m => m.path.Equals(path, StringComparison.OrdinalIgnoreCase));
            string[] names = meshes.Select((m) => m.name).Cast<string>().ToArray();
            int currentMesh = 0;
            // todo add undo redo
            if (Path.GetExtension(path) == ".mesh")
            {
                Debug.LogWarning("mesh clicked");

                FrameBuffer frameBuffer = new FrameBuffer(1024, 1024);
                PreViewCamera camera = new PreViewCamera(meshes[0].boundingBox);

                var window = new TempraryWindow("Model wiever", () =>
                {
                    if (ImGui.BeginTabBar("Viewer"))
                    {
                        if (ImGui.TabItemButton("Close"))
                        {
                            action?.Invoke(meshes[currentMesh]);
                            return true;
                        }
                    }
                    ImGui.EndTabBar();

                    GUI.EnumFieldInt(ref currentMesh, names, "Meshes");

                    frameBuffer.Bind();
                    { 
                        // basic.mat's shader
                        var shader = AssetManager.materials[2].shader;
                        shader.Use();
                        shader.SetDefaults(camera);
                        shader.SetMatrix4("model", Matrix4.Identity);

                        meshes[currentMesh].DrawOnce();

                        AssetManager.DefaultMaterial.shader.Detach();
                    }
                    frameBuffer.unbind();

                    GUI.Image((IntPtr)frameBuffer.texID, new Vector2(400, 300));

                    return false;
                });

                window.windowScaleChanged += (scale) => { frameBuffer.Invalidate(scale.X, scale.Y); };
                window.windowScaleChanged += camera.OnWindowScaleChanged;
            }
        }

        private class PreViewCamera : ICamera
        {
            private Vector3 position;
            private Matrix4 projection, view;

            public PreViewCamera(in Box3d boundingBox) {
                CalculatePosition(boundingBox);
                projection = Matrix4.CreatePerspectiveFieldOfView(90, 16/9, 0.1f, 300);
                view = Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
            }

            public void CalculatePosition(in Box3d boundingBox) { 
                position = (-Vector3.UnitZ) * Vector3.Distance((Vector3)boundingBox.Min, (Vector3)boundingBox.Max) * 2;
            }
            
            public void OnWindowScaleChanged(Vector2i Scale) {
                projection = Matrix4.CreatePerspectiveFieldOfView(90, Scale.X / Scale.Y, 0.1f, 300);
            }
            public Vector3 GetForward() => Vector3.UnitZ;
            public ref Vector3 GetPosition() => ref position;
            public ref Matrix4 GetProjectionMatrix() => ref projection;
            public Vector3 GetRight() => Vector3.UnitX;
            public Vector3 GetUp() => Vector3.UnitY;
            public ref Matrix4 GetViewMatrix() => ref view;
        }

        private enum CreationState : byte
        {
            none, material, folder, scene
        }

        private static void CreateMenu()
        {
            string materialName = string.Empty;
            string folderName = string.Empty;
            string sceneName = string.Empty;

            CreationState creationState = CreationState.none;

            new TempraryWindow("Create", () =>
            {
                if (ImGui.BeginTabBar("Create State"))
                {
                    if (ImGui.TabItemButton("Material"))
                    {
                        creationState = CreationState.material;
                    }
                    if (ImGui.TabItemButton("Folder"))
                    {
                        creationState = CreationState.folder;
                    }
                    if (ImGui.TabItemButton("Scene"))
                    {
                        creationState = CreationState.scene;
                    }
                }
                ImGui.EndTabBar();

                if (creationState == CreationState.material)
                {
                    GUI.TextField(ref materialName, "material name");

                    if (ImGui.Button("create"))
                    {
                        Material mat = new Material(AssetManager.DefaultMaterial.shader);
                        mat.path = currentDirectory + materialName + mat;
                        mat.name = materialName;
                        mat.SaveToFile();
                        return true;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("ok")) return true;
                }

                if (creationState == CreationState.folder)
                {
                    GUI.TextField(ref folderName, "folder name");

                    if (ImGui.Button("create"))
                    {
                        Directory.CreateDirectory(currentDirectory + Path.DirectorySeparatorChar + folderName);
                        RefreshCurrentDirectory();
                        return true;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("ok")) return true;
                }

                if (creationState == CreationState.scene)
                {
                    foreach (var scene in SceneManager.instance.scenes)
                    {
                        if (ImGui.Button(scene.name))
                        {
                            SceneManager.LoadScene(scene.name);
                            return true;
                        }
                    }

                    GUI.TextField(ref sceneName, "scene name");

                    if (ImGui.Button("create"))
                    {
                        Scene scene = new Scene(new RenderSaveData(), sceneName);
                        scene.path = AssetManager.AssetsPath + $"Scene{Path.DirectorySeparatorChar}" + sceneName;
                        RefreshCurrentDirectory();
                        return true;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("ok")) return true;
                }

                return null;
            });
        }

        protected override void OnGUI() { }

        // todo add scene detection
        // visual improwment: generate texture prewiew for texture files
        private IconAndElementType ChoseIconAndElementType(in string file)
        {
            string fileExtension = Path.GetExtension(file);

            return fileExtension switch
            {
                obj  or fbx  or gltf or blend or dae or ".mesh" => new(MODEL, (IntPtr)ThreeDTexture.texID), // model extensions
                png  or tga  or jpg => new(TEXTURE, (IntPtr)GetTextureOrAdd(file).texID), // texture extensions
                glsl or vert or frag => new(SHADER, (IntPtr)ShaderIcon.texID), // shader extensions
                mp3  or vaw  or ogg  => new(SOUND, (IntPtr)FileTexture.texID), // sound extensions
                cs   => new(CODE, (IntPtr)FileTexture.texID), // script texture ile degistir
                mat  => new(MATERIAL, (IntPtr)FileTexture.texID), // todo add material texture
                _ => new(TEXTURE, (IntPtr)FileTexture.texID), // default texture
            };
        }

        public record IconAndElementType(in string elementType, in IntPtr icon);

        private static void RightClickPopUp(in string path)
        {
            if (ImGui.BeginPopupContextWindow(path))
            {
                if (ImGui.MenuItem("Show In file explorer"))
                {
                    System.Diagnostics.Process.Start("Explorer.exe", path);
                }
                if (ImGui.MenuItem("Delete"))
                {
                    if (Directory.Exists(path)) {
                        FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else {
                        FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                    }
                    RefreshCurrentDirectory();
                }
                ImGui.EndPopup();
            }
        }

        public static void RefreshCurrentDirectory()
        {
            instance.folders = Directory.GetDirectories(currentDirectory);
            instance.files = Directory.GetFiles(currentDirectory);
        }


        static readonly Dictionary<string, Texture> FilesAndTextures = new Dictionary<string, Texture>();

        /// <summary>creates all texture images for prewiew</summary> 
        public static void RecrusiveScanTextureIcons(in string directory)
        {
            if (!Directory.Exists(directory)) return;

            var directories = Directory.GetDirectories(directory);
            var files = Directory.GetFiles(directory);

            short i = 0;
            for (; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]).Contains(png, jpg, tga)) // is this texture
                {
                    if (!FilesAndTextures.ContainsKey(files[i]))
                    {
                        Texture texture = AssetManager.GetTexture(files[i], generateMipMap: false);
                        texture.SetAsUI();
                        FilesAndTextures.Add(files[i], texture);
                    }
                }
            }

            for (i = 0; i < directories.Length; i++)
            {
                RecrusiveScanTextureIcons(directories[i]);
            }
        }

        private static Texture GetTextureOrAdd(in string fileName)
        {
            if (Path.GetExtension(fileName).Contains(png, jpg, tga)) // is this texture
            {
                if (!FilesAndTextures.ContainsKey(fileName))
                {
                    Texture texture = AssetManager.GetTexture(fileName, generateMipMap: false);
                    texture.SetAsUI();
                    FilesAndTextures.Add(fileName, texture);
                }
            }
            return FilesAndTextures[fileName];
        }


        public EditorResources()
        {
            instance = this; title = "Resources";

            currentDirectory = Path.GetFullPath(AssetManager.AssetsPathBackSlash);
            folders = Directory.GetDirectories(currentDirectory);
            files = Directory.GetFiles(currentDirectory);
            startPath = currentDirectory;

            FolderTexture = AssetManager.GetTexture("Images/folder.png", generateMipMap: false);
            FileBack = AssetManager.GetTexture("Images/fileBack.png", generateMipMap: false);
            FileTexture = AssetManager.GetTexture("Images/file.png", generateMipMap: false);
            ThreeDTexture = AssetManager.GetTexture("Images/3D icon.png", generateMipMap: false);
            TextureIcon = AssetManager.GetTexture("Images/Texture_Icon.png", generateMipMap: false);
            ShaderIcon = AssetManager.GetTexture("Images/Shader_Icon.png", generateMipMap: false);

            FolderTexture.SetAsUI(); FileTexture.SetAsUI();
            FileBack.SetAsUI(); ThreeDTexture.SetAsUI();
            TextureIcon.SetAsUI(); ShaderIcon.SetAsUI();

            RecrusiveScanTextureIcons(currentDirectory);
        }
    }
}
