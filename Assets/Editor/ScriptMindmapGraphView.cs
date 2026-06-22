using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using System.Collections.Generic;

public class ScriptMindmapGraphView : GraphView
{
    private Dictionary<Type, Color> featureColors = new Dictionary<Type, Color>();

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

    public void CreateNoteNode(Vector2 position)
    {
        var node = new Node();
        node.SetPosition(new Rect(position.x, position.y, 250, 180));

        Label titleLabel = node.titleContainer.Q<Label>("title-label");
        if (titleLabel != null)
        {
            titleLabel.style.display = DisplayStyle.None;
        }

        TextField titleTextField = new TextField
        {
            value = "Type Feature Group Name"
        };
        titleTextField.style.flexGrow = 1;
        titleTextField.style.marginRight = 5;
        node.titleContainer.Insert(0, titleTextField);

        node.style.borderTopColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderBottomColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderLeftColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderRightColor = new Color(1.0f, 0.65f, 0.0f);
        node.style.borderTopWidth = 2;
        node.style.borderBottomWidth = 2;
        node.style.borderLeftWidth = 2;
        node.style.borderRightWidth = 2;

        VisualElement listContainer = new VisualElement();
        listContainer.style.paddingLeft = 5;
        listContainer.style.paddingRight = 5;
        node.extensionContainer.Add(listContainer);

        Button addFeatureButton = new Button(() =>
        {
            TextField newFeatureField = new TextField
            {
                value = "New custom feature description..."
            };
            newFeatureField.style.whiteSpace = WhiteSpace.Normal;
            newFeatureField.style.marginTop = 2;
            newFeatureField.style.marginBottom = 2;
            listContainer.Add(newFeatureField);
            node.RefreshExpandedState();
        })
        {
            text = "Add Feature Idea"
        };
        
        addFeatureButton.style.marginTop = 5;
        addFeatureButton.style.marginBottom = 5;
        node.extensionContainer.Add(addFeatureButton);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
    }

    public void PopulateGraphFromCode(string assemblyName, string baseInterfaceFilter)
    {
        DeleteElements(graphElements);
        featureColors.Clear();

        Assembly assembly;
        try
        {
            assembly = Assembly.Load(assemblyName);
        }
        catch
        {
            return;
        }

        Type[] allTypes = assembly.GetTypes();
        Dictionary<Type, Node> createdNodes = new Dictionary<Type, Node>();

        Vector2 _positionX = Vector2.zero;
        Vector2 _positionY = Vector2.zero;
        
        int _indexX = 0;
        int _indexY = 0;

        Type filterType = null;
        if (!string.IsNullOrEmpty(baseInterfaceFilter))
        {
            foreach (var t in allTypes)
            {
                if (t.IsInterface && t.Name.Equals(baseInterfaceFilter, StringComparison.OrdinalIgnoreCase))
                {
                    filterType = t;
                    break;
                }
            }
        }

        foreach (var type in allTypes)
        {
            bool isValid = false;
            if (filterType != null)
            {
                isValid = filterType.IsAssignableFrom(type);
            }
            else
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.Name.Contains("Command") || i.Name.Contains("Input"))
                    {
                        isValid = true;
                        break;
                    }
                }
            }

            if (isValid)
            {
                Vector2 _position = _positionX + _positionY;
                Node newNode = CreateNodeFromType(type, _position);
                createdNodes.Add(type, newNode);

                if (_indexX >= _indexY)
                {
                    _indexY++;
                    _positionY += new Vector2(0, 225);
                }
                else
                {
                    _indexX++;
                    _positionY -= new Vector2(0, 225);
                    _positionX += new Vector2(225, 0);
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
                        Edge edge = new Edge
                        {
                            output = outputPort,
                            input = inputPort
                        };

                        edge.input.Connect(edge);
                        edge.output.Connect(edge);

                        if (featureColors.TryGetValue(@interface, out Color assignedColor))
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
        node.expanded = true;

        Color nodeColor = GetColorForFeature(type);

        node.style.borderTopColor = nodeColor;
        node.style.borderBottomColor = nodeColor;
        node.style.borderLeftColor = nodeColor;
        node.style.borderRightColor = nodeColor;
        node.style.borderTopWidth = 2f;
        node.style.borderBottomWidth = 2f;
        node.style.borderLeftWidth = 2f;
        node.style.borderRightWidth = 2f;

        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach(var method in methods)
        {
            node.extensionContainer.Add(new Label(method.Name));
        }

        var letfPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        letfPort.portColor = nodeColor;
        rightPort.portColor = nodeColor;

        node.inputContainer.Add(letfPort);
        node.outputContainer.Add(rightPort);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
        return node;
    }

    private Color GetColorForFeature(Type type)
    {
        Type targetKey = type.IsInterface ? type : null;

        if (targetKey == null)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (i.Name.Contains("Command") || i.Name.Contains("Input"))
                {
                    targetKey = i;
                    break;
                }
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