using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Rendering
{
    public class Skybox : IDisposable
    {
        private const string texture0 = nameof(texture0);

        private readonly string[] textureLocations =
        {
            "Images/skybox/right.jpg",
            "Images/skybox/left.jpg",
            "Images/skybox/top.jpg",
            "Images/skybox/bottom.jpg",
            "Images/skybox/front.jpg",
            "Images/skybox/back.jpg"
        };
        // gpu efficent way
        // private static readonly float[] vertices = {
        //     -1.0f,  1.0f, -1.0f, 
        //     -1.0f, -1.0f, -1.0f, 
        //      1.0f, -1.0f, -1.0f, 
        //      1.0f,  1.0f, -1.0f, 
        //     -1.0f, -1.0f,  1.0f, 
        //     -1.0f,  1.0f,  1.0f, 
        //      1.0f, -1.0f,  1.0f, 
        //      1.0f,  1.0f,  1.0f  
        // };
        //  
        // private static readonly byte[] indices = {
        //     0, 1, 2, 2, 3, 0,
        //     4, 1, 0, 0, 5, 4,
        //     2, 6, 7, 7, 3, 2,
        //     4, 5, 7, 7, 6, 4,
        //     0, 3, 7, 7, 5, 0,
        //     1, 4, 2, 2, 4, 6
        // };
        // 

        // cpu efficent way
        private static readonly float[] vertices = {
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
                            
            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,
                            
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
    
            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,
                        
            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,
                            
            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };

        private readonly CubeMap cubeMap;
        private readonly Shader skyBoxShader;

        private readonly int vaoID, vboID;//, eboID;

        private static readonly Matrix4 ModelMatrix = Matrix4.CreateTranslation(Vector3.Zero) * Matrix4.CreateScale(10000);

        public Skybox()
        {
            skyBoxShader = AssetManager.GetShader("Shaders/Skybox/SkyboxVert.glsl", "Shaders/Skybox/SkyboxFrag.glsl");
            cubeMap = new CubeMap(textureLocations);

            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, 3 * sizeof(float), IntPtr.Zero);

            // eboID = GL.GenBuffer();
            // GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboID);
            // GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(byte) * indices.Length, indices, BufferUsageHint.StaticDraw);

            Debug.Log(GL.GetError());
        }

        public void Use(ICamera camera)
        {
            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);

            skyBoxShader.Use();
            skyBoxShader.SetInt(texture0, 0);

            skyBoxShader.SetMatrix4("model", ModelMatrix, true);
            skyBoxShader.SetMatrix4("projection", ref camera.GetProjectionMatrix(), true);
            skyBoxShader.SetMatrix4("view", ref camera.GetViewMatrix(), true);
            skyBoxShader.SetFloat("angle", RenderConfig.sun.angle);

            GL.BindVertexArray(vaoID);
            cubeMap.Bind();

            GL.EnableVertexAttribArray(0);

            // GL.DrawElements(PrimitiveType.Triangles,indices.Length, DrawElementsType.UnsignedByte, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.DisableVertexAttribArray(0);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);

            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vboID);
            // GL.DeleteBuffer(eboID);
            GC.SuppressFinalize(this);
        }

    }
}
