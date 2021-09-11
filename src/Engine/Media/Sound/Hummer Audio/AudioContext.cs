using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Audio.OpenAL;

namespace ZargoEngine.Media.Sound
{
    // this codes base writen by discord: 𝓗𝓾𝓜𝓜𝓮𝓡#6619
    // Windows Dir string path = Path.GetPathRoot(Environment.SystemDirectory);
    // https://openal-soft.org/#download
    public class AudioContext : IDisposable
    {
        

        public static string DefaultDevice => AvailableDevices.Any() ? AvailableDevices[0] : default;
        public static IList<string> AvailableDevices => ALC.GetString(AlcGetStringList.AllDevicesSpecifier);
        public static AudioContext CurrentContext { get; private set; }

        public string CurrentDevice { get; }
        public AlcError CurrentError { get; private set; }

        public AudioContext() : this(default, new()) { }
        public AudioContext(AudioSettings audioSettings) : this(default, audioSettings) { }
        public AudioContext(string device) : this(device, new()) { }
        public AudioContext(string device, AudioSettings audioSettings)
        {
            _device = ALC.OpenDevice(CurrentDevice = device ?? DefaultDevice);
            _context = ALC.CreateContext(_device, audioSettings.Attributes);
            
            MakeCurrent();

            CheckErrors();
            SupportsExtension(FLOAT32_EXTENSION);
        }

        public static List<AudioSource> audioSources = new List<AudioSource>();

        public void MakeCurrent()
        {
            if (!ALC.MakeContextCurrent(_context)){
                //throw new InvalidOperationException("Unable to make context current");
            }

            CurrentContext = this;
            Console.WriteLine(CurrentDevice);
        }

        public void CheckErrors()
        {
            var ALError = AL.GetError();
            if (ALError != ALError.NoError)
            {
                // throw new Exception($"OpenAL Error: {ALError}");
            }

            CurrentError = ALC.GetError(_device);
            if (CurrentError != AlcError.NoError)
            {
                // throw new Exception($"OpenALC Error: {CurrentError}");
            }
        }

        public bool SupportsExtension(string extension)
        {
            if (!AL.IsExtensionPresent(extension)){
                // Debug.LogError($"This program requires '{extension}' OpenAL extension please correctly initialize openal32.dll");
            }

            return true;
        }

        public void Dispose()
        {
            if (_context != IntPtr.Zero){
                ALC.MakeContextCurrent(default);
                ALC.DestroyContext(_context);
                _context = default;
            }

            if (_device != IntPtr.Zero){
                ALC.CloseDevice(_device);
                _device = default;
            }
        }

        private ALDevice _device;
        private ALContext _context;
        private const string FLOAT32_EXTENSION = "AL_EXT_FLOAT32";
    }
}
