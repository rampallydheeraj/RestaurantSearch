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
    class ChicagoReader : ISearchEngine
    {
        //returns 15 results per page        
        
        private int code;
        private String Name;
        //private int maxNumPages = 3;
        //private int maxNumResults = 20;
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);

        public ChicagoReader(int c, String n)
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
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/ChicagoReader_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/ChicagoReader_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
            get { return "http://www.chicagoreader.com/chicago/LocationSearch?locationSection="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            string LocKey;
            string CuiKey;
            string PriKey;
            List<string> queryURLs = new List<string>();

            foreach (string location in locations)
            {
                switch (price)
                {
                    case "$":
                        PriKey = "$";
                        break;
                    case "$$":
                        PriKey = "$$";
                        break;
                    case "$$$":
                        PriKey = "$$$";
                        break;
                    case "$$$$":
                        PriKey = "$$$$";
                        break;
                    case "$$$$$":
                        PriKey = "$$$$$";
                        break;
                    default:
                        PriKey = "";
                        break;
                }
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
                    CuiKey = cuisine_crosswalk[cuisine];
                }

                string rating = "&readerRating=0";
                string queryStr = BaseRestaurantUrl + "857642" + "&locationCategory=" + CuiKey
                                  + "&diningFeature=" + "&neighborhood=" + LocKey + rating
                                  + "&diningPriceKey=" + PriKey + "&keywords=" + keyword;
                queryURLs.Add(queryStr);
                rating = "&readerRating=5";
                queryStr = BaseRestaurantUrl + "857642" + "&locationCategory=" + CuiKey
                                  + "&diningFeature=" + "&neighborhood=" + LocKey + rating
                                  + "&diningPriceKey=" + PriKey + "&keywords=" + keyword;
                queryURLs.Add(queryStr);
                rating = "&readerRating=4";
                queryStr = BaseRestaurantUrl + "857642" + "&locationCategory=" + CuiKey
                                  + "&diningFeature=" + "&neighborhood=" + LocKey + rating
                                  + "&diningPriceKey=" + PriKey + "&keywords=" + keyword;
                queryURLs.Add(queryStr);
                rating = "&readerRating=3";
                queryStr = BaseRestaurantUrl + "857642" + "&locationCategory=" + CuiKey
                                  + "&diningFeature=" + "&neighborhood=" + LocKey + rating
                                  + "&diningPriceKey=" + PriKey + "&keywords=" + keyword;
                queryURLs.Add(queryStr);

                DownloadAll(queryURLs, location).ExecuteAndWait();
                queryURLs.Clear();

            }
            for (int i = Globals.chicagoreaderTotalQueries; i < Globals.chicagoreaderLocalDataAllRuns.Count; i++)
            {
                //Trace.WriteLine("chicagoreaderTotalQueries: " + Globals.chicagoreaderTotalQueries);
                Globals.chicagoreaderLocalDataAllRuns[i].Ranking = i - Globals.chicagoreaderTotalQueries + 1;
            }
        }

        static IEnumerable<IAsync> DownloadAll(List<string> queryURLs, string location)
        {
            if (queryURLs.Count == 1)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0],location)
                   );
                yield return methods;
            }
            else if (queryURLs.Count == 2)
            {
                var methods = Async.Parallel(
                   processRequestsAsync(queryURLs[0],location),
                   processRequestsAsync(queryURLs[1],location)
                   );
                yield return methods;
            }
            else if (queryURLs.Count == 3)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 4)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 5)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 6)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location),
                    processRequestsAsync(queryURLs[5],location)
                    );
                yield return methods;
            }

            else if (queryURLs.Count == 7)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location),
                    processRequestsAsync(queryURLs[5],location),
                    processRequestsAsync(queryURLs[6],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 8)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location),
                    processRequestsAsync(queryURLs[5],location),
                    processRequestsAsync(queryURLs[6],location),
                    processRequestsAsync(queryURLs[7],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 9)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location),
                    processRequestsAsync(queryURLs[5],location),
                    processRequestsAsync(queryURLs[6],location),
                    processRequestsAsync(queryURLs[7],location),
                    processRequestsAsync(queryURLs[8],location)
                    );
                yield return methods;
            }
            else if (queryURLs.Count == 10)
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0],location),
                    processRequestsAsync(queryURLs[1],location),
                    processRequestsAsync(queryURLs[2],location),
                    processRequestsAsync(queryURLs[3],location),
                    processRequestsAsync(queryURLs[4],location),
                    processRequestsAsync(queryURLs[5],location),
                    processRequestsAsync(queryURLs[6],location),
                    processRequestsAsync(queryURLs[7],location),
                    processRequestsAsync(queryURLs[8],location),
                    processRequestsAsync(queryURLs[9],location)
                    );
                yield return methods;
            }
            else
            {
                var methods = Async.Parallel(
                    processRequestsAsync(queryURLs[0], location),
                    processRequestsAsync(queryURLs[1], location),
                    processRequestsAsync(queryURLs[2], location),
                    processRequestsAsync(queryURLs[3],location)
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
                Globals.chicagoreaderLocalDataAllRuns.Insert(0, run_url);
            }
            Globals.chicagoreaderTotalQueries = Globals.chicagoreaderTotalQueries + queryURLs.Count;
            
        }

        static IEnumerable<IAsync> processRequestsAsync(string url, string location)
        {
            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";

            WebRequest req = HttpWebRequest.Create(url);
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
                error_url.Name = "YUMI_WEB_ERROR: CHICAGOREADER" + e.Message;
                error_url.ZipCode = "99999";
                Globals.chicagoreaderLocalDataAllRuns.Add(run_url);
                Globals.chicagoreaderLocalDataAllRuns.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }
            yield return webresp;

            Stream resp;
            if (webresp != null)
                resp = webresp.Result.GetResponseStream();
            else
            {
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: CHICAGOREADER";
                error_url.ZipCode = "99999";
                Globals.chicagoreaderLocalDataAllRuns.Add(run_url);
                Globals.chicagoreaderLocalDataAllRuns.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }

            // download HTML using the asynchronous extension method
            // instead of using synchronous StreamReader
            Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            yield return html;

            string response = html.Result;


            response = response.Replace("\n", "");
            response = response.Replace("\t", "");
            response = response.Replace("\r", "");
            response = response.Replace("<li class=\"l0 locationItem\">", "\n<li class=\"l0 locationItem\">");

            //Regex regLocationItem = new Regex("<li class=\"l0 locationItem\">.*<li class=\"l1 tags\">");
            Regex regLocationItem = new Regex("<li class=\"l0 locationItem\">.*</div> <!-- end locationListing -->");
            Regex regNeighborhood = new Regex("neighborhood=.*</a></span>");
            Regex regPrice = new Regex("diningPriceKey\">.*</span>");
            Regex regCuisine = new Regex("locationCategory=.*</a>");
            Regex regAddress = new Regex("</a></span>.*<a href");
            Regex regPhoneNum = new Regex("alt=\"phone\" />.*</span>");
            Regex regRatings = new Regex("<span class=\"rating\">.*</span>");
            Regex regReviews = new Regex("readerComments.*review");

            MatchCollection locations = regLocationItem.Matches(response);

            for (int i = 0; i < locations.Count; ++i)
            {
                Restaurant restaurant = new Restaurant();
                Match locationItem = locations[i];

                Match addr = regAddress.Match(locationItem.Value);
                //Trace.WriteLine(locationItem.Value);
                Match neighborhood = regNeighborhood.Match(locationItem.Value);
                //Trace.WriteLine("neighborhood: " + neighborhood.Value);
                Match price = regPrice.Match(locationItem.Value);
                //Trace.WriteLine(price.Value);
                Match cuisine = regCuisine.Match(locationItem.Value);
                //Trace.WriteLine(cuisine.Value);
                Match phone = regPhoneNum.Match(locationItem.Value);
                Match rating = regRatings.Match(locationItem.Value);
                Match review = regReviews.Match(locationItem.Value);

                //more string processesing to get the name
                string nameStr = locationItem.Value.Substring(locationItem.Value.IndexOf("Location?oid="),
                                                          locationItem.Value.IndexOf("</a>") + 4);
                nameStr = nameStr.Substring(nameStr.IndexOf(">") + 1, nameStr.IndexOf("<") - nameStr.IndexOf(">") - 1);
                restaurant.Name = nameStr;

                restaurant.Neighborhood = neighborhood.Value.Substring(neighborhood.Value.IndexOf("\">") + 2, neighborhood.Value.IndexOf("</a>") - neighborhood.Value.IndexOf("\">") - 2);
                //Trace.WriteLine(restaurant.Neighborhood);

                if (price.Value.Length > 0)
                {
                    restaurant.Price = price.Value.Substring(price.Value.IndexOf("\">") + 2, price.Value.IndexOf("</span>") - price.Value.IndexOf("\">") - 2);
                    //Trace.WriteLine(restaurant.Price);
                }

                //restaurant.Cuisine = cuisine.Value.Substring(cuisine.Value.IndexOf("tag\">") + 5, cuisine.Value.IndexOf("</a>") - cuisine.Value.IndexOf("tag\">") - 5);
                //Trace.WriteLine(restaurant.Cuisine);

                //more string processesing to get the address                
                restaurant.Address = addr.Value.Substring(addr.Value.IndexOf("</span>") + 7, addr.Value.IndexOf("(<a href") - addr.Value.IndexOf("</span>") - 7);
                restaurant.Address = restaurant.Address.Trim();
                if (location.Equals("Evanston") || location.Equals("Berwyn") || location.Equals("Cicero") || location.Equals("Forest Park"))
                    restaurant.City = location;
                else
                    restaurant.City = "Chicago";
                restaurant.State = "IL";
                restaurant.FullAddress = restaurant.Address + ", " + restaurant.City + " " + restaurant.State;
                restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.ChicagoReader;
                restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE | Constraints.PRICE);

                //get phone number
                if (!phone.Value.Equals(""))
                {
                    restaurant.PhoneNumber = phone.Value.Substring(phone.Value.IndexOf('>') + 1, phone.Value.IndexOf('<') - phone.Value.IndexOf('>') - 1);
                }
                //Trace.WriteLine(restaurant.PhoneNumber);

                //get rating and number of reviews
                string tmpStr = rating.Value.Replace("><", ">*<");
                string[] tags = tmpStr.Split(new Char[] { '*' });
                int starCount = 0;

                for (int s = 0; s < tags.Length; s++)
                    if (tags[s].Contains("one-star.gif"))
                        starCount++;
                if (starCount > 0)
                {
                    tmpStr = review.Value;
                    tmpStr = tmpStr.Substring(tmpStr.IndexOf("based on") + 8, tmpStr.LastIndexOf("user") - tmpStr.IndexOf("based on") - 8);
                    tmpStr = tmpStr.Trim();
                    restaurant.NumReviews = Convert.ToInt16(tmpStr);
                }
                else
                    restaurant.NumReviews = 0;

                restaurant.Rating = starCount;
                restaurant.Ranking = i;

                //restaurant.PriceLvl = price;

                Globals.chicagoreaderLocalDataAllRuns.Add(restaurant);
            }
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }

        private void processRequestForPage(string response, List<Restaurant> restaurants, int price, string location)
        {
            
        }
    }
}
