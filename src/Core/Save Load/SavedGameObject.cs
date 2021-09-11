
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ZargoEngine.Rendering;

namespace ZargoEngine.SaveLoad
{
    [XmlRoot("SceneSaveData")]
    [Serializable]
    public class SceneSaveData
    {
        public List<SavedGameObject> savedGameObjects = new List<SavedGameObject>();
        public CameraProperties cameraProperties = new CameraProperties();

        public SceneSaveData(List<GameObject> gameObjects, Camera camera)
        {
            cameraProperties.cameraPos = camera.Position;
            cameraProperties.camUp = camera.Up; cameraProperties.camFront = camera.Front; cameraProperties.camRight = camera.Right; 

            for (int i = 0; i < gameObjects.Count; i++)
            {
                savedGameObjects.Add(new SavedGameObject(gameObjects[i]));
            }
        }

        internal SceneSaveData() {}
    }

    [XmlRoot("SavedGameObject")]
    [Serializable]
    public class SavedGameObject 
    {
        public string name;
        public Vector3 position = Vector3.Zero;
        public Vector3 scale = Vector3.Zero;
        public Vector3 euler = Vector3.Zero;
        public MeshFileInfo meshFileInfo;

        public GameObject Load()
        {
            var go = new GameObject(name);
            
            meshFileInfo?.Generate(go);

            go.transform.SetScale(scale, false);
            go.transform.SetPosition(position, false);
            go.transform.SetEuler(euler, true);
            return go;
        }

        internal SavedGameObject() {}

        public unsafe SavedGameObject(GameObject go)
        {
            name     = Unsafe.AsRef(go.name);
            position = Unsafe.AsRef(go.transform.position);
            euler    = Unsafe.AsRef(go.transform._eulerAngles); // we are getting _euler instead of euler cause _euler returning degrees
            scale    = Unsafe.AsRef(go.transform.scale);

            meshFileInfo = new MeshFileInfo();
            if (go.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshFileInfo.initialize(meshRenderer);
            }
        }
    }

    [Serializable]
    public class CameraProperties
    {
        public Vector3 cameraPos;
        public Vector3 camFront;
        public Vector3 camUp;
        public Vector3 camRight;

        internal CameraProperties() {}
    }

    [Serializable]
    public class MeshFileInfo
    {
        public string meshPath = string.Empty;
        public string materialPath = string.Empty;

        internal MeshFileInfo() { }

        public void initialize(MeshRenderer meshRenderer)
        { 
            meshPath = meshRenderer.mesh.path;
            materialPath = meshRenderer.Material.path;
        }

        public void Generate(GameObject gameObject)
        {
            if (meshPath == string.Empty) return;
            Mesh mesh     = AssetManager.GetMeshFullPath<Mesh>(meshPath);
            Material material = AssetManager.GetMaterial(materialPath);
            new MeshRenderer(mesh, gameObject, material);
        }
    }

}
