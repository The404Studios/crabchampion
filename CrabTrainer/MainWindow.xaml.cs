using System.Windows;
using CrabTrainer.ViewModels;

namespace CrabTrainer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);

            // Dispose of the view model
            if (DataContext is TrainerViewModel vm)
            {
                vm.Dispose();
            }
        }
    }
}
