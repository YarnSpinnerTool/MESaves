using System.Text.Json.Serialization;

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
    public string Label { get; set; } = "";

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