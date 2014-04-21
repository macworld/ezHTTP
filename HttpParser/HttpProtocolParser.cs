using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace HttpParser
{
    public class HttpProtocolParser
    {
        Byte[] rawData;
        Hashtable headers;  
        Hashtable parameters;
        String rawUrl;
        int statusCode;
        long chunkSize;

        public HttpProtocolParser()
        {

        }

        public HttpProtocolParser(Byte[] rawData)
        {
            this.rawData = rawData;
        }

        public void SetRawData(Byte[] rawData)
        {
            this.rawData = rawData;
        }

        public String QueryString(String query)
        {
            return this.parameters[query].ToString();
        }

        public String GetResourceUrl()
        {
            String resourceUrl ;
            if(this.rawUrl.Contains('?'))
                resourceUrl = this.rawUrl.Substring(0, this.rawUrl.LastIndexOf("?") + 1);
            else
                resourceUrl = this.rawUrl;
            if (resourceUrl.EndsWith("/"))
                return Properties.HttpParserSettings.Default.WelcomeFilePath;
            else
                return resourceUrl;
        }

        public bool IsHttpRequest()
        {
            bool isLegal = false;
            String context = Encoding.ASCII.GetString(this.rawData);
            String[] applDataLines = context.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            String[] firstLineWords = applDataLines[0].Split(new String[] { " " }, StringSplitOptions.None);

            if (firstLineWords[0] == "GET")
            {
                this.rawUrl = firstLineWords[1];
                int index = this.rawUrl.LastIndexOf('?');
                int length = this.rawUrl.Length;

                if (index != -1 && index < length - 3)
                {
                    this.parameters = new Hashtable();
                    this.ProcessParameters(this.rawUrl.Substring(index, length - index));
                }
                isLegal = true;
                this.headers = new Hashtable();
            }
            else
            {
                return isLegal;
            }

            for (int i = 1; i < applDataLines.Length; ++i)
            {
                String[] otherLineWords = applDataLines[i].Split(new String[] { ": " }, StringSplitOptions.None);
                if (otherLineWords.Length == 2)
                {
                    this.headers[otherLineWords[0]] = otherLineWords[1];
                }
            }
            return isLegal;
        }

        public void SetStatusCode(int statusCode)
        {
            this.statusCode = statusCode;
        }

        public Byte[] GetWrappedResponse(Byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            DateTime dateTime = DateTime.Now;
            if (this.statusCode == 200)
            {
                stringBuilder.Append("HTTP/1.1 200 OK\r\nDate: ");
                stringBuilder.Append(dateTime.GetDateTimeFormats('r')[0].ToString());
                stringBuilder.Append("\r\n");
                stringBuilder.Append("Server: ezHttp\r\n");
                stringBuilder.Append("Connection: keepalive\r\n");
                if(GetResourceUrl().EndsWith(".svg"))
                    stringBuilder.Append("Content-Type: image/svg+xml" + "; charset=utf8\r\n");
                else
                    stringBuilder.Append("Content-Type: "+ headers["Accept"] + "; charset=utf8\r\n");
                
                //String html = "<html><head><title>解读HTTP包示例</title></head><body>test</br></body></html>\r\n";
                //Byte[] buffer = Encoding.UTF8.GetBytes(html);
                stringBuilder.Append("Content-Length: ");
                stringBuilder.Append(data.Length);

                stringBuilder.Append("\r\n\r\n");

                byte[] firstBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                byte[] lastBytes = Encoding.UTF8.GetBytes("\r\n");
 
                byte[] wrapBytes = new byte[firstBytes.Length + data.Length + lastBytes.Length];
                System.Buffer.BlockCopy(firstBytes, 0, wrapBytes, 0, firstBytes.Length);
                System.Buffer.BlockCopy(data, 0, wrapBytes, firstBytes.Length,  data.Length);
                System.Buffer.BlockCopy(lastBytes, 0, wrapBytes, firstBytes.Length + data.Length, lastBytes.Length);
                return wrapBytes;
            }
            else if (this.statusCode == 404)
            {
                stringBuilder.Append("HTTP/1.1 404 Not Found\r\nDate: ");
                stringBuilder.Append(dateTime.GetDateTimeFormats('r')[0].ToString());
                stringBuilder.Append("\r\nServer: ezHttp\r\nConnection: close\r\nContent-Length: 0\r\n\r\n"); 
            }
            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }


        private void ProcessParameters(String preParameteres)
        {
            String[] parameters = preParameteres.Split(new String[]{"&&"}, StringSplitOptions.None);
            foreach(String parameter in parameters)
            {
                String[] keyValuePair = parameter.Split(new String[]{"="}, StringSplitOptions.None);
                if(keyValuePair.Length == 2)
                    this.parameters[keyValuePair[0]] = keyValuePair[1];
            }
        }
    }
}
