using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable warnings
namespace ZargoEngine
{
    public class SceneManager 
    {
        public static readonly SceneManager instance;
        public List<Scene> scenes = new();
        public static Scene currentScene;

        static SceneManager()
        {
            instance = new SceneManager();
            string[] scenes = Directory.GetFiles(AssetManager.AssetsPath + "Scenes");
            for (ushort i = 0; i < scenes.Length; i++)
            {
                AddScene(new Scene(scenes[i]));
            }
        }

        public static void AddScene(Scene scene){
            instance.scenes.Add(scene);
        }

        public static void LoadScene(int index)
        {
            currentScene?.DestroyScene();

            var findedScene = instance.scenes[index];

            if (findedScene == null){
                Debug.LogWarning("Scene index doesnt exist");
                return;
            }
            currentScene = findedScene;
            currentScene.LoadScene();
        }

        public static void LoadScene(string name)
        {
            currentScene?.DestroyScene();

            var findedScene = instance.scenes.Find(x => x.name == name);

            if (findedScene == null){
                if (!File.Exists(name)) {

                    if (Path.IsPathFullyQualified(name)) { // this returns true if value starts with "C:/" etc
                        findedScene = new Scene(name);
                    }
                    else {
                        findedScene = new Scene(AssetManager.AssetsPath + @$"Scenes\{name}");
                    }
                }
                else {
                    findedScene = new Scene(name);
                }
                Debug.LogWarning("scene couldnt finded and created new one ");
            }
            currentScene = findedScene;
            currentScene.LoadScene();
        }
        
        public static string GetUniqeName(string name){
            if (instance.scenes.Any(x => x.name == name)) return "scene" + instance.scenes.Count;
            return name;
        }

        public static GameObject FindGameObjectByName(string name)
        {
            return currentScene.gameObjects.Find(x => x.name == name);
        }

        public static T FindObjectOfType<T>() where T : Companent
        {
            for (int i = 0; i < currentScene.gameObjects.Count; i++){

                if (currentScene.gameObjects[i].TryGetComponent(out T value)){
                    return value;
                } 
            }
            return null;
        }

        public static string GetName(){
            return "scene" + instance.scenes.Count;
        }

        public static void Dispose()
        {
            currentScene?.Dispose();
        }
    }
}