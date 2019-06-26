using CatImageRecognizer.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CatImageRecognizer
{
    public class CatImageTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var catImageType = (ImageType)value;
            return catImageType == ImageType.CAT ? "Cat" : "Not a Cat";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultipleBooleanANDConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool finalResult = true;
            foreach(bool value in values)
            {
                finalResult = finalResult && value;
            }
            var invert = parameter != null ? (bool)parameter : false;
            return invert ? !finalResult : finalResult;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultipleBooleanORConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool finalResult = false;
            foreach (bool value in values)
            {
                finalResult = finalResult || value;
            }
            var invert = parameter != null ? (bool)parameter : false;
            return invert ? !finalResult : finalResult;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reverse = false;
            if(parameter != null)
            {
                reverse = (bool)parameter;
            }

            if((bool)value == true)
            {
                return reverse ? Visibility.Hidden : Visibility.Visible;
            }
            return reverse ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class BoolToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reverse = false;
            if (parameter != null)
            {
                reverse = (bool)parameter;
            }

            if ((bool)value == true)
            {
                return reverse ? Visibility.Collapsed : Visibility.Visible;
            }
            return reverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class EmptyListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reverse = false;
            if (parameter != null)
            {
                reverse = (bool)parameter;
            }

            if ((int)value > 0)
            {
                return reverse ? Visibility.Hidden : Visibility.Visible;
            }
            return reverse ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    public class EmgCVImageToBitmapImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emguCVImage = (Emgu.CV.IImage)value;
            if(emguCVImage == null)
            {
                return new BitmapImage();
            }
            return GetJPEGEncodedImage(emguCVImage, GetJPEGEncoderParameters());
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private EncoderParameters GetJPEGEncoderParameters()
        {
            System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters jpegEncoderParameters = new EncoderParameters(1);
            EncoderParameter qualityEncoderParamter = new EncoderParameter(qualityEncoder, 50L);
            jpegEncoderParameters.Param[0] = qualityEncoderParamter;
            return jpegEncoderParameters;
        }

        private BitmapImage GetJPEGEncodedImage(Emgu.CV.IImage rawImage, EncoderParameters encoderParameters)
        {
            MemoryStream imageMemoryStream = new MemoryStream();
            ((System.Drawing.Bitmap)rawImage.Bitmap).Save(imageMemoryStream, GetEncoder(ImageFormat.Jpeg), GetJPEGEncoderParameters());
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            imageMemoryStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = imageMemoryStream;
            image.EndInit();
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BooleanInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

}
