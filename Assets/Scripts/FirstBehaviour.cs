
using ImGuiNET;
using OpenTK.Mathematics;
using ZargoEngine.Editor;
using ZargoEngine.Editor.Attributes;
using ZargoEngine.Media.Sound;
using ZargoEngine.Sound;

namespace ZargoEngine
{
    
    public class FirstBehaviour : MonoBehaviour
    {

        [NonSerialized]
        public ImGuiBackendFlags backendFlags;

        public AudioClip sound;

        readonly AudioSource audioSource;
        
        public FirstBehaviour(GameObject go) : base(go)
        {
            audioSource = new AudioSource();
            sound = new AudioClip(go, ref audioSource, AssetManager.GetFileLocation("Sounds/Car Engine start.wav"));
            name = "First Behaviour";
        }

        public override void DrawWindow()
        {
            base.DrawWindow();
            audioSource.DrawWindow();
        }

        public override void Start(){
            gameObject.AddComponent(sound);
        }

        [Button(name = "start")]
        public void StartSound()
        {
            audioSource.Play();
        }

        [Button(name = "stop")]
        public void Stop()
        {
            audioSource.Stop();
        }

    }
}