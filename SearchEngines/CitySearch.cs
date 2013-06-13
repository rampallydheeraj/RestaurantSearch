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
    class CitySearch : ISearchEngine
    {
        //returns 20 results per page

        private int code;
        private string Name;
        //private int maxNumOfPages = 5;
        //private int maxNumResults = 20;
        //private string defaultLocNum = "58044";
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE);

        public CitySearch(int c, String n)
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
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/CitySearch_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/CitySearch_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
            //get { return "http://chicago.citysearch.com/listings/chicago-il/restaurant/"; }
            get { return "http://chicago.citysearch.com/listings/chicago-il-metro/"; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return null;
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {
            //DateTime startTime = DateTime.Now;

            //string locationKey = "";

            //if (location.Equals("All"))
            //{
            //    locationKey = defaultLocNum;
            //}
            //else
            //{
            //    locationKey = crosswalk[location];
            //}

            string LocKey;
            string CuiKey;

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

            string queryStr;
            string whatStr = keyword.Equals("") ? "restaurant" : keyword;
            if (!CuiKey.Equals("") && !LocKey.Equals(""))
            {
                //cuisine, and location specified
                if (keyword.Equals(""))
                {
                    //keyword is not specified
                    queryStr = BaseRestaurantUrl + CuiKey + "/" + LocKey + "?rpp=20";
                }
                else
                {
                    //keyword is specified
                    queryStr = BaseRestaurantUrl + CuiKey + "/" + LocKey + "?what=" + whatStr + "&rpp=20";
                }
            }
            else if (CuiKey.Equals("") && !LocKey.Equals(""))
            {
                //location specified
                queryStr = BaseRestaurantUrl + LocKey + "?what=" + whatStr + "&rpp=20";
            }
            else if (CuiKey.Equals("") && LocKey.Equals(""))
            {
                //neither location nor cuisnie are specified
                queryStr = BaseRestaurantUrl + "?what=" + whatStr + "&rpp=20";
            }
            else
            {
                if (keyword.Equals(""))
                {
                    //keyword is not specified
                    queryStr = BaseRestaurantUrl + CuiKey + LocKey + "?rpp=20";
                }
                else
                {
                    //keywors is specified
                    queryStr = BaseRestaurantUrl + CuiKey + LocKey + "?what=" + whatStr + "&rpp=20";
                }
            }


            //string queryStr;
            //string whatStr = keyword.Equals("") ? "restaurant" : keyword;
            //if (!CuiKey.Equals("") && !LocKey.Equals(""))
            //    queryStr = BaseRestaurantUrl + CuiKey + "/" + LocKey + "?rpp=20";
            //else if (CuiKey.Equals("") && !LocKey.Equals(""))
            //    queryStr = BaseRestaurantUrl + LocKey + "?what=restaurants&rpp=20";
            //else if (CuiKey.Equals("") && LocKey.Equals(""))
            //    queryStr = BaseRestaurantUrl + "?what=restaurants&rpp=20";
            //else
            //    queryStr = BaseRestaurantUrl + CuiKey + LocKey + "?rpp=20";

            string response = WebDocumentRetriever.GetDocument(queryStr, null);
            List<Restaurant> restaurants = new List<Restaurant>();

            Restaurant run_url = new Restaurant();
            run_url.Name = queryStr;
            run_url.ZipCode = "99999";

            if (response.Contains("YUMI_WEB_ERROR: "))
            {
                Restaurant error_url = new Restaurant();
                error_url.Name = response;
                error_url.ZipCode = "99999";
                restaurants.Add(run_url);
                restaurants.Add(error_url);
                Globals.webErrors = Globals.webErrors + response + "\n";
                return restaurants;
            }



            processRequestForPage(response, restaurants, location);

            //int currentCount = restaurants.Count;
            //int pageNum = 2;
            //do
            //{
            //    queryStr = BaseRestaurantUrl + locationKey + "?what=" + keyword + "&sortField=locCSRating_Sort-desc&rpp=20&page=" + pageNum;
            //    response = WebDocumentRetriever.GetDocument(queryStr, null);
            //    processRequestForPage(response, restaurants, location);
            //    pageNum = pageNum + 1;
            //}
            //while (restaurants.Count != currentCount && restaurants.Count < maxNumResults && pageNum < maxNumOfPages);

            //shrink results to match threshold
            //while (restaurants.Count > maxNumResults)
            //    restaurants.RemoveAt(maxNumResults);

            //Restaurant run_url = new Restaurant();
            //run_url.Name = queryStr;
            //run_url.ZipCode = "99999";
            restaurants.Insert(0, run_url);
            if (restaurants.Count == 1)
            {
                Restaurant message = new Restaurant();
                message.Name = "NO RESULTS FOR THIS REQUEST";
                message.ZipCode = "99999";
                restaurants.Add(message);
            }

            //DateTime EndTime = DateTime.Now;
            //TimeSpan span = EndTime.Subtract(startTime);
            //run_url.City = "Requests done in " + span.Seconds + " seconds";
            return restaurants;
        }

        public void processRequestForPage(string response, List<Restaurant> restaurants, string location)
        {
            Globals.citysearchTotalQueries++;
            //regex's to parse out the name and the address
            if (response.Contains("We did not find any results for"))
                return;
            response = response.Replace("\r", "");
            response = response.Replace("\n", "");
            response = response.Replace("Citysearch.TempConfig.Search.resultSet.push", "\nCitysearch.TempConfig.Search.resultSet.push");
            response = response.Replace("  ", "");

            Regex regGeoLoc = new Regex("'divId'.*'href'");
            Regex regRating = new Regex("'stars'.*</span>'");
            Regex regReviews = new Regex("<span class=\"reviewCount\">.*<p class=\"tags\">");
            //Regex regTags = new Regex("<p class=\"tags\">.*<p class=\"adr\">");
            Regex regTags = new Regex("<p class=\"tags\">.*<script type=\"text/javascript\">");
            Regex regHoods = new Regex("<p class=\"neighborhood\">.*</p>");
            
            MatchCollection geoLocs = regGeoLoc.Matches(response);
            MatchCollection ratings = regRating.Matches(response);
            MatchCollection reviews = regReviews.Matches(response);
            MatchCollection tags = regTags.Matches(response);
            MatchCollection hoods = regHoods.Matches(response);

            String name, address, rateStr, reviewsNum;
            //int maxRecords = geoLocs.Count < MergeResults.Threshold ? geoLocs.Count : MergeResults.Threshold;

            for (int i = 0; i < geoLocs.Count; ++i)
            {
                Restaurant restaurant = new Restaurant();
                Match loc = geoLocs[i];
                Match rate = ratings[i];
                Match reviewCount = reviews[i];
                Match tag = tags[i];
                //Match hood = hoods[i];   
                if (!tag.Value.Contains("Restaurants"))
                    continue;
                //string hoodStr = "";
                
                string tagStr = tag.Value.Substring(tag.Value.IndexOf("tags\">") + 6, tag.Value.IndexOf("</p>") - tag.Value.IndexOf("tags\">") - 6);
                //Trace.WriteLine(tagStr);
                string[] tagArr = tagStr.Split(new char[] { ',' });
                foreach (string tagString in tagArr)
                {
                    if (tagString.Trim().Equals("Restaurants") || tagString.Trim().Equals("Catering") || tagString.Trim().Equals("Food Delivery") || tagString.Trim().Equals("Dine At The Bar") || tagString.Trim().Equals("Carry Out"))
                        continue;
                    else
                    {
                        restaurant.Cuisine = tagString.Trim();
                        break;
                    }
                }
                //Trace.WriteLine(restaurant.Cuisine);

                //restaurant.Neighborhood = hood.Value.Substring(hood.Value.IndexOf("hood: ") + 6, hood.Value.IndexOf("</p>") - hood.Value.IndexOf("hood: ") - 6);
                //Trace.WriteLine(restaurant.Neighborhood);

                string city = tag .Value.Substring(tag.Value.IndexOf("<span class=\"locality\">"),
                                                  tag.Value.IndexOf("<span class=\"region\">") - tag.Value.IndexOf("<span class=\"locality\">"));

                //if (city.Contains("Chicago<"))
                //{
                //    hoodStr = tag.Value.Substring(tag.Value.IndexOf("<p class=\"neighborhood\">"),
                //              tag.Value.LastIndexOf("</p>") - tag.Value.IndexOf("<p class=\"neighborhood\">") + 4);                                       
                //}

                //if ((!hoodStr.Contains(location) || !tagStr.Contains("Restaurants")) && !location.Equals("All"))
                //    continue;

                //if (!city.Contains("Chicago<"))
                //    continue;

                //more string processesing to get the name                    
                name = loc.Value.Substring(loc.Value.IndexOf("'name'"), loc.Value.IndexOf("'address'") - loc.Value.IndexOf("'name'"));
                restaurant.Name = name.Substring(name.IndexOf('>') + 1, name.IndexOf("</a>") - name.IndexOf('>') - 1);
                restaurant.Name = restaurant.Name.Replace("\\'", "'");

                //check if restaurant is closed
                if (restaurant.Name.Contains("-- CLOSED"))
                {
                    restaurant.IsClosed = true;
                    //we removed CLOSED from the name in order for edit distance to work correctly
                    restaurant.Name = restaurant.Name.Replace("-- CLOSED", "").Trim();
                }

                //more string processesing to get the address
                address = loc.Value.Substring(loc.Value.IndexOf("'address'"), loc.Value.IndexOf("'href'") - loc.Value.IndexOf("'address'"));
                address = address.Substring(12, address.IndexOf("',") - 12);
                address = address.Replace("<br/>", " ");
                address = address.Replace("&nbsp;", ", ");
                address = address.Replace("'", "");
                restaurant.Address = address.Substring(0, address.IndexOf(",")).Trim();
                //restaurant.City = "Chicago";
                restaurant.City = city.Substring(city.IndexOf(">") + 1, city.IndexOf("</") - city.IndexOf(">") - 1);
                restaurant.State = "IL";
                restaurant.FullAddress = restaurant.Address + ", " + restaurant.City + " " + "IL";
                restaurant.ZipCode = address.Substring(address.IndexOf("IL,") + 4, address.Length - address.IndexOf("IL,") - 4);
                restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.CitySearch;
                restaurant.Criteria = constraints;
                
                //get ratings
                if (rate.Value.Equals("'stars' : '<span class=\"ratingReviews\"></span>'"))
                {
                    rateStr = "0";
                }
                else
                {
                    rateStr = rate.Value.Substring(rate.Value.IndexOf("alt=") + 5, rate.Value.IndexOf("Star Rating") - rate.Value.IndexOf("alt=") - 5);
                }

                restaurant.Rating = Convert.ToDouble(rateStr);

                //get num of reviews
                if(reviewCount.Value.Contains("writeReviewLink"))
                {
                    reviewsNum = "0";
                }
                else
                {


                    reviewsNum = reviewCount.Value.Substring(reviewCount.Value.IndexOf("citysearch.com/review/"),
                                                             reviewCount.Value.IndexOf("</a>") - reviewCount.Value.IndexOf("citysearch.com/review/"));

                    reviewsNum = reviewsNum.Substring(reviewsNum.IndexOf(">") + 1, reviewsNum.IndexOf("Review") - reviewsNum.IndexOf(">") - 1);
                    
                    
                }

                restaurant.NumReviews = Convert.ToInt16(reviewsNum);
                //restaurant.Ranking = i;

                restaurants.Add(restaurant);
            }
        }        
    }
}
