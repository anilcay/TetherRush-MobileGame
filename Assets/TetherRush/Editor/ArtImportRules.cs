using System;
using UnityEditor;

namespace TetherRush.Editor
{
    /// <summary>Controlled FBX delivery; the .blend source is kept outside Assets.</summary>
    public sealed class ArtImportRules : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/TetherRush/Resources/ArtModels/", StringComparison.Ordinal)) return;
            var model = (ModelImporter)assetImporter;
            model.importAnimation = false;
            model.addCollider = false;
            model.importNormals = ModelImporterNormals.Import;
            model.bakeAxisConversion = true;
            model.useFileScale = true;
            model.globalScale = 1;
            model.isReadable = false;
        }
        public override uint GetVersion() => 1;
    }
}
