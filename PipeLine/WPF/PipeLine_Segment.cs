using PipeLine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PipeLine
{
    public class PipeLine_Segment : Border, INotifyPropertyChanged
    {
        //Public Fields
        public PipeLine_NodeItem InputNodeItem { get; set; }
        public PipeLine_NodeItem OutputNodeItem { get; set; }

        public PipeLine_NodeConnector InputNodeConnector { get; set; }
        public PipeLine_NodeConnector OutputNodeConnector { get; set; }


        //Path
        private BindingPoint _startPoint; public BindingPoint StartPoint

        {
            get { return _startPoint; }
            set { _startPoint = value; OnPropertyChanged(); }
        }
        private BindingPoint _middlePoint; public BindingPoint MiddlePoint
        {
            get { return _middlePoint; }
            set { _middlePoint = value; OnPropertyChanged(); }
        }
        private BindingPoint _endPoint; public BindingPoint EndPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; OnPropertyChanged(); }
        }


        //Bezier
        private BindingPoint _startBezierPoint; public BindingPoint StartBezierPoint

        {
            get { return _startBezierPoint; }
            set { _startBezierPoint = value; OnPropertyChanged(); }
        }
        private BindingPoint _endBezierPoint; public BindingPoint EndBezierPoint
        {
            get { return _endBezierPoint; }
            set { _endBezierPoint = value; OnPropertyChanged(); }
        }


        private bool _isActivated; public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                _isActivated = value;

                var color = ((SolidColorBrush)baseBrush).Color;
                SolidColorBrush semiTransparentBrush = new SolidColorBrush(color)
                {
                    Opacity = 0.4
                };
                semiTransparentBrush.Freeze();

                Brush = value ? baseBrush : semiTransparentBrush;
                Thickness = value ? baseThickness : baseThickness * 0.75;
            }
        }

        private Brush baseBrush;

        private double baseThickness;

        private Brush _brush; public Brush Brush
        {
            get { return _brush; }
            set { _brush = value; OnPropertyChanged(); }
        }
        private double _thickness; public double Thickness
        {
            get { return _thickness; }
            set { _thickness = value; OnPropertyChanged(); }
        }

        public string Name { get; set; }

        private Canvas canvas { get; set; }



        public PipeLine_Segment(
            PipeLine_NodeItem inputNode, 
            PipeLine_NodeItem outputNode, 
            Brush brush = default, 
            double thickness = 8)
        {
            this.InputNodeItem = inputNode;
            this.OutputNodeItem = outputNode;

            this.InputNodeConnector = inputNode.outputConnector;
            this.OutputNodeConnector = outputNode.inputConnector;

            this.Name = $"{inputNode.BaseNode.Name}_{outputNode.BaseNode.Name}";
            

            this.baseBrush = brush;
            this.baseThickness = thickness;

            //Trigger change of brush and thickness
            this.IsActivated = false;

            this.InputNodeConnector.IsActive = true;
            this.OutputNodeConnector.IsActive = true;

            //Canvas
            canvas = new Canvas() { Background = Brushes.Transparent };
            this.Child = canvas;

            this.LayoutUpdated += Segment_LayoutUpdated;
        }

        //Private Methods
        private void Segment_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.canvas.Children.Count == 0)
            {
                Init();
            }
            else
            {
                RefreshPoints();
            }

            var activated = this.InputNodeItem.BaseNode.State != NodeState.Default &&
                            this.InputNodeItem.BaseNode.State != NodeState.Failed &&
                            this.InputNodeItem.BaseNode.State != NodeState.Running &&
                            this.OutputNodeItem.BaseNode.State != NodeState.Default;

            if (this.IsActivated != activated)
            {
                this.IsActivated = activated;
            }
        }

        private void RefreshPoints()
        {
            int stepX = OutputNodeItem.BaseNode.Column - InputNodeItem.BaseNode.Column;
            int stepY = OutputNodeItem.BaseNode.Row - InputNodeItem.BaseNode.Row;

            if (stepY == 0)
            {
                //Line Only
            }
            else
            {
                if (stepX == 1)
                {
                    //Bezier Only
                    this.StartBezierPoint.X = EndPoint.X;
                    this.StartBezierPoint.Y = StartPoint.Y;
                    this.EndBezierPoint.X = StartPoint.X;
                    this.EndBezierPoint.Y = EndPoint.Y;
                }
                else
                {
                    //Bezier + Line
                    double dX_connector = InputNodeItem.outputConnector.GetCenterPoint().X - InputNodeItem.inputConnector.GetCenterPoint().X;
                    double dX = OutputNodeConnector.GetCenterPoint().X - InputNodeConnector.GetCenterPoint().X;
                    dX -= (stepX - 1) * dX_connector;
                    dX /= stepX;

                    if (stepY > 0) //Descente, input collée
                    {
                        //Bezier
                        this.MiddlePoint.X = StartPoint.X + dX;
                        this.MiddlePoint.Y = EndPoint.Y;
                        this.StartBezierPoint.X = MiddlePoint.X;
                        this.StartBezierPoint.Y = StartPoint.Y;
                        this.EndBezierPoint.X = StartPoint.X;
                        this.EndBezierPoint.Y = MiddlePoint.Y;
                    }
                    else //Montée; output collée
                    {
                        //Bezier
                        this.MiddlePoint.X = EndPoint.X - dX;
                        this.MiddlePoint.Y = StartPoint.Y;
                        this.StartBezierPoint.X = EndPoint.X;
                        this.StartBezierPoint.Y = StartPoint.Y;
                        this.EndBezierPoint.X = MiddlePoint.X;
                        this.EndBezierPoint.Y = EndPoint.Y;
                    }
                }
            }
        }

        private void Init()
        {
            List<Path> paths = new List<Path>();

            int stepX = OutputNodeItem.BaseNode.Column - InputNodeItem.BaseNode.Column;
            int stepY = OutputNodeItem.BaseNode.Row - InputNodeItem.BaseNode.Row;

            if (stepY == 0)
            {
                //Line Only
                this.StartPoint = InputNodeConnector.GetCenterPoint();
                this.EndPoint = OutputNodeConnector.GetCenterPoint();
                paths.Add(DefineLineSegment(this.StartPoint, this.EndPoint));
            }
            else
            {
                if (stepX == 1)
                {
                    //Bezier Only
                    this.StartPoint = InputNodeConnector.GetCenterPoint();
                    this.EndPoint = OutputNodeConnector.GetCenterPoint();
                    paths.Add(DefineBezierSegment(this.StartPoint, this.EndPoint));
                }
                else
                {
                    //Bezier + Line
                    double dX_connector = (InputNodeItem.outputConnector.GetCenterPoint().X - InputNodeItem.inputConnector.GetCenterPoint().X) / 2;
                    double dX = OutputNodeConnector.GetCenterPoint().X - InputNodeConnector.GetCenterPoint().X;
                    dX -= (stepX - 1) * dX_connector;
                    dX /= stepX;

                    if (stepY > 0) //Descente, input collée
                    {
                        this.StartPoint = InputNodeConnector.GetCenterPoint();
                        this.EndPoint = OutputNodeConnector.GetCenterPoint();
                        this.MiddlePoint = new BindingPoint(this.StartPoint.X + dX, this.EndPoint.Y);

                        paths.Add(DefineBezierSegment(this.StartPoint, this.MiddlePoint));
                        paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
                    }
                    else //Montée; output collée
                    {
                        this.StartPoint = InputNodeConnector.GetCenterPoint();
                        this.EndPoint = OutputNodeConnector.GetCenterPoint();
                        this.MiddlePoint = new BindingPoint(this.EndPoint.X - dX, this.StartPoint.Y);

                        paths.Add(DefineLineSegment(this.StartPoint, this.MiddlePoint));
                        paths.Add(DefineBezierSegment(this.MiddlePoint, this.EndPoint));
                    }
                }
            }

            foreach (Path path in paths)
            {
                //Stroke
                var b = new Binding(nameof(Brush)) { Source = this, Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(path, Path.StrokeProperty, b);

                //Thickness
                b = new Binding(nameof(Thickness)) { Source = this, Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(path, Path.StrokeThicknessProperty, b);

                this.UseLayoutRounding = true;
                this.SnapsToDevicePixels = true;
                this.canvas.Children.Add(path);
            }
        }

        private Path DefineBezierSegment(BindingPoint startPoint, BindingPoint endPoint)
        {
            this.StartBezierPoint = new BindingPoint(endPoint.X, startPoint.Y);
            this.EndBezierPoint = new BindingPoint(startPoint.X, endPoint.Y);

            BezierSegment spline = new BezierSegment { IsStroked = true };

            var b = new Binding("Point") { Source = StartBezierPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(spline, BezierSegment.Point1Property, b);

            b = new Binding("Point") { Source = EndBezierPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(spline, BezierSegment.Point2Property, b);

            b = new Binding("Point") { Source = endPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(spline, BezierSegment.Point3Property, b);

            var pColl = new PathSegmentCollection { spline };

            var pFig = new PathFigure(startPoint.Point, pColl, false);

            b = new Binding("Point") { Source = startPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(pFig, PathFigure.StartPointProperty, b);

            var pfColl = new PathFigureCollection { pFig };

            return new Path() { Data = new PathGeometry(pfColl) };
        }
        private Path DefineLineSegment(BindingPoint startPoint, BindingPoint endPoint)
        {
            LineSegment line = new LineSegment { IsStroked = true };

            var b = new Binding("Point") { Source = endPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(line, LineSegment.PointProperty, b);

            var pColl = new PathSegmentCollection { line };

            var pFig = new PathFigure(startPoint.Point, pColl, false);

            b = new Binding("Point") { Source = startPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(pFig, PathFigure.StartPointProperty, b);

            var pfColl = new PathFigureCollection { pFig };

            return new Path() { Data = new PathGeometry(pfColl) };
        }



        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}