using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace dVRC
{
    public class RipperHandler
    {
        private const string URI = "http://127.0.0.1:42176/";
        private const int SPECIFIC_PORT = 42176;
        
        public string WorkingDirectory { get; private set; }
    
        private string DownloadURL
        {
            get
            {
                string baseURL = "https://github.com/AssetRipper/AssetRipper/releases/latest/download/AssetRipper_";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    baseURL += "win_";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    baseURL += "mac_";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    baseURL += "linux_";
                else
                    throw new Exception("Unknown OSPlatform");
                if (RuntimeInformation.ProcessArchitecture == Architecture.X86 ||
                    RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    baseURL += "x64";
                else
                    baseURL += "arm64";
                baseURL += ".zip";
                return baseURL;
            }
        }

        private string ExecutableName
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "AssetRipper.GUI.Free.exe";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "AssetRipper.GUI.Free";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "AssetRipper.GUI.Free";
                throw new Exception("Unknown OSPlatform");
            }
        }
    
        public bool isPresent => File.Exists(Path.Combine(WorkingDirectory, ExecutableName));

        public bool IsRunning => Process.GetProcessesByName("AssetRipper.GUI.Free").Length > 0;
        public bool IsWorking { get; private set; }
        public float Progress { get; private set; }

        private static StreamWriter myStreamWriter;

        public void SetWorkingDirectory()
        {
            WorkingDirectory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "AssetRipper");
        }

        public void Download(Action onDone)
        {
            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);
            else
                Directory.Delete(WorkingDirectory, true);
            DownloadTools.DownloadAndSaveFile(DownloadURL, WorkingDirectory + "/AssetRipper.zip", () =>
            {
                DownloadTools.ExtractArchive(WorkingDirectory + "/AssetRipper.zip", WorkingDirectory);
                onDone.Invoke();
            });
        }

        public void StartApplication()
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = $"--port {SPECIFIC_PORT} --launch-browser false",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                string data = eventArgs.Data ?? String.Empty;
                Debug.Log(data);
                float? p = GetProgress(data);
                if(p == null) return;
                Progress = p.Value;
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = Path.Combine(WorkingDirectory, ExecutableName);
                process.Start();
                process.StandardInput.AutoFlush = true;
                myStreamWriter = process.StandardInput;
                process.BeginOutputReadLine();
            }
            else
            {
                // Make sure the file is executable first
                Process chmodProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("chmod", $"+x {Path.Combine(WorkingDirectory, ExecutableName)}")
                    {
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };
                chmodProcess.Exited += (sender, args) =>
                {
                    process.StartInfo.FileName = Path.Combine(WorkingDirectory, ExecutableName);
                    process.Start();
                    process.StandardInput.AutoFlush = true;
                    myStreamWriter = process.StandardInput;
                    process.BeginOutputReadLine();
                };
                chmodProcess.Start();
            }
        }

        public void StopApplication()
        {
            foreach (Process process in Process.GetProcessesByName("AssetRipper.GUI.Free"))
            {
                process.Kill();
            }
        }

        private float? GetProgress(string inp)
        {
            string[] s = inp.Split(' ');
            if (s.Length <= 0) return null;
            if (s[0].ToLower() != "exportprogress") return null;
            string progressString = s[2];
            progressString = progressString.TrimStart('(');
            progressString = progressString.TrimEnd(')');
            string[] progressSplit = progressString.Split('/');
            int num = Convert.ToInt32(progressSplit[0]);
            int den = Convert.ToInt32(progressSplit[1]);
            return (float) num / den;
        }

        public async void Rip(string assetFile, string outputDirectoryName, Action complete = null)
        {
            if (!IsRunning) StartApplication();
            IsWorking = true;
            // Create HTTP Client
            using HttpClient httpClient = new HttpClient();
            // Reset
            await httpClient.PostAsync(URI + "Reset", new StringContent(""));
            // Load Target File
            Dictionary<string, string> loadParameters = new Dictionary<string, string>
            {
                ["Path"] = Path.GetFullPath(assetFile)
            };
            await httpClient.PostAsync(URI + "LoadFolder", new FormUrlEncodedContent(loadParameters));
            // Export to Unity Project
            Dictionary<string, string> exportParameters = new Dictionary<string, string>
            {
                ["Path"] = outputDirectoryName
            };
            await httpClient.PostAsync(URI + "Export/UnityProject", new FormUrlEncodedContent(exportParameters));
            IsWorking = false;
            StopApplication();
            complete?.Invoke();
            Progress = 0;
        }
    }
}