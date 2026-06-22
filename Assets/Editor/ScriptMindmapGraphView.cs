using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Reflection;
using System.Collections.Generic;

public class ScriptMindmapGraphView : GraphView
{
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

        PopulateGraphFromCode();

    }

    public void CreateTestNode()
    {
        var node = new Node();
        node.title = "Test Node";
        var letfPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        node.inputContainer.Add(letfPort);
        node.outputContainer.Add(rightPort);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
    }

    public void CreateNewNode()
    {
        var node = new Node();
        var letfPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        node.inputContainer.Add(letfPort);
        node.outputContainer.Add(rightPort);

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
    }

    public void PopulateGraphFromCode()
    {
        Type[] allTypes = Assembly.Load("Assembly-CSharp").GetTypes();
        Dictionary<Type, Node> createdNodes = new Dictionary<Type, Node>();


        Vector2 _positionX = Vector2.zero;
        Vector2 _positionY = Vector2.zero;
        

        int _indexX = 0;
        int _indexY = 0;



        foreach (var type in allTypes)
        {
            Vector2 _position = _positionX + _positionY;

            if(typeof(ICommandInputs).IsAssignableFrom(type))
            {
                Node newNode = CreateNodeFromType(type, _position);
                createdNodes.Add(type, newNode);
                if(_indexX >= _indexY)
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

                    edge.style.unityBackgroundImageTintColor = Color.green;

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

        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach(var method in methods)
        {
            node.extensionContainer.Add(new Label(method.Name));
        }

        var letfPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        var rightPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        node.inputContainer.Add(letfPort);
        node.outputContainer.Add(rightPort);
        //node.outputContainer;

        node.RefreshPorts();
        node.RefreshExpandedState();

        this.AddElement(node);
        return node;

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