using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.IO;
using ZargoEngine.Media.Codecs;
using ZargoEngine.Media.Sound;

namespace ZargoEngine.Sound
{
    public class AudioClip : Component, IDisposable
    {
        public IWaveProvider provider;

        public AudioClip(GameObject go, ref AudioSource source, string filePath) : base(go) {
            if (!File.Exists(filePath)) return;

            source = new AudioSource();

            if (!Path.HasExtension(filePath)){
                Debug.LogError("sound file has no extension");
                return;
            }

            string extension = Path.GetExtension(filePath);

            Debug.Log("sound file extension: " + extension);

            provider = extension switch
            {
                ".wav" => new WavCodec(filePath),
                ".raw" => new RawCodec(filePath),
                ".mp3" => new MP3Codec(filePath),
                ".ogg" => new VorbisWaveReader(filePath),
                _ => throw new NotImplementedException(),
            };

            source.Init(provider);

            Debug.Log("sound initialized");
        }

        public override void Dispose(){
            GC.SuppressFinalize(this);
        }
    }
}
