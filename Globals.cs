using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using Subgurim.Controles;

namespace yumi
{
    public static class Globals
    {
        public static int Threshold = 16;
        public static int MinData = 3;
        public static int MaxResults = 20;
        public static int TimeOutValue = 3000;
        public static int finalResultsCount = 0;

        //public static double nameEditDistanceRatio = .87;
        public static double nameEditDistanceRatio = .7;
        public static double addressEditDistanceRatio = .7;

        public static Color frameColor = Color.DodgerBlue;

        public static List<LocationList> hoods = new List<LocationList>();
        public static List<LocationList> shortListedNeighborHoods = new List<LocationList>();
        public static List<LocationList> hoodsAllIntersections = new List<LocationList>();
        public static SerializableDictionary<string, List<Point>> RectangleCoords = new SerializableDictionary<string, List<Point>>();
        public static List<CuisineList> cuisines = new List<CuisineList>();
        public static List<ISearchEngine> searchEngines = new List<ISearchEngine>();
        public static ISearchEngine goldStandard;
        public static List<List<Restaurant>> masterData;
        public static List<List<Restaurant>> allData = new List<List<Restaurant>>();
        public static List<List<Restaurant>> allDataEngineTables = new List<List<Restaurant>>();
        public static List<List<Point>> cornersAllSelectedLocs = new List<List<Point>>();
        public static List<int> enabledSE = new List<int>();
        public static List<string> urlQueries = new List<string>();
        public static int metromixTotalQueries;
        public static int dexknowsTotalQueries;
        public static int yelpTotalQueries;
        public static int chicagoreaderTotalQueries;
        public static int menuismTotalQueries;
        public static int menupagesTotalQueries;
        public static int yahooTotalQueries;
        public static int yellowpagesTotalQueries;
        public static int citysearchTotalQueries;
        public static int zagatTotalQueries;
        public static int numberGoogleRequests;
        public static string webErrors;
        public static int matches = 0;
        public static int finalResultsCountTotal = 0;
        public static double totalSecondsNeighborhood = 0;
        public static double totalSecondsArea = 0;
        public static double avgSecondsNeighborhood = 0;
        public static double avgSecondsArea = 0;
        public static int requestsNeighborhood = 0;
        public static int requestsArea = 0;
        public static List<Restaurant> metromixLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> metromixLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> metromixLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> dexknowsLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> dexknowsLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> dexknowsLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> yelpLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> yelpLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> yelpLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> chicagoreaderLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> chicagoreaderLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> chicagoreaderLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> menuismLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> menuismLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> menuismLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> menupagesLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> menupagesLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> menupagesLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> yahooLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> yahooLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> yahooLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> yellowpagesLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> yellowpagesLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> yellowpagesLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> citysearchLocalDataAllRuns = new List<Restaurant>();
        public static List<Restaurant> citysearchLocalDataMerged = new List<Restaurant>();
        public static List<Restaurant> citysearchLocalDataMergedSorted = new List<Restaurant>();
        public static List<Restaurant> zagatData = new List<Restaurant>();
        public static List<Restaurant> mergeList = new List<Restaurant>();
        public static List<GMap> Maps = new List<GMap>();
        public static SerializableDictionary<string, Point> Geocodes = new SerializableDictionary<string, Point>();
        public static Boolean isAuto = false;
        public static TextWriter w;
        public static string results_header;
        public static string location;
        public static string cuisine;
        public static List<Restaurant> results = new List<Restaurant>();
        public static int queryConstraints;
        public static List<Restaurant> closedRestaurants = new List<Restaurant>();
        public static List<String[]> inputRecords = new List<String[]>();
        public static DateTime startTime;
        public static int metromixSpan;
        public static int dexknowsSpan;
        public static int yelpSpan;
        public static int chicagoreaderSpan;
        public static int menuismSpan;
        public static int menupagesSpan;
        public static int yahooSpan;
        public static int yellowpagesSpan;
        public static int citysearchSpan;
        public static int zagatSpan;

        public static bool selected=true;
        
        public static String[] metromixList = new String [1000];
        public static String[] dexknowsList = new String[1000];
        public static String[] yelpList = new String[1000];
        public static String[] chicagoList = new String[1000];
        public static String[] menuList = new String[1000];
        public static String[] menuPagesList = new String[1000];
        public static String[] yahooList = new String[1000];
        public static String[] yellowList = new String[1000];
        public static String[] cityList = new String[1000];
        public static String[] zagatList = new String[1000];
        public static String[] baba = new String[1000];
        


        public static List<String> getLocations(String locName, int cd)
        {
            foreach (LocationList l in Globals.hoods)
            {
                if (locName.Equals(l.getName()))
                {
                    return l.getLocationsForSearchEngine(cd);
                }
            }

            return null;
        }

        public static int getLevel(String locName)
        {
            foreach (LocationList l in Globals.hoods)
            {
                if (locName.Equals(l.getName()))
                {                    
                    return l.Level;
                }
            }

            return 0;
        }

        public static List<String> getLocationsAllIntersections(String locName, int cd)
        {
            foreach (LocationList l in Globals.hoodsAllIntersections)
            {
                if (locName.Equals(l.getName()))
                {
                    return l.getLocationsForSearchEngine(cd);
                }
            }

            return null;
        }

        public static List<String> getCuisines(String cuiName, int cd)
        {
            foreach (CuisineList c in Globals.cuisines)
            {
                if (cuiName.Equals(c.getName()))
                {
                    return c.getCuisinesForSearchEngine(cd);
                }
            }

            return null;
        }

        public static void WriteOutput(string text)
        {
            using (StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("XML/Output.txt"), true))
            {
                writer.WriteLine(text);
            }
        }

        public static void copyList(List<Restaurant> targetList, List<Restaurant> sourceList)
        {
            foreach (Restaurant r in sourceList)
            {
                Restaurant r2 = new Restaurant();
                r2.FullAddress = r.FullAddress;
                r2.Address = r.Address;
                r2.City = r.City;
                r2.Name = r.Name;
                r2.NumReviews = r.NumReviews;
                r2.PhoneNumber = r.PhoneNumber;
                r2.PriceLvl = r.PriceLvl;
                r2.Ranking = r.Ranking;
                r2.Rating = r.Rating;
                r2.SearchEngine = r.SearchEngine;
                r2.State = r.State;
                r2.ZipCode = r.ZipCode;
                r2.Criteria = r.Criteria;
                r2.Latitude = r.Latitude;
                r2.Longitude = r.Longitude;
                r2.IsClosed = r.IsClosed;
                r2.Neighborhood = r.Neighborhood;
                r2.Cuisine = r.Cuisine;
                r2.Price = r.Price;
                targetList.Add(r2);
            }
        }

        public static Restaurant copyRestaurant(Restaurant r)
        {            
                Restaurant r2 = new Restaurant();
                r2.FullAddress = r.FullAddress;
                r2.Address = r.Address;
                r2.City = r.City;
                r2.Name = r.Name;
                r2.NumReviews = r.NumReviews;
                r2.PhoneNumber = r.PhoneNumber;
                r2.PriceLvl = r.PriceLvl;
                r2.Ranking = r.Ranking;
                r2.Rating = r.Rating;
                r2.SearchEngine = r.SearchEngine;
                r2.State = r.State;
                r2.ZipCode = r.ZipCode;
                r2.Criteria = r.Criteria;
                r2.Latitude = r.Latitude;
                r2.Longitude = r.Longitude;
                r2.IsClosed = r.IsClosed;
                r2.Neighborhood = r.Neighborhood;
                r2.Cuisine = r.Cuisine;
                r2.Price = r.Price;
                return r2;           
        }

        public static double EditDistance(string s1, string s2)
        {
            //performs the levenshtein distance algorithm, returns the ratio of similarity.
            if (s1 == null || s2 == null)
                return 0;
            s1 = s1.ToLower();
            s2 = s2.ToLower();

            if (s1.Equals(s2))
                return 1;

            int m = s1.Length;
            int n = s2.Length;

            int[,] matrix = new int[m + 1, n + 1];

            for (int l = 0; l <= m; ++l)
                matrix[l, 0] = l;

            for (int k = 0; k <= n; ++k)
                matrix[0, k] = k;

            for (int j = 1; j < n + 1; ++j)
            {
                for (int i = 1; i < m + 1; ++i)
                {
                    if (s1[i - 1] == s2[j - 1])
                        matrix[i, j] = matrix[i - 1, j - 1];
                    else
                        matrix[i, j] = Math.Min(
                                            Math.Min(matrix[i - 1, j], matrix[i, j - 1]),
                                            matrix[i - 1, j - 1]) + 1;
                }
            }


            return 1 - (matrix[m, n] / Convert.ToDouble(Math.Max(m, n)));
        }

    }
}
