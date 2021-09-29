namespace ZargoEngine
{
    public static class EngineConstsants
    {
        // file names
        public const string obj  = ".obj" , fbx   = ".fbx"  ,
                            png  = ".png" , jpg   = ".jpg"  ,
                            vaw  = ".vaw" , ogg   = ".ogg"  ,
                            gltf = ".gltf", blend = ".blend",
                            glsl = ".glsl", cs    = ".cs"   ,
                            mp3  = ".mp3" , tga   = ".tga"  ,
                            vert = ".vert", frag  = ".frag" ,
                            dae =  ".dae" , mat   = ".mat"  ; 

        public static readonly Assimp.Vector2D Zero2 = new Assimp.Vector2D(0, 0);
        public static readonly Assimp.Vector3D Zero  = new Assimp.Vector3D(0, 0, 0);
    }
}
