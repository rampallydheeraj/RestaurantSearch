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
    class Menuism : ISearchEngine
    {
        private int code;
        private string Name;
        //private int maxNumPages = 5;
        //private int maxNumResults = 20;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE);
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;

        public Menuism(int c, String n)
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
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Menuism_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Menuism_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {            
            get { return "http://www.menuism.com/cities/us/il/chicago"; }
        }

        public string BaseRestaurantUrlKeyword
        {
            get { return "http://www.menuism.com/search?q="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            string LocKey;
            string CuiKey;
            List<string> queryURLs = new List<string>();

            foreach (String location in locations)
            {
                if (location.Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = "/n/" + location_crosswalk[location];
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    CuiKey = "/tags/" + cuisine_crosswalk[cuisine];
                }

                //string queryStr = BaseRestaurantUrl + LocKey + CuiKey;
                string queryStr;
                if (CuiKey.Equals(""))
                {
                    if (keyword.Equals(""))
                    {
                        queryStr = BaseRestaurantUrl + LocKey + CuiKey;
                    }
                    else
                    {
                        queryStr = BaseRestaurantUrlKeyword + keyword + "&l=" + LocKey + "+Chicago%2C+IL&x=15&y=9";
                    }
                }
                else
                {
                    queryStr = BaseRestaurantUrl + LocKey + CuiKey;
                }

                queryURLs.Add(queryStr);
                queryURLs.Add(queryStr + "/by-page_2");
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.menuismLocalDataAllRuns.Count; i++)
            {
                Globals.menuismLocalDataAllRuns[i].Ranking = i - Globals.menuismTotalQueries + 1;
            }
        }

        static IEnumerable<IAsync> DownloadAll(List<string> queryURLs)
        {

            if (queryURLs.Count == 2)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0]),
                   processRequestsAsync(queryURLs[1])
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
            else if (queryURLs.Count == 7)
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
            else
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
            //Globals.WriteOutput("Completed all!");
            //Trace.WriteLine("Completed all!");

            for (int i = 0; i < queryURLs.Count; i++)
            {
                Restaurant run_url = new Restaurant();
                run_url.ZipCode = "99999";
                run_url.Name = queryURLs[i];
                Globals.menuismLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.menuismTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {
            //Restaurant run_url = new Restaurant();
            //run_url.Name = url;
            //run_url.ZipCode = "99999";

            //WebRequest req = HttpWebRequest.Create(url);

            //// asynchronously get the response from http server
            //Async<WebResponse> webresp = null;

            //try
            //{
            //    webresp = req.GetResponseAsync();
            //}
            //catch (WebException e)
            //{
            //    Restaurant error_url = new Restaurant();
            //    error_url.Name = "YUMI_WEB_ERROR: MENUISM" + e.Message;
            //    error_url.ZipCode = "99999";
            //    Globals.menuismLocalDataAllRuns.Add(run_url);
            //    Globals.menuismLocalDataAllRuns.Add(error_url);
            //    Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
            //    yield break;
            //}
            //yield return webresp;

            //Stream resp = webresp.Result.GetResponseStream();

            //// download HTML using the asynchronous extension method
            //// instead of using synchronous StreamReader
            //Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            //yield return html;

            //string response = html.Result;

            //Restaurant response_url = new Restaurant();
            //response_url.Name = response;
            //response_url.ZipCode = "99999";
            //Globals.menuismLocalDataAllRuns.Add(response_url);            

            string response = WebDocumentRetriever.GetDocument(url, null);
            response = response.Replace("\t", "");
            response = response.Replace("\n", "");

            //Regex regNames = new Regex("<div class=\"left-part\"><h2>.*</h2>");
            Regex regNames = new Regex("<div class=\"left-part\"><h3>.*</h3>");
            //Regex regAddress = new Regex("</a><em>.*</em>");
            Regex regAddress = new Regex("<em>+.*</em>");
            Regex regLocality = new Regex("<strong><a+.*</a></strong>");
            Regex regRating = new Regex("<li class=\"current-rating average\"+.*rating</li>");
            Regex regReviews = new Regex("<span class='star-display-rating-count'>+.*review");
            Regex regHood = new Regex("<div class=\"right-part\">.*</div>");
            Regex regCuisine = new Regex("<span class='star-display-rating-count'>+.*<div class=\"right-part\">");

            MatchCollection names = regNames.Matches(response.Replace("</a></strong>", "</a></strong>\n"));
            MatchCollection addresses = regAddress.Matches(response.Replace("</a></strong>", "</a></strong>\n"));
            MatchCollection localities = regLocality.Matches(response.Replace("</a></strong>", "</a></strong>\n"));
            MatchCollection ratings = regRating.Matches(response.Replace("</a></strong>", "</a></strong>\n"));
            MatchCollection reviews = regReviews.Matches(response.Replace("</a></strong>", "</a></strong>\n"));
            MatchCollection hoods = regHood.Matches(response.Replace("<div class=\"right-part\">", "\n<div class=\"right-part\">"));
            MatchCollection cuisines = regCuisine.Matches(response.Replace("</a></strong>", "</a></strong>\n"));

            String locality = "";
            String tmpStr = "";
            //int maxRecords = names.Count < MergeResults.Threshold ? names.Count : MergeResults.Threshold;

            //Trace.WriteLine(cuisines.Count);
            for (int i = 0; i < names.Count; ++i)
            {
                Restaurant restaurant = new Restaurant();
                Match name = names[i];
                Match addr = addresses.Count < i || addresses.Count == 0 ? null : addresses[i];
                Match loc = localities[i];
                Match rating = ratings[i];
                Match review = reviews[i];
                Match hood = hoods[i];
                Match cuisine = cuisines[i];
                //Trace.WriteLine(cuisine.Value);

                string hoodStr = "";

                if (hood.Value.IndexOf("<a href='http://www.menuism.com/cities/us/il/chicago/n/") > 0)
                {
                    hoodStr = hood.Value.Substring(hood.Value.IndexOf("<a href='http://www.menuism.com/cities/us/il/chicago/n/"),
                              hood.Value.LastIndexOf("</a>") - hood.Value.IndexOf("<a href='http://www.menuism.com/cities/us/il/chicago/n/"));

                    hoodStr = hoodStr.Substring(hoodStr.IndexOf(">") + 1, hoodStr.IndexOf("</a") - hoodStr.IndexOf(">") - 1);
                    char first = char.ToUpper(hoodStr[0]);
                    hoodStr.Remove(1);
                    restaurant.Neighborhood = hoodStr;
                    restaurant.Neighborhood.PadLeft(1, first);
                }

                //if (!hoodStr.Equals(location) && !location.Equals("All"))
                //    continue;

                if (name != null)
                {
                    //more string processesing to get the name                    
                    tmpStr = name.Value.Substring(name.Value.IndexOf("<a"), (name.Value.IndexOf("</h3") - name.Value.IndexOf("<a")));
                    restaurant.Name = tmpStr.Substring(tmpStr.IndexOf('>') + 1, (tmpStr.IndexOf("</a") - tmpStr.IndexOf('>')) - 1);

                    //checking if the restaurant is closed
                    if (restaurant.Name.Contains("(CLOSED)") || restaurant.Name.Contains("(closed)"))
                    {
                        restaurant.IsClosed = true;
                        //we removed CLOSED from the name in order for edit distance to work correctly
                        restaurant.Name = restaurant.Name.Replace("(CLOSED)", "").Trim();
                        restaurant.Name = restaurant.Name.Replace("(closed)", "").Trim();
                    }

                    //more string processesing to get the address
                    tmpStr = loc.Value.Substring(loc.Value.IndexOf("<a"), loc.Value.IndexOf("</strong>") - loc.Value.IndexOf("<a"));
                    locality = tmpStr.Substring(tmpStr.IndexOf('>') + 1, (tmpStr.IndexOf("</a") - tmpStr.IndexOf('>')) - 1);

                    if (addr != null)
                    {
                        tmpStr = addr.Value.Substring(addr.Value.IndexOf("<em>") + 4, addr.Value.IndexOf("</em>") - addr.Value.IndexOf("<em>") - 4);
                        restaurant.Address = tmpStr;
                        restaurant.Address = restaurant.Address.Replace("<em>", "");
                    }

                    restaurant.City = locality.Substring(0, locality.IndexOf(","));
                    restaurant.State = locality.Substring(locality.IndexOf(",") + 1, locality.Length - locality.IndexOf(",") - 1);
                    restaurant.FullAddress = restaurant.Address + ", " + restaurant.City + " " + restaurant.State;
                    //restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.Menuism;

                    //get rating info
                    tmpStr = rating.Value.Substring(rating.Value.IndexOf(">") + 1, rating.Value.IndexOf("star") - rating.Value.IndexOf(">") - 2);
                    restaurant.Rating = Convert.ToDouble(tmpStr);

                    //tmpStr = review.Value.Substring(review.Value.IndexOf(">") + 1, review.Value.IndexOf("</span>") - review.Value.IndexOf(">") - 1);
                    tmpStr = review.Value;
                    string ratingStr = tmpStr.Substring(tmpStr.IndexOf("(") + 1, tmpStr.LastIndexOf("rating") - tmpStr.IndexOf("(") - 2);
                    string reviewStr = tmpStr.Substring(tmpStr.IndexOf(",") + 1, tmpStr.IndexOf("review") - tmpStr.IndexOf(",") - 2);
                    restaurant.NumReviews = Convert.ToInt16(ratingStr);

                    if (cuisine.Value.Contains("Tagged"))
                    {
                        restaurant.Cuisine = cuisine.Value.Substring(cuisine.Value.IndexOf("Tag']);\">") + 9, cuisine.Value.IndexOf("</a>") - cuisine.Value.IndexOf("Tag']);\">") - 9);
                        //Trace.WriteLine(restaurant.Cuisine);
                    }
                }

                restaurant.Ranking = i;
                restaurant.SearchEngine = 0;
                restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.Menuism;
                restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE);

                Globals.menuismLocalDataAllRuns.Add(restaurant);
            }

            yield break;
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }

        public void processRequestForPage(string response, List<Restaurant> restaurants, string location)
        {

        }
    }
}