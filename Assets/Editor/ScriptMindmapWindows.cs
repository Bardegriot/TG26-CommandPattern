using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptMindmapWindows : EditorWindow
{
    private ScriptMindmapGraphView graphView;
    private TextField assemblyInput;
    private TextField interfaceInput;

    [MenuItem("Tools/BreakingFrame/ScriptMindmap")]
    public static void ShowWindow()
    {
         GetWindow<ScriptMindmapWindows>("Script Mindmap");
    }    
    
    public void CreateGUI()
    {
        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.paddingTop = 5;
        toolbar.style.paddingBottom = 5;
        toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

        assemblyInput = new TextField("Assembly:")
        {
            value = "Assembly-CSharp"
        };
        assemblyInput.style.width = 220;
        assemblyInput.style.marginLeft = 5;

        interfaceInput = new TextField("Base Interface:")
        {
            value = "ICommandInputs"
        };
        interfaceInput.style.width = 220;
        interfaceInput.style.marginLeft = 5;

        Button generateButton = new Button(() =>
        {
            graphView.PopulateGraphFromCode(assemblyInput.value, interfaceInput.value);
        })
        {
            text = "Generate Mindmap"
        };
        generateButton.style.marginLeft = 5;

        Button addNoteButton = new Button(() =>
        {
            graphView.CreateNoteNode(new Vector2(100, 100));
        })
        {
            text = "Add Custom Note Node"
        };
        addNoteButton.style.marginLeft = 15;

        toolbar.Add(assemblyInput);
        toolbar.Add(interfaceInput);
        toolbar.Add(generateButton);
        toolbar.Add(addNoteButton);

        rootVisualElement.Add(toolbar);

        graphView = new ScriptMindmapGraphView();
        rootVisualElement.Add(graphView);

        graphView.PopulateGraphFromCode(assemblyInput.value, interfaceInput.value);
    }
}