using System;
using System.Collections.Generic;
using System.Text;

namespace yumi
{
    public class Crosswalk
    {
        private string name;
        private string key;

        public Crosswalk()
        {
            name = "";
            key = "";
        }

        public Crosswalk(string n, string k)
        {
            name = n;
            key = k;
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
    }
}
