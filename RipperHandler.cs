using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace dVRC
{
    public class RipperHandler
    {
        public string WorkingDirectory { get; private set; }
    
        private string DownloadURL
        {
            get
            {
                string baseURL = "https://github.com/AssetRipper/AssetRipper/releases/latest/download/AssetRipper_";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    baseURL += "win_x64";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    baseURL += "mac_x64";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    baseURL += "linux_x64";
                else
                    throw new Exception("Unknown OSPlatform");
                baseURL += ".zip";
                return baseURL;
            }
        }

        private string ExecutableName
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "AssetRipper.exe";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "AssetRipper.exe";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "AssetRipper.exe";
                throw new Exception("Unknown OSPlatform");
            }
        }
    
        public bool isPresent => File.Exists(Path.Combine(WorkingDirectory, ExecutableName));

        public bool IsWorking
        {
            get
            {
                if (CurrentProcess != null)
                    return !CurrentProcess.HasExited;
                return false;
            }
        }
    
        public Action<string> OnData = s => { };

        private Process CurrentProcess;
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

        public void Rip(string assetFile, string outputDirectoryName)
        {
            CurrentProcess = new Process();
            CurrentProcess.StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = assetFile + " -o " + outputDirectoryName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            CurrentProcess.OutputDataReceived += (sender, eventArgs) =>
            {
                string data = eventArgs.Data ?? String.Empty;
                OnData.Invoke(data);
                Debug.Log(data);
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CurrentProcess.StartInfo.FileName = Path.Combine(WorkingDirectory, ExecutableName);
                CurrentProcess.Start();
                CurrentProcess.StandardInput.AutoFlush = true;
                myStreamWriter = CurrentProcess.StandardInput;
                CurrentProcess.BeginOutputReadLine();
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
                    CurrentProcess.StartInfo.FileName = Path.Combine(WorkingDirectory, ExecutableName);
                    CurrentProcess.Start();
                    CurrentProcess.StandardInput.AutoFlush = true;
                    myStreamWriter = CurrentProcess.StandardInput;
                    CurrentProcess.BeginOutputReadLine();
                };
                chmodProcess.Start();
            }
        }
    }
}