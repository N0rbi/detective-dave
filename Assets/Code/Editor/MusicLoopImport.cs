using UnityEditor;

namespace DetectiveDave.EditorTools
{
    static class MusicLoopImport
    {
        public static void MakeGapless(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) return;
            var settings = importer.defaultSampleSettings;
            if (!IsUnityDefault(settings)) return;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        static bool IsUnityDefault(AudioImporterSampleSettings settings) =>
            settings.loadType == AudioClipLoadType.DecompressOnLoad && settings.compressionFormat == AudioCompressionFormat.Vorbis;
    }
}
