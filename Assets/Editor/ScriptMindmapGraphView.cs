using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Reflection;
using System.Collections.Generic;

public class ScriptMindmapGraphView : GraphView
{
    public Dictionary<Type, Color> featureColors = new Dictionary<Type, Color>();

    public ScriptMindmapGraphView()
    {
        style.flexGrow = 1;
        this.AddManipulator(new ContentDragger());   
        this.AddManipulator(new SelectionDragger());   
        this.AddManipulator(new RectangleSelector()); 

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); 
        
        GridBackground grid = new GridBackground();
        Add(grid);
        grid.SendToBack();
    }

    public Node CreateNoteNode(Vector2 position, string nodeId = "", string defaultTitle = "Type Feature Group Name", List<string> defaultFeatures = null)
    {
        var node = new Node();
        node.SetPosition(new Rect(position.x, position.y, 250, 180));
        node.viewDataKey = string.IsNullOrEmpty(nodeId) ? Guid.NewGuid().ToString() : nodeId;

        Label titleLabel = node.titleContainer.Q<Label>("title-label");
        if (titleLabel != null) titleLabel.style.display = DisplayStyle.None;

        TextField titleTextField = new TextField { value = defaultTitle };
        titleTextField.style.flexGrow = 1;
        titleTextField.style.marginRight = 5;
        node.titleContainer.Insert(0, titleTextField);

        node.style.borderTopColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderBottomColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderLeftColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderRightColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderTopWidth = 3; node.style.borderBottomWidth = 3;
        node.style.borderLeftWidth = 3; node.style.borderRightWidth = 3;

        VisualElement listContainer = new VisualElement();
        listContainer.style.paddingLeft = 5; listContainer.style.paddingRight = 5;
        node.extensionContainer.Add(listContainer);

        System.Action<string> addFeatureField = (textValue) =>
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 2; row.style.marginBottom = 2;

            TextField newFeatureField = new TextField { value = textValue };
            newFeatureField.style.flexGrow = 1;
            newFeatureField.style.whiteSpace = WhiteSpace.Normal;

            Button deleteRowButton = new Button(() => {
                listContainer.Remove(row);
                node.RefreshExpandedState();
            }) { text = "X" };
            deleteRowButton.style.marginLeft = 4;
            deleteRowButton.style.paddingLeft = 4; deleteRowButton.style.paddingRight = 4;

            row.Add(newFeatureField);
            row.Add(deleteRowButton);
            listContainer.Add(row);
            
            node.RefreshExpandedState();
        };

        if (defaultFeatures != null)
        {
            foreach (var feature in defaultFeatures) addFeatureField(feature);
        }

        Button addFeatureButton = new Button(() => addFeatureField("New custom feature description...")) { text = "Add Feature Idea" };
        addFeatureButton.style.marginTop = 5; addFeatureButton.style.marginBottom = 5;
        node.extensionContainer.Add(addFeatureButton);

        var leftPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        leftPort.portName = "In"; rightPort.portName = "Out";
        leftPort.portColor = new Color(1.0f, 0.65f, 0.0f);
        rightPort.portColor = new Color(1.0f, 0.65f, 0.0f);
        node.inputContainer.Add(leftPort);
        node.outputContainer.Add(rightPort);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
        return node;
    }

    public void PopulateGraphFromCode(string assemblyName, string baseInterfaceFilter, List<GeneratedNodeData> presetPositions = null)
    {
        var elementsToDelete = new List<GraphElement>();
        foreach (var element in graphElements)
        {
            if (!element.viewDataKey.Contains("-") && element is Node) elementsToDelete.Add(element);
            if (element is Edge) elementsToDelete.Add(element);
        }
        DeleteElements(elementsToDelete);

        if (string.IsNullOrEmpty(assemblyName)) return;

        Assembly assembly;
        try { assembly = Assembly.Load(assemblyName); } catch { return; }

        Type[] allTypes = assembly.GetTypes();
        Dictionary<Type, Node> createdNodes = new Dictionary<Type, Node>();

        Vector2 _positionX = Vector2.zero; Vector2 _positionY = Vector2.zero;
        int _indexX = 0; int _indexY = 0;

        Type filterType = null;
        bool shouldFilter = !string.IsNullOrEmpty(baseInterfaceFilter) && baseInterfaceFilter != "[All Features / Classes]";
        
        if (shouldFilter)
        {
            foreach (var t in allTypes)
            {
                if (t.IsInterface && t.Name.Equals(baseInterfaceFilter, StringComparison.OrdinalIgnoreCase))
                {
                    filterType = t; break;
                }
            }
        }

        foreach (var type in allTypes)
        {
            if (type.Name.Contains("<") || type.IsNested) continue;

            bool isValid = false;
            if (!shouldFilter)
            {
                isValid = type.IsClass || type.IsInterface || (type.IsValueType && !type.IsEnum);
            }
            else if (filterType != null)
            {
                isValid = filterType.IsAssignableFrom(type);
            }
            else
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.Name.Contains("Command") || i.Name.Contains("Input")) { isValid = true; break; }
                }
            }

            if (isValid)
            {
                Vector2 calculatedPosition = _positionX + _positionY;
                if (presetPositions != null)
                {
                    var foundPreset = presetPositions.Find(p => p.typeName == type.FullName);
                    if (foundPreset != null) calculatedPosition = foundPreset.position;
                }

                Node newNode = CreateNodeFromType(type, calculatedPosition);
                createdNodes.Add(type, newNode);

                if (presetPositions == null)
                {
                    if (_indexX >= _indexY) { _indexY++; _positionY += new Vector2(0, 225); }
                    else { _indexX++; _positionY -= new Vector2(0, 225); _positionX += new Vector2(225, 0); }
                }
            } 
        }

        foreach (var node in createdNodes)
        {
            Type currentType = node.Key;
            Node currentNode = node.Value;
            Type[] interfaces = currentType.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (createdNodes.TryGetValue(@interface, out Node interfaceNode))
                {
                    Port outputPort = interfaceNode.outputContainer.Q<Port>();
                    Port inputPort = currentNode.inputContainer.Q<Port>();

                    if (outputPort != null && inputPort != null)
                    {
                        Edge edge = new Edge { output = outputPort, input = inputPort };
                        edge.input.Connect(edge); edge.output.Connect(edge);

                        Type targetInterface = @interface;
                        if (featureColors.TryGetValue(targetInterface, out Color assignedColor))
                        {
                            edge.style.backgroundColor = assignedColor;
                        }

                        AddElement(edge);
                    }
                }
            }
        }
    }

    private Node CreateNodeFromType(Type type, Vector2 position)
    {
        var node = new Node();
        node.SetPosition(new Rect(position.x, position.y, 0, 0));
        node.title = type.Name;
        node.viewDataKey = type.FullName;
        node.expanded = true;

        Type colorKey = type.IsInterface ? type : null;
        if (colorKey == null)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i.Name.Contains("Command") || i.Name.Contains("Input")) { colorKey = i; break; }
            }
        }
        if (colorKey == null) colorKey = type;

        Color nodeColor = GetColorForFeature(type);

        node.style.borderTopColor = nodeColor; node.style.borderBottomColor = nodeColor;
        node.style.borderLeftColor = nodeColor; node.style.borderRightColor = nodeColor;
        node.style.borderTopWidth = 3f; node.style.borderBottomWidth = 3f;
        node.style.borderLeftWidth = 3f; node.style.borderRightWidth = 3f;

        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach(var method in methods) node.extensionContainer.Add(new Label(method.Name));

        var leftPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        leftPort.portName = "In"; rightPort.portName = "Out";
        leftPort.portColor = nodeColor; rightPort.portColor = nodeColor;

        node.inputContainer.Add(leftPort);
        node.outputContainer.Add(rightPort);

        ColorField colorPicker = new ColorField("Feature Color Options") { value = nodeColor };
        colorPicker.style.marginTop = 8;
        colorPicker.style.marginBottom = 4;
        colorPicker.RegisterValueChangedCallback(evt =>
        {
            featureColors[colorKey] = evt.newValue;
            UpdateGraphColors();
        });
        node.extensionContainer.Add(colorPicker);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
        return node;
    }

    public void UpdateGraphColors()
    {
        graphElements.ForEach(element =>
        {
            if (element is Node node && node.viewDataKey != null && !node.viewDataKey.Contains("-"))
            {
                Type nodeType = GetTypeFromAllAssemblies(node.viewDataKey);
                if (nodeType != null)
                {
                    Color updatedColor = GetColorForFeature(nodeType);
                    node.style.borderTopColor = updatedColor; node.style.borderBottomColor = updatedColor;
                    node.style.borderLeftColor = updatedColor; node.style.borderRightColor = updatedColor;

                    node.inputContainer.Query<Port>().ForEach(p => p.portColor = updatedColor);
                    node.outputContainer.Query<Port>().ForEach(p => p.portColor = updatedColor);

                    ColorField picker = node.extensionContainer.Q<ColorField>();
                    if (picker != null) picker.SetValueWithoutNotify(updatedColor);
                }
            }
            if (element is Edge edge && edge.output != null)
            {
                Node sourceNode = edge.output.node;
                if (sourceNode != null && sourceNode.viewDataKey != null)
                {
                    Type sourceType = GetTypeFromAllAssemblies(sourceNode.viewDataKey);
                    if (sourceType != null) edge.style.backgroundColor = GetColorForFeature(sourceType);
                }
            }
        });
    }

    private Type GetTypeFromAllAssemblies(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = asm.GetType(typeName);
            if (t != null) return t;
        }
        return null;
    }

    private Color GetColorForFeature(Type type)
    {
        Type targetKey = type.IsInterface ? type : null;
        if (targetKey == null)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i.Name.Contains("Command") || i.Name.Contains("Input")) { targetKey = i; break; }
            }
        }
        if (targetKey == null) targetKey = type;

        if (!featureColors.TryGetValue(targetKey, out Color customColor))
        {
            customColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 0.8f, 0.7f, 0.9f);
            featureColors[targetKey] = customColor;
        }
        return customColor;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        foreach (Port port in ports)
        {
            if(startPort.direction == port.direction || startPort.node == port.node ||  startPort.portType != port.portType) continue;
            compatiblePorts.Add(port);
        }
        return compatiblePorts;
    }
}