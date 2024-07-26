using PipeLine;
using PipeLine.Core;
using PipeLine.Core.WPF;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestApplication.MVVM.ViewModel
{
    internal class MainWindow_ViewModel : ObservableObject
    {
        public ObservableCollection<Node> Nodes { get; set; }


        public RelayCommand AddNodeCommand { get; set; }
        public RelayCommand RemoveNodeCommand { get; set; }
        public RelayCommand TestProcessCommand { get; set; }

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
            TestProcessCommand = new RelayCommand(o => TestProcess2());
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

            startNode.SetState(NodeState.Empty);
            step1Node.SetState(NodeState.Aborted);
            startNode.SetState(NodeState.Running);
        }

        private void TestProcess()
        {
            Nodes.Clear();

            Node startNode = Node.Create("Start", "Start", 0, 0, 50, Brushes.LightGray, NodeState.Empty);
            Node step1Node = Node.Create("Step 1", "Step 1", 0, 1, 50, Brushes.LightGray, NodeState.Default);
            Node step2Node = Node.Create("Step 2", "Step 2", 0, 2, 50, Brushes.LightGray, NodeState.Default);
            Node step2BisNode = Node.Create("Step 2b", "Step 2 bis", 1, 2, 50, Brushes.LightGray, NodeState.Default);
            Node step3Node = Node.Create("Step 3", "Step 3", 0, 3, 50, Brushes.LightGray, NodeState.Default);
            Node endNode = Node.Create("End", "End", 0, 4, 50, Brushes.LightGray, NodeState.Empty);

            startNode.AddNextNode(step1Node);
            step1Node.AddNextNode(step2Node);
            step1Node.AddNextNode(step2BisNode);
            step2Node.AddNextNode(step3Node);
            step2BisNode.AddNextNode(step3Node);
            step3Node.AddNextNode(endNode);

            Nodes.Add(startNode);
            Nodes.Add(step1Node);
            Nodes.Add(step2Node);
            Nodes.Add(step2BisNode);
            Nodes.Add(step3Node);
            Nodes.Add(endNode);

            int threadSleepTime = 1500;

            Task.Run(() =>
            {
                //Step 1 Validation
                step1Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step1Node.SetState(NodeState.Complited);

                //Step 2 Failure
                step2Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step2Node.SetState(NodeState.Failed);

                //Step 2 bis Validation
                step2BisNode.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step2BisNode.SetState(NodeState.Complited);

                //Step 3 Validation
                step3Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step3Node.SetState(NodeState.Complited);
            });
        }
        private void TestProcess2()
        {
            Nodes.Clear();

            Node startNode = Node.Create("Start", "Start", 0, 0, 50, Brushes.LightGray, NodeState.Empty);
            Node step1Node = Node.Create("Step 1", "Step 1", 0, 1, 50, Brushes.LightGray, NodeState.Default);            
            Node endNode = Node.Create("End", "End", 0, 2, 50, Brushes.LightGray, NodeState.Empty);

            startNode.AddNextNode(step1Node);
            step1Node.AddNextNode(endNode);

            Nodes.Add(startNode);
            Nodes.Add(step1Node);
            Nodes.Add(endNode);

            int threadSleepTime = 1500;

            Task.Run(() =>
            {
                //Step 1
                step1Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step1Node.SetState(NodeState.Complited);
                                
                endNode.RemovePreviousNodes();
                endNode.MoveTo(0, 3);

                //Step 2
                Node step2Node = Node.Create("Step 2", "Step 2", 0, 2, 50, Brushes.LightGray, NodeState.Default);
                step1Node.AddNextNode(step2Node);
                step2Node.AddNextNode(endNode);
                AddNode(step2Node);
                step2Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step2Node.SetState(NodeState.Failed);

                //Step 2 bis
                Node step2BisNode = Node.Create("Step 2b", "Step 2 bis", 1, 2, 50, Brushes.LightGray, NodeState.Default);
                step1Node.AddNextNode(step2BisNode);
                step2BisNode.AddNextNode(endNode);
                AddNode(step2BisNode);
                step2BisNode.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step2BisNode.SetState(NodeState.Complited);

                endNode.RemovePreviousNodes();
                endNode.MoveTo(0, 4);

                //Step 3
                Node step3Node = Node.Create("Step 3", "Step 3", 0, 3, 50, Brushes.LightGray, NodeState.Default);
                step2Node.AddNextNode(step3Node);
                step2BisNode.AddNextNode(step3Node);
                step3Node.AddNextNode(endNode);
                AddNode(step3Node);
                step3Node.SetState(NodeState.Running);
                System.Threading.Thread.Sleep(threadSleepTime);
                step3Node.SetState(NodeState.Complited);
            });
        }

        private void AddNode(Node node)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Nodes.Add(node);
            });
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
