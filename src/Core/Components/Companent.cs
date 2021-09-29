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
using System.Collections.Generic;

namespace ZargoEngine
{
    using NonSerialized = Editor.Attributes.NonSerializedAttribute;
    using Color3 = System.Numerics.Vector3;
    using Color4 = System.Numerics.Vector4;

    public class Companent : IDrawable, IRenderable
    {
        public const float ImguiDragSpeed = .01f;

        [NonSerialized]
        public string name;

        public bool enabled = true;

        private Transform _transform;
        public Transform transform
        {
            get => _transform;
            set
            {
                // value changed
                if (value != transform) {
                    _transform = value;
                }
            }
        }


        public GameObject gameObject;

        public Companent(GameObject go)
        {
            name = GetType().Name;
            this.gameObject = go;
            this.transform = go.transform;
            go.AddComponent(this);
        }

        #region virtual voids
        public virtual void OnValidate() {}
        public virtual void OnComponentAdded() {}
        public virtual void Start() {}
        public virtual void Update() {}
        public virtual void Render() {}
        public virtual void DeleteBuffers() { }
        public virtual void PhysicsUpdate() { }
        #endregion

        public virtual void DrawWindow()
        {
            ImGui.Text(name);
            SerializeComponent();
            ImGui.Separator();
        }

        protected void SerializeComponent()
        {
            if (!ImGui.CollapsingHeader(name ?? "companent", ImGuiTreeNodeFlags.CollapsingHeader)) return;
            SerializeFields();
            SerializeMethods();
        }

        private void SerializeMethods()
        {
            var methods = GetType().GetMethods();

            foreach (var method in methods)
            {
                foreach(var item in method.GetCustomAttributes())
                {
                    if (item is not ButtonAttribute button) continue;
                    // attribute içerisinde özellikle isim belirtilmediyse methodun adını kullan
                    var buttonName = method.Name == string.Empty ? method.Name : button.name; 
                    if (ImGui.Button(buttonName , button.size))
                    {
                        method.Invoke(this,null);
                    }
                }
            }
        }
        
        private void SerializeFields()
        {
            FieldInfo[] fields = GetType().GetFields();

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(this);
                bool drawn = false;

                if (field.IsPrivate || value is GameObject) continue;
                if (value is Companent component){
                    component.DrawWindow();
                    continue;
                }

                var attributes = field.GetCustomAttributes().ToArray();

                if (attributes.Length > 0)
                {
                    if (CheckDrawIf(ref attributes)) continue;

                    if (attributes.Any(x => x is NonSerialized)) continue;
                    
                    if (CheckHasAttribute(ref attributes, out ColorAttribute colorAttribute))
                    {
                        switch (value)
                        {
                            case Color3: GUI.ColorEdit3(field, this, OnValidate, colorAttribute.flags); continue;
                            case Color4: GUI.ColorEdit4(field, this, OnValidate, colorAttribute.flags); continue;
                        }
                    }

                    if (CheckHasAttribute(ref attributes, out EnumFieldAttribute enumTypeAttribute))
                    {
                        var @enum = value as Enum;
                        var method = GetType().GetMethods().ToList().Find(x => x.Name == enumTypeAttribute.OnSellect);
                        GUI.EnumField(ref @enum, enumTypeAttribute.Header, field, this, () => method.Invoke(this, null), enumTypeAttribute.ImGuiComboFlags);
                        continue;
                    }

                    foreach (var attribute in attributes)
                    {
                        if (attribute is not GUIAttributeBase attributeBase) continue;
                        if (!attributeBase.Proceed(field, value, this)) continue;
                        drawn = true;
                    }

                    foreach (var attribute in attributes)
                    {
                        if (attribute is GUIMinMaxBase minmax)
                        {
                            field.SetValue(this, minmax.Get(ref value));
                        }
                    }
                }

                // if has no attribute draw default 
                if (drawn) continue;
                if (value != null)
                    DefaultDraw(field, value);
            }
        } //SerializeFields end

        private void DefaultDraw(FieldInfo field, object value)
        { 
            switch (value)
            {
                case float: GUI.FloatField(field,   this, OnValidate); break;
                case int: GUI.IntField(field,     this, OnValidate); break;
                case bool: GUI.BoolField(field,    this, OnValidate); break;
                case string: GUI.TextField(field,    this, OnValidate); break;
                case Vector2: GUI.Vector2Field(field, this, OnValidate); break;
                case Vector3: GUI.Vector3Field(field, this, OnValidate); break;
                case Vector4: GUI.Vector4Field(field, this, OnValidate); break;
                //case Enum @enum      : GUI.EnumField(ref @enum,"",field,this,OnValidate); break;
                case Color4: GUI.ColorEdit4(field, this, OnValidate); break;
                case Color3: GUI.ColorEdit3(field, this, OnValidate); break;
                case Texture: GUI.TextureField(field, this); break;
                case MeshBase: GUI.ModelField(field, this); break; 
            }
        }

        private bool CheckDrawIf(ref Attribute[] attributes)
        {
            var methods = GetType().GetMethods();
            for (var i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is not DrawIfAttribute drawIfAttribute) continue;
                
                for (var j = 0; j < methods.Length; j++)
                {
                    if (methods[j].Name != drawIfAttribute.methodName) continue;
                    if (!drawIfAttribute.Proceed(methods[j].Invoke(this, null), drawIfAttribute.otherObject)) return true;
                }
            }
            return false;
        }

        private static bool CheckHasAttribute<A>(ref Attribute[] attributes,out A attribute)
        {
            attribute = default;

            foreach (var a in attributes)
            {
                if (a.GetType() is not A findedAttribute) continue;
                attribute = findedAttribute;
                return true;
            }
            return false;
        }

        #region serializing
        /// <returns>properties and values</returns>
        internal FieldData[] GetSerializationData ()
        {
            FieldInfo[] fields = this.GetType().GetFields();
            List<FieldData> datas = new List<FieldData>(fields.Length);

            for (ushort i = 0; i < datas.Capacity; i++)
            {
                FieldInfo field = fields[i];
                IEnumerable<Attribute> attributes = field.GetCustomAttributes();

                bool serializable = (field.FieldType.IsValueType || field.FieldType == typeof(string)) && field.IsPublic;
                serializable &= attributes.All(a => a.GetType() != typeof(NonSerialized));
                if (serializable)
                {
                    datas.Add(new FieldData(field.GetValue(this).ToString(), field.FieldType.AssemblyQualifiedName, field.Name));
                }
            }

            return datas.ToArray();
        }

        internal void InitializeFields(FieldData[] fieldDatas)
        {
            FieldInfo[] fields = GetType().GetFields();

            for (ushort i = 0; i < fieldDatas.Length; i++)
            {
                FieldData fieldData = fieldDatas[i];
                for (ushort j = 0; j < fields.Length; j++)
                {
                    if (fields[j].Name == fieldData.fieldName)
                    {
                        fields[j].SetValue(this, StringToObject(fieldData.assemblyQualified, fieldData.value));
                        break;
                    }
                }
            }
        }

        private static object StringToObject(string assemblyQualified, string value)
        {
            Type type = Type.GetType(assemblyQualified);

            if (type == typeof(int))     return float.Parse(value);
            if (type == typeof(short))   return short.Parse(value);
            if (type == typeof(byte))    return byte.Parse(value);
            if (type == typeof(float))   return float.Parse(value);
            if (type == typeof(bool))    return bool.Parse(value);
            if (type == typeof(Vector2)) return Parser.ParseVec2(value);
            if (type == typeof(Vector3)) return Parser.ParseVec3(value);
            if (type == typeof(Color3))  return Parser.ParseColor3(value);
            if (type == typeof(Color4))  return Parser.ParseColor4(value);
            if (type == typeof(string))  return value;

            Debug.LogWarning($"Serialized Type is not supported please add it to Component.cs or use NonserializedAttribute type is {type.Name}");
            return default;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    [Serializable]
    public class FieldData
    {
        public string fieldName;
        public string assemblyQualified;
        public string value;

        public FieldData(string value, string typeName, string fieldName)
        {
            this.fieldName = fieldName;
            this.assemblyQualified = typeName;
            this.value = value;
        }
    }

    [Serializable]
    public class CompanentData
    {
        public string AssemblyQualifiedName;
        public bool enabled;
        public FieldData[] fieldDatas;
        public CompanentData() { }

        public CompanentData(string assemblyQualifiedName, bool enabled, FieldData[] fieldDatas)
        {
            AssemblyQualifiedName = assemblyQualifiedName;
            this.enabled = enabled;
            this.fieldDatas = fieldDatas;
        }

        internal CompanentData(Companent component)  
        {
            AssemblyQualifiedName = component.GetType().AssemblyQualifiedName;
            fieldDatas = component.GetSerializationData();
            enabled = component.enabled;
        }
    }
    #endregion serializing
}
