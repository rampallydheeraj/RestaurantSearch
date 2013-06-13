using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Web;
using System.Diagnostics;

namespace yumi
{
    

    public static class GeocodeRetreiver
    {
       

        public static void GeocodeRestaurants(List<Restaurant> originalList)
        {
            //SerializableDictionary<string, Point> Geocodes = new SerializableDictionary<string, Point>();
            //XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, Point>));
            Point location;
            //try
            //{
            //TextReader reader = new StreamReader("XML/StoredGeocodes.xml");
            //TextReader reader = new StreamReader(HttpContext.Current.Server.MapPath("XML/StoredGeocodes.xml"));
            //Globals.Geocodes = serializer.Deserialize(reader) as SerializableDictionary<string, Point>;
            //reader.Dispose();
            //}
            //catch
            //{
            //    Globals.WriteOutput("File does not exist");
            //}
            string baseurl =
                "http://maps.google.com/maps/geo?output=csv&oe=utf8&sensor=false&key=ABQIAAAAtzZZ2kxXyhTjdmwS8oL2vBQZa90HcAdVgfne3APeOMDDNBbLsxTY7XG15fkmNMMJX0FrbEJg1MVFzA";
        
            //foreach (Restaurant r in originalList)
            //for (int i = 0; i < originalList.Count && i < 20; i++)
            for (int i = 0; i < originalList.Count; i++)
            {
                if (originalList[i].FullAddress != null)
                {
                    //Globals.WriteOutput("doing " + originalList[i].Name);                    
                    
                    //clean up address string
                    string[] addressPieces = originalList[i].FullAddress.Split(new char[] { ',' });
                    string address = originalList[i].FullAddress;

                    if (addressPieces.Length > 2 && addressPieces[2].Contains("Chicago"))
                        address = addressPieces[0] + "," + addressPieces[2];

                    address = address.Replace("Mkt.", "Market");

                    if (Globals.Geocodes.TryGetValue(address, out location))
                    {
                        //Trace.WriteLine("Found stored geocode for: " + address);
                        //Trace.WriteLine(location.x + "'" + location.y);
                        originalList[i].Latitude = Convert.ToDouble(location.y);
                        originalList[i].Longitude = Convert.ToDouble(location.x);
                    }
                    else
                    {
                        Globals.numberGoogleRequests++;
                        //send request to google API
                        //string url = baseurl + "&q=" + ((address.Contains("Chicago")) ? address : address + ", Chicago, IL");
                        string url = baseurl + "&q=" + address;
                        //Trace.WriteLine(url);
                        //Globals.WriteOutput(getEngineFlags(originalList[i].SearchEngine));
                        string response = WebDocumentRetriever.GetDocument(url);
                        //Trace.WriteLine(response);
                        //format is <response_code>,<accuracy>,<lat>,<long>
                        try
                        {
                            string[] data = response.Split(new char[] { ',' });
                            if (data.Length >= 4 && Convert.ToInt32(data[1]) >= 6)
                            {
                                originalList[i].Latitude = Convert.ToDouble(data[2]);
                                originalList[i].Longitude = Convert.ToDouble(data[3]);
                            }

                            //if (originalList[i].Latitude == 0.00 || originalList[i].Longitude == 0.00)
                            //{
                            //    int val;
                            //}
                            //else
                            if (originalList[i].Latitude != 0.00 && originalList[i].Longitude != 0.00)
                            {
                                Point p = new Point(Double.Parse(data[3]), Double.Parse(data[2]));
                                Globals.Geocodes.Add(address, p);
                            }

                            Thread.Sleep(100);
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                }
                //TextWriter writer = new StreamWriter("XML/StoredGeocodes.xml");
                //StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("XML/StoredGeocodes.xml"));
                //serializer.Serialize(writer, Geocodes);
                //writer.Dispose();
             
            }

            
            //remove any restaurant that has no geolocation
            //for (int i = 0; i < originalList.Count; i++)
            //{
            //    if (originalList[i].Address.Equals(", Chicago, IL") && (originalList[i].Longitude == 0.00 || originalList[i].Latitude == 0.00))
            //    {
            //        originalList.RemoveAt(i);
            //    }
            //}
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

    }
}
