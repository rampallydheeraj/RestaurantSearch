
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Diagnostics;


namespace yumi
{
    [Flags]
    public enum Constraints
    {
        LOCATION = 0x01,
        CUISINE = 0x02,
        PRICE = 0x04,
        FEATURE = 0x08,
    }

    [Flags]
    public enum Rectangle
    {
        TARGET = 1,
        CHOSEN = 2,
        NOTCHOSEN = 4,
        YUMITARGET = 8,
        YUMIADDITIONAL = 16,
    }

    [Flags]
    public enum Engine
    {
        Metromix = 1,
        DexKnows = 2,
        Yelp = 4,
        ChicagoReader = 8,
        Menuism = 16,
        MenuPages = 32,
        Yahoo = 64,
        YellowPages = 128,
        CitySearch = 256,
        Zagat = 512,
    }

    public class Point
    {
        public double x;
        public double y;

        public Point()
        {
            this.x = 0;
            this.y = 0;
        }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

    }


    public class PointListComparer : IEqualityComparer<List<Point>>
    {

        #region IEqualityComparer<List<Point>> Members
        bool IEqualityComparer<List<Point>>.Equals(List<Point> a, List<Point> b)
        {
            return (a[0].x == b[0].x && a[0].y == b[0].y && a[1].x == b[1].x && a[1].y == b[1].y);
        }
        int IEqualityComparer<List<Point>>.GetHashCode(List<Point> obj)
        {
            return obj[0].x.GetHashCode();
        }
        #endregion
    }

    public class SearchEngineThread
    {
        private string name;
        private string key;
        private string price;
        private int code;
        private string keyword;

        public SearchEngineThread()
        {
            name = "";
            key = "";
            code = 0;            
        }

        public SearchEngineThread(string n, string k, string p, int c, string kw)
        {
            name = n;
            key = k;
            code = c;
            price = p;
            keyword = kw;
        }
        
        public void process()
        {
            List<Restaurant> localData = new List<Restaurant>();
            List<String> seHoods=null;
            List<String> seCuisines;
            int maxRunCount = 0;

            if (name.Equals("All"))
            {
                seHoods = new List<String>();
                seHoods.Add(name);
            }
            else
            {
                List<String> s = new List<String>();
                if (Globals.selected == true)
                {
                    
                  if (code != 9)
                    seHoods = Globals.getLocations(name, Globals.searchEngines[code].getCode);
                  else
                    seHoods = Globals.getLocations(name, 10);
            
                }
                /*
                if (code != 9)
                {
                    seHoods = Globals.getLocations(name, Globals.searchEngines[code].getCode);
                    */




                if (code == 0 && Globals.selected == false)
                {
                    
                    for (int i = 0; i < Globals.metromixList.Length; i++)
                    {
                        
                        if (!s.Contains(Globals.metromixList[i]))
                        {
                            //Globals.metromixList[i] = Globals.metromixList[i].Substring(0, Globals.metromixList[i].Length - 1);
                            s.Add(Globals.metromixList[i]);
                        }
                    }
                    seHoods = s;
                }

                else if (code == 1 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.dexknowsList.Length; i++)
                    {
                        if (!s.Contains(Globals.dexknowsList[i]))
                            s.Add(Globals.dexknowsList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 2 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.yelpList.Length; i++)
                    {
                        if (!s.Contains(Globals.yelpList[i]))
                            s.Add(Globals.yelpList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 3 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.chicagoList.Length; i++)
                    {
                        if (!s.Contains(Globals.chicagoList[i]))
                            s.Add(Globals.chicagoList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 4 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.menuList.Length; i++)
                    {
                        if (!s.Contains(Globals.menuList[i]))
                            s.Add(Globals.menuList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 5 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.menuPagesList.Length; i++)
                    {
                        if (!s.Contains(Globals.menuPagesList[i]))
                            s.Add(Globals.menuPagesList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 6 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.yahooList.Length; i++)
                    {
                        if (!s.Contains(Globals.yahooList[i]))
                            s.Add(Globals.yahooList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 7 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.yellowList.Length; i++)
                    {
                        if (!s.Contains(Globals.yellowList[i]))
                            s.Add(Globals.yellowList[i]);
                    }
                    seHoods = s;
                }
                else if (code == 8 && Globals.selected == false)
                {
                    for (int i = 0; i < Globals.cityList.Length; i++)
                    {
                        if (!s.Contains(Globals.cityList[i]))
                            s.Add(Globals.cityList[i]);
                    }
                    seHoods = s;
                }

                else 
                {
                    if (code == 9 && Globals.selected == false)
                    {
                    //seHoods = Globals.getLocations(name, 10);
                    for (int i = 0; i < Globals.zagatList.Length; i++)
                    {
                        if (!s.Contains(Globals.zagatList[i]))
                            s.Add(Globals.zagatList[i]);
                    }
                    seHoods = s;
                    }
                }
            
                
            }
            if (seHoods.Count == 0)
            {
                Restaurant message = new Restaurant();
                message.Name = "NO CORRESPONDING NEIGHBORHOOD IN THIS ENGINE";
                message.ZipCode = "99999";
                localData.Add(message);
                if (code == 1)
                    Globals.dexknowsLocalDataAllRuns.Add(message);      
                if (code == 2)
                    Globals.yelpLocalDataAllRuns.Add(message);
                if (code == 3)
                    Globals.chicagoreaderLocalDataAllRuns.Add(message);
                if (code == 4)                
                    Globals.menuismLocalDataAllRuns.Add(message);
                if (code == 5)
                    Globals.menupagesLocalDataAllRuns.Add(message);
                if (code == 6)
                    Globals.yahooLocalDataAllRuns.Add(message);
                if (code == 7)
                    Globals.yellowpagesLocalDataAllRuns.Add(message);
                if (code == 8)
                    Globals.citysearchLocalDataAllRuns.Add(message);
                return;
            }
            if (key.Equals("All"))
            {
                seCuisines = new List<String>();
                seCuisines.Add(key);
            }
            else
            {
                if (code != 9)
                    seCuisines = Globals.getCuisines(key, Globals.searchEngines[code].getCode);
                else
                    seCuisines = Globals.getCuisines(key, 10);
            }
            if (seCuisines.Count == 0)
            {
                Restaurant message = new Restaurant();
                message.Name = "NO CORRESPONDING CUISINE IN THIS ENGINE";
                message.ZipCode = "99999";
                localData.Add(message);
                if (code == 1)
                    Globals.dexknowsLocalDataAllRuns.Add(message);
                if (code == 2)
                    Globals.yelpLocalDataAllRuns.Add(message);
                if (code == 3)
                    Globals.chicagoreaderLocalDataAllRuns.Add(message);
                if (code == 4)
                    Globals.menuismLocalDataAllRuns.Add(message);
                if (code == 5)
                    Globals.menupagesLocalDataAllRuns.Add(message);
                if (code == 6)
                    Globals.yahooLocalDataAllRuns.Add(message);
                if (code == 7)
                    Globals.yellowpagesLocalDataAllRuns.Add(message);
                if (code == 8)
                    Globals.citysearchLocalDataAllRuns.Add(message);
                return;
            }

            DateTime startTime = DateTime.Now;
            if (code == 0)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 1)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 2)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);   
            }
            else if (code == 3)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 4)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 5)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 6)
            {
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 7)
            {
                //Trace.WriteLine("Yellowpages Code Received");
                Globals.searchEngines[code].createRequestsAsync(seHoods, seCuisines[0], price, keyword);
            }
            else if (code == 8)
            {
                for (int j = 0; j < seHoods.Count; j++)
                {
                    List<Restaurant> currentRun = Globals.searchEngines[code].processRequest(seHoods[j], seCuisines[0], price, keyword);

                    if (currentRun.Count > maxRunCount)
                        maxRunCount = currentRun.Count;
                    for (int i = 1; i < currentRun.Count; i++)
                    {
                        currentRun[i].Ranking = i;
                    }
                    MergeResults.mergeSubListsKeepDupes(Globals.citysearchLocalDataAllRuns, currentRun);
                    MergeResults.mergeSubLists(localData, currentRun);
                }
            }
            else if (code == 9)
            {
                //Trace.WriteLine("Zagat Code Received");
                //Globals.goldStandard.createRequestsAsync(seHoods, seCuisines[0], price, keyword);
                for (int j = 0; j < seHoods.Count; j++)
                {
                    List<Restaurant> currentRun = Globals.goldStandard.processRequest(seHoods[j], seCuisines[0], price, keyword);

                    if (currentRun.Count > maxRunCount)
                        maxRunCount = currentRun.Count;
                    for (int i = 1; i < currentRun.Count; i++)
                    {
                        currentRun[i].Ranking = i;
                    }
                    //currentRun.RemoveAll(r => r.ZipCode.Equals("99999"));
                    MergeResults.mergeSubListsKeepDupes(Globals.zagatData, currentRun);
                }
                Globals.zagatTotalQueries = seHoods.Count;
            }
            

            if (code == 0)
            {
                MergeResults.mergeSubLists(localData, Globals.metromixLocalDataAllRuns);
            }
            if (code == 1)
            {
                MergeResults.mergeSubLists(localData, Globals.dexknowsLocalDataAllRuns);
            }
            if (code == 2)
            {
                MergeResults.mergeSubLists(localData, Globals.yelpLocalDataAllRuns);
            }
            if (code == 3)
            {
                MergeResults.mergeSubLists(localData, Globals.chicagoreaderLocalDataAllRuns);
            }
            if (code == 4)
            {
                MergeResults.mergeSubLists(localData, Globals.menuismLocalDataAllRuns);
            }
            if (code == 5)
            {
                MergeResults.mergeSubLists(localData, Globals.menupagesLocalDataAllRuns);
            }
            if (code == 6)
            {
                MergeResults.mergeSubLists(localData, Globals.yahooLocalDataAllRuns);
            }
            if (code == 7)
            {
                MergeResults.mergeSubLists(localData, Globals.yellowpagesLocalDataAllRuns);   
            }
            //if (code == 8)
            //{
            //    MergeResults.mergeSubListsKeepDupes(localData, Globals.citysearchLocalDataAllRuns);
            //}
            localData.RemoveAll(r => r.ZipCode.Equals("99999"));

            DateTime EndTime = DateTime.Now;
            TimeSpan span = EndTime.Subtract(startTime);            

            if (code == 0)
            {
                Globals.metromixSpan = span.Seconds;
                Globals.metromixLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.metromixLocalDataAllRuns[0].Address = Globals.metromixTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.metromixLocalDataMerged, localData);                
                Globals.copyList(Globals.allData[code], localData);
            }
            else if (code == 1)
            {
                Globals.dexknowsSpan = span.Seconds;
                Globals.dexknowsLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.dexknowsLocalDataAllRuns[0].Address = Globals.dexknowsTotalQueries.ToString() + " requests"; ;
                //MergeResults.mergeSubLists(Globals.dexknowsLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                                
            }
            else if (code == 2)
            {
                Globals.yelpSpan = span.Seconds;
                Globals.yelpLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.yelpLocalDataAllRuns[0].Address = Globals.yelpTotalQueries.ToString() + " requests"; 
                //MergeResults.mergeSubLists(Globals.yelpLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 3)
            {
                Globals.chicagoreaderSpan = span.Seconds;
                Globals.chicagoreaderLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.chicagoreaderLocalDataAllRuns[0].Address = Globals.chicagoreaderTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.chicagoreaderLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 4)
            {
                Globals.menuismSpan = span.Seconds;
                Globals.menuismLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.menuismLocalDataAllRuns[0].Address = Globals.menuismTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.menuismLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 5)
            {
                Globals.menupagesSpan = span.Seconds;
                Globals.menupagesLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.menupagesLocalDataAllRuns[0].Address = Globals.menupagesTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.menupagesLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);
            }
            else if (code == 6)
            {
                Globals.yahooSpan = span.Seconds;
                Globals.yahooLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.yahooLocalDataAllRuns[0].Address = Globals.yahooTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.yahooLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 7)
            {
                Globals.yellowpagesSpan = span.Seconds;
                Globals.yellowpagesLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.yellowpagesLocalDataAllRuns[0].Address = Globals.yellowpagesTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.yellowpagesLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 8)
            {
                Globals.citysearchSpan = span.Seconds;
                Globals.citysearchLocalDataAllRuns[0].City = "requests done in " + span.Seconds + " seconds";
                Globals.citysearchLocalDataAllRuns[0].Address = Globals.citysearchTotalQueries.ToString() + " requests";
                //MergeResults.mergeSubLists(Globals.citysearchLocalDataMerged, localData);
                Globals.copyList(Globals.allData[code], localData);                
            }
            else if (code == 9)
            {
                Globals.zagatSpan = span.Seconds;
            }
            
                                  
        }


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public int Code
        {
            get { return code; }
            set { code = value; }
        }
    }
}