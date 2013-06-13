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
    class MenuPages : ISearchEngine
    {
        private int code;        
        private string Name;
        //private int maxNumResults = 25;
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);

        public MenuPages(int c, String n)
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

        public string BaseRestaurantUrl
        {
            get { return "http://chicago.menupages.com/restaurants/"; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void GetLocationCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/MenuPages_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/MenuPages_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            string price_val;
            int sort = 0;
            string LocKey;
            string CuiKey;
            string queryStr;
            List<string> queryURLs = new List<string>();

            foreach (String location in locations)
            {
                if (location.Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = location_crosswalk[location];
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    CuiKey = "/" + cuisine_crosswalk[cuisine];
                }

                switch (price)
                {
                    case "$":
                        price_val = "/sort/Price-0/";
                        sort = 1;
                        break;
                    case "$$":
                        price_val = "/sort/Price-0/";
                        sort = 2;
                        break;
                    case "$$$":
                        price_val = "/sort/Price-1/";
                        sort = 3;
                        break;
                    case "$$$$":
                        price_val = "/sort/Price-1/";
                        sort = 4;
                        break;
                    case "$$$$$":
                        price_val = "/sort/Price-1/";
                        sort = 5;
                        break;
                    default:
                        price_val = "/sort/Rating-1/";
                        sort = 0;
                        break;
                }

                queryStr = BaseRestaurantUrl;
                
                if (!keyword.Equals(""))
                {
                    queryStr = queryStr + "adv/" + keyword + "___/";
                }

                if (location.Equals("All") && cuisine.Equals("All"))
                {
                    //location and cuisne are not specified
                    queryStr = queryStr + "all-areas/all-neighborhoods/all-cuisines" + price_val;
                }
                else if (location.Equals("All") && !cuisine.Equals("All"))
                {
                    //cuisine is specified but location is not
                    queryStr = queryStr + "all-areas/all-neighborhoods" + CuiKey + price_val;
                }
                else if (!location.Equals("All") && cuisine.Equals("All"))
                {
                    //location is specified but cuisine is not
                    queryStr = queryStr + "all-areas/" + LocKey + "/all-cuisines" + price_val;
                }
                else
                {
                    queryStr = queryStr + "all-areas/" + LocKey + CuiKey + price_val;
                }

                queryURLs.Add(queryStr);
            }

            DownloadAll(queryURLs, sort, price).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.menupagesLocalDataAllRuns.Count; i++)
            {
                Globals.menupagesLocalDataAllRuns[i].Ranking = i - Globals.menupagesTotalQueries + 1;
            }

        }

        static IEnumerable<IAsync> DownloadAll(List<string> queryURLs, int sort, string price)
        {

            if (queryURLs.Count == 1)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0], sort, price)
                   );
                yield return methods;
            }
            else if (queryURLs.Count == 2)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0], sort, price),
                   processRequestsAsync(queryURLs[1], sort, price)
                   );
                yield return methods;
            }
            else if (queryURLs.Count == 3)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0], sort, price),
                    processRequestsAsync(queryURLs[1], sort, price),
                    processRequestsAsync(queryURLs[2], sort, price)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 7)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],sort,price),
                    processRequestsAsync(queryURLs[1],sort,price),
                    processRequestsAsync(queryURLs[2],sort,price),
                    processRequestsAsync(queryURLs[3],sort,price),
                    processRequestsAsync(queryURLs[4],sort,price),
                    processRequestsAsync(queryURLs[5],sort,price),
                    processRequestsAsync(queryURLs[6],sort,price)
                    );
                yield return methods;
            }
            else
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0], sort, price),
                    processRequestsAsync(queryURLs[1], sort, price),
                    processRequestsAsync(queryURLs[2], sort, price),
                    processRequestsAsync(queryURLs[3], sort, price)
                    );
                yield return methods;
            }
            //Globals.WriteOutput("Completed all!");
            //Trace.WriteLine("Completed all!");


            if (sort != 0)
            {
                MergeResults.orderList(Globals.menupagesLocalDataAllRuns);
            }


            for (int i = 0; i < queryURLs.Count; i++)
            {
                Restaurant run_url = new Restaurant();
                run_url.ZipCode = "99999";
                run_url.Name = queryURLs[i];
                Globals.menupagesLocalDataAllRuns.Insert(0, run_url);
            }


            Globals.menupagesTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url, int sort, string price)
        {
            int maxNumResults = 25;

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
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: YAHOO" + e.Message;
                error_url.ZipCode = "99999";
                Globals.yahooLocalDataAllRuns.Add(run_url);
                Globals.yahooLocalDataAllRuns.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }
            yield return webresp;

            Stream resp = webresp.Result.GetResponseStream();

            // download HTML using the asynchronous extension method
            // instead of using synchronous StreamReader
            Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            yield return html;

            string response = html.Result;
            response = response.Replace("\t", "");
            response = response.Replace("\n", "");

            //List<Restaurant> restaurants = new List<Restaurant>();

            response = response.Replace("<td class=\"name-address\" scope=\"row\">", "\n<td class=\"name-address\" scope=\"row\">");
            Regex regNames = new Regex("<td class=\"name-address\" scope=\"row\">+.*</td>");
            Regex regAddress = new Regex("\r\n<div class=\"rest_addr\">\r\n.*<br>\r\n.*<br>\r\n");

            MatchCollection names = regNames.Matches(response);
            int maxCount = names.Count > maxNumResults ? maxNumResults - 1 : names.Count;

            for (int i = 0; i < maxCount; ++i)
            {
                Restaurant restaurant = new Restaurant();
                Match name = names[i];

                if (name != null)
                {

                    //more string processesing to get the name  

                    restaurant.Name = name.Value.Substring(name.Value.IndexOf("</span>") + 7,
                                                           name.Value.IndexOf("</a>") - name.Value.IndexOf("</span>") - 7);

                    //if (restaurant.Name.Contains("CLOSED"))
                    //    continue;

                    //clean up restaurant name
                    restaurant.Name = restaurant.Name.Replace("&#39;", "'");
                    restaurant.Name = restaurant.Name.Replace("&amp;", "&");

                    if (restaurant.Name.Contains("(CLOSED)"))
                    {
                        restaurant.IsClosed = true;
                        //we removed CLOSED from the name in order for edit distance to work correctly
                        restaurant.Name = restaurant.Name.Replace("(CLOSED)", "").Trim();
                    }

                    restaurant.Address = name.Value.Substring(name.Value.IndexOf("</a>") + 4,
                                                              name.Value.IndexOf('|') - name.Value.IndexOf("</a>") - 4);
                    restaurant.City = "Chicago";
                    restaurant.State = "IL";
                    restaurant.FullAddress = restaurant.Address + ", Chicago IL";
                    restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.MenuPages;
                    restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);

                    string ratingStr = name.Value.Substring(name.Value.IndexOf("images/star") + 11, name.Value.IndexOf(".gif") - name.Value.IndexOf("images/star") - 11);
                    ratingStr = ratingStr.Replace("_", ".");
                    restaurant.Rating = Convert.ToDouble(ratingStr);
                    string reviewStr = name.Value.Substring(name.Value.IndexOf("reviews") + 9, name.Value.LastIndexOf("</td>") - name.Value.IndexOf("reviews") - 9);
                    restaurant.NumReviews = Convert.ToInt16(name.Value.Substring(name.Value.IndexOf("reviews") + 9, name.Value.LastIndexOf("</td>") - name.Value.IndexOf("reviews") - 9));
                }
                string tmp_dollars = name.ToString().Substring(name.ToString().IndexOf("price"));
                tmp_dollars = tmp_dollars.Substring(tmp_dollars.IndexOf("class"));
                //tmp_dollars = name.ToString().Substring(name.ToString().IndexOf("price1"));
                tmp_dollars = tmp_dollars.Substring(tmp_dollars.IndexOf(">") + 1, tmp_dollars.IndexOf("<") - tmp_dollars.IndexOf(">") - 1);
                restaurant.Price = tmp_dollars;

                
                if (sort != 0)
                {
                    if (tmp_dollars.Equals(price))
                        Globals.menupagesLocalDataAllRuns.Add(restaurant);
                }
                else
                    Globals.menupagesLocalDataAllRuns.Add(restaurant);
            }
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;            
        }        
    }
}
