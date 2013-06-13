using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace yumi
{
    public static class WebDocumentRetrieverProxy
    {
        private static List<string> proxyList = null;  
         
        public static StreamReader make_request(string url)
        {
            StreamReader reader = null;
            int proxyIndex = 0;

            if (proxyList == null)
                getProxiesFromFile();

            do
            {
                Uri newUri = new Uri(proxyList[proxyIndex]);
                try
                {                    
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3 ( .NET CLR 3.5.30729)";
                    request.Method = "GET";                    
                    //request.Credentials = CredentialCache.DefaultCredentials;
                    //request.AllowAutoRedirect = true;
                    //request.KeepAlive = false;

                    WebProxy mywebproxy = new WebProxy();
                    mywebproxy.Address = newUri;
                    mywebproxy.Credentials = new NetworkCredential("", "", "");                    
                    request.Proxy = mywebproxy;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    reader = new StreamReader(response.GetResponseStream());
                }
                catch
                {
                    Console.WriteLine("Timeout " + proxyIndex);                    
                    proxyIndex = proxyIndex + 1;
                    if (proxyIndex >= proxyList.Count)
                    {
                        Thread.Sleep(20000);
                        proxyIndex = 0;
                    }
                }

            } while (reader == null);
              
            return reader;
        }
        
        public static string GetDocument(string url)
        {
            StreamReader reader;
            
            string response = "";
            do
            {
                reader = make_request(url);
                if (reader == null)
                {
                    Thread.Sleep(3000);
                }
            //} while (reader == null || (response = reader.ReadToEnd()).Contains("CoDeeN"));
            } while (reader == null || response.Contains("CoDeeN"));
            return response;
        }

        private static void getProxiesFromFile()
        {
            //reading proxies from file
            proxyList = new List<string>();

            String proxyFile = "txt/proxies.txt";
            StreamReader fileReader = new StreamReader(proxyFile);

            String line = fileReader.ReadLine();

            while (line != null)
            {
                proxyList.Add("http://" + line.Trim() + "/");
                line = fileReader.ReadLine();
            }

            fileReader.Close();
        }
    }
}
