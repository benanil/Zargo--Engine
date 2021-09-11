using BulletSharp;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ZargoEngine.Editor;
using ZargoEngine.Editor.Attributes;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    
    [XmlRoot("GameObject")]
    public class GameObject : IDisposable , IDrawable
    {
        [Editor.Attributes.NonSerialized]
        public int id;

        [EnumField(nameof(PhysicsLayer))]
        public PhysicsLayer physicsLayer = PhysicsLayer.none;

        public List<Component> components = new List<Component>();

        public List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();

        public Transform transform;
        public string name;

        public event Action OnGameObjectDeleted = () => { };
        public event Action OnUpdate = () => { };

        internal GameObject() {}

        public GameObject(in string name)
        {
            this.name = name;
            transform = new Transform(this, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            id = SceneManager.currentScene.GetUniqueID();
            SceneManager.currentScene.AddGameObject(this);
        }

        public void Start()
        {
            for (int i = 0; i < components.Count; i++){
                components[i].Start();
            }
        }

        public void Update()
        {
            OnUpdate?.Invoke();
            for (int i = 0; i < components.Count; i++){
                components[i].Update();
            }
        }

        public void Render()
        {
            for (int i = 0; i < components.Count; i++) {
                if (components[i] is not MeshRenderer)
                {
                    components[i].Render();
                }
            }
        }

        private bool WindowActive = false;

        public void DrawWindow()
        {
            GUI.HeaderIn("GameObject");
            ImGui.SameLine();
            const string GO_Handle = "Gameobject Name";
            GUI.TextField(ref name, GO_Handle, OnValidate);

            for (int i = 0; i < components.Count; i++) {
                components[i].DrawWindow();
            }
        }

        private void OnValidate()
        { 
            
        }

        public void OnTriggerEnter(CollisionObject other, AlignedManifoldArray details)
        {
            for (int i = 0; i < components.Count; i++) monoBehaviours[i].OnTriggerEnter(other, details);
        }

        public void OnTriggerExit(CollisionObject other)
        {
            for (int i = 0; i < components.Count; i++) monoBehaviours[i].OnTriggerExit(other);
        }

        public void OnTriggerStay(CollisionObject other, AlignedManifoldArray details)
        {
            for (int i = 0; i < components.Count; i++) monoBehaviours[i].OnTriggerStay(other, details);
        }

        public T AddComponent<T>(T component) where T : Component
        {
            component.gameObject = this;
            component.transform = transform;
            component.OnComponentAdded();
            components.Add(component);
            if (component is MonoBehaviour mono)
            {
                monoBehaviours.Add(mono);
            }
            return component;
        }

        public void RemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < monoBehaviours.Count; i++)
            {
                if (monoBehaviours[i] is T)
                {
                    var behaviour = monoBehaviours[i];
                    monoBehaviours.RemoveAt(i);
                    components.Remove(behaviour);
                    behaviour.Dispose();
                }
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T) {
                    components[i].Dispose();
                    components.RemoveAt(i);
                }
            }
        }

        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Count; i++)
                if (components[i] is T result)
                    return result;

            return null;
        }

        public bool TryGetComponent<T>(out T value) where T : Component
        {
            value = GetComponent<T>();
            return value != null;
        }

        public Component GetComponent(Type type)
        {
            return components.Find(x => x.GetType() == type);
        }

        public bool HasComponent<T>()
        {
            for (int i = 0; i < components.Count; i++)
                if (components[i] is T)
                    return true;
            return false;
        }

        public void DebugBehaviours()
        {
            for (int i = 0; i < components.Count; i++)
            {
                Debug.Log(components[i].name);
            }
        }

        // now its working finaly
        public void Dispose()
        {
            SceneManager.currentScene.gameObjects.Remove(this);

            if (TryGetComponent(out RendererBase mesh)) {
                Debug.Log("Renderer Exist");
                SceneManager.currentScene.meshRenderers.Remove(mesh);
            }

            foreach (var component in components) {
                component.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}