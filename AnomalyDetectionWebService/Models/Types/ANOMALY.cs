using System;
using System.Collections.Generic;

// Span should be list of 2 elements: [start_inclusive, end_exclusive]
using Span = System.Collections.Generic.List<long>;

namespace AnomalyDetectionWebService.Models.Types
{
    // {"anomalies":{“altitude_gps”: [[100, 110], [20, 120] …], “heading_gps”: [] , ... },
    //  "reason" :  {“altitude_gps”: "Minimal Circle with feature123" , ... } }
    public class ANOMALY
    {
        public Dictionary<string, List<Span>> anomalies { get; set; }
        public Dictionary<string, string> reason { get; set; }
    }
}


