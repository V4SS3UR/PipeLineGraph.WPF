using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PipeLine.Core;

namespace PipeLine
{
    public class PipeLine_NodeItem : Border
    {
        private Node baseNode;
        public Node BaseNode => baseNode;

        private Grid layoutGrid; //  °|O|°
        private Border coreBorder;  //  O
        private Grid coreContent = new Grid();

        //PathData
        private string pathData_checkFilled = "M21.05 33.1 35.2 18.95l-2.3-2.25-11.85 11.85-6-6-2.25 2.25ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Z";
        private string pathData_cancelFilled = "m16.5 33.6 7.5-7.5 7.5 7.5 2.1-2.1-7.5-7.5 7.5-7.5-2.1-2.1-7.5 7.5-7.5-7.5-2.1 2.1 7.5 7.5-7.5 7.5ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Z";
        private string pathData_circle = "M24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm0-3q7.1 0 12.05-4.975Q41 31.05 41 24q0-7.1-4.95-12.05Q31.1 7 24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24 41Zm0-17Z";
        private string pathData_circleFilled = "M24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Z";


        public PipeLine_NodeConnector inputConnector { get; set; }
        public PipeLine_NodeConnector outputConnector { get; set; }
        public Brush Brush { get; private set; }

        
        public PipeLine_NodeItem(Node input)
        {
            this.baseNode = input;

            this.Brush = input.Background != null ? input.Background : Brushes.White;
            this.SetValue(Grid.RowProperty, this.baseNode.Row);
            this.SetValue(Grid.ColumnProperty, this.baseNode.Column);
            OnStateChange(this.baseNode.State);

            this.baseNode.RowChanged += BaseNode_RowChanged;
            this.baseNode.ColumnChanged += BaseNode_ColumnChanged;
            this.baseNode.StateChanged += (node, state) => OnStateChange(state);            

            CreateLayout();
        }

        private void BaseNode_RowChanged(Node arg1, int arg2)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.SetValue(Grid.RowProperty, arg2);
            });
        }
        private void BaseNode_ColumnChanged(Node arg1, int arg2)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                this.SetValue(Grid.ColumnProperty, arg2);
            });
        }

        private void CreateLayout()
        {
            //Init the sub grid for Core and Connectors => °|O|°
            layoutGrid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5,0,5,0),
            };
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            //Init Core Border => O
            coreBorder = new Border()
            {
                Height = this.baseNode.Radius,
                Width = this.baseNode.Radius,
                CornerRadius = new CornerRadius(this.baseNode.Radius / 2),
                Background = Brushes.White,
            };

            //Init Connectors
            inputConnector = new PipeLine_NodeConnector();
            outputConnector = new PipeLine_NodeConnector();

            //Add elements to layout grid
            layoutGrid.Children.Add(inputConnector);
            layoutGrid.Children.Add(coreBorder);
            layoutGrid.Children.Add(outputConnector);

            //Set position of elements
            inputConnector.SetValue(Grid.ColumnProperty, 0);
            coreBorder.SetValue(Grid.ColumnProperty, 1);
            outputConnector.SetValue(Grid.ColumnProperty, 2);

            if (this.baseNode.Caption != null)
            {
                //Add Caption, the height of the row is set to Auto and shared
                layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, SharedSizeGroup = "NodeCaption" });

                TextBlock textBlock = new TextBlock()
                {
                    Text = this.baseNode.Caption.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    MaxWidth = coreBorder.Width * 2,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };

                layoutGrid.Children.Add(textBlock);
                textBlock.SetValue(Grid.ColumnProperty, 0);
                textBlock.SetValue(Grid.ColumnSpanProperty, 3);
                textBlock.SetValue(Grid.RowProperty, 1);
            }

            //Add Core content
            coreBorder.Child = coreContent;

            this.Child = layoutGrid;
        }

        private void OnStateChange(NodeState state)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.coreContent.Children.Clear();
            });
            

            string sData = default;
            Brush brush = default;

            switch (state)
            {
                case NodeState.Default:
                    sData = pathData_circleFilled;
                    brush = Brushes.LightGray;
                    break;

                case NodeState.Validate:
                    sData = pathData_checkFilled;
                    brush = Brushes.LightGreen;
                    break;

                case NodeState.Failed:
                    sData = pathData_cancelFilled;
                    brush = Brushes.Red;
                    break;

                case NodeState.Running:
                    sData = pathData_circle;
                    brush = Brushes.LightGray;
                    break;

                case NodeState.Empty:
                    sData = pathData_circleFilled;
                    brush = Brushes.Gray;
                    break;
            }

            

            Application.Current.Dispatcher.Invoke(() =>
            {
                var converter = TypeDescriptor.GetConverter(typeof(Geometry));
                Path path = new Path()
                {
                    Stretch = Stretch.Fill,
                    Data = (Geometry)converter.ConvertFrom(sData),
                    Fill = brush,
                    Stroke = Brushes.DarkSlateGray,
                    StrokeThickness = 0,
                    Opacity = 1,
                    Margin = new Thickness(2),
                };
                coreContent.Children.Add(path);

                if (state == NodeState.Running)
                {
                    Path animatedPath = new Path()
                    {
                        Stretch = Stretch.Fill,
                        Data = (Geometry)converter.ConvertFrom(pathData_circleFilled),
                        Fill = brush,
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new ScaleTransform(0.9, 0.9),
                    };
                    coreContent.Children.Add(animatedPath);

                    DoubleAnimation doubleAnimationX = new DoubleAnimation()
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                        RepeatBehavior = RepeatBehavior.Forever,
                        AutoReverse = true,
                        EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                    };
                    Storyboard.SetTarget(doubleAnimationX, animatedPath);
                    Storyboard.SetTargetProperty(doubleAnimationX, new PropertyPath("RenderTransform.ScaleX"));

                    DoubleAnimation doubleAnimationY = new DoubleAnimation()
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                        RepeatBehavior = RepeatBehavior.Forever,
                        AutoReverse = true,
                        EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut }
                    };
                    Storyboard.SetTarget(doubleAnimationY, animatedPath);
                    Storyboard.SetTargetProperty(doubleAnimationY, new PropertyPath("RenderTransform.ScaleY"));

                    Storyboard storyBorad = new Storyboard();
                    storyBorad.Children.Add(doubleAnimationX);
                    storyBorad.Children.Add(doubleAnimationY);
                    storyBorad.Begin();
                }
            });            
        }
    }
}