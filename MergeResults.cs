using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace yumi
{
    public static class MergeResults
    {
        //public static TextWriter w;
        
        public static List<Restaurant> orderList(List<Restaurant> results)
        {
            Boolean doSwap = false;
            do
            {
                doSwap = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (i < results.Count - 1 && results[i].Rating < results[i + 1].Rating)
                    {
                        //swap
                        Restaurant tmpRest = results[i];
                        results[i] = results[i + 1];
                        results[i + 1] = tmpRest;
                        doSwap = true;
                    }

                    if (i < results.Count - 1 && results[i].Rating == results[i + 1].Rating)
                    {
                        if (results[i].NumReviews < results[i + 1].NumReviews)
                        {
                            //swap
                            Restaurant tmpRest = results[i];
                            results[i] = results[i + 1];
                            results[i + 1] = tmpRest;
                            doSwap = true;
                        }
                    }
                }//end for loop
            }
            while (doSwap);

            return results;
        }


        public static void orderListByRanking(List<Restaurant> results)
        {
            Boolean doSwap = false;
            do
            {
                doSwap = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (i < results.Count - 1 && results[i].Ranking > results[i + 1].Ranking)
                    {
                        //swap
                        Restaurant tmpRest = results[i];
                        results[i] = results[i + 1];
                        results[i + 1] = tmpRest;
                        doSwap = true;
                    }
                }//end for loop
            }
            while (doSwap);

            //return results;
        }

        public static void orderListByRankingDesc(List<Restaurant> results)
        {
            Boolean doSwap = false;
            do
            {
                doSwap = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (i < results.Count - 1 && results[i].Ranking < results[i + 1].Ranking)
                    {
                        //swap
                        Restaurant tmpRest = results[i];
                        results[i] = results[i + 1];
                        results[i + 1] = tmpRest;
                        doSwap = true;
                    }
                }//end for loop
            }
            while (doSwap);

            //return results;
        }

        public static void orderPolygonsByArea(List<PolygonWrapper> results)
        {
            Boolean doSwap = false;
            do
            {
                doSwap = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (i < results.Count - 1 && results[i].Area < results[i + 1].Area)
                    {
                        //swap
                        PolygonWrapper tmpRest = results[i];
                        results[i] = results[i + 1];
                        results[i + 1] = tmpRest;
                        doSwap = true;
                    }
                }//end for loop
            }
            while (doSwap);            
        }
        
        public static List<Restaurant> merge(List<List<Restaurant>> data)
        {
            int highest_rank = 0;

            //ASSIGN RANKING ACCORDING TO THE BALANCED BORDA
            /* One way to merge lists by Borda when the lists are of uneven lengths is as follows. 
             * First, all lists are unioned.  Let its length be m. 
             * Then a list  having k objects is assigned weight as followed: 
             * First object gets weight m, the second object gets weight (m-1) etc.  
             * Let P1 = m + (m-1) + … + 1. Then, the list so far gets  sum of weights 
             * P2=  m + (m-1) + … (m-k +1). Each of the other objects which does not appear 
             * in the list is assigned weight = (P1 – P2)/(m-k). In other words,  
             * the objects which do not belong to the list are still assigned weights  
             * so that each list gets the same sum of weights =P1, but the weights of 
             * its objects are distributed differently.
             */

            highest_rank = Globals.mergeList.Count(); //i.e., P2
            int nTotalRank = highest_rank * (highest_rank + 1) / 2; //i.e., P1
            int[] tempRestaurantsPankings = new int[highest_rank];
            for (int x = 0; x < highest_rank; x++)
                tempRestaurantsPankings[x] = 0;

            for (int x = 0; x < data.Count; x++)
                for (int y = 0; y < data[x].Count; y++)
                    //data[x][y].Ranking += highest_rank - y;
                    data[x][y].Ranking = highest_rank - y;

            bool[] seenRecordFromSE = new bool[data.Count];//data.Count is # of search engines
            foreach (Restaurant r in Globals.mergeList)
            {
                //Globals.WriteOutput(r.Name);
                r.Ranking = 0;
                //this is to know which search engine have a matching record with r
                for (int i = 0; i < seenRecordFromSE.Length; i++)
                    seenRecordFromSE[i] = false;

                string matchRankings = "";
                foreach (Restaurant rm in r.matchingRestaurants)
                {
                    matchRankings =  matchRankings + rm.Ranking.ToString() + " ";
                    r.Ranking = r.Ranking + rm.Ranking;
                    int arrIndex = (int)Math.Log(rm.SearchEngine, 2); //it is assumed that rm.SearchEngine it is the index/ID of the SE where rm came from
                    seenRecordFromSE[arrIndex] = true;
                }
                //Globals.WriteOutput("Rankings of matching restaurants: " + matchRankings);
                //addjust the ranking for the rest of SEs
                string adjustmentRankings = "";
                for (int i = 0; i < seenRecordFromSE.Length; i++)
                {
                    if (seenRecordFromSE[i] == true)
                        continue;

                    int indexSE = (int)Math.Pow(2, i);
                    int numRecordsinSE = 0;
                    if (Globals.allData[i].Count == 0)
                        continue;

                    switch (indexSE)
                    {
                        case (int)Engine.Metromix:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.DexKnows:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.Yelp:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.ChicagoReader:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.Menuism:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.MenuPages:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.Yahoo:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.YellowPages:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        case (int)Engine.CitySearch:
                            numRecordsinSE = Globals.allData[i].Count;
                            break;
                        default:
                            break;
                    }

                    int adjustment = 0;
                    if (numRecordsinSE != 0 || numRecordsinSE != highest_rank)
                    {
                        if ((highest_rank - numRecordsinSE) == 0)
                            adjustment = (nTotalRank - numRecordsinSE * (numRecordsinSE + 1) / 2) / 1;
                        else
                            adjustment = (nTotalRank - numRecordsinSE * (numRecordsinSE + 1) / 2) / (highest_rank - numRecordsinSE);
                        adjustmentRankings = adjustmentRankings + adjustment.ToString() + " ";
                        r.Ranking += adjustment;
                    }
                }
                //Globals.WriteOutput("Adjustment Rankings: " + adjustmentRankings);

            }

            //END BALANCED BORDA
            

            foreach (Restaurant r in Globals.mergeList)
            {
                if (r.IsClosed)
                    Globals.closedRestaurants.Add(r);
            }
            foreach (Restaurant r in Globals.closedRestaurants)
            {
                Globals.mergeList.Remove(r);
            }
            
            Globals.finalResultsCount = Globals.mergeList.Count;
            

            orderListByRankingDesc(Globals.mergeList);
            Globals.mergeList.AddRange(Globals.closedRestaurants);
            int k = 0;
            for (int i = 0; i < Globals.mergeList.Count; i++)
            {
                if (Globals.mergeList[i].Name.Contains("THE FOLLOWING"))
                {
                    continue;
                }
                k++;
                Globals.mergeList[i].Ranking = k;
            }

            //compute the nDCG between Yumi and Zagat
            //if (!Globals.cuisine.Equals("Fondue"))
            //{
            //    double[] nDCGs = null;
            //    List<Restaurant> arbitratorList = new List<Restaurant>();
            //    Globals.copyList(arbitratorList, Globals.zagatData);
            //    for (int i = 0; i < arbitratorList.Count; i++)
            //        arbitratorList.RemoveAll(r => r.ZipCode.Equals("99999"));
            //    computeNDCG(Globals.mergeList, arbitratorList, out nDCGs);
            //    Globals.WriteOutput(Globals.location + "," + Globals.cuisine + "," + nDCGs[0] + "," + nDCGs[1]);
            //}
            return Globals.mergeList;
        }


        public static void findMatchesMergeRatingCriteria(List<List<Restaurant>> data)
        {
            for (int i = 0; i < Globals.searchEngines.Count; i++)
            {
                if (Globals.enabledSE.Count > 0 && !Globals.enabledSE.Contains(Globals.searchEngines[i].getCode))
                    continue;
                else
                    mergeSubListsRatingsCriteria(Globals.mergeList, data[i]);
            }
            
            foreach (Restaurant r in Globals.mergeList)
            {
                if (r.NumReviews < 10)
                    r.Rating = Math.Max(r.Rating - .75, 1);
                else if (r.NumReviews > 50)
                    r.Rating = Math.Min(r.Rating + .75, 5);
                r.Rating = Math.Round(r.Rating, 2, MidpointRounding.AwayFromZero);
            }
            foreach (Restaurant r in Globals.mergeList)
            {                
                foreach (Restaurant rm in r.matchingRestaurants)
                {
                    rm.Rating = r.Rating;
                    rm.NumReviews = r.NumReviews;
                    rm.Criteria = r.Criteria;
                }
            }
        }

        public static List<Restaurant> sortByConstraints(List<Restaurant> data)
        {
            List<Restaurant> allConstraints = new List<Restaurant>();
            List<Restaurant> someConstraints = new List<Restaurant>();
            List<Restaurant> noConstraints = new List<Restaurant>();
            List<Restaurant> mergedList = new List<Restaurant>();
            foreach (Restaurant r in data)
            {
                if ((Globals.queryConstraints & r.Criteria) == Globals.queryConstraints)
                {
                    allConstraints.Add(r);
                }
                else if ((Globals.queryConstraints & r.Criteria) == 0)
                {
                    noConstraints.Add(r);
                }
                else
                {
                    someConstraints.Add(r);
                }
            }
            orderList(allConstraints);
            orderList(someConstraints);
            orderList(noConstraints);
            if (allConstraints.Count > 0)
            {
                Restaurant message = new Restaurant();
                message.Name = "THE FOLLLOWING SATISFY ALL CONSTRAINTS OF THE QUERY";
                message.ZipCode = "99999";
                mergedList.Add(message);

            }
            mergedList.AddRange(allConstraints);
            if (someConstraints.Count > 0)
            {
                Restaurant message = new Restaurant();
                message.Name = "THE FOLLLOWING SATISFY SOME CONSTRAINTS OF THE QUERY";
                message.ZipCode = "99999";
                mergedList.Add(message);

            }
            mergedList.AddRange(someConstraints);
            if (noConstraints.Count > 0)
            {
                Restaurant message = new Restaurant();
                message.Name = "THE FOLLLOWING SATISFY NONE OF THE CONSTRAINTS OF THE QUERY";
                message.ZipCode = "99999";
                mergedList.Add(message);

            }
            mergedList.AddRange(noConstraints);
            int k = 0;
            for (int i = 0; i < mergedList.Count; i++)
            {
                if (mergedList[i].Name.Contains("THE FOLLLOWING SATISFY"))
                {
                    continue;
                }
                k++;
                mergedList[i].Ranking = k;
            }
            //foreach (Restaurant r in mergedList)
            //{
            //    Globals.WriteOutput(r.Name);
            //}
            return mergedList;
            
        }


        public static List<Restaurant> mergeSubLists(List<Restaurant> list1, List<Restaurant> list2)
        {
            for (int i = 0; i < list2.Count; i++)
            {
                if(isInList(list1, list2[i]) == -1)
                    list1.Add(list2[i]);
            }

            return list1;
        }

        public static List<Restaurant> mergeSubListsKeepDupes(List<Restaurant> list1, List<Restaurant> list2)
        {
            for (int i = 0; i < list2.Count; i++)
            {
                list1.Add(list2[i]);
            }

            return list1;
        }

        public static List<Restaurant> mergeSubListsRatingsCriteria(List<Restaurant> list1, List<Restaurant> list2)
        {
            int index;
            double totalReviews;
            for (int i = 0; i < list2.Count; i++)
            {
                index = isInList(list1, list2[i]);
                if (index == -1)
                {
                    Restaurant r = Globals.copyRestaurant(list2[i]);
                    r.matchingRestaurants.Add(list2[i]);
                    list1.Add(r);
                }
                else
                {
                    totalReviews = list1[index].NumReviews + list2[i].NumReviews;
                    list1[index].Criteria = list1[index].Criteria | list2[i].Criteria;
                    list1[index].SearchEngine = list1[index].SearchEngine | list2[i].SearchEngine;
                    if (list1[index].Neighborhood.Equals("") && !list2[i].Neighborhood.Equals(""))
                        list1[index].Neighborhood = list2[i].Neighborhood;
                    if (list1[index].Cuisine.Equals("") && !list2[i].Cuisine.Equals(""))
                        list1[index].Cuisine = list2[i].Cuisine;    
                    if (list1[index].Price.Equals("") && !list2[i].Price.Equals(""))
                        list1[index].Price = list2[i].Price;
                    if (list1[index].PhoneNumber.Equals("") && !list2[i].PhoneNumber.Equals(""))
                        list1[index].PhoneNumber = list2[i].PhoneNumber;
                    list1[index].matchingRestaurants.Add(list2[i]);
                    if (totalReviews != 0)
                    {
                        list1[index].Rating = (list1[index].Rating * (list1[index].NumReviews / totalReviews)) + (list2[i].Rating * (list2[i].NumReviews / totalReviews));
                        list1[index].NumReviews = totalReviews;                        
                    }
                }
            }

            return list1;
        }

        private static int isInList(List<Restaurant> list, Restaurant r)
        {
            if (list.Count == 0)
                return -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (r.isMatch(list[i]))
                    return i;
                else if (isInList(list[i].matchingRestaurants, r) > -1)
                    return i;
            }

            return -1;
        }

        //Normalized discounted cumulative gain (DCG) is a measure of effectiveness of a Web search engine algorithm 
        //Ideally, we compute the NDCG at rank 5 and 10
        public static double[] computeNDCG(List<Restaurant> yumiList,List<Restaurant> arbitratorList)
        {
            const double CT_NO_RECORDS = -1000.0;
            double[] nDCGs = new double[2];
            nDCGs[0] = CT_NO_RECORDS; //nDCG at rank 5
            nDCGs[1] = CT_NO_RECORDS; //nDCG at rank 10
            
            
            //if ZAGAT does not return any rezult the query is ignored
            //however we are interested if the tested search engine does return results
            //in this case we encode:
            // nDCGs[0] is for ZAGAT and nDCGs[1] is for the search engine which is tested
            if (arbitratorList == null || arbitratorList.Count() == 0)
            {
                nDCGs[1] = (yumiList == null || yumiList.Count() == 0) ? CT_NO_RECORDS : -1 * yumiList.Count();
                return nDCGs;
            }

            //return 0;
            if(yumiList == null || yumiList.Count() == 0)
            {                
                    nDCGs[0] = 0.0;
                    nDCGs[1] = 0.0;
                    return nDCGs;                
            }
            
            //both lists have records.
            //if the arbitrator have less than 5 records then igonre the query
            //if(arbitratorList.Count() < 5)
            //    return nDCGs;

            const int rank10 = 10;
            //get the highest Borda index
            int nMaxArbitratorElements = arbitratorList.Count() > rank10 ? rank10 : arbitratorList.Count();
            for(int i = 0; i < nMaxArbitratorElements; i++)
            {
                Restaurant r = arbitratorList.ElementAt(i);                
                r.RelevanceScore = nMaxArbitratorElements - i;
            }

            //copy the top-10 from yumi to a temporaty list
            List<Restaurant> yumiListTop10 = new List<Restaurant>();
            for (int i = 0; i < yumiList.Count() && i < rank10; i++)
                yumiListTop10.Add(yumiList.ElementAt(i));

            //assign the relevance scores according to the arbitrator
            bool hasMatch = false;
            for (int i = 0; i < yumiListTop10.Count(); i++)
            {
                Restaurant rYumi = yumiListTop10.ElementAt(i);
                for (int j = 0; j < nMaxArbitratorElements; j++)
                {
                    Restaurant rArbitrator = arbitratorList.ElementAt(j);
                    if (rYumi.isMatch(rArbitrator))
                    {
                        rYumi.RelevanceScore = rArbitrator.RelevanceScore;
                        hasMatch = true;
                        break;
                    }

                }
            }



            if (!hasMatch)
            {
                nDCGs[0] = -1 * CT_NO_RECORDS;
                nDCGs[1] = -1 * CT_NO_RECORDS;
                return nDCGs; 
            }

            for (int i = 0; i < yumiListTop10.Count(); i++)
                Trace.WriteLine(yumiListTop10[i].RelevanceScore);

            //compute DCG at rank 5
            const int rank5 = 5;
            double DCG5 = yumiListTop10.ElementAt(0).RelevanceScore;
            //we want to account for the case when the returned list has fewer than 5 records
            for (int i = 1; i < rank5 && i < yumiListTop10.Count(); i++)
            {
                Restaurant rYumi = yumiListTop10.ElementAt(i);
                int relScore = rYumi.RelevanceScore;
                DCG5 += (double)relScore / Math.Log(i + 1, 2);
            }

            //compute DCG at rank 10
            double DCG10 = DCG5;
            //we want to account for the case when the returned list has fewer than 5 records
            for (int i = 5; i < yumiListTop10.Count() && i < rank10; i++)
            {
                Restaurant rYumi = yumiListTop10.ElementAt(i);
                int relScore = rYumi.RelevanceScore;
                DCG10 += (double)relScore / Math.Log(i + 1, 2);
            }

            //now we need to compute the IDCG
            //get the ideal query ordering


            /**********************************/
            /*ARBIUTRATOR BASED ndcg*/
            /**********************************/
            //compute DCG at rank 5
            double IDCG5_arb = arbitratorList.ElementAt(0).RelevanceScore;
            for (int i = 1; i < rank5 && i < arbitratorList.Count(); i++)
            {
                Restaurant rArbitrator = arbitratorList.ElementAt(i);
                int relScore = rArbitrator.RelevanceScore;
                IDCG5_arb += (double)relScore / Math.Log(i + 1, 2);
            }

            //compute DCG at rank 10
            double IDCG10_arb = IDCG5_arb;
            for (int i = 5; i < arbitratorList.Count() && i < rank10; i++)
            {
                Restaurant rArbitrator = arbitratorList.ElementAt(i);
                int relScore = rArbitrator.RelevanceScore;
                IDCG10_arb += (double)relScore / Math.Log(i + 1, 2);
            }

            nDCGs[0] = DCG5 / IDCG5_arb;
            nDCGs[1] = DCG10 / IDCG10_arb;
            Trace.WriteLine("DCG5: " + DCG5);
            Trace.WriteLine("DCG10: " + DCG10);
            Trace.WriteLine("IDCG5: " + IDCG5_arb);
            Trace.WriteLine("IDCG10: " + IDCG10_arb);
            /**********************************/

            /*
            //copy the top-5 from yumi to a temporaty list
            List<Restaurant> yumiListTop5 = new List<Restaurant>();
            for (int i = 0; i < yumiListTop5.Count(); i++)
                yumiListTop5.Add(yumiListTop10.ElementAt(i));

            yumiListTop10.Sort();
            yumiListTop5.Sort();

            //compute DCG at rank 5
            double IDCG5 = yumiListTop5.ElementAt(0).RelevanceScore;
            for (int i = 1; i < rank5 && i < yumiListTop5.Count(); i++)
            {
                Restaurant rYumi = yumiListTop5.ElementAt(i);
                int relScore = rYumi.RelevanceScore;
                IDCG5 += (double)relScore / Math.Log(i + 1, 2);
            }

            //compute DCG at rank 10
            double IDCG10 = yumiListTop10.ElementAt(0).RelevanceScore;
            for (int i = 1; i < yumiListTop10.Count() && i < rank10; i++)
            {
                Restaurant rYumi = yumiListTop10.ElementAt(i);
                int relScore = rYumi.RelevanceScore;
                IDCG10 += (double)relScore / Math.Log(i + 1, 2);
            }

            nDCGs[0] = DCG5 / IDCG5;
            nDCGs[1] = DCG10 / IDCG10;
             * */
            return nDCGs;
        }



        //public static void computeNDCGNine(List<List<Restaurant>> yumiList, List<Restaurant> arbitratorList, String searchEngine)
        //{
        //    //Globals.WriteOutput("yumiEightList:");
        //    //listCounts(yumiList);
        //    //Globals.WriteOutput("arbitratorList: " + arbitratorList);

        //    MergeResults.findMatchesMergeRatingCriteria(yumiList);
        //    for (int i = 0; i < yumiList.Count; i++)
        //    {
        //        yumiList[i] = MergeResults.sortByConstraints(yumiList[i]);   
        //    }

        //    //System.Diagnostics.Trace.WriteLine("Globals.allData: "  + Globals.allData.Count); 

        //    for (int i = 0; i < yumiList.Count; i++)
        //        yumiList[i].RemoveAll(r => r.ZipCode.Equals("99999"));

        //    MergeResults.merge(yumiList);


        //    if (searchEngine.Equals("Metromix"))
        //    {
        //        DateTime EndTime = DateTime.Now;
        //        TimeSpan span = EndTime.Subtract(Globals.startTime);
        //        double[] nDCGYumi = null;
        //        nDCGYumi = MergeResults.computeNDCG(Globals.mergeList, Globals.zagatData);
        //        double[] nDCGNinth = null;
        //        nDCGNinth = MergeResults.computeNDCG(arbitratorList, Globals.zagatData);
        //        Globals.WriteOutput(searchEngine + "," + Globals.location + "," + Globals.cuisine + "," + nDCGYumi[0] + "," + nDCGYumi[1] + "," + nDCGNinth[0] + "," + nDCGNinth[1] + "," + span.Seconds);
        //    }
        //    else
        //    {
        //        double[] nDCGYumi = null;
        //        //Globals.WriteOutput("mergeList: " + Globals.mergeList.Count);
        //        //Globals.WriteOutput("zagatData: " + Globals.zagatData.Count);
        //        nDCGYumi = MergeResults.computeNDCG(Globals.mergeList, Globals.zagatData);
        //        double[] nDCGNinth = null;
        //        //Globals.WriteOutput("arbitrator: " + arbitratorList.Count);
        //        //Globals.WriteOutput("zagatData: " + Globals.zagatData.Count);
        //        nDCGNinth = MergeResults.computeNDCG(arbitratorList, Globals.zagatData);
        //        Globals.WriteOutput(searchEngine + "," + Globals.location + "," + Globals.cuisine + "," + nDCGYumi[0] + "," + nDCGYumi[1] + "," + nDCGNinth[0] + "," + nDCGNinth[1]);
        //    }
        //    Globals.mergeList.Clear();
        //    //double[] nDCGs = null;
        //    //List<Restaurant> arbitratorList = new List<Restaurant>();
        //    //Globals.copyList(arbitratorList, Globals.zagatData);
        //    //for (int i = 0; i < arbitratorList.Count; i++)
        //    //    arbitratorList.RemoveAll(r => r.ZipCode.Equals("99999"));
        //    //MergeResults.computeNDCG(Globals.mergeList, arbitratorList, out nDCGs);
        //    //Globals.WriteOutput(Globals.location + "," + Globals.cuisine + "," + nDCGs[0] + "," + nDCGs[1]);

        //}

        public static void listCounts(List<List<Restaurant>> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                //Globals.WriteOutput("[" + i + "]: " + data[i].Count); 
            }

        }

   
    }
}

