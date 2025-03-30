using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#nullable enable

public class MEVariableDeclarationGenerator
{
    const string LastSavePathKey = "yarn_me_lastgeneratedfilepath";

    enum VariableType
    {
        Bool,
        Int
    }

    [MenuItem("Tools/Generate ME Plot Variables Yarn File...")]
    public static void GenerateYarnFile()
    {
        var selection = EditorUtility.OpenFolderPanel("Save Generated Variables", "Choose a location to save the generated file.", GeneratedFilesDirectory ?? Environment.CurrentDirectory);

        if (selection == null)
        {
            return;
        }

        GeneratedFilesDirectory = selection;

        var stringBuilder = new StringBuilder();

        var invalidCharacterRegex = new Regex(@"[^A-Za-z0-9_]");

        var seenVariables = new HashSet<string>();

        stringBuilder.AppendLine("title: MEBools");
        stringBuilder.AppendLine("---");

        var databases = new List<(PlotDatabase Database, string AccessorPrefix)> {
            (ME1PlotDatabase.Value, "me1"),
            (ME2PlotDatabase.Value, "me2"),
        };

        foreach (var database in databases)
        {
            foreach (var plotElement in database.Database.AllElements.Values)
            {
                VariableType variableType;

                if (plotElement is PlotBool plotBool)
                {
                    variableType = VariableType.Bool;

                    if (plotBool.SubType == PlotElementType.State)
                    {
                        // This is not a single stored value, but rather a
                        // _group_ of stored values. The state resolves to true
                        // if any of its sub-states are true.

                        // TODO: create smart variables for States that are true if
                        // any of their SubStates are true
                        continue;
                    }
                }
                else if (plotElement is PlotInteger)
                {
                    variableType = VariableType.Int;
                }
                else if (plotElement is PlotGroup)
                {
                    // ignore - not a variable, just a container for other variables
                    continue;
                }
                else
                {
                    Debug.LogWarning($"Unknown plot element type {plotElement.GetType()} for \"{plotElement.Label}\"");
                    continue;
                }

                var variableName = string.Join("_", database.Database.GetInheritanceChain(plotElement).Select(e => e.Label));

                variableName = invalidCharacterRegex.Replace(variableName, "_");
                variableName = "$" + variableName;

                if (seenVariables.Contains(variableName))
                {
                    continue;
                }
                else
                {
                    seenVariables.Add(variableName);
                }

                switch (variableType)
                {
                    case VariableType.Bool:
                        stringBuilder.AppendLine($"<<declare {variableName} = get_{database.AccessorPrefix}_bool({plotElement.PlotId}) as bool>>");
                        break;
                    case VariableType.Int:
                        stringBuilder.AppendLine($"<<declare {variableName} = get_{database.AccessorPrefix}_int({plotElement.PlotId}) as number>>");
                        break;
                }

            }
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine("===");

        var yarnFile = Path.Combine(GeneratedFilesDirectory, "GeneratedVariables.yarn");

        File.WriteAllText(yarnFile, stringBuilder.ToString());
        AssetDatabase.ImportAsset(yarnFile);

        static void PingPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
        }

        PingPath(yarnFile);

    }

    private static PlotDatabase GetPlotDatabase(string name)
    {
        var plotDatabaseGUID = AssetDatabase.FindAssets(name).FirstOrDefault()
                    ?? throw new FileNotFoundException($"Failed to find {name} plot database file");

        var plotDatabasePath = AssetDatabase.GUIDToAssetPath(plotDatabaseGUID);

        var plotDatabaseJSON = File.ReadAllText(plotDatabasePath);

        try
        {
            var plotDatabase = JsonSerializer.Deserialize<PlotDatabase>(plotDatabaseJSON, serializerOptions);
            return plotDatabase ?? throw new InvalidOperationException($"Failed to parse {name} plot database file for an unknown reason.");
        }
        catch (JsonException e)
        {
            Debug.LogError($"{nameof(JsonException)} thrown when parsing plot database: line {e.LineNumber + 1}: {e.Message}");
            throw;
        }
    }

    public static Lazy<PlotDatabase> ME1PlotDatabase = new(() => GetPlotDatabase("le1"));
    public static Lazy<PlotDatabase> ME2PlotDatabase = new(() => GetPlotDatabase("le2"));

    private static JsonSerializerOptions serializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = {
            new JsonStringEnumConverter<PlotElementType>()
        }
    };

    private static string? GeneratedFilesDirectory
    {
        get
        {
            if (EditorPrefs.HasKey(LastSavePathKey))
            {
                var path = EditorPrefs.GetString(LastSavePathKey);

                if (Path.IsPathRooted(path))
                {
                    var projectPath = Directory.GetParent(Application.dataPath).ToString();
                    path = Path.GetRelativePath(projectPath, path);
                }
                return Directory.Exists(path) ? path : null;
            }
            else
            {
                return null;
            }
        }
        set
        {
            var path = value;
            if (Path.IsPathRooted(path))
            {
                var projectPath = Directory.GetParent(Application.dataPath).ToString();
                path = Path.GetRelativePath(projectPath, path);
            }
            EditorPrefs.SetString(LastSavePathKey, path);
        }
    }

}


