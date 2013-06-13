using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;

namespace yumi
{ 
    public static class WebDocumentRetriever
    {
        //simple web request, tired of typing this out                
        public static string GetDocument(string url, CookieContainer cookie)
        {
            Console.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;
            StreamReader reader = null;

            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7 GTB6 (.NET CLR 3.5.30729)";
            //request.KeepAlive = false;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
            }
            catch (WebException e)
            {
                return "YUMI_WEB_ERROR: " + e.Message;
            }

            try
            {
                return (reader.ReadToEnd());
            }
            catch (WebException e)
            {
                return "YUMI_WEB_ERROR: " + e.Message;
            }
        }

        public static string GetDocumentWithTimeout(string url, CookieContainer cookie)
        {
            Console.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;
            StreamReader reader = null;
            request.ReadWriteTimeout = 6000;

            if (cookie != null)
            {
                request.CookieContainer = cookie;
            }

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7 GTB6 (.NET CLR 3.5.30729)";
            //request.KeepAlive = false;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
            }
            catch (WebException e)
            {
                return "YUMI_WEB_ERROR: " + e.Message;
            }

            try
            {
                return (reader.ReadToEnd());
            }
            catch (WebException e)
            {
                return "YUMI_WEB_ERROR: " + e.Message;
            }
        }

        public static string GetDocument(string url)
        {
            Console.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response;
            StreamReader reader = null;
                        
            request.Method = "GET";
            

            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.1.7) Gecko/20091221 Firefox/3.5.7 GTB6 (.NET CLR 3.5.30729)";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
            }
            catch (WebException e)
            {
                return "YUMI_WEB_ERROR: " + e.Message;   
            }
                      
            return (reader.ReadToEnd());
        }

        //private static CookieContainer CreateCookies()
        //{
        //    Cookie location = new Cookie("UGLC", "D=Chicago&GS=GeoChildID|969&TSP=7%2f19%2f2010+8%3a04%3a27+PM&V=|32|27|11|8", "/", "www.zagat.com");
        //    CookieContainer container = new CookieContainer();
        //    container.Add(location);
        //    return container;
        //}
    }


}