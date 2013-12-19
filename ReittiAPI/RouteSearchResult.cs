using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace ReittiAPI
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
