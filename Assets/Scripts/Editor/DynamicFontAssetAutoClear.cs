// TextMeshPro dynamic font assets have a very annoying habit of saving their
// dynamically generated binary data in the same text file as their
// configuration data. This causes massive headaches for version control.
//
// This script addresses the above issue. It runs whenever any assets in the
// project are about to be saved. If any of those assets are a TMP dynamic font
// asset, they will have their dynamically generated data cleared before they
// are saved, which prevents that data from ever polluting the version control.
//
// For more information, see this thread: https://discussions.unity.com/t/868941


#if UNITY_EDITOR

using TMPro;
using UnityEditor;

internal class DynamicFontAssetAutoClear : AssetModificationProcessor
{
    protected static string[] OnWillSaveAssets(string[] paths)
    {
        foreach (string path in paths)
        {
            if (AssetDatabase.LoadAssetAtPath(path, typeof(TMP_FontAsset)) is TMP_FontAsset fontAsset)
            {
                if (fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                {
                    //Debug.Log("Clearing font asset data at " + path);
                    fontAsset.ClearFontAssetData(setAtlasSizeToZero: true);
                }
            }
        }

        return paths;
    }
}

#endif