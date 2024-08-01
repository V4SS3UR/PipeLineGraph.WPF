using PipeLine.Core;
using PipeLine.Core.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

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
            set 
            { 
                _row = value; 
                RowChanged?.Invoke(this, value);
                OnPropertyChanged(); 
            }
        }
        private int _column; public int Column
        {
            get { return _column; }
            set 
            { 
                _column = value; 
                ColumnChanged?.Invoke(this, value);
                OnPropertyChanged(); 
            }
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
            Node node = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                node = new Node(name, text, row, column, radius, background, state);
            });

            return node;
        }

        public void AddPreviousNode(Node node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PreviousNodes.Add(node);
                node.NextNodes.Add(this);
            });
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.NextNodes.Add(node);
                node.PreviousNodes.Add(this);
            });
        }
        public void AddNextNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                AddNextNode(node);
            }
        }

        public void RemovePreviousNode(Node node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PreviousNodes.Remove(node);
                node.NextNodes.Remove(this);
            });
        }
        public void RemovePreviousNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                RemovePreviousNode(node);
            }
        }
        public void RemovePreviousNodes()
        {
           foreach (var node in PreviousNodes.ToArray())
            {
                RemovePreviousNode(node);
            }
        }
        public void RemoveNextNode(Node node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.NextNodes.Remove(node);
                node.PreviousNodes.Remove(this);
            });
        }
        public void RemoveNextNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                RemoveNextNode(node);
            }
        }
        public void RemoveNextNodes()
        {
            foreach (var node in NextNodes.ToArray())
            {
                RemoveNextNode(node);
            }
        }

        public void MoveTo(int row, int column)
        {
            Row = row;
            Column = column;
        }
        public void Shift(int rowShift, int columnShift, bool shiftNextNodes = false)
        {
            List<Node> nodeToShift = new List<Node>();

            Action<Node> getNodes = null;
            getNodes = (node) =>
            {
                if (!nodeToShift.Contains(node))
                {
                    nodeToShift.Add(node);

                    if (shiftNextNodes)
                    {
                        foreach (var nextNode in node.NextNodes)
                        {
                            getNodes(nextNode);
                        }
                    }
                }                
            };
            getNodes(this);

            foreach (var nodeToMove in nodeToShift)
            {
                nodeToMove.MoveTo(nodeToMove.Row + rowShift, nodeToMove.Column + columnShift);
            }
        }

        public void SetState(NodeState state)
        {
            State = state;
        }
    }
}