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
    class DexKnows : ISearchEngine
    {
        private int code;
        private string Name;
        private string defaultLoc = "Chicago%2C+IL+(city)";
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE);
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        
        public DexKnows(int c, String n)
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
        }

        public string getName
        {
            get { return Name; }         
        }

        public int getConstraints
        {
            get { return constraints; }
        }

        public void GetLocationCrosswalkFromXml() 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/DexKnows_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/DexKnows_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }
        public void GetCuisineCrosswalkFromXml() 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/DexKnows_Cuisine_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/DexKnows_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
            //get { return "http://www.dexknows.com/local/food_and_beverage/dining/restaurants/geo/"; }
            get { return "http://www.dexknows.com/local/food_and_beverage/restaurants/geo/"; }
        }

        public string BaseRestaurantUrlKeyword
        {
            //get { return "http://www.dexknows.com/search/?sort=2&what=Restaurants%3A+"; }
            get { return "http://www.dexknows.com/search/?what="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            string LocKey = "";
            string CuiKey;
            string queryStr;
            List<string> queryURLs = new List<string>();

            foreach (String location in locations)
            {
                if (!keyword.Equals(""))
                {
                    if (location.Equals("All"))
                    {
                        LocKey = defaultLoc;
                    }
                    else
                    {
                        //string[] locationSplit = location_crosswalk[location].Split(new char[] { '_' });
                        //LocKey = locationSplit[1].Substring(0, locationSplit[1].IndexOf("-")).Trim();
                        LocKey = location_crosswalk[location];
                        LocKey = LocKey.Substring(11, LocKey.IndexOf("-il") - 11);
                        LocKey = LocKey.Replace("_", "+") + "%2C+Chicago%2C+IL+(neighborhood)";
                    }

                    keyword = keyword.Replace(" ", "+");
                    queryStr = BaseRestaurantUrlKeyword + keyword + "&where=" + LocKey + "&mr=20&st=0&distance=&view=list";
                }
                else
                {
                    if (location.Equals("All"))
                    {
                        LocKey = "c-chicago-il/";
                    }
                    else
                    {
                        LocKey = location_crosswalk[location] + "/";
                    }

                    if (cuisine.Equals("All"))
                    {
                        CuiKey = "";
                    }
                    else
                    {
                        CuiKey = "att/" + cuisine_crosswalk[cuisine] + "/";
                    }

                    queryStr = BaseRestaurantUrl + LocKey + CuiKey + "?";

                    queryStr = queryStr + "st=0&distance=0&pageset=1&view=list&sort=2";
                }

                queryURLs.Add(queryStr);
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.dexknowsLocalDataAllRuns.Count; i++)
            {
                Globals.dexknowsLocalDataAllRuns[i].Ranking = i - Globals.dexknowsTotalQueries + 1;
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
                Globals.dexknowsLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.dexknowsTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {

            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";

            WebRequest req = HttpWebRequest.Create(url);
            //Globals.WriteOutput(url + " starting");
            //Trace.WriteLine(url + " starting");

            // asynchronously get the response from http server
            Async<WebResponse> webresp = null;

            try
            {
                webresp = req.GetResponseAsync();
            }
            catch (WebException e)
            {
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: DEXKNOWS" + e.Message;
                error_url.ZipCode = "99999";
                Globals.dexknowsLocalDataAllRuns.Add(run_url);
                Globals.dexknowsLocalDataAllRuns.Add(error_url);
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

            //Globals.WriteOutput("RESPONSE " + response);
            response = response.Replace("\n", "");
            response = response.Replace("\r", "");
            response = response.Replace("<div id=\"listing", "\n<div id=\"listing");
            response = response.Replace("<div class=\"listingTools\">", "<div class=\"listingTools\">\n");

            //Regex regNames = new Regex("<div class=\"details\"><p class=\"phone\">\r\n+.*<span class=\"linkDiv\">");
            Regex regNames = new Regex("<div id=\"listing+.*<div class=\"listingTools\">");

            MatchCollection names = regNames.Matches(response);
            String tmpStr = "";
            int maxRecords = names.Count < Globals.Threshold ? names.Count : Globals.Threshold;

            for (int i = 0; i < maxRecords; ++i)
            {
                Restaurant restaurant = new Restaurant();
                Match name = names[i];
                //Trace.WriteLine("DEXKNOWS RAW " + name.Value);

                if (name != null)
                {
                    //more string processesing to get the name                    
                    tmpStr = name.Value.Substring(name.Value.IndexOf("title="), name.Value.IndexOf("</A>") + 3 - name.Value.IndexOf("title="));
                    restaurant.Name = tmpStr.Substring(tmpStr.IndexOf('>') + 1, tmpStr.IndexOf('<') - tmpStr.IndexOf('>') - 1);
                    //Trace.WriteLine("DEXKNOWS NAME " + restaurant.Name);

                    //more string processesing to get the address
                    if (name.Value.Contains("<div class=\"address\">") && name.Value.Contains("<span class=\"linkDiv\">"))
                    {
                        tmpStr = name.Value.Substring(name.Value.IndexOf("<div class=\"address\">") + 21,
                                 name.Value.IndexOf("<span class=\"linkDiv\">") - name.Value.IndexOf("<div class=\"address\">") - 21).Trim();
                        //Trace.WriteLine("DEXKNOWS ADDRESS " + tmpStr);
                        restaurant.Address = tmpStr.Substring(0, tmpStr.IndexOf(","));
                        restaurant.City = "Chicago";
                        restaurant.State = "IL";
                        restaurant.ZipCode = tmpStr.Substring(tmpStr.IndexOf("IL,") + 4, tmpStr.Length - tmpStr.IndexOf("IL,") - 4);
                        restaurant.FullAddress = tmpStr;
                    }
                    else if (name.Value.Contains("</h2>") && name.Value.Contains("<span class=\"linkDiv\">"))
                    {
                        tmpStr = name.Value.Substring(name.Value.IndexOf("</h2>") + 5,
                                 name.Value.IndexOf("<span class=\"linkDiv\">") - name.Value.IndexOf("</h2>") - 5).Trim();
                        //Trace.WriteLine("DEXKNOWS H2 " + tmpStr);
                        restaurant.Address = tmpStr.Substring(0, tmpStr.IndexOf(","));
                        restaurant.City = "Chicago";
                        restaurant.State = "IL";
                        restaurant.ZipCode = tmpStr.Substring(tmpStr.IndexOf("IL,") + 4, tmpStr.Length - tmpStr.IndexOf("IL,") - 4);
                        restaurant.FullAddress = tmpStr;
                    }
                    else
                        continue; //restaurant has no address, we drop it

                    
                    string ratStr = "", reviewStr = "";
                    if (name.Value.IndexOf("rating_dk") < 0)
                    //if (name.Value.IndexOf("<div class=\"ratings\"></div>") > 0 && name.Value.IndexOf("rating_dk") < 0)
                    {
                        ratStr = "0";
                        reviewStr = "0";
                    }
                    else
                    {
                        //tmpStr = name.Value.Substring(name.Value.IndexOf("<div class=\"ratings\">"), 
                        //                              name.Value.IndexOf("</div><div class=\"cl\">") - name.Value.IndexOf("<div class=\"ratings\">"));

                        tmpStr = name.Value.Substring(name.Value.IndexOf("<img class=\"rating_dk"),
                                                      name.Value.IndexOf("<div class=\"cl\">") - name.Value.IndexOf("<img class=\"rating_dk"));

                        ratStr = tmpStr.Substring(tmpStr.IndexOf("rating_dk rating") + 16, tmpStr.IndexOf("\" width") - tmpStr.IndexOf("rating_dk rating") - 16);
                        ratStr = ratStr.Replace("\"", "");

                        if (tmpStr.IndexOf("dexknows (") > 0)
                        {
                            reviewStr = tmpStr.Substring(tmpStr.IndexOf("dexknows (") + 10, tmpStr.IndexOf(") </a>") - tmpStr.IndexOf("dexknows (") - 10);
                        }
                        else if (tmpStr.IndexOf("logo_dexknows_mini.png") > 0)
                        {
                            reviewStr = tmpStr.Substring(tmpStr.IndexOf("</img> (") + 8, tmpStr.IndexOf(") </a>") - tmpStr.IndexOf("</img> (") - 8);
                        }
                    }

                    restaurant.Rating = Convert.ToDouble(ratStr);
                    restaurant.NumReviews = Convert.ToInt16(reviewStr);

                    if (name.Value.IndexOf("<p class=\"phone\">") > 0)
                    {
                        tmpStr = name.Value.Substring(name.Value.IndexOf("<p class=\"phone\">") + 17, name.Value.IndexOf("</p><h2>")
                                                                         - name.Value.IndexOf("<p class=\"phone\">") - 17);
                    }
                    if (tmpStr.IndexOf("<img") > 0)
                        tmpStr = tmpStr.Substring(0, tmpStr.IndexOf("<img"));

                    restaurant.PhoneNumber = tmpStr.Replace("<br>", String.Empty);
                    //Trace.WriteLine(restaurant.PhoneNumber);

                }
                restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.DexKnows;
                restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE);
                Globals.dexknowsLocalDataAllRuns.Add(restaurant);
            }
   
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }        
    }
}
