using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine.Editor
{
    using static EngineConstsants;
    using SysVec2 = System.Numerics.Vector2;
    using SysVec3 = System.Numerics.Vector3;
    using SysVec4 = System.Numerics.Vector4;

    public record TitleAndAction(in string title, in Action Action);
    
    public static unsafe class GUI
    {
        private enum OnOf
        { 
            off = 0,on = 1
        }

        public const byte TextMaxLength = 20;
        public const string IsActive = nameof(IsActive);

        private static readonly SysVec2 multinileTextSize = new SysVec2(100, 50);

        /// <summary>
        /// for bools
        /// </summary>
        public static void OnOfField(ref bool value, string Header = IsActive, FieldInfo fieldInfo = null, object @object = null,
                                        Action onSellect = null, ImGuiComboFlags comboFlags = ImGuiComboFlags.None)
        {
            OnOf onOf = value ? OnOf.on : OnOf.off;
            EnumField(ref onOf, Header, null, null, onSellect, comboFlags);
            value = onOf == OnOf.on;
            fieldInfo?.SetValue(@object, value);
        }

        public static void EnumField<T>(ref T @enum, string Header = "", FieldInfo fieldInfo = null, object @object = null,
                                        Action onSellect = null, ImGuiComboFlags comboFlags = ImGuiComboFlags.None) where T : Enum
        {
            if (Header == ""){
                Header = @enum.GetType().ToString();
            }

            var values = Enum.GetNames(@enum.GetType());

            if (ImGui.BeginCombo(Header, @enum.ToString(), comboFlags))
            {

                for (int i = 0; i < values.Length; i++)
                {
                    bool sellected = values[i] == Enum.GetName(@enum.GetType(),@enum);

                    if (ImGui.Selectable(values[i], sellected))
                    {
                        @enum = (T)Enum.Parse(@enum.GetType(), values[i]);
                        fieldInfo?.SetValue(@object, @enum);
                        onSellect?.Invoke();
                    }
                }
                ImGui.EndCombo();
            }
        }

        public static void EnumFieldInt(ref int @enum, in string[] names, in string label, in Action onSellect = null, ImGuiComboFlags comboFlags = ImGuiComboFlags.None) 
        {
            if (ImGui.BeginCombo(label, names[@enum], comboFlags))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    bool sellected = i == @enum;

                    if (ImGui.Selectable(names[i], sellected))
                    {
                        Undo.Record(ref @enum);
                        @enum = i;
                        onSellect?.Invoke();
                    }
                }
                ImGui.EndCombo();
            }
        }

        public static void BoolField(ref bool value, in string name, in Action OnValueChanged = null)
        {
            if (ImGui.Checkbox(name, ref value))
            {
                Undo.Record(ref value);
                OnValueChanged?.Invoke();
            }
        }

        public static void BoolField(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null)
        {
            bool value = (bool)fieldInfo.GetValue(@object);

            bool edited = false;
            if (ImGui.Checkbox(fieldInfo.Name, ref value))
            {
                edited = true;
                fieldInfo.SetValue(@object, value);
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }


        public static Material MaterialField(Material material)
        {
            ImGui.BeginGroup();
            {
                ImageButton((IntPtr)AssetManager.DefaultTexture.texID, MiniSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Drop Material Here");
                    ImGui.EndTooltip();
                }
                DropUIElementString(EditorResources.MATERIAL, (matPath) =>
                {
                    string oldPath = material.path;
                    Undo.AddActions(() =>
                    {
                        material = AssetManager.GetMaterial(matPath);
                    }, 
                    () =>
                    {
                        material = AssetManager.GetMaterial(oldPath);
                    });
                });
                RightClickPopUp("edit", new TitleAndAction("reset", () => material = AssetManager.DefaultMaterial));
                if (material != null)
                {
                    ImGui.SameLine();
                    TextField(ref material.name, "name");
                    ImGui.SameLine();
                    if (ImGui.Button("save"))
                    {
                        material.SaveToFile();
                    }
                }
            }
            ImGui.EndGroup();
            return material;
        }

        public static void TextureField(in string title, ref int textureID, ref string texturePath)
        {
            ImGui.BeginGroup();
            {
                HeaderIn(title);

                ImGui.Image((IntPtr)textureID, new SysVec2(35, 35));
                
                int changedTextureID = 0;
                string changedTexturePath = string.Empty;
                string oldPath = texturePath;

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Drop Texture Here");
                    ImGui.EndTooltip();
                } 
                DropUIElementString(EditorResources.TEXTURE, (filename) =>
                {
                    Rendering.Texture texture = default;
                    Undo.AddActions(() =>  {
                        texture = AssetManager.GetTexture(filename);
                    },
                    () => {
                        texture = AssetManager.GetTexture(oldPath);
                    });
                    changedTextureID = texture.texID;
                    changedTexturePath = texture.path;
                });
                RightClickPopUp("edit", new TitleAndAction("reset", () =>
                {
                    changedTextureID = AssetManager.DefaultTexture.texID;
                    changedTexturePath = AssetManager.DefaultTexture.path;
                }));
                
                if (changedTextureID != 0) { // texture changed
                    textureID = changedTextureID;
                    texturePath = changedTexturePath;
                }
            }
            ImGui.EndGroup();
        }

        public static Rendering.Texture TextureField(in string title, Rendering.Texture texture)
        {
            ImGui.BeginGroup();
            {
                HeaderIn(title);
                string oldPath = texture?.path ?? AssetManager.DefaultTexture.path;

                void ChangeTexture(string fileName)
                {
                    Undo.AddActions(() =>
                    {
                        texture = AssetManager.GetTexture(fileName);
                    },
                    () =>
                    {
                        texture = AssetManager.GetTexture(oldPath);
                    });
                }

                IntPtr textureID = texture != null ? (IntPtr)texture.texID : (IntPtr)AssetManager.DefaultTexture.texID; // for null termination

                ImGui.Image(textureID, new SysVec2(35, 35));
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Drop Texture Here");
                    ImGui.EndTooltip();
                }
                DropUIElementString(EditorResources.TEXTURE, ChangeTexture);
                RightClickPopUp("edit", new TitleAndAction("reset", () => texture = AssetManager.DefaultTexture));
            }
            ImGui.EndGroup();
            return texture;
        }

        public static void TextureField(FieldInfo info, object component)
        {
            Rendering.Texture texture = (Rendering.Texture)info.GetValue(component);
            string oldPath = texture?.path ?? AssetManager.DefaultTexture.path;

            void ChangeTexture(string fileName)
            {
                Undo.AddActions(() =>
                {
                    info.SetValue(component, AssetManager.GetTexture(fileName));
                },
                () =>
                {
                    info.SetValue(component, AssetManager.GetTexture(oldPath));
                });
            };
            
            ImGui.BeginGroup();
            {
                HeaderIn(info.Name);

                IntPtr textureID = texture != null ? (IntPtr)texture.texID : (IntPtr)AssetManager.DefaultTexture.texID;

                ImGui.Image(textureID, new SysVec2(35, 35));
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Drop Texture Here");
                    ImGui.EndTooltip();
                }
                RightClickPopUp("edit", new TitleAndAction("reset", () => texture = AssetManager.DefaultTexture));
                DropUIElementString(EditorResources.TEXTURE, ChangeTexture);
            }
            ImGui.EndGroup();
        }

        public static MeshBase ModelField(MeshBase mesh)
        {
            void LoadModel(string fileName)
            {
                EditorResources.GenerateMeshViewer(fileName, (m) => mesh = m);
            }

            ImGui.Button("change mesh");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Drop Model Here");
                ImGui.EndTooltip();
            }
            DropUIElementString(EditorResources.MODEL, LoadModel);
            return mesh;
        }

        public static void ModelField(in FieldInfo info, in Companent component)
        {
            MeshBase mesh = (MeshBase)info.GetValue(component);

            string oldPath = mesh.path; 
            void LoadModel(string fileName)
            {
                EditorResources.GenerateMeshViewer(fileName, (m) => mesh = m);
            }

            ImGui.Button("change mesh");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Drop Model Here");
                ImGui.EndTooltip();
            }
            DropUIElementString(EditorResources.MODEL, LoadModel);
        }

        public static void HeaderIn(in string text, float scale = 0)
        {
            float oldSize = ImGuiController.BoldFontSize;

            ImGuiController.RobotoBold.NativePtr->Scale = scale == 0 ? oldSize : scale;  
            ImGui.PushFont(ImGuiController.RobotoBold);
            ImGui.TextColored(Color4.Orange.ToSystem(), text);
            ImGui.PopFont();

            ImGuiController.RobotoBold.NativePtr->Scale = oldSize;
        }

        public static void Header(ref string text)
        {
            ImGui.PushFont(ImGuiController.RobotoBold);
            ImGui.TextColored(Color4.Orange.ToSystem(),text);
            ImGui.PopFont();
        }

        public static void FloatField(ref float value, in string name, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            bool editing = false;
            if (ImGui.DragFloat(name, ref value, speed))
            {
                editing = true;
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.Record(ref value, editing);
        }

        public static void FloatField(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            if (@object is not Companent component) {
                Debug.Log("please use other int field method");
                return;
            }

            float value = (float)fieldInfo.GetValue(@object);
            bool editing = false;
            // if components current item is this change value
            if (ImGui.DragFloat(fieldInfo.Name, ref value, speed))
            {
                editing = true;
                fieldInfo.SetValue(@object, value);
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, editing);
        }

        public static void IntField(ref int value, in string name, in Action OnValueChanged = null)
        {
            bool edited = false;
            if (ImGui.DragInt(name, ref value))
            {
                edited = true; 
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.Record(ref value, edited);
        }

        public static void IntField(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null)
        {
            if (@object is not Companent component) {
                Debug.Log("please use other int field method");
                return;
            }

            int value = (int)fieldInfo.GetValue(@object);
            bool edited = false;
            if (ImGui.DragInt(fieldInfo.Name, ref value))
            {
                edited = true;
                fieldInfo.SetValue(@object, value);
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }

        public static void TextField(ref string value, in string name, in Action OnValueChanged = null, bool multiline = false)
        {
            if (value == null) value = string.Empty;

            if (multiline)
            {
                if (ImGui.InputTextMultiline(name, ref value, 20, multinileTextSize))
                {
                    // for now no undo
                    OnValueChanged?.Invoke();    
                }
            }
            else if (ImGui.InputText(name, ref value, 20))
            {
                OnValueChanged?.Invoke();
            }
        }

        public static void TextField(in FieldInfo fieldInfo, in object @object, in Action OnValueChanged = null, bool multiline = false)
        {
            if (@object is not Companent component) {
                Debug.Log("please use other int field method");
                return;
            }

            string value = (string)fieldInfo.GetValue(@object);

            if (multiline)
            {
                if (ImGui.InputTextMultiline(fieldInfo.Name, ref value, 20, multinileTextSize))
                {
                    fieldInfo.SetValue(@object, value);
                    OnValueChanged?.Invoke();
                }
            }
            else if (ImGui.InputText(fieldInfo.Name, ref value, 20))
            {
                fieldInfo.SetValue(@object, value);
                OnValueChanged?.Invoke();
            }
        }

        public static void Vector2Field(ref Vector2 value, in string name, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec2 vector2 = value.ToSystemRef();
            bool editing = false;
            if (ImGui.DragFloat2(name, ref vector2, speed))
            {
                editing = true;
                Undo.Record(ref value);
                value = vector2.ToOpenTKRef();
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.Record(ref value, editing);
        }

        public static void Vector2Field(ref SysVec2 value, in string name, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            bool editing = false;
            if (ImGui.DragFloat2(name, ref value, speed))
            {
                editing = true;
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.Record(ref value, editing);
        }

        public static void Vector2Field(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec2 vector2 = ((Vector2)fieldInfo.GetValue(@object)).ToSystem();

            bool edited = false;
            if (ImGui.DragFloat2(fieldInfo.Name, ref vector2, speed))
            {
                edited = true;
                fieldInfo.SetValue(@object, vector2.ToOpenTK());
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }

        public static void Vector3Field(ref Vector3 value, in string name, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec3 vector3 = value.ToSystemRef();

            bool editing = false;
            if (ImGui.DragFloat3(name, ref vector3, speed)) {
                Undo.Record(ref value);
                value = vector3.ToOpenTKRef();
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.Record(ref value, editing);
        }

        public static void Vector3Field(in FieldInfo fieldInfo, in object @object, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec3 vector3 = ((Vector3)fieldInfo.GetValue(@object)).ToSystem();

            bool edited = false;
            if (ImGui.DragFloat3(fieldInfo.Name, ref vector3, speed)) {
                edited = true;
                fieldInfo.SetValue(@object, vector3.ToOpenTK());
                OnValueChanged?.Invoke();
                Bindings.MouseBindings.InfiniteMouse();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }

        public static void Vector4Field(ref Vector4 value, in string name, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec4 vector4 = value.ToSystemRef();
            bool editing = false;

            if (ImGui.DragFloat4(name, ref vector4, speed)) {
                editing = true;
                Bindings.MouseBindings.InfiniteMouse();
                value = vector4.ToOpenTKRef();
                OnValueChanged?.Invoke();
            }
            Undo.Record(ref value, editing);
        }

        public static void Vector4Field(in FieldInfo fieldInfo, in object @object, in Action OnValueChanged = null, in float speed = Companent.ImguiDragSpeed)
        {
            SysVec4 vector4 = ((Vector4)fieldInfo.GetValue(@object)).ToSystem();

            bool edited = false;
            if (ImGui.DragFloat4(fieldInfo.Name, ref vector4, speed)) {
                edited = true;
                Bindings.MouseBindings.InfiniteMouse();
                fieldInfo.SetValue(@object, vector4.ToOpenTK());
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }

        public static void ColorEdit3(ref SysVec3 value, in string name, in Action OnValueChanged = null, ImGuiColorEditFlags flags = ImGuiColorEditFlags.None)
        {
            bool editing = false;
            if (ImGui.ColorEdit3(name, ref value, flags))
            {
                editing = true;
                OnValueChanged?.Invoke();
            }
            Undo.Record(ref value, editing);
        }

        public static void ColorEdit3(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null, in ImGuiColorEditFlags flags = ImGuiColorEditFlags.None)
        {
            SysVec3 vector3 = (SysVec3)fieldInfo.GetValue(@object);

            bool editing = false;
            if (ImGui.ColorEdit3(fieldInfo.Name, ref vector3, flags)) {
                editing = true;
                fieldInfo.SetValue(@object, vector3);
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, editing);
        }

        public static void ColorEdit4(ref SysVec4 value, in string name, in Action OnValueChanged = null, in ImGuiColorEditFlags flags = ImGuiColorEditFlags.None) 
        {
            bool editing = false;
            if (ImGui.ColorEdit4(name, ref value, flags))
            {
                editing = true;
                OnValueChanged?.Invoke();
            }
            Undo.Record(ref value, editing);
        }

        public static void ColorEdit4(FieldInfo fieldInfo, object @object, in Action OnValueChanged = null, in ImGuiColorEditFlags flags = ImGuiColorEditFlags.None)
        {
            SysVec4 vector4 = (SysVec4)fieldInfo.GetValue(@object);

            bool edited = false;
            if (ImGui.ColorEdit4(fieldInfo.Name, ref vector4, flags))
            {
                edited = true;
                fieldInfo.SetValue(@object, vector4);
                OnValueChanged?.Invoke();
            }
            Undo.RecordField(fieldInfo, @object, edited);
        }

        
        public static void RightClickPopUp(in string title, params TitleAndAction[] menuItems)
        {
            if (ImGui.BeginPopupContextWindow(title))
            {
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (ImGui.MenuItem(menuItems[i].title))
                    {
                        menuItems[i].Action?.Invoke();
                    }
                }
                ImGui.EndPopup();
            }
        }

        ///////////// PAYLOAD ////////////

        /// <summary>
        /// allows you to drag drop files
        /// </summary>
        /// <param name="texture">the texture will be shown when dragging</param>
        /// <param name="type">header of the drag source</param>
        public static void DragUIElementString(ref string file, in string type, in Rendering.Texture texture) 
        {
            DragUIElementString(ref file, in type, (IntPtr)texture.texID);
        }

        /// <summary>
        /// allows you to drag drop files
        /// </summary>
        /// <param name="texture">the texture will be shown when dragging</param>
        /// <param name="type">header of the drag source</param>
        public static bool DragUIElementString(ref string file, in string type, in IntPtr texture)
        {
            if (ImGui.IsItemFocused() && ImGui.IsAnyMouseDown() && ImGui.BeginDragDropSource())
            {
                fixed (char* folderPtr = file)
                {
                    ImGui.SetDragDropPayload(type, (IntPtr)folderPtr, (uint)(file.Length * sizeof(char)), ImGuiCond.Always);
                    if (texture != IntPtr.Zero) ImGui.Image(texture, _FileSize, uv0, uv1);
                }
                ImGui.EndDragDropSource();
                return true;
            }
            return false;
        }

        static readonly string[] extensions = {
            "vert", "frag",
             obj  , fbx   ,
             png  , jpg   ,
             vaw  , ogg   ,
             gltf , blend ,
             glsl , cs    ,
             mp3  , tga   ,
            ".mat", dae
        };

        /// <summary>
        /// still returning first file of the editor window
        /// </summary>
        public static void DropUIElementString(string type, in Action<string> DropAction) 
        {
            if (ImGui.BeginDragDropTarget())
            {
                ImGuiPayload* payloadPtr = ImGui.AcceptDragDropPayload(type, ImGuiDragDropFlags.None);

                if (payloadPtr != null)
                {
                    if ((IntPtr)payloadPtr->Data != IntPtr.Zero)
                    {
                        string filePath = Marshal.PtrToStringUni((IntPtr)payloadPtr->Data);

                        Debug.LogWarning(new string((char*)payloadPtr->Data));
                        
                        // filepath's last characters will be corrupt,
                        // this is last proper index of filepath
                        int lastReliableIndex = 0; 

                        string lastFour = string.Empty;
                        bool cleaned = false; // getting rid of the useless chars

                        // idk this teqnique is right or not we wwill see
                        for (; lastReliableIndex < filePath.Length; lastReliableIndex++)
                        {
                            lastFour = filePath.Substring((int)MathF.Max(0, lastReliableIndex-4), 4);

                            if (lastFour.Contains(extensions)) {
                                cleaned = true;
                                break;
                            }
                        }

                        filePath = filePath.Substring(0, lastReliableIndex);

                        if (filePath != null && cleaned) {
                            DropAction.Invoke(filePath);
                        }
                        else {
                            Debug.LogWarning($"unsupported file format '{lastFour}' is not supported please add it in GUI.DropUIElementString");
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
        }

        /// <param name="type">header of the drag source const value recomended this string will be use droping</param>
        public static void DragUIElement<T>(ref T file, in string type, in Rendering.Texture texture) where T : unmanaged 
        {
            if (ImGui.IsItemFocused() && ImGui.BeginDragDropSource())
            {
                fixed (T* folderPtr = &file)
                {
                    ImGui.SetDragDropPayload(type, (IntPtr)folderPtr, (uint)Marshal.SizeOf<T>(), ImGuiCond.Always);
                    if (texture != null) ImGui.Image((IntPtr)texture.texID, _FileSize, uv0, uv1);
                }
                ImGui.EndDragDropSource();
            }
        }

        public static void DropUIElement<T>(in string type, in Action<T> DropAction)
        {
            if (ImGui.BeginDragDropTarget())
            {
                ImGuiPayload* payloadPtr = ImGui.AcceptDragDropPayload(type, ImGuiDragDropFlags.None);

                if (payloadPtr != null)
                {
                    if ((IntPtr)payloadPtr->Data != IntPtr.Zero)
                    {
                        T @object = Marshal.PtrToStructure<T>((IntPtr)payloadPtr->Data);

                        if (@object != null)
                        {
                            DropAction.Invoke(@object);
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }
        }

        public static bool ImageButton(in Rendering.Texture texture)
        {
            return ImGui.ImageButton(texture != null ? (IntPtr)texture.texID : (IntPtr)AssetManager.DefaultTexture.texID, _FileSize, uv0, uv1);
        }

        public static bool ImageButton(in IntPtr textureHandle)
        {
            return ImGui.ImageButton(textureHandle, _FileSize, uv0, uv1);
        }

        public static bool ImageButton(in IntPtr textureHandle, in SysVec2 scale)
        {
            return ImGui.ImageButton(textureHandle, scale, uv0, uv1);
        }

        public static void Image(in IntPtr image, in SysVec2 size)
        {
            ImGui.Image(image, size, uv0, uv1);
        }

        private const float FileSize = 35;
        public static readonly SysVec2 MiniSize = new SysVec2(12.5f, 12.5f); 
        public static readonly SysVec2 _FileSize = new SysVec2(FileSize, FileSize);
        static readonly SysVec2 uv0  = SysVec2.UnitY;
        static readonly SysVec2 uv1  = SysVec2.UnitX;
    }
}
