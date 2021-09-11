using OpenTK.Audio.OpenAL;

namespace ZargoEngine.Media.Sound
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    public class AudioSettings
    {
        public ALContextAttributes Attributes { get; set; } = new();

        private const string DIR_NAME = "%AppData%";
        private const string FILE_NAME = "alsoft.ini";
    }
}
