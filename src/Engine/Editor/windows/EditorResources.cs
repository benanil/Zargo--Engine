#pragma warning disable CA2211 // Non-constant fields should not be visible

using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZargoEngine.AnilTools;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

#nullable disable warnings

namespace ZargoEngine.Editor
{
    using Vector2 = System.Numerics.Vector2;
    using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
    using static EngineConstsants;
    
    public unsafe class EditorResources : EditorWindow
    {
        public static EditorResources instance;

        public readonly Texture
            folderTexture, fileTexture, 
            threeDTexture, fileBack   , 
            textureIcon  , shaderIcon ;

        public static string currentDirectory;

        private string[] files, folders;

        private readonly Stack<string> oldPaths = new Stack<string>();

        private const float fileSize = 35;

        private readonly string startPath;

        ///<summary>const args that assigned in ımguı drag drop</summary>
        public const string
            MODEL   = nameof(MODEL)  , SOUND = nameof(SOUND),
            CODE    = nameof(CODE)   , TEXTURE = nameof(TEXTURE),
            SHADER  = nameof(SHADER) , MATERIAL = nameof(MATERIAL);

        const string Dots = "..";
        const byte MaxFileNameLenght = 10;
        static readonly Vector2 backButtonSize = new Vector2(12, 12);

        private bool itemOpen, testOpen;

        public override void DrawWindow()
        {
            if (ImGui.Begin(title, ref windowActive, ImGuiWindowFlags.None | ImGuiWindowFlags.AlwaysVerticalScrollbar))
            {

                if (ImGui.BeginTabBar("Creation"))
                {
                    if (ImGui.TabItemButton("Create"))
                    {
                        CreateMaterial();
                    }
                }
                ImGui.EndTabBar();

                Hovered = ImGui.IsWindowHovered();
                Focused = ImGui.IsWindowFocused();

                // back folder button
                if (GUI.ImageButton((IntPtr)fileBack.texID, backButtonSize))
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

                    void click() {
                        currentDirectory = folders[i];
                        oldPaths.Push(currentDirectory);
                        RefreshCurrentDirectory();
                        ImGui.EndGroup();
                        ImGui.End();
                    }

                    if (GUI.ImageButton((IntPtr)folderTexture.texID)) {
                        click();
                        return;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(Path.GetFileName(folders[i]));
                        ImGui.EndTooltip();
                        if (Input.MouseButtonUp(MouseButton.Left)) {
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

                    IconAndElementType? fileAndIcon = ChoseIconAndElementType(files[i]);
                    GUI.ImageButton(fileAndIcon.icon);

                    // this line of code handles all drag end drop functions
                    // fix: this is always returning first file of the resources window
                    GUI.DragUIElementString(ref files[i], fileAndIcon.elementType, fileAndIcon.icon);  

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        {
                            ImGui.Text(Path.GetFileName(files[i]));
                        }
                        ImGui.EndTooltip();
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            if (fileAndIcon.elementType == MATERIAL) { // if material clicked
                                Inspector.currentObject = AssetManager.GetMaterial(files[i]);
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

        enum CreationState : byte
        { 
            none, material
        }

        private void CreateMaterial()
        {
            string materialName = string.Empty;
            CreationState creationState = CreationState.none;

            new TempraryWindow("Create", () =>
            {
                if (ImGui.BeginTabBar("Create State"))
                {
                    if (ImGui.TabItemButton("Material"))
                    {
                        creationState = CreationState.material;
                    }
                }
                ImGui.EndTabBar();

                if (creationState == CreationState.material)
                {
                    GUI.TextField(ref materialName, "material name");

                    if (ImGui.Button("create")) {
                        Material mat = new Material(AssetManager.DefaultMaterial.shader);
                        mat.name = materialName;
                        mat.SaveToFile();
                        return true;
                    }
                    if (ImGui.Button("ok")) return true;
                }

                return false;
            });
        }

        protected override void OnGUI() {}

        // visual improwment: generate texture prewiew for texture files
        private IconAndElementType ChoseIconAndElementType(in string file)
        {
            string fileExtension = Path.GetExtension(file);

            return fileExtension switch
            {
                obj  or fbx  or gltf or blend or dae => new (MODEL   , (IntPtr)threeDTexture.texID) , // mnodel extensions
                png  or tga  or jpg   => new (TEXTURE , (IntPtr)GetTextureOrAdd(file).texID) , // texture extensions
                glsl or vert or frag  => new (SHADER  , (IntPtr)shaderIcon.texID)    , // shader extensions
                mp3  or vaw  or ogg   => new (SOUND   , (IntPtr)fileTexture.texID)   , // sound extensions
                cs                    => new (CODE    , (IntPtr)fileTexture.texID)   , // script texture ile degistir
                mat                   => new (MATERIAL, (IntPtr)fileTexture.texID)   , // todo add material texture
                _ => new (TEXTURE, (IntPtr)fileTexture.texID), // default texture
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
                    File.Delete(path);
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

            string[] directories = Directory.GetDirectories(directory);
            string[] files = Directory.GetFiles(directory);

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

            for (i = 0; i < directories.Length; i++) {
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
            folders     = Directory.GetDirectories(currentDirectory);
            files       = Directory.GetFiles(currentDirectory);
            startPath   = currentDirectory;
            
            folderTexture = AssetManager.GetTexture("Images/folder.png"      , generateMipMap: false);
            fileBack      = AssetManager.GetTexture("Images/fileBack.png"    , generateMipMap: false);
            fileTexture   = AssetManager.GetTexture("Images/file.png"        , generateMipMap: false);
            threeDTexture = AssetManager.GetTexture("Images/3D icon.png"     , generateMipMap: false);
            textureIcon   = AssetManager.GetTexture("Images/Texture_Icon.png", generateMipMap: false);
            shaderIcon    = AssetManager.GetTexture("Images/Shader_Icon.png" , generateMipMap: false);
            
            folderTexture.SetAsUI();  fileTexture  .SetAsUI();
            fileBack     .SetAsUI();  threeDTexture.SetAsUI();
            textureIcon  .SetAsUI();  shaderIcon   .SetAsUI();
            
            RecrusiveScanTextureIcons(currentDirectory);
        }
    }
}
