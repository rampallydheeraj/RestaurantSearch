using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace yumi
{
    class TargetCuisine
    {
        private int SearchEngineCode;
        private String Name;

        public TargetCuisine()
        {
            SearchEngineCode = 0;
            Name = "";
        }
        
        public TargetCuisine(int code, String loc)
        {
            SearchEngineCode = code;
            Name = loc;
        }   

        public int getSearchEngineCode(){return SearchEngineCode;}
        public String getName() { return Name; }

        public void setSearchEngineCode(int code)
        {
            SearchEngineCode = code;
        }

        public void setName(String loc)
        {
            Name = loc;
        }

        public Boolean belongsToSearchEngine(int code)
        {
            return SearchEngineCode == code;
        }
    }
}
