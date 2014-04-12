using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using System.IO;

namespace FileManager
{
    /// <summary>
    /// this class is used to manage the filebuffer including replacement policy
    /// it's also designed in singleton model
    /// </summary>
    public class FileBuffer
    {
        //I use the way like paging to manage the FileBuffer
        int pageSize;//size of each page
        int totalPage;//the max num of page
        int bufferSize;//the size of FileBuffer
        int currentPageNum;//the offset of pages which has been used 
        int totalFreeSpace;//to recorde the size of free space in the buffer
        byte[] memoryBuffer;
        bool isRun = false;

        Stack<int> freePageStack;//the page which has been 
        Dictionary<string, string> urlToPageDic;//used to store the pageNums of the file in the buffer 
        LinkedList<string> LruList;//to relize LRU 
        Dictionary<string, long> fileLengthDictionary;//store the length of the file store in the buffer

        private static FileBuffer localFileBuffer=new FileBuffer();
        private object LockSSConflict = new object();  //used to avoid save & save conflict;
        private object LockRDConflict = new object();  //used to avoid read & delete conflict
        private FileBuffer()
        {
            pageSize = Properties.FileManagerSettings.Default.PageSize;
            totalPage = Properties.FileManagerSettings.Default.MaxPageNum;
            bufferSize = pageSize * totalPage;
            totalFreeSpace = bufferSize;
            memoryBuffer = new Byte[bufferSize];
            currentPageNum = 0;
            freePageStack = new Stack<int>();
            urlToPageDic = new Dictionary<string, string>();
            LruList = new LinkedList<string>();
            fileLengthDictionary = new Dictionary<string, long>();
        }
        /// <summary>
        /// when you want to use this service,you need to call the Run() function
        /// </summary>
        public void Run()
        {
            if(!isRun)
            {
                //run fileWatcher
                FileWatcher.getInstance().Run();
                isRun = true;
            }
            else
            {
                Logger.GetLogger().Info("FileBuffer has already run");
            }
        }
        /// <summary>
        /// when stop this service we need to clear the buffer for fear of files changing 
        /// which will cause the different bettween local file and file buffer
        /// it's definitely dangerous
        /// </summary>
        public void Stop()
        {
            if(isRun)
            {
                isRun = false;
                FileWatcher.getInstance().Stop();
                clear();
            }
            else
            {
                Logger.GetLogger().Info("FileBuffer is not running.It can't be stopped");
            }
        }
        /// <summary>
        /// clear all the data in buffer
        /// </summary>
        private void clear()
        {
            totalFreeSpace = bufferSize;
            currentPageNum = 0;
            freePageStack.Clear();
            urlToPageDic.Clear();
            LruList.Clear();
            fileLengthDictionary.Clear();
        }
       
        public static FileBuffer GetInstance()
        {
            return localFileBuffer;
        }
        
        /// <summary>
        /// when a new file is read from local file system, the function will be called to store it in the buffer
        /// </summary>
        /// <returns></returns>
        private bool SaveFile(Byte[] fileByteStream,string url)
        {
            long fileLength = fileByteStream.Length;
            if(totalFreeSpace<fileLength)
            {
                if(fileLength>bufferSize) 
                {
                    //Logger.GetLogger().Info("The file is to big to buffer it");
                    return false;
                }
                //Logger.GetLogger().Info("There is not enough space in the fileBuffer.We'll try to release some older file");
                while(totalFreeSpace<fileLength)
                {
                    if(!ReleaseFileLRU(fileLength)) // while there is no file to realse ,return false
                    {
                        return false;
                    }
                 }
                SaveFile(fileByteStream, url);//when there are enough buffer space after delete
            }
            else
            {   
                long fileOffset=0;
                lock (LockSSConflict) // used to avoid the conflic of saving the same  
                {
                    if(fileLengthDictionary.ContainsKey(url))
                    {
                        return false;
                    }
                    LruList.AddFirst(url);
                    fileLengthDictionary.Add(url, fileLength);
                    string urlPages = "";
                    int pageNum = 0;
                    while (fileOffset < fileLength)
                    {
                        if (freePageStack.Count > 0)//while there is page while has been released
                        {
                            pageNum = freePageStack.Pop();
                            if (fileLength - fileOffset < pageSize)
                            {
                                System.Buffer.BlockCopy(fileByteStream, (Int32)fileOffset,
                                    memoryBuffer, pageNum * pageSize, (int)(fileLength - fileOffset));
                                fileOffset = fileLength;
                            }
                            else
                            {
                                System.Buffer.BlockCopy(fileByteStream, (Int32)fileOffset, memoryBuffer, pageNum * pageSize, pageSize);
                                fileOffset += pageSize;
                            }
                        }
                        else
                        {
                            pageNum = currentPageNum++;
                            if (fileLength - fileOffset < pageSize)
                            {
                                System.Buffer.BlockCopy(fileByteStream, (Int32)fileOffset,
                                    memoryBuffer, pageNum * pageSize, (int)(fileLength - fileOffset));
                                fileOffset = fileLength;
                            }
                            else
                            {
                                System.Buffer.BlockCopy(fileByteStream, (Int32)fileOffset, memoryBuffer, pageNum * pageSize, pageSize);
                                fileOffset += pageSize;
                            }
                        }
                        if (urlPages != "")
                        {
                            urlPages += ",";
                        }
                        urlPages += Convert.ToString(pageNum);
                        totalFreeSpace -= pageSize;
                    }
                    urlToPageDic.Add(url, urlPages);//added to the dictionnary which is used as a page table
                }
            }
            return true;
        }
  
        /// <summary>
        /// used to release files in the buffer to get more free space
        /// the strategy now is  LRU
        /// </summary>
        /// <returns></returns>
        private bool ReleaseFileLRU(long fileLength)
        {
            if(LruList.Count!=0)
            {
                string url = LruList.Last.Value;
                return RemoveFileInBuffer(url,fileLength);
            }
            else
            {
                Logger.GetLogger().Info("There is no file to release");
                return false;
            }
        }
        /// <summary>
        /// used to remove the files in the buffer
        /// </summary>
        /// <param name="url"></param>
        /// <returns>when the file is not stored in the buffer,return false</returns>
        private bool RemoveFileInBuffer(string url)
        {
            lock (LockRDConflict)//avoid the conflict when read the file which has been deleted
            { 
                if(!LruList.Remove(url))//when the url is not in the LruList
                {
                    return false;
                }
                string pageString = urlToPageDic[url];
                urlToPageDic.Remove(url);
                fileLengthDictionary.Remove(url);
                string[] pageNumStr = pageString.Split(',');
                int pageNumInt = 0;
                for (int i = 0; i < pageNumStr.Length; ++i)
                {
                    pageNumInt = Convert.ToInt32(pageNumStr[i]);
                    freePageStack.Push(pageNumInt);//release the buffer
                    totalFreeSpace += pageSize; //get the new size of FreeSpace
                }
                return true;
            }
        }
        /// <summary>
        /// used to avoid too much delete caused by multithreading
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileLength"></param>
        /// <returns></returns>
        private bool RemoveFileInBuffer(string url,long fileLength)
        {
            lock (LockRDConflict)//avoid the conflict when read the file which has been deleted
            {
                if (totalFreeSpace>fileLength | !LruList.Remove(url))//when the url is not in the LruList
                {
                    return false;
                }
                string pageString = urlToPageDic[url];
                urlToPageDic.Remove(url);
                fileLengthDictionary.Remove(url);
                string[] pageNumStr = pageString.Split(',');
                int pageNumInt = 0;
                for (int i = 0; i < pageNumStr.Length; ++i)
                {
                    pageNumInt = Convert.ToInt32(pageNumStr[i]);
                    freePageStack.Push(pageNumInt);//release the buffer
                    totalFreeSpace += pageSize; //get the new size of FreeSpace
                }
                return true;
            }
        }

        /// <summary>
        /// read file from the buffer & local file system
        /// if the file is read success ,statusCode will reutrn 200
        /// else return 404
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Byte[] readFile(string url,ref int statusCode)
        {
            if(!isRun)
            {
                Logger.GetLogger().Info("please run the filebuffer before reading file");
                return null;
            }
            if(!isUrlEffective(url))
            {
                statusCode = 404;
                Logger.GetLogger().Info(url+" is not an effective url");
                return null;
            }
            if(fileLengthDictionary.ContainsKey(url))//if the file was exsited in filebuffer
            {
                lock(LockRDConflict)
                {
                    if(!fileLengthDictionary.ContainsKey(url))//when the delete part has delete the file in the buffer,
                        //due to  the wait the last contain is not effective now,we need to make sure if the file exist or not again
                    {
                        Byte[] readBuffer = FileSystem.GetInstance().readFile(url);
                        SaveFile(readBuffer, url);//save the file to fileBuffer in memory
                        statusCode = 200;
                        return readBuffer;
                    }
                    long fileLength = fileLengthDictionary[url];    
                    Byte[] fileByteStream = new Byte[fileLength];
                    string pageString = urlToPageDic[url];
                    string[] pageNumStr = pageString.Split(',');
                    int pageNumInt = 0;
                    int fileOffset = 0;
                    for (int i = 0; i < pageNumStr.Length; ++i)
                    {
                        pageNumInt = Convert.ToInt32(pageNumStr[i]);
                        if (fileLength - fileOffset >= pageSize)
                        {
                            System.Buffer.BlockCopy(memoryBuffer, pageNumInt * pageSize, fileByteStream, (int)fileOffset, pageSize);
                        }
                        else
                        {
                            System.Buffer.BlockCopy(memoryBuffer, pageNumInt * pageSize, fileByteStream, (int)fileOffset, (int)(fileLength - fileOffset));
                        }
                        fileOffset += pageSize;
                    }
                    //to realize LRU,the file which was read need to be put into the head of the list
                    LinkedListNode<string> readNode = LruList.Find(url);
                    LruList.Remove(url);
                    LruList.AddFirst(readNode);
                    statusCode = 200;
                    return fileByteStream;
                }
            }
            else //when the file is not in fileBuffer,read the file from local fileSystem
            {
                Byte[] readBuffer=FileSystem.GetInstance().readFile(url);
                if (readBuffer.Length != 0) //in test,we find that some file's lengths could be zero 
                {
                    SaveFile(readBuffer, url);//save the file to fileBuffer in memory
                }
                statusCode = 200;
                return readBuffer;
            }
        }

        private bool isUrlEffective(string url)
        {
            if(url.Contains("..")) 
            {
                return false;
            }
            string path=Properties.FileManagerSettings.Default.ServerDirectory + url;
            return File.Exists(path);
        }
        /// <summary>
        /// used by FileWatcher when there was file deleted
        /// </summary>
        /// <param name="url"></param>
        internal void OnFileChanged(string url)
        {
            if(urlToPageDic.ContainsKey(url))
            {
                RemoveFileInBuffer(url);
            }
        }

        internal void OnFileOrDirectoryDeleted(string url)
        {
            if(urlToPageDic.ContainsKey(url))//when a file was deleted
            {
                RemoveFileInBuffer(url);
                return;
            }
            else
            {
                //when a directory was deleted or the file deleted was not included in the buffer
               string[] keys=new string[urlToPageDic.Count];
               urlToPageDic.Keys.CopyTo(keys,0);
               foreach (string key in keys)
               {
                   //when the forward component was same with url ,it was in the directory
                   if (key.Substring(0, url.Length) == url)
                   {
                       RemoveFileInBuffer(key);
                   }
               }
                
            }
           
        }
    }
}
