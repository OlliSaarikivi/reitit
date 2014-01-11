﻿using System;
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
using Windows.Devices.Geolocation;

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

        public static bool TryParse(string s, out ReittiCoordinate coordinate)
        {
            var substrings = s.Split(',');
            double latitude, longitude;
            if (substrings.Length == 2 &&
                double.TryParse(substrings[0], NumberStyles.Any, CultureInfo.InvariantCulture, out latitude) &&
                double.TryParse(substrings[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
            {
                coordinate = new ReittiCoordinate(latitude, longitude);
                return true;
            }
            coordinate = null;
            return false;
        }

        public static ReittiCoordinate Parse(string s)
        {
            var substrings = s.Split(',');
            if (substrings.Length == 2)
            {
                return new ReittiCoordinate(
                    double.Parse(substrings[1], CultureInfo.InvariantCulture),
                    double.Parse(substrings[0], CultureInfo.InvariantCulture));
            }
            throw new FormatException("Not enough parts");
        }

        public static implicit operator GeoCoordinate(ReittiCoordinate c)
        {
            return new GeoCoordinate(c.Latitude, c.Longitude);
        }

        public static explicit operator ReittiCoordinate(GeoCoordinate c)
        {
            return new ReittiCoordinate(c.Latitude, c.Longitude);
        }

        public static explicit operator ReittiCoordinate(Geocoordinate c)
        {
            return new ReittiCoordinate(c.Latitude, c.Longitude);
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
