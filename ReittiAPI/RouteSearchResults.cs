using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ReittiAPI
{
    [DataContract]
    public class RouteSearchResults
    {
        [DataMember]
        public List<RouteSearchResult> Results;
        [DataMember]
        public bool PreferLast;
        [DataMember]
        public ReittiCoordinate From;
        [DataMember]
        public ReittiCoordinate To;
        [DataMember]
        public string FromName;
        [DataMember]
        public string ToName;
        [DataMember]
        public string Optimize;
        [DataMember]
        public int Margin;
        [DataMember]
        public int WalkSpeed;
        [DataMember]
        public IEnumerable<string> TransportTypes;

        public RouteSearchResults()
        {
            Results = new List<RouteSearchResult>();
            TransportTypes = new List<string>();
        }
    }
}
