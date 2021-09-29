// this class manages shaders and materials each material registers here and renders here
// and shadow rendering and managind turns here

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

#nullable disable warnings

namespace ZargoEngine.Rendering {

    public static class Renderer3D
    {
        private static readonly Shader DepthShader = null;// AssetManager.GetShader("DepthVert.glsl","Depth.frag");

        internal sealed record ShaderMaterials(in Shader shader, in List<Material> materials);
        internal static readonly List<ShaderMaterials> Shaders = new List<ShaderMaterials>();

        internal static void RenderMaterials(ICamera camera)
        {
            // loop every material in game  
            foreach (var (shader, materials) in Shaders) // key = shader value = material
            {
                shader.Use();
                shader.SetDefaults(camera);
                
                for (short i = 0; i < materials.Count; i++)
                {
                    materials[i].Render();
                }

                Shader.DetachShader();
            }
        }

        /// <summary>
        /// renders depth texture relative to frame buffer
        /// this method can be usefull for shadows and depth effects
        /// </summary>
        internal static void RenderDepthTexture(in Matrix4 viewMatrix, in Matrix4 projectionMatrix, in int frameBuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            {
                DepthShader.Use();
                DepthShader.SetMatrix4Location(28, viewMatrix * projectionMatrix);
                foreach (var shaderMaterial in Shaders)
                {
                    foreach (var material in shaderMaterial.materials)
                    {
                        material.RenderMeshes();
                    }
                }
                DepthShader.Detach();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }


        // Registering and Managing
        internal static void AssignMaterial(Material material)
        {
            ShaderMaterials containingPair = Shaders.Find(shaderPair => shaderPair.shader == material.shader);

            if (containingPair != null)
            {
                if (!containingPair.materials.Contains(material)) // block assigning same material mltiple times
                {
                    containingPair.materials.Add(material);
                }
            }
            else
            {
                Shaders.Add(new ShaderMaterials(material.shader, new List<Material>() { material }));
            }
        }

        internal static void RemoveMaterial(Material material)
        {
            ShaderMaterials containingPair = Shaders.Find(shaderPair => shaderPair.shader == material.shader);
            containingPair?.materials.Remove(material);
        }

        internal static void OnShaderChanged(Material material, Shader beforeShader, Shader afterShader)
        {
            if (beforeShader != afterShader)
            {
                Shaders.Find(pair => pair.shader == beforeShader)?.materials.Remove(material);
                material.ChangeShader(afterShader);
                AssignMaterial(material);
            }
        }
    } 
}