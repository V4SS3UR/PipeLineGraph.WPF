using PipeLine;
using PipeLine.Core;
using PipeLine.Core.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace TestApplication.MVVM.ViewModel
{
    internal class MainWindow_ViewModel : ObservableObject
    {
        public ObservableCollection<Node> Nodes { get; set; }


        public RelayCommand AddNodeCommand { get; set; }
        public RelayCommand RemoveNodeCommand { get; set; }

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

        public NodeState[] NodeStates { get; set; }


        public MainWindow_ViewModel()
        {
            Nodes = new ObservableCollection<Node>();

            NodeStates = (NodeState[])Enum.GetValues(typeof(NodeState));

            InitNodes();

            AddNodeCommand = new RelayCommand(o => AddNode(Row, Column));
            RemoveNodeCommand = new RelayCommand(o => RemoveNode(o));
        }

        private void InitNodes()
        {
            Node startNode = Node.Create("Start", "Start", 0, 0, 50, Brushes.LightGray, NodeState.Default);

            Node step1Node = Node.Create("Step 1", "Step 1", 0, 1, 50, Brushes.LightGray, NodeState.Default);
            Node step2Node = Node.Create("Step 2", "Step 2", 1, 1, 50, Brushes.LightGray, NodeState.Default);

            Node endNode = Node.Create("End", "End", 0, 2, 50, Brushes.LightGray, NodeState.Default);


            startNode.AddNextNode(step1Node);
            startNode.AddNextNode(step2Node);
            step1Node.AddNextNode(endNode);
            step2Node.AddNextNode(endNode);

            Nodes.Add(startNode);
            Nodes.Add(step1Node);
            Nodes.Add(step2Node);
            Nodes.Add(endNode);

            startNode.SetState(NodeState.Running);
        }

        private void AddNode(int row, int column)
        {
            //If row and column already taken, return
            if (Nodes.Any(n => n.Row == row && n.Column == column))
            {
                return;
            }

            Node newNode = Node.Create("New Node", "New Node", row, column, 50, Brushes.LightGray, NodeState.Default);
            Nodes.Add(newNode);
        }

        private void RemoveNode(object o)
        {
            Node node = o as Node;
            if (node == null)
            {
                return;
            }
            Nodes.Remove(node);
        }
    }
}
