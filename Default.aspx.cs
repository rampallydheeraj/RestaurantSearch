using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Timers;
using Subgurim.Controles;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Net;
using System.Xml;


namespace yumi
{
    public partial class _Default : System.Web.UI.Page
    {
        //global variables        
        //public static List<int> enabledSE = new List<int>(); 
        
        string[] xcoordinates = new string[500]; XmlReader reader1;
        string[] ycoordinates = new string[500]; int l = 454; //loop should run these many times
        
        int i1 = 0, j = 0, k = 0;//loop variables

        GInfoWindow window;

        int length;
        string[] nn = new string[300];//nn for neighborhood name
        string[] neighborhood = new string[300];//same as nn but populated with selected ones
        string[] shortListednn = new string[300];//only selected ones
        string[] childList = new string[200];

        double dist,latitude,longitude,maxElement;//dist is radius, latitude and longitude of center and x1 is minimum element in the array of distances
        double dlong, dlat, a, c, d;//used for distance calculation

        string s1;
        double[] distance = new double[500];//storing the 4 distances between the center and 4 edges

        bool c1, c2;
        GLatLng latlong=null;
        
        double x1, x2, x3;
        double y1, y2, y3;
        GMarker marker;
        
        double x4, x5;
        double y4, y5;
        double dx, dy, A, B, C, det;

        int m = 0, d1 = 0, y = 0, ch = 0, me = 0, mp = 0, ya = 0, ye = 0, ci = 0, za = 0;
     
        public static bool hideFlag = false;
        public static List<Thread> threads;
        ArrayList locDisabledList;
        ArrayList cuiDisabledList;
        

        


        protected void Page_Load(object sender, EventArgs e)
        {
            CheckBoxList1.Visible = true;            

            //if (ViewState["locDisabledListInViewState"] != null)
            //{
            //    locDisabledList = (ArrayList)ViewState["locDisabledListInViewState"];
            //    foreach (int i in locDisabledList)
            //    {
            //        DropDownList1.Items[i].Attributes.Add("disabled", "disabled");
            //    }
            //}
            if (ViewState["cuiDisabledListInViewState"] != null)
            {
                cuiDisabledList = (ArrayList)ViewState["cuiDisabledListInViewState"];
                foreach (int i in cuiDisabledList)
                {
                    DropDownList3.Items[i].Attributes.Add("disabled", "disabled");
                }
            }


            if (!IsPostBack)
            {
                //crearte search engine                
                Globals.searchEngines.Clear();
                Globals.searchEngines.Add(new Metromix(1, "Metromix"));
                Globals.searchEngines.Add(new DexKnows(2, "DexKnows"));
                Globals.searchEngines.Add(new Yelp(3, "Yelp"));
                Globals.searchEngines.Add(new ChicagoReader(4, "ChicagoReader"));
                Globals.searchEngines.Add(new Menuism(5, "Menuism"));
                Globals.searchEngines.Add(new MenuPages(6, "MenuPages"));
                Globals.searchEngines.Add(new Yahoo(7, "Yahoo"));
                Globals.searchEngines.Add(new YellowPages(8, "YellowPages"));
                Globals.searchEngines.Add(new CitySearch(9, "CitySearch"));
                Globals.goldStandard = new Zagat(10, "Zagat");


                Globals.allData.Clear();
                for (int i = 0; i < 9; i++)
                {
                    Globals.allData.Add(new List<Restaurant>());
                }


                //get neighborhoods
                Globals.hoods.Clear();
                Globals.cuisines.Clear();
                ProcessMergeFile(Globals.hoods, "~/merge/mergedHierarchyAnalysis.csv");
                //ProcessMergeFile(Globals.hoods, "~/merge/mergedHierarchyAllIntersections.csv");
                ProcessMergeFile(Globals.hoodsAllIntersections, "~/merge/mergedHierarchyAllIntersections.csv");
                ProcessMergeFileCuisine();



                XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, List<Point>>));
                StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("XML/NeighborhoodBoundingRectangles.xml"));
                Globals.RectangleCoords = serializer.Deserialize(reader) as SerializableDictionary<string, List<Point>>;
                reader.Dispose();

                XmlSerializer geoserializer = new XmlSerializer(typeof(SerializableDictionary<string, Point>));
                TextReader georeader = new StreamReader(HttpContext.Current.Server.MapPath("XML/StoredGeocodes.xml"));
                Globals.Geocodes = geoserializer.Deserialize(georeader) as SerializableDictionary<string, Point>;
                reader.Dispose();


                DropDownList1.Items.Clear();
                DropDownList1.Items.Add("All");
              

                StringWriter writer = new StringWriter();
                string padding = "&nbsp; &nbsp; &nbsp;";
                Server.HtmlDecode(padding, writer);
                padding = writer.ToString();
                locDisabledList = new ArrayList();

                for (int j = 0; j < Globals.hoods.Count(); j++)
                {
                    DropDownList1.Items.Add(Globals.hoods[j].getName());
                    DropDownList1.Items[j + 1].Text = padding + Globals.hoods[j].getName();
                    DropDownList1.Items[j + 1].Value = Globals.hoods[j].getName();
                    if (Globals.hoods[j].Level == 1)
                    {
                        DropDownList1.Items[j + 1].Text = Globals.hoods[j].getName();
                    }

                }


                DropDownList2.Items.Add("");
                DropDownList2.Items.Add("$");
                DropDownList2.Items.Add("$$");
                DropDownList2.Items.Add("$$$");
                DropDownList2.Items.Add("$$$$");
                DropDownList2.Items.Add("$$$$$");


                DropDownList3.Items.Clear();
                DropDownList3.Items.Add("All");

                cuiDisabledList = new ArrayList();
                for (int j = 0; j < Globals.cuisines.Count(); j++)
                {
                    DropDownList3.Items.Add(Globals.cuisines[j].getName());
                    DropDownList3.Items[j + 1].Text = padding + Globals.cuisines[j].getName();
                    DropDownList3.Items[j + 1].Value = Globals.cuisines[j].getName();
                    if (Globals.cuisines[j].Level == 1)
                    {
                        DropDownList3.Items[j + 1].Text = Globals.cuisines[j].getName();
                    }
                    if (Globals.cuisines[j].getCuisinesCount() == 1 || Globals.cuisines[j].Level == 1)
                    {
                        DropDownList3.Items[j + 1].Attributes.Add("disabled", "disabled");
                        cuiDisabledList.Add(j + 1);
                    }
                }


                //create searchengines check boxes
                CheckBoxList1.Items.Clear();
                for (int i = 0; i < Globals.searchEngines.Count; i++)
                {
                    CheckBoxList1.Items.Add(Globals.searchEngines[i].getName);
                    if (i == 0 || i == 1 || i == 2 || i == 4 || i == 5 || i == 6 || i == 8)
                        CheckBoxList1.Items[i].Selected = true;
                }

                CheckBoxList1.Style["display"] = "none";
                Table1.Visible = false;
                Table2.Visible = false;


            }
        }

        void Page_PreRender(object sender, EventArgs e)
        {
            //ViewState.Add("locDisabledListInViewState", locDisabledList);
            ViewState.Add("cuiDisabledListInViewState", cuiDisabledList);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //Boolean resumeQueries = false;
            //foreach (ListItem hood in DropDownList1.Items)
            //{
            //    DropDownList1.SelectedValue = hood.Value;
            //    if (DropDownList1.SelectedValue.Equals("All"))
            //        continue;
            //    //if (hood.Value.Contains("Summit"))
            //    //        resumeQueries = true;
            //    //if (resumeQueries)
            //    //{
            //        ((IPostBackEventHandler)Button1).RaisePostBackEvent(null);
            //        //Globals.WriteOutput("  Results: " + Globals.finalResultsCount);
            //        Thread.Sleep(15000);
            //    //}
            //}
            //foreach (ListItem cuisine in DropDownList3.Items)
            //{
            //    if (cuisine.Attributes.Count != 1)
            //    {
            //        DropDownList3.SelectedValue = cuisine.Value;
            //        ((IPostBackEventHandler)Button1).RaisePostBackEvent(null);
            //    }
            //    Thread.Sleep(5000);
            //}
            //int queryCount = 0;
            ////Boolean resumeQueries = false;
            //foreach (ListItem hood in DropDownList1.Items)
            //{
            //    foreach (ListItem cuisine in DropDownList3.Items)
            //    {
            //        if (cuisine.Attributes.Count != 1)
            //        {
            //            DropDownList1.SelectedValue = hood.Value;
            //            DropDownList3.SelectedValue = cuisine.Value;
            //            //if (hood.Value.Contains("Hyde Park/University Of Chicago") && cuisine.Value.Contains("Italian"))
            //            //    resumeQueries = true;
            //            //if (resumeQueries)
            //            //{
            //                ((IPostBackEventHandler)Button1).RaisePostBackEvent(null);
            //                queryCount++;
            //                Thread.Sleep(6000);
            //            //}
            //        }
            //    }
            //    if (queryCount > 500)
            //        break;
            //}


            //ProcessBatchFile();
            //foreach (String[] inputRecord in Globals.inputRecords)
            //{
            //    if (inputRecord[2].Equals("-1000"))
            //    {

            //    }
            //    else
            //    {
            //        DropDownList1.SelectedValue = DropDownList1.Items.FindByValue(inputRecord[0]).Value;
            //        DropDownList3.SelectedValue = DropDownList3.Items.FindByValue(inputRecord[1]).Value;
            //        ((IPostBackEventHandler)Button1).RaisePostBackEvent(null);
            //        Thread.Sleep(20000);
            //    }

            //}
            //Globals.finalResultsCountTotal = Globals.finalResultsCountTotal + Globals.finalResultsCount;
            //Globals.WriteOutput("Total matches: " + Globals.matches);
            //Globals.WriteOutput("Total results: " + Globals.finalResultsCountTotal);
            //Globals.WriteOutput("Requests neighborhood: " + Globals.requestsNeighborhood);
            //Globals.WriteOutput("Requests area: " + Globals.requestsArea);
            //Globals.WriteOutput("Avg response neighborhood: " + Globals.totalSecondsNeighborhood / (Globals.requestsNeighborhood - 1));
            //Globals.WriteOutput("Avg response area: " + Globals.totalSecondsArea / Globals.requestsArea);
            //Globals.WriteOutput(Globals.webErrors);
            //Response.Write("<script>alert('Test Done')</script>");
        }
        
        protected void Buttonuser_Click(object sender, EventArgs e)
        {
            if (DropDownList1.SelectedValue == "Location")
            {
                Locate_Click(sender, e);
                Button1_Click(sender, e);
            }
            else
            {
                Button1_Click(sender, e);
            }

        }
          
        protected void Button1_Click(object sender, EventArgs e)
        {
            //from here u need to remove comments
            CheckBoxList1.Style["display"] = "none";
            Table1.Rows.Clear();
            Table2.Rows.Clear();
            Table3.Rows.Clear();
            Table4.Rows.Clear();
            Table5.Rows.Clear();
            Table6.Rows.Clear();
            Table7.Rows.Clear();
            Table8.Rows.Clear();
            Table9.Rows.Clear();
            Table10.Rows.Clear();
            Table11.Rows.Clear();
            Table12.Rows.Clear();
            Table13.Rows.Clear();
            Table14.Rows.Clear();
            Table15.Rows.Clear();
            Table16.Rows.Clear();
            Table17.Rows.Clear();
            Table18.Rows.Clear();
            Table19.Rows.Clear();
            Table20.Rows.Clear();
            Globals.metromixLocalDataAllRuns.Clear();
            Globals.metromixLocalDataMerged.Clear();
            Globals.metromixLocalDataMergedSorted.Clear();
            Globals.dexknowsLocalDataAllRuns.Clear();
            Globals.dexknowsLocalDataMerged.Clear();
            Globals.dexknowsLocalDataMergedSorted.Clear();
            Globals.yelpLocalDataAllRuns.Clear();
            Globals.yelpLocalDataMerged.Clear();
            Globals.yelpLocalDataMergedSorted.Clear();
            Globals.chicagoreaderLocalDataAllRuns.Clear();
            Globals.chicagoreaderLocalDataMerged.Clear();
            Globals.chicagoreaderLocalDataMergedSorted.Clear();
            Globals.menuismLocalDataAllRuns.Clear();
            Globals.menuismLocalDataMerged.Clear();
            Globals.menuismLocalDataMergedSorted.Clear();
            Globals.menupagesLocalDataAllRuns.Clear();
            Globals.menupagesLocalDataMerged.Clear();
            Globals.menupagesLocalDataMergedSorted.Clear();
            Globals.yahooLocalDataAllRuns.Clear();
            Globals.yahooLocalDataMerged.Clear();
            Globals.yahooLocalDataMergedSorted.Clear();
            Globals.yellowpagesLocalDataAllRuns.Clear();
            Globals.yellowpagesLocalDataMerged.Clear();
            Globals.yellowpagesLocalDataMergedSorted.Clear();
            Globals.citysearchLocalDataAllRuns.Clear();
            Globals.citysearchLocalDataMerged.Clear();
            Globals.citysearchLocalDataMergedSorted.Clear();
            Globals.zagatData.Clear();
            Globals.results.Clear();
            Globals.cornersAllSelectedLocs.Clear();
            Globals.closedRestaurants.Clear();
            Globals.mergeList.Clear();

            Globals.metromixTotalQueries = 0;
            Globals.dexknowsTotalQueries = 0;
            Globals.yelpTotalQueries = 0;
            Globals.chicagoreaderTotalQueries = 0;
            Globals.menuismTotalQueries = 0;
            Globals.menupagesTotalQueries = 0;
            Globals.yahooTotalQueries = 0;
            Globals.yellowpagesTotalQueries = 0;
            Globals.citysearchTotalQueries = 0;
            Globals.zagatTotalQueries = 0;
            Globals.metromixSpan = 0;
            Globals.dexknowsSpan = 0;
            Globals.yelpSpan = 0;
            Globals.chicagoreaderSpan = 0;
            Globals.menuismSpan = 0; ;
            Globals.menupagesSpan = 0; ;
            Globals.yahooSpan = 0;
            Globals.yellowpagesSpan = 0;
            Globals.citysearchSpan = 0;
            Globals.zagatSpan = 0;

            GMap1.reset();
            GMap2.reset();
            GMap3.reset();
            GMap4.reset();
            GMap4.reset();
            GMap5.reset();
            GMap6.reset();
            GMap7.reset();
            GMap8.reset();
            GMap9.reset();
            GMap10.reset();


            MultiView1.ActiveViewIndex = 0;
            Menu1.Items[0].ImageUrl = "/images/yumiselectedtab.gif";
            Menu1.Items[0].Selected = true;
            for (int i = 1; i < Menu1.Items.Count; i++)
            {
                if (i == 1)
                    this.Menu1.Items[i].ImageUrl = "/images/metromixunselectedtab.gif";
                else if (i == 2)
                    this.Menu1.Items[i].ImageUrl = "/images/dexknowsunselectedtab.gif";
                else if (i == 3)
                    this.Menu1.Items[i].ImageUrl = "/images/yelpunselectedtab.gif";
                else if (i == 4)
                    this.Menu1.Items[i].ImageUrl = "/images/chicagoreaderunselectedtab.gif";
                else if (i == 5)
                    this.Menu1.Items[i].ImageUrl = "/images/menuismunselectedtab.gif";
                else if (i == 6)
                    this.Menu1.Items[i].ImageUrl = "/images/menupagesunselectedtab.gif";
                else if (i == 7)
                    this.Menu1.Items[i].ImageUrl = "/images/yahoounselectedtab.gif";
                else if (i == 8)
                    this.Menu1.Items[i].ImageUrl = "/images/yellowpagesunselectedtab.gif";
                else if (i == 9)
                    this.Menu1.Items[i].ImageUrl = "/images/citysearchunselectedtab.gif";
                Menu1.Items[i].Selected = false;
            }

            Globals.queryConstraints = 0;
            Globals.allData.Clear();
            for (int i = 0; i < 9; i++)
            {
                Globals.allData.Add(new List<Restaurant>());
            }
            Globals.allDataEngineTables.Clear();
            for (int i = 0; i < 9; i++)
            {
                Globals.allDataEngineTables.Add(new List<Restaurant>());
            }

            
            if (Globals.selected == false)
            {
                for (int i = 0; i < Globals.shortListedNeighborHoods.Count(); i++)
                {
                    List<Point> chosenLoc = Globals.RectangleCoords["Metromix" + "/" + Globals.shortListedNeighborHoods[i].getName()];
                    if (Contains(chosenLoc, latlong))
                    {
                        Globals.location = Globals.shortListedNeighborHoods[i].getName();
                        break;
                    }
                    else
                    {
                        Globals.location = Globals.shortListedNeighborHoods[0].getName();
                    }
                }
            }
                
            else
            {
                //Label1.Text = "entered into " + Globals.selected.ToString();
                Globals.location = DropDownList1.SelectedValue;
            }
            string price = DropDownList2.SelectedValue;
            string cuisine = DropDownList3.SelectedValue;
            string keyword = TextBox1.Text;


            if (Globals.location.Equals("All") && cuisine.Equals("All") && price.Equals("") && keyword.Equals(""))
            {
                TableRow header = new TableRow();
                TableCell cell = new TableCell();
                cell.Text = "Invalid query. Please choose at least one constraint.";
                cell.Font.Bold = true;
                header.HorizontalAlign = HorizontalAlign.Center;
                header.Cells.Add(cell);
                MultiView1.ActiveViewIndex = 0;
                Table1.Rows.Add(header);
                Table1.Visible = true;
            }
            
            else
            {

                divImg.Visible = true;
                Globals.startTime = DateTime.Now;

                //get enabled searchengines
                Globals.enabledSE.Clear();
                for (int x = 0; x < CheckBoxList1.Items.Count; x++)
                {
                    if (CheckBoxList1.Items[x].Selected)
                        Globals.enabledSE.Add(x + 1);
                }

                //execute query
                if (!Globals.location.Equals("All"))
                    Globals.queryConstraints = Globals.queryConstraints | (int)Constraints.LOCATION;
                if (!cuisine.Equals("All"))
                    Globals.queryConstraints = Globals.queryConstraints | (int)Constraints.CUISINE;
                if (!price.Equals(""))
                    Globals.queryConstraints = Globals.queryConstraints | (int)Constraints.PRICE;

                Globals.cuisine = cuisine;
                Globals.results = executeQuery(Globals.location, cuisine, price, keyword);
                string looooo = Globals.location;

                if (Globals.location.Equals("All"))
                {
                    GLatLng centerNoLoc = new GLatLng(42, -87.5);
                    GMap1.setCenter(centerNoLoc, 9);
                    placeMarkers(10);
                    GMap1.enableHookMouseWheelToZoom = true;
                    GMap2.setCenter(centerNoLoc, 9);
                    placeMarkers(0);
                    GMap2.enableHookMouseWheelToZoom = true;
                    GMap3.setCenter(centerNoLoc, 9);
                    placeMarkers(1);
                    GMap3.enableHookMouseWheelToZoom = true;
                    GMap4.setCenter(centerNoLoc, 9);
                    placeMarkers(2);
                    GMap4.enableHookMouseWheelToZoom = true;
                    GMap5.setCenter(centerNoLoc, 9);
                    placeMarkers(3);
                    GMap5.enableHookMouseWheelToZoom = true;
                    GMap6.setCenter(centerNoLoc, 9);
                    placeMarkers(4);
                    GMap6.enableHookMouseWheelToZoom = true;
                    GMap7.setCenter(centerNoLoc, 9);
                    placeMarkers(5);
                    GMap7.enableHookMouseWheelToZoom = true;
                    GMap8.setCenter(centerNoLoc, 9);
                    placeMarkers(6);
                    GMap8.enableHookMouseWheelToZoom = true;
                    GMap9.setCenter(centerNoLoc, 9);
                    placeMarkers(7);
                    GMap9.enableHookMouseWheelToZoom = true;
                    GMap10.setCenter(centerNoLoc, 9);
                    placeMarkers(8);
                    GMap10.enableHookMouseWheelToZoom = true;
                }
                else if (Globals.getLevel(Globals.location) == 1)
                {
                    List<List<Point>> cornersList = new List<List<Point>>();
                    List<GPolygon> rectangles = new List<GPolygon>();
                    List<Point> corners = new List<Point>();
                    List<Point> cornersChosenLoc = new List<Point>();
                    GPolygon target = new GPolygon();

                    cornersChosenLoc = Globals.RectangleCoords["Metromix" + "/" + Globals.location];
                    cornersList.Add(cornersChosenLoc);
                    rectangles = constructRectangles(cornersList, Rectangle.TARGET);
                    target = rectangles[0];

                    GLatLng center = new GLatLng((cornersChosenLoc[0].y + cornersChosenLoc[1].y) / 2,
                                                 (cornersChosenLoc[0].x + cornersChosenLoc[1].x) / 2);
                    GMap1.setCenter(center, 10);
                    GMap1.Add(target);
                    placeMarkers(10);
                    GMap1.enableHookMouseWheelToZoom = true;
                    GMap2.setCenter(center, 10);
                    GMap2.Add(target);
                    placeMarkers(0);
                    GMap2.enableHookMouseWheelToZoom = true;
                    GMap3.setCenter(center, 10);
                    GMap3.Add(target);
                    placeMarkers(1);
                    GMap3.enableHookMouseWheelToZoom = true;
                    GMap4.setCenter(center, 10);
                    GMap4.Add(target);
                    placeMarkers(2);
                    GMap4.enableHookMouseWheelToZoom = true;
                    GMap5.setCenter(center, 10);
                    GMap5.Add(target);
                    placeMarkers(3);
                    GMap5.enableHookMouseWheelToZoom = true;
                    GMap6.setCenter(center, 10);
                    GMap6.Add(target);
                    placeMarkers(4);
                    GMap6.enableHookMouseWheelToZoom = true;
                    GMap7.setCenter(center, 10);
                    GMap7.Add(target);
                    placeMarkers(5);
                    GMap7.enableHookMouseWheelToZoom = true;
                    GMap8.setCenter(center, 10);
                    GMap8.Add(target);
                    placeMarkers(6);
                    GMap8.enableHookMouseWheelToZoom = true;
                    GMap9.setCenter(center, 10);
                    GMap9.Add(target);
                    placeMarkers(7);
                    GMap9.enableHookMouseWheelToZoom = true;
                    GMap10.setCenter(center, 10);
                    GMap10.Add(target);
                    placeMarkers(8);
                    GMap10.enableHookMouseWheelToZoom = true;
                }
                else
                {
                    constructMap(Globals.location, 0);
                    constructMap(Globals.location, 1);
                    constructMap(Globals.location, 2);
                    constructMap(Globals.location, 3);
                    constructMap(Globals.location, 4);
                    constructMap(Globals.location, 5);
                    constructMap(Globals.location, 6);
                    constructMap(Globals.location, 7);
                    constructMap(Globals.location, 8);
                    constructMap(Globals.location, 10);
                }

                DateTime EndTime = DateTime.Now;
                TimeSpan span = EndTime.Subtract(Globals.startTime);

                Globals.results_header = "Found " + Globals.finalResultsCount + " restaurants for " + cuisine + " in " + " (" + span.Minutes + " minutes " + span.Seconds + " seconds)"+"in the radius of "+dist+"miles";
                Globals.finalResultsCountTotal = Globals.finalResultsCountTotal + Globals.finalResultsCount;

                ///////////////////////////////Testing - Timing//////////////////////////////////////////////
                //if (Globals.getLevel(Globals.location) == 1)
                //    Globals.WriteOutput(Globals.location + "," + Globals.metromixSpan + "," + Globals.dexknowsSpan + "," + Globals.yelpSpan + "," + Globals.chicagoreaderSpan + "," + Globals.menuismSpan + "," + Globals.menupagesSpan + "," + Globals.yahooSpan + "," + Globals.yellowpagesSpan + "," + Globals.citysearchSpan + "," + Globals.zagatSpan + "," + span.Seconds + ",Large Area");
                //else
                //    Globals.WriteOutput(Globals.location + "," + Globals.metromixSpan + "," + Globals.dexknowsSpan + "," + Globals.yelpSpan + "," + Globals.chicagoreaderSpan + "," + Globals.menuismSpan + "," + Globals.menupagesSpan + "," + Globals.yahooSpan + "," + Globals.yellowpagesSpan + "," + Globals.citysearchSpan + "," + Globals.zagatSpan + "," + span.Seconds);
                /////////////////////////////////////////////////////////////////////////////////////////////

                constructTable(Globals.results, 10);
                constructTable(Globals.zagatData, 9);


                if (Globals.getLevel(Globals.location) == 1)
                {
                    Globals.requestsArea++;
                    Globals.totalSecondsArea = Globals.totalSecondsArea + span.Seconds;
                }
                if (Globals.getLevel(Globals.location) == 0)
                {
                    Globals.requestsNeighborhood++;
                    Globals.totalSecondsNeighborhood = Globals.totalSecondsNeighborhood + span.Seconds;
                }

                /////////////////////////////////Testing - Verbose Timings & Google Requests//////////////////////////////////
                //try
                //{
                //    Globals.WriteOutput("Query location was " + Globals.location + " cuisine was " + cuisine + " done in " + span.Seconds);
                //    Globals.WriteOutput("  Metromix " + Globals.metromixLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.metromixLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  DexKnows " + Globals.dexknowsLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.dexknowsLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  Yelp " + Globals.yelpLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.yelpLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  ChicagoReader " + Globals.chicagoreaderLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.chicagoreaderLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  Menuism " + Globals.menuismLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.menuismLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  MenuPages " + Globals.menupagesLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.menupagesLocalDataAllRuns[0].Address);

                //    Globals.WriteOutput("  Yahoo " + Globals.yahooLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.yahooLocalDataAllRuns[0].Address);

                //    //Globals.WriteOutput("  Zagat " + Globals.zagatData[0].City);
                //    //Globals.WriteOutput("  " + Globals.zagatData[0].Address);

                //    try
                //    {
                //        Globals.WriteOutput("  YellowPages " + Globals.yellowpagesLocalDataAllRuns[0].City);
                //        Globals.WriteOutput("  " + Globals.yellowpagesLocalDataAllRuns[0].Address);
                //    }
                //    catch (Exception)
                //    {
                //        //expected exception if yellowpages thread aborted 
                //    }
                //    Globals.WriteOutput("  CitySearch " + Globals.citysearchLocalDataAllRuns[0].City);
                //    Globals.WriteOutput("  " + Globals.citysearchLocalDataAllRuns[0].Address);
                //    Globals.WriteOutput("  google requests " + Globals.numberGoogleRequests);
                //}

                //catch (Exception ex)
                //{
                //    Response.Write("<script>alert('WriteOutput error')</script>");
                //    Globals.WriteOutput("THREAD ERROR");
                //}
                //////////////////////////////////////////////////////////////////////////////////////////////////
                var d2r = Math.PI / 180;   // degrees to radians
                var r2d = 180 / Math.PI;   // radians to degrees
                var earthsradius = 3963; // 3963 is the radius of the earth in miles
                var points = 32;
                //var radius = 10;    
                double rlat = ((double)dist / earthsradius) * r2d;
                double rlng = rlat / Math.Cos(latitude * d2r);
                List<GLatLng> extp = new List<GLatLng>();
                for (var i = 0; i < points + 1; i++)
                {
                    double theta = Math.PI * (i / (double)(points / 2));
                    double ex = longitude + (rlng * Math.Cos(theta));
                    double ey = latitude + (rlat * Math.Sin(theta));
                    extp.Add(new GLatLng(ey, ex));
                }


                GIcon icon1 = new GIcon();
                icon1.image = "/images/urhere2.png";
                //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                icon1.iconSize = new GSize(30, 30);
                icon1.shadowSize = new GSize(22, 20);
                icon1.iconAnchor = new GPoint(6, 20);
                icon1.infoWindowAnchor = new GPoint(5, 1);
                GMarkerOptions mOpts1 = new GMarkerOptions();
                mOpts1.clickable = false;
                mOpts1.icon = icon1;
                GMarker marker1 = new GMarker(latlong, mOpts1);

                
                this.GMap1.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap1.addGMarker(marker1);
                this.GMap2.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap2.addGMarker(marker1);
                this.GMap3.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap3.addGMarker(marker1);
                this.GMap4.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap4.addGMarker(marker1);
                this.GMap5.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap5.addGMarker(marker1);
                this.GMap6.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap6.addGMarker(marker1);
                this.GMap7.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap7.addGMarker(marker1);
                this.GMap8.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap8.addGMarker(marker1);
                this.GMap9.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap9.addGMarker(marker1);
                this.GMap10.addPolygon(new GPolygon(extp, "#00FF00", 0.3));
                GMap10.addGMarker(marker1);

                GMap1.Visible = true;
                GMap2.Visible = true;
                GMap3.Visible = true;
                GMap4.Visible = true;
                GMap5.Visible = true;
                GMap6.Visible = true;
                GMap7.Visible = true;
                GMap8.Visible = true;
                GMap9.Visible = true;
                GMap10.Visible = true;
                Table1.Visible = true;
                Table2.Visible = true;

                //clear form
                DropDownList1.SelectedIndex = 0;
                DropDownList3.SelectedIndex = 0;

            }//till here
            //*/

        }

        private List<Restaurant> executeQuery(String n, String c, String p, String k)
        {
            threads = new List<Thread>();
            int yellowPagesThread = 0;

            //List<List<Restaurant>> yumi8 = new List<List<Restaurant>>();
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8.Add(new List<Restaurant>());
            //}

            for (int i = 0; i < Globals.searchEngines.Count; i++)
            {
                if (Globals.enabledSE.Count > 0 && !Globals.enabledSE.Contains(Globals.searchEngines[i].getCode))
                    continue;

                SearchEngineThread seThread = new SearchEngineThread(n, c, p, i, k);
                Thread InstanceCaller = new Thread(new ThreadStart(seThread.process));
                InstanceCaller.Start();
                //new QueryTimer(i);
                if (Globals.searchEngines[i].getCode == 8)
                {
                    yellowPagesThread = threads.Count;
                    threads.Add(InstanceCaller);
                }
                else
                    threads.Add(InstanceCaller);
                
            }
            SearchEngineThread gsThread = new SearchEngineThread(n, c, p, 9, k);
            Thread gsInstanceCaller = new Thread(new ThreadStart(gsThread.process));
            gsInstanceCaller.Start();
            threads.Add(gsInstanceCaller);


            bool doneFlag = false;
            int threadId = 0;
            int clockTicks = 0;
            while (!doneFlag)
            {
                doneFlag = true;
                //Globals.WriteOutput("THREADPASS");
                for (int i = 0; i < threads.Count; i++)
                {
                    //Globals.WriteOutput("   " + threads[i].ThreadState.ToString());
                    if (threads[i].ThreadState == System.Threading.ThreadState.Running || threads[i].ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    {
                        doneFlag = false;
                        //if (i == yellowPagesThread && clockTicks > 20 && Globals.getLevel(n) == 0)
                        //{
                        //    threads[i].Abort();
                        //    Globals.WriteOutput("yellowpages timeout");
                        //    Globals.webErrors = Globals.webErrors + "yellowpages timeout\n";
                        //}
                        //else if (i == yellowPagesThread && clockTicks > 80 && Globals.getLevel(n) == 1)
                        //{
                        //    threads[i].Abort();
                        //    Globals.WriteOutput("yellowpages timeout");
                        //    Globals.webErrors = Globals.webErrors + "yellowpages timeout\n";
                        //}
                    }
                }
                Thread.Sleep(200);
                clockTicks++;
            }

            int count = threadId;

            Globals.numberGoogleRequests = 0;
            for (int i = 0; i < Globals.allData.Count; i++)
            {
                GeocodeRetreiver.GeocodeRestaurants(Globals.allData[i]);
            }
            ///////////////////////////////////ADDING GEOCODES TO DICTIONARY///////////////////////////////////////
            //GeocodeRetreiver.GeocodeRestaurants(Globals.zagatData);
            //XmlSerializer geoserializer = new XmlSerializer(typeof(SerializableDictionary<string, Point>));
            //TextWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("XML/StoredGeocodes.xml"));
            //geoserializer.Serialize(writer, Globals.Geocodes);
            //writer.Dispose();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Label1.Text+="Before"+Globals.allData.Count.ToString();

            if (!n.Equals("All"))
            {
                int level = Globals.getLevel(n);
                List<Point> cornersChosenLoc = Globals.RectangleCoords["Metromix" + "/" + n];
               // Response.Write("Latitude=" + latitude + "Longitude=" + longitude + "radius=" + dist);
                for (int i = 0; i < Globals.allData.Count; i++)
                {
                    List<Restaurant> tempList = new List<Restaurant>();
                    for (int j = 0; j < Globals.allData[i].Count; j++)
                    {
                        GLatLng latlng = new GLatLng(Globals.allData[i][j].Latitude, Globals.allData[i][j].Longitude);
                        if (!Globals.selected)
                        {
                            double theta = Globals.allData[i][j].Longitude - longitude;
                            double dist1 = Math.Sin(Globals.allData[i][j].Latitude * Math.PI / 180.0) * Math.Sin(latitude * Math.PI / 180.0) + Math.Cos(Globals.allData[i][j].Latitude * Math.PI / 180.0) * Math.Cos(latitude * Math.PI / 180.0) * Math.Cos(theta * Math.PI / 180.0);
                            dist1 = Math.Acos(dist1);
                            dist1 = dist1 / Math.PI * 180.0;
                            dist1 = dist1 * 60 * 1.1515;
                            
                            if (dist1 <= dist)
                            {
                                tempList.Add(Globals.allData[i][j]);
                            }

                        }
                    }
                    Globals.allData[i] = tempList;
                }

            }

                //Label1.Text += "After"+Globals.allData.Count.ToString();

            MergeResults.orderListByRanking(Globals.zagatData);
            for (int i = Globals.zagatTotalQueries; i < Globals.zagatData.Count; i++)
            {
                Globals.zagatData[i].Ranking = i - Globals.zagatTotalQueries + 1;
            }

            ////////////////////////////////////////////Testing - Partial Yumi vs Individual Engines NDGC/////////////////////////////////////////////////
            //List<Restaurant> emptyList = new List<Restaurant>();
            ////Globals.WriteOutput("yumiFullList:");
            ////MergeResults.listCounts(Globals.allData);
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("METROMIX");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "Metromix");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Metromix)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("METROMIX");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "Metromix");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("DEXKNOWS");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "DexKnows");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.DexKnows)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("DEXKNOWS");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "DexKnows");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("YELP");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "Yelp");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Yelp)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("YELP");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "Yelp");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("CHICAGOREADER");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "ChicagoReader");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.ChicagoReader)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("CHICAGOREADER");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "ChicagoReader");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("MENUISM");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "Menuism");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Menuism)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("MENUISM");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "Menuism");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("MENUPAGES");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "MenuPages");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.MenuPages)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("MENUPAGES");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "MenuPages");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("YAHOO");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "Yahoo");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Yahoo)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("YAHOO");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "Yahoo");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("YELLOWPAGES");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "YellowPages");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.YellowPages)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("YELLOWPAGES");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "YellowPages");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        //Globals.WriteOutput("CITYSEARCH");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + emptyList.Count);
            //        MergeResults.computeNDCGNine(yumi8, emptyList, "CitySearch");
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.CitySearch)
            //    {
            //        yumi8[m].Clear();
            //        //Globals.WriteOutput("CITYSEARCH");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        MergeResults.computeNDCGNine(yumi8, Globals.allData[m], "CitySearch");
            //        break;
            //    }
            //}
            //for (int j = 0; j < 9; j++)
            //{
            //    yumi8[j].Clear();
            //}
            //for (int h = 0; h < 9; h++)
            //{
            //    Globals.copyList(yumi8[h], Globals.allData[h]);
            //}
            ////////////////////////////////////////////Testing//////////////////////////////////////////////////// 

            ////////////////////////////////////////////Testing - Zagat vs Individual Engines NDGC/////////////////////////////////////////////////
            //List<Restaurant> emptyList = new List<Restaurant>();
            //double[] nDCG_SE = new double[2];
            //List<Restaurant> arbitratorList = new List<Restaurant>();
            //Globals.copyList(arbitratorList, Globals.zagatData);
            //for (int i = 0; i < arbitratorList.Count; i++)
            //    arbitratorList.RemoveAll(r => r.ZipCode.Equals("99999"));

            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("Metromix," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Metromix)
            //    {
            //        //Globals.WriteOutput("METROMIX");
            //        //Globals.WriteOutput("yumiEightList:");
            //        //MergeResults.listCounts(yumi8);
            //        //Globals.WriteOutput("ninthList: " + Globals.allData[m].Count);
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("Metromix," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("DexKnows," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.DexKnows)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("DexKnows," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("Yelp," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Yelp)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("Yelp," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("ChicagoReader," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.ChicagoReader)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("ChicagoReader," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("Menuism," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Menuism)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("Menuism," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("MenuPages," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.MenuPages)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("MenuPages," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("Yahoo," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.Yahoo)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("Yahoo," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("YellowPages," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.YellowPages)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("YellowPages," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}
            //for (int m = 0; m < 10; m++)
            //{
            //    if (m == 9)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(emptyList, arbitratorList);
            //        Globals.WriteOutput("CitySearch," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //    if (Globals.allData[m].Count != 0 && Globals.allData[m][0].SearchEngine == (int)Engine.CitySearch)
            //    {
            //        nDCG_SE = MergeResults.computeNDCG(Globals.allData[m], arbitratorList);
            //        Globals.WriteOutput("CitySearch," + Globals.location + "," + Globals.cuisine + "," + nDCG_SE[0] + "," + nDCG_SE[1]);
            //        break;
            //    }
            //}


            ////////////////////////////////////////////Testing - Zagat vs Individual Engines NDGC/////////////////////////////////////////////////




            MergeResults.findMatchesMergeRatingCriteria(Globals.allData);
            for (int i = 0; i < Globals.searchEngines.Count; i++)
            {
                if (Globals.enabledSE.Count > 0 && !Globals.enabledSE.Contains(Globals.searchEngines[i].getCode))
                    continue;
                else
                {
                    Globals.allData[i] = MergeResults.sortByConstraints(Globals.allData[i]);
                    Globals.copyList(Globals.allDataEngineTables[i], Globals.allData[i]);
                }
            }


            for (int i = 0; i < Globals.allData.Count; i++)
                Globals.allData[i].RemoveAll(r => r.ZipCode.Equals("99999"));


            List<Restaurant> finalResults = MergeResults.merge(Globals.allData);

            ////////////////////////////////////////////////Testing - NDGC////////////////////////////////////////////
            //double[] nDCGs = null;
            //nDCGs = MergeResults.computeNDCG(Globals.mergeList, arbitratorList);
            //Globals.WriteOutput("Yumi," + Globals.location + "," + Globals.cuisine + "," + nDCGs[0] + "," + nDCGs[1]);
            //Globals.WriteOutput(arbitratorList.Count().ToString());
            //////////////////////////////////////////////Testing/////////////////////////////////////////////

            //////////////////////////////////////////////Testing - HOW MANY RESTAURANTS ARE OUT OF BOUNDS///////////
            //int outOfBoundsRestaurant = 0;
            //if (!n.Equals("All"))
            //{
            //    List<Point> cornersChosenLoc = Globals.RectangleCoords["Metromix" + "/" + n];
            //    for (int j = 0; j < finalResults.Count && j < 10; j++)
            //    {
            //        GLatLng latlng = new GLatLng(finalResults[j].Latitude, finalResults[j].Longitude);
            //        if (!Contains(cornersChosenLoc, latlng))
            //        {
            //            outOfBoundsRestaurant++;
            //        }
            //    }
            //}
            //Globals.WriteOutput(Globals.location + "," + outOfBoundsRestaurant);
            //////////////////////////////////////////////////////////////////////////////////////////////////////////


            return finalResults;
        }

        
        private Boolean haversine2(double lat1, double lon1, double lat2, double lon2, double rad)
        {
            double theta = lon1 - lon2;
            double dist1 = Math.Sin(lat1 * Math.PI / 180.0) * Math.Sin(lat2 * Math.PI / 180.0) + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Cos(theta * Math.PI / 180.0);
            dist1 = Math.Acos(dist1);
            dist1 = dist1 / Math.PI * 180.0;
            dist1 = dist1 * 60 * 1.1515;
            if (dist1 <= rad)
            {
                return true;
            }
            else { return false; }

        }

        private void constructTable(List<Restaurant> restaurants, int code)
        {
            TableRow header_runs = new TableRow();
            TableCell runs = new TableCell();
            runs.Text = "The results collected for each query in this engine:";
            runs.Font.Bold = true;
            runs.ColumnSpan = 4;
            header_runs.HorizontalAlign = HorizontalAlign.Left;
            header_runs.Cells.Add(runs);

            TableRow header_local_merged = new TableRow();
            TableCell local = new TableCell();
            local.Text = "The combined results for this engine's queries:";
            local.Font.Bold = true;
            local.ColumnSpan = 4;
            header_local_merged.HorizontalAlign = HorizontalAlign.Left;
            header_local_merged.Cells.Add(local);

            TableRow zagat_header = new TableRow();
            TableCell zagat = new TableCell();
            zagat.Text = "Zagat Results:";
            zagat.Font.Bold = true;
            zagat.ColumnSpan = 4;
            zagat_header.HorizontalAlign = HorizontalAlign.Left;
            zagat_header.Cells.Add(zagat);

            TableRow blankRow = new TableRow();
            TableCell blank = new TableCell();
            blank.Text = "&nbsp;";
            blank.ColumnSpan = 4;
            blankRow.Cells.Add(blank);

            TableRow header = new TableRow();
            TableCell cell = new TableCell();
            cell.Text = Globals.results_header;
            cell.Font.Bold = true;
            cell.ColumnSpan = 4;
            header.HorizontalAlign = HorizontalAlign.Left;
            header.Cells.Add(cell);

            TableRow headerSep = new TableRow();
            TableCell bottomLine2 = new TableCell();
            bottomLine2.ColumnSpan = 4;
            bottomLine2.BorderWidth = 3;
            headerSep.BackColor = Globals.frameColor;
            headerSep.Cells.Add(bottomLine2);

            if (code == 10)
            {
                Table1.Rows.Add(header);
                Table1.Rows.Add(blankRow);
                Table1.Rows.Add(headerSep);
            }

            if (code == 9)
            {
                Table2.Rows.Add(zagat_header);
                Table2.Rows.Add(blankRow);
                Table2.Rows.Add(headerSep);
            }

            if (restaurants.Count != 0)
            {
                if (code == 0 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table4.Rows.Add(header_runs);
                    Table4.Rows.Add(headerSep);
                }
                else if (code == 0)
                {
                    Table3.Rows.Add(header_local_merged);
                    Table3.Rows.Add(headerSep);
                }
                if (code == 1 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table6.Rows.Add(header_runs);
                    Table6.Rows.Add(headerSep);
                }
                else if (code == 1)
                {
                    Table5.Rows.Add(header_local_merged);
                    Table5.Rows.Add(headerSep);
                }
                if (code == 2 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table8.Rows.Add(header_runs);
                    Table8.Rows.Add(headerSep);
                }
                else if (code == 2)
                {
                    Table7.Rows.Add(header_local_merged);
                    Table7.Rows.Add(headerSep);
                }
                if (code == 3 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table10.Rows.Add(header_runs);
                    Table10.Rows.Add(headerSep);
                }
                else if (code == 3)
                {
                    Table9.Rows.Add(header_local_merged);
                    Table9.Rows.Add(headerSep);
                }
                if (code == 4 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table12.Rows.Add(header_runs);
                    Table12.Rows.Add(headerSep);
                }
                else if (code == 4)
                {
                    Table11.Rows.Add(header_local_merged);
                    Table11.Rows.Add(headerSep);
                }
                if (code == 5 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table14.Rows.Add(header_runs);
                    Table14.Rows.Add(headerSep);
                }
                else if (code == 5)
                {
                    Table13.Rows.Add(header_local_merged);
                    Table13.Rows.Add(headerSep);
                }
                if (code == 6 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table16.Rows.Add(header_runs);
                    Table16.Rows.Add(headerSep);
                }
                else if (code == 6)
                {
                    Table15.Rows.Add(header_local_merged);
                    Table15.Rows.Add(headerSep);
                }
                if (code == 7 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table18.Rows.Add(header_runs);
                    Table18.Rows.Add(headerSep);
                }
                else if (code == 7)
                {
                    Table17.Rows.Add(header_local_merged);
                    Table17.Rows.Add(headerSep);
                }
                if (code == 8 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                {
                    Table20.Rows.Add(header_runs);
                    Table20.Rows.Add(headerSep);
                }
                else if (code == 8)
                {
                    Table19.Rows.Add(header_local_merged);
                    Table19.Rows.Add(headerSep);
                }

                foreach (Restaurant r in restaurants)
                {

                    TableRow sepRow = new TableRow();
                    TableCell sep = new TableCell();
                    sep.Text = "&nbsp;";
                    sep.ColumnSpan = 4;
                    sepRow.Cells.Add(sep);

                    TableRow topSep = new TableRow();
                    TableCell topLine = new TableCell();
                    topLine.ColumnSpan = 4;
                    topLine.Height = 5;
                    topSep.BackColor = Globals.frameColor;
                    topSep.Cells.Add(topLine);

                    TableRow bottomSep = new TableRow();
                    TableCell bottomLine = new TableCell();
                    bottomLine.ColumnSpan = 4;
                    bottomLine.BorderWidth = 2;
                    bottomSep.BackColor = Globals.frameColor;
                    bottomSep.Cells.Add(bottomLine);

                    TableCell leftSep = new TableCell();
                    leftSep.BackColor = Globals.frameColor;
                    leftSep.Width = 3;
                    leftSep.RowSpan = 3;
                    TableCell rightSep = new TableCell();
                    rightSep.BackColor = Globals.frameColor;
                    rightSep.Width = 3;
                    rightSep.RowSpan = 3;

                    TableRow nameRow = new TableRow();
                    TableCell nameCell = new TableCell();
                    nameCell.ColumnSpan = 2;

                    TableRow neighborhoodRow = new TableRow();
                    TableCell neighborhoodCell = new TableCell();
                    neighborhoodCell.ColumnSpan = 2;
                    neighborhoodCell.Text = "&nbsp;" + r.Neighborhood;
                    neighborhoodRow.Cells.Add(neighborhoodCell);

                    TableRow cuisinePriceRow = new TableRow();
                    TableCell cuisinePriceCell = new TableCell();
                    cuisinePriceCell.ColumnSpan = 2;
                    cuisinePriceCell.Text = "&nbsp;" + r.Cuisine;
                    if (r.Price.Length > 0)
                    {
                        if (r.Cuisine.Length > 0)
                            cuisinePriceCell.Text = cuisinePriceCell.Text + "/" + r.Price;
                        else
                            cuisinePriceCell.Text = "&nbsp;" + r.Price;
                    }
                    cuisinePriceRow.Cells.Add(cuisinePriceCell);

                    TableRow timeRow = new TableRow();
                    TableCell timeCell = new TableCell();
                    timeCell.ColumnSpan = 4;
                    timeRow.Cells.Add(timeCell);

                    if (!r.ZipCode.Equals("99999"))
                        nameCell.Text = "&nbsp;" + r.Ranking + ". " + r.Name + (r.IsClosed ? " [CLOSED]" : "");
                    else
                    {
                        if (r.Name.Contains("http"))
                        {
                            HyperLink myLink = new HyperLink();
                            nameCell.Controls.Add(myLink);
                            myLink.NavigateUrl = r.Name;
                            myLink.Text = r.Name;
                            nameCell.ColumnSpan = 4;
                            //r.City in this caser contains the timings of the http requests
                            timeCell.Text = "&nbsp;" + r.City;
                        }
                        else
                            nameCell.Text = "&nbsp;" + r.Name;
                    }
                    nameCell.Font.Bold = true;
                    nameCell.Font.Size = FontUnit.Medium;
                    nameRow.Cells.Add(nameCell);

                    TableRow addressRatingRow = new TableRow();
                    TableCell addressCell = new TableCell();
                    addressCell.Text = "&nbsp;" + r.Address;
                    addressRatingRow.Cells.Add(addressCell);
                    TableCell ratingCell = new TableCell();
                    if (!r.ZipCode.Equals("99999"))
                        ratingCell.Text = "Rating: " + r.Rating + " based on " + r.NumReviews + " reviews";
                    else
                        ratingCell.Text = "";
                    ratingCell.HorizontalAlign = HorizontalAlign.Right;
                    addressRatingRow.Cells.Add(ratingCell);

                    TableRow cityRow = new TableRow();
                    TableCell cityCell = new TableCell();
                    if (!r.ZipCode.Equals("99999"))
                        cityCell.Text = "&nbsp;" + r.City + ", " + r.State + " " + r.ZipCode;
                    else
                        cityCell.Text = "";
                    cityCell.ColumnSpan = 2;
                    cityRow.Cells.Add(cityCell);

                    TableRow phoneRow = new TableRow();
                    TableCell phoneCell = new TableCell();
                    phoneCell.Text = "&nbsp;" + r.PhoneNumber;
                    phoneCell.ColumnSpan = 2;
                    phoneRow.Cells.Add(phoneCell);

                    TableRow engineRow = new TableRow();
                    TableCell engineCell = new TableCell();
                    engineCell.Text = getEngineFlags(r.SearchEngine);
                    engineCell.ColumnSpan = 2;
                    engineRow.Cells.Add(engineCell);

                    if (code == 0 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        //Table4.Rows.Add(topSep);
                        Table4.Rows.Add(nameRow);
                        Table4.Rows.Add(addressRatingRow);
                        Table4.Rows.Add(timeRow);
                        Table4.Rows.Add(engineRow);
                        Table4.Rows.Add(bottomSep);
                        //Table4.Rows.Add(sepRow);
                        Table4.Visible = true;
                    }
                    else if (code == 0)
                    {
                        //Table3.Rows.Add(topSep);
                        Table3.Rows.Add(nameRow);
                        Table3.Rows.Add(neighborhoodRow);
                        Table3.Rows.Add(cuisinePriceRow);
                        Table3.Rows.Add(addressRatingRow);
                        Table3.Rows.Add(cityRow);
                        Table3.Rows.Add(engineRow);
                        Table3.Rows.Add(bottomSep);
                        //Table3.Rows.Add(sepRow);
                        Table3.Visible = true;
                    }


                    if (code == 1 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        //Table6.Rows.Add(topSep);
                        Table6.Rows.Add(nameRow);
                        Table6.Rows.Add(addressRatingRow);
                        Table6.Rows.Add(timeRow);
                        Table6.Rows.Add(engineRow);
                        Table6.Rows.Add(bottomSep);
                        //Table6.Rows.Add(sepRow);
                        Table6.Visible = true;
                    }
                    else if (code == 1)
                    {
                        //Table5.Rows.Add(topSep);
                        Table5.Rows.Add(nameRow);
                        Table5.Rows.Add(addressRatingRow);
                        Table5.Rows.Add(cityRow);
                        Table5.Rows.Add(phoneRow);
                        Table5.Rows.Add(engineRow);
                        Table5.Rows.Add(bottomSep);
                        //Table5.Rows.Add(sepRow);
                        Table5.Visible = true;
                    }

                    if (code == 2 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table8.Rows.Add(nameRow);
                        Table8.Rows.Add(addressRatingRow);
                        Table8.Rows.Add(timeRow);
                        Table8.Rows.Add(engineRow);
                        Table8.Rows.Add(bottomSep);
                        Table8.Visible = true;
                    }
                    else if (code == 2)
                    {
                        Table7.Rows.Add(nameRow);
                        Table7.Rows.Add(neighborhoodRow);
                        Table7.Rows.Add(cuisinePriceRow);
                        Table7.Rows.Add(addressRatingRow);
                        Table7.Rows.Add(cityRow);
                        Table7.Rows.Add(phoneRow);
                        Table7.Rows.Add(engineRow);
                        Table7.Rows.Add(bottomSep);
                        Table7.Visible = true;
                    }

                    if (code == 3 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table10.Rows.Add(nameRow);
                        Table10.Rows.Add(addressRatingRow);
                        Table10.Rows.Add(timeRow);
                        Table10.Rows.Add(engineRow);
                        Table10.Rows.Add(bottomSep);
                        Table10.Visible = true;
                    }
                    else if (code == 3)
                    {
                        Table9.Rows.Add(nameRow);
                        Table9.Rows.Add(neighborhoodRow);
                        Table9.Rows.Add(cuisinePriceRow);
                        Table9.Rows.Add(addressRatingRow);
                        Table9.Rows.Add(cityRow);
                        Table9.Rows.Add(phoneRow);
                        Table9.Rows.Add(engineRow);
                        Table9.Rows.Add(bottomSep);
                        Table9.Visible = true;
                    }
                    if (code == 4 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table12.Rows.Add(nameRow);
                        Table12.Rows.Add(addressRatingRow);
                        Table12.Rows.Add(timeRow);
                        Table12.Rows.Add(engineRow);
                        Table12.Rows.Add(bottomSep);
                        Table12.Visible = true;
                    }
                    else if (code == 4)
                    {
                        Table11.Rows.Add(nameRow);
                        Table11.Rows.Add(neighborhoodRow);
                        Table11.Rows.Add(cuisinePriceRow);
                        Table11.Rows.Add(addressRatingRow);
                        Table11.Rows.Add(cityRow);
                        Table11.Rows.Add(engineRow);
                        Table11.Rows.Add(bottomSep);
                        Table11.Visible = true;
                    }
                    if (code == 5 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table14.Rows.Add(nameRow);
                        Table14.Rows.Add(addressRatingRow);
                        Table14.Rows.Add(timeRow);
                        Table14.Rows.Add(engineRow);
                        Table14.Rows.Add(bottomSep);
                        Table14.Visible = true;
                    }
                    else if (code == 5)
                    {
                        Table13.Rows.Add(nameRow);
                        Table13.Rows.Add(cuisinePriceRow);
                        Table13.Rows.Add(addressRatingRow);
                        Table13.Rows.Add(cityRow);
                        Table13.Rows.Add(engineRow);
                        Table13.Rows.Add(bottomSep);
                        Table13.Visible = true;
                    }
                    if (code == 6 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table16.Rows.Add(nameRow);
                        Table16.Rows.Add(addressRatingRow);
                        Table16.Rows.Add(timeRow);
                        Table16.Rows.Add(engineRow);
                        Table16.Rows.Add(bottomSep);
                        Table16.Visible = true;
                    }
                    else if (code == 6)
                    {
                        Table15.Rows.Add(nameRow);
                        Table15.Rows.Add(addressRatingRow);
                        Table15.Rows.Add(cityRow);
                        Table15.Rows.Add(phoneRow);
                        Table15.Rows.Add(engineRow);
                        Table15.Rows.Add(bottomSep);
                        Table15.Visible = true;
                    }
                    if (code == 7 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table18.Rows.Add(nameRow);
                        Table18.Rows.Add(addressRatingRow);
                        Table18.Rows.Add(timeRow);
                        Table18.Rows.Add(engineRow);
                        Table18.Rows.Add(bottomSep);
                        Table18.Visible = true;
                    }
                    else if (code == 7)
                    {
                        Table17.Rows.Add(nameRow);
                        Table17.Rows.Add(neighborhoodRow);
                        Table17.Rows.Add(cuisinePriceRow);
                        Table17.Rows.Add(addressRatingRow);
                        Table17.Rows.Add(cityRow);
                        Table17.Rows.Add(phoneRow);
                        Table17.Rows.Add(engineRow);
                        Table17.Rows.Add(bottomSep);
                        Table17.Visible = true;
                    }
                    if (code == 8 && restaurants[0].ZipCode.Equals("99999") && restaurants[0].Name.Contains("http"))
                    {
                        Table20.Rows.Add(nameRow);
                        Table20.Rows.Add(addressRatingRow);
                        Table20.Rows.Add(timeRow);
                        Table20.Rows.Add(engineRow);
                        Table20.Rows.Add(bottomSep);
                        Table20.Visible = true;
                    }
                    else if (code == 8)
                    {
                        Table19.Rows.Add(nameRow);
                        Table19.Rows.Add(neighborhoodRow);
                        Table19.Rows.Add(cuisinePriceRow);
                        Table19.Rows.Add(addressRatingRow);
                        Table19.Rows.Add(cityRow);
                        Table19.Rows.Add(engineRow);
                        Table19.Rows.Add(bottomSep);
                        Table19.Visible = true;
                    }
                    if (code == 9)
                    {
                        Table2.Rows.Add(nameRow);
                        Table2.Rows.Add(addressRatingRow);
                        Table2.Rows.Add(cityRow);
                        Table2.Rows.Add(engineRow);
                        Table2.Rows.Add(bottomSep);
                        Table2.Visible = true;
                    }
                    if (code == 10)
                    {
                        //Table1.Rows.Add(topSep);
                        Table1.Rows.Add(nameRow);
                        Table1.Rows.Add(neighborhoodRow);
                        Table1.Rows.Add(cuisinePriceRow);
                        Table1.Rows.Add(addressRatingRow);
                        Table1.Rows.Add(cityRow);
                        Table1.Rows.Add(phoneRow);
                        Table1.Rows.Add(engineRow);
                        Table1.Rows.Add(bottomSep);
                        //Table1.Rows.Add(sepRow);
                        Table1.Visible = true;
                    }


                }

            }
        }


        private void constructMap(string chosenLoc, int code)
        {
            List<List<Point>> cornersList = new List<List<Point>>();
            List<GPolygon> rectangles = new List<GPolygon>();
            List<string> seHoods = new List<string>();
            List<string> allHoods = new List<string>();
            List<Point> corners = new List<Point>();
            List<Point> cornersChosenLoc = new List<Point>();
            GPolygon target = new GPolygon();

            cornersChosenLoc = Globals.RectangleCoords["Metromix" + "/" + chosenLoc];
            cornersList.Add(cornersChosenLoc);
            rectangles = constructRectangles(cornersList, Rectangle.TARGET);
            target = rectangles[0];

            PolygonWrapper N = new PolygonWrapper();
            N.Polygon = target;
            N.HoodName = chosenLoc;
            N.Area = getArea(cornersChosenLoc);

            rectangles.Clear();
            cornersList.Clear();
            GLatLng center = new GLatLng((cornersChosenLoc[0].y + cornersChosenLoc[1].y) / 2,
                                         (cornersChosenLoc[0].x + cornersChosenLoc[1].x) / 2);


            if (code == 0)
            {
                GMap2.enableDragging = true;
                GMap2.enableGoogleBar = true;
                GMap2.enableHookMouseWheelToZoom = true;
                GMap2.Language = "eng";
                GMap2.BackColor = Color.White;
                GMap2.Add(target);
                placeMarkers(code);
                GMap2.setCenter(center, 12);
            }
            if (code == 1)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["DexKnows" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["DexKnows" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap3.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap3.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap3.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap3.enableDragging = true;
                GMap3.enableHookMouseWheelToZoom = true;
                GMap3.Language = "eng";
                GMap3.BackColor = Color.White;
                placeMarkers(code);
                GMap3.setCenter(center, 12);
            }
            if (code == 2)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["Yelp" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["Yelp" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap4.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap4.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap4.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap4.enableDragging = true;
                //GMap4.enableGoogleBar = true;
                GMap4.enableHookMouseWheelToZoom = true;
                GMap4.Language = "eng";
                GMap4.BackColor = Color.White;
                placeMarkers(code);
                GMap4.setCenter(center, 12);
            }
            if (code == 3)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["ChicagoReader" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["ChicagoReader" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap5.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap5.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap5.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap5.enableDragging = true;
                GMap5.enableHookMouseWheelToZoom = true;
                GMap5.Language = "eng";
                GMap5.BackColor = Color.White;
                placeMarkers(code);
                GMap5.setCenter(center, 12);
            }
            if (code == 4)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["Menuism" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["Menuism" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap6.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap6.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap6.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap6.enableDragging = true;
                GMap6.enableHookMouseWheelToZoom = true;
                GMap6.Language = "eng";
                GMap6.BackColor = Color.White;
                placeMarkers(code);
                GMap6.setCenter(center, 12);
            }
            if (code == 5)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["MenuPages" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["MenuPages" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap7.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap7.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap7.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap7.enableDragging = true;
                GMap7.enableHookMouseWheelToZoom = true;
                GMap7.Language = "eng";
                GMap7.BackColor = Color.White;
                placeMarkers(code);
                GMap7.setCenter(center, 12);
            }
            if (code == 6)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["Yahoo" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["Yahoo" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap8.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap8.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap8.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap8.enableDragging = true;
                GMap8.enableHookMouseWheelToZoom = true;
                GMap8.Language = "eng";
                GMap8.BackColor = Color.White;
                placeMarkers(code);
                GMap8.setCenter(center, 12);
            }
            if (code == 7)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["YellowPages" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["YellowPages" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap9.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap9.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap9.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap9.enableDragging = true;
                GMap9.enableHookMouseWheelToZoom = true;
                GMap9.Language = "eng";
                GMap9.BackColor = Color.White;
                placeMarkers(code);
                GMap9.setCenter(center, 12);
            }
            if (code == 8)
            {
                seHoods = Globals.getLocations(chosenLoc, Globals.searchEngines[code].getCode);
                allHoods = Globals.getLocationsAllIntersections(chosenLoc, Globals.searchEngines[code].getCode);
                List<PolygonWrapper> polygons = new List<PolygonWrapper>();
                foreach (string hood in seHoods)
                {
                    PolygonWrapper rectangle = new PolygonWrapper();
                    corners = Globals.RectangleCoords["CitySearch" + "/" + hood];
                    rectangle.Area = getArea(corners);
                    rectangle.HoodName = hood;
                    rectangle.Polygon = constructRectangle(corners, Rectangle.CHOSEN);
                    polygons.Add(rectangle);
                    Globals.cornersAllSelectedLocs.Add(corners);
                }
                foreach (string hood in allHoods)
                {
                    if (!seHoods.Contains(hood))
                    {
                        PolygonWrapper rectangle = new PolygonWrapper();
                        corners = Globals.RectangleCoords["CitySearch" + "/" + hood];
                        rectangle.Area = getArea(corners);
                        rectangle.HoodName = hood;
                        rectangle.Polygon = constructRectangle(corners, Rectangle.NOTCHOSEN);
                        polygons.Add(rectangle);
                    }
                }
                polygons.Add(N);
                MergeResults.orderPolygonsByArea(polygons);
                foreach (PolygonWrapper r in polygons)
                {
                    GMap10.Add(r.Polygon);
                    string name = r.HoodName;
                    name = "\"" + name + "\"";
                    GMap10.addListener(new GListener(r.Polygon.PolygonID, GListener.Event.click, "function(center) {     var myHtml =" + name + ";     subgurim_GMap10.openInfoWindowHtml(center, myHtml);   }"));
                }
                GMap10.enableDragging = true;
                GMap10.enableHookMouseWheelToZoom = true;
                GMap10.Language = "eng";
                GMap10.BackColor = Color.White;
                placeMarkers(code);
                GMap10.setCenter(center, 12);
            }
            if (code == 10)
            {
                GMap1.enableDragging = true;
                GMap1.enableGoogleBar = true;
                GMap1.enableHookMouseWheelToZoom = true;
                GMap1.Language = "eng";
                GMap1.BackColor = Color.White;
                
                GPolygon yumiTarget = new GPolygon();

                corners = Globals.RectangleCoords["Metromix" + "/" + chosenLoc];
                cornersList.Add(corners);
                rectangles = constructRectangles(cornersList, Rectangle.TARGET);
                yumiTarget = rectangles[0];
                rectangles.Clear();
                cornersList.Clear();
                GMap1.Add(yumiTarget);
                Globals.cornersAllSelectedLocs = Globals.cornersAllSelectedLocs.Distinct(new PointListComparer()).ToList();
                rectangles.AddRange(constructRectangles(Globals.cornersAllSelectedLocs, Rectangle.CHOSEN));
                foreach (GPolygon polygon in rectangles)
                {
                    GMap1.Add(polygon);
                }
                placeMarkers(code);
                GMap1.setCenter(center, 12);

            }

        }


        private void placeMarkers(int code)
        {
            if (code == 0)
            {
                for (int i = 0; i < Globals.allDataEngineTables[0].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[0][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placMarkers" + code + ": " + Globals.allDataEngineTables[0][i].Latitude + "," + Globals.allDataEngineTables[0][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[0][i].Latitude, Globals.allDataEngineTables[0][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[0][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        //GMarker marker = new GMarker(latlng);
                        GMap2.addGMarker(marker);
                    }
                }
            }
            if (code == 1)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap3.addGMarker(marker);
                    }
                }
            }
            if (code == 2)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap4.addGMarker(marker);

                    }
                }
            }
            if (code == 3)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap5.addGMarker(marker);
                    }
                }
            }
            if (code == 4)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap6.addGMarker(marker);
                    }
                }
            }
            if (code == 5)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap7.addGMarker(marker);
                    }
                }
            }
            if (code == 6)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap8.addGMarker(marker);
                    }
                }
            }
            if (code == 7)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap9.addGMarker(marker);
                    }
                }
            }
            if (code == 8)
            {
                for (int i = 0; i < Globals.allDataEngineTables[code].Count && i < 20; i++)
                {
                    if (!Globals.allDataEngineTables[code][i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.allDataEngineTables[code][i].Latitude + "," + Globals.allDataEngineTables[code][i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.allDataEngineTables[code][i].Latitude, Globals.allDataEngineTables[code][i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.allDataEngineTables[code][i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap10.addGMarker(marker);
                    }
                }
            }
            if (code == 10)
            {
                for (int i = 0; i < Globals.results.Count && i < 20; i++)
                {
                    if (!Globals.results[i].ZipCode.Equals("99999"))
                    {
                        //Globals.WriteOutput("In placeMarkers" + code + ": " + Globals.results[i].Latitude + "," + Globals.results[i].Longitude);
                        GLatLng latlng = new GLatLng(Globals.results[i].Latitude, Globals.results[i].Longitude);
                        GIcon icon = new GIcon();
                        icon.image = "/images/blue" + Globals.results[i].Ranking + ".png";
                        //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
                        icon.iconSize = new GSize(30, 30);
                        icon.shadowSize = new GSize(22, 20);
                        icon.iconAnchor = new GPoint(6, 20);
                        icon.infoWindowAnchor = new GPoint(5, 1);

                        GMarkerOptions mOpts = new GMarkerOptions();
                        mOpts.clickable = false;
                        mOpts.icon = icon;

                        GMarker marker = new GMarker(latlng, mOpts);
                        GMap1.addGMarker(marker);
                    }
                }
            }

        }

        private List<GPolygon> constructRectangles(List<List<Point>> cornersList, Rectangle type)
        {
            List<GPolygon> polygons = new List<GPolygon>();
            foreach (List<Point> corners in cornersList)
            {
                List<GLatLng> rectanglePts = new List<GLatLng>();
                rectanglePts.Add(new GLatLng(corners[0].y, corners[1].x));
                rectanglePts.Add(new GLatLng(corners[0].y, corners[0].x));
                rectanglePts.Add(new GLatLng(corners[1].y, corners[0].x));
                rectanglePts.Add(new GLatLng(corners[1].y, corners[1].x));
                if (type == Rectangle.TARGET)
                {
                    GPolygon rectangle = new GPolygon(rectanglePts, "ffffff", 0, 0, "000080", 0.3);
                    rectangle.clickable = true;
                    rectangle.close();
                    polygons.Add(rectangle);
                }
                if (type == Rectangle.CHOSEN)
                {
                    GPolygon rectangle = new GPolygon(rectanglePts, "0000ff", 2, 1, "ffffff", 0);
                    rectangle.clickable = true;
                    rectangle.close();
                    polygons.Add(rectangle);
                }
                if (type == Rectangle.NOTCHOSEN)
                {
                    GPolygon rectangle = new GPolygon(rectanglePts, "ff0000", 2, 1, "ffffff", 0);
                    rectangle.clickable = true;
                    rectangle.close();
                    polygons.Add(rectangle);
                }
                if (type == Rectangle.YUMITARGET)
                {
                    GPolygon rectangle = new GPolygon(rectanglePts, "0000ff", 2, 1, "ffffff", 0);
                    rectangle.clickable = true;
                    rectangle.close();
                    polygons.Add(rectangle);
                }

            }
            return polygons;
        }

        private GPolygon constructRectangle(List<Point> corners, Rectangle type)
        {
            GPolygon rectangle = new GPolygon();
            List<GLatLng> rectanglePts = new List<GLatLng>();
            rectanglePts.Add(new GLatLng(corners[0].y, corners[1].x));
            rectanglePts.Add(new GLatLng(corners[0].y, corners[0].x));
            rectanglePts.Add(new GLatLng(corners[1].y, corners[0].x));
            rectanglePts.Add(new GLatLng(corners[1].y, corners[1].x));
            if (type == Rectangle.TARGET)
            {
                rectangle = new GPolygon(rectanglePts, "ffffff", 0, 0, "000080", 0.3);
                rectangle.clickable = true;
                rectangle.close();
            }
            else if (type == Rectangle.CHOSEN)
            {
                rectangle = new GPolygon(rectanglePts, "0000ff", 2, 1, "ffffff", 0);
                rectangle.clickable = true;
                rectangle.close();
            }
            else if (type == Rectangle.NOTCHOSEN)
            {
                rectangle = new GPolygon(rectanglePts, "ff0000", 2, 1, "ffffff", 0);
                rectangle.clickable = true;
                rectangle.close();
            }
            else if (type == Rectangle.YUMITARGET)
            {
                rectangle = new GPolygon(rectanglePts, "0000ff", 2, 1, "ffffff", 0);
                rectangle.clickable = true;
                rectangle.close();
            }


            return rectangle;
        }

        private double getArea(List<Point> corners)
        {
            double width = corners[1].x - corners[0].x;
            double length = corners[0].y - corners[1].y;
            return Math.Abs(length * width);
        }

        private Boolean Contains(List<Point> chosenLoc, GLatLng point)
        {
            if (chosenLoc[0].x >= point.lng && chosenLoc[0].y >= point.lat && chosenLoc[1].x <= point.lng && chosenLoc[1].y <= point.lat)
                return true;
            else
                return false;
        }

        private Boolean containsWithBuffer(List<Point> chosenLoc, GLatLng point)
        {
            double originalWidth = chosenLoc[1].x - chosenLoc[0].x;
            double originalHeight = chosenLoc[0].y - chosenLoc[1].y;
            double bufferWidth = (originalWidth * 0.5) / 2;
            double bufferHeight = (originalHeight * 0.5) / 2;
            if ((chosenLoc[0].x - bufferWidth) >= point.lng && (chosenLoc[0].y + bufferHeight) >= point.lat && (chosenLoc[1].x + bufferWidth) <= point.lng && (chosenLoc[1].y - bufferHeight) <= point.lat)
                return true;
            else
                return false;
        }


        private static void ProcessMergeFile(List<LocationList> hoods, string file)
        {
            int defaultSearchEngine = 0;
            //String mergeFile = "merge/mergedHierarchyAnalysis.csv";
            //StreamReader reader = new StreamReader(mergeFile);
            StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath(file));

            String line = reader.ReadLine();
            char[] seps = { ',' };
            char[] hoodSeps = { ';' };

            while (line != null)
            {
                while (line.Equals(""))
                {
                    line = reader.ReadLine();
                }

                String[] lineParts = line.Split(seps, 4);
                defaultSearchEngine = findSearchEngine(lineParts[1]);
                string defaultSearchEngineHood = lineParts[0];
                if (lineParts[0].IndexOf('(') > 0)
                {
                    defaultSearchEngineHood = lineParts[0].Substring(0, lineParts[0].IndexOf('(')).Trim();
                }

                LocationList newHood = new LocationList(defaultSearchEngineHood);
                Console.WriteLine("Working on " + defaultSearchEngineHood);
                newHood.addLocation(defaultSearchEngine, defaultSearchEngineHood);
                if (lineParts[2].Equals("0"))
                    newHood.Level = 0;
                else
                    newHood.Level = 1;

                if (lineParts.Count() > 3)
                {
                    String[] locationMatches = lineParts[3].Split(hoodSeps);
                    for (int i = 0; i < locationMatches.Length; i++)
                    {
                        if (locationMatches[i].Length > 0)
                        {
                            String name = locationMatches[i].Substring(0, locationMatches[i].IndexOf(':'));
                            if (name.IndexOf('(') > 0 && !name.Equals("Chicago (O'Hare Area)"))
                            {
                                name = name.Substring(0, name.IndexOf('('));
                            }

                            String se = locationMatches[i].Substring(locationMatches[i].IndexOf(':') + 1, locationMatches[i].Length - locationMatches[i].IndexOf(':') - 1);
                            int seNum = findSearchEngine(se);

                            newHood.addLocation(seNum, name);
                        }
                    }
                }
                hoods.Add(newHood);
                line = reader.ReadLine();
            }

            reader.Close();
        }

        private static void ProcessMergeFileCuisine()
        {
            int defaultSearchEngine = 0;
            //String mergeFile = "merge/mergedCuisineAnalysis.csv";
            //StreamReader reader = new StreamReader(mergeFile);
            StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/merge/mergedCuisineAnalysis.csv"));

            String line = reader.ReadLine();
            char[] seps = { ',' };
            char[] hoodSeps = { ';' };

            while (line != null)
            {
                while (line.Equals(""))
                {
                    line = reader.ReadLine();
                }

                String[] lineParts = line.Split(seps, 4);
                defaultSearchEngine = findSearchEngine(lineParts[1]);
                string defaultSearchEngineCuisine = lineParts[0];
                if (lineParts[0].IndexOf('(') > 0)
                {
                    defaultSearchEngineCuisine = lineParts[0].Substring(0, lineParts[0].IndexOf('(')).Trim();
                }

                CuisineList newCuisine = new CuisineList(defaultSearchEngineCuisine);
                Console.WriteLine("Working on " + defaultSearchEngineCuisine);
                newCuisine.addCuisine(defaultSearchEngine, defaultSearchEngineCuisine);
                if (lineParts[2].Equals("0"))
                    newCuisine.Level = 0;
                else
                    newCuisine.Level = 1;

                if (lineParts.Count() > 3)
                {
                    String[] cuisineMatches = lineParts[3].Split(hoodSeps);
                    for (int i = 0; i < cuisineMatches.Length; i++)
                    {
                        if (cuisineMatches[i].Length > 0)
                        {
                            String name = cuisineMatches[i].Substring(0, cuisineMatches[i].IndexOf(':'));
                            if (name.IndexOf('(') > 0 && !name.Equals("American (Regional)"))
                            {
                                name = name.Substring(0, name.IndexOf('('));
                            }

                            String se = cuisineMatches[i].Substring(cuisineMatches[i].IndexOf(':') + 1, cuisineMatches[i].Length - cuisineMatches[i].IndexOf(':') - 1);
                            int seNum = findSearchEngine(se);

                            newCuisine.addCuisine(seNum, name);
                        }
                    }
                }
                Globals.cuisines.Add(newCuisine);
                line = reader.ReadLine();
            }

            reader.Close();
        }

        private static void ProcessBatchFile()
        {
            //String[] inputRecord = new String[4];
            //StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Input.txt"));

            //string line = reader.ReadLine();
            //char[] seps = { ',' };

            //while (line != null)
            //{
            //    while (line.Equals(""))
            //    {
            //        line = reader.ReadLine();
            //    }
            //    string[] lineParts = line.Split(seps, 4);
            //    Globals.inputRecords.Add(lineParts);
            //}
            using (StreamReader sr = new StreamReader(HttpContext.Current.Server.MapPath("~/xml/Input_Filtered_Yumi03.txt")))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] cells = line.Split(new string[] { "," }, StringSplitOptions.None);
                    if (cells.Length > 0)
                    {
                        //System.Diagnostics.Trace.WriteLine(cells[0] + "," + cells[1] + "," + cells[2] + "," + cells[3]);  
                        Globals.inputRecords.Add(cells);
                    }
                }
            }
        }

        private static int findSearchEngine(String se)
        {
            for (int i = 0; i < Globals.searchEngines.Count; i++)
            {
                if (Globals.searchEngines[i].getName.Equals(se))
                {
                    return Globals.searchEngines[i].getCode;
                }
            }
            if (se.Equals("Zagat"))
                return 10;

            return -1;
        }

        private static string getEngineFlags(int flags)
        {
            string engineNames = "";
            Engine e;
            if (flags == 0)
                return engineNames;
            for (int i = 0; i < 9; i++)
            {
                if ((flags & (int)Math.Pow(2, i)) == (int)Math.Pow(2, i))
                {
                    e = (Engine)Math.Pow(2, i);
                    engineNames = engineNames + e + "/";
                }
            }
            engineNames = engineNames.Substring(0, engineNames.Length - 1);
            return engineNames;
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            if (hideFlag)
            {
                CheckBoxList1.Style["display"] = "none";
                hideFlag = false;
            }
            else
            {
                CheckBoxList1.Style["display"] = "";
                hideFlag = true;
            }
        }

        //timer class to kill threads after a certain time.
        //public class QueryTimer
        //{
        //    private int idx;
        //    private System.Timers.Timer abortTimer;

        //    public QueryTimer(int x)
        //    {
        //        idx = x;
        //        abortTimer = new System.Timers.Timer(Globals.TimeOutValue);
        //        abortTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        //        abortTimer.Enabled = true;
        //    }

        //    private void OnTimedEvent(object source, ElapsedEventArgs e)
        //    {
        //        //abort thread                
        //        if (idx < threads.Count && threads[idx].ThreadState != System.Threading.ThreadState.Stopped)
        //        {
        //            threads[idx].Abort();
        //        }

        //        abortTimer.Enabled = false;
        //    }
        //}


        protected void DropDownList2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        {

        }

        protected void Menu1_MenuItemClick1(object sender, MenuEventArgs e)
        {
            MultiView1.ActiveViewIndex = Int32.Parse(e.Item.Value);
            for (int i = 0; i < this.Menu1.Items.Count; i++)
            {
                if (i == Int32.Parse(e.Item.Value))
                {
                    //this.Menu1.Items[i].ImageUrl = "/images/selectedtab.gif";
                    if (i == 0)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/yumiselectedtab.gif";
                        constructTable(Globals.results, 10);
                        constructTable(Globals.zagatData, 9);
                    }
                    if (i == 1)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/metromixselectedtab.gif";
                        constructTable(Globals.metromixLocalDataAllRuns, 0);
                        constructTable(Globals.allDataEngineTables[0], 0);
                        //constructMap(Globals.location, 0);
                    }
                    if (i == 2)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/dexknowsselectedtab.gif";
                        constructTable(Globals.dexknowsLocalDataAllRuns, 1);
                        constructTable(Globals.allDataEngineTables[1], 1);
                        //constructMap(Globals.location, 1);
                    }
                    if (i == 3)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/yelpselectedtab.gif";
                        constructTable(Globals.yelpLocalDataAllRuns, 2);
                        constructTable(Globals.allDataEngineTables[2], 2);
                    }
                    if (i == 4)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/chicagoreaderselectedtab.gif";
                        constructTable(Globals.chicagoreaderLocalDataAllRuns, 3);
                        constructTable(Globals.allDataEngineTables[3], 3);
                    }
                    if (i == 5)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/menuismselectedtab.gif";
                        constructTable(Globals.menuismLocalDataAllRuns, 4);
                        constructTable(Globals.allDataEngineTables[4], 4);
                    }
                    if (i == 6)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/menupagesselectedtab.gif";
                        constructTable(Globals.menupagesLocalDataAllRuns, 5);
                        constructTable(Globals.allDataEngineTables[5], 5);
                    }
                    if (i == 7)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/yahooselectedtab.gif";
                        constructTable(Globals.yahooLocalDataAllRuns, 6);
                        constructTable(Globals.allDataEngineTables[6], 6);
                    }
                    if (i == 8)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/yellowpagesselectedtab.gif";
                        constructTable(Globals.yellowpagesLocalDataAllRuns, 7);
                        constructTable(Globals.allDataEngineTables[7], 7);
                    }
                    if (i == 9)
                    {
                        this.Menu1.Items[i].ImageUrl = "/images/citysearchselectedtab.gif";
                        constructTable(Globals.citysearchLocalDataAllRuns, 8);
                        constructTable(Globals.allDataEngineTables[8], 8);
                    }

                }
                else
                {
                    if (i == 0)
                        this.Menu1.Items[i].ImageUrl = "/images/yumiunselectedtab.gif";
                    if (i == 1)
                        this.Menu1.Items[i].ImageUrl = "/images/metromixunselectedtab.gif";
                    if (i == 2)
                        this.Menu1.Items[i].ImageUrl = "/images/dexknowsunselectedtab.gif";
                    if (i == 3)
                        this.Menu1.Items[i].ImageUrl = "/images/yelpunselectedtab.gif";
                    if (i == 4)
                        this.Menu1.Items[i].ImageUrl = "/images/chicagoreaderunselectedtab.gif";
                    if (i == 5)
                        this.Menu1.Items[i].ImageUrl = "/images/menuismunselectedtab.gif";
                    if (i == 6)
                        this.Menu1.Items[i].ImageUrl = "/images/menupagesunselectedtab.gif";
                    if (i == 7)
                        this.Menu1.Items[i].ImageUrl = "/images/yahoounselectedtab.gif";
                    if (i == 8)
                        this.Menu1.Items[i].ImageUrl = "/images/yellowpagesunselectedtab.gif";
                    if (i == 9)
                        this.Menu1.Items[i].ImageUrl = "/images/citysearchunselectedtab.gif";
                }
            }

        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        protected string GMap2_Click(object s, GAjaxServerEventArgs e)
        {
            return default(string);
        }

        protected void Locate_Click(object sender, EventArgs e)
        {
            //Label1.Visible = true;
            Globals.selected = false;
            DropDownList1.SelectedIndex = -1;
            if (TextBox3.Text == "")
            {
                
                Label1.Text="Please enter the Radius";
                TextBox3.Focus();
            }
            else
            {
                string coordinates = TextBox2.Text;
                TextBox2.Text = "";
                int x = coordinates.Length;
                dist = Convert.ToDouble(TextBox3.Text);
                TextBox3.Text = "";
                //string x = coordinates.Substring(2, coordinates.Length - 1);
                //Label1.Text = x;
                string actual = coordinates.Substring(1, x - 2);
                int pos = actual.IndexOf(',');
                string lat = actual.Substring(0, actual.IndexOf(','));
                latitude = Convert.ToDouble(lat);
                string lng = actual.Substring(actual.IndexOf(',') + 2);
                longitude = Convert.ToDouble(lng);
                GMap11.addControl(new GControl(GControl.preBuilt.GOverviewMapControl));
                GMap11.addControl(new GControl(GControl.preBuilt.LargeMapControl));
                latlong = new GLatLng(latitude, longitude);
                marker = new GMarker(new GLatLng(latitude, longitude));
                window = new GInfoWindow(marker, "<center><b>" + actual + "</b></center>", true);
                GMap11.addInfoWindow(window);
                GMap11.setCenter(latlong, 12);
                drawCircle2(latitude, longitude, dist);
            }
        }
        private void drawCircle2(double lat, double lng, double radius)
        {
            // globals
            //drawing the circle with the latitude and longitude
            var d2r = Math.PI / 180;   // degrees to radians
            var r2d = 180 / Math.PI;   // radians to degrees
            var earthsradius = 3963; // 3963 is the radius of the earth in miles
            var points = 32;
            //var radius = 10;    
            double rlat = ((double)radius / earthsradius) * r2d;
            double rlng = rlat / Math.Cos(lat * d2r);
            List<GLatLng> extp = new List<GLatLng>();
            for (var i = 0; i < points + 1; i++)
            {
                double theta = Math.PI * (i / (double)(points / 2));
                double ex = lng + (rlng * Math.Cos(theta));
                double ey = lat + (rlat * Math.Sin(theta));
                extp.Add(new GLatLng(ey, ex));
            }


            GIcon icon1 = new GIcon();
            icon1.image = "/images/urhere2.png";
            //icon.shadow = "http://labs.google.com/ridefinder/images/mm_20_shadow.png";
            icon1.iconSize = new GSize(30, 30);
            icon1.shadowSize = new GSize(22, 20);
            icon1.iconAnchor = new GPoint(6, 20);
            icon1.infoWindowAnchor = new GPoint(5, 1);
            GMarkerOptions mOpts1 = new GMarkerOptions();
            mOpts1.clickable = false;
            mOpts1.icon = icon1;
            GMarker marker1 = new GMarker(latlong, mOpts1);
            
            this.GMap11.addPolygon(new GPolygon(extp, "##FF0000", 0.3));
            GMap11.addGMarker(marker1);
        


            


            //end of code for drawing circle with given lat long and radius
            //code for XML Retrieval

            
            using (reader1 = XmlReader.Create("C:/Users/Dheeraj Rampally/Desktop/dheeraj/SEM 2 ebooks/Query Processing/Project/yumi_web/yumi/xml/XMLFile1.xml"))
            {
                while (reader1.Read() && i1 <= l && j < l && k < l)
                {
                    if (reader1.IsStartElement())
                    {
                        switch (reader1.Name)
                        {
                            case "dictionary": break;
                            case "item": break;
                            case "key": break;
                            case "string": if (reader1.Read())
                                {
                                    if (reader1.Value.Trim().Contains("Metromix"))
                                    {
                                        nn[k] = reader1.Value.Trim();
                                        //Label1.Text = Label1.Text + reader.Value.Trim() + " "+"<br/>";
                                        k++;
                                    }
                                }
                                break;
                            case "value": break;
                            case "ArrayOfPoint": break;
                            case "Point": break;
                            case "x": if (reader1.Read())
                                {
                                    xcoordinates[i1] = reader1.Value.Trim();
                                    //Label2.Text += "xcoordinates[" + i1 + "]=" + reader.Value.Trim() + " " +"<br/>";
                                    i1++;
                                }
                                break;
                            case "y": if (reader1.Read())
                                {
                                    ycoordinates[j] = reader1.Value.Trim();
                                    //Label3.Text += "ycoordinates[" + j + "]=" + reader.Value.Trim() + " " + "<br/>";
                                    j++;
                                }
                                break;
                        }
                    }
                }    
            }//end of code for the retrieval of XML coordinates
            
            
            //code snippet to draw a rectangle
            for (int z = 0; z <= 422; z=z+2)//looping through all the neighborhoods <=452
            {
                for (int j2 = 0; j2 < 4; j2++)//looping through all the edges within a neighborhood
                {
                    d2r = Math.PI / 180D;
                    if (j2 == 0)
                    {
                        //Haversine's Formula
                        dlong = (lng-double.Parse(xcoordinates[z])) * d2r;
                        dlat = (lat-double.Parse(ycoordinates[z])) * d2r;
                        a = Math.Pow(Math.Sin(dlat / 2.0), 2) + Math.Cos(lat * d2r) * Math.Cos(double.Parse(ycoordinates[z]) * d2r) * Math.Pow(Math.Sin(dlong / 2.0), 2);
                        c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        d = 3956 * c;//3956 is radius of earth in miles 
                    }
                    else if (j2 == 1)
                    {
                        //Haversine's Formula
                        dlong = (lng-double.Parse(xcoordinates[z + 1])) * d2r;
                        dlat = (lat-double.Parse(ycoordinates[z])) * d2r;
                        a = Math.Pow(Math.Sin(dlat / 2.0), 2) + Math.Cos(lat * d2r) * Math.Cos(double.Parse(ycoordinates[z]) * d2r) * Math.Pow(Math.Sin(dlong / 2.0), 2);
                        c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        d = 3956 * c; 
                    }
                    else if (j2 == 2)
                    {
                        //Haversine's Formula
                        dlong = (lng-double.Parse(xcoordinates[z])) * d2r;
                        dlat = (lat-double.Parse(ycoordinates[z+1])) * d2r;
                        a = Math.Pow(Math.Sin(dlat / 2.0), 2) + Math.Cos(lat * d2r) * Math.Cos(double.Parse(ycoordinates[z]) * d2r) * Math.Pow(Math.Sin(dlong / 2.0), 2);
                        c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        d = 3956 * c; 
                    }
                    else if (j2 == 3)
                    {
                        //Haversine's Formula
                        dlong = (lng-double.Parse(xcoordinates[z+1])) * d2r;
                        dlat = (lat-double.Parse(ycoordinates[z + 1])) * d2r;
                        a = Math.Pow(Math.Sin(dlat / 2.0), 2) + Math.Cos(lat * d2r) * Math.Cos(double.Parse(ycoordinates[z]) * d2r) * Math.Pow(Math.Sin(dlong / 2.0), 2);
                        c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        d = 3956 * c; 
                    }
                    distance[j2] = d;
                    //Label1.Text += distance[j2].ToString()+"<br/>";
                }
                maxElement = distance[0];//code for retriving the min element in an array
                for (int i3 = 0; i3 < 4; i3++)
                {
                    if (distance[i3] <maxElement)
                    {
                        maxElement = distance[i3];
                        //Label1.Text += "The minimum element is in loop" + x1 + "<br/>";
                    }  
                }
                //x1 y2 x2 y1
                
                x1 = double.Parse(ycoordinates[z]);
                //Label1.Text += x1+" ";
                x3=lat;
                //Label1.Text += x2+" ";
                x2 = double.Parse(ycoordinates[z + 1]);
                //Label1.Text += x3 + "<br/>";
                if(x2<x3&&x3<x1)
                {
                    c1=true;
                  //  Label1.Text+="c1";
                }
                y1=(double.Parse(xcoordinates[z]));
                //Label1.Text += c2l + " ";
                y3=lng;
                //Label1.Text += c2m + " ";
                y2=(double.Parse(xcoordinates[z+1]));
                //Label1.Text += c2r +"<br/>";
                if((y1>y3)&&(y3>y2))
                {
                    c2=true;
                //    Label1.Text += "c2";
                }

                
                //vertex falling under circle
                
                if (radius >= maxElement)
                {
                        //code snippet to draw a rectangle
                    if (z == 0)
                    {
                        //Label1.Text += nn[0];
                        neighborhood[0] = nn[0];
                    }
                    else
                    {
                        //Label1.Text += nn[z / 2];
                        neighborhood[z / 2] = nn[z / 2];
                    }
                        GPolygon rectangle = new GPolygon();
                        List<GLatLng> rectanglePts = new List<GLatLng>();
                        rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z + 1])));
                        rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z])));
                        rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z])));
                        rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z + 1])));
                        rectangle = new GPolygon(rectanglePts, "0000CD", 2, 2, "FCD116", 0.3);
                        rectangle.clickable = true;
                        rectangle.close();
                        GMap11.Add(rectangle);
                        continue;
                }
                
                
                //circle lying inside neighborhood
                    
                else if ((x2 < x3 && x3 < x1) && (y1 > y3 && y3 > y2))
                {
                    if (z == 0)
                    {
                        //Label1.Text += nn[0];
                        neighborhood[0] = nn[0];
                    }
                    else
                    {
                        //Label1.Text += nn[z / 2];
                        neighborhood[z / 2] = nn[z / 2];
                    }
                    GPolygon rectangle = new GPolygon();
                    List<GLatLng> rectanglePts = new List<GLatLng>();
                    
                    rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z + 1])));
                    rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z])));
                    rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z])));
                    rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z + 1])));
                    rectangle = new GPolygon(rectanglePts, "0000CD", 2, 2, "FCD116", 0.3);
                    rectangle.clickable = true;
                    rectangle.close();
                    GMap11.Add(rectangle);
                    continue;
                }
                      
                // the circle and line intersection with vertex lying outside circle
                    for (int j3 = 0; j3 < 4; j3++)
                    {
                        if (j3 == 0)
                        {
                            x4 = double.Parse(ycoordinates[z]);
                            y4 = double.Parse(xcoordinates[z]);
                            x5 = double.Parse(ycoordinates[z + 1]);
                            y5 = double.Parse(xcoordinates[z]);
                        }
                        if (j3 == 1)
                        {
                            x4 = double.Parse(ycoordinates[z + 1]);
                            y4 = double.Parse(xcoordinates[z]);
                            x5 = double.Parse(ycoordinates[z + 1]);
                            y5 = double.Parse(xcoordinates[z + 1]);
                        }
                        if (j3 == 2)
                        {
                            x4 = double.Parse(ycoordinates[z + 1]);
                            y4 = double.Parse(xcoordinates[z + 1]);
                            x5 = double.Parse(ycoordinates[z]);
                            y5 = double.Parse(xcoordinates[z + 1]);
                        }
                        if (j3 == 3)
                        {
                            x4 = double.Parse(ycoordinates[z]);
                            y4 = double.Parse(xcoordinates[z + 1]);
                            x5 = double.Parse(ycoordinates[z]);
                            y5 = double.Parse(xcoordinates[z]);
                        }
                        dx = x5 - x4;
                        dy = y5 - y4;
                        A = dx * dx + dy * dy;
                        B = 2 * (dx * (x4 - lat) + dy * (y4 - lng));
                        C = (x4 - lat) * (x4 - lat) + (y4 - lng) * (y4 - lng) - Math.Pow(0.014513788,2)*radius * radius;
                        det = B * B - 4 * A * C;
                        
                        double t, ix1, iy1,ix2,iy2;
                        if ((A <= 0.0000001) || (det < 0))
                        {
                            
                        }
                        else if(det==0)
                        {
                            t = -B / (2 * A);
                            ix1 = x4 + t * dx;
                            iy1 = y4 + t * dy;
                            if ((x4 >= ix1 && ix1 >= x5) && (y4 <= iy1 && iy1 <= y5) || (x4 <= ix1 && ix1 <= x5) && (y4 >= iy1 && iy1 >= y5))
                            {
                                if (z == 0)
                                {
                                    //Label1.Text += nn[0];
                                    neighborhood[0] = nn[0];
                                }
                                else
                                {
                                    //Label1.Text += nn[z / 2];
                                    neighborhood[z / 2] = nn[z / 2];
                                }
                                GPolygon rectangle = new GPolygon();
                                List<GLatLng> rectanglePts = new List<GLatLng>();
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z + 1])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z + 1])));
                                rectangle = new GPolygon(rectanglePts, "0000CD", 2, 2, "FCD116", 0.3);
                                rectangle.clickable = true;
                                rectangle.close();
                                GMap11.Add(rectangle);
                            }
                        }
                        else
                        {
                            t = (-B + Math.Sqrt(det)) / (2 * A);
                            ix1 = x4 + t * dx;
                            iy1 = y4 + t * dy;
                            t = (-B - Math.Sqrt(det)) / (2 * A);
                            ix2 = x4 + t * dx;
                            iy2 = y4 + t * dy;
                            if((x4>=ix1&&ix1>=x5)&&(y4<=iy1&&iy1<=y5)&&(x4>=ix2&&ix2>=x5)&&(y4<=iy2&&iy2<=y5)||
                                (x4 <= ix1 && ix1 <= x5) && (y4 >= iy1 && iy1 >= y5) && (x4 <= ix2 && ix2 <= x5) && (y4 >= iy2 && iy2 >= y5))
                            {
                                if (z == 0)
                                {
                                    //Label1.Text += nn[0];
                                    neighborhood[0] = nn[0];
                                }
                                else
                                {
                                    //Label1.Text += nn[z / 2];
                                    neighborhood[z / 2] = nn[z / 2];
                                }
                                GPolygon rectangle = new GPolygon();
                                List<GLatLng> rectanglePts = new List<GLatLng>();
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z + 1])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z]), double.Parse(xcoordinates[z])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z])));
                                rectanglePts.Add(new GLatLng(double.Parse(ycoordinates[z + 1]), double.Parse(xcoordinates[z + 1])));
                                rectangle = new GPolygon(rectanglePts, "0000CD", 2, 2, "FCD116", 0.3);
                                rectangle.clickable = true;
                                rectangle.close();
                                GMap11.Add(rectangle);
                            }
                        }
                    }
                    
            }

            for (int i = 0, x = 0; i < neighborhood.Length; i++)
            {
                if (neighborhood[i] == null)
                    continue;
                else
                {
                    shortListednn[x] = neighborhood[i];
                    shortListednn[x] = shortListednn[x].Substring(9);
                    //Label1.Text += "ShortlistedNN " + x + "=" + shortListednn[x];
                    x++;
                }
            }
            
            for (int i = 0; i < Globals.hoods.Count(); i++)
            {
                for (int j = 0; j < shortListednn.Length; j++)
                {
                    if (Globals.hoods[i].getName() == shortListednn[j])
                    {
                        LocationList newHood = new LocationList(shortListednn[j]);
                        newHood = Globals.hoods[i];
                        Globals.shortListedNeighborHoods.Add(newHood);
                    }
                }

            }
            /*
            for (int i = 1; i < Globals.baba.Length; i++)
            {
                Globals.baba[i] = null;
            }

            for (int i = 0; i < Globals.shortListedNeighborHoods.Count(); i++)
            {
                for (int j = 0; j < Globals.shortListedNeighborHoods[i].getLocationsCount(); j++)
                {
                    if (Globals.baba[Globals.shortListedNeighborHoods[i].getSearchEngineCodeForMapping(j)] == null)
                    {
                        Globals.baba[Globals.shortListedNeighborHoods[i].getSearchEngineCodeForMapping(j)] = Globals.shortListedNeighborHoods[i].getNeighborhood(j);
                        continue;
                    }
                    Globals.baba[Globals.shortListedNeighborHoods[i].getSearchEngineCodeForMapping(j)]=Globals.baba[Globals.shortListedNeighborHoods[i].getSearchEngineCodeForMapping(j)]+","+Globals.shortListedNeighborHoods[i].getNeighborhood(j);
                }
            }

            */
            
            for (int i = 0; i < Globals.shortListedNeighborHoods.Count(); i++)
            {
                for (int j = 0; j < Globals.shortListedNeighborHoods[i].getLocationsCount(); j++)
                {
                    int k = Globals.shortListedNeighborHoods[i].getSearchEngineCodeForMapping(j);
                    s1 += Globals.shortListedNeighborHoods[i].getNeighborhood(j) + k.ToString() + ",";
                    //Label1.Text+= Globals.shortListedNeighborHoods[i].getNeighborhood(j) + k.ToString() +"<br/>";

                }

                //Label1.Text += "<------------Next Set of Neighborhoods for new neighborhood--------->"+"<br/>";
            }
            //Label1.Text = s1;
              
            char[] seps = { ',' };
            childList = s1.Split(seps);

            //for (int i = 0; i < childList.Length - 1; i++)
            //{
            //    Label1.Text += childList[i] + "<br/>";
            //}
            
            
            

            for (int i = 0; i < childList.Length - 1; i++)
            {
                //Label1.Text += childList[i]+"<br/>";
                length = childList[i].Length;
                if ((childList[i].Substring(length - 1)) == "1")
                {
                    Globals.metromixList[m] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "Metromix" +Globals.metromixList[m] + " " + "<br/>";
                    m++;
                }
                else if ((childList[i].Substring(length - 1)) == "2")
                {
                    Globals.dexknowsList[d1] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "dexknows"+Globals.dexknowsList[d1] + " " + "<br/>";
                    d1++;
                }
                else if ((childList[i].Substring(length - 1)) == "3")
                {
                    Globals.yelpList[y] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "yelp"+Globals.yelpList[y] + " " + "<br/>";
                    y++;
                }
                else if ((childList[i].Substring(length - 1)) == "4")
                {
                    Globals.chicagoList[ch] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "chicagoreader"+Globals.chicagoList[ch] + " " + "<br/>";
                    ch++;
                }
                else if ((childList[i].Substring(length - 1)) == "5")
                {
                    Globals.menuList[me] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "menu"+Globals.menuList[me] + " " + "<br/>";
                    me++;
                }
                else if ((childList[i].Substring(length - 1)) == "6")
                {
                    Globals.menuPagesList[mp] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "menupages"+Globals.menuPagesList[mp] + " " + "<br/>";
                    mp++;
                }
                else if ((childList[i].Substring(length - 1)) == "7")
                {
                    Globals.yahooList[ya] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "yahoo"+Globals.yahooList[ya] + " " + "<br/>";
                    ya++;
                }
                else if ((childList[i].Substring(length - 1)) == "8")
                {
                    Globals.yellowList[ye] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "yellow"+Globals.yellowList[ye] + " " + "<br/>";
                    ye++;
                }
                else if ((childList[i].Substring(length - 1)) == "9")
                {
                    Globals.cityList[ci] = childList[i].Substring(0, length - 1);
                    //Label1.Text += "city"+Globals.cityList[ci] + " " + "<br/>";
                    ci++;
                }
                else if ((childList[i].Substring(length - 2)) == "10")
                {
                    Globals.zagatList[za] = childList[i].Substring(0, length - 2);
                    //Label1.Text += "zagat"+Globals.zagatList[za] + " " + "<br/>";
                    za++;
                }
            }

            Array.Resize(ref Globals.metromixList, m);
            Array.Resize(ref Globals.zagatList, za);
            Array.Resize(ref Globals.yellowList,ye);
            Array.Resize(ref Globals.yahooList, ya);
            Array.Resize(ref Globals.menuPagesList, mp);
            Array.Resize(ref Globals.menuList, me);
            Array.Resize(ref Globals.chicagoList, ch);
            Array.Resize(ref Globals.yelpList, y);
            Array.Resize(ref Globals.dexknowsList, d1);
            Array.Resize(ref Globals.cityList, ci);
            
            //for (int i = 0; i < Globals.dexknowsList.Length; i++)
            //{
            //    Label1.Text += Globals.dexknowsList[i];
            //}
             
        }


        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GMap11.reset();
            GMap11.resetPolygon();
            Label1.Text = "";
            Globals.shortListedNeighborHoods.Clear();
            Globals.selected = true;
            Array.Resize(ref Globals.metromixList,1000);
            Array.Resize(ref Globals.zagatList, 1000);
            Array.Resize(ref Globals.yellowList,1000);
            Array.Resize(ref Globals.yahooList, 1000);
            Array.Resize(ref Globals.menuPagesList, 1000);
            Array.Resize(ref Globals.menuList, 1000);
            Array.Resize(ref Globals.chicagoList, 1000);
            Array.Resize(ref Globals.yelpList, 1000);
            Array.Resize(ref Globals.dexknowsList, 1000);
            Array.Resize(ref Globals.cityList, 1000);
           
        }
    }
}