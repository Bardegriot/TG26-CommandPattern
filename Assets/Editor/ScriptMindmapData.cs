using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptMindmapData : ScriptableObject
{
    public string targetAssembly = "Assembly-CSharp";
    public string targetInterface = "";
    public List<NoteNodeData> savedNotes = new List<NoteNodeData>();
}

[Serializable]
public class NoteNodeData
{
    public string nodeTitle;
    public Vector2 position;
    public List<string> features = new List<string>();
}