using PipeLine.Core;
using PipeLine.Core.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PipeLine
{
    public class Node : ObservableObject
    {
        public event Action<Node, NodeState> StateChanged;
        public event Action<Node, int> RowChanged;
        public event Action<Node, int> ColumnChanged;

        private string _name; public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
        private string _caption; public string Caption
        {
            get { return _caption; }
            set { _caption = value; OnPropertyChanged(); }
        }


        private int _row; public int Row
        {
            get { return _row; }
            set { _row = value; OnPropertyChanged(); }
        }
        private int _column; public int Column
        {
            get { return _column; }
            set { _column = value; OnPropertyChanged(); }
        }


        private double _radius; public double Radius
        {
            get { return _radius; }
            set { _radius = value; OnPropertyChanged(); }
        }

        private Brush _background; public Brush Background
        {
            get { return _background; }
            set { _background = value; OnPropertyChanged(); }
        }

        private NodeState _state; public NodeState State
        {
            get { return _state; }
            set
            {
                _state = value;
                StateChanged?.Invoke(this, value);
                OnPropertyChanged();
            }
        }



        public ObservableCollection<Node> PreviousNodes { get; set; }
        public ObservableCollection<Node> NextNodes { get; set; }



        
        public Node(string name, string text, int row, int column, double radius, Brush background, NodeState state)
        {
            Name = name;
            Caption = text;
            Row = row;
            Column = column;
            Radius = radius;
            Background = background;
            State = state;

            PreviousNodes = new ObservableCollection<Node>();
            NextNodes = new ObservableCollection<Node>();
        }

        public static Node Create(string name, string text, int row, int column, double radius, Brush background, NodeState state)
        {
            return new Node(name, text, row, column, radius, background, state);
        }

        public void AddPreviousNode(Node node)
        {
            this.PreviousNodes.Add(node);
            node.NextNodes.Add(this);
        }
        public void AddPreviousNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                AddPreviousNode(node);
            }
        }
        public void AddNextNode(Node node)
        {
            this.NextNodes.Add(node);
            node.PreviousNodes.Add(this);
        }
        public void AddNextNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                AddNextNode(node);
            }
        }

        public void SetState(NodeState state)
        {
            State = state;
        }
    }
}