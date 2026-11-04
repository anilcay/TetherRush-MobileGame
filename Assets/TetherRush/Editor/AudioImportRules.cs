using UnityEditor;
using UnityEngine;

namespace TetherRush.Editor
{
    public sealed class AudioImportRules : AssetPostprocessor
    {
        public override uint GetVersion() => 1;

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/TetherRush/Resources/Audio/")) return;
            var importer = (AudioImporter)assetImporter;
            bool music = assetPath.EndsWith("TetherFlow.ogg");
            // Effects are authored mono already. Enabling Force To Mono also enables
            // Unity's default normalization, which would undo the authored headroom.
            importer.forceToMono = false;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality = music ? .7f : 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
        }
    }
}
