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
    class YellowPages : ISearchEngine
    {
        private int code;
        private String Name;        
        //private int maxNumResults = 30;
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE);

        public YellowPages(int c, String n)
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
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/YellowPages_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/YellowPages_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
            get { return "http://www.yellowpages.com/chicago-il/"; }
            //get { return "http://www.yellowpages.com/chicago-il/restaurants?"; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            return null;
        }


        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            //DateTime startTime = DateTime.Now;
            List<string> queryURLs = new List<string>();
            string CuiKey;
            string LocKey;

            foreach (string location in locations)
            {
                string q = (keyword.Equals("") ? "restaurants" : keyword) + "?";
                string queryStr;

                if (location.Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = "refinements[neighborhood]=" + location_crosswalk[location];
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    CuiKey = "refinements[headingtext]=" + cuisine_crosswalk[cuisine];
                }

                if (!CuiKey.Equals("") && !LocKey.Equals(""))
                {
                    queryStr = BaseRestaurantUrl + q + LocKey + "&" + CuiKey;
                }
                else
                {
                    queryStr = BaseRestaurantUrl + q + LocKey + CuiKey;
                    //queryStr = BaseRestaurantUrl + keyword + "?refinements[neighborhood]=" + location + "&refinements[headingtext]=Restaurants";
                }
                queryURLs.Add(queryStr);
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.yellowpagesLocalDataAllRuns.Count; i++)
            {
                //Trace.WriteLine("yellowpagesTotalQueries: " + Globals.yellowpagesTotalQueries);
                Globals.yellowpagesLocalDataAllRuns[i].Ranking = i - Globals.yellowpagesTotalQueries + 1;
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
            //Globals.WriteOutput("Completed all!");
            //Trace.WriteLine("Completed all!");

            for (int i = 0; i < queryURLs.Count; i++)
            {
                Restaurant run_url = new Restaurant();
                run_url.ZipCode = "99999";
                run_url.Name = queryURLs[i];
                Globals.yellowpagesLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.yellowpagesTotalQueries = queryURLs.Count;
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
                error_url.Name = "YUMI_WEB_ERROR: YELLOWPAGES" + e.Message;
                error_url.ZipCode = "99999";
                Globals.yellowpagesLocalDataAllRuns.Add(run_url);
                Globals.yellowpagesLocalDataAllRuns.Add(error_url);
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

            //if (html.Result.Contains("YUMI_WEB_ERROR: "))
            //{
            //    Restaurant error_url = new Restaurant();
            //    error_url.Name = response;
            //    error_url.ZipCode = "99999";
            //    Globals.yellowpagesLocalDataAllRuns.Add(run_url);
            //    Globals.yellowpagesLocalDataAllRuns.Add(error_url);
            //    Globals.webErrors = Globals.webErrors + response + "\n";
            //}
            //else
            //{
                response = response.Replace("\n", "");
                response = response.Replace("<div class=\"listing_content\">", "\n<div class=\"listing_content\">");
                //Trace.WriteLine(response);  

                Regex regNames = new Regex("<h3 class=\"business-name fn org\">+.*</h3>");
                Regex regRecData = new Regex("<span class=\"listing-address adr\">+.*<span class=\"additional-phones\">");                
                Regex regRating = new Regex("<span class=\"rating-+.*</span><span class=\"review-count\">");
                Regex regReviews = new Regex("<span class=\"review-count\">+.*</span>");                
                Regex regPins = new Regex("<div class=\"listing_content\">+.*<div class=\"info\">");
                Regex regNeighborhood = new Regex("<div class=\"neighborhoods\">+.*<div class=\"categories\">");
                //Regex regCuisine = new Regex("<div class=\"categories\">+.*<span class=\"geo hidden\">");
                Regex regCuisine = new Regex("<div class=\"categories\">.+?<span class=\"geo hidden\">");
                Regex regCuisineDetail = new Regex("nofollow\">.+?</a>");



                MatchCollection names = regNames.Matches(response);
                MatchCollection records = regRecData.Matches(response);
                MatchCollection ratings = regRating.Matches(response);
                MatchCollection reviews = regReviews.Matches(response);
                MatchCollection pins = regPins.Matches(response);
                MatchCollection neighborhoods = regNeighborhood.Matches(response);
                MatchCollection cuisines = regCuisine.Matches(response);

                String city = "", zipcode = "", addrStr = "", phone = "";

                //Trace.WriteLine("Names Count " + names.Count);
                for (int i = 0; i < names.Count; ++i)
                {
                    Restaurant restaurant = new Restaurant();
                    Match name = names[i];
                    Match rec = records[i];
                    Match rtg = ratings[i];
                    Match rv = reviews[i];
                    Match pn = pins[i];
                    //Match nh = neighborhoods[i];
                    //Match cui = cuisines[i];
                    //Trace.WriteLine(cui.Value);

                    if (!name.Value.Equals("") && pn.Value.IndexOf("<div class=\"rank pin\">") > 0)
                    {
                        //more string processesing to get the name                    
                        string nameStr = "";
                        nameStr = name.Value.Substring(name.Value.IndexOf("class=\"url \">") + 13, name.Value.IndexOf("</") - name.Value.IndexOf("class=\"url \">") - 13);
                        if (nameStr.Contains(">"))
                            nameStr = nameStr.Substring(nameStr.LastIndexOf(">") + 1, nameStr.Length - nameStr.LastIndexOf(">") - 1);

                        restaurant.Name = nameStr;
                        restaurant.Name = restaurant.Name.Replace("&amp;", "&");
                        //Trace.WriteLine(restaurant.Name);

                        //check if restaurant is closed
                        if (restaurant.Name.Contains("-- CLOSED"))
                        {
                            restaurant.IsClosed = true;
                            //we removed CLOSED from the name in order for edit distance to work correctly
                            restaurant.Name = restaurant.Name.Replace("-- CLOSED", "").Trim();
                        }

                        //more string processesing to get the address
                        addrStr = "";
                        if (rec.Value.Contains("<span class=\"street-address\">"))
                        {
                            //has address
                            addrStr = rec.Value.Substring(rec.Value.IndexOf("<span class=\"street-address\">"), rec.Value.IndexOf("</span>") - rec.Value.IndexOf("<span class=\"street-address\">") + 7);
                            addrStr = addrStr.Substring(addrStr.IndexOf('>') + 1, (addrStr.IndexOf("</span") - addrStr.IndexOf('>')) - 2);
                        }

                        city = "";
                        zipcode = "";
                        if (rec.Value.Contains("<span class=\"city-state\">"))
                        {
                            if (rec.Value.Contains("<span class=\"locality\">") && rec.Value.Contains(">Chicago<"))
                            {
                                city = "Chicago";
                            }

                            if (rec.Value.Contains("<span class=\"postal-code\">"))
                            {
                                zipcode = rec.Value.Substring(rec.Value.IndexOf("<span class=\"postal-code\">") + 26, 5);
                            }
                        }

                        if (city.Equals("")) //if restaurant is not in chicago then drop it
                            continue;

                        phone = rec.Value.Substring(rec.Value.IndexOf("<span class=\"business-phone phone\">") + 35,
                                                    rec.Value.LastIndexOf("</span>") - rec.Value.IndexOf("<span class=\"business-phone phone\">") - 35);

                        
                        //restaurant.Neighborhood = nh.Value.Substring(nh.Value.IndexOf("nofollow\">") + 10,
                        //                            nh.Value.IndexOf("</a>") - nh.Value.IndexOf("nofollow\">") - 10);
                        //Trace.WriteLine(restaurant.Neighborhood);
                       

                        //MatchCollection cuisineDetails = regCuisineDetail.Matches(cui.Value);
                        //Trace.WriteLine(cuisineDetails.Count);
                        //Trace.WriteLine(cuisineDetails[0].Value);

                       
                        //if (cuisineDetails.Count > 1)
                        //    restaurant.Cuisine = cuisineDetails[1].Value.Substring(cuisineDetails[1].Value.IndexOf("nofollow\">") + 10,
                        //                            cuisineDetails[1].Value.IndexOf("</a>") - cuisineDetails[1].Value.IndexOf("nofollow\">") - 10);
                       

                        restaurant.Address = addrStr;
                        restaurant.City = city;
                        restaurant.State = "IL";
                        restaurant.FullAddress = addrStr + ", " + city + " " + "IL";
                        restaurant.ZipCode = zipcode;
                        restaurant.PhoneNumber = phone;
                        restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.YellowPages;
                        restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE); 

                        string tmpstr = rtg.Value.Substring(rtg.Value.IndexOf('>') + 1, (rtg.Value.IndexOf("</span") - rtg.Value.IndexOf('>')) - 1);
                        tmpstr = tmpstr.Substring(tmpstr.IndexOf(">") + 1, 3);
                        restaurant.Rating = Convert.ToDouble(tmpstr);

                        if (rv.Value.IndexOf("(<span class=\"no-tracks count\">") > 0)
                        {
                            tmpstr = rv.Value.Substring(rv.Value.IndexOf("(<span class=\"no-tracks count\">") + 31, rv.Value.IndexOf("</span>)")
                                                        - rv.Value.IndexOf("(<span class=\"no-tracks count\">") - 31);
                            restaurant.NumReviews = Convert.ToInt16(tmpstr);
                        }
                        else
                            restaurant.NumReviews = 0;

                        Globals.yellowpagesLocalDataAllRuns.Add(restaurant);
                    }//end if
                }//end for loop
            //}




        }

        
    }
}
