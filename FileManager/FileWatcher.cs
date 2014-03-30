using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileManager
{
    /// <summary>
    /// this class is used to watch the changes in local file system
    /// </summary>
    public class FileWatcher
    {
        private static FileSystemWatcher systemFileWatcher;
        private static FileWatcher myFileWatcher = new FileWatcher();
        private FileWatcher()
        {
            systemFileWatcher = new FileSystemWatcher(Properties.FileManagerSettings.Default.ServerDirectory);
            //when  filesystem changes,it can raise events
            systemFileWatcher.IncludeSubdirectories = true;
            systemFileWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            systemFileWatcher.Changed += new FileSystemEventHandler(OnChanged);
            systemFileWatcher.Deleted += new FileSystemEventHandler(OnDeleted);
            systemFileWatcher.Renamed += new RenamedEventHandler(OnRenamed);
        }
        /// <summary>
        /// when file or directory changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //to avoid more conflict between save & read , I delete the file which was changed ,instead of update it in the buffer
            //in test, due to "\\",the filepath can't be find in fileBuffer dictionary
            string relativePath = e.FullPath.Substring(Properties.FileManagerSettings.Default.ServerDirectory.Length );
            relativePath = relativePath.Replace("\\", "/");
            //when changes a file
            FileBuffer.GetInstance().OnFileChanged(relativePath);
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            string relativePath = e.FullPath.Substring(Properties.FileManagerSettings.Default.ServerDirectory.Length);
            relativePath = relativePath.Replace("\\", "/");
            //when changes a file
            FileBuffer.GetInstance().OnFileOrDirectoryDeleted(relativePath);
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {

            if(Directory.Exists(e.FullPath))
            {
                //rename a Directory
                DirectoryInfo newDirectory = new DirectoryInfo(e.FullPath);
                deleteWholeDirectoryInBuffer(newDirectory, e.OldFullPath);
            }
            else
            {
                //rename a single file
                string relativePath = e.OldFullPath.Substring(Properties.FileManagerSettings.Default.ServerDirectory.Length);
                relativePath = relativePath.Replace("\\", "/");
                FileBuffer.GetInstance().OnFileChanged(relativePath);  
            }
                   
        }

        /// <summary>
        /// delete the files of the directory in buffer iteratively
        /// </summary>
        /// <param name="DirectoryPath"></param>
        private void deleteWholeDirectoryInBuffer(DirectoryInfo directory,string oldDirectoryPath)
        {
            string directoryPath=directory.FullName;
            string relativePath;
            if(!Directory.Exists(directoryPath))
            {
                return;
            }

            FileInfo[] files = directory.GetFiles();
            //delete each file of the directory in buffer
            foreach (FileInfo file in files)
            {
                relativePath = oldDirectoryPath.Substring(Properties.FileManagerSettings.Default.ServerDirectory.Length);
                relativePath = relativePath.Replace("\\", "/") + "/" + file.Name;
                FileBuffer.GetInstance().OnFileChanged(relativePath);
            }
            //delete each file of the subDirectory in filebuffer
            //oldDirectoryPath=oldDirectoryPath+"/"+directory.Name;
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach(DirectoryInfo subDirectory in subDirectories)
            {
                string oldFullPath = oldDirectoryPath + "\\" + subDirectory.Name;
                deleteWholeDirectoryInBuffer(subDirectory,oldFullPath);
            }

        }
        /// <summary>
        /// start watching local file system
        /// which be called when the fileBuffer start to run
        /// </summary>
        internal void Run()
        {
            systemFileWatcher.EnableRaisingEvents = true;
        }

        internal void Stop()
        {
            systemFileWatcher.EnableRaisingEvents = false;
        }

        public static FileWatcher getInstance()
        {
            return myFileWatcher;
        }
    }
}
