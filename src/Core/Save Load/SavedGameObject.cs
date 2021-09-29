
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using ZargoEngine.Rendering;

#nullable disable warnings
namespace ZargoEngine.SaveLoad
{
    [Serializable]
    public class SceneSaveData
    {
        public string Name;
        public Vector3 cameraPos;
        public Vector3 cameraUp;
        public Vector3 cameraForward;
        public RenderSaveData RenderSaveData;
        public GameObjectSaveData[] savedGOs;

        public SceneSaveData() {}

        public SceneSaveData(string sceneName, List<GameObject> gameObjects)
        {
            Name = sceneName;
            cameraPos = Camera.SceneCamera.Position;
            cameraUp = Camera.SceneCamera.Up;
            cameraForward = Camera.SceneCamera.Front;
            RenderSaveData = RenderSaveData.TakeFromCurrent();
            savedGOs = new GameObjectSaveData[gameObjects.Count];

            for (int i = 0; i < savedGOs.Length; i++)
            {
                savedGOs[i] = new GameObjectSaveData(gameObjects[i]);
            }
        }
    }

    [Serializable]
    public class GameObjectSaveData
    {
        public string name;
        public Matrix4 translation;
        public CompanentData[] companentDatas;

        public GameObjectSaveData()  {}

        public GameObject Generate()
        {
            GameObject go = new GameObject(name);

            go.transform.position = translation.ExtractTranslation();
            go.transform.rotation = translation.ExtractRotation();
            go.transform.scale = translation.ExtractScale();
            go.transform.UpdateTranslation();

            for (int i = 0; i < companentDatas.Length; i++)
            {
                CompanentData data = companentDatas[i];
                Type type = Type.GetType(data.AssemblyQualifiedName);

                if (type == typeof(Transform)) continue;
                if (type == typeof(MeshRenderer))
                {
                    string meshPath = MeshCreator.CreateCube().path;
                    string materialPath = AssetManager.DefaultMaterial.path;

                    for (int j = 0; j < data.fieldDatas.Length; j++)
                    {
                        Debug.LogWarning("field name: " + data.fieldDatas[j].fieldName);

                        if (data.fieldDatas[j].fieldName == "MaterialPath")
                        {
                            materialPath = data.fieldDatas[j].value;
                        }
                        if (data.fieldDatas[j].fieldName == "MeshPath")
                        {
                            meshPath = data.fieldDatas[j].value;
                        }
                    }

                    new MeshRenderer(AssetManager.GetMesh(meshPath), go, AssetManager.GetMaterial(materialPath));
                    continue;
                }

                Companent companent = (Companent)Activator.CreateInstance(type, go);
                companent.InitializeFields(data.fieldDatas);
            }

            return go;
        }

        public GameObjectSaveData(GameObject go)
        {
            name = go.name ?? "go";
            translation = go.transform.Translation;
            companentDatas = new CompanentData[go.components.Count];

            for (int i = 0; i < companentDatas.Length; i++)
            {
                if (go.components[i] is MeshRenderer meshRenderer) {
                    meshRenderer.MaterialPath = meshRenderer.Material.path;
                    meshRenderer.MeshPath = meshRenderer.mesh.GetIdentifier();
                }
                companentDatas[i] = new CompanentData(go.components[i]);
            }
        }

        public GameObjectSaveData(string name, Matrix4 translation, CompanentData[] companentDatas)
        {
            this.name = name;
            this.translation = translation;
            this.companentDatas = companentDatas;
        }
    }
}
