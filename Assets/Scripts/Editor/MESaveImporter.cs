using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "pcsav")]
public class MESaveImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var saveData = File.ReadAllBytes(ctx.assetPath);

        var asset = ScriptableObject.CreateInstance<MESaveData>();
        asset.data = saveData;

        ctx.AddObjectToAsset("save", asset);
        ctx.SetMainObject(asset);
    }
}
