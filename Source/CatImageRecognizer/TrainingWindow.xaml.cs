using CatImageRecognizer.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CatImageRecognizer
{
    /// <summary>
    /// Interaction logic for TrainingWindow.xaml
    /// </summary>
    public partial class TrainingWindow : MetroWindow
    {
        public TrainingWindowViewModel TrainingViewModel { get; set; }

        public TrainingWindow(TrainingWindowViewModel trainingViewModel)
        {
            InitializeComponent();
            TrainingViewModel = trainingViewModel;
            this.DataContext = TrainingViewModel;
        }


        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = (this.DataContext as TrainingWindowViewModel);
            if (viewModel.TrainingNeuralNetwork)
            {
                var result = MessageBox.Show("Training In Progress, Want to exit training?", "Training...", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    viewModel.StopTraining();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (viewModel.TestingNeuralNetwork)
            {
                var result = MessageBox.Show("Testing In Progress, Want to exit testing?", "Testing...", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    viewModel.StopTesting();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            if(viewModel.LoadingFilesFromDirectory)
            {
                var result = MessageBox.Show("Loading Files In Progress, Want to exit?", "Loading Files...", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    viewModel.StopLoadingFilesFromDirectory();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
