using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Device.Location;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace ReittiAPI
{
    [DataContract(IsReference = true)]
    public class Location
    {
        [DataMember]
        public string LocType;
        [DataMember]
        public Dictionary<string, string> NamesByLang;
        [DataMember]
        public string MatchedName;
        [DataMember]
        public string MatchedLongName;
        [DataMember]
        public Dictionary<string, string> CitiesByLang;
        [DataMember]
        public ReittiCoordinate Coords;

        public string Name
        {
            get
            {
                return GetName();
            }
        }

        public string LongName
        {
            get
            {
                return Utils.FormatLongName(Name, Utils.GetPreferredName(CitiesByLang));
            }
        }

        public Location()
        {
            NamesByLang = new Dictionary<string, string>();
            CitiesByLang = new Dictionary<string, string>();
        }

        public virtual void UpdateFromGeoResponse(JToken source, string responseLang)
        {
            var name = source.Value<string>("name");
            NamesByLang[responseLang] = name;

            var city = source.Value<string>("city");
            CitiesByLang[responseLang] = city;

            LocType = source.Value<string>("locType");
            MatchedName = source.Value<string>("matchedName");
            if (MatchedName != null)
            {
                MatchedLongName = Utils.FormatLongName(MatchedName, city);
                var lang = source.Value<string>("lang");
                NamesByLang[lang] = MatchedName;
            }
            else
            {
                MatchedLongName = null;
            }

            string coordintateString = source.Value<string>("coords");
            Coords = new ReittiCoordinate(coordintateString);
        }

        protected virtual string GetName()
        {
            return Utils.GetPreferredName(NamesByLang);
        }
    }

    [DataContract(IsReference = true)]
    public class Stop : Location
    {
        private static readonly Departure[] EmptyDepartures = new Departure[0];

        [DataMember]
        public string Code;
        [DataMember]
        public string ShortCode;
        [DataMember]
        public List<ConnectedLines> ConnectedLines;
        [DataMember]
        public Departure[] Departures;
        [DataMember]
        public Uri TimetableLink;
        [DataMember]
        public Uri OmatlahdotLink;
        [DataMember]
        public int Dist;

        public string DisplayCode
        {
            get
            {
                if (ShortCode != null)
                {
                    return ShortCode;
                }
                return Code;
            }
        }

        public Stop(string code)
        {
            Code = code;
            ConnectedLines = new List<ConnectedLines>();
            Departures = EmptyDepartures;
        }

        public override void UpdateFromGeoResponse(JToken source, string responseLang)
        {
            base.UpdateFromGeoResponse(source, responseLang);

            JToken details = source["details"];

            string newCode = details.Value<string>("code");
            if (!Code.Equals(newCode))
            {
                throw new ArgumentException("The unique code of the source does not match the existing one.");
            }

            string shortCode = details.Value<string>("shortCode");
            if (shortCode != "")
            {
                ShortCode = shortCode;
            }

            JArray linesArray = (JArray)details["lines"];
            if (linesArray != null)
            {
                var connectedLinesByCode = new Dictionary<string, ConnectedLines>();
                for (int i = 0; i < linesArray.Count; ++i)
                {
                    var line = Utils.ParseShortLine(linesArray.Value<string>(i));

                    string codeSansDirection = line.Code.Substring(0, line.Code.Length - 1);
                    if (!Char.IsLetter(codeSansDirection[codeSansDirection.Length - 1]))
                    {
                        codeSansDirection = codeSansDirection.Substring(0, codeSansDirection.Length - 1);
                    }

                    ConnectedLines connectedLines;
                    if (!connectedLinesByCode.TryGetValue(codeSansDirection, out connectedLines))
                    {
                        connectedLines = new ConnectedLines { CodeSansDirection = codeSansDirection };
                        connectedLinesByCode[codeSansDirection] = connectedLines;
                    }

                    connectedLines.Lines.Add(line);
                }
                ConnectedLines.AddRange(connectedLinesByCode.Values);
            }
        }

        public void UpdateFromStopResponse(JToken source)
        {
            string newCode = source.Value<string>("code");
            if (!Code.Equals(newCode))
            {
                throw new ArgumentException("The unique code of the source does not match the existing one.");
            }

            ShortCode = source.Value<string>("code_short");

            NamesByLang["fi"] = source.Value<string>("name_fi");
            NamesByLang["sv"] = source.Value<string>("name_sv");

            CitiesByLang["fi"] = source.Value<string>("city_fi");
            CitiesByLang["sv"] = source.Value<string>("city_sv");

            JArray linesArray = source["lines"] as JArray;
            if (linesArray != null)
            {
                var connectedLinesByCode = new Dictionary<string, ConnectedLines>();

                for (int i = 0; i < linesArray.Count; ++i)
                {
                    var line = Utils.ParseShortLine(linesArray.Value<string>(i));

                    string codeSansDirection = line.Code.Substring(0, line.Code.Length - 1);
                    if (!Char.IsLetter(codeSansDirection[codeSansDirection.Length - 1]))
                    {
                        codeSansDirection = codeSansDirection.Substring(0, codeSansDirection.Length - 1);
                    }

                    ConnectedLines connectedLines;
                    if (!connectedLinesByCode.TryGetValue(codeSansDirection, out connectedLines))
                    {
                        connectedLines = new ConnectedLines { CodeSansDirection = codeSansDirection };
                        connectedLinesByCode[codeSansDirection] = connectedLines;
                    }

                    connectedLines.Lines.Add(line);
                }

                ConnectedLines.Clear();
                ConnectedLines.AddRange(connectedLinesByCode.Values);
            }

            string coordinateString = source.Value<string>("coords");
            Coords = new ReittiCoordinate(coordinateString);

            JArray departuresArray = source["departures"] as JArray;
            if (departuresArray != null)
            {
                var _departuresArray = new Departure[departuresArray.Count];
                Departures = _departuresArray;
                for (int i = 0; i < _departuresArray.Length; ++i)
                {
                    JToken departureToken = departuresArray[i];

                    string code = departureToken.Value<string>("code");
                    string date = departureToken.Value<string>("date");
                    string time = departureToken.Value<string>("time");

                    string hoursChars = time.Substring(0, time.Length - 2);
                    string minutesChars = time.Substring(time.Length - 2);

                    int days = 0;
                    int hours = int.Parse(hoursChars);
                    while (hours >= 24)
                    {
                        hours -= 24;
                        days += 1;
                    }

                    int minutes = int.Parse(minutesChars);

                    var dateTime = DateTime.ParseExact(date, "yyyyMMdd", null);
                    dateTime = dateTime.Add(new TimeSpan(days, hours, minutes, 0));

                    _departuresArray[i] = new Departure
                    {
                        Line = ModelCache.GetOrCreate(code, () => new Line(code)),
                        Time = dateTime
                    };
                }
            }

            string timetableLinkString = source.Value<string>("timetable_link");
            TimetableLink = new Uri(Utils.CleanUrl(timetableLinkString));

            string omatlahdotLinkString = source.Value<string>("omatlahdot_link");
            OmatlahdotLink = new Uri(Utils.CleanUrl(omatlahdotLinkString));
        }

        public void UpdateFromLineResponse(JToken source, string responseLang)
        {
            string newCode = source.Value<string>("code");
            if (!Code.Equals(newCode))
            {
                throw new ArgumentException("The unique code of the source does not match the existing one.");
            }

            ShortCode = source.Value<string>("codeShort");

            NamesByLang[responseLang] = source.Value<string>("name");
            CitiesByLang[responseLang] = source.Value<string>("city_name");

            string coordinateString = source.Value<string>("coords");
            Coords = new ReittiCoordinate(coordinateString);
        }

        public void UpdateFromStopsInAreaResponse(JToken source, string responseLang)
        {
            string newCode = source.Value<string>("code");
            if (!Code.Equals(newCode))
            {
                throw new ArgumentException("The unique code of the source does not match the existing one.");
            }

            ShortCode = source.Value<string>("codeShort");

            NamesByLang[responseLang] = source.Value<string>("name");
            CitiesByLang[responseLang] = source.Value<string>("city");

            string coordinateString = source.Value<string>("coords");
            Coords = new ReittiCoordinate(coordinateString);

            Dist = source.Value<int>("dist");
        }
    }

    [DataContract(IsReference = true)]
    public class ConnectedLocations<T> where T : Location
    {
        [DataMember]
        public string Name;
        [DataMember]
        public List<T> Locations;

        public ConnectedLocations()
        {
            Locations = new List<T>();
        }
    }

    [DataContract(IsReference = true)]
    public class ConnectedStops : ConnectedLocations<Stop>
    {
        private const double _combineThreshold = 0.000000185;

        [DataMember]
        public ReittiCoordinate Center;
        [DataMember]
        public string DisplayCode;

        public ConnectedStops(Stop stop)
            : base()
        {
            Locations.Add(stop);
            Center = stop.Coords;
            Name = stop.Name;
            DisplayCode = stop.DisplayCode;
        }

        public bool TryAdd(Stop stop)
        {
            if (Name != stop.Name)
            {
                return false;
            }

            if (DisplayCode != stop.DisplayCode)
            {
                return false;
            }

            double dLa = Center.Latitude - stop.Coords.Latitude;
            double dLo = Center.Longitude - stop.Coords.Longitude;

            if (dLa * dLa + dLo * dLo > _combineThreshold)
            {
                return false;
            }

            Locations.Add(stop);

            UpdateCenter();

            return true;
        }

        public void UpdateCenter()
        {
            double lat = 0, lon = 0;
            foreach (var stop in Locations)
            {
                lat += stop.Coords.Latitude;
                lon += stop.Coords.Longitude;
            }
            lat /= Locations.Count;
            lon /= Locations.Count;

            Center = new ReittiCoordinate(lat, lon);
        }

        public void Sort()
        {
            Locations.Sort((s1, s2) =>
            {
                return s1.Code.CompareTo(s2.Code);
            });
        }
    }

    [DataContract(IsReference = true)]
    public class Address : Location
    {
        [DataMember]
        public int HouseNumber;

        public override void UpdateFromGeoResponse(JToken source, string responseLang)
        {
            base.UpdateFromGeoResponse(source, responseLang);

            var details = source.SelectToken("details");
            if (details != null)
            {
                HouseNumber = details.Value<int>("houseNumber");

                if (MatchedName != null)
                {
                    MatchedName = MatchedName + " " + HouseNumber;
                    MatchedLongName = Utils.FormatLongName(MatchedName, CitiesByLang[responseLang]);
                }
            }

            var correctedNames = new Dictionary<string,string>();
            foreach (var name in NamesByLang)
            {
                string city;
                if (CitiesByLang.TryGetValue(name.Key, out city))
                {
                    int i;
                    if ((i = name.Value.LastIndexOf(" " + HouseNumber + ", " + city)) != -1)
                    {
                        correctedNames.Add(name.Key, name.Value.Remove(i));
                    }
                }
            }
            foreach (var name in correctedNames)
            {
                NamesByLang[name.Key] = name.Value;
            }
        }

        protected override string GetName()
        {
            return base.GetName() + " " + HouseNumber;
        }
    }

    [DataContract(IsReference = true)]
    public class Poi : Location
    {
        [DataMember]
        public string PoiType;

        public override void UpdateFromGeoResponse(JToken source, string responseLang)
        {
            base.UpdateFromGeoResponse(source, responseLang);

            PoiType = source.Value<string>("poiType");
        }
    }

    [DataContract(IsReference = true)]
    public class ConnectedLines
    {
        [DataMember]
        public string CodeSansDirection;
        [DataMember]
        public List<Line> Lines;

        public ConnectedLines()
        {
            Lines = new List<Line>();
        }
    }

    [DataContract(IsReference = true)]
    public class Line
    {
        private static readonly TimeSpan[] EmptyStopTimes = new TimeSpan[0];
        private static readonly ReittiCoordinate[] EmptyReittiCoordinates = new ReittiCoordinate[0];

        [DataMember]
        public string Code;
        [DataMember]
        public string ShortName;
        [DataMember]
        public int? TransportTypeId;
        [DataMember]
        public string LineStart;
        [DataMember]
        public string LineEnd;
        [DataMember]
        public Dictionary<string, string> NamesByLang;
        [DataMember]
        public Uri TimetableUrl;
        [DataMember]
        public ReittiCoordinate[] LineShape;
        [DataMember]
        public Stop[] LineStops;
        [DataMember]
        public TimeSpan[] StopTimes;

        public Line(string code)
        {
            Code = code;
            ParseCode();
            NamesByLang = new Dictionary<string, string>();
            LineShape = EmptyReittiCoordinates;
            StopTimes = EmptyStopTimes;
        }

        public void UpdateFromLineResponse(JToken source, string responseLang)
        {
            string newCode = source.Value<string>("code");
            if (!Code.Equals(newCode))
            {
                throw new ArgumentException("The unique code of the source does not match the existing one.");
            }

            //ShortCode = source.Value<string>("code_short"); // The short codes from the API are often incorrect/uninformative

            TransportTypeId = source.Value<int>("transport_type_id");

            LineStart = source.Value<string>("line_start");

            LineEnd = source.Value<string>("line_end");

            NamesByLang[responseLang] = source.Value<string>("name");

            string timetableUrlString = source.Value<string>("timetable_url");
            TimetableUrl = new Uri(Utils.CleanUrl(timetableUrlString));

            string lineShapeString = source.Value<string>("line_shape");
            LineShape = Utils.ParseAsArray(lineShapeString, (s) => new ReittiCoordinate(s));

            JArray stopsArray = source["line_stops"] as JArray;
            if (stopsArray != null)
            {
                LineStops = new Stop[stopsArray.Count];
                StopTimes = new TimeSpan[stopsArray.Count];
                for (int i = 0; i < StopTimes.Length; ++i)
                {
                    JToken token = stopsArray[i];
                    string code = token.Value<string>("code");
                    int time = token.Value<int>("time");

                    var stop = ModelCache.GetOrCreate(code, () => new Stop(code));
                    stop.UpdateFromLineResponse(token, responseLang);
                    LineStops[i] = stop;
                    StopTimes[i] = TimeSpan.FromMinutes(time);
                }
            }
        }

        private static Regex startingLetters = new Regex(@"(\D*)");

        private void ParseCode()
        {
            char areaCode = Code[0];
            string lineCode = Code.Substring(1, 3);

            if (areaCode == '1')
            {
                if (lineCode[0] == '3')
                {
                    if (Code[4] == 'V')
                    {
                        ShortName = "Metro";
                        return;
                    }
                    else
                    {
                        ShortName = "Metro";
                        return;
                    }
                }
                else if (lineCode == "019")
                {
                    ShortName = "Ferry";
                    return;
                }
                else if (lineCode[0] == '1' || lineCode[0] == '2')
                {
                    lineCode = lineCode.Substring(1);
                    ShortName = ParseShortCode(lineCode);
                    return;
                }
            }
            else if (areaCode == '2')
            {
                if (lineCode[0] == '9')
                {
                    lineCode = lineCode.Substring(1);
                    ShortName = ParseShortCode(lineCode);
                    return;
                }
            }
            else if (areaCode == '3') // Local trains
            {
                ShortName = Utils.RemoveLeadingNumbers(ParseShortCode(lineCode));
                return;
            }

            ShortName = ParseShortCode(lineCode);
        }

        private string ParseShortCode(string lineCode)
        {
            while (lineCode[0] == '0')
            {
                lineCode = lineCode.Substring(1);
            }

            string letterVariant = Code.Substring(4, 2);
            var match = startingLetters.Match(letterVariant.Trim());
            letterVariant = match.Groups[0].Value;

            return lineCode + letterVariant;
        }
    }

    [DataContract]
    public class Departure
    {
        [DataMember]
        public Line Line;
        [DataMember]
        public DateTime Time;
    }

    [DataContract]
    public class CompoundRoute
    {
        private static readonly Route[] EmptyRoutes = new Route[0];

        public CompoundRoute()
        {
            Routes = EmptyRoutes;
        }

        [DataMember]
        public Route[] Routes;
    }

    [DataContract]
    public class Route
    {
        public Route(JToken token)
        {
            Length = token.Value<double>("length");

            int durationSeconds = token.Value<int>("duration");
            Duration = TimeSpan.FromSeconds(durationSeconds);

            JArray legTokens = token["legs"] as JArray;
            if (legTokens != null)
            {
                Legs = new Leg[legTokens.Count];
                for (int i = 0; i < Legs.Length; ++i)
                {
                    Legs[i] = new Leg(legTokens[i]);
                }
            }
        }

        [DataMember]
        public double Length;
        [DataMember]
        public TimeSpan Duration;
        [DataMember]
        public Leg[] Legs;
    }

    [DataContract]
    public class Leg
    {
        public Leg(JToken token)
        {
            Length = token.Value<double>("length");

            double durationSeconds = token.Value<double>("duration");
            Duration = TimeSpan.FromSeconds((int)Math.Round(durationSeconds));

            Type = token.Value<string>("type");

            string lineCode = token.Value<string>("code");
            if (lineCode != null)
            {
                Line = ModelCache.GetOrCreate(lineCode, () => new Line(lineCode));
            }

            JArray locTokens = token["locs"] as JArray;
            if (locTokens != null)
            {
                Locs = new LegLocation[locTokens.Count];
                for (int i = 0; i < Locs.Length; ++i)
                {
                    Locs[i] = new LegLocation(locTokens[i]);
                }
            }
            else
            {
                Locs = new LegLocation[0];
            }

            JArray shapeTokens = token["shape"] as JArray;
            if (shapeTokens != null)
            {
                Shape = new ReittiCoordinate[shapeTokens.Count];
                for (int i = 0; i < Shape.Length; ++i)
                {
                    var shapeToken = shapeTokens[i];
                    double x = shapeToken.Value<double>("x");
                    double y = shapeToken.Value<double>("y");
                    var coordinate = new ReittiCoordinate(y, x);
                    Shape[i] = coordinate;
                }
            }
        }

        [DataMember]
        public double Length;
        [DataMember]
        public TimeSpan Duration;
        [DataMember]
        public string Type;
        [DataMember]
        public Line Line;
        [DataMember]
        public LegLocation[] Locs;
        [DataMember]
        public ReittiCoordinate[] Shape;
    }

    [DataContract]
    public class LegLocation
    {
        public LegLocation(JToken token)
        {
            var coordToken = token["coord"];
            double x = coordToken.Value<double>("x");
            double y = coordToken.Value<double>("y");
            Coord = new ReittiCoordinate(y, x);

            string arrTimeString = token.Value<string>("arrTime");
            ArrTime = DateTime.ParseExact(arrTimeString, "yyyyMMddHHmm", null);

            string depTimeString = token.Value<string>("depTime");
            DepTime = DateTime.ParseExact(depTimeString, "yyyyMMddHHmm", null);

            Name = token.Value<string>("name");
        }

        [DataMember]
        public ReittiCoordinate Coord;
        [DataMember]
        public DateTime ArrTime;
        [DataMember]
        public DateTime DepTime;
        [DataMember]
        public string Name;
    }
}
