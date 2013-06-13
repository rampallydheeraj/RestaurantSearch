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
    class Yahoo : ISearchEngine
    {
        private int code;
        private String Name;        
        //private int maxNumResults = 20;
        //private int maxNumPages = 5;
        private Dictionary<string, string> crosswalk;
        private Dictionary<string, string> location_crosswalk;
        private Dictionary<string, string> cuisine_crosswalk;
        private int constraints = (int)(Constraints.LOCATION | Constraints.CUISINE);

        public Yahoo(int c, String n)
        {
            code = c;
            Name = n;
            crosswalk = new Dictionary<string, string>();
            location_crosswalk = new Dictionary<string, string>();
            cuisine_crosswalk = new Dictionary<string, string>();
            GetLocationCrosswalkFromXml();
            GetCuisineCrosswalkFromXml();
            //GetCrosswalkFromXml();
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
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Yahoo_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                location_crosswalk.Add(c.Name, c.Key);
            }
        }

        public void GetCuisineCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Yahoo_Cuisine_crosswalk.xml"));
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys)
            {
                cuisine_crosswalk.Add(c.Name, c.Key);
            }
        }

        public string BaseRestaurantUrl
        {
            //get { return "http://local.yahoo.com/results?p=restaurants"; }
            get { return "http://local.yahoo.com/results?p="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return BaseRestaurantUrl + k + "&radius=3&sortby=drating+dnrating";
        }

        public void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword)
        {
            string LocKey;
            string CuiKey;
            List<string> queryURLs = new List<string>();

            foreach (string location in locations)
            {
                if (location.Equals("All"))
                {
                    LocKey = "";
                }
                else
                {
                    LocKey = "&neighborhoodfilt=" + location_crosswalk[location];
                }

                if (cuisine.Equals("All"))
                {
                    CuiKey = "";
                }
                else
                {
                    CuiKey = "&ycatfilt=" + cuisine_crosswalk[cuisine];
                }

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
                                    CuiKey = "&ycatfilt=" + entry.Value;
                                    break;
                                }
                            }
                            if (!CuiKey.Equals(""))
                                break;
                        }
                        else if (Globals.EditDistance(keyword.ToLower().Trim(), entry.Key.ToLower().Trim()) >= 0.8)
                        {
                            CuiKey = "&ycatfilt=" + entry.Value;
                            break;
                        }
                    }
                }
                //string queryStr = BaseRestaurantUrl + keyword + "&csz=Chicago%2C+IL" + locStr + "&radius=3&sortby=drating+dnrating&ycatfilt=96926236";
                string p = keyword.Equals("") ? "restaurants" : keyword;
                string queryStr = BaseRestaurantUrl + p + CuiKey + "&csz=Chicago%2C+IL" + LocKey + "&sortby=drating+dnrating";

                queryURLs.Add(queryStr);
                queryURLs.Add(queryStr + "&pg_nm=2");                
                
            }

            DownloadAll(queryURLs).ExecuteAndWait();

            for (int i = queryURLs.Count; i < Globals.yahooLocalDataAllRuns.Count; i++)
            {
                Globals.yahooLocalDataAllRuns[i].Ranking = i - Globals.yahooTotalQueries + 1;
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
                Globals.yahooLocalDataAllRuns.Insert(0, run_url);
            }

            Globals.yahooTotalQueries = queryURLs.Count;
        }

        static IEnumerable<IAsync> processRequestsAsync(string url)
        {
            Restaurant run_url = new Restaurant();
            run_url.Name = url;
            run_url.ZipCode = "99999";

            WebRequest req = HttpWebRequest.Create(url);

            // asynchronously get the response from http server
            Async<WebResponse> webresp = null;

            //try
            //{
            webresp = req.GetResponseAsync();
            //}
            //catch (WebException e)
            //{
                //Restaurant error_url = new Restaurant();
                //error_url.Name = "YUMI_WEB_ERROR: YAHOO" + e.Message;
                //error_url.ZipCode = "99999";
                //Globals.yahooLocalDataAllRuns.Add(run_url);
                //Globals.yahooLocalDataAllRuns.Add(error_url);
                //Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                //yield break;
            //}
            yield return webresp;

            Stream resp;
            if (webresp.Result != null)
                resp = webresp.Result.GetResponseStream();
            else
            {
                Restaurant error_url = new Restaurant();
                error_url.Name = "YUMI_WEB_ERROR: YAHOO";
                error_url.ZipCode = "99999";
                Globals.yahooLocalDataAllRuns.Add(run_url);
                Globals.yahooLocalDataAllRuns.Add(error_url);
                Globals.webErrors = Globals.webErrors + error_url.Name + "\n";
                yield break;
            }
            
        
            // download HTML using the asynchronous extension method
            // instead of using synchronous StreamReader
            Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
            yield return html;

            string response = html.Result;

            //Trace.WriteLine(response);
            string[] tmp = response.Split(new string[] { "<tr class=\"yls-rs-listinfo\">" }, StringSplitOptions.RemoveEmptyEntries);
            for (int tmp_count = 1; tmp_count < tmp.Length; tmp_count++)
            {
                Restaurant restaurant = new Restaurant();
                string tmp_name = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("content=") + 9);
                tmp_name = tmp_name.Replace("<b>", "");
                tmp_name = tmp_name.Replace("</b>", "");
                string tmp_address = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("vcard:street-address") + 31);
                string tmp_city = tmp_address.Substring(tmp_address.IndexOf("locality") + 19);
                string final_name = tmp_name.Substring(tmp_name.IndexOf(">") + 1, tmp_name.IndexOf("<") - tmp_name.IndexOf(">") - 1);
                string final_address = tmp_address.Substring(0, tmp_address.IndexOf(">") - 1);
                string final_city = tmp_city.Substring(0, tmp_city.IndexOf(">") - 1);
                string phone_number = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("tel") + 17, 14);
                //Trace.WriteLine(phone_number);

                restaurant.Name = final_name.Replace("&amp;", "&");
                restaurant.Address = final_address;
                restaurant.City = final_city;
                restaurant.State = "IL";
                restaurant.FullAddress = final_address + ", " + final_city + " " + "IL";
                restaurant.PhoneNumber = phone_number;
                restaurant.SearchEngine = restaurant.SearchEngine | (int)Engine.Yahoo;
                restaurant.Criteria = (int)(Constraints.LOCATION | Constraints.CUISINE); 

                if (tmp[tmp_count].IndexOf("Average Rating") > -1)
                {
                    string test1 = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("Average Rating") + 16);
                    string test2;
                    string numReviews = "";
                    double rating;
                    int reviews = 0;
                    try
                    {
                        test2 = test1.Substring(0, test1.IndexOf("out") - 1);
                        //numReviews = test1.Substring(test1.IndexOf("("), test1.IndexOf(")") - test1.IndexOf("("));

                    }
                    catch
                    {
                        test1 = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("User Rating:") + 12);
                        test2 = test1.Substring(test1.IndexOf("(") + 1, test1.IndexOf(")") - test1.IndexOf("(") - 1);
                    }
                    try
                    {
                        rating = Convert.ToDouble(test2);
                        //reviews = Convert.ToInt16(numReviews);
                    }
                    catch
                    {
                        if (tmp[tmp_count].IndexOf("User Rating:") > 0)
                        {
                            test1 = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("User Rating:") + 12);
                            test2 = test1.Substring(test1.IndexOf("(") + 1, test1.IndexOf(")") - test1.IndexOf("(") - 1);
                            rating = Convert.ToDouble(test2);
                        }
                        else
                        {
                            rating = 0.0;
                        }
                    }

                    test1 = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("Average Rating") + 16);
                    numReviews = test1.Substring(test1.IndexOf("(") + 1, test1.IndexOf(")") - test1.IndexOf("(") - 1);
                    reviews = Convert.ToInt16(numReviews);
                    restaurant.Rating = rating;
                    restaurant.NumReviews = reviews;
                }
                else
                {
                    restaurant.Rating = 0.0;
                    restaurant.NumReviews = 0;
                }

                Globals.yahooLocalDataAllRuns.Add(restaurant);
            }
        }

        public List<Restaurant> processRequest(string location, string cuisine, string price, string keyword)
        {            
            return null;
        }

    }
}
