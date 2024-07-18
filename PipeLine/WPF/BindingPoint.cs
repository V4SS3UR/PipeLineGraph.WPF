using PipeLine.Core.WPF;
using System.Windows;

namespace PipeLine
{
    public class BindingPoint : ObservableObject
    {
        public double X
        {
            get { return point.X; }
            set
            {
                point.X = value;
                OnPropertyChanged();
                OnPropertyChanged("Point");
            }
        }
        public double Y
        {
            get { return point.Y; }
            set
            {
                point.Y = value;
                OnPropertyChanged();
                OnPropertyChanged("Point");
            }
        }
        private Point point; public Point Point
        {
            get { return point; }
            set { point = value; OnPropertyChanged(); }
        }


        public BindingPoint(double x, double y)
        {
            point = new Point(x, y);
        }
    }
}