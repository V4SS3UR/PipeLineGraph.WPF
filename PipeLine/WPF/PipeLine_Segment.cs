using PipeLineGraph.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PipeLineGraph
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
        private BindingPoint _middlePoint2; public BindingPoint MiddlePoint2
        {
            get { return _middlePoint2; }
            set { _middlePoint2 = value; OnPropertyChanged(); }
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
                UpdateVisualState(value);
            }
        }

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

        private Canvas _canvas;
        private Brush _baseBrush;
        private double _baseThickness;
        private int _stepX;
        private int _stepY;



        public PipeLine_Segment(PipeLine_NodeItem inputNode, PipeLine_NodeItem outputNode, Brush brush = default, double thickness = 8) {
            this.InputNodeItem = inputNode;
            this.OutputNodeItem = outputNode;

            this.InputNodeConnector = inputNode.OutputConnector;
            this.OutputNodeConnector = outputNode.InputConnector;

            this.Name = $"{inputNode.BaseNode.Name}_{outputNode.BaseNode.Name}";


            this._baseBrush = brush;
            this._baseThickness = thickness;
            
            this.IsActivated = false; //Trigger change of brush and thickness

            this.InputNodeConnector.IsActive = true;
            this.OutputNodeConnector.IsActive = true;

            //Canvas
            _canvas = new Canvas()
            {
                Background = Brushes.Transparent,
                MaxHeight = 100000,
                MaxWidth = 100000
            };
            this.Child = _canvas;

            this.LayoutUpdated += Segment_LayoutUpdated;
        }



        private void Segment_LayoutUpdated(object sender, EventArgs e)
        {
            var newStepX = OutputNodeItem.BaseNode.Column - InputNodeItem.BaseNode.Column;
            var newStepY = OutputNodeItem.BaseNode.Row - InputNodeItem.BaseNode.Row;

            if (_stepX != newStepX || _stepY != newStepY)
            {
                _stepX = newStepX;
                _stepY = newStepY;

                InitializePath();
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



        //row;column => Path
        //(stepY; stepX)

        //0;-n => elbow + horizontal line + elbow
        //0;-1 => elbow + horizontal line + elbow
        //0;+0 => nothing
        //0;+1 => horizontal line
        //0;+n => horizontal line

        //-1;-n => vertical bezier
        //-1;-1 => vertical bezier
        //-1;+0 => vertical bezier
        //-1;+1 => horizontal bezier
        //-1;+n => horizontal line + horizontal bezier

        //+1;-n => vertical bezier
        //+1;-1 => vertical bezier
        //+1;+0 => vertical bezier
        //+1;+1 => horizontal bezier
        //+1;+n => horizontal line + horizontal bezier

        //-n;-n => vertical bezier + vertical line
        //-n;-1 => vertical bezier + vertical line
        //-n;+0 => vertical bezier + vertical line
        //-n;+1 => horizontal bezier
        //-n;+n => horizontal line + horizontal bezier

        //+n;-n => vertical bezier + vertical line
        //+n;-1 => vertical bezier + vertical line
        //+n;0 => vertical bezier + vertical line
        //+n;+1 => horizontal bezier
        //+n;+n => horizontal line + horizontal bezier


        private void InitializePath()
        {
            List<Path> paths = new List<Path>();

            this.StartPoint = InputNodeConnector.CenterPoint;
            this.EndPoint = OutputNodeConnector.CenterPoint;

            var elbowOffset = InputNodeItem.BaseNode.Radius / 2 * 1.25;

            if (_stepY == 0 && _stepX < -1)
            {
                // Elbow + horizontal line + elbow
                this.MiddlePoint = new BindingPoint(this.StartPoint.X - elbowOffset, this.StartPoint.Y - elbowOffset);
                this.MiddlePoint2 = new BindingPoint(this.EndPoint.X + elbowOffset, this.StartPoint.Y - elbowOffset);
                paths.Add(DefineCurveSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.MiddlePoint2));
                paths.Add(DefineCurveSegment(this.MiddlePoint2, this.EndPoint));
            }
            else if (_stepY == 0 && _stepX == -1)
            {
                // Elbow + horizontal line + elbow
                this.MiddlePoint = new BindingPoint(this.StartPoint.X - elbowOffset, this.StartPoint.Y - elbowOffset);
                this.MiddlePoint2 = new BindingPoint(this.EndPoint.X + elbowOffset, this.StartPoint.Y - elbowOffset);
                paths.Add(DefineCurveSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.MiddlePoint2));
                paths.Add(DefineCurveSegment(this.MiddlePoint2, this.EndPoint));
            }
            else if(_stepY == 0 && _stepX == 0)
            {
                // Nothing
            }
            else if (_stepY == 0 && _stepX == 1)
            {
                // Horizontal line
                paths.Add(DefineLineSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == 0 && _stepX > 1)
            {
                // Horizontal line
                paths.Add(DefineLineSegment(this.StartPoint, this.EndPoint));
            }

            else if (_stepY == -1 && _stepX < -1)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == -1 && _stepX == -1)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == -1 && _stepX == 0)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == -1 && _stepX == 1)
            {
                // Horizontal bezier
                paths.Add(DefineHorizontalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == -1 && _stepX > 1)
            {
                // Horizontal line + Horizontal bezier
                this.MiddlePoint = new BindingPoint(this.EndPoint.X - CalculateDx(_stepX), this.StartPoint.Y);
                paths.Add(DefineLineSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineHorizontalBezierSegment(this.MiddlePoint, this.EndPoint));
            }

            else if (_stepY == 1 && _stepX < -1)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == 1 && _stepX == -1)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == 1 && _stepX == 0)
            {
                // Vertical bezier
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == 1 && _stepX == 1)
            {
                // Horizontal bezier
                paths.Add(DefineHorizontalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY == 1 && _stepX > 1)
            {
                // Horizontal line + Horizontal bezier
                this.MiddlePoint = new BindingPoint(this.EndPoint.X - CalculateDx(_stepX), this.StartPoint.Y);
                paths.Add(DefineLineSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineHorizontalBezierSegment(this.MiddlePoint, this.EndPoint));
            }

            else if (_stepY < -1 && _stepX < -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y - CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY < -1 && _stepX == -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y - CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY < -1 && _stepX == 0)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y - CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY < -1 && _stepX == 1)
            {
                // Horizontal bezier
                paths.Add(DefineHorizontalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY < -1 && _stepX > 1)
            {
                // Horizontal line + horizontal bezier
                this.MiddlePoint = new BindingPoint(this.EndPoint.X - CalculateDx(_stepX), this.StartPoint.Y);
                paths.Add(DefineLineSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineHorizontalBezierSegment(this.MiddlePoint, this.EndPoint));
            }

            else if (_stepY > 1 && _stepX < -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y + CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY > 1 && _stepX == -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y + CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY > 1 && _stepX == 0)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint = new BindingPoint(this.StartPoint.X, this.EndPoint.Y + CalculateDy(_stepY));
                paths.Add(DefineVerticalBezierSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineLineSegment(this.MiddlePoint, this.EndPoint));
            }
            else if (_stepY > 1 && _stepX == 1)
            {
                //horizontal bezier
                paths.Add(DefineHorizontalBezierSegment(this.StartPoint, this.EndPoint));
            }
            else if (_stepY > 1 && _stepX > 1)
            {
                // Horizontal line + horizontal bezier
                this.MiddlePoint = new BindingPoint(this.EndPoint.X - CalculateDx(_stepX), this.StartPoint.Y);
                paths.Add(DefineLineSegment(this.StartPoint, this.MiddlePoint));
                paths.Add(DefineHorizontalBezierSegment(this.MiddlePoint, this.EndPoint));
            }


            this._canvas.Children.Clear();
            foreach (Path path in paths)
            {
                BindPathProperties(path);
                this._canvas.Children.Add(path);
            }
        }

        private void RefreshPoints()
        {
            var startPoint = InputNodeConnector.CenterPoint;
            var endPoint = OutputNodeConnector.CenterPoint;

            var elbowOffset = InputNodeItem.BaseNode.Radius / 2 * 1.25;

            if (_stepY == 0 && _stepX < -1)
            {
                // Elbow + horizontal line + elbow
                this.MiddlePoint.X = startPoint.X - elbowOffset;
                this.MiddlePoint.Y = startPoint.Y - elbowOffset;
                this.MiddlePoint2.X = endPoint.X + elbowOffset;
                this.MiddlePoint2.Y = startPoint.Y - elbowOffset;
                UpdateLinePoints(startPoint, endPoint);

            }
            else if (_stepY == 0 && _stepX == -1)
            {
                // Elbow + horizontal line + elbow
                this.MiddlePoint.X = startPoint.X - elbowOffset;
                this.MiddlePoint.Y = startPoint.Y - elbowOffset;
                this.MiddlePoint2.X = endPoint.X + elbowOffset;
                this.MiddlePoint2.Y = startPoint.Y - elbowOffset;
                UpdateLinePoints(startPoint, endPoint);
            }
            else if (_stepY == 0 && _stepX == 0)
            {
                // Nothing
            }
            else if (_stepY == 0 && _stepX == 1)
            {
                // Horizontal line
                UpdateLinePoints(startPoint, endPoint);
            }
            else if (_stepY == 0 && _stepX > 1)
            {
                // Horizontal line
                UpdateLinePoints(startPoint, endPoint);
            }
            else if (_stepY == -1 && _stepX < -1)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == -1 && _stepX == -1)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == -1 && _stepX == 0)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == -1 && _stepX == 1)
            {
                // Horizontal bezier
                UpdateHorizontalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == -1 && _stepX > 1)
            {
                // Horizontal line + Horizontal bezier
                this.MiddlePoint.X = endPoint.X -+ CalculateDx(_stepX);
                this.MiddlePoint.Y = startPoint.Y;
                UpdateHorizontalBezierPoints(this.MiddlePoint, endPoint);
            }
            else if (_stepY == 1 && _stepX < -1)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == 1 && _stepX == -1)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == 1 && _stepX == 0)
            {
                // Vertical bezier
                UpdateVerticalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == 1 && _stepX == 1)
            {
                // Horizontal bezier
                UpdateHorizontalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY == 1 && _stepX > 1)
            {
                // Horizontal line + Horizontal bezier
                this.MiddlePoint.X = endPoint.X - CalculateDx(_stepX);
                this.MiddlePoint.Y = startPoint.Y;
                UpdateHorizontalBezierPoints(this.MiddlePoint, endPoint);
            }
            else if (_stepY < -1 && _stepX < -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y - CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY < -1 && _stepX == -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y - CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY < -1 && _stepX == 0)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y - CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY < -1 && _stepX == 1)
            {
                // Horizontal bezier
                UpdateHorizontalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY < -1 && _stepX > 1)
            {
                // Horizontal line + horizontal bezier
                this.MiddlePoint.X = endPoint.X - CalculateDx(_stepX);
                this.MiddlePoint.Y = startPoint.Y;
                UpdateHorizontalBezierPoints(this.MiddlePoint, endPoint);
            }
            else if (_stepY > 1 && _stepX < -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y + CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY > 1 && _stepX == -1)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y + CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY > 1 && _stepX == 0)
            {
                // Vertical bezier + vertical line
                this.MiddlePoint.X = endPoint.X;
                this.MiddlePoint.Y = startPoint.Y + CalculateDy(_stepY);
                UpdateVerticalBezierPoints(startPoint, this.MiddlePoint);
            }
            else if (_stepY > 1 && _stepX == 1)
            {
                // Horizontal bezier
                UpdateHorizontalBezierPoints(startPoint, endPoint);
            }
            else if (_stepY > 1 && _stepX > 1)
            {
                // Horizontal line + horizontal bezier
                this.MiddlePoint.X = endPoint.X - CalculateDx(_stepX);
                this.MiddlePoint.Y = startPoint.Y;
                UpdateHorizontalBezierPoints(this.MiddlePoint, endPoint);
            }
        }






        private double CalculateDx(int stepX)
        {
            double dX_connector = (InputNodeItem.OutputConnector.CenterPoint.X - InputNodeItem.InputConnector.CenterPoint.X) / 2;
            double dX = OutputNodeConnector.CenterPoint.X - InputNodeConnector.CenterPoint.X;
            dX -= (stepX - 1) * dX_connector;
            dX /= stepX;
            return dX;
        }
        private double CalculateDy(int stepY)
        {
            double dY = OutputNodeConnector.CenterPoint.Y - InputNodeConnector.CenterPoint.Y;
            dY -= (stepY - 1);
            dY /= stepY;
            return dY;
        }




        private void UpdateHorizontalBezierPoints(BindingPoint startPoint, BindingPoint endPoint)
        {
            this.StartBezierPoint.X = endPoint.X;
            this.StartBezierPoint.Y = startPoint.Y;
            this.EndBezierPoint.X = startPoint.X;
            this.EndBezierPoint.Y = endPoint.Y;
        }

        private void UpdateVerticalBezierPoints(BindingPoint startPoint, BindingPoint endPoint)
        {            
            this.StartBezierPoint.X = startPoint.X;
            this.StartBezierPoint.Y = endPoint.Y;
            this.EndBezierPoint.X = endPoint.X;
            this.EndBezierPoint.Y = startPoint.Y;
        }
        private void UpdateLinePoints(BindingPoint startPoint, BindingPoint endPoint)
        {
            this.StartPoint.X = startPoint.X;
            this.StartPoint.Y = startPoint.Y;
            this.EndPoint.X = endPoint.X;
            this.EndPoint.Y = endPoint.Y;
        }


        private Path DefineHorizontalBezierSegment(BindingPoint startPoint, BindingPoint endPoint)
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

            return new Path()
            {
                Data = new PathGeometry(pfColl),
                Stretch = System.Windows.Media.Stretch.None,
                Style = null,
            };
        }
        private Path DefineVerticalBezierSegment(BindingPoint startPoint, BindingPoint endPoint)
        {
            this.StartBezierPoint = new BindingPoint(startPoint.X, endPoint.Y);
            this.EndBezierPoint = new BindingPoint(endPoint.X, startPoint.Y);

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

            return new Path()
            {
                Data = new PathGeometry(pfColl),
                Stretch = System.Windows.Media.Stretch.None,
                Style = null,
            };
        }
        private Path DefineCurveSegment(BindingPoint startPoint, BindingPoint endPoint)
        {
            //Define SweepDirection according to the startpoint and endpoint
            var stepX = endPoint.X - startPoint.X;
            var stepY = endPoint.Y - startPoint.Y;

            SweepDirection sweepDirection = stepX > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

            double size = Math.Abs(endPoint.X - startPoint.X);

            ArcSegment arc = new ArcSegment()
            {
                Point = endPoint.Point,
                Size = new Size(size, size),
                IsLargeArc = false,
                IsStroked = true,
                SweepDirection = sweepDirection,
                RotationAngle = 0
            };

            var b = new Binding("Point") { Source = endPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(arc, ArcSegment.PointProperty, b);

            var pColl = new PathSegmentCollection { arc };

            var pFig = new PathFigure(startPoint.Point, pColl, false);

            b = new Binding("Point") { Source = startPoint, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(pFig, PathFigure.StartPointProperty, b);

            var pfColl = new PathFigureCollection { pFig };

            return new Path()
            {
                Data = new PathGeometry(pfColl),
                Stretch = System.Windows.Media.Stretch.None,
                Style = null,
            };
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

            return new Path()
            {
                Data = new PathGeometry(pfColl),
                Stretch = System.Windows.Media.Stretch.None,
                Style = null,
            };
        }



        private void BindPathProperties(Path path)
        {
            //Stroke
            var b = new Binding(nameof(Brush)) { Source = this, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(path, Path.StrokeProperty, b);

            //Thickness
            b = new Binding(nameof(Thickness)) { Source = this, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(path, Path.StrokeThicknessProperty, b);

            this.UseLayoutRounding = true;
            this.SnapsToDevicePixels = true;
        }
        private void UpdateVisualState(bool value)
        {
            var color = ((SolidColorBrush)_baseBrush).Color;
            SolidColorBrush semiTransparentBrush = new SolidColorBrush(color)
            {
                Opacity = 0.4
            };
            semiTransparentBrush.Freeze();

            Brush = value ? _baseBrush : semiTransparentBrush;
            Thickness = value ? _baseThickness : _baseThickness * 0.75;
        }




        //Notify
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}