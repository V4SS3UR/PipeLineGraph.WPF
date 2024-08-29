using PipeLineGraph;
using System;
using System.Windows;
using System.Windows.Controls;
using TestApplication.MVVM.ViewModel;

namespace TestApplication.View
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindow_ViewModel viewModel => this.DataContext as MainWindow_ViewModel;

        private void UButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Node node = button.DataContext as Node;
            node.Shift(-1, 0);
        }

        private void DButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Node node = button.DataContext as Node;
            node.Shift(1, 0);
        }

        private void LButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Node node = button.DataContext as Node;
            node.Shift(0, -1);
        }

        private void RButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Node node = button.DataContext as Node;
            node.Shift(0, 1);
        }
    }
}
