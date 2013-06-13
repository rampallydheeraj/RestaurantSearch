using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;
using EeekSoft.Asynchronous;

namespace yumi
{
    class Yelp : ISearchEngine
    {
        private int code;
        private String Name;
        //private int maxNumPages = 5;
        //private int maxNumResults = 30;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;

        public Yelp(int c, String n)
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

        public void GetLocationCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/Yelp_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Yelp_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }
        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            //TextReader reader = new StreamReader("xml/Yelp_Cuisine_crosswalk.xml");
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Yelp_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
           // get { return "http://www.yelp.com/search?find_desc=restaurant&rpp=40"; }
            get { return "http://www.yelp.com/search?find_desc="; }
        }

        public int getConstraints
        {
            get { return constraints; }

        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            List<string> queryURLs = new List<string>();
            string queryStr;
            string priKey;
            string cuiKey;
            string locKey;

            foreach (string location in locations)
            {
                switch (price)
                {
                    case "$":
                        priKey = "&attrs=RestaurantsPriceRange2.1";
                        break;
                    case "$$":
                        priKey = "&attrs=RestaurantsPriceRange2.2";
                        break;
                    case "$$$":
                        priKey = "&attrs=RestaurantsPriceRange2.2";
                        break;
                    case "$$$$":
                        priKey = "&attrs=RestaurantsPriceRange2.3";
                        break;
                    case "$$$$$":
                        priKey = "&attrs=RestaurantsPriceRange2.4";
                        break;
                    default:
                        priKey = "";
                        break;
                }

                if (!location.Equals("All"))
                {
                    //locKey = "&find_loc=:IL:Chicago::" + location_crosswalk[location];
                    locKey = "&find_loc=" + location_crosswalk[location];
                }
                else
                {
                    locKey = "";
                }
                if (!cuisine.Equals("All"))
                {
                    cuiKey = "&cflt=" + cuisine_crosswalk[cuisine];
                }
                else
                {
                    cuiKey = "";
                }

                if (cuiKey.Equals("") && !keyword.Equals(""))
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
                                    cuiKey = "&cflt=" + entry.Value;
                                    break;
                                }
                            }
                            if (!cuiKey.Equals(""))
                                break;
                        }
                        else if (Globals.EditDistance(keyword.ToLower().Trim(), entry.Key.ToLower().Trim()) >= 0.8)
                        {
                            cuiKey = "&cflt=" + entry.Value;
                            break;
                        }
                    }
                }
                //string desc = (keyword.Equals("") ? "restaurant" : keyword) + "&rpp=40";
                string desc = (keyword.Equals("") ? "restaurant" : keyword);
                queryStr = BaseRestaurantUrl + desc + locKey + cuiKey + priKey + "&rpp=29&sortby=rating";
                
                queryURLs.Add(queryStr);
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.yelpLocalDataAllRuns.Count; i++)
            {
                //Trace.WriteLine("yellowpagesTotalQueries: " + Globals.yellowpagesTotalQueries);
                Globals.yelpLocalDataAllRuns[i].Ranking = i - Globals.yelpTotalQueries + 1;
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
                Globals.yelpLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.yelpTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {
            
            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";

            //WebRequest req = HttpWebRequest.Create(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.CookieContainer = CreateCookies();

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
                Trace.WriteLine("In Yelp catch block");
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: YELP" + e.Message;
                error_url.ZipCode = "99999";
                Globals.yelpLocalDataAllRuns.Add(run_url);
                Globals.yelpLocalDataAllRuns.Add(error_url);
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
            //Trace.WriteLine("Yelp response is " + response);

            response = response.Replace("\t", "");
            response = response.Replace("\n", "");

            response = response.Replace("</div><div class=\"rightcol\">", "</div>\n<div class=\"rightcol\">");
            response = response.Replace("</div><div class=\"review_info\">", "</div>\n<div class=\"review_info\">");

            Regex regLeftCol = new Regex("<div class=\"leftcol\">.*</div>");
            Regex regRightCol = new Regex("<div class=\"rightcol\">.*</div>");

            MatchCollection leftCols = regLeftCol.Matches(response);
            MatchCollection rightCols = regRightCol.Matches(response);

            try
            {
                for (int i = 0; i < leftCols.Count; ++i)
                {
                    Restaurant restaurant = new Restaurant();
                    Match leftSide = leftCols[i];
                    Match rightSide = rightCols[i];

                    //get neighborhood
                    string hoodStr = "";
                    if (leftSide.Value.Contains("itemneighborhoods"))
                    {
                        hoodStr = leftSide.Value.Substring(leftSide.Value.IndexOf("<div class=\"itemneighborhoods\">"),
                                    leftSide.Value.LastIndexOf("</div>") - leftSide.Value.IndexOf("<div class=\"itemneighborhoods\">"));
                        hoodStr = hoodStr.Substring(hoodStr.IndexOf("<a") + 2, hoodStr.IndexOf("</a>") - hoodStr.IndexOf("<a") + 2);
                        hoodStr = hoodStr.Substring(hoodStr.IndexOf(">") + 1, hoodStr.IndexOf("</a>") - hoodStr.IndexOf(">") - 1);
                    }
                    restaurant.Neighborhood = hoodStr.Replace("&#39;", "'");
                    //Trace.WriteLine(restaurant.Neighborhood);

                    string catStr = "";
                    if (leftSide.Value.Contains("itemcategories"))
                    {
                        catStr = leftSide.Value.Substring(leftSide.Value.IndexOf("<div class=\"itemcategories\">"),
                                    leftSide.Value.LastIndexOf("</div>") - leftSide.Value.IndexOf("<div class=\"itemcategories\">"));
                        catStr = catStr.Substring(catStr.IndexOf("<a") + 2, catStr.IndexOf("</a>") - catStr.IndexOf("<a") + 2);
                        catStr = catStr.Substring(catStr.IndexOf(">") + 1, catStr.IndexOf("</a>") - catStr.IndexOf(">") - 1);
                    }
                    restaurant.Cuisine = catStr.Replace("&#39;", "'");
                    //Trace.WriteLine(restaurant.Cuisine);
                    //if (!hoodStr.Equals(hoodName) && !hoodName.Equals("All"))
                    //        continue;

                    //get the name
                    string nameStr = leftSide.Value.Substring(leftSide.Value.IndexOf("<a id=\"bizTitleLink") + 20,
                                        leftSide.Value.IndexOf("</div>") - leftSide.Value.IndexOf("<a id=\"bizTitleLink") - 20);
                    nameStr = nameStr.Substring(nameStr.IndexOf(".") + 1, nameStr.IndexOf("</a>") - nameStr.IndexOf(".") - 1).Trim();
                    nameStr = nameStr.Replace("&#39;", "'");
                    nameStr = nameStr.Replace("&amp;", "&");
                    nameStr = nameStr.Replace("<span class=\"highlighted\">", "");
                    nameStr = nameStr.Replace("</span>", "");

                    if (nameStr.Contains("- CLOSED"))
                    {
                        //if restaurant is closed, the we don't need it
                        //continue;
                        restaurant.IsClosed = true;
                        //we removed CLOSED from the name in order for edit distance to work correctly
                        nameStr = nameStr.Replace("- CLOSED", "").Trim();
                    }

                    //if (nameStr != null)
                    //{

                    //get Address
                    string addrStr = rightSide.Value.Substring(rightSide.Value.IndexOf("<address>"),
                                        rightSide.Value.IndexOf("</address>") - rightSide.Value.IndexOf("<address>")).Trim();

                    string streeAddrStr = null;
                    string cityStr = "";

                    if (addrStr.IndexOf("<br>") > 0)
                    {
                        streeAddrStr = addrStr.Substring(addrStr.IndexOf(">") + 1, addrStr.IndexOf("<br>") - addrStr.IndexOf(">") - 1);
                        try
                        {
                            cityStr = addrStr.Substring(addrStr.IndexOf("<br>") + 4, addrStr.LastIndexOf(",") - addrStr.IndexOf("<br>") - 4).Trim();
                        }
                        catch (Exception e)
                        {
                            //Trace.WriteLine("yelp error: " + e.Message);
                        }
                    }
                    else
                    {
                        cityStr = addrStr.Substring(addrStr.IndexOf("<address>") + 9, addrStr.IndexOf(",") - addrStr.IndexOf("<address>") - 9).Trim();
                    }

                    string stateStr = addrStr.Substring(addrStr.IndexOf(",") + 1, 3).Trim();
                    string zipStr = addrStr.Substring(addrStr.IndexOf(stateStr) + 2, 6).Trim();

                    //get phone number
                    string phoneStr = addrStr.Substring(addrStr.IndexOf("<div class=\"phone\">") + 19,
                                        addrStr.IndexOf("</div>") - addrStr.IndexOf("<div class=\"phone\">") - 19).Trim();

                    //get rating and num of reviews
                    string scoreStr = "", ratingStr = "0.00", reviewStr = "0";

                    if (rightSide.Value.Contains("<div class=\"rating\">"))
                    {

                        scoreStr = rightSide.Value.Substring(rightSide.Value.IndexOf("<div class=\"rating\">"),
                                    rightSide.Value.IndexOf("</a>") + 4 - rightSide.Value.IndexOf("<div class=\"rating\">"));

                        //Trace.WriteLine(scoreStr);
                        //Trace.WriteLine(nameStr);
                        ratingStr = scoreStr.Substring(scoreStr.IndexOf("title=\"") + 7, scoreStr.IndexOf(" star rating") - scoreStr.IndexOf("title=\"") - 7);

                        reviewStr = scoreStr.Substring(scoreStr.IndexOf("class=\"reviews\">") + 16,
                                    scoreStr.IndexOf(" review") - scoreStr.IndexOf("class=\"reviews\">") - 16);
                    }

                    restaurant.Name = nameStr;
                    restaurant.Address = streeAddrStr;
                    restaurant.City = cityStr;
                    restaurant.State = stateStr;
                    restaurant.ZipCode = zipStr;
                    restaurant.FullAddress = streeAddrStr + ", " + cityStr + " " + stateStr + " " + zipStr;
                    restaurant.PhoneNumber = phoneStr;
                    //Trace.WriteLine(restaurant.PhoneNumber);
                    restaurant.Rating = Convert.ToDouble(ratingStr);
                    restaurant.NumReviews = Convert.ToInt16(reviewStr);
                    restaurant.Ranking = i;
                    //restaurant.PriceLvl = p;
                    restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.Yelp;
                    restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);

                    Globals.yelpLocalDataAllRuns.Add(restaurant);

                }
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.ToString());
            }
            

        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }

        //private static CookieContainer CreateCookies(string hoodStr)
        //{
        //    string locStr = hoodStr.Equals("") ? "Chicago%2C%2BIL" : hoodStr.Replace("+", "") + "%20Chicago%2C%2BIL";
        //    Cookie location = new Cookie("location", locStr, "/", ".yelp.com");
        //    CookieContainer container = new CookieContainer();
        //    container.Add(location);
        //    return container;
        //}

        private static CookieContainer CreateCookies()
        {
            Cookie location = new Cookie("location", "Chicago%2C%20IL", "/", ".yelp.com");
            CookieContainer container = new CookieContainer();
            container.Add(location);
            return container;
        }
    }    
}
