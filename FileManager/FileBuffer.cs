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

        Stack<int> freePageStack;//the page which has been 
        Dictionary<string, string> urlToPageDic;//used to store the pageNums of the file in the buffer 
        LinkedList<string> LruList;//to relize LRU 
        Dictionary<string, long> fileLengthDictionary;//store the length of the file store in the buffer

        private static FileBuffer localFileBuffer=new FileBuffer();
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
            fileLengthDictionary=new Dictionary<string,long>();

        }

        public static FileBuffer GetInstance()
        {
            return localFileBuffer;
        }
        
        /// <summary>
        /// when a new file is read from local file system, the function will be called to store it in the buffer
        /// </summary>
        /// <returns></returns>
        public bool SaveFile(Byte[] fileByteStream,string url)
        {
            long fileLength = fileByteStream.Length;
            if(totalFreeSpace<fileLength)
            {
                if(fileLength>bufferSize) 
                {
                    Logger.GetLogger().Info("The file is to big to buffer it");
                    return false;
                }
                Logger.GetLogger().Info("There is not enough space in the fileBuffer.We'll try to release some older file");
                while(totalFreeSpace<fileLength)
                {
                   if(!ReleaseFileLRU()) // while there is no file to realse ,return false
                   {
                       return false;
                   }
                }
                SaveFile(fileByteStream, url);
            }
            else
            {
                long fileOffset=0;
                LruList.AddFirst(url);
                fileLengthDictionary.Add(url, fileLength);
                string urlPages = "";  
                int pageNum=0;
                while(fileOffset<fileLength)
                {
                    if(freePageStack.Count>0)//while there is page while has been released
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
            return true;
        }
  
        /// <summary>
        /// used to release files in the buffer to get more free space
        /// the strategy now is  LRU
        /// </summary>
        /// <returns></returns>
        private bool ReleaseFileLRU()
        {
            if(LruList.Count!=0)
            {
                string url = LruList.Last.Value;
                LruList.RemoveLast();
                string pageString = urlToPageDic[url];
                urlToPageDic.Remove(url);
                fileLengthDictionary.Remove(url);
                string[] pageNumStr = pageString.Split(',');
                int pageNumInt=0;
                for(int i=0;i<pageNumStr.Length;++i)
                {
                    pageNumInt = Convert.ToInt32(pageNumStr[i]);
                    freePageStack.Push(pageNumInt);//release the buffer
                    totalFreeSpace += pageSize; //get the new size of FreeSpace
                }
                Logger.GetLogger().Info(url+" has been delete from the fileBuffer");
                return true;
            }
            else
            {
                Logger.GetLogger().Info("There is no file to release");
                return false;
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
            if(!isUrlEffective(url))
            {
                statusCode = 404;
                Logger.GetLogger().Info(url+" is not an effective url");
                return null;
            }
            if(fileLengthDictionary.ContainsKey(url))//if the file was exsited in filebuffer
            {
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
                LinkedListNode<string> readNode=LruList.Find(url);
                LruList.Remove(url);
                LruList.AddFirst(readNode);
                statusCode = 200;
                return fileByteStream;
          
            }
            else //when the file is not in fileBuffer,read the file from local fileSystem
            {
                Byte[] readBuffer=FileSystem.GetInstance().readFile(url);
                SaveFile(readBuffer, url);//save the file to fileBuffer in memory
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
    }
}
