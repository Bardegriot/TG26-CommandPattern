using UnityEditor;

public class ScriptMindmapWindows : EditorWindow
{
    [MenuItem("Tools/BreakingFrame/ScriptMindmap")]
    public static void ShowWindow()
    {
         GetWindow<ScriptMindmapWindows>("Script Mindmap");
    }    
    
    public void CreateGUI()
    {
        ScriptMindmapGraphView graphView = new ScriptMindmapGraphView();
        rootVisualElement.Add(graphView);
    }

}
