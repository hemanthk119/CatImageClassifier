using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatImageRecognizer.NeuralNetworks
{
    public struct AnchorBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class DetectionData
    {
        public static Random random = new Random();
        public static List<AnchorBox> GetInitialAnchorBoxes()
        {
            var anchorBoxes = new List<AnchorBox>();
            anchorBoxes.Add(new AnchorBox { Y = 0, X = 0, Width = 1, Height = 1 });
            foreach(int i in Enumerable.Range(0, 50))
            {
                var x = random.NextDouble();
                var y = random.NextDouble();

                var width = random.Next(1000, (int)((1 - x) * 100000)+1000) / (double)100000;
                var height = random.Next(1000, (int)((1 - y) * 100000)+1000) / (double)100000;

                anchorBoxes.Add(new AnchorBox() {
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                });
            }
            return anchorBoxes;
        }

        public static Image<Bgr, Byte> DrawAnchorBoxesOnImage(Image<Bgr, Byte> originalImage, IEnumerable<AnchorBox> anchorBoxes)
        {
            foreach(var anchorBox in anchorBoxes)
            {
                DrawAnchorBoxOnImage(originalImage, anchorBox);
            }
            return originalImage;
        }

        public static Image<Bgr, Byte> DrawAnchorBoxOnImage(Image<Bgr, Byte> originalImage, AnchorBox anchorBox)
        {
            originalImage.Draw(new System.Drawing.Rectangle((int)(originalImage.Width * anchorBox.X), (int)(originalImage.Height * anchorBox.Y), (int)(originalImage.Width * anchorBox.Width), (int)(originalImage.Height * anchorBox.Height)), new Bgr(255, 0, 0), 4);
            return originalImage;
        }
    }
}
