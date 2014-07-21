using System;
using System.Net;
using System.Windows;
using System.Runtime.Serialization;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace Reitit.API
{

    [DataContract]
    public class ReittiBoundingBox
    {
        [DataMember]
        public double East;
        [DataMember]
        public double North;
        [DataMember]
        public double West;
        [DataMember]
        public double South;

        public ReittiBoundingBox(double east, double north, double west, double south)
        {
            East = east;
            North = north;
            West = west;
            South = south;
        }
    }

    [DataContract]
    public class ReittiCoordinate
    {
        public ReittiCoordinate() { }

        public ReittiCoordinate(double latitude, double longitude, double? altitude = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public static bool TryParse(string s, out ReittiCoordinate coordinate)
        {
            var substrings = s.Split(',');
            double latitude, longitude, altitude;
            if (substrings.Length == 2 &&
                double.TryParse(substrings[0], NumberStyles.Any, CultureInfo.InvariantCulture, out latitude) &&
                double.TryParse(substrings[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
            {
                coordinate = new ReittiCoordinate(latitude, longitude);
                return true;
            }
            else if (substrings.Length == 3 &&
                double.TryParse(substrings[0], NumberStyles.Any, CultureInfo.InvariantCulture, out latitude) &&
                double.TryParse(substrings[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longitude) &&
                double.TryParse(substrings[2], NumberStyles.Any, CultureInfo.InvariantCulture, out altitude))
            {

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
            else if (substrings.Length == 3)
            {
                return new ReittiCoordinate(
                    double.Parse(substrings[0], CultureInfo.InvariantCulture),
                    double.Parse(substrings[1], CultureInfo.InvariantCulture),
                    double.Parse(substrings[2], CultureInfo.InvariantCulture));
            }
            throw new FormatException("Not enough parts");
        }

        public static implicit operator BasicGeoposition(ReittiCoordinate c)
        {
            var position = new BasicGeoposition
            {
                Latitude = c.Latitude,
                Longitude = c.Longitude,
            };
            if (c.Altitude.HasValue)
            {
                position.Altitude = c.Altitude.Value;
            }
            return position;
        }

        public static implicit operator Geopoint(ReittiCoordinate c)
        {
            return new Geopoint(c);
        }

        public static explicit operator ReittiCoordinate(BasicGeoposition c)
        {
            return new ReittiCoordinate(c.Latitude, c.Longitude, c.Altitude);
        }

        public static explicit operator ReittiCoordinate(Geopoint c)
        {
            return (ReittiCoordinate)c.Position;
        }

        public static explicit operator ReittiCoordinate(Geocoordinate c)
        {
            return (ReittiCoordinate)c.Point;
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

        [DataMember]
        public double? Altitude;
    }
}
