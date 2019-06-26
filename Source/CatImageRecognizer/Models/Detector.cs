using CatImageRecognizer.NeuralNetworks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatImageRecognizer.Models
{
    public class AnchorBox
    {
        public static Random random = new Random();
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public static AnchorBox GenerateNew()
        {
            double x = (random.NextDouble()) / (double)1.1;
            double y = (random.NextDouble()) / (double)1.1;
            var smallerValue = x > y ? x : y;
            double width = random.Next(100000, (int)(((double)1 - smallerValue) * 10000000)) / (double)10000000;
            double height = width;
            return new AnchorBox()
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }

        public static List<AnchorBox> GenerateRandomAnchorBoxes()
        {
            var anchorBoxList = new List<AnchorBox>() { new AnchorBox() { Height = 1, Width = 1, X = 0, Y = 0 } };
            foreach (int i in Enumerable.Range(0, 1000))
            {   
                anchorBoxList.Add(AnchorBox.GenerateNew());
            }
            return anchorBoxList;
        }

        public static System.Drawing.Rectangle GetBoundingReactange(List<System.Drawing.Rectangle> detectedRectangles)
        {
            System.Drawing.Rectangle boundingRectangle = new System.Drawing.Rectangle();
            foreach(var rectangle in detectedRectangles)
            {
                boundingRectangle = System.Drawing.Rectangle.Union(boundingRectangle, rectangle);
            }
            return boundingRectangle;
        }
    }

    public static class Detector 
    {
        private static List<System.Drawing.Rectangle> GetDetectionRectangles(INeuralNetwork neuralNetwork, Image<Bgr, Byte> originalImage, Action<double> progressUpdater)
        {
            List<System.Drawing.Rectangle> DetectedRectangles = new List<System.Drawing.Rectangle>();
            var anchorBoxes = AnchorBox.GenerateRandomAnchorBoxes();
            int counter = 0;
            Parallel.ForEach(anchorBoxes, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, (anchorBox) => {
                (var croppedImage, var rectangle) = GetAreaUnderAnchorBox(originalImage, anchorBox);
                var catDetected = DetectCat(neuralNetwork, croppedImage);
                if (catDetected)
                {
                    DetectedRectangles.Add(rectangle);
                }
                progressUpdater(((double)counter / (double)anchorBoxes.Count()) * 100);
                counter++;
            });
            return DetectedRectangles;
        }

        private static System.Drawing.Rectangle GetRectangleFromAnchroBox(Image<Bgr, Byte> originalImage, AnchorBox anchorBox)
        {
            int areaX = (int)((double)(originalImage.Width) * anchorBox.X);
            int areaY = (int)((double)(originalImage.Height) * anchorBox.Y);
            int areaWidth = (int)((double)(originalImage.Width) * anchorBox.Width);
            int areaHeight = (int)((double)(originalImage.Height) * anchorBox.Height);

            return new System.Drawing.Rectangle(areaX, areaY, areaWidth, areaHeight);
        }

        private static (Image<Gray, Byte>, System.Drawing.Rectangle) GetAreaUnderAnchorBox(Image<Bgr, Byte> originalImage, AnchorBox anchorBox)
        {
            var originalImageCopy = originalImage.Copy();
            var rectangle = GetRectangleFromAnchroBox(originalImageCopy, anchorBox);
            originalImageCopy.ROI = rectangle;
            var croppedImage = LocalImage.ConvertOriginalImageToGrayScaleAndProcess(originalImageCopy.Copy());
            originalImageCopy.ROI = System.Drawing.Rectangle.Empty;
            return (croppedImage, rectangle);
        }

        private static bool DetectCat(INeuralNetwork neuralNetwork, Image<Gray, Byte> image)
        {
            double[] networkFeed = LocalImage.GetNetworkFeedArray(image);
            var networkOutput = neuralNetwork.GenerateOutput(networkFeed);
            var outputValue = networkOutput[0];
            var complementaryOutputValue = networkOutput[1];
            return outputValue > 0.95 && complementaryOutputValue < 0.05;
        }

        public static (ImageType, List<System.Drawing.Rectangle>) DetectCatInImageFile(INeuralNetwork neuralNetwork, Image<Bgr, Byte> originalImage, Image<Gray, Byte> processedImage, Action<double> progressUpdater, bool getDetectionRectangles = true)
        {
            var detectedRectangles = getDetectionRectangles ? GetDetectionRectangles(neuralNetwork, originalImage, progressUpdater) : new List<System.Drawing.Rectangle>();
            var catDetected = DetectCat(neuralNetwork, processedImage);
            progressUpdater(100);
            return (catDetected ? ImageType.CAT :ImageType.NOT_CAT, detectedRectangles);
        }
    }
}
