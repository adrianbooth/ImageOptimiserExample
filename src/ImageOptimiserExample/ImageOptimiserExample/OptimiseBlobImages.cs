using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ImageOptimiserExample
{
    public static class OptimiseBlobImages
    {
        [FunctionName("OptimiseBlobImages")]
        public static void Run([BlobTrigger("imageuploads/{name}", Connection = "AzureWebJobsStorage")] Stream image,
             [Blob("imageuploads/__processedItems", FileAccess.Read, Connection = "AzureWebJobsStorage")] Stream listRead,
             [Blob("imageuploads/__processedItems", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream listWrite,
             [Blob("imageuploads/{name}", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream imageSmall, string name, TraceWriter log)
        {
            if (!IsSupported(name))
                return;

            if (AlreadyOptimised(name, listRead, listWrite))
                return;

            var imageCompressor = new ImageCompressor();
            var result = imageCompressor.ProcessImage(name, image, imageSmall, true);
            log.Info(result);
        }

        private static bool IsSupported(string name)
        {
            // Currently using this file as a record of what has been optimised, it should be ignored
            if (name == "__processedItems")
                return false;

            // This is not the ideal solution, instead of relying on the file extension in the name we should ideally read the 
            // magic bytes from the start of the file stream to check what file type it is
            var fileType = Path.GetExtension(name).ToLowerInvariant();
            if (!ImageCompressorHelper.SupportedFormats.Contains(fileType))
                return false;

            return true;
        }

        private static void WriteFileListToStream(Stream processedFiles, List<string> optimisedFiles)
        {
            using (var writer = new StreamWriter(processedFiles))
            {
                foreach (var file in optimisedFiles)
                    writer.WriteLine(file);
            }
        }

        private static bool AlreadyOptimised(string name, Stream listRead, Stream listWrite)
        {
            //should probably hash the file stream and save the hash to the list of hashed files so if files are changed they will be re-optimised
            var optimisedFiles = new List<string>();

            if (listRead != null)
                optimisedFiles = ReadLinesFromStream(listRead);

            if (optimisedFiles.Contains(name))
                return true;

            optimisedFiles.Add(name);
            WriteFileListToStream(listWrite, optimisedFiles);

            return false;
        }

        private static List<string> ReadLinesFromStream(Stream listRead)
        {
            var optimisedFiles = new List<string>();

            using (var reader = new StreamReader(listRead))
            {
                while (!reader.EndOfStream)
                {
                    optimisedFiles.Add(reader.ReadLine());
                }
            }
            return optimisedFiles;
        }
    }
}