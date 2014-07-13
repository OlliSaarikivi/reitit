using System;
using System.Net;
using System.Windows;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Reitit.API
{
    [DataContract]
    public class RouteSearchResult
    {
        [DataMember]
        public string FromName;
        [DataMember]
        public string ToName;
        [DataMember]
        public CompoundRoute CompoundRoute;
    }
}
