using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CatImageRecognizer.Models
{
    public class FileHelper
    {
        public static void CreateDirectoryForPath(string filePath)
        {
            var pathRoot = System.IO.Path.GetDirectoryName(filePath);
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(pathRoot);
        }

        public static async Task WriteTextAsync(string filePath, byte[] encodedText)
        {
            try
            {
                CreateDirectoryForPath(filePath);
                using (FileStream sourceStream = new FileStream(filePath,
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
                    bufferSize: 4096, useAsync: true))
                {
                    var result = sourceStream.BeginWrite(encodedText, 0, encodedText.Length, new AsyncCallback((obj) => { }), new { });
                    sourceStream.EndWrite(result);
                    return;              
                };
            }
            catch
            {
                MessageBox.Show("Error in file");
            }
        }
    }
}
