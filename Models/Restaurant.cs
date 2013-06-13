using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace yumi
{
    public class Restaurant : IComparable<Restaurant>

    {
        private String name;
        private String address;
        private String fullAddress;
        private String neighborhood;
        private String cuisine;
        private String price;
        private String city;
        private String state;
        private String zipcode;
        private String phoneNumber;

        private String m_nameNormalized = null;
        private String m_addressNormalized = null;
        private String m_phoneNumberNormalized = null;

        private int searchEngine;
        private int ranking;
        private double rating;
        private double numReviews;
        private int priceLvl;
        private int relevanceScore;
        public int[] rankingMatrix;
        public int[][] ratingMatrix;
        private int criteria;
        private double latitude;
        private double longitude;
        private Boolean isClosed;
        public List<Restaurant> matchingRestaurants;

        private const string CT_BETWEEN = "between";
        private const string CT_NEAR = "near";
        private const string CT_AT = " at ";
        private const string CT_THIRD = " third ";
        private const string CT_THE = "the";
        private const string CT_KFC = "kentucky fried chicken";
        private const string CT_IHOP = "international house of pancakes";

        private string[] stopWords = { " by ", " the ", " a ", " on ", " st ", "street", "cafe", "shop", "shoppe", "restaurant", "restaurante", "restaurants", "restrnt", "sandwich", "catering", "cuisine", "cuisiane", "concina", "kitchen", "fine", "house", "grill", "grille", "pizza", "pizzeria", "pizzerie", "deli", "hotel", "inn", "banquet", "dining room", "boutique", "trattoria", "taqueria", "bakey", "and", "&", "bbq", "b b q", "bar b que", "barbque", "bar-b-que", "bar-b-q", "pub", "bar", "lounge", "liquor", "company", "inc", "italian", "chinese", "japanese", "indian", "mexican", "mexicana", "chicago", "-", "'", ".", " " };
        private string[] addressWords = { " st ", "street", "avenue", " ave ", "place", " pl ", "road", " rd ", " blvd ", "boulevard", "drive", " dr ", 
                                                " ct ", "court", " east ", " west ", ".", ","};
        private string[] phoneSymboles = { "(", ")", " ", "-", "/" };

        private const double CT_NAME_DISTANCE_THRESHOLD = 0.87;
        private const double CT_LOW_NAME_DISTANCE_THRESHOLD = 0.80;
        private const double CT_LOWER_NAME_DISTANCE_THRESHOLD = 0.65;
        private const double CT_LOWEST_NAME_DISTANCE_THRESHOLD = 0.4;
        private const double CT_ADDRESS_DISTANCE_THRESHOLD = 0.70;
        private const double CT_LOW_ADDRESS_DISTANCE_THRESHOLD = 0.60;


        public Restaurant()
        {
            rankingMatrix = new int[9];
            for (int i = 0; i < 9; i++)
                rankingMatrix[i] = 99;
            ratingMatrix = new int[9][];
            for (int i = 0; i < 9; i++)
            {
                ratingMatrix[i] = new int[2];
                ratingMatrix[i][0] = 0;
                ratingMatrix[i][1] = 0;
            }
            //name = "";
            //address = "";
            city = "";
            state = "";
            zipcode = "";
            phoneNumber = "";
            neighborhood = "";
            cuisine = "";
            price = "";
            searchEngine = 0;
            isClosed = false;
            matchingRestaurants = new List<Restaurant>();
        }

        public Restaurant(string n, string a, int r)
        {
            name = n;
            address = a;
            ranking = r;
            city = "";
            state = "";
            zipcode = "";
            phoneNumber = "";
            searchEngine = 0;
            isClosed = false;
            rankingMatrix = new int[9];
            for (int i = 0; i < 9; i++)
                rankingMatrix[i] = 99;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string FullAddress
        {
            get { return fullAddress; }
            set { fullAddress = value; }
        }

        public string Neighborhood
        {
            get { return neighborhood; }
            set { neighborhood = value; }
        }

        public string Cuisine
        {
            get { return cuisine; }
            set { cuisine = value; }
        }

        public string Price
        {
            get { return price; }
            set { price = value; }
        }

        public string City
        {
            get { return city; }
            set { city = value; }
        }

        public string State
        {
            get { return state; }
            set { state = value; }
        }

        public string ZipCode
        {
            get { return zipcode; }
            set { zipcode = value; }
        }

        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = value; }
        }

        public int SearchEngine
        {
            get { return searchEngine; }
            set { searchEngine = value; }
        }

        public double Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        public double NumReviews
        {
            get { return numReviews; }
            set { numReviews = value; }
        }

        public int Ranking
        {
            get { return ranking; }
            set { ranking = value; }
        }

        public int PriceLvl
        {
            get { return priceLvl; }
            set { priceLvl = value; }
        }

        public int Criteria
        {
            get { return criteria; }
            set { criteria = value; }
        }

        public double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public Boolean IsClosed
        {
            get { return isClosed; }
            set { isClosed = value; }
        }

        public String NameNormalized
        {
            get {
                if (m_nameNormalized != null)
                    return m_nameNormalized;

                m_nameNormalized = removeStopWords(name, stopWords).Trim();
                if (m_nameNormalized != null)
                    m_nameNormalized.Trim();

                return m_nameNormalized; 
            }
        }

        public String PhoneNumberNormalized
        {
            get { 
                if(m_phoneNumberNormalized != null)
                    return m_phoneNumberNormalized;

                m_phoneNumberNormalized = removeStopWords(phoneNumber, phoneSymboles);
                if (m_phoneNumberNormalized != null)
                    m_phoneNumberNormalized.Trim();

                return m_phoneNumberNormalized; 
            }            
        }

        public String AddressNormalized
        {
            get { 
                if(m_addressNormalized != null)
                    return m_addressNormalized;

                m_addressNormalized = cleanAddress(address, addressWords);
                if (m_addressNormalized != null)
                    m_addressNormalized.Trim();

                return m_addressNormalized;
            }
        }

        public int RelevanceScore
        {
            get { return relevanceScore; }
            set { relevanceScore = value; }
        }


        public bool isMatch(Restaurant restaurant)
        {
            return isMatch(this, restaurant);
        }

        public static bool isMatch(Restaurant sourceRestaurant, Restaurant destRestaurant)
        {
            string sourceName = sourceRestaurant.NameNormalized;            
            string destinationName = destRestaurant.NameNormalized;
            string sourceAddr = sourceRestaurant.AddressNormalized;
            string destinationAddr = destRestaurant.AddressNormalized;            
            string sourcePhone = sourceRestaurant.PhoneNumberNormalized;
            string destPhone = destRestaurant.PhoneNumberNormalized;
            //Trace.WriteLine(sourceName + " " + destinationName);
            //Trace.WriteLine(sourceAddr + " " + destinationAddr);

            if (sourceAddr != null && destinationAddr != null)
            {
                //both addresses are not null we can use them to compare restaurants
                if (Globals.EditDistance(sourceAddr, destinationAddr) >= CT_ADDRESS_DISTANCE_THRESHOLD ||
                    sourceAddr.StartsWith(destinationAddr) || destinationAddr.StartsWith(sourceAddr))
                {
                    //Trace.WriteLine("Address Match");
                    //address are similar, let's lower the threshold and compare names
                    //here we are assuming that two restaurants can have different names but the same address
                    if (Globals.EditDistance(sourceName, destinationName) >= CT_LOWER_NAME_DISTANCE_THRESHOLD ||
                        sourceName.StartsWith(destinationName) || destinationName.StartsWith(sourceName))
                    {
                        //Trace.WriteLine("Name Match: Return True");
                        return true;
                    }
                }


                if (Globals.EditDistance(sourceAddr, destinationAddr) >= CT_LOW_ADDRESS_DISTANCE_THRESHOLD)
                /*||
                    sourceAddr.StartsWith(destinationAddr) || destinationAddr.StartsWith(sourceAddr))
                 * */
                {
                    //Trace.WriteLine("Address Similarity");
                    //address have similarity >= A2
                    if (sourcePhone != null && destPhone != null && sourcePhone.Equals(destPhone))
                    {
                        //both phone numbers are the same so these two must be the same restaurant
                        if (Globals.EditDistance(sourceName, destinationName) >= CT_LOWEST_NAME_DISTANCE_THRESHOLD ||
                            sourceName.StartsWith(destinationName) || destinationName.StartsWith(sourceName))
                            return true;                     
                    }
                }


                if (Globals.EditDistance(sourceName, destinationName) >= CT_NAME_DISTANCE_THRESHOLD ||
                        sourceName.StartsWith(destinationName) || destinationName.StartsWith(sourceName))
                {
                    /*
                    //names have similarity >= N2
                    if (sourceAddr.Equals(destinationAddr))
                    {
                        //addresses are identical
                        return true;
                    }
                     * */
                    if (sourcePhone != null && destPhone != null && sourcePhone.Equals(destPhone))
                        return true; //both phone numbers are the same so these two must be the same restaurant
                }
            }
            else
            {
                //one of the address is null or missing, so we need to compare the names                    
                if (Globals.EditDistance(sourceName, destinationName) >= CT_LOW_NAME_DISTANCE_THRESHOLD ||
                    sourceName.StartsWith(destinationName) || destinationName.StartsWith(sourceName))
                {
                    if (sourcePhone != null && destPhone != null && sourcePhone.Equals(destPhone))
                    {
                        //both phone numbers are the same so these two must be the same restaurant
                        return true;                     
                    }
                }

                if (Globals.EditDistance(sourceName, destinationName) >= CT_LOW_NAME_DISTANCE_THRESHOLD ||
                    sourceName.StartsWith(destinationName) || destinationName.StartsWith(sourceName))
                {
                    //addresses and phones numbers are not available for both restaurants
                    return true;
                }
            }

            return false;
        }
       

        private static string removeStopWords(string str, string[] list)
        {
            if (str == null || str.Trim().Length == 0)
                return null;

            string strTemp = str;
            //make str all lower case
            str = str.ToLower();
            //remove paranthesis
            str = Regex.Replace(str, @"\(.*\)", "").Trim();
            //words starts with The or ends with The
            if (str.StartsWith(CT_THE + " "))
                str = str.Substring(4);
            if (str.EndsWith(" " + CT_THE))
                str = str.Remove(str.Length - 4);

            str = replaceAcronymInName(str);

            foreach (string SW in list)
            {
                if (str.Contains(SW))
                    if (str.IndexOf(" on ") > 0)
                        str = str.Substring(0, str.IndexOf(" on "));
                    else
                        str = str.Replace(SW, "");
            }

            str = str.Replace("#039;", "");
            str = str.Replace("  ", " ");
            //if the sting becomes empty then (semi)backtrack
            //it needs to be implemented
            return str;
        }

        private static string cleanAddress(string str, string[] list)
        {
            if (str == null || str.Trim().Length == 0)
                return null;

            //make str all lower case
            str = str.ToLower();


            if (str.Contains(CT_BETWEEN))
            {
                int index = str.IndexOf(CT_BETWEEN);
                str = str.Substring(0, index - 1);
                str.TrimEnd();
            }

            if (str.Contains(CT_NEAR))
            {
                int index = str.IndexOf(CT_NEAR);
                str = str.Substring(0, index - 1);
                str.TrimEnd();
            }


            if (str.Contains(CT_AT))
            {
                int index = str.IndexOf(CT_AT);
                str = str.Substring(0, index - 1);
                str.TrimEnd();
            }

            if (str.Contains(CT_THIRD))
            {
                str = str.Replace(CT_THIRD, "3rd");
                //str = str.Substring(0, index - 1);
                str.Trim();
            }


            foreach (string SW in list)
            {
                if (str.Contains(SW))
                    str = str.Replace(SW, "");
            }

            return str.Replace("  ", " ");
        }

        private static string replaceAcronymInName(string str)
        {
            //str = str.Replace("bbq", CT_BBQ);
            str = str.Replace("kfc", CT_KFC);
            str = str.Replace("ihop", CT_IHOP);
            return str;

        }

        public int CompareTo(Restaurant r)
        {
            return r.RelevanceScore.CompareTo(this.relevanceScore);
        }


    }
}
