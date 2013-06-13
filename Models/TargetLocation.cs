using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace yumi
{
    class TargetLocation
    {
        private int SearchEngineCode;
        private String Name;

        public TargetLocation()
        {
            SearchEngineCode = 0;
            Name = "";
        }
        
        public TargetLocation(int code, String loc)
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
