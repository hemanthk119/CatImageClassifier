using CatImageRecognizer.Models;
using CatImageRecognizer.NeuralNetworks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CatImageRecognizer.ViewModels
{
    public class DetectorViewModel : INotifyPropertyChanged
    {
        public INeuralNetwork NeuralNetwork { get; set; }
        public Action<string> StatusMessageUpdater;

        public DetectorViewModel(ref INeuralNetwork neuralNetwork, Action<string> statusMessageUpdater)
        {
            NeuralNetwork = neuralNetwork;
            LoadImageCommand = new RelayCommand(OnLoadImageCommand);
            StatusMessageUpdater = statusMessageUpdater;
            NetworkName = neuralNetwork.GetNetworkName();
        }

        public string NetworkName { get; set; }

        private bool detectionInProgress = false;
        public bool DetectionInProgress
        {
            get
            {
                return detectionInProgress;
            }
            set
            {
                detectionInProgress = value;
                RaiseProperty("DetectionInProgress");
            }
        }

        private Image<Bgr, Byte> originalImage = new Image<Bgr, byte>("WelcomeCat.jpg");
        public Image<Bgr, Byte> OriginalImage
        {
            get
            {
                return originalImage;
            }
            set
            {
                originalImage = value;
                RaiseProperty("OriginalImage");
            }
        }

        private Image<Gray, Byte> processedImage;
        public Image<Gray, Byte> ProcessedImage
        {
            get
            {
                return processedImage;
            }
            set
            {
                processedImage = value;
                RaiseProperty("ProcessedImage");
            }
        } 
        
        public ICommand LoadImageCommand { get; set; }

        public void OnLoadImageCommand()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.BMP;*.JPG;*.JPEG,*.PNG";
            fileDialog.ShowDialog();
            if (fileDialog.CheckFileExists && fileDialog.FileName != "")
            {
                OnLoadImageFormFilePath(fileDialog.FileName);
            }
        }

        public CancellationTokenSource detectImageTaskCancellationToken;
        public void OnLoadImageFormFilePath(string filePath)
        {
            detectImageTaskCancellationToken = new CancellationTokenSource();
            detectImageTaskCancellationToken.Token.Register(() =>
            {

            });
            var testingProgressMessage = "Detection in Progress. Please Wait... ";
            this.DetectionInProgress = true;
            StatusMessageUpdater(testingProgressMessage);
            Task task = new Task(() =>
            {
                Image<Bgr, Byte> image = new Image<Bgr, byte>(filePath);
                this.OriginalImage = image.Resize(1000, 1000, Inter.Cubic, false);
                var processedImage = LocalImage.ConvertOriginalImageToGrayScaleAndProcess(this.OriginalImage);
                this.ProcessedImage = processedImage;

                (ImageType imageType, List<System.Drawing.Rectangle> rectangles) = Detector.DetectCatInImageFile(NeuralNetwork, this.OriginalImage, this.ProcessedImage, (progress) => {
                    StatusMessageUpdater(testingProgressMessage + " Progress: " + progress + "%");
                });

                if (imageType == ImageType.CAT)
                {
                    var rectanglesImage = new Image<Bgr, Byte>(OriginalImage.Width, OriginalImage.Height, new Bgr(0, 0, 0));
                    foreach (var rectangle in rectangles)
                    {
                        rectanglesImage.Draw(rectangle, new Bgr(150, 0, 0), 2);
                    }
                    OriginalImage = OriginalImage.AddWeighted(rectanglesImage, 1, 1, 0);
                    var mainRectangle = AnchorBox.GetBoundingReactange(rectangles);
                    OriginalImage.Draw(mainRectangle, new Bgr(0, 200, 0), 5);
                    OriginalImage.Draw("CAT", new System.Drawing.Point(30, 120), FontFace.HersheyPlain, 8f, new Bgr(0, 200, 0), 10);
                }
                else
                {
                    OriginalImage.Draw("NOT CAT", new System.Drawing.Point(30, 120), FontFace.HersheyPlain, 8f, new Bgr(0, 0, 255), 10);
                }

                var temp = this.OriginalImage;
                this.OriginalImage = null;
                this.OriginalImage = temp;
                this.OriginalImage.ROI = System.Drawing.Rectangle.Empty;
            });
            task.Start();
            task.ContinueWith((t) => {
                this.DetectionInProgress = false;
                StatusMessageUpdater("Ready");
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
