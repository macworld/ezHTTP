using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileManager
{
    public class FileSystem
    {
        //used to set the workspace of the webserver
        internal string ServerDiretory;

        private static FileSystem localFileSystem = new FileSystem();
        private FileSystem()
        {
            this.ServerDiretory = Properties.FileManagerSettings.Default.ServerDirectory;
        }
        /// <summary>
        /// get the instance of this singleton model class
        /// </summary>
        /// <returns></returns>
        public static FileSystem GetInstance()
        {
            return localFileSystem;
        }
        /// <summary>
        /// search the file in local file system
        /// if not exist return false
        /// </summary>
        /// <param name="url">the url which has been deal with,eg:index.html or contern/1.jpg</param>
        /// <returns></returns>
        public bool SearchFile(string url)
        {
            string localPath = ServerDiretory + url;
            return File.Exists(localPath);
        }

        public byte[] readFile(string url) //?此处关于readBuffer的回收问题
        {
            string localPath = ServerDiretory + url;
            FileInfo targetFile = new FileInfo(localPath);
            long fileLength = targetFile.Length;
            FileStream readIn = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            Byte[] readBuffer = new byte[fileLength];
            readIn.Read(readBuffer, 0, (int)fileLength);
            return readBuffer;
        }

        public void ResetSeverDirectory()
        {
            this.ServerDiretory = Properties.FileManagerSettings.Default.ServerDirectory;
        }

    }
}
