using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace yumi
{
    public class CuisineList
    {
        private List<TargetCuisine> cuisines;
        private String Name;
        private int level;

        public CuisineList(String nm)
        {
            cuisines = new List<TargetCuisine>();
            Name = nm;
        }

        public String getName() { return Name; }

        public void addCuisine(int code, String cui)
        {
            if(!doesExits(cui, code))
                cuisines.Add(new TargetCuisine(code, cui));
        }

        public List<String> getCuisinesForSearchEngine(int code)
        {
            List<String> cuis = new List<String>();
            foreach (TargetCuisine t in cuisines)
            {
                if (t.belongsToSearchEngine(code))
                {
                    cuis.Add(t.getName());
                }
            }

            return cuis;
        }

        public String getCuisine(int index)
        {
            return cuisines[index].getName();
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public Boolean doesExits(string cui, int code)
        {
            foreach (TargetCuisine t in cuisines)
                if (t.getName().Equals(cui) && t.getSearchEngineCode() == code)
                    return true;

            return false;
        }

        public int getCuisinesCount()
        {
            return cuisines.Count();
        }
    }
}
