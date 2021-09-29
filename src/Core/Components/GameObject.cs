using BulletSharp;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using ZargoEngine.Editor;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    
    public class GameObject : IDrawable
    {
        [Editor.Attributes.NonSerialized]
        public int id;

        public List<Companent> components = new();

        public List<MonoBehaviour> monoBehaviours = new();

        public Transform transform;
        public string name;

        public event Action OnGameObjectDeleted;
        public event Action OnUpdate = () => { };

        public GameObject(in string name)
        {
            this.name = name;
            transform = new Transform(this, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            
            id = SceneManager.currentScene.GetUniqueID();
            OnGameObjectDeleted = () => { };
            SceneManager.currentScene.AddGameObject(this);
        }

        public void Start()
        {
            foreach (var companent in components)
            {
                companent.Start();
            }
        }

        public void Update()
        {
            OnUpdate?.Invoke();
            foreach (var companent in components)
            {
                companent.Update();
            }
        }

        public void Render()
        {
            foreach (var companent in components.Where(companent => companent is not MeshRenderer))
            {
                companent.Render();
            }
        }
        

        public void DrawWindow()
        {
            GUI.HeaderIn("GameObject");
            ImGui.SameLine();
            const string GO_Handle = "Gameobject Name";
            GUI.TextField(ref name, GO_Handle, OnValidate);

            foreach (var companent in components)
            {
                companent.DrawWindow();
            }
        }

        private void OnValidate()
        { 
            
        }

        public void OnTriggerEnter(CollisionObject other, AlignedManifoldArray details)
        {
            foreach (var mono in monoBehaviours)
                mono.OnTriggerEnter(other, details);
        }

        public void OnTriggerExit(CollisionObject other)
        {
            foreach (var mono in monoBehaviours)
                mono.OnTriggerExit(other);
        }

        public void OnTriggerStay(CollisionObject other, AlignedManifoldArray details)
        {
            foreach (var mono in monoBehaviours)
                mono.OnTriggerStay(other, details);
        }

        public T AddComponent<T>(T component) where T : Companent
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

        public void RemoveComponent<T>() where T : Companent
        {
            for (var i = 0; i < monoBehaviours.Count; i++)
            {
                if (monoBehaviours[i] is not T) continue;
                var behaviour = monoBehaviours[i];
                monoBehaviours.RemoveAt(i);
                components.Remove(behaviour);
                behaviour.Dispose();
                break;
            }

            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] is not T) continue;
                components[i].Dispose();
                components.RemoveAt(i);
                break;
            }
        }

        public void RemoveComponent(Companent companent)
        {
            if (companent != null && components != null)  components.Remove(companent);
        }

        public T GetComponent<T>() where T : Companent
        {
            foreach (var companent in components)
                if (companent is T result)
                    return result;

            return null;
        }

        public bool TryGetComponent<T>(out T value) where T : Companent
        {
            value = GetComponent<T>();
            return value != null;
        }

        public Companent GetComponent(Type type)
        {
            return components.Find(x => x.GetType() == type);
        }

        public bool HasComponent<T>()
        {
            return components.OfType<T>().Any();
        }

        public void DebugBehaviours()
        {
            foreach (var companent in components)
            {
                Debug.Log(companent.name);
            }
        }

        // now its working finaly
        public void Dispose()
        {
            foreach (var companent in components)
            {
                companent?.Dispose();
            }
            components.Clear();
            GC.SuppressFinalize(this);
        }
    }
}