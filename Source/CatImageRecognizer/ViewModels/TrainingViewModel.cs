using CatImageRecognizer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.IO;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading;
using CatImageRecognizer.NeuralNetworks;
using PagedList;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using LiveCharts.Configurations;

namespace CatImageRecognizer.ViewModels
{
    public class TrainingWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RemoteImage> RemoteImageCollection { get; set; }
        public ObservableCollection<LocalImage> LocalImageCollection { get; set; }
        public ICommand OpenFileForRemoteImageCollection { get; set; }
        public ICommand ReadRemoteListAndGenerateLocalList { get; set; }
        public ICommand RefreshLocalImagesFromDirectory { get; set; }
        public ICommand TrainNeuralNetwork { get; set; }
        public ICommand AddLocalImage { get; set; }
        public ICommand DeleteLocalImageCommad { get; set; }
        public ICommand TestNeuralNetworkWithLocalImages { get; set; }
        public ICommand ShowGraphWindowCommand { get; set; }
        public GraphData GraphData { get; set; }

        public string NetworkName { get; set; }

        private int maxIterationsOnEachPage = 1000;
        public int MaxIterationsOnEachPage
        {
            get
            {
                return maxIterationsOnEachPage;
            }
            set
            {
                maxIterationsOnEachPage = value;
                RaiseProperty("MaxIterationsOnEachPage");
            }
        }

        private double stopPageTrainingErrorRate = 0.01d;
        public double StopPageTrainingErrorRate
        {
            get
            {
                return stopPageTrainingErrorRate;
            }
            set
            {
                stopPageTrainingErrorRate = value;
                RaiseProperty("StopPageTrainingErrorRate");
            }
        }

        private double stopIterationTrainingErrorRate = 1d;
        public double StopIterationTrainingErrorRate
        {
            get
            {
                return stopIterationTrainingErrorRate;
            }
            set
            {
                stopIterationTrainingErrorRate = value;
                RaiseProperty("StopIterationTrainingErrorRate");
            }
        }

        private int maxIterations = 1000;
        public int MaxIterations
        {
            get
            {
                return maxIterations;
            }
            set
            {
                maxIterations = value;
                RaiseProperty("MaxIterations");
            }
        }

        private bool loadingFilesFromDirectory = false;
        public bool LoadingFilesFromDirectory
        {
            get
            {
                return loadingFilesFromDirectory;
            }
            set
            {
                loadingFilesFromDirectory = value;
                RaiseProperty("LoadingFilesFromDirectory");
            }
        }

        private bool generatingLocalImagesFromRemote = false;
        public bool GeneratingLocalImagesFromRemote
        {
            get
            {
                return generatingLocalImagesFromRemote;
            }
            set
            {
                generatingLocalImagesFromRemote = value;
                RaiseProperty("GeneratingLocalImagesFromRemote");
            }
        }

        private bool trainingNeuralNetwork = false;
        public bool TrainingNeuralNetwork
        {
            get
            {
                return trainingNeuralNetwork;
            }
            set
            {
                trainingNeuralNetwork = value;
                RaiseProperty("TrainingNeuralNetwork");
            }
        }

        private bool testingNeuralNetwork = false;
        public bool TestingNeuralNetwork
        {
            get
            {
                return testingNeuralNetwork;
            }
            set
            {
                testingNeuralNetwork = value;
                RaiseProperty("TestingNeuralNetwork");
            }
        }


        private double trainingProgress = 0;
        public double TrainingProgress
        {
            get
            {
                return trainingProgress;
            }
            set
            {
                trainingProgress = value;
                RaiseProperty("TrainingProgress");
            }
        }

        private string statusMessage = "Ready";
        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                statusMessage = value;
                RaiseProperty("StatusMessage");
            }
        }

        public INeuralNetwork NeuralNetwork { get; set; }



        public TrainingWindowViewModel(ref INeuralNetwork neuralNetwork)
        {
            RemoteImageCollection = new ObservableCollection<RemoteImage>();
            LocalImageCollection = new ObservableCollection<LocalImage>();
            OpenFileForRemoteImageCollection = new RelayCommand(OnOpenFileForRemoteImageCollection);
            ReadRemoteListAndGenerateLocalList = new RelayCommand(OnConvertRemoteImagesToLocal);
            RefreshLocalImagesFromDirectory = new RelayCommand(OnRefreshLocalImagesFromDirectory);
            TrainNeuralNetwork = new RelayCommand(OnTrainNeuralNetwork);
            AddLocalImage = new RelayCommand(OnAddLocalImage);
            DeleteLocalImageCommad = new RelayCommand<string>(OnDeleteLocalImage);
            TestNeuralNetworkWithLocalImages = new RelayCommand(OnTestLocalImageCollection);
            ShowGraphWindowCommand = new RelayCommand(OnShowGraphWindow);
            NeuralNetwork = neuralNetwork;
            GraphData = new GraphData();
            NetworkName = $"Train Network - {neuralNetwork.GetNetworkName()} Network";
        }

        public CancellationTokenSource trainingTaskCancellationToken;
        public CancellationTokenSource testingTaskCancellationToken;
        public CancellationTokenSource loadingLocalFilesTaskCancellationToken;

        public void OnShowGraphWindow()
        {
            GraphWindow graphWindow = new GraphWindow(this);
            graphWindow.ShowDialog();
        }

        public void StopTraining()
        {
            if (trainingTaskCancellationToken != null)
                trainingTaskCancellationToken.Cancel();

            this.TrainingNeuralNetwork = false;
            this.StatusMessage = "Ready";
        }

        public void StopTesting()
        {
            if (testingTaskCancellationToken != null)
                testingTaskCancellationToken.Cancel();

            this.TestingNeuralNetwork = false;
            this.StatusMessage = "Ready";
        }

        public void StopLoadingFilesFromDirectory()
        {
            if (loadingLocalFilesTaskCancellationToken != null)
                loadingLocalFilesTaskCancellationToken.Cancel();

            this.LoadingFilesFromDirectory = false;
            this.StatusMessage = "Ready";
        }

        public void OnTestLocalImageCollection()
        {
            if(LocalImageCollection.Count == 0)
            {
                MessageBox.Show("No Images in Local Image List to Test With!", "Add Local Images");
                return;
            }

            testingTaskCancellationToken = new CancellationTokenSource();
            testingTaskCancellationToken.Token.Register(() =>
            {

            });

            //Stop training!
            //if (trainingTaskCancellationToken != null)
            //    trainingTaskCancellationToken.Cancel();

            var testingProgressMessage = "Testing in Progress. Please Wait... ";
            this.TestingNeuralNetwork = false;
            Task task = new Task(() =>
            {
                this.StatusMessage = testingProgressMessage;
                this.TestingNeuralNetwork = true;
                int counter = 0;
                int passedCount = 0;

                foreach(var localImage in LocalImageCollection)
                {
                    localImage.TestFailed = false;
                    localImage.TestPassed = false;
                }

                foreach (var localImage in LocalImageCollection)
                {
                    (ImageType imageType, List<System.Drawing.Rectangle> rectangles) = Detector.DetectCatInImageFile(NeuralNetwork, localImage.OriginalImage, localImage.CompressedImage, (progress)=> {
                    }, false);
                    if (imageType != localImage.ImageType)
                    {
                        localImage.TestFailed = true;
                        localImage.TestPassed = false;
                    }
                    else
                    {
                        localImage.TestFailed = false;
                        localImage.TestPassed = true;
                        passedCount++;
                    }
                    counter++;
                    this.StatusMessage = testingProgressMessage + " Progress: " + counter.ToString() + "/" + LocalImageCollection.Count + " Passed: " + passedCount.ToString() + "/" + LocalImageCollection.Count;
                }
                this.StatusMessage = "Passed: " + passedCount.ToString() + " / " + LocalImageCollection.Count;
            }, testingTaskCancellationToken.Token);
            task.Start();
            task.ContinueWith((t) => {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.TestingNeuralNetwork = false;
                });
            });
        }

        public (double[][], double[][]) GetBatchDataFromImages(IPagedList<LocalImage> localImages)
        {
            var numberOfImages = localImages.Count;
            double[][] batchInputs = new double[numberOfImages][];
            double[][] batchOutputs = new double[numberOfImages][];
            foreach (int i in Enumerable.Range(0, numberOfImages))
            {
                var currentLocalImage = localImages[i];
                (double[] normalizedImageData, ImageType imageType) = LocalImage.GetImageInformationForNeuralNetwork(currentLocalImage);
                batchInputs[i] = normalizedImageData;
                batchOutputs[i] = new double[] { currentLocalImage.ImageType == ImageType.CAT ? 1 : 0, currentLocalImage.ImageType == ImageType.CAT ? 0 : 1 };
            }
            return (batchInputs, batchOutputs);
        }

        private void UpdateParametersFromNeuralNetwork()
        {
            MaxIterationsOnEachPage = this.NeuralNetwork.GetMaxIterationsOnEachPage();
            StopPageTrainingErrorRate = this.NeuralNetwork.GetStopPageTrainingErrorRate();
            StopIterationTrainingErrorRate = this.NeuralNetwork.GetStopIterationTrainingErrorRate();
        }

        public void OnTrainNeuralNetwork()
        {
            if (LocalImageCollection.Count == 0)
            {
                MessageBox.Show("No Images in Local Image List to Train With!", "Add Local Images");
                return;
            }

            this.GraphData.ClearDataCollection();

            trainingTaskCancellationToken = new CancellationTokenSource();
            trainingTaskCancellationToken.Token.Register(() =>
            {

            });
            if (NeuralNetwork == null)
            {
                MessageBox.Show("Neural Network Incompatible", "Error in Neural Network");
                return;
            }
            UpdateParametersFromNeuralNetwork();
            this.TrainingNeuralNetwork = true;
            var trainingProgressMessage = "Training in Progress. Please Wait... ";
            Task task = new Task(() => {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.StatusMessage = trainingProgressMessage;
                    this.TrainingNeuralNetwork = true;
                    foreach (var localImage in LocalImageCollection)
                    {
                        localImage.TrainingInProgress = true;
                        localImage.TestFailed = false;
                        localImage.TestPassed = false;
                    }
                });

                var imagesOnEachPage = NeuralNetwork.GetBatchSubSize();
                var totalPages = (int)Math.Ceiling((double)LocalImageCollection.Count / (double)imagesOnEachPage);

                foreach (var mainIteration in Enumerable.Range(1, MaxIterations))
                {
                    double avrageIterationError = 0;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Randomizer.Shuffle(LocalImageCollection);
                    });

                    foreach (var currentPage in Enumerable.Range(1, totalPages))
                    {
                        double averagePageError = 0;
                        var currentPageImages = LocalImageCollection.ToPagedList(currentPage, imagesOnEachPage);
                        (var batchInputs, var batchOutputs) = GetBatchDataFromImages(currentPageImages);

                        int batchTrainCounter = 0;
                        NeuralNetwork.BatchTrain(batchInputs, batchOutputs, MaxIterationsOnEachPage, (currentError, pageIteration, message) => {
                            this.StatusMessage = $"{trainingProgressMessage} Message: {message}; Iteration: {mainIteration}; Page: {currentPage}/{totalPages}; Page Iteration: {pageIteration}; Error: {currentError}";
                            foreach (var localImage in currentPageImages)
                            {
                                localImage.TrainedIterations = (pageIteration) * (mainIteration);
                                localImage.TrainingInProgress = true;
                            }
                            if(currentError <= StopPageTrainingErrorRate || pageIteration > MaxIterationsOnEachPage)
                            {
                                NeuralNetwork.StopBatchTraining();
                            }
                            GraphData.AddDataPoint(currentError);
                            averagePageError += currentError;
                            batchTrainCounter++;
                        });
                        averagePageError /= (double)batchTrainCounter;

                        GraphData.AddPageChanged();
                        avrageIterationError += averagePageError;
                        GC.Collect();
                    }
                    GraphData.AddIterationChanged();

                    avrageIterationError /= totalPages;
                    if(avrageIterationError < StopIterationTrainingErrorRate)
                    {
                        break;
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    this.StatusMessage = trainingProgressMessage;
                });

            }, trainingTaskCancellationToken.Token);
            task.Start();
            task.ContinueWith((t) => {
                App.Current.Dispatcher.Invoke(() =>
                {
                    this.StatusMessage = "Ready";
                    this.TrainingNeuralNetwork = false;
                    foreach (var localImage in LocalImageCollection)
                    {
                        localImage.TrainingInProgress = false;
                    }
                });
            });
        }

        public void OnDeleteLocalImage(string filePath)
        {
            var result = MessageBox.Show("Sure you want to delete this image?", "Really?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                var toDeleteLocalImage = LocalImageCollection.FirstOrDefault(r => r.LocalPath == filePath);
                File.Delete(filePath);
                LocalImageCollection.Remove(toDeleteLocalImage);
            }
        }

        public void OnAddLocalImage()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.BMP;*.JPG;*.JPEG,*.PNG";
            fileDialog.ShowDialog();
            if (fileDialog.CheckFileExists && fileDialog.FileName != "")
            {
                var result = MessageBox.Show("Was that a cat?", "Iz Cat?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                try
                {
                    byte[] selectImageBytes;
                    using (var selectFileStream = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate))
                    {
                        selectImageBytes = new byte[selectFileStream.Length];
                        selectFileStream.Read(selectImageBytes, 0, (int)selectFileStream.Length);

                    }
                    var localImage = LocalImage.GenerateNewLocalFileFromImageTypeAndData(result == MessageBoxResult.Yes ? ImageType.CAT : ImageType.NOT_CAT, selectImageBytes).Result;
                    LocalImageCollection.Insert(0, localImage);
                }
                catch
                {
                    MessageBox.Show("Could not open the file: " + fileDialog.FileName, "Could not open!");
                }
            }
        }

        public void OnRefreshLocalImagesFromDirectory()
        {
            loadingLocalFilesTaskCancellationToken = new CancellationTokenSource();
            loadingLocalFilesTaskCancellationToken.Token.Register(() =>
            {

            });
            this.LoadingFilesFromDirectory = false;
            Task task = new Task(() =>
            {
                try
                {
                    var standardMessage = "Reading from directory. Please Wait...";
                    this.LoadingFilesFromDirectory = true;
                    this.StatusMessage = standardMessage;
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        LocalImageCollection.Clear();
                    });
                    var localImages = LocalImage.ReadAllLocalImagesFromDirectory((progress) => {
                        this.StatusMessage = standardMessage + " Progress: " + progress + "%";
                    }).OrderBy(r=>Path.GetFileName(r.LocalPath));

                    if(localImages.Count() == 0)
                    {
                        MessageBox.Show("No Images in the directories", "No Images", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var localImage in localImages)
                        {
                            LocalImageCollection.Add(localImage);
                        }
                    });
                    this.LoadingFilesFromDirectory = false;
                    this.StatusMessage = "Ready";
                }
                catch(LocalImageFileReadException exception)
                {
                    MessageBox.Show("Curropt File Name: " + exception.FilePath, "File Curropt, Delete It");
                    this.StatusMessage = "Delete: " + exception.FilePath;
                    this.LoadingFilesFromDirectory = false;
                }
            }, loadingLocalFilesTaskCancellationToken.Token);
            task.Start();
            task.ContinueWith((t) =>
            {
                this.StatusMessage = "Ready";
                this.LoadingFilesFromDirectory = false;
            });
        }

        public void OnConvertRemoteImagesToLocal()
        {
            if (RemoteImageCollection.Count == 0)
            {
                MessageBox.Show("No Images in Remote Image List to Download!", "Add Remote Images by CSV File");
                return;
            }

            foreach (var remoteImage in RemoteImageCollection)
            {
                remoteImage.Uploaded = false;
            }
            this.GeneratingLocalImagesFromRemote = true;
            var standardStatusMessage = "Converting Remote Urls into LocalImages. Please Wait... ";
            this.StatusMessage = standardStatusMessage;
            var counter = 0;
            Task task = new Task(() =>
            {
                Parallel.ForEach(RemoteImageCollection, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (remoteImage) => {
                    remoteImage.UploadInProgress = true;
                    LocalImage localImage;
                    try
                    {
                        localImage = LocalImage.GenerateLocalImageFromRemote(remoteImage).Result;
                    }
                    catch
                    {
                        remoteImage.Failed = true;
                        return;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        LocalImageCollection.Insert(0, localImage);
                        remoteImage.Uploaded = true;
                    });
                    remoteImage.UploadInProgress = false;
                    counter++;
                    this.StatusMessage = (standardStatusMessage + (((double)counter/(double)RemoteImageCollection.Count)*100).ToString("F2") + "%");
                });
            });
            task.Start();
            task.ContinueWith(new Action<Task>((task1) => {
                this.GeneratingLocalImagesFromRemote = false;
                this.StatusMessage = "Ready";
            }));
        }

        public void OnOpenFileForRemoteImageCollection()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "csv files (*.csv)|*.csv";
            fileDialog.ShowDialog();
            if(fileDialog.CheckFileExists && fileDialog.FileName != "")
            {
                try
                {
                    var remoteImages = RemoteImage.ReadRemoteImagesFromFile(fileDialog.FileName);
                    remoteImages.ForEach((remoteImage) =>
                    {
                        RemoteImageCollection.Add(remoteImage);
                    });
                }
                catch
                {
                    MessageBox.Show("CSV File Error, Please Check the CSV File for Errors!", "Check CSV");
                }
            }
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
