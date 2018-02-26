using System.Diagnostics;
using System.IO;

namespace ImageOptimiserExample
{
    public class ImageCompressor
    {
        public string ProcessImage(string name, Stream originalImage, Stream optimisedImage, bool allowLossyProcessing)
        {
            var workingDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            var filename = Path.Combine(workingDirectory, name);

            using (Stream file = File.Create(filename))
                originalImage.CopyTo(file);

            var result = CompressFile(filename, true);

            if (result.Saving < 1)
            {
                originalImage.CopyTo(optimisedImage);
                ImageCompressorHelper.Cleanup(result);
                return "Optimisation failed to shrink file";
            }

            using (var stream = File.Open(result.ResultFileName, FileMode.Open))
                stream.CopyTo(optimisedImage);

            ImageCompressorHelper.Cleanup(result);

            return result.ToString();
        }



        private CompressionResult CompressFile(string fileName, bool lossy)
        {
            string optimisedFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(fileName));

            ProcessStartInfo start = new ProcessStartInfo("cmd")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.Combine(Directory.GetParent(Path.GetDirectoryName(GetType().Assembly.Location)).FullName, @"Utilities\"),
                Arguments = ImageCompressorHelper.OptimiseImageCommand(fileName, optimisedFile, lossy),
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(start))
            {
                process.WaitForExit();
            }

            return new CompressionResult(fileName, optimisedFile);
        }
    }
}