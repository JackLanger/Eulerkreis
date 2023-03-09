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

    private Dictionary<Ellipse, (List<TextBlock>,Node<int>)> Nodes { get; } = new();


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
    private readonly Dictionary<Ellipse, Stack<(Line, TextBlock)>> _graphLines = new();
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
            var res = new Graph<int>(pair.Value.Item2).Traverse();
            if (res is null) continue;
            sb.Append($"{++iter}:");
            foreach (var j in res) sb.Append(j+1).Append("->");
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
        if (_isDelete || e.LeftButton == MouseButtonState.Released) return;
        
        var pos = e.GetPosition(_canvas);
        foreach (var nodePair in Nodes)
        {
            // if Node already exist within radius of 15 do not draw new node.
            var margin = nodePair.Key.Margin;
            if (Math.Abs(margin.Left - pos.X) < 15 && Math.Abs(margin.Top - pos.Y) < 15)
                return;
        }

        var (node,tb) = DrawUtil.DrawNode(pos, Brushes.Blue, ref _canvas, index: Nodes.Count + 1);

        // subscribe to events
        setNodeEvents(node);
        // create a new data object for the position
        var data = new Node<int>(Nodes.Count);
        Nodes.Add(node, (tb, data));
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
            Nodes[_clickedNode].Item2.addKante(Nodes[currentNode].Item2, 1);
            Nodes[currentNode].Item2.addKante(Nodes[_clickedNode].Item2, 1);

            if (!_graphLines.ContainsKey(_clickedNode)) _graphLines.Add(_clickedNode, new Stack<(
                Line, TextBlock)>());
            if (!_graphLines.ContainsKey(currentNode)) _graphLines.Add(currentNode, new Stack<(Line,TextBlock)>());

            _graphLines[_clickedNode].Push((line,lbl));
            _graphLines[currentNode].Push((line,lbl));
        }

        _lastClickPosition = null;
        sortCanvas();
    }

    private void DeleteNode(object sender, MouseButtonEventArgs e)
    {
        if (!_isDelete && e.LeftButton == MouseButtonState.Pressed) return;
        
        var node = sender as Ellipse;
        NodeList.Remove(Nodes[node].Item2);
        // todo delete kanten
        
        foreach (var tb in Nodes[node].Item1)
        {
            _canvas.Children.Remove(tb);
        }
        _canvas.Children.Remove(node);
        Nodes.Remove(node);
        _lastClickPosition = null;
        Line line = null;
        try
        {
            while (_graphLines[node].Count > 0)
            {
                var tup = _graphLines[node].Pop();
                _canvas.Children.Remove(tup.Item1);
                _canvas.Children.Remove(tup.Item2);
            }
        }
        catch (Exception ex)
        {
            // do nothing
        }
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