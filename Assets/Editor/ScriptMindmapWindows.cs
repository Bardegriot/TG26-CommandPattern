using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

public class ScriptMindmapWindows : EditorWindow
{
    private ScriptMindmapGraphView graphView;
    private PopupField<string> assemblyPopup;
    private PopupField<string> interfacePopup;
    private PopupField<string> presetPopup;
    
    private List<string> assemblies = new List<string>();
    private List<string> interfaces = new List<string>();
    private List<string> presets = new List<string>();
    private Dictionary<string, string> presetPaths = new Dictionary<string, string>();
    private string activePresetPath = "";

    [MenuItem("Tools/BreakingFrame/ScriptMindmap #m")]
    public static void ShowWindow()
    {
         GetWindow<ScriptMindmapWindows>("Script Mindmap");
    }    
    
    public void CreateGUI()
    {
        LoadProjectReflectionData();
        ScanProjectForPresets();

        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.paddingTop = 5; toolbar.style.paddingBottom = 5;
        toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

        assemblyPopup = new PopupField<string>("Assembly:", assemblies, 0);
        assemblyPopup.style.width = 200; assemblyPopup.style.marginLeft = 5;
        assemblyPopup.RegisterValueChangedCallback(evt => UpdateInterfaceOptions(evt.newValue));

        interfacePopup = new PopupField<string>("Filter:", interfaces, 0);
        interfacePopup.style.width = 200; interfacePopup.style.marginLeft = 5;

        UpdateInterfaceOptions(assemblyPopup.value);

        presetPopup = new PopupField<string>("Presets:", presets, 0);
        presetPopup.style.width = 180; presetPopup.style.marginLeft = 10;
        presetPopup.RegisterValueChangedCallback(evt => OnPresetSelected(evt.newValue));

        Button generateButton = new Button(() => {
            if (presetPopup != null) presetPopup.SetValueWithoutNotify("[None]");
            activePresetPath = "";
            graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value);
            graphView.UpdateGraphColors();
        }) { text = "Generate" };
        generateButton.style.marginLeft = 5;

        Button addNoteButton = new Button(() => {
            graphView.CreateNoteNode(new Vector2(100, 100));
        }) { text = "+ Note Node" };
        addNoteButton.style.marginLeft = 10;

        Button saveButton = new Button(SaveMindmapData) { text = "Save Layout" };
        saveButton.style.marginLeft = 10;

        toolbar.Add(assemblyPopup);
        toolbar.Add(interfacePopup);
        toolbar.Add(generateButton);
        toolbar.Add(presetPopup);
        toolbar.Add(addNoteButton);
        toolbar.Add(saveButton);

        rootVisualElement.Add(toolbar);

        graphView = new ScriptMindmapGraphView();
        rootVisualElement.Add(graphView);

        graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value);
        graphView.UpdateGraphColors();
    }

    private void LoadProjectReflectionData()
    {
        assemblies.Clear();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = assembly.GetName().Name;
            if (name.Contains("Assembly-CSharp") || name.Equals("Unity.InternalAPIEngineBridgeDev.001"))
            {
                if (!assemblies.Contains(name)) assemblies.Add(name);
            }
        }
        if (assemblies.Count == 0) assemblies.Add("Assembly-CSharp");
    }

    private void ScanProjectForPresets()
    {
        presets.Clear(); presetPaths.Clear();
        presets.Add("[None]");

        string[] guids = AssetDatabase.FindAssets("t:ScriptMindmapData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileNameWithoutExtension(path);
            if (!presets.Contains(filename))
            {
                presets.Add(filename);
                presetPaths[filename] = path;
            }
        }
    }

    private void UpdateInterfaceOptions(string assemblyName)
    {
        interfaces.Clear(); 
        interfaces.Add("[All Features / Classes]");

        try
        {
            Assembly assembly = Assembly.Load(assemblyName);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsInterface && !interfaces.Contains(type.Name)) interfaces.Add(type.Name);
            }
        }
        catch { }

        if (interfacePopup != null)
        {
            interfacePopup.choices = interfaces;
            interfacePopup.value = interfaces[0];
        }
    }

    private void OnPresetSelected(string presetName)
    {
        if (presetName == "[None]" || !presetPaths.TryGetValue(presetName, out string path))
        {
            activePresetPath = ""; return;
        }

        activePresetPath = path;
        ScriptMindmapData data = AssetDatabase.LoadAssetAtPath<ScriptMindmapData>(path);
        if (data == null) return;

        if (assemblies.Contains(data.targetAssembly)) assemblyPopup.value = data.targetAssembly;
        UpdateInterfaceOptions(assemblyPopup.value);
        if (interfaces.Contains(data.targetInterface)) interfacePopup.value = data.targetInterface;

        graphView.DeleteElements(graphView.graphElements);
        graphView.featureColors.Clear();

        foreach (var colData in data.savedFeatureColors)
        {
            Type t = Type.GetType(colData.typeKeyName);
            if (t != null) graphView.featureColors[t] = colData.colorValue;
        }

        graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value, data.savedGeneratedNodes);

        foreach (var savedNote in data.savedNotes)
        {
            graphView.CreateNoteNode(savedNote.position, savedNote.nodeId, savedNote.nodeTitle, savedNote.features);
        }

        graphView.graphElements.ForEach(element =>
        {
            if (element is Node node)
            {
                node.inputContainer.Query<Port>().ForEach(p => p.DisconnectAll());
                node.outputContainer.Query<Port>().ForEach(p => p.DisconnectAll());
            }
        });

        foreach (var link in data.savedLinks)
        {
            Port outputPort = null; Port inputPort = null;
            graphView.graphElements.ForEach(el =>
            {
                if (el is Node node)
                {
                    if (node.viewDataKey == link.outputNodeId) outputPort = node.outputContainer.Q<Port>(link.outputPortName);
                    if (node.viewDataKey == link.inputNodeId) inputPort = node.inputContainer.Q<Port>(link.inputPortName);
                }
            });

            if (outputPort != null && inputPort != null)
            {
                Edge edge = new Edge { output = outputPort, input = inputPort };
                edge.input.Connect(edge); edge.output.Connect(edge);
                graphView.AddElement(edge);
            }
        }

        graphView.UpdateGraphColors();
    }

    private void SaveMindmapData()
    {
        string path = activePresetPath;
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.SaveFilePanelInProject("Save Mindmap Layout", "NewMindmapLayout", "asset", "Save Layout");
            if (string.IsNullOrEmpty(path)) return;
        }

        ScriptMindmapData data = ScriptableObject.CreateInstance<ScriptMindmapData>();
        data.targetAssembly = assemblyPopup.value;
        data.targetInterface = interfacePopup.value;

        foreach (var pair in graphView.featureColors)
        {
            if (pair.Key != null)
            {
                data.savedFeatureColors.Add(new ColorData { typeKeyName = pair.Key.AssemblyQualifiedName, colorValue = pair.Value });
            }
        }

        graphView.graphElements.ForEach(element =>
        {
            if (element is Node node)
            {
                if (node.viewDataKey.Contains("-"))
                {
                    NoteNodeData noteData = new NoteNodeData();
                    noteData.nodeId = node.viewDataKey;
                    TextField titleField = node.titleContainer.Q<TextField>();
                    noteData.nodeTitle = titleField != null ? titleField.value : "Note";
                    noteData.position = node.GetPosition().position;

                    node.extensionContainer.Query<TextField>().ForEach(txt => { noteData.features.Add(txt.value); });
                    data.savedNotes.Add(noteData);
                }
                else if (!string.IsNullOrEmpty(node.viewDataKey))
                {
                    GeneratedNodeData genNode = new GeneratedNodeData
                    {
                        typeName = node.viewDataKey, position = node.GetPosition().position
                    };
                    data.savedGeneratedNodes.Add(genNode);
                }
            }
            if (element is Edge edge && edge.output != null && edge.input != null)
            {
                Node outNode = edge.output.node; Node inNode = edge.input.node;
                if (outNode != null && inNode != null)
                {
                    data.savedLinks.Add(new EdgeLinkData
                    {
                        outputNodeId = outNode.viewDataKey, outputPortName = edge.output.portName,
                        inputNodeId = inNode.viewDataKey, inputPortName = edge.input.portName
                    });
                }
            }
        });

        if (!string.IsNullOrEmpty(activePresetPath))
        {
            ScriptMindmapData existingData = AssetDatabase.LoadAssetAtPath<ScriptMindmapData>(path);
            if (existingData != null)
            {
                EditorUtility.CopySerialized(data, existingData);
                AssetDatabase.SaveAssets();
            }
        }
        else
        {
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            activePresetPath = path;
        }

        ScanProjectForPresets();
        if (presetPopup != null)
        {
            presetPopup.choices = presets;
            string filename = Path.GetFileNameWithoutExtension(path);
            presetPopup.SetValueWithoutNotify(filename);
        }
    }
}