using Assimp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using System.IO;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using ZargoEngine.Helper;
using System.Linq;
#nullable disable warnings

namespace ZargoEngine.Rendering
{
    using AIBone  = Assimp.Bone; 
    using AIScene = Assimp.Scene;
    using AIMesh  = Assimp.Mesh;
    using AIMaterial = Assimp.Material;

    public static class AssimpImporter
    {
        const PostProcessSteps flags = PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SortByPrimitiveType   |
                                       PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateNormals       |
                                       PostProcessSteps.Triangulate           | PostProcessSteps.FixInFacingNormals    |
                                       PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.ValidateDataStructure |
                                       PostProcessSteps.OptimizeGraph         | PostProcessSteps.OptimizeMeshes         ;


        /// <summary> this must used for loading once 
        /// like draging to scene so editor only </summary> 
        public static GameObject ImportAssimpScene(in string path)
        {
            AssimpContext context = new AssimpContext();
            AIScene scene = context.ImportFile(path, flags);

            Mesh[] meshes = new Mesh[scene.Meshes.Count];
            Material[] materials = new Material[scene.Materials.Count];
            string directory = path.Remove(path.Length - Path.GetFileName(path).Length);

            // import materials
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                AIMaterial aiMaterial = scene.Materials[i];
                materials[i] = AssetManager.GetMaterial(directory + scene.RootNode.Name + scene.Materials[i].Name + ".mat");

                Debug.LogError(i);

                if (aiMaterial.HasTextureDiffuse)
                {
                    if (!File.Exists(directory + aiMaterial.TextureDiffuse.FilePath)) continue;
                    var texture = AssetManager.GetTexture(directory + aiMaterial.TextureDiffuse.FilePath, generateMipMap: true);
                    texture.SetWrapS(ConvertWrapModeToOTK(aiMaterial.TextureDiffuse.WrapModeU));
                    texture.SetWrapT(ConvertWrapModeToOTK(aiMaterial.TextureDiffuse.WrapModeU));
                    materials[i].SetTexture(0, texture);
                }
                if (aiMaterial.HasTextureSpecular)
                {
                    if (!File.Exists(directory + aiMaterial.TextureSpecular.FilePath)) continue;

                    var texture = AssetManager.GetTexture(directory + aiMaterial.TextureSpecular.FilePath, generateMipMap: true);
                    texture.SetWrapS(ConvertWrapModeToOTK(aiMaterial.TextureSpecular.WrapModeU));
                    texture.SetWrapT(ConvertWrapModeToOTK(aiMaterial.TextureSpecular.WrapModeU));
                    materials[i].SetTexture(1, texture);
                    materials[i].SetFloat("specPower", aiMaterial.Shininess);
                }
                // todo importing bump map
                materials[i].SaveToFile();
            }
            
            // import meshes
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                AIMesh mesh = scene.Meshes[i];
                meshes[i] = new Mesh(path, mesh); // creates a mesh from aiMesh
            }

            var mainGO = default(GameObject);
            recurisiveLoad(scene.RootNode, new GameObject(scene.RootNode.Name));

            void recurisiveLoad(Node node, GameObject go)
            {
                go.transform.SetMatrix(node.Transform.toTK());
                
                for (int i = 0; i < node.MeshIndices.Count; i++)
                {
                    if (i == 0) mainGO = go;
                    Debug.LogIf(go.transform.scale != Vector3.Zero, "node scale is not 1, shadow can be broken");
                    new MeshRenderer(meshes[i], go, materials[scene.Meshes[i].MaterialIndex]);
                }

                for (int i = 0; i < node.Children.Count; i++)
                {
                    var child = new GameObject(node.Children[i].Name);
                    go.transform.AddChild(child);
                    recurisiveLoad(node.Children[i], child);
                }
            }

            // save meshes as binary 
            Serializer.GetWriter(directory + Path.GetFileNameWithoutExtension(path) + ".mesh", out var writer, out var stream);

            writer.Write((ushort)meshes.Length);

            for (ushort i = 0; i < meshes.Length; i++) { 
                writer.Write(meshes[i].name);
            }

            for (ushort i = 0; i < meshes.Length; i++) {
                Mesh mesh = meshes[i];
                mesh.SaveToFile(writer);
            }

            writer.Dispose();
            stream.Dispose();

            scene.Clear();

            return mainGO;
        }

        // converts assimp mesh to binary mesh, saves it and creates meshes for further usages in asset manager
        public static void ConvertToBinary(in string path)
        {
            AssimpContext context = new AssimpContext();
            AIScene scene = context.ImportFile(path, flags);

            Mesh[] meshes = new Mesh[scene.Meshes.Count];
            string directory = path.Remove(path.Length - Path.GetFileName(path).Length);

            // import meshes
            for (int i = 0; i < scene.Meshes.Count; i++) {
                AIMesh mesh = scene.Meshes[i];
                meshes[i] = new Mesh(path, mesh); // creates a mesh from aiMesh
            }

            // save meshes as binary 
            Serializer.GetWriter(directory + Path.GetFileNameWithoutExtension(path) + ".mesh", out var writer, out var stream);

            writer.Write((ushort)meshes.Length);

            for (ushort i = 0; i < meshes.Length; i++) {
                writer.Write(meshes[i].name);
            }

            for (ushort i = 0; i < meshes.Length; i++) {
                Mesh mesh = meshes[i];
                mesh.SaveToFile(writer);
            }

            writer.Dispose();
            stream.Dispose();
        }

        public static void ImportBinaryMeshes(string path)
        {
            Serializer.GetReader(path, out var reader, out var stream);

            ushort meshCount = reader.ReadUInt16();

            Mesh[] meshes = new Mesh[meshCount];

            for (ushort i = 0; i < meshCount; i++) {
                string name = reader.ReadString();
                meshes[i] =  new Mesh(path + '|' + name) { name = name};
            }

            for (ushort i = 0; i < meshCount; i++) {
                meshes[i].LoadFromFile(reader);
            }

            reader.Dispose();
            stream.Dispose();
        }

        private static TextureWrapMode ConvertWrapModeToOTK(Assimp.TextureWrapMode textureWrapMode)
        {
            return textureWrapMode switch
            {
                Assimp.TextureWrapMode.Clamp => TextureWrapMode.ClampToEdge,
                Assimp.TextureWrapMode.Decal => TextureWrapMode.ClampToBorder,
                Assimp.TextureWrapMode.Mirror => TextureWrapMode.MirroredRepeat,
                Assimp.TextureWrapMode.Wrap => TextureWrapMode.Repeat,
                _ => TextureWrapMode.ClampToBorder,
            };
        }

        public static void LoadVladh(in string path, out SkinnedMesh skinnedMesh, out Animator animation)
        {
            AssimpContext context = new AssimpContext();
            AIScene scene = context.ImportFile(path, flags);

            int vertexCount = 0;

            for (byte i = 0; i < scene.Meshes.Count; i++) vertexCount += scene.Meshes[i].Vertices.Count;

            Vector3D[] positions = new Vector3D[vertexCount]; Vector2D[] texCoords  = new Vector2D[vertexCount];
            Vector3D[] normals   = new Vector3D[vertexCount]; List<int>  indiceList = new List<int>();
            Vector4[] weights    = new Vector4[vertexCount];  Vector4i[] ids        = new Vector4i[vertexCount];

            for (uint i = 0; i < vertexCount; i++)
            {
                weights[i] = new Vector4(-1);
                ids[i] = new Vector4i(-1);
            }

            int boneIndex = 0, vertexIndex = 0;

            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            foreach (var mesh in scene.Meshes)
            {
                // adding bone data to vertexes and creating bonemap
                for (ushort boneID = 0; boneID < mesh.BoneCount; boneID++) // max 65k bones :)
                {
                    AIBone bone = mesh.Bones[boneID];
                    boneMap.Add(bone.Name, boneIndex + boneID);
                    foreach (VertexWeight weight in bone.VertexWeights)
                    {
                        int weightIndex = vertexIndex + weight.VertexID;
                        AddBoneData(ref weights[weightIndex], ref ids[weightIndex], weight.Weight, boneIndex + boneID);
                    }
                }

                // nothing special here coppying data from assimp 
                {
                    indiceList.AddRange(mesh.GetIndices());
                    mesh.Vertices.CopyTo(0, positions, vertexIndex, mesh.Vertices.Count);
                    
                    if (mesh.HasTangentBasis)
                    mesh.Normals.CopyTo(0, normals, vertexIndex, mesh.Vertices.Count);

                    if (mesh.HasTextureCoords(0))
                    Array.Copy(ToVec2Array(mesh.TextureCoordinateChannels[0].ToArray()), 0, texCoords, vertexIndex, mesh.Vertices.Count);
                }
                
                boneIndex += mesh.BoneCount;
                vertexIndex += mesh.VertexCount; 
            }

            animation = new Animator(scene, boneMap);
            skinnedMesh = new SkinnedMesh(path, positions, texCoords, normals, indiceList.ToArray(), weights, ids);
        }

        private static void AddBoneData(ref Vector4 weights, ref Vector4i ids, in float weight, in int boneIndex)
        {
            for (byte i = 0; i < 4; i++)
            {
                if (weights[i] == -1)
                {
                    weights[i] = weight;
                    ids[i] = boneIndex;
                    return;
                }
            }
        }

        public unsafe static Vector2D[] ToVec2Array(Vector3D[] vec3Array)
        {
            Vector2D[] outVecs = new Vector2D[vec3Array.Length];

            for (int i = 0; i < vec3Array.Length; i++)
            {
                fixed (void* outVecPtr = &outVecs[i])
                fixed (void* vec3Ptr = &vec3Array[i])
                Unsafe.CopyBlock(outVecPtr, vec3Ptr, (uint)Vector2.SizeInBytes);
            }
            return outVecs;
        }
    }
}