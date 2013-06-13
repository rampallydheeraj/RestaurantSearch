using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Diagnostics;
using EeekSoft.Asynchronous;

namespace yumi
{
    class Zagat : ISearchEngine
    {
        private int code;
        private String Name;
        //private int maxNumResults = 30;
        //private string defaultLoc = "%2C Chicago";
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        //private List<string> cuisine_keys;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);
        private string queryStr;
        private static CookieContainer cookieContainer = new CookieContainer();

        public Zagat(int c, String n)
        {
            code = c;
            Name = n;
            location_crosswalk = new Dictionary<string, string>();
            cuisine_crosswalk = new Dictionary<string, string>();
            GetLocationCrosswalkFromXml();
            GetCuisineCrosswalkFromXml();

            //string pageSource;
            ////string formUrl = "https://www.zagat.com/account/signin.aspx?HID=signin_top_left_ns&SignInKey=HeaderSignInLink&RURL=http://www.zagat.com/index.aspx";
            //string formUrl = "https://www.zagat.com/login";

            //CookieContainer cookieContainer = new CookieContainer();
            //try
            //{
            //    HttpWebRequest webRequest = WebRequest.Create(formUrl) as HttpWebRequest;
            //    webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";
            //    webRequest.CookieContainer = CreateCookies();

            //    StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
            //    string responseData = responseReader.ReadToEnd();

            //    //Trace.WriteLine(responseData);


            //    responseReader.Close();          // extract the viewstate value and build out POST data         
            //    string formBuildID = ExtractFormBuildID(responseData);
            //    //string eventValidation = ExtractEventValidation(responseData);

            //    Trace.WriteLine("BUILDID: " + formBuildID);


            //    //string formParams = string.Format("VAM_GROUP={0}&ctl00$MainMasterPageContentHolder$ucLogin$txtEmail={1}&ctl00$MainMasterPageContentHolder$ucLogin$txtPassword={2}&__EVENTVALIDATION={3}&__VIEWSTATE={4}&ctl00$MainMasterPageContentHolder$ucLogin$btnLogIn.x={5}&ctl00$MainMasterPageContentHolder$ucLogin$btnLogIn.y={6}", "", "bbeirne3@yahoo.com", "yumizagat", eventValidation, viewState, "2", "1");
            //    string formParams = string.Format("name={0}&pass={1}&form_build_id={2}&form_id={3}", "bbeirne3@yahoo.com", "yumizagat", formBuildID, "user_login");
            //    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(formUrl);
            //    req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";

            //    req.CookieContainer = cookieContainer;
            //    req.ContentType = "application/x-www-form-urlencoded";
            //    req.Method = "POST";
            //    req.AllowAutoRedirect = true;
            //    req.MaximumAutomaticRedirections = 10;
            //    byte[] bytes = Encoding.ASCII.GetBytes(formParams);
            //    req.ContentLength = bytes.Length;
            //    using (Stream os = req.GetRequestStream())
            //    {
            //        os.Write(bytes, 0, bytes.Length);
            //    }
            //    WebResponse resp = req.GetResponse();
            //    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            //    {
            //        pageSource = sr.ReadToEnd();
            //    }
            //    //Trace.WriteLine(pageSource);

            //    string getUrl = "http://www.zagat.com/Search/AdvancedResults.aspx?Nf=LatLong|GCLT+41.849998,-87.650001+45&N=120&VID=8&Neighborhoods=24150&Ns=Name";
            //    //string getUrl = "http://www.zagat.com/";
            //    //string getUrl = "http://www.zagat.com/search?text=loop&where[name]=Chicago,+IL&where[id]=4293471866&where[lat]=41.849998&where[lon]=-87.650001&where[locale]=Chicago&where[radius]=45";
            //    //string getUrl = "http://www.zagat.com/search?text=lakeview+pizza&where[name]=Chicago%2C+IL";
            //    req = (HttpWebRequest)HttpWebRequest.Create(getUrl);
            //    req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";
            //    req.CookieContainer = CreateCookies();

            //    //Cookie perPage = new Cookie("STNM_PG", "PG=8:50&STNM=chicago-milwaukee", "/", "www.zagat.com");
            //    //req.CookieContainer.Add(perPage);
            //    //req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";
            //    req.Method = "GET";
            //    HttpWebResponse getResponse = (HttpWebResponse)req.GetResponse();

            //    using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            //    {
            //        pageSource = sr.ReadToEnd();
            //    }
            //    Trace.WriteLine(pageSource);
            //    if (pageSource.Contains("Brian"))
            //        Trace.WriteLine("LOGGED IN");
            //    else
            //        Trace.WriteLine("NOT LOGGED IN");
            //}
            //catch (WebException e)
            //{
            //}
            //using (WebClient client = new WebClient())
            //{
            //    client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";
            //    // Download data.
            //    //byte[] arr = client.DownloadData("http://www.zagat.com/search?text=lakeview+pizza&where[name]=Chicago%2C+IL");
            //    byte[] arr = client.DownloadData("http://www.zagat.com/Search/AdvancedResults.aspx?Nf=LatLong|GCLT+41.849998,-87.650001+45&N=120&VID=8&Neighborhoods=24150&Ns=Name");

            //    // Write values.
            //    Trace.WriteLine("--- WebClient result ---");
            //    Trace.WriteLine(arr.Length);
            //}

        }

        public int getCode
        {
            get { return code; }
            //return code;
        }

        public string getName
        {
            get { return Name; }
            //return Name;
        }

        public int getConstraints
        {
            get { return constraints; }

        }

        public void GetLocationCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/Zagat_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Zagat_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                //if (!c.Name.Equals("Near North") && !c.Name.Equals("North") && !c.Name.Equals("Northwest") && !c.Name.Equals("South") && !c.Name.Equals("West"))
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/Zagat_Cuisine_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Zagat_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }

        }

        public string BaseRestaurantUrl
        {
            //get { return "http://www.zagat.com/Search/AdvancedResults.aspx?Nf=LatLong|GCLT+41.849998,-87.650001+45&N=120&VID=8"; }
            get { return "http://www.zagat.com/search?text="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {

            string LocKey;
            string CuiKey;
            List<String> queryURLs = new List<String>();

            foreach (string location in locations)
            {
                if (locations[0].Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = location;
                    LocKey = LocKey.Replace(' ', '+');
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    if (!LocKey.Equals(""))
                        CuiKey = "+" + cuisine;
                    else
                        CuiKey = cuisine;
                }

                if (!keyword.Equals(""))
                {
                    if (!LocKey.Equals("") || !CuiKey.Equals(""))
                        keyword = "+" + keyword;
                }
                
                //queryStr = BaseRestaurantUrl + CuiKey + LocKey + keyword + "&Ns=Name";
                queryStr = BaseRestaurantUrl + LocKey + CuiKey + keyword + "&where[name]=Chicago,+IL&where[id]=&where[lat]=41.8781136&where[lon]=-87.6297982&where[locale]=Chicago&where[radius]=8&sliders[food]=0&sliders[decor]=0&sliders[service]=0";
                queryURLs.Add(queryStr);
            }
            

            DownloadAll(queryURLs).ExecuteAndWait();

            //for (int i = queryURLs.Count; i < Globals.zagatData.Count; i++)
            //{
            //    Globals.zagatData[i].Ranking = i - Globals.zagatTotalQueries + 1;
            //}
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            string LocKey;
            string CuiKey;
            List<String> queryURLs = new List<String>();

            
            if (location.Equals("All"))
            {
                LocKey = "";
            }
            else
            {
                LocKey = location;
                LocKey = LocKey.Replace(' ', '+');
            }

            if (cuisine.Equals("All"))
            {
                CuiKey = "";
            }
            else
            {
                if (!LocKey.Equals(""))
                    CuiKey = "+" + cuisine;
                else
                    CuiKey = cuisine;
            }

            if (!keyword.Equals(""))
            {
                if (!LocKey.Equals("") || !CuiKey.Equals(""))
                    keyword = "+" + keyword;
            }

            //queryStr = BaseRestaurantUrl + CuiKey + LocKey + keyword + "&Ns=Name";
            queryStr = BaseRestaurantUrl + LocKey + CuiKey + keyword + "&where[name]=Chicago,+IL&where[id]=&where[lat]=41.8781136&where[lon]=-87.6297982&where[locale]=Chicago&where[radius]=8&sliders[food]=0&sliders[decor]=0&sliders[service]=0";
            queryURLs.Add(queryStr);
            

            string response = WebDocumentRetriever.GetDocument(queryStr, null);
            List<Restaurant> restaurants = new List<Restaurant>();

            Restaurant run_url = new Restaurant();
            run_url.Name = queryStr;
            run_url.ZipCode = "99999";

            if (response.Contains("YUMI_WEB_ERROR: "))
            {
                //Restaurant error_url = new Restaurant();
                //error_url.Name = response;
                //error_url.ZipCode = "99999";
                restaurants.Add(run_url);
                //restaurants.Add(error_url);
                Globals.webErrors = Globals.webErrors + response + "\n";
                return restaurants;
            }



            processRequestForPage(response, restaurants, location);

            restaurants.Insert(0, run_url);
            //if (restaurants.Count == 1)
            //{
            //    Restaurant message = new Restaurant();
            //    message.Name = "NO RESULTS FOR THIS REQUEST";
            //    message.ZipCode = "99999";
            //    restaurants.Add(message);
            //}

            return restaurants;
        }

        public void processRequestForPage(string response, List<Restaurant> restaurants, string location)
        {
            response = response.Replace("\t", "");
            response = response.Replace("\n", "");
            //Trace.WriteLine(response);

            Regex regRecords = new Regex("<li class=\"result zr.*?</li>");
            MatchCollection records = regRecords.Matches(response);
            Trace.WriteLine("Match Records: " + records.Count);

            for (int i = 0; i < records.Count; i++)
            {
                Trace.WriteLine(records[i].Value);
                Trace.WriteLine("----------------------------------------------------");



                Restaurant restaurant = new Restaurant();
                Match record = records[i];

                Regex nameRecord = new Regex("<h3>.*?</h3>");
                Match nameMatch = nameRecord.Match(record.Value);
                //Trace.WriteLine(nameMatch.Value);

                //get name
                string name = nameMatch.Value.Substring(nameMatch.Value.IndexOf("\">") + 2,
                                                     nameMatch.Value.IndexOf("</a>") - nameMatch.Value.IndexOf("\">") - 2);
                //Trace.WriteLine(name);

                restaurant.Name = name;

                Regex addressRecord = new Regex("address\">.*?</span>");
                Match addressMatch = addressRecord.Match(record.Value);
                //Trace.WriteLine(addressMatch.Value);

                //get address
                string address = addressMatch.Value.Substring(addressMatch.Value.IndexOf("address\">") + 9,
                                                        addressMatch.Value.IndexOf("(") - addressMatch.Value.IndexOf("address\">") - 9).TrimEnd();
                //Trace.WriteLine(address);

                ////get city
                string city = addressMatch.Value.Substring(addressMatch.Value.IndexOf("<br />") + 6,
                                                        addressMatch.Value.IndexOf("<br /></span>") - addressMatch.Value.IndexOf("<br />") - 6);
                //Trace.WriteLine(city);

                restaurant.FullAddress = address + ", " + city;
                restaurant.Address = address;
                restaurant.City = city;

                string review = "";
                if (record.Value.Contains("<dd>"))
                    review = record.Value.Substring(record.Value.IndexOf("<dd>") + 4, record.Value.IndexOf("%</dd>") - record.Value.IndexOf("<dd>") - 4);
                //Trace.WriteLine(review);

                restaurant.Ranking = i + 1;
                

                Globals.zagatData.Add(restaurant);
            }  


        }

        static IEnumerable<IAsync> DownloadAll(List<string> queryURLs)
        {
            if (queryURLs.Count == 1)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0])
                   );
                yield return methods;
            }
            else if (queryURLs.Count == 2)
            {

                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 3)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2])
                    );
                yield return methods;
            }
            
            else
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3])
                    );
                yield return methods;
            }

            for (int i = 0; i < queryURLs.Count; i++)
            {
                Restaurant run_url = new Restaurant();
                run_url.ZipCode = "99999";
                run_url.Name = queryURLs[i];
                Globals.zagatData.Insert(0, run_url);
            }

            Globals.zagatTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {
            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";
            run_url.Ranking = 0;

            //WebRequest req = HttpWebRequest.Create(url);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.13 (KHTML, like Gecko) Chrome/9.0.597.98 Safari/534.13";
            //req.CookieContainer = cookieContainer;
            //Cookie perPage = new Cookie("STNM_PG", "PG=8:50&STNM=chicago-milwaukee", "/", "www.zagat.com");
            //req.CookieContainer.Add(perPage);
            //req.CookieContainer.SetCookies(req.RequestUri, "STNM_PG=PG=8:400&STNM=chicago-milwaukee");

            // asynchronously get the response from http server
            Async<WebResponse> webresp = null;

            try
            {
                webresp = req.GetResponseAsync();
            }
            catch (WebException e)
            {
                Trace.WriteLine("In Zagat catch block");
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: ZAGAT" + e.Message;
                error_url.ZipCode = "99999";
                Globals.zagatData.Add(run_url);
                Globals.zagatData.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }
            yield return webresp;

            //Trace.WriteLine("[{0}] got response", url);
            //Stream resp = webresp.Result.GetResponseStream();

            Stream resp;
            if (webresp.Result != null)
                resp = webresp.Result.GetResponseStream();
            else
            {
                Trace.WriteLine("ZAGAT WEB ERROR");
                Trace.WriteLine(webresp);
                yield break;
            }

            // download HTML using the asynchronous extension method
            // instead of using synchronous StreamReader
            Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            yield return html;

            string response = html.Result;
            response = response.Replace("\t", "");
            response = response.Replace("\n", "");
            //Trace.WriteLine(response);

            Regex regRecords = new Regex("<li class=\"result zr.*?</li>");
            MatchCollection records = regRecords.Matches(response);
            Trace.WriteLine("Match Records: " + records.Count);

            for (int i = 0; i < records.Count; i++)
            {
                Trace.WriteLine(records[i].Value);
                Trace.WriteLine("----------------------------------------------------");



                Restaurant restaurant = new Restaurant();
                Match record = records[i];

                Regex nameRecord = new Regex("<h3>.*?</h3>");
                Match nameMatch = nameRecord.Match(record.Value);
                //Trace.WriteLine(nameMatch.Value);

                //get name
                string name = nameMatch.Value.Substring(nameMatch.Value.IndexOf("\">") + 2,
                                                     nameMatch.Value.IndexOf("</a>") - nameMatch.Value.IndexOf("\">") - 2);
                //Trace.WriteLine(name);

                restaurant.Name = name;

                Regex addressRecord = new Regex("address\">.*?</span>");
                Match addressMatch = addressRecord.Match(record.Value);
                //Trace.WriteLine(addressMatch.Value);

                //get address
                string address = addressMatch.Value.Substring(addressMatch.Value.IndexOf("address\">") + 9,
                                                        addressMatch.Value.IndexOf("(") - addressMatch.Value.IndexOf("address\">") - 9).TrimEnd();
                //Trace.WriteLine(address);

                ////get city
                string city = addressMatch.Value.Substring(addressMatch.Value.IndexOf("<br />") + 6,
                                                        addressMatch.Value.IndexOf("<br /></span>") - addressMatch.Value.IndexOf("<br />") - 6);
                //Trace.WriteLine(city);

                restaurant.FullAddress = address + ", " + city;
                restaurant.Address = address;
                restaurant.City = city;
                
                string review = "";
                if (record.Value.Contains("<dd>"))
                    review = record.Value.Substring(record.Value.IndexOf("<dd>") + 4, record.Value.IndexOf("%</dd>") - record.Value.IndexOf("<dd>") - 4);
                //Trace.WriteLine(review);

                restaurant.Ranking = i + 1;
                //if (!record.Value.Contains("<h6>"))
                //    review = record.Value.Substring(record.Value.IndexOf("znum") + 27, record.Value.IndexOf("</span></p>") - record.Value.IndexOf("znum") - 27);
                //else
                //    review = record.Value.Substring(record.Value.IndexOf("<h6>Food") + 27, record.Value.IndexOf("</span></p>") - record.Value.IndexOf("<h6>Food") - 27);
                ////Trace.WriteLine(review);
                //if (review.Equals("-"))
                //    review = "0";
                //int reviewNum = Int32.Parse(review);
                //if (reviewNum == 0)
                //    restaurant.Rating = 0;
                //else if (reviewNum <= 3)
                //    restaurant.Rating = 0.5;
                //else if (reviewNum <= 6)
                //    restaurant.Rating = 1;
                //else if (reviewNum <= 9)
                //    restaurant.Rating = 1.5;
                //else if (reviewNum <= 12)
                //    restaurant.Rating = 2;
                //else if (reviewNum <= 15)
                //    restaurant.Rating = 2.5;
                //else if (reviewNum <= 18)
                //    restaurant.Rating = 3;
                //else if (reviewNum <= 21)
                //    restaurant.Rating = 3.5;
                //else if (reviewNum <= 24)
                //    restaurant.Rating = 4;
                //else if (reviewNum <= 27)
                //    restaurant.Rating = 4.5;
                //else if (reviewNum <= 30)
                //    restaurant.Rating = 5;

                Globals.zagatData.Add(restaurant);
            }
        }

        

        private static CookieContainer CreateCookies()
        {
            //Cookie location = new Cookie("UGLC", "D=Chicago&GS=GeoChildID|969&TSP=7%2f19%2f2010+8%3a04%3a27+PM&V=|32|27|11|8", "/", "www.zagat.com");
            Cookie location = new Cookie("UGLC", "D=Chicago+Metro+Area&LL=41.849998%2c-87.650001&R=45&ZDID=2&TSP=2%2f8%2f2011+4%3a43%3a21+PM&V=|32|27|11|8", "/", "www.zagat.com");

            //Cookie ZL = new Cookie("ZL", "chicago", "/", "www.zagat.com");
            //Cookie zLAP = new Cookie("zLAP", "%2Fsearch", "/", "www.zagat.com");
            //Cookie USession = new Cookie("USession", "a0ddc06bc8b54b94825ad6885748bd01", "/", "www.zagat.com");
            //Cookie ZLOC = new Cookie("ZLOC", "%5B%7B%22EID%22%3A4293471866%2C%22N%22%3A%22Chicago%2C%20IL%22%2C%22LCL%22%3A%22Chicago%22%2C%22LCLF%22%3A%22chicago%22%7D%5D", "/", "www.zagat.com");
            CookieContainer container = new CookieContainer();
            container.Add(location);
            //container.Add(USession);
            //container.Add(ZL);
            //container.Add(zLAP);
            //container.Add(ZLOC);

            return container;
        }

        private string ExtractFormBuildID(string s)
        {
            string viewStateNameDelimiter = "form_build_id";
            string valueDelimiter = "id=\"";
            int viewStateNamePosition = s.IndexOf(viewStateNameDelimiter);
            int viewStateValuePosition = s.IndexOf(valueDelimiter, viewStateNamePosition);
            int viewStateStartPosition = viewStateValuePosition + valueDelimiter.Length;
            int viewStateEndPosition = s.IndexOf("\"", viewStateStartPosition);
            return HttpUtility.UrlEncodeUnicode(s.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition));
        }

        private string ExtractEventValidation(string s)
        {
            string viewStateNameDelimiter = "__EVENTVALIDATION";
            string valueDelimiter = "value=\"";
            int viewStateNamePosition = s.IndexOf(viewStateNameDelimiter);
            int viewStateValuePosition = s.IndexOf(valueDelimiter, viewStateNamePosition);
            int viewStateStartPosition = viewStateValuePosition + valueDelimiter.Length;
            int viewStateEndPosition = s.IndexOf("\"", viewStateStartPosition);
            return HttpUtility.UrlEncodeUnicode(s.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition));
        }
    }
}

       