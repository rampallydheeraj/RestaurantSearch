using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace yumi
{   
    public interface ISearchEngine
    {
        int getCode { get;}        
        string getName { get; }
        int getConstraints { get; }
        void GetLocationCrosswalkFromXml();
        void GetCuisineCrosswalkFromXml();
        string ComplexRestaurantUrl(String key);
        List<Restaurant> processRequest(string location, string cuisine, string price, string keyword);
        void createRequestsAsync(List<string> locations, string cuisine, string price, string keyword);
        
    }
}
