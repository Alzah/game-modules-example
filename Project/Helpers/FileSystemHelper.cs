using Project.Extensions;
using Project.Helpers.Exceptions;
using Project.Systems.Logging;
using RSG;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Project.Helpers
{
    public static class FileSystemHelper
    {
        public static Promise<bool> WriteToStorageAsync(string path, string content)
        {
            var promise = new Promise<bool>();
            WriteToStorage(path, content).ContinueWith((task) =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        promise.Resolve(true);
                        break;
                    case TaskStatus.Faulted:
                        promise.Reject(task.Exception);
                        break;
                    case TaskStatus.Canceled:
                        promise.Reject(new FileSystemCancelTaskException());
                        break;
                    default:
                        promise.Reject(new FileSystemWriteException(task));
                        break;
                }
            },
            TaskScheduler.FromCurrentSynchronizationContext());
            return promise;
        }

        public static Promise<string> ReadFromStorageAsync(string path)
        {
            var promise = new Promise<string>();
            ReadFromStorage(path).ContinueWith((task) =>
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        promise.Resolve(task.Result);
                        break;
                    case TaskStatus.Faulted:
                        promise.Reject(task.Exception);
                        break;
                    case TaskStatus.Canceled:
                        promise.Reject(new FileSystemCancelTaskException());
                        break;
                    default:
                        promise.Reject(new FileSystemReadException(task));
                        break;
                }
            },
            TaskScheduler.FromCurrentSynchronizationContext());
            return promise;
        }

        private static async Task WriteToStorage(string path, string data)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir.IsNullOrEmpty())
            {
                throw LogSystem.Exception(new Exception($"Path broken: {path}"));
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
            {
                byte[] buffer = Encoding.Default.GetBytes(data);
                await fstream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private static async Task<string> ReadFromStorage(string path)
        {
            using (FileStream fstream = File.OpenRead(path))
            {
                byte[] buffer = new byte[fstream.Length];
                await fstream.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.Default.GetString(buffer);
            }
        }

        public static void OpenDirectory(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe",
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(startInfo);
            }
            else
            {
                throw LogSystem.Exception(new DirectoryNotFoundException(folderPath));
            }
        }
    }
}
