using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HttpParser
{
    class ContentTypeConverrter
    {

        private static volatile ContentTypeConverrter instance;
        private static object syncRoot = new Object();
        private Dictionary<string,string> typeDictionary = new Dictionary<string, string>(); 
        private ContentTypeConverrter()
        {
            XDocument doc = XDocument.Load("ConentTypeList.xml");

            var types = doc.Descendants("ContentType");

            foreach (var type in types)
            {
                typeDictionary.Add(type.Element("Extension").Value, type.Element("Type").Value);
            }
        }

        public static ContentTypeConverrter Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ContentTypeConverrter();
                    }
                }

                return instance;
            }
        }

        public string GetType(string url)
        {
            int index = url.LastIndexOf(".");
            string extension = "default";
            string type = "";

            if(index != -1 && index < url.Length-1)
                extension = url.Substring(index+1);

            if (typeDictionary.TryGetValue(extension, out type))
                return type;

            if(typeDictionary.TryGetValue("default", out type))
                return type;

            return "";
        }

    }
}
