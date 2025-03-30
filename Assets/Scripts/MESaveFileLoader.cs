using System;
using System.IO;
using UnityEngine;
using Yarn.Unity;

#nullable enable

public class MESaveFileLoader : MonoBehaviour
{
    [SerializeField] MESaveData saveData;

    Gibbed.MassEffect2.FileFormats.SaveFileBase? saveFile;

    public static Gibbed.MassEffect2.FileFormats.SaveFileBase? CurrentSaveFile
    {
        get
        {
            var loader = FindAnyObjectByType<MESaveFileLoader>();
            if (loader == null)
            {
                Debug.LogWarning($"Can't get ME save data: no {nameof(MESaveFileLoader)} in scene");
                return null;
            }

            if (loader.saveFile == null)
            {
                Debug.LogWarning($"Can't get ME save data: no data loaded");
                return null;
            }

            return loader.saveFile;
        }
    }

    [YarnFunction("get_me_name")]
    public static string GetMEName() => CurrentSaveFile?.PlayerRecord.FirstName ?? "<not loaded>";

    [YarnFunction("get_me1_bool")]
    public static bool GetME1Bool(int index) => CurrentSaveFile?.ME1PlotRecord.GetBoolVariable(index) ?? false;

    [YarnFunction("get_me2_bool")]
    public static bool GetME2Bool(int index) => CurrentSaveFile?.PlotRecord.GetBoolVariable(index) ?? false;

    [YarnFunction("get_me1_int")]
    public static int GetME1Int(int index) => CurrentSaveFile?.ME1PlotRecord.GetIntVariable(index) ?? 0;

    [YarnFunction("get_me2_int")]
    public static int GetME2Int(int index) => CurrentSaveFile?.PlotRecord.GetIntVariable(index) ?? 0;

    void Start()
    {
        var saveStream = new MemoryStream(saveData.data);

        try
        {
            saveFile = Gibbed.MassEffect2.FileFormats.SaveFileBase.Load(saveStream);
        }
        catch (FormatException e)
        {
            // Failed to load the file
            Debug.LogError("Failed to load save file: " + e);
            saveFile = null;
            return;
        }
    }
}
