using CatImageRecognizer.NeuralNetworks;
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
    /// Interaction logic for SelectNetworkWindow.xaml
    /// </summary>
    public partial class SelectNetworkWindow : MetroWindow, INotifyPropertyChanged
    {
        public SelectNetworkWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            AccordNetLaunchCommand = new RelayCommand(AccordNetLaunch);
            ConvNetSharpLaunchCommand = new RelayCommand(ConvNetSharpLaunch);
        }

        public ICommand AccordNetLaunchCommand { get; set; }
        public ICommand ConvNetSharpLaunchCommand { get; set; }

        private bool launching;
        public bool Launching
        {
            get
            {
                return launching;
            }
            set
            {
                launching = value;
                RaiseProperty("Launching");
            }
        }
        public void AccordNetLaunch()
        {
            LaunchMainWindow(() => new NeuralNetworks.AccordNetwork());
        }

        public void ConvNetSharpLaunch()
        {
            LaunchMainWindow(() => new NeuralNetworks.ConvNetSharpNetwork());
        }

        private void LaunchMainWindow(Func<INeuralNetwork> getNeuralNetwork)
        {
            Task launchTask = new Task(() =>
            {
                this.Launching = true;
                var neuralNetwork = getNeuralNetwork();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow(new ViewModels.MainWindowViewModel(neuralNetwork));
                    mainWindow.Show();
                });
            });
            launchTask.Start();
            launchTask.ContinueWith((t) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Launching = false;
                    this.Close();
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
