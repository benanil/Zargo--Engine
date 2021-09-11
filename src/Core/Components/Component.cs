#pragma warning disable CS8601 // Possible null reference assignment.

using OpenTK.Mathematics;
using System.Linq;
using System.Reflection;
using ZargoEngine.Editor;
using ImGuiNET;
using ZargoEngine.Rendering;
using ZargoEngine.Editor.Attributes;
using System;
using ZargoEngine.Attributes;
using Newtonsoft.Json;

#nullable disable warnings

namespace ZargoEngine
{
    using NonSerialized = Editor.Attributes.NonSerializedAttribute;
    
    public class Component : IDrawable, IRenderable, IDisposable
    {
        public const float ImguiDragSpeed = .01f;

        [NonSerialized]
        public string currentItemName = "current item name";
        [NonSerialized]
        public string name;
        
        private Transform _transform;
        public Transform transform
        {
            get => _transform;
            set
            {
                // value changed
                if (value != transform){
                    _transform = value;
                }
            }
        }

        [JsonIgnore]
        public GameObject gameObject;
        
        public Component(GameObject go)
        {
            this.gameObject = go;
            this.transform = go.transform;
            go.AddComponent(this);
        }

        public virtual void OnValidate() {}
        public virtual void OnComponentAdded() {}
        public virtual void OnComponentRemoved() {}
        public virtual void Start() {}
        public virtual void Update() {}
        public virtual void Render() {}
        public virtual void DeleteBuffers() { }
        public virtual void OnDispose() { }
        public virtual void PhysicsUpdate() { }

        public virtual void DrawWindow()
        {
            ImGui.Text(name);
            SerializeComponent();
            ImGui.Separator();
        }

        protected void SerializeComponent()
        {
            SerializeFields();
            SerializeMethods();
        }

        private void SerializeMethods()
        {
            var Methods = this.GetType().GetMethods();

            for (int i = 0; i < Methods.Length; i++)
            foreach(var item in Methods[i].GetCustomAttributes())
            if (item is ButtonAttribute button)
            {
                // attribute içerisinde özellikle isim belirtilmediyse methodun adını kullan
                string buttonName = Methods[i].Name == string.Empty ? Methods[i].Name : button.name; 
                if (ImGui.Button(buttonName , button.size))
                {
                    Methods[i].Invoke(this,null);
                }
            }
        }
        
        private void SerializeFields()
        {
            FieldInfo[] fields = this.GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(this);
                bool drawn = false;

                if (field.IsPrivate || value is GameObject) continue;
                if (value is Component component){
                    component.DrawWindow();
                    continue;
                }

                var attributes = field.GetCustomAttributes().ToArray();

                if (attributes != default && attributes.Length > 0)
                {
                    if (CheckDrawIf(ref attributes)) continue;

                    if (attributes.Any(x => x is NonSerialized)) continue;
                    
                    if (CheckHasAttribute(ref attributes, out ColorAttribute colorAttribute))
                    {
                        if (value is System.Numerics.Vector3 color3){
                            GUI.ColorEdit3(field, this, OnValidate, colorAttribute.flags);
                            continue;
                        }
                        if (value is System.Numerics.Vector4 color4){
                            ImGui.ColorEdit4(field.Name, ref color4, colorAttribute.flags);
                            continue;
                        }
                    }

                    if (CheckHasAttribute(ref attributes, out EnumFieldAttribute enumTypeAttribute))
                    {
                        var @enum = value as Enum;
                        var Method = this.GetType().GetMethods().ToList().Find(x => x.Name == enumTypeAttribute.OnSellect);
                        GUI.EnumField(ref @enum, enumTypeAttribute.Header, field, this, () => Method.Invoke(this, null), enumTypeAttribute.ImGuiComboFlags);
                        continue;
                    }

                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j] is GUIAttributeBase attributeBase)
                        {
                            if (attributeBase.Proceed(field, value, this))
                            {
                                drawn = true; continue;
                            }
                        }
                    }

                    for (int j = 0; j < attributes.Length; j++)
                    {
                        if (attributes[j] is GUIMinMaxBase minmax)
                        {
                            field.SetValue(this, minmax.Get(ref value));
                        }
                    }
                }

                // if has no attribute draw default 
                if (!drawn) DefaultDraw(field, value);
            }
        } //SerializeFields end

        private void DefaultDraw(FieldInfo field, object value)
        { 
            switch (value)
            {
                case float val       : GUI.FloatField(field,   this, OnValidate); break;
                case int intVal      : GUI.IntField(field,     this, OnValidate); break;
                case bool boolVal    : GUI.BoolField(field,    this, OnValidate); break;
                case string strValue : GUI.TextField(field,    this, OnValidate); break;
                case Vector2 vector2 : GUI.Vector2Field(field, this, OnValidate); break;
                case Vector3 vector3 : GUI.Vector3Field(field, this, OnValidate); break;
                case Vector4 vector4 : GUI.Vector4Field(field, this, OnValidate); break;
                case Enum @enum      : GUI.EnumField(ref @enum,"",field,this,OnValidate); break;
                case System.Numerics.Vector4 color4: GUI.ColorEdit4(field, this, OnValidate); break;
                case System.Numerics.Vector3 color3: GUI.ColorEdit3(field, this, OnValidate); break;
                case Texture tex     : GUI.TextureField(field, this); break;
                case MeshBase model  : GUI.ModelField(field, this); break; 
            }
        }

        private bool CheckDrawIf(ref Attribute[] attributes)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                var Methods = this.GetType().GetMethods();
                if (attributes[i] is DrawIfAttribute drawIfAttribute)
                {
                    for (int j = 0; j < Methods.Length; j++)
                    {
                        if (Methods[i].Name == drawIfAttribute.methodName)
                        {
                            if (!drawIfAttribute.Proceed(Methods[i].Invoke(this, null), drawIfAttribute.otherObject)) return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool CheckHasAttribute<A>(ref Attribute[] attributes,out A attribute)
        {
            attribute = default;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].GetType() is A findedAttribute)
                {
                    attribute = findedAttribute;
                    return true;
                }
            }
            return false;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
