using CatImageRecognizer.ViewModels;
using LiveCharts;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : MetroWindow
    {
        public TrainingWindowViewModel TrainingWindowViewModel { get; set; }
        public GraphWindow(TrainingWindowViewModel trainingWindowViewModel)
        {
            InitializeComponent();
            TrainingWindowViewModel = trainingWindowViewModel;
            this.DataContext = trainingWindowViewModel;
        }
    }
}
