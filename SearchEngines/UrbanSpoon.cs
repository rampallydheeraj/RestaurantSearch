using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace yumi
{
    class UrbanSpoon : ISearchEngine
    {
        private int code;
        private String Name;
        private string xmlFileName = "xml/UrbanSpoon_crosswalk.xml";
        private Dictionary<string, string> crosswalk;
        private int constraints;

        public UrbanSpoon(int c, String n)
        {
            code = c;
            Name = n;
            crosswalk = new Dictionary<string, string>();
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

        public string BaseRestaurantUrl
        {
            get { return "http://www.urbanspoon.com/s/2?q="; }
        }

        public String ComplexRestaurantUrl(String k)
        {
            return BaseRestaurantUrl + k + "&sortby=adistance";
        }

        public void GetCrosswalkFromXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Crosswalk>));
            TextReader reader = new StreamReader(xmlFileName);
            List<Crosswalk> nameKeys = serializer.Deserialize(reader) as List<Crosswalk>;

            foreach (Crosswalk c in nameKeys) { crosswalk.Add(c.Name.Substring(0, c.Name.IndexOf("(")).Trim() , c.Key); }
        }
        public void GetLocationCrosswalkFromXml() { }
        public void GetCuisineCrosswalkFromXml() { }

        public List<Restaurant> processRequest(string location, string keyword, string price, string keyword2)
        {

            //string hood = crosswalk[location.Trim()];
            string queryStr = BaseRestaurantUrl + keyword + "+" + location;            
            string response = WebDocumentRetrieverProxy.GetDocument(queryStr);
            Globals.urlQueries.Add(queryStr);
            //string response = WebDocumentRetriever.GetDocument(queryStr, null);

            List<Restaurant> restaurants = new List<Restaurant>();

            string[] tmp = response.Split(new string[] { "/r/2" }, StringSplitOptions.RemoveEmptyEntries);
            for (int tmp_count = 1; tmp_count < tmp.Length; tmp_count++)
            {
                Restaurant restaurant = new Restaurant();
                string tmp_name = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf(">") + 1, tmp[tmp_count].IndexOf("<") - tmp[tmp_count].IndexOf(">") - 1);
                tmp_name = tmp_name.Replace("<b>", "");
                tmp_name = tmp_name.Replace("</b>", "");

                string tmp_address_info = WebDocumentRetrieverProxy.GetDocument("http://www.urbanspoon.com/r/2" + tmp[tmp_count].Substring(0, tmp[tmp_count].IndexOf("restaurant") - 1));
                //string tmp_address_info = WebDocumentRetriever.GetDocument("http://www.urbanspoon.com/r/2" + tmp[tmp_count].Substring(0, tmp[tmp_count].IndexOf("restaurant") - 1), null);

                string tmp_address = tmp_address_info.Substring(tmp_address_info.IndexOf("street-address") + 16);
                string tmp_city = tmp_address_info.Substring(tmp_address_info.IndexOf("locality") + 10);
                string tmp_state = tmp_address_info.Substring(tmp_address_info.IndexOf("region") + 8);
                string tmp_phone = tmp_address_info.Substring(tmp_address_info.IndexOf("phone tel") + 11);
                string tmp_rating = tmp_address_info.Substring(tmp_address_info.IndexOf("percent-text rating average") + 29);                             

                string final_name = tmp_name; //tmp_name.Substring(tmp_name.IndexOf(">") + 1, tmp_name.IndexOf("<") - tmp_name.IndexOf(">") - 1);
                string final_address = tmp_address.Substring(0, tmp_address.IndexOf("<")); //tmp_address.Substring(0, tmp_address.IndexOf(">") - 1);
                string final_city = tmp_city.Substring(0, tmp_city.IndexOf("<"));
                string final_state = tmp_state.Substring(0, tmp_state.IndexOf("<"));
                string phone_number = tmp_phone.Substring(0, tmp_phone.IndexOf("<"));

                restaurant.Name = final_name;
                restaurant.Address = final_address;
                restaurant.City = final_city;
                restaurant.State = "IL";
                restaurant.PhoneNumber = phone_number;

                //string test1 = tmp[tmp_count].Substring(tmp[tmp_count].IndexOf("Average Rating") + 16);
                //string test2 = test1.Substring(0, test1.IndexOf("out") - 1);
                if (tmp_address_info.IndexOf("percent-text rating average") > 0)
                    tmp_rating = tmp_rating.Substring(0, tmp_rating.IndexOf("<") - 1);
                else 
                    tmp_rating = "0.0";

                double rating = Convert.ToDouble(tmp_rating) / 20;

                restaurant.Rating = rating;
                restaurant.Ranking = tmp_count;

                restaurants.Add(restaurant);
            }

            //order list of restaurants
            MergeResults.orderList(restaurants);

            //shrink results to match threshold
            do
                restaurants.RemoveAt(Globals.Threshold);
            while (restaurants.Count > Globals.Threshold);

            //insert rankings
            for (int i = 0; i < restaurants.Count; i++)
                restaurants.ElementAt(i).Ranking = i;

            return restaurants;
        }        
    }
}
