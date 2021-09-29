using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using ZargoEngine.AnilTools;

#nullable disable warnings

namespace ZargoEngine {
    using Rendering;
    using Bindings;
    using Editor;
    using SaveLoad;

    public class Scene : IDisposable
    {
        public string name;

        public List<GameObject> gameObjects = new List<GameObject>();

        public bool isPlaying;
        private Vector2 mouseOldPos;
        public float cameraRotateSpeed = 100, cameraMoveSpeed = 3f;

        public PlayerCamera playerCamera;
        
        SceneSaveData sceneSaveData;

        public string path;

        public int GetUniqueID()
        {
            int index = 0;

            while (gameObjects.Any(x => x.id == index))
            {
                index++;
            }
            return index;
        }

        public GameObject FindGameObjectByID(int id )
        {
            // this is actualy fast thing but if tou remove gameobjects it might have bug because of replacements
            GameObject attempt = gameObjects[id];

            if (attempt != null) return attempt;
            
            for (int i = 0; i < gameObjects.Count; i++)
                if (gameObjects[i] != null && gameObjects[i].id == id)
                    return gameObjects[i];

            return null;
        }

        public Scene(string path)
        {
            this.name = SceneManager.GetUniqeName(name);
            this.path = path;
            SceneManager.AddScene(this);
        }

        /// <summary> for creating new scene </summary>
        public Scene(RenderSaveData data, string name)
        {
            this.name = SceneManager.GetUniqeName(name);
            path = $"{AssetManager.AssetsPath}Scenes{Path.DirectorySeparatorChar}{name}.scene"; 
            RenderConfig.SetData(data);
            SceneManager.AddScene(this);
            var lastScene = SceneManager.currentScene;
            SceneManager.currentScene = this;
            GameObject go = new GameObject("Main Camera");
            playerCamera = new PlayerCamera(go);

            new GameObject("First Obj");

            SceneManager.currentScene = lastScene;
            SaveScene();
            DestroyScene();
        }

        public void Start()
        {
            mouseOldPos = Input.MousePosition();
            isPlaying = true;

            for (int i = 0; i < gameObjects.Count; i++) { 
                gameObjects[i].Start();
            }
        }

        public void Stop()
        {
            isPlaying = false;
            DestroyScene();
            SceneManager.currentScene = this;
            LoadScene();
        }

        public void Render()
        {
            if (!isPlaying) return;

            for (short i = 0; i < gameObjects.Count; i++) {
                gameObjects[i].Render();
            }
        }

        public void Update()
        {
            SceneMovement();
        
            if (!isPlaying) {
                if (Input.GetKey(Keys.F5)) {
                    Start();
                }
                return;
            }
            else {
                if (Input.GetKey(Keys.F5)) {
                    Stop();
                    return;
                }
            }

            for (int i = 0; i < gameObjects.Count; i++){
                gameObjects[i].Update();
            }

        }

        public void PhysicsUpdate()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                for (int j = 0; j < gameObjects[i].components.Count; j++)
                {
                    gameObjects[i].components[j].PhysicsUpdate();
                }
            }
        }


        public void SaveScene()
        {
            if (!Directory.Exists(Path.Combine(AssetManager.AssetsPath, "Scenes"))) Directory.CreateDirectory(Path.Combine(AssetManager.AssetsPath, "Scenes"));

            sceneSaveData = new SceneSaveData(name, gameObjects);
            
            Serializer.SerializeScene(sceneSaveData, path);

            Debug.LogWarning("scene saved");
        }

        public void LoadScene()
        {
            if (!Directory.Exists(AssetManager.AssetsPath + "Scenes")) Directory.CreateDirectory(AssetManager.AssetsPath + "Scenes");

            if (File.Exists(path))
            {
                sceneSaveData = Serializer.DeserializeScene(path);
                name = sceneSaveData.Name;

                if (sceneSaveData != null)
                {
                    for (int i = 0; i < sceneSaveData.savedGOs.Length; i++)
                    {
                        sceneSaveData.savedGOs[i].Generate();
                    }
                
                    Camera.SceneCamera.SetRotation(sceneSaveData.cameraPos, sceneSaveData.cameraForward, sceneSaveData.cameraUp);
                    RenderConfig.SetData(sceneSaveData.RenderSaveData);
                }
            }
            
            playerCamera = SceneManager.FindObjectOfType<PlayerCamera>();
            if (playerCamera == null) {
                GameObject go = new GameObject("Main Camera");
                playerCamera = new PlayerCamera(go);
            }
            
            Debug.LogWarning("Scene Loaded");
        }

        bool zooming;

        private unsafe void SceneMovement()
        {
            if (!Engine.IsEditor || isPlaying) return;

            if (Input.GetKey(Keys.LeftControl) && Input.GetKeyUp(Keys.S))
            {
                Debug.LogWarning("scene saved via ctrl+s");
                SaveScene();
            }

            if (Input.GetKey(Keys.LeftControl) && Input.GetKeyUp(Keys.D))
            {
                Inspector.Duplicate();
            }

            #region raycasting
            // if (Input.MouseButton(MouseButton.Left))
            // {
            //     if (BepuHandle.Raycast(new Ray(Camera.SceneCamera.Position, Camera.SceneCamera.Front), out HitHandler hit))
            //     {
            //         Debug.Log("hit: " + hit);
            //     }
            //     #region bulletRaycast
            //     // if (BulletPhysics.RayCastCameraMiddle(out RayResult callback))
            //     // {
            //     //     var hitTransform = callback.GetTransform();
            //     //     sphere.transform.position = callback.HitPointWorld.BulletToTK();
            //     // 
            //     //     if (hitTransform.gameObject.TryGetComponent(out MeshRenderer meshRenderer))
            //     //     {
            //     //         // if another object chosed and chosen object are gameobject 
            //     //         if (hitTransform.gameObject != Inspector.currentObject )
            //     //         {
            //     //             Debug.Log("has transform");
            //     //             if (Inspector.currentObject is GameObject oldGO)
            //     //             {
            //     //                 if (oldGO.TryGetComponent(out MeshRenderer oldRenderer))
            //     //                 {
            //     //                     oldRenderer.color -= new System.Numerics.Vector4(.5f, 0, 0, 0);
            //     //                 }
            //     //             }
            //     //             meshRenderer.color += new System.Numerics.Vector4(.5f, 0, 0, 0);
            //     //         }
            //     //     }
            //     // 
            //     //     Inspector.currentObject = hitTransform.gameObject;
            //     // 
            //     //     Debug.Log("hit name: " + hitTransform.name);
            //     // }
            //     #endregion
            // }
            #endregion raycasting

            if (zooming || !SceneViewWindow.instance.Focused) return;

            // when press f key camera directly goes sellected object
            if (Input.GetKeyUp(Keys.F))
            {
                if (Inspector.currentObject is GameObject go)
                {
                    zooming = true;

                    RegisterUpdate.UpdateWhile(() =>
                    {
                        Camera.SceneCamera.Position = Vector3.Lerp(Camera.SceneCamera.Position, go.transform.position, Time.DeltaTime * 5);
                        Camera.SceneCamera.UpdateVectors();
                    }, () => go.transform.Distance(Camera.SceneCamera.Position) > 1 && !Input.Any(), endAction: () => zooming = false);
                }
            }

            if (!Input.MouseButtonDown(MouseButton.Right) || (/*!GameViewWindow.instance.Hovered &&*/ !SceneViewWindow.instance.Focused)) {
                Program.MainGame.CursorVisible = true;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default; // return default cursor
                return;
            }

            // change cursor
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeAll;

            // zoom in and out with scroll
            Camera.SceneCamera.Position += Input.ScrollY * (Camera.SceneCamera.Front * 100) * Time.DeltaTime;

            Program.MainGame.CursorVisible = false;
            float targetMoveSpeed = Input.GetKey(Keys.LeftShift) ? cameraMoveSpeed * 8 : cameraMoveSpeed;

            if (Input.GetKey(Keys.W)) Camera.SceneCamera.Position += Camera.SceneCamera.Front * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.S)) Camera.SceneCamera.Position -= Camera.SceneCamera.Front * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.A)) Camera.SceneCamera.Position -= Camera.SceneCamera.Right * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.D)) Camera.SceneCamera.Position += Camera.SceneCamera.Right * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.Q)) Camera.SceneCamera.Position -= Camera.SceneCamera.Up * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.E)) Camera.SceneCamera.Position += Camera.SceneCamera.Up * targetMoveSpeed * Time.DeltaTime;

            Vector2 mouseDirection = Input.MousePosition() - mouseOldPos;
            if (mouseDirection.Length < 200)
            {
                Camera.SceneCamera.Pitch -= mouseDirection.Y * Time.DeltaTime * cameraRotateSpeed;
                Camera.SceneCamera.Yaw += mouseDirection.X * Time.DeltaTime * cameraRotateSpeed;
            }

            MouseBindings.InfiniteMouse();

            mouseOldPos = Input.MousePosition();
        }

        public GameObject AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        internal void DestroyScene()
        {
            SceneManager.currentScene = null;
            RenderConfig.lights.Clear(); 
            gameObjects.ForEach(go => go.Dispose());
            gameObjects.Clear();
        }
        
        public void Dispose()
        {
            DestroyScene();
            GC.SuppressFinalize(this);
        }
    }
}