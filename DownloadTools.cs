using System;
using System.IO;
using System.Net;
using ZipFile = System.IO.Compression.ZipFile;

namespace dVRC
{
    public static class DownloadTools
    {
        public static Stream DownloadFile(string url)
        {
            // TODO: Use HttpClient
            using (WebClient client = new WebClient())
                return new MemoryStream(client.DownloadData(url));
        }

        public static void DownloadAndSaveFile(string url, string outputFile, Action callback = null)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += (sender, args) => callback?.Invoke();
                client.DownloadFileAsync(new Uri(url), outputFile);
            }
        }

        public static void ExtractArchive(string fileName, string outputPath) =>
            ZipFile.ExtractToDirectory(fileName, outputPath);
    }
}