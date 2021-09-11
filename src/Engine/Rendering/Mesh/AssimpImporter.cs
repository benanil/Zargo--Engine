using Assimp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    using AIBone  = Assimp.Bone; 
    using AIScene = Assimp.Scene;
    using AIMesh  = Assimp.Mesh;
    
    public static class AssimpImporter
    {
        const PostProcessSteps flags = PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.SortByPrimitiveType   |
                                       PostProcessSteps.CalculateTangentSpace | PostProcessSteps.GenerateNormals       |
                                       PostProcessSteps.Triangulate           | PostProcessSteps.FixInFacingNormals    |
                                       PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.ValidateDataStructure |
                                       PostProcessSteps.OptimizeGraph         | PostProcessSteps.OptimizeMeshes         ;

        public static void LoadMesh(in string path,
            out Vector3D[] positions, out Vector2D[] texCoords,
            out Vector3D[] normals  , out int[] indices) {

            AssimpContext context = new AssimpContext();
            AIScene scene = context.ImportFile(path, flags);

            Debug.Assert(scene == null, "assimp error when mesh importing (scene is null)\n path: " + path);

            int i = 0; int vertexCount = 0; 
            for (; i < scene.Meshes.Count; i++) vertexCount += scene.Meshes[i].Vertices.Count;
    
            positions = new Vector3D[vertexCount]; texCoords = new Vector2D[vertexCount];
            normals   = new Vector3D[vertexCount]; List<int> indiceList = new List<int>();
    
            int lastVertexIndex = 0;
            for (i = 0; i < scene.Meshes.Count; i++)
            {
                AIMesh mesh = scene.Meshes[i];

                indiceList.AddRange(mesh.GetIndices());
    
                if (mesh.HasTextureCoords(0))
                    mesh.Vertices.CopyTo(0, positions, lastVertexIndex, mesh.Vertices.Count);
                if (mesh.HasTangentBasis)
                    mesh.Normals.CopyTo(0, normals, lastVertexIndex, mesh.Vertices.Count);

                Array.Copy(ToVec2Array(mesh.TextureCoordinateChannels[0].ToArray()), 0, texCoords, lastVertexIndex, mesh.Vertices.Count);

                lastVertexIndex = mesh.VertexCount;
            }

            indices = indiceList.ToArray();
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