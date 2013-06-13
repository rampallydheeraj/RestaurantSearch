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
    class Metromix : ISearchEngine
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

        public Metromix(int c, String n)
        {
            code = c;
            Name = n;
            location_crosswalk = new Dictionary<string, string>();
            cuisine_crosswalk = new Dictionary<string, string>();
            GetLocationCrosswalkFromXml();
            GetCuisineCrosswalkFromXml();
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
            //TextReader reader = new StreamReader("xml/Metromix_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Metromix_crosswalk.xml"));
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
            //TextReader reader = new StreamReader("xml/Metromix_Cuisine_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Metromix_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys) 
            { 
                cuisine_crosswalk.Add(c.Name, c.Key); 
            }
            
        }

        public string BaseRestaurantUrl
        {            
            //get { return "http://chicago.metromix.com/browse/home/type.listing.venue,restaurant?keywords="; }
            get { return "http://chicago.metromix.com/browse/restaurants"; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }
        
        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            //DateTime startTime = DateTime.Now;

            //int priceInt = 0;
            
            string LocKey;
            string CuiKey;
            string PriKey;
            List<String> queryURLs = new List<String>(); 

            //string queryStr;
            foreach (String location in locations)
            {
                switch (price)
                {
                    case "$":
                        PriKey = "/3,4";
                        //priceInt = 1;
                        break;
                    case "$$":
                        PriKey = "/3,1";
                        //priceInt = 2;
                        break;
                    case "$$$":
                        PriKey = "/3,2";
                        //priceInt = 3;
                        break;
                    case "$$$$":
                        PriKey = "/3,0";
                        //priceInt = 4;
                        break;
                    case "$$$$$":
                        PriKey = "/3,3";
                        //priceInt = 5;
                        break;
                    default:
                        PriKey = "/3,4,1,2,0,3";
                        //priceInt = 0;
                        break;
                }
                if (location.Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = "/1," + location_crosswalk[location];
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    CuiKey = "/0," + cuisine_crosswalk[cuisine];
                }

                //string queryStr = BaseRestaurantUrl + keyword + price_val + "%20" + hoodStr + "&override_for_add=true&sort=rating_desc&page_size=50";
                if (CuiKey.Equals("") && !keyword.Equals(""))
                {
                    foreach (KeyValuePair<string, string> entry in cuisine_crosswalk)
                    {
                        if (entry.Key.Contains('/'))
                        {
                            string[] matchingNameSplit = entry.Key.Split(new char[] { '/' });
                            foreach (string match in matchingNameSplit)
                            {
                                if (Globals.EditDistance(keyword.ToLower().Trim(), match.ToLower().Trim()) >= 0.8)
                                {
                                    CuiKey = "/0," + entry.Value;
                                    break;
                                }
                            }
                            if (!CuiKey.Equals(""))
                                break;
                        }
                        else if (Globals.EditDistance(keyword.ToLower().Trim(), entry.Key.ToLower().Trim()) >= 0.8)
                        {
                            CuiKey = "/0," + entry.Value;
                            break;
                        }
                    }
                }

                if (CuiKey.Equals(""))
                {
                    queryStr = BaseRestaurantUrl + LocKey + PriKey + "?keywords=" + keyword + "&override_for_ads=false&page=1&page_size=29&sort=rating_desc";
                }
                else
                {
                    queryStr = BaseRestaurantUrl + CuiKey + LocKey + PriKey + "?keywords=" + keyword + "&override_for_ads=false&page=1&page_size=29&sort=rating_desc";
                }
                queryURLs.Add(queryStr);
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.metromixLocalDataAllRuns.Count; i++)
            {
                //Trace.WriteLine("yellowpagesTotalQueries: " + Globals.yellowpagesTotalQueries);
                Globals.metromixLocalDataAllRuns[i].Ranking = i - Globals.metromixTotalQueries + 1;
            }
        }

        static IEnumerable<IAsync> DownloadAll(List<string> queryURLs)
        {
            /*
            var methods = Async.Parallel(
                processRequestsAsync(queryURLs[0])
                );
             */

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
            else if (queryURLs.Count == 4)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 5)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 6)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4]),
                    processRequestsAsync(queryURLs[5])
                    );
                yield return methods;
            }

            else if(queryURLs.Count==7)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4]),
                    processRequestsAsync(queryURLs[5]), 
                    processRequestsAsync(queryURLs[6])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 8)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4]),
                    processRequestsAsync(queryURLs[5]),
                    processRequestsAsync(queryURLs[6]),
                    processRequestsAsync(queryURLs[7])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 9)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4]),
                    processRequestsAsync(queryURLs[5]),
                    processRequestsAsync(queryURLs[6]),
                    processRequestsAsync(queryURLs[7]),
                    processRequestsAsync(queryURLs[8])
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 10)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0]),
                    processRequestsAsync(queryURLs[1]),
                    processRequestsAsync(queryURLs[2]),
                    processRequestsAsync(queryURLs[3]),
                    processRequestsAsync(queryURLs[4]),
                    processRequestsAsync(queryURLs[5]),
                    processRequestsAsync(queryURLs[6]),
                    processRequestsAsync(queryURLs[7]),
                    processRequestsAsync(queryURLs[8]),
                    processRequestsAsync(queryURLs[9])
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
            //Globals.WriteOutput("Completed all!");
            //Trace.WriteLine("Completed all!");

            for (int i = 0; i < queryURLs.Count; i++)
            {
                Restaurant run_url = new Restaurant();
                run_url.ZipCode = "99999";
                run_url.Name = queryURLs[i];
                Globals.metromixLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.metromixTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {
            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";

            WebRequest req = HttpWebRequest.Create(url);
            
            // asynchronously get the response from http server
            Async<WebResponse> webresp = null;

            try
            {
                webresp = req.GetResponseAsync();
            }
            catch (WebException e)
            {
                Trace.WriteLine("In Metromix catch block");
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: METROMIX" + e.Message;
                error_url.ZipCode = "99999";
                Globals.metromixLocalDataAllRuns.Add(run_url);
                Globals.metromixLocalDataAllRuns.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }
            yield return webresp;

            //Trace.WriteLine("[{0}] got response", url);
            Stream resp = webresp.Result.GetResponseStream();

            // download HTML using the asynchronous extension method
            // instead of using synchronous StreamReader
            Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            yield return html;

            string response = html.Result;

            string rating = "";
            string[] splitByRestaurants = response.Split(new string[] { "class='results-type'" }, StringSplitOptions.None);
            Console.WriteLine(splitByRestaurants.Length);
            //using (StreamWriter writer = new StreamWriter("xml/Output.txt", true))
            //{
            //    writer.WriteLine(splitByRestaurants.Length);
            //}


            if (splitByRestaurants.Length > 1)
            {
                try
                {
                    for (int i = 1; i < splitByRestaurants.Length; i++)
                    {
                        string markup = splitByRestaurants[i];
                        //using (StreamWriter writer = new StreamWriter("xml/Output.txt", true))
                        //{
                        //    writer.WriteLine(markup);
                        //}
                        string entryType = markup.Substring(markup.IndexOf(" / ") + 3, markup.IndexOf("</h4>") - markup.IndexOf(" / ") - 3);
                        //Trace.WriteLine(entryType);
                        if (entryType.Equals("Class") || entryType.Equals("Dining Event") || entryType.Equals("Deal or Special"))
                            continue;
                        string[] markupTemp;
                        markupTemp = markup.Split(new string[] { "class='results-details" }, StringSplitOptions.None);
                        markup = markupTemp[1];
                        Regex regexNameFirst = new Regex("<a.*</a>");
                        Regex regexNameSecond = new Regex(">.*</");

                        string name = regexNameSecond.Match(regexNameFirst.Match(markup).Value).Value.Replace(">", string.Empty).Replace("</", string.Empty);

                        //get address
                        string markupForAddress = markup.Substring(0, markup.IndexOf("</h5>") + 5);
                        //Trace.WriteLine("markupForAddress: " + markupForAddress);
                        //Regex regexAddress = new Regex("<h5>[^(]*", RegexOptions.Singleline);
                        Regex regexAddress = new Regex("<h5>.*</h5>", RegexOptions.Singleline);
                        string addressTemp = regexAddress.Match(markupForAddress).Value.Replace("<h5>", string.Empty).Replace("</h5>", string.Empty).Trim();
                        //Trace.WriteLine("addressTemp: " + addressTemp);
                        string[] addressArr = addressTemp.Split(new char[] { '-' });
                        string finalAddress = addressTemp;

                        Restaurant r = new Restaurant();
                        if (addressArr.Length > 1)
                        {
                            //Trace.WriteLine(addressArr.Length);
                            //Trace.WriteLine(addressArr[1]);
                            if (addressArr.Length == 3)
                            {
                                r.Address = addressArr[0].Trim() + "-" + addressArr[1].Trim();
                                finalAddress = addressArr[0].Trim() + "-" + addressArr[1].Trim() + ", " + addressArr[2].Substring(0, addressArr[2].IndexOf("(") - 1) + ", IL";
                                r.City = addressArr[2].Substring(0, addressArr[2].IndexOf("(") - 1).Trim();
                                r.Neighborhood = addressArr[2].Substring(addressArr[2].IndexOf("(") + 1, addressArr[2].IndexOf(")") - addressArr[2].IndexOf("(") - 1).Trim();
                            }
                            else
                            {
                                r.Address = addressArr[0].Trim();
                                finalAddress = addressArr[0].Trim() + ", " + addressArr[1].Substring(0, addressArr[1].IndexOf("(") - 1) + ", IL";
                                r.City = addressArr[1].Substring(0, addressArr[1].IndexOf("(") - 1).Trim();
                                r.Neighborhood = addressArr[1].Substring(addressArr[1].IndexOf("(") + 1, addressArr[1].IndexOf(")") - addressArr[1].IndexOf("(") - 1).Trim();
                            }
                        }
                        else
                            continue;

                        finalAddress = finalAddress.Replace(", A", "-A ");
                        //Restaurant r = new Restaurant();

                        //using (StreamWriter writer = new StreamWriter("xml/Output.txt", true))
                        //{
                        //    writer.WriteLine(name);
                        //    writer.WriteLine(finalAddress);
                        //}
                        if (finalAddress.Equals(""))
                            continue; //no address skip restaurant

                        Regex regexRating = new Regex("/images/stars/yellow/large.*?png");
                        rating = regexRating.Match(markup).Value;
                        rating = rating.Substring(rating.IndexOf("large") + 6, rating.IndexOf("png") - rating.IndexOf("large") - 7);
                        r.Rating = Convert.ToDouble(rating);
                        if (r.Rating > 0)
                            r.NumReviews = 1;
                        else
                            r.NumReviews = 0;

                        r.Name = name;
                        //r.Address = addressArr[0].Trim();
                        r.FullAddress = finalAddress;
                        //r.City = addressArr[1].Trim();
                        //r.City = addressArr[1].Substring(0, addressArr[1].IndexOf("(") - 1).Trim();
                        //r.Neighborhood = addressArr[1].Substring(addressArr[1].IndexOf("(") + 1, addressArr[1].IndexOf(")") - addressArr[1].IndexOf("(") - 1).Trim();
                        //Trace.WriteLine(r.Neighborhood);
                        r.State = "IL";
                        r.ZipCode = " ";
                        r.Cuisine = entryType;
                        r.SearchEngine = r.SearchEngine | (int)Engine.Metromix;
                        r.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);
                        Globals.metromixLocalDataAllRuns.Add(r);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }
               
            }  
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }

             
    }
}
