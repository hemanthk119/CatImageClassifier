using CatImageRecognizer.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CatImageRecognizer.UserControls
{
    /// <summary>
    /// Interaction logic for DetectorArea.xaml
    /// </summary>
    public partial class DetectorArea : UserControl
    {
        public DetectorViewModel DetectorViewModel { get; set; }
        public DetectorArea()
        {
            InitializeComponent();
           
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Count() > 1)
                {
                    MessageBox.Show("Select only one file");
                    return;
                }
                DetectorViewModel.OnLoadImageFormFilePath(files[0]);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DetectorViewModel = (this.DataContext as DetectorViewModel);
        }
    }
}
