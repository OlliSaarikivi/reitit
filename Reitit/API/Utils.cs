using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;

namespace Reitit.API
{
    public static partial class Utils
    {
        // simplifyDP():
        //  This is the Douglas-Peucker recursive simplification routine
        //  It just marks vertices that are part of the simplified polyline
        //  for approximating the polyline subchain v[j] to v[k].
        //    Input:  tol = approximation tolerance
        //            v[] = polyline array of vertex points 
        //            j,k = indices for the subchain v[j] to v[k]
        //    Output: mk[] = array of markers matching vertex array v[]
        static void SimplifyDP(double tol2, ReittiCoordinate[] v, int j, int k, bool[] mk)
        {
            if (k <= j + 1) // there is nothing to simplify
                return;

            // check for adequate approximation by segment S from v[j] to v[k]
            int maxi = j;          // index of vertex farthest from S
            double maxd2 = 0;         // distance squared of farthest vertex
            double s0Lat = v[j].Latitude, s0Long = v[j].Longitude, s1Lat = v[k].Latitude, s1Long = v[k].Longitude;
            double vLat = s1Lat - s0Lat;
            double vLong = s1Long - s0Long;
            double cv = vLat * vLat + vLong * vLong;     // segment length squared

            // test each vertex v[i] for max distance from S
            // compute using the Feb 2001 Algorithm's  dist_Point_to_Segment()
            // Note: this works in any dimension (2D, 3D, ...)
            double b, cw, dv2;        // dv2 = distance v[i] to S squared

            for (int i = j + 1; i < k; i++)
            {
                double pLat = v[i].Latitude, pLong = v[i].Longitude;
                // compute distance squared
                double wLat = pLat - s0Lat;
                double wLong = pLong - s0Long;
                cw = vLat * wLat + vLong * wLong;
                if (cw <= 0)
                {
                    dv2 = wLat * wLat + wLong * wLong;
                }
                else if (cv <= cw)
                {
                    double uLat = pLat - s1Lat;
                    double uLong = pLong - s1Long;
                    dv2 = uLat * uLat + uLong * uLong;
                }
                else
                {
                    b = cw / cv;
                    double pbLat = s0Lat + b * vLat - pLat;
                    double pbLong = s0Long + b * vLong - pLong;
                    dv2 = pbLat * pbLat + pbLong * pbLong;
                }
                // test with current max distance squared
                if (dv2 <= maxd2)
                    continue;
                // v[i] is a new max vertex
                maxi = i;
                maxd2 = dv2;
            }
            if (maxd2 > tol2)        // error is worse than the tolerance
            {
                // split the polyline at the farthest vertex from S
                mk[maxi] = true;      // mark v[maxi] for the simplified polyline
                // recursively simplify the two subpolylines at v[maxi]
                SimplifyDP(tol2, v, j, maxi, mk);  // polyline v[j] to v[maxi]
                SimplifyDP(tol2, v, maxi, k, mk);  // polyline v[maxi] to v[k]
            }
            // else the approximation is OK, so ignore intermediate vertices
            return;
        }

        public static string FormatLongName(string name, string city)
        {
            return new StringBuilder(name).Append(", ").Append(city).ToString();
        }

        public static StringBuilder AppendList(this StringBuilder builder, IEnumerable<string> list)
        {
            bool first = true;
            foreach (string item in list)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append("|");
                }
                builder.Append(item);
            }

            return builder;
        }

        public static StringBuilder AppendList(this StringBuilder builder, IEnumerable<int> list)
        {
            bool first = true;
            foreach (int item in list)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append("|");
                }
                builder.Append(item);
            }

            return builder;
        }

        public static Line ParseShortLine(string s)
        {
            int separatorIndex = s.IndexOf(':');

            if (separatorIndex != -1)
            {
                string code = s.Substring(0, separatorIndex);

                Line line = ModelCache.GetOrCreate(code, () => new Line(code));
                line.LineEnd = s.Substring(separatorIndex + 1);

                if (code[0] == '3') // local trains
                {
                    line.ShortName = RemoveLeadingNumbers(line.ShortName);
                }

                return line;
            }
            else
            {
                throw new ArgumentException("Invalid short line");
            }
        }

        private static Regex trimNumbersPattern = new Regex(@"\d*(?<letters>.*)");

        public static string RemoveLeadingNumbers(string s)
        {
            var match = trimNumbersPattern.Match(s);
            return match.Groups["letters"].Value;
        }

        public delegate T ItemParser<T>(string s);

        public static IEnumerable<T> ParseAsList<T>(string s, ItemParser<T> itemParser)
        {
            var substrings = s.Split('|');
            foreach (var substring in substrings)
            {
                yield return itemParser(substring);
            }
        }

        public static T[] ParseAsArray<T>(string s, ItemParser<T> itemParser)
        {
            var substrings = s.Split('|');
            var result = new T[substrings.Length];
            for (int i = 0; i < substrings.Length; ++i)
            {
                result[i] = itemParser(substrings[i]);
            }
            return result;
        }

        private static string[] _defaultLanguagePreference = { "en", "fi", "sv", "slangi" };

        public static IEnumerable<string> GetLanguagePreference()
        {
            string currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (_defaultLanguagePreference.Contains(currentLanguage))
            {
                yield return currentLanguage;
            }

            foreach (string language in _defaultLanguagePreference)
            {
                if (language != currentLanguage)
                {
                    yield return language;
                }
            }
        }

        public static string GetPreferredName(Dictionary<string, string> namesByLanguage)
        {
            foreach (string language in GetLanguagePreference())
            {
                string preferredName;
                if (namesByLanguage.TryGetValue(language, out preferredName))
                {
                    return preferredName;
                }
            }

            return null;
        }

        private static Regex _urlCleanPattern = new Regex(@"\\/");

        public static string CleanUrl(string input)
        {
            return WebUtility.UrlDecode(_urlCleanPattern.Replace(input, "/"));
        }

        public static bool AddStop(this List<ConnectedStops> list, Stop stop)
        {
            foreach (var stops in list)
            {
                if (stops.TryAdd(stop))
                {
                    return false;
                }
            }

            var newStops = new ConnectedStops(stop);
            list.Add(newStops);
            return true;
        }
    }
}
