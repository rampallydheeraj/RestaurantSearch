using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Subgurim.Controles;

namespace yumi
{
    public class PolygonWrapper
    {
        private double area;
        private GPolygon polygon;
        private string hoodName;

        public double Area
        {
            get { return area; }
            set { area = value; }
        }

        public GPolygon Polygon
        {
            get { return polygon; }
            set { polygon = value; }
        }

        public string HoodName
        {
            get { return hoodName; }
            set { hoodName = value; }
        }
    }
}