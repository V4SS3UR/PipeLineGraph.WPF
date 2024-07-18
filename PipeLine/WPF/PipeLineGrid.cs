using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace PipeLine
{
    public class PipeLineGrid : Grid
    {
        public static readonly DependencyProperty NodeColorProperty = DependencyProperty.Register(nameof(NodeColor), typeof(Brush), typeof(PipeLineGrid), new PropertyMetadata(Brushes.LightGray, null));
        public Brush NodeColor
        {
            get { return (Brush)GetValue(NodeColorProperty); }
            set { SetValue(NodeColorProperty, value); }
        }

        public static readonly DependencyProperty NodeRadiusProperty = DependencyProperty.Register(nameof(NodeRadius), typeof(double), typeof(PipeLineGrid), new PropertyMetadata(50.0, null));
        public double NodeRadius
        {
            get { return (double)GetValue(NodeRadiusProperty); }
            set { SetValue(NodeRadiusProperty, value); }
        }
        
        public static readonly DependencyProperty SegmentColorProperty = DependencyProperty.Register(nameof(SegmentColor), typeof(Brush), typeof(PipeLineGrid), new PropertyMetadata(Brushes.LightGray, null));
        public Brush SegmentColor
        {
            get { return (Brush)GetValue(SegmentColorProperty); }
            set { SetValue(SegmentColorProperty, value); }
        }
        public static readonly DependencyProperty SegmentThicknessProperty = DependencyProperty.Register(nameof(SegmentThickness), typeof(double), typeof(PipeLineGrid), new PropertyMetadata(3.0, null));
        public double SegmentThickness
        {
            get { return (double)GetValue(SegmentThicknessProperty); }
            set { SetValue(SegmentThicknessProperty, value); }
        }
        
        public static readonly DependencyProperty NodeItemSourceProperty = DependencyProperty.Register(nameof(NodeItemSource), typeof(IEnumerable<Node>), typeof(PipeLineGrid), new PropertyMetadata(new ObservableCollection<Node>(), new PropertyChangedCallback(OnNodeItemSourceChanged)));
        public IEnumerable<Node> NodeItemSource
        {
            get { return (IEnumerable<Node>)GetValue(NodeItemSourceProperty); }
            set { SetValue(NodeItemSourceProperty, value); }
        }
        private static void OnNodeItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pipeLineGrid = (PipeLineGrid)d;

            //Manage the conversion between the Node and the PipePline_NodeItem
            if (e.OldValue != null)
            {
                //If the old collection is an INotifyCollectionChanged, remove the event handler
                if (e.OldValue is INotifyCollectionChanged oldColl)
                {
                    oldColl.CollectionChanged -= pipeLineGrid.NodeItemSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                //If the new collection is an INotifyCollectionChanged, add the event handler
                if (e.NewValue is INotifyCollectionChanged newColl)
                {
                    newColl.CollectionChanged += pipeLineGrid.NodeItemSource_CollectionChanged;
                }

                //Remove the old nodes from the grid
                foreach (PipeLine_NodeItem nodeItem in pipeLineGrid.pipeLine_NodeItems)
                {
                    pipeLineGrid.pipeLine_NodeItems.Remove(nodeItem);
                }

                //Clear the existing nodeItemsSource and add the new items
                pipeLineGrid.pipeLine_NodeItems.Clear();
                foreach (Node node in e.NewValue as IEnumerable<Node>)
                {
                    pipeLineGrid.pipeLine_NodeItems.Add(new PipeLine_NodeItem(node));
                }
            }

            pipeLineGrid.RefreshGrid();
        }

        //Manage the conversion between the Node and the PipePline_NodeItem
        private void NodeItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Node node in e.NewItems)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var newNodeItem = new PipeLine_NodeItem(node);
                            pipeLine_NodeItems.Add(newNodeItem);
                        });
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Node node in e.OldItems)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var pipeLine_NodeItem = pipeLine_NodeItems.FirstOrDefault(o => o.BaseNode.Name == node.Name);
                            pipeLine_NodeItems.Remove(pipeLine_NodeItem);
                        });
                    }
                    break;
            }

            RefreshGrid();
        }



        private ObservableCollection<PipeLine_NodeItem> pipeLine_NodeItems { get; set; }
        private ObservableCollection<PipeLine_Segment> pipeLine_Segments { get; set; }




        static PipeLineGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PipeLineGrid), new FrameworkPropertyMetadata(typeof(PipeLineGrid)));
        }

        public PipeLineGrid()
        {
            this.SetValue(Grid.IsSharedSizeScopeProperty, true);
            pipeLine_NodeItems = new ObservableCollection<PipeLine_NodeItem>();
            pipeLine_NodeItems.CollectionChanged += PipeLine_NodeItems_CollectionChanged;
            pipeLine_Segments = new ObservableCollection<PipeLine_Segment>();
        }

        private void PipeLine_NodeItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PipeLine_NodeItem nodeItem in e.NewItems)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AddNodeItem(nodeItem);                            
                        });
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (PipeLine_NodeItem nodeItem in e.OldItems)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            RemoveNodeItem(nodeItem);                            
                        });
                    }
                    break;
            }

            RefreshGrid();
        }




        private void RefreshGrid()
        {
            //Refresh rows and columns
            int maxRow = NodeItemSource.Max(o => o.Row);
            this.RowDefinitions.Clear();
            for (int i = 0; i <= maxRow; i++)
            {
                this.RowDefinitions.Add(new RowDefinition());
            }

            int maxColumn = NodeItemSource.Max(o => o.Column);
            this.ColumnDefinitions.Clear();
            for (int i = 0; i <= maxColumn; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //Check for new segments
            foreach (var node in NodeItemSource)
            {
                var nextNodes = node.NextNodes;
                if (nextNodes.Any())
                {
                    foreach (var nextNode in nextNodes)
                    {
                        var segmentName = $"{node.Name}_{nextNode.Name}";
                        if (!pipeLine_Segments.Any(o => o.Name == segmentName))
                        {
                            AddSegment(node.Name, nextNode.Name);
                        }
                    }
                }
            }
            //Check for removed segments
            foreach (var segment in pipeLine_Segments.ToArray())
            {
                var segmentParts = segment.Name.Split('_');
                var inputNodeKey = segmentParts[0];
                var outputNodeKey = segmentParts[1];

                if (!NodeItemSource.Any(o => o.Name == inputNodeKey) || !NodeItemSource.Any(o => o.Name == outputNodeKey))
                {
                    RemoveSegment(inputNodeKey, outputNodeKey);
                }
            }                
        }



        private void AddSegment(string inputNodeKey, string outputNodeKey)
        {
            PipeLine_NodeItem inputNode = pipeLine_NodeItems.FirstOrDefault(o => o.BaseNode.Name == inputNodeKey);
            PipeLine_NodeItem outputNode = pipeLine_NodeItems.FirstOrDefault(o => o.BaseNode.Name == outputNodeKey);

            if(inputNode == null || outputNode == null)
            {
                return;
            }

            PipeLine_Segment newSegment = new PipeLine_Segment(inputNode, outputNode, SegmentColor, SegmentThickness);
            this.Children.Add(newSegment);

            pipeLine_Segments.Add(newSegment);

            Debug.WriteLine("Added " + newSegment.Name);
        }
        private void RemoveSegment(string inputNodeKey, string outputNodeKey)
        {
            PipeLine_Segment Segment = pipeLine_Segments.FirstOrDefault(o => o.Name == $"{inputNodeKey}_{outputNodeKey}");
            if (Segment != null)
            {
                pipeLine_Segments.Remove(Segment);
                this.Children.Remove(Segment);
                DestroyVisualTree(Segment);

                Debug.WriteLine("Removed " + Segment.Name);
            }
        }
        private void AddNodeItem(PipeLine_NodeItem nodeItem)
        {
            this.Children.Add(nodeItem);

            Debug.WriteLine("Added " + nodeItem.BaseNode.Name);
        }
        private void RemoveNodeItem(PipeLine_NodeItem nodeItem)
        {
            DestroyVisualTree(nodeItem);
            this.Children.Remove(nodeItem);

            Debug.WriteLine("Removed " + nodeItem.BaseNode.Name);

        }

        private void DestroyVisualTree(Visual visual)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(visual);
            for (int i = 0; i < childrenCount; i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                DestroyVisualTree(child);
            }

            if (visual is IDisposable disposableVisual)
            {
                disposableVisual.Dispose();
            }
        }
    }
}