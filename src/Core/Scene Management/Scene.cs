using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using ZargoEngine.Rendering;
using ZargoEngine.Bindings;
using ZargoEngine.Editor;
using ZargoEngine.Core;
using ZargoEngine.SaveLoad;
using ZargoEngine.AnilTools;
using ZargoEngine.Physics;

#nullable disable warnings

namespace ZargoEngine
{
    public class Scene : IDisposable
    {
        public string name;
        public RenderHandeller renderHandeller;

        public List<GameObject> gameObjects = new List<GameObject>();
        public List<RendererBase> meshRenderers = new List<RendererBase>();

        private bool started;
        private Vector2 mouseOldPos;
        public float cameraRotateSpeed = 100, cameraMoveSpeed = 3f;

        Camera camera => renderHandeller.camera;

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

        public Scene(RenderHandeller renderHandeller, string name)
        {
            this.name = SceneManager.GetUniqeName(name);
            this.renderHandeller = renderHandeller;
        }

        public Scene(RenderHandeller renderHandeller)
        {
            this.name = SceneManager.GetName();
            this.renderHandeller = renderHandeller;
        }

        public void Start()
        {
            LoadScene();

            mouseOldPos = Input.MousePosition();
            started = true;

            for (short i = 0; i < gameObjects.Count; i++) {
                gameObjects[i].Start();
            }
        }

        public void Render()
        {
            if (!started) return;

            for (short i = 0; i < gameObjects.Count; i++) {
                gameObjects[i].Render();
            }
        }

        public void Update()
        {
            if (!started) return;

            for (int i = 0; i < gameObjects.Count; i++){
                gameObjects[i].Update();
            }

            if (Input.GetKeyDown(Keys.Enter) || Input.GetKeyDown(Keys.KeyPadEnter)){
                LogGame();
            }

            SceneMovement();
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

        public void LogGame()
        {
            Debug.Log("model: " + meshRenderers[0].transform.Translation);
            Debug.Log("view: " + RenderHandeller.instance.camera.ViewMatrix);
            Debug.Log("projection: " + RenderHandeller.instance.camera.projectionMatrix);
        }

        private const string SceneSaveFileName = "/SceneSave.xml";
        private const string RenderSettingsFileName = "/RenderSettings.xml";

        public void SaveScene()
        {
            string saveDirectory = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Middle Games"), ProjectSettings.ProjectName);
            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

            // save Scene
            { 
                string sceneSaveFilePath = saveDirectory + SceneSaveFileName;
                XmlSerializer serializer  = new XmlSerializer(typeof(SceneSaveData));

                FileStream stream;
                if (!File.Exists(sceneSaveFilePath)) 
                     stream = File.Create(sceneSaveFilePath);
                else stream = new FileStream(sceneSaveFilePath, FileMode.OpenOrCreate);
                
                serializer.Serialize(stream, new SceneSaveData(gameObjects, camera));
                stream.Close(); stream.Dispose();
            }

            // save render settings
            {
                string renderSettingsFilePath = saveDirectory + RenderSettingsFileName;
                XmlSerializer serializer = new XmlSerializer(typeof(RenderSaveData));

                FileStream stream;
                if (!File.Exists(renderSettingsFilePath))
                     stream = File.Create(renderSettingsFilePath);
                else stream = new FileStream(renderSettingsFilePath, FileMode.OpenOrCreate);

                serializer.Serialize(stream, new RenderSaveData(renderHandeller));
                stream.Close(); stream.Dispose();
            }

            Debug.Log("scene saved");
        }

        public void LoadScene()
        {
            string saveDirectory = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Middle Games"), ProjectSettings.ProjectName);
            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

            if (File.Exists(saveDirectory + SceneSaveFileName))
            { 
                using (StreamReader reader = new StreamReader(saveDirectory + SceneSaveFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SceneSaveData));

                    SceneSaveData? holder = (SceneSaveData)serializer.Deserialize(reader);

                    holder?.savedGameObjects.ForEach(x => x.Load());
                    //camera.SetRotation(holder.cameraProperties);
                }
            }

            if (File.Exists(saveDirectory + RenderSettingsFileName))
            {
                using (StreamReader reader1 = new StreamReader(saveDirectory + RenderSettingsFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RenderSaveData));

                    ((RenderSaveData)serializer.Deserialize(reader1))?.Apply(renderHandeller);
                }
            }
        }

        bool zooming;

        private unsafe void SceneMovement()
        {
            if (Input.GetKey(Keys.LeftControl) && Input.GetKeyUp(Keys.S))
            {
                SaveScene();
            }

            if (Input.GetKey(Keys.LeftControl) && Input.GetKeyUp(Keys.D))
            {
                Inspector.Duplicate();
            }

            if (Input.MouseButton(MouseButton.Left))
            {
                if (BepuHandle.Raycast(new Ray(camera.Position, camera.Front), out HitHandler hit))
                {
                    Debug.Log("hit: " + hit);
                }
                #region bulletRaycast
                // if (BulletPhysics.RayCastCameraMiddle(out RayResult callback))
                // {
                //     var hitTransform = callback.GetTransform();
                //     sphere.transform.position = callback.HitPointWorld.BulletToTK();
                // 
                //     if (hitTransform.gameObject.TryGetComponent(out MeshRenderer meshRenderer))
                //     {
                //         // if another object chosed and chosen object are gameobject 
                //         if (hitTransform.gameObject != Inspector.currentObject )
                //         {
                //             Debug.Log("has transform");
                //             if (Inspector.currentObject is GameObject oldGO)
                //             {
                //                 if (oldGO.TryGetComponent(out MeshRenderer oldRenderer))
                //                 {
                //                     oldRenderer.color -= new System.Numerics.Vector4(.5f, 0, 0, 0);
                //                 }
                //             }
                //             meshRenderer.color += new System.Numerics.Vector4(.5f, 0, 0, 0);
                //         }
                //     }
                // 
                //     Inspector.currentObject = hitTransform.gameObject;
                // 
                //     Debug.Log("hit name: " + hitTransform.name);
                // }
                #endregion
            }

            if (zooming) return;

            // when press f key camera directly goes sellected object
            if (Input.GetKeyUp(Keys.F))
            {
                if (Inspector.currentObject is GameObject go)
                {
                    zooming = true;

                    RegisterUpdate.UpdateWhile(() =>
                    {
                        camera.Position = Vector3.Lerp(camera.Position, go.transform.position, Time.DeltaTime * 5);
                        camera.UpdateVectors();
                    }, () => go.transform.Distance(Camera.main.Position) > 1 && !Input.Any(), endAction: () => zooming = false);
                }
            }

            if (!Input.MouseButtonDown(MouseButton.Right) || (/*!GameViewWindow.instance.Hovered &&*/ !GameViewWindow.instance.Focused)) {
                Program.MainGame.CursorVisible = true;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default; // return default cursor
                return;
            }

            // change cursor
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeAll;

            // zoom in and out with scroll
            camera.Position += Input.ScrollY * (camera.Front * 100) * Time.DeltaTime;

            Program.MainGame.CursorVisible = false;
            float targetMoveSpeed = Input.GetKey(Keys.LeftShift) ? cameraMoveSpeed * 8 : cameraMoveSpeed;

            if (Input.GetKey(Keys.W)) camera.Position += camera.Front * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.S)) camera.Position -= camera.Front * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.A)) camera.Position -= camera.Right * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.D)) camera.Position += camera.Right * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.Q)) camera.Position -= camera.Up * targetMoveSpeed * Time.DeltaTime;
            if (Input.GetKey(Keys.E)) camera.Position += camera.Up * targetMoveSpeed * Time.DeltaTime;

            Vector2 mouseDirection = Input.MousePosition() - mouseOldPos;
            if (mouseDirection.Length < 200)
            {
                renderHandeller.camera.Pitch -= mouseDirection.Y * Time.DeltaTime * cameraRotateSpeed;
                renderHandeller.camera.Yaw += mouseDirection.X * Time.DeltaTime * cameraRotateSpeed;
            }

            MouseBindings.InfiniteMouse();

            mouseOldPos = Input.MousePosition();
        }

        public void Stop()
        {
            started = false;
        }

        public GameObject AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
            return gameObject;
        }

        public RendererBase AddMeshRenderer(RendererBase meshRenderer)
        {
            meshRenderers.Add(meshRenderer);
            return meshRenderer;
        }

        public void Dispose()
        {
            //gameObjects.ForEach(x => Dispose());
            meshRenderers.ForEach(x => x.Dispose());
            GC.SuppressFinalize(this);
        }
    }
}