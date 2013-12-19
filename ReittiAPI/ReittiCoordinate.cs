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
using System.Device.Location;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace ReittiAPI
{

    [DataContract]
    public class ReittiCoordinate
    {
        public ReittiCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public ReittiCoordinate(GeoCoordinate coordinate)
            : this(coordinate.Latitude, coordinate.Longitude)
        {
        }

        public ReittiCoordinate(string s)
        {
            var substrings = s.Split(',');
            if (substrings.Length == 2)
            {
                try
                {
                    Longitude = double.Parse(substrings[0], CultureInfo.InvariantCulture);
                    Latitude = double.Parse(substrings[1], CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid coordinate string: " + s);
                }
            }
            else
            {
                throw new ArgumentException("Invalid coordinate string: " + s);
            }
        }

        public GeoCoordinate AsGeoCoordinate()
        {
            return new GeoCoordinate(Latitude, Longitude);
        }


        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Longitude.ToString("F10", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(Latitude.ToString("F10", CultureInfo.InvariantCulture));

            return builder.ToString();
        }

        [DataMember]
        public double Latitude;

        [DataMember]
        public double Longitude;
    }
}
