using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class PlotDatabase
{
    [JsonPropertyName("bools")]
    public List<PlotBool> Bools { get; set; } = new();

    [JsonPropertyName("organizational")]
    public List<PlotGroup> Groups { get; set; } = new();

    [JsonPropertyName("ints")]
    public List<PlotInteger> Integers { get; set; } = new();

    private Dictionary<int, PlotElement>? _allElements;

    private ILookup<int, PlotElement>? _children;

    public Dictionary<int, PlotElement> AllElements
    {
        get
        {
            _allElements ??= Enumerable.ToDictionary(
                Bools.Concat<PlotElement>(Groups).Concat(Integers),
                k => k.ElementId
            );

            return _allElements;
        }
    }

    public IEnumerable<PlotElement> GetInheritanceChain(PlotElement element)
    {
        var parentChain = new List<PlotElement>();

        while (element != null)
        {
            parentChain.Insert(0, element);

            AllElements.TryGetValue(element.ParentElementId, out element);
        }
        return parentChain;
    }

    public IEnumerable<PlotElement> GetChildren(PlotElement element)
    {
        _children ??= AllElements.Values.ToLookup(e => e.ParentElementId);

        return _children[element.ElementId];
    }
}
