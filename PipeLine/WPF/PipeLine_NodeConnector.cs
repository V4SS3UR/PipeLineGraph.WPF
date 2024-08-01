using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PipeLine
{
    public class PipeLine_NodeConnector : Border, INotifyPropertyChanged
    {   
        public BindingPoint CenterPoint { get; private set; }
        private PipeLine_NodeItem _parentNode;

        private bool _isActive; public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                this.Visibility = IsActive ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }


        public PipeLine_NodeConnector()
        {
            this.Height = 10;
            this.Width = 10;
            this.CornerRadius = new CornerRadius(5);
            this.CenterPoint = new BindingPoint(0, 0);

            IsActive = false;

            this.Loaded += OnLoaded;
            this.LayoutUpdated += OnLayoutUpdated;
        }


        #region Methods

        private PipeLineGrid GetParentGraph()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);

            while (parent != null && !(parent is PipeLineGrid))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is PipeLineGrid)
            {
                return parent as PipeLineGrid;
            }

            return null;
        }

        private PipeLine_NodeItem GetParentNode()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);

            while (parent != null && !(parent is PipeLine_NodeItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as PipeLine_NodeItem;
        }

        private void UpdateCenterPoint()
        {
            var parent = GetParentGraph();
            if (parent == null)
            {
                return;
            }

            //Position relative sur le graph
            Point relativeLocation = this.TransformToAncestor(parent).Transform(new Point(0, 0));

            //Point centrale
            Point center = new Point(relativeLocation.X + this.ActualWidth / 2, relativeLocation.Y + this.ActualHeight / 2);
            CenterPoint.X = center.X;
            CenterPoint.Y = center.Y;
        }

        #endregion Methods

        #region Events

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentNode = GetParentNode();
            if (_parentNode != null)
            {
                this.Background = _parentNode.Brush;
            }

            UpdateCenterPoint();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {          
            UpdateCenterPoint();

            if(_parentNode == null)
            {
                _parentNode = GetParentNode();
                if (_parentNode != null)
                {
                    this.Background = _parentNode.Brush;
                }
            }
        }

        #endregion Events

        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}