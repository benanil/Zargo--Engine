using System;
using System.Collections.Generic;
using System.IO;
using Assimp;

using AIscene = Assimp.Scene;
using AIMesh  = Assimp.Mesh;

namespace ZargoEngine.Rendering
{
    public static class FbxExporter
    {

        //public static void Export(Mesh mesh, in string path)
        //{
        //    if (!File.Exists(path)) {
        //        Debug.LogError("export path is not exist: " + path);
        //        return;
        //    }
        //    
        //    AIscene scene = new AIscene();
        //    AssimpContext context = new AssimpContext();
        //    ConvertToAIMesh(mesh, out AIMesh aiMesh); 
        //
        //    scene.Meshes.Add(aiMesh);
        //
        //    context.ExportFile(scene, path, "fbx");
        //}

        // private static void ConvertToAIMesh(Mesh mesh, out AIMesh aIMesh)
        // {
        //     aIMesh = new AIMesh(mesh.name, PrimitiveType.Triangle);
        // 
        //     aIMesh.Vertices.Capacity = mesh.vertices.Length; 
        //     aIMesh.Normals.Capacity  = mesh.vertices.Length;
        //     aIMesh.TextureCoordinateChannels[0] = new List<Vector3D>(mesh.vertices.Length);
        // 
        //     for (int i = 0; i < mesh.vertices.Length; i++)
        //     {
        //         aIMesh.Vertices[i] = mesh.vertices[i].Position;
        //         aIMesh.Normals[i]  = mesh.vertices[i].Normal;
        //         aIMesh.TextureCoordinateChannels[0][i] = mesh.vertices[i].Position;
        //     }
        // }
    }
}
