using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MEVariableData : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct VariableInfo
    {
        public string variableName;
        public int elementID;
        public VariableType type;
    }

    public enum VariableType
    {
        Bool,
        Int
    }

    [SerializeField] List<VariableInfo> variables = new();

    private Dictionary<string, int> BoolIDs = new();
    private Dictionary<string, int> IntIDs = new();

    public bool TryAddItem(string variableName, int elementID, VariableType type)
    {
        Dictionary<string, int> target = type switch
        {
            VariableType.Bool => this.BoolIDs,
            VariableType.Int => this.IntIDs,
            _ => throw new System.InvalidOperationException("Unknown variable type " + type)
        };

        return target.TryAdd(variableName, elementID);
    }

    public void OnBeforeSerialize()
    {
        this.variables.Clear();

        this.variables.AddRange(this.BoolIDs.Select(kv => new VariableInfo
        {
            variableName = kv.Key,
            elementID = kv.Value,
            type = VariableType.Bool
        }));
        this.variables.AddRange(this.IntIDs.Select(kv => new VariableInfo
        {
            variableName = kv.Key,
            elementID = kv.Value,
            type = VariableType.Int
        }));
    }

    public void OnAfterDeserialize()
    {
        this.BoolIDs.Clear();
        this.IntIDs.Clear();
        foreach (var info in this.variables)
        {
            Dictionary<string, int> target = info.type switch
            {
                VariableType.Bool => this.BoolIDs,
                VariableType.Int => this.IntIDs,
                _ => throw new System.InvalidOperationException("Unknown variable type " + info.type)
            };

            target.Add(info.variableName, info.elementID);
        }
    }
}