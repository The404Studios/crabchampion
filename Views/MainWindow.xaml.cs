using System.Windows;
using System.Windows.Controls;
using UnrealSavEditor.ViewModels;

namespace UnrealSavEditor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel && e.NewValue is PropertyTreeItemViewModel selectedItem)
            {
                viewModel.SelectedProperty = selectedItem;
            }
        }
    }
}
