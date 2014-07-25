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
using Windows.Foundation;

namespace Reitit.API
{
    public static partial class Utils
    {
        // Copyright 2002, softSurfer (www.softsurfer.com)
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        // Assume that classes are already given for the objects:
        //    Point and Vector with
        //        coordinates {float x, y, z;}    // as many as are needed
        //        operators for:
        //            == to test equality
        //            != to test inequality
        //            (Vector)0 = (0,0,0)         (null vector)
        //            Point  = Point ± Vector
        //            Vector = Point - Point
        //            Vector = Vector ± Vector
        //            Vector = Scalar * Vector    (scalar product)
        //            Vector = Vector * Vector    (cross product)
        //    Segment with defining endpoints {Point P0, P1;}
        //===================================================================

        // dot product (3D) which allows vector operations in arguments
        //#define dot(u,v)   ((u).x * (v).x + (u).y * (v).y + (u).z * (v).z)
        //#define norm2(v)   dot(v,v)        // norm2 = squared length of vector
        //#define norm(v)    sqrt(norm2(v))  // norm = length of vector
        //#define d2(u,v)    norm2(u-v)      // distance squared = norm2 of difference
        //#define d(u,v)     norm(u-v)       // distance = norm of difference

        // poly_simplify():
        //    Input:  tol = approximation tolerance
        //            V[] = polyline array of vertex points 
        //            n   = the number of points in V[]
        //    Output: sV[]= simplified polyline vertices (max is n)
        //    Return: m   = the number of points in sV[]
        public static IEnumerable<ReittiCoordinate> PolySimplify(double tol, ReittiCoordinate[] V)
        {
            int n = V.Length;
            int i, k, pv;            // misc counters
            double tol2 = tol * tol;       // tolerance squared
            ReittiCoordinate[] vt = new ReittiCoordinate[n];      // vertex buffer
            bool[] mk = new bool[n];  // marker buffer

            // STAGE 1.  Vertex Reduction within tolerance of prior vertex cluster
            vt[0] = V[0];              // start at the beginning
            for (i = k = 1, pv = 0; i < n; i++)
            {
                double dLat = V[i].Latitude - V[pv].Latitude;
                double dLong = V[i].Longitude - V[pv].Longitude;
                double d = dLat * dLat + dLong * dLong;
                if (d < tol2)
                    continue;
                vt[k++] = V[i];
                pv = i;
            }
            if (pv < n - 1)
                vt[k++] = V[n - 1];      // finish at the end

            // STAGE 2.  Douglas-Peucker polyline simplification
            mk[0] = mk[k - 1] = true;       // mark the first and last vertices
            SimplifyDP(tol2, vt, 0, k - 1, mk);

            // copy marked vertices to the output simplified polyline
            for (i = 0; i < k; i++)
            {
                if (mk[i])
                    yield return vt[i];
            }
        }

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

        private class VWTri : IComparable
        {
            public Point P3 { get; set; }
            public Point P2 { get; set; }
            public Point P1 { get; set; }
            public int MiddleIndex { get; set; }
            public double Score { get; set; }
            public VWTri Previous { get; set; }
            public VWTri Next { get; set; }

            public VWTri(Point p1, Point p2, Point p3, int middleIndex)
            {
                P1 = p1;
                P2 = p2;
                P3 = p3;
                Score = Area();
                MiddleIndex = middleIndex;
            }

            public double Area()
            {
                return Math.Abs(-P2.X * P1.Y + P3.X * P1.Y + P1.X * P2.Y - P3.X * P2.Y - P1.X * P3.Y + P2.X * P3.Y);
            }

            public int CompareTo(object obj)
            {
                return Score.CompareTo((obj as VWTri).Score);
            }
        }

        public static IEnumerable<ReittiCoordinate> PolySimplifyVW(double tolerance, ReittiCoordinate[] coordinates)
        {
            int n = coordinates.Length;
            var remove = new bool[n];
            var points = new Point[n];
            for (int i = 0; i < n; ++i)
            {
                points[i] = coordinates[i].ToUniformPoint();
            }

            var triangles = new SortedSet<VWTri>();
            VWTri previous = null;
            for (int i = 1; i < n - 1; ++i)
            {
                var next = new VWTri(points[i - 1], points[i], points[i + 1], i);
                triangles.Add(next);
                if (previous != null)
                {
                    previous.Next = next;
                    next.Previous = previous;
                }
                previous = next;
            }

            Action<VWTri> update = t =>
            {
                triangles.Remove(t);
                t.Score = t.Area();
                triangles.Add(t);
            };

            while (triangles.Count > 0)
            {
                var tri = triangles.Min;
                if (tri.Score > tolerance)
                {
                    break;
                }
                triangles.Remove(tri);
                remove[tri.MiddleIndex] = true;
                if (tri.Previous != null)
                {
                    tri.Previous.Next = tri.Next;
                    tri.Previous.P3 = tri.P3;
                    update(tri.Previous);
                }
                if (tri.Next != null)
                {
                    tri.Next.Previous = tri.Previous;
                    tri.Next.P1 = tri.P1;
                    update(tri.Next);
                }
            }

            for (int i = 0; i < n; ++i)
            {
                if (!remove[i])
                {
                    yield return coordinates[i];
                }
            }
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


        public static void BubbleSort<T>(this ObservableCollection<T> o, IComparer<T> comparer)
        {
            for (int i = o.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    T o1 = o[j - 1];
                    T o2 = o[j];
                    if (comparer.Compare(o1, o2) > 0)
                    {
                        o.Move(j - 1, j);
                    }
                }
            }
        }
    }
}
