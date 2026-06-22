using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Reflection;
using System.Collections.Generic;

public class ScriptMindmapWindows : EditorWindow
{
    private ScriptMindmapGraphView graphView;
    private PopupField<string> assemblyPopup;
    private PopupField<string> interfacePopup;
    private List<string> assemblies = new List<string>();
    private List<string> interfaces = new List<string>();

    [MenuItem("Tools/BreakingFrame/ScriptMindmap")]
    public static void ShowWindow()
    {
         GetWindow<ScriptMindmapWindows>("Script Mindmap");
    }    
    
    public void CreateGUI()
    {
        LoadProjectReflectionData();

        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.paddingTop = 5;
        toolbar.style.paddingBottom = 5;
        toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

        assemblyPopup = new PopupField<string>("Assembly:", assemblies, 0);
        assemblyPopup.style.width = 250;
        assemblyPopup.style.marginLeft = 5;
        assemblyPopup.RegisterValueChangedCallback(evt => UpdateInterfaceOptions(evt.newValue));

        interfacePopup = new PopupField<string>("Interface:", interfaces, 0);
        interfacePopup.style.width = 250;
        interfacePopup.style.marginLeft = 5;

        UpdateInterfaceOptions(assemblyPopup.value);

        Button generateButton = new Button(() => {
            graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value);
        }) { text = "Generate Mindmap" };
        generateButton.style.marginLeft = 5;

        Button addNoteButton = new Button(() => {
            graphView.CreateNoteNode(new Vector2(100, 100));
        }) { text = "Add Note Node" };
        addNoteButton.style.marginLeft = 10;

        Button saveButton = new Button(SaveMindmapData) { text = "Save Layout" };
        saveButton.style.marginLeft = 10;

        Button loadButton = new Button(LoadMindmapData) { text = "Load Layout" };
        loadButton.style.marginLeft = 5;

        toolbar.Add(assemblyPopup);
        toolbar.Add(interfacePopup);
        toolbar.Add(generateButton);
        toolbar.Add(addNoteButton);
        toolbar.Add(saveButton);
        toolbar.Add(loadButton);

        rootVisualElement.Add(toolbar);

        graphView = new ScriptMindmapGraphView();
        rootVisualElement.Add(graphView);

        graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value);
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

    private void UpdateInterfaceOptions(string assemblyName)
    {
        interfaces.Clear();
        interfaces.Add(""); 

        try
        {
            Assembly assembly = Assembly.Load(assemblyName);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsInterface && !interfaces.Contains(type.Name))
                {
                    interfaces.Add(type.Name);
                }
            }
        }
        catch { }

        if (interfacePopup != null)
        {
            interfacePopup.choices = interfaces;
            interfacePopup.value = interfaces[0];
        }
    }

    private void SaveMindmapData()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Mindmap Layout", "NewMindmapLayout", "asset", "Save Layout");
        if (string.IsNullOrEmpty(path)) return;

        ScriptMindmapData data = ScriptableObject.CreateInstance<ScriptMindmapData>();
        data.targetAssembly = assemblyPopup.value;
        data.targetInterface = interfacePopup.value;

        graphView.graphElements.ForEach(element =>
        {
            if (element is Node node && node.viewDataKey == "CustomNoteNode")
            {
                NoteNodeData noteData = new NoteNodeData();
                TextField titleField = node.titleContainer.Q<TextField>();
                noteData.nodeTitle = titleField != null ? titleField.value : "Note";
                noteData.position = node.GetPosition().position;

                node.extensionContainer.Query<TextField>().ForEach(txt =>
                {
                    noteData.features.Add(txt.value);
                });

                data.savedNotes.Add(noteData);
            }
        });

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
    }

    private void LoadMindmapData()
    {
        string path = EditorUtility.OpenFilePanel("Load Mindmap Layout", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = "Assets" + path.Substring(Application.dataPath.Length);
        ScriptMindmapData data = AssetDatabase.LoadAssetAtPath<ScriptMindmapData>(path);
        if (data == null) return;

        if (assemblies.Contains(data.targetAssembly)) assemblyPopup.value = data.targetAssembly;
        UpdateInterfaceOptions(assemblyPopup.value);
        if (interfaces.Contains(data.targetInterface)) interfacePopup.value = data.targetInterface;

        graphView.DeleteElements(graphView.graphElements);

        graphView.PopulateGraphFromCode(assemblyPopup.value, interfacePopup.value);

        foreach (var savedNote in data.savedNotes)
        {
            graphView.CreateNoteNode(savedNote.position, savedNote.nodeTitle, savedNote.features);
        }
    }
}