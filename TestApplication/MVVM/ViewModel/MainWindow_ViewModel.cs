using PipeLineGraph;
using PipeLineGraph.Core;
using PipeLineGraph.Core.WPF;
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

            TestProcess3();

            AddNodeCommand = new RelayCommand(o => AddNode(Row, Column));
            RemoveNodeCommand = new RelayCommand(o => RemoveNode(o));
            TestProcessCommand = new RelayCommand(o => TestProcess3());
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
        private void TestProcess3()
        {
            Nodes.Clear();

            Node step1Node = Node.Create("Step 1", "Step 1", 1, 0, 50, Brushes.LightGray, NodeState.Default);
            Node step1aNode = Node.Create("Step 1a", "Step 1a", 0, 1, 50, Brushes.LightGray, NodeState.Default);
            Node step1bNode = Node.Create("Step 1b", "Step 1b", 1, 1, 50, Brushes.LightGray, NodeState.Default);
            Node step1cNode = Node.Create("Step 1c", "Step 1c", 2, 1, 50, Brushes.LightGray, NodeState.Default);
            step1Node.AddNextNode(step1aNode);
            step1Node.AddNextNode(step1bNode);
            step1Node.AddNextNode(step1cNode);
            Nodes.Add(step1Node);
            Nodes.Add(step1aNode);
            Nodes.Add(step1bNode);
            Nodes.Add(step1cNode);

            Node step2Node = Node.Create("Step 2", "Step 2", 0, 3, 50, Brushes.LightGray, NodeState.Default);
            Node step2aNode = Node.Create("Step 2a", "Step 2a", 1, 2, 50, Brushes.LightGray, NodeState.Default);
            Node step2bNode = Node.Create("Step 2b", "Step 2b", 1, 3, 50, Brushes.LightGray, NodeState.Default);
            Node step2cNode = Node.Create("Step 2c", "Step 2c", 1, 4, 50, Brushes.LightGray, NodeState.Default);
            Node step2dNode = Node.Create("Step 2d", "Step 2d", 2, 3, 50, Brushes.LightGray, NodeState.Default);
            step2aNode.AddNextNode(step2Node);
            step2bNode.AddNextNode(step2Node);
            step2bNode.AddNextNode(step1Node);
            step2cNode.AddNextNode(step2Node);
            step2Node.AddNextNode(step2aNode);
            step2Node.AddNextNode(step2bNode);
            step2Node.AddNextNode(step2cNode);
            step2aNode.AddNextNode(step2dNode);
            step2bNode.AddNextNode(step2dNode);
            step2cNode.AddNextNode(step2dNode);
            step2dNode.AddNextNode(step2aNode);
            step2dNode.AddNextNode(step2bNode);
            step2dNode.AddNextNode(step2cNode);
            Nodes.Add(step2Node);
            Nodes.Add(step2aNode);
            Nodes.Add(step2bNode);
            Nodes.Add(step2cNode);
            Nodes.Add(step2dNode);

            step1aNode.AddNextNode(step2bNode);
            step1cNode.AddNextNode(step2bNode);

            Node step3Node = Node.Create("Step 3", "Step 3", 0, 6, 50, Brushes.LightGray, NodeState.Default);
            Node step3aNode = Node.Create("Step 3a", "Step 3a", 2, 4, 50, Brushes.LightGray, NodeState.Default);
            Node step3bNode = Node.Create("Step 3b", "Step 3b", 2, 6, 50, Brushes.LightGray, NodeState.Default);
            Node step3cNode = Node.Create("Step 3c", "Step 3c", 2, 8, 50, Brushes.LightGray, NodeState.Default);
            Node step3dNode = Node.Create("Step 3d", "Step 3d", 4, 6, 50, Brushes.LightGray, NodeState.Default);
            step3aNode.AddNextNode(step3Node);
            step3bNode.AddNextNode(step3Node);
            step3cNode.AddNextNode(step3Node);
            step3Node.AddNextNode(step3aNode);
            step3Node.AddNextNode(step3bNode);
            step3Node.AddNextNode(step3cNode);
            step3aNode.AddNextNode(step3dNode);
            step3bNode.AddNextNode(step3dNode);
            step3cNode.AddNextNode(step3dNode);
            step3dNode.AddNextNode(step3aNode);
            step3dNode.AddNextNode(step3bNode);
            step3dNode.AddNextNode(step3cNode);
            Nodes.Add(step3Node);
            Nodes.Add(step3aNode);
            Nodes.Add(step3bNode);
            Nodes.Add(step3cNode);
            Nodes.Add(step3dNode);

            Node step4aNode = Node.Create("Step 4a", "Step 4a", 4, 5, 50, Brushes.LightGray, NodeState.Default);
            step3dNode.AddNextNode(step4aNode);
            Nodes.Add(step4aNode);
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
