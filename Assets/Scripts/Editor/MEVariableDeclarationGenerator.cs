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

    [MenuItem("Tools/Generate ME Plot Variables Yarn File...")]
    public static void GenerateYarnFile()
    {
        var stringBuilder = new StringBuilder();

        var variableData = ScriptableObject.CreateInstance<MEVariableData>();

        var invalidCharacterRegex = new Regex(@"[^A-Za-z0-9_]");

        stringBuilder.AppendLine("title: ME2Bools");
        stringBuilder.AppendLine("---");

        foreach (var plotBool in PlotDatabase.Value.AllElements.Value.Values)
        {

            string initialValue;
            MEVariableData.VariableType variableType;

            if (plotBool is PlotBool)
            {
                initialValue = "false";
                variableType = MEVariableData.VariableType.Bool;
            }
            else if (plotBool is PlotInteger)
            {
                initialValue = "0";
                variableType = MEVariableData.VariableType.Int;
            }
            else if (plotBool is PlotGroup)
            {
                // ignore - not a variable, just a container for other variables
                continue;
            }
            else
            {
                Debug.LogWarning($"Unknown plot element type {plotBool.GetType()} for \"{plotBool.Label}\"");
                continue;
            }

            var variableName = string.Join("_", PlotDatabase.Value.GetInheritanceChain(plotBool).Select(e => e.Label));
            variableName = invalidCharacterRegex.Replace(variableName, "_");
            variableName = "$" + variableName;

            stringBuilder.AppendLine($"<<declare {variableName} = {initialValue}>>");

            variableData.TryAddItem(variableName, plotBool.ElementId, variableType);
        }

        stringBuilder.AppendLine("===");

        GeneratedFilesDirectory ??= EditorUtility.OpenFolderPanel("Save Generated Variables", "Choose a location to save the generated file.", Environment.CurrentDirectory);


        var yarnFile = Path.Combine(GeneratedFilesDirectory, "GeneratedVariables.yarn");


        File.WriteAllText(yarnFile, stringBuilder.ToString());
        AssetDatabase.ImportAsset(yarnFile);

        var variablesFile = Path.Combine(GeneratedFilesDirectory, "MEVariableData.asset");
        AssetDatabase.CreateAsset(variableData, variablesFile);

        void PingPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
        }

        PingPath(yarnFile);
        PingPath(variablesFile);

    }

    public static Lazy<PlotDatabase> PlotDatabase = new(() =>
    {
        var plotDatabaseGUID = AssetDatabase.FindAssets("le2").FirstOrDefault()
            ?? throw new FileNotFoundException("Failed to find LE2 plot database file");

        var plotDatabasePath = AssetDatabase.GUIDToAssetPath(plotDatabaseGUID);

        var plotDatabaseJSON = File.ReadAllText(plotDatabasePath);

        try
        {

            var plotDatabase = JsonSerializer.Deserialize<PlotDatabase>(plotDatabaseJSON, serializerOptions);
            return plotDatabase ?? throw new InvalidOperationException($"Failed to parse LE2 plot database file for an unknown reason.");
        }
        catch (JsonException e)
        {
            Debug.LogError($"{nameof(JsonException)} thrown when parsing plot database: line {e.LineNumber + 1}: {e.Message}");
            throw;
        }

    });

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

public class PlotDatabase
{
    [JsonPropertyName("bools")]
    public List<PlotBool> Bools { get; set; } = new();

    [JsonPropertyName("organizational")]
    public List<PlotGroup> Groups { get; set; } = new();

    [JsonPropertyName("ints")]
    public List<PlotInteger> Integers { get; set; } = new();

    public Lazy<IDictionary<int, PlotElement>> AllElements => new(() => Enumerable.ToDictionary(
        Bools.Concat<PlotElement>(Groups).Concat(Integers),
        k => k.ElementId
    ));

    public IEnumerable<PlotElement> GetInheritanceChain(PlotElement element)
    {
        var parentChain = new List<PlotElement>();

        while (element != null)
        {
            parentChain.Insert(0, element);

            AllElements.Value.TryGetValue(element.ParentElementId, out element);
        }
        return parentChain;
    }
}

public abstract class PlotElement
{
    /// <summary>The in-game id for this element, -1 if not applicable</summary>
    [JsonPropertyName("plotid")]
    public int PlotId { get; set; }

    /// <summary>The database id for this element</summary>
    /// <remarks>ElementIds should be unique within a database</remarks>
    [JsonPropertyName("elementid")]
    public int ElementId { get; set; }

    /// <summary>The element id of this element's parent</summary>
    /// <remarks>This property is used during deserialization to determine the hierarchical structure of plot elements.
    [JsonPropertyName("parentelementid")]
    public int ParentElementId { get; set; }

    /// <summary>The name or label associated with this element</summary>
    [JsonPropertyName("label")]
    public string Label { get; set; }

    /// <summary>A value that likely associates this element to something in an internal BioWare system</summary>
    [JsonPropertyName("sequence")]
    public float Sequence { get; set; }

    /// <summary>The type of data this element represents</summary>
    [JsonPropertyName("type")]
    public PlotElementType Type { get; set; }
}

public class PlotGroup : PlotElement { }
public class PlotInteger : PlotElement { }

public class PlotBool : PlotElement
{
    /// <summary>The bool subtype of this element</summary>
    [JsonPropertyName("subtype")]
    public PlotElementType? SubType { get; set; }
}

public enum PlotElementType : int
{
    /// <summary>No plot element type set</summary>
    None = -1,
    /// <summary>A preset region of the game</summary>
    Region = 1,
    /// <summary>A plot or sub-plot</summary>
    Plot = 2,
    /// <summary>A group of flags</summary>
    FlagGroup = 3,
    /// <summary>A flag, represented in-game as a boolean plot variable</summary>
    Flag = 4,
    /// <summary>A linear plot state (true or false), represented in-game as a boolean plot variable</summary>
    State = 5,
    /// <summary>A sub-flag or sub-state (either first or many trigger a state change), represented in-game as a boolean plot variable</summary>
    SubState = 6,
    /// <summary>A boolean expression used to test a game state or set of states, represented in-game as a conditional function</summary>
    Conditional = 7,
    /// <summary>A set of actions to take upon state change, represented in-game as a plot transition</summary>
    Consequence = 8,
    /// <summary>A method to call to set the value of various states</summary>
    Transition = 9,
    /// <summary>The primary goal of an entire quest</summary>
    JournalGoal = 10,
    /// <summary>A single task in a quest</summary>
    JournalTask = 11,
    /// <summary>A plot item</summary>
    JournalItem = 12,
    /// <summary>An integer, represented in-game as an int plot variable</summary>
    Integer = 13,
    /// <summary>A floating point number, represented in-game as a float plot variable</summary>
    Float = 14,
    /// <summary>A category for elements for a single mod or modder</summary>
    Mod = 15,
    /// <summary>A category of various elements in a mod</summary>
    Category = 16
}

public class StringEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
    {
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(StringEnumConverterInner<>).MakeGenericType(type),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
            binder: null,
            args: new[] { options },
            culture: null)!;

        return converter;
    }

    private class StringEnumConverterInner<TEnum> : JsonConverter<TEnum> where TEnum : Enum
    {
        public StringEnumConverterInner(JsonSerializerOptions options)
        {

        }
        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();

            int value;

            if (reader.TokenType == JsonTokenType.Number)
            {
                value = reader.GetInt32();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (!int.TryParse(reader.GetString(), out value))
                {
                    throw new JsonException("Failed to parse string as integer");
                }
            }
            else
            {
                throw new JsonException($"{typeof(TEnum).Name}: Expected to read a number, or a string containing a number, not a {reader.TokenType} (parsing {reader.GetString()})");
            }

            return (TEnum)(object)value;
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            var resultString = ((int)(object)value).ToString();
            writer.WriteStringValue(resultString);
        }
    }
}