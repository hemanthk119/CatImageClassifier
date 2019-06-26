using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace CatImageRecognizer.Models
{
    public enum ImageType
    {
        CAT=1, NOT_CAT=0
    }

    public class RemoteImage : INotifyPropertyChanged
    {
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                RaiseProperty("Url");
            }
        }

        private ImageType imageType;
        public ImageType ImageType
        {
            get
            {
                return imageType;
            }
            set
            {
                imageType = value;
                RaiseProperty("ImageType");
            }
        }

        private bool uploadInProgress;

        [CsvHelper.Configuration.Attributes.Ignore]
        public bool UploadInProgress
        {
            get
            {
                return uploadInProgress;
            }
            set
            {
                uploadInProgress = value;
                RaiseProperty("UploadInProgress");
            }
        }

        private bool uploaded;

        [CsvHelper.Configuration.Attributes.Ignore]
        public bool Uploaded
        {
            get
            {
                return uploaded;
            }
            set
            {
                uploaded = value;
                RaiseProperty("Uploaded");
            }
        }

        private bool failed;

        [CsvHelper.Configuration.Attributes.Ignore]
        public bool Failed
        {
            get
            {
                return failed;
            }
            set
            {
                failed = value;
                RaiseProperty("Failed");
            }
        }

        public RemoteImage()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public static List<RemoteImage> ReadRemoteImagesFromFile(string filePath)
        {
            using (var streamReader = new StreamReader(filePath))
            using (var csv = new CsvHelper.CsvReader(streamReader))
            {
                var objects = csv.GetRecords<RemoteImage>().ToList();
                return (List<RemoteImage>)objects;
            }
        }
    }

    public class LocalImageFileReadException : Exception
    {
        public string FilePath { get; set; }
        public LocalImageFileReadException(string filePath)
        {
            FilePath = filePath;
        }
    }

    public class LocalImage : INotifyPropertyChanged
    {
        public static Random random = new Random(new Random().Next());

        public string LocalPath { get; set; }
        public ImageType ImageType { get; set; }
        public Emgu.CV.Image<Bgr, Byte> OriginalImage { get; set; }
        public Emgu.CV.Image<Gray, Byte> CompressedImage { get; set; }

        private bool trainingInProgress;
        public bool TrainingInProgress
        {
            get
            {
                return trainingInProgress;
            }
            set
            {
                trainingInProgress = value;
                RaiseProperty("TrainingInProgress");
            }
        }

        private bool testFailed;
        public bool TestFailed
        {
            get
            {
                return testFailed;
            }
            set
            {
                testFailed = value;
                RaiseProperty("TestFailed");
            }
        }

        private bool testPassed;
        public bool TestPassed
        {
            get
            {
                return testPassed;
            }
            set
            {
                testPassed = value;
                RaiseProperty("TestPassed");
            }
        }

        private int trainedIterations = 0;
        public int TrainedIterations
        {
            get
            {
                return trainedIterations;
            }
            set
            {
                trainedIterations = value;
                RaiseProperty("TrainedIterations");
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

        public static (double[], ImageType) GetImageInformationForNeuralNetwork(LocalImage localImage)
        {
            return (GetNetworkFeedArray(localImage.CompressedImage), localImage.ImageType);
        }

        public static double[] GetNetworkFeedArray(Image<Gray, Byte> image)
        {
            var imageBytes = image.Bytes;
            double[] networkFeed = new double[imageBytes.Count()];
            for (int i = 0; i < imageBytes.Length; i++)
            {
                networkFeed[i] = ((double)imageBytes[i] / 256);
            }
            return networkFeed;
        }

        public static List<LocalImage> ReadAllLocalImagesFromDirectory(Action<double> progressCallback)
        {
            string catFolder = Environment.CurrentDirectory + "/Images/CAT";
            string notCatFolder = Environment.CurrentDirectory + "/Images/NOT_CAT";
            List<LocalImage> localImages = new List<LocalImage>();

            int counter = 0;
            IEnumerable<string> catFiles = new List<string>();
            IEnumerable<string> notCatFiles = new List<string>();
            if (Directory.Exists(catFolder))
            {
                catFiles = Directory.EnumerateFiles(catFolder);
            }
            if (Directory.Exists(notCatFolder))
            {
                notCatFiles = Directory.EnumerateFiles(notCatFolder);
            }
            var totalFiles = catFiles.Count() + notCatFiles.Count();

            foreach (var catFile in catFiles)
            {
                localImages.Add(ReadLocalImageFromLocalFile(catFile, ImageType.CAT));
                counter++;
                progressCallback(((double)counter/ (double)totalFiles)*100);
            }

            foreach (var notCatFile in notCatFiles)
            {
                localImages.Add(ReadLocalImageFromLocalFile(notCatFile, ImageType.NOT_CAT));
                counter++;
                progressCallback(((double)counter / (double)totalFiles) * 100);
            }
            progressCallback(100);
            return localImages;
        }

        public static Image<Gray, Byte> ConvertOriginalImageToGrayScaleAndProcess(Image<Bgr, Byte> orginalImage)
        {
            var grayScale = orginalImage.Convert<Gray, Byte>();
            return grayScale.Resize(50,50, Emgu.CV.CvEnum.Inter.Cubic, false);
        }

        public static LocalImage ReadLocalImageFromLocalFile(string localPath, ImageType imageType)
        {
            try
            {
                var orginalImage = new Emgu.CV.Image<Bgr, byte>(localPath);
                var processedImage = ConvertOriginalImageToGrayScaleAndProcess(orginalImage);

                return new LocalImage()
                {
                    OriginalImage = orginalImage,
                    LocalPath = localPath,
                    CompressedImage = processedImage,
                    ImageType = imageType
                };
            }
            catch
            {
                throw new LocalImageFileReadException(localPath);
            }
        }

        public static string GetNewFilePath(ImageType imageType, out string newFilePath)
        {
            var localPath = Environment.CurrentDirectory + "/Images/" + imageType.ToString() + "/" + Guid.NewGuid().ToString() + ".jpg";
            newFilePath = localPath;
            return localPath;
        } 

        public static async Task<LocalImage> GenerateNewLocalFileFromImageTypeAndData(ImageType imageType, byte[] data)
        {
            return await Task<LocalImage>.FromResult<LocalImage>(((Func<LocalImage>)(() =>
            {
                MemoryStream stream = new MemoryStream(data);
                var image = Image.FromStream(stream);
                GetNewFilePath(imageType, out string newFilePath);
                FileHelper.CreateDirectoryForPath(newFilePath);
                image.Save(newFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ReadLocalImageFromLocalFile(newFilePath, imageType);
            }))());
        }

        public static async Task<LocalImage> GenerateLocalImageFromRemote(RemoteImage remoteImage)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage responseMessage;
                Stream imageStream;

                try
                {
                    responseMessage = await httpClient.GetAsync(remoteImage.Url);
                    imageStream = await responseMessage.Content.ReadAsStreamAsync();

                    if(responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new Exception("Remote URL Exception");
                }

                byte[] imageBytes = new byte[imageStream.Length];
                await imageStream.ReadAsync(imageBytes, 0, (int)imageStream.Length);

                try
                {
                    return await GenerateNewLocalFileFromImageTypeAndData(remoteImage.ImageType, imageBytes);
                }
                catch (Exception exception)
                {
                    throw exception;
                }             
            }
        }
    }

    public class DataCollection
    {
        public static List<RemoteImage> GetLocalImages(string csvFilePath)
        {
            return RemoteImage.ReadRemoteImagesFromFile(csvFilePath);
        }
    }
}
