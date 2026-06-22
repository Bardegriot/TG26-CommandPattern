using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptMindmapData : ScriptableObject
{
    public string targetAssembly = "Assembly-CSharp";
    public string targetInterface = "";
    public List<NoteNodeData> savedNotes = new List<NoteNodeData>();
    public List<GeneratedNodeData> savedGeneratedNodes = new List<GeneratedNodeData>();
    public List<ColorData> savedFeatureColors = new List<ColorData>();
    public List<EdgeLinkData> savedLinks = new List<EdgeLinkData>();
}

[Serializable]
public class NoteNodeData
{
    public string nodeId;
    public string nodeTitle;
    public Vector2 position;
    public List<string> features = new List<string>();
}

[Serializable]
public class GeneratedNodeData
{
    public string typeName;
    public Vector2 position;
}

[Serializable]
public class ColorData
{
    public string typeKeyName;
    public Color colorValue;
}

[Serializable]
public class EdgeLinkData
{
    public string outputNodeId;
    public string outputPortName;
    public string inputNodeId;
    public string inputPortName;
}