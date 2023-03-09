using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EulerGraph.Controller.Commands;
using EulerGraph.graph;
using EulerGraph.service;
using EulerGraph.tools;
using Microsoft.Win32;

namespace EulerGraph.Controller;

public class CanvasController : BaseController
{
    public CanvasController(Canvas canvas)
    {
        _canvas = canvas;
        _nodeFileParser = new NodeFileParser();
        _canvas.MouseDown += AddNode;
    }

    #region properties

    public ObservableCollection<Node<int>> NodeList { get; } = new();

    #endregion

    #region fields

    private Dictionary<Ellipse, Node<int>> Nodes { get; } = new();


    // Ad parameterization to class to enable different Types.

    private Canvas _canvas;
    private Graph<int> _graph;
    private RelayCommand _clearCommand;
    private bool _isDelete;
    private string _path;
    private Point? _lastClickPosition;
    private ICommand _traverseCommand;
    private ICommand _loadFileCommand;

    private Ellipse _clickedNode;
    private readonly Dictionary<Ellipse, Stack<Line>> _graphLines = new();
    private readonly NodeFileParser _nodeFileParser;

    #endregion

    #region getter

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public bool IsDelete
    {
        get => _isDelete;
        set => SetField(ref _isDelete, value);
    }

    #endregion

    #region commands

    public ICommand TraverseCommand => _traverseCommand ??= new RelayCommand(() =>
    {
        var iter = 0;
        var pathSb = new StringBuilder();
        foreach (var pair in Nodes)
        {
            var sb = new StringBuilder();
            // Traverse is not respecting immutability so far we need to create deep copies
            // of the nodes before passing them to the graph.
            var res = new Graph<int>(pair.Value).Traverse();
            if (res is null) continue;
            sb.Append($"{++iter}:");
            foreach (var j in res) sb.Append(j).Append("->");
            pathSb.Append(sb.ToString().Substring(0, sb.Length - 2)).Append("\n");
        }

        Path = pathSb.ToString();
    });

    public ICommand ClearCommand => _clearCommand ??= new RelayCommand(() =>
    {
        Nodes.Clear();
        OnPropertyChanged(nameof(Nodes));
        _canvas.Children.Clear();
    });

    public ICommand LoadFileCommand => _loadFileCommand ??= new RelayCommand(LoadFile);

    private void LoadFile()
    {
        var ofd = new OpenFileDialog();
        ofd.ShowDialog();
        var size = 1024 * 1024;
        byte[] buff;
        if (ofd.FileName != null)
        {
            using var fs = ofd.OpenFile();
            size = (int)fs.Length;
            buff = new byte[size];
            var i = 0;
            var maxRead = 1024 * 1024;
            var sb = new StringBuilder();
            size = size < maxRead ? size : maxRead;
            while (size * i < fs.Length)
            {
                fs.Read(buff, size * i++, size);
                sb.Append(Encoding.Default.GetString(buff));
            }

            var nodes = _nodeFileParser.Parse<int>(sb.ToString());
            if (nodes is not null)
                foreach (var node in nodes)
                {
                    // create the nodes and their respective edges
                }
        }
    }

    #endregion

    #region private methods

    private void AddNode(object sender, MouseButtonEventArgs e)
    {
        if (_isDelete) return;
        var pos = e.GetPosition(_canvas);
        foreach (var nodePair in Nodes)
        {
            // if Node already exist within radius of 15 do not draw new node.
            var margin = nodePair.Key.Margin;
            if (Math.Abs(margin.Left - pos.X) < 15 && Math.Abs(margin.Top - pos.Y) < 15)
                return;
        }

        var node = DrawUtil.DrawNode(pos, Brushes.Blue, ref _canvas, index: Nodes.Count + 1);

        // subscribe to events
        setNodeEvents(node);
        // create a new data object for the position
        var data = new Node<int>(Nodes.Count);
        Nodes.Add(node, data);
        NodeList.Add(data);
    }

    private void setNodeEvents(Ellipse node)
    {
        node.MouseEnter += (o, args) => { (o as Ellipse).Fill = Brushes.Gold; };
        node.MouseLeave += (o, args) => { (o as Ellipse).Fill = Brushes.Blue; };
        node.MouseDown += nodeOnMouseDown;
        node.MouseUp += nodeOnMouseUp;
        node.MouseDown += DeleteNode;
    }

    private void nodeOnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var n = sender as Ellipse;
        // set position for first click todo fix for center
        _lastClickPosition = new Point(n.Margin.Left + 7, n.Margin.Top + 7);
        _clickedNode = n;
    }

    private void nodeOnMouseUp(object sender, MouseEventArgs e)
    {
        if (_lastClickPosition is null) return;

        var current = e.GetPosition(_canvas);

        if (Math.Abs(current.X - _lastClickPosition.Value.X) > 15 ||
            Math.Abs(current.Y - _lastClickPosition.Value.Y) > 15)
        {
            var currentNode = sender as Ellipse;
            var (line, lbl) = DrawUtil.DrawWeightedLine(_lastClickPosition.Value, current, Brushes.Gray, 1,
                ref _canvas);
            Nodes[_clickedNode].addKante(Nodes[currentNode], 1);
            Nodes[currentNode].addKante(Nodes[_clickedNode], 1);

            if (!_graphLines.ContainsKey(_clickedNode)) _graphLines.Add(_clickedNode, new Stack<Line>());
            if (!_graphLines.ContainsKey(currentNode)) _graphLines.Add(currentNode, new Stack<Line>());

            _graphLines[_clickedNode].Push(line);
            _graphLines[currentNode].Push(line);
        }

        _lastClickPosition = null;
        sortCanvas();
    }

    private void DeleteNode(object sender, MouseButtonEventArgs e)
    {
        if (!_isDelete) return;

        var node = sender as Ellipse;
        NodeList.Remove(Nodes[node]);
        // todo delete kanten

        Nodes.Remove(node);
        _canvas.Children.Remove(node);

        Line line = null;
        while (_graphLines[node].Count > 0) _canvas.Children.Remove(_graphLines[node].Pop());
    }

    private void sortCanvas()
    {
        Queue<Ellipse> ellipses = new();
        Queue<TextBlock> labels = new();
        foreach (UIElement canvasChild in _canvas.Children)
        {
            if (canvasChild is Ellipse)
                ellipses.Enqueue(canvasChild as Ellipse);
            if (canvasChild is TextBlock)
                labels.Enqueue(canvasChild as TextBlock);
        }

        foreach (var ellipse in ellipses)
        {
            _canvas.Children.Remove(ellipse);
            _canvas.Children.Add(ellipse);
        }

        foreach (var l in labels)
        {
            _canvas.Children.Remove(l);
            _canvas.Children.Add(l);
        }
    }

    #endregion
}