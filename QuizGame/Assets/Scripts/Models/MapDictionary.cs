using System;
using System.Collections.Generic;
namespace MapDictionary {
    
    public class AreaData
    {
        public string Name { get; set; }
        public List<string> Areas { get; set; }
    }

    public class Root
    {
        public List<AreaData> AreaList { get; set; }
    }

}