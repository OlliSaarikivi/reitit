﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Reitit
{
    class LambdaConverter : IValueConverter
    {

    }

    static class Utils
    {
        //public static readonly double MapEpsilon = 0.000001;
        public static readonly double MapEpsilon = 0.1;

        private static int _jiggle_index = 0;
        public static GeoCoordinate Jiggle(this GeoCoordinate coordinate)
        {
            if (_jiggle_index < 3)
            {
                coordinate.Latitude += MapEpsilon;
            }
            else if (3 < _jiggle_index && _jiggle_index < 7)
            {
                coordinate.Latitude -= MapEpsilon;
            }
            if (1 < _jiggle_index && _jiggle_index < 5)
            {
                coordinate.Longitude -= MapEpsilon;
            }
            else if (_jiggle_index == 0 || _jiggle_index > 5)
            {
                coordinate.Longitude += MapEpsilon;
            }
            _jiggle_index = (_jiggle_index + 1) % 8;
            return coordinate;
        }

        public static GeoCoordinate Copy(this GeoCoordinate coordinate)
        {
            return new GeoCoordinate
            {
                Altitude = coordinate.Altitude,
                Course = coordinate.Course,
                HorizontalAccuracy = coordinate.HorizontalAccuracy,
                Latitude = coordinate.Latitude,
                Longitude = coordinate.Longitude,
                Speed = coordinate.Speed,
                VerticalAccuracy = coordinate.VerticalAccuracy,
            };
        }

        private static Random _displace_randomizer = new Random();
        public static GeoCoordinate DisplaceFrom(this GeoCoordinate coordinate, GeoCoordinate other)
        {
            if (Math.Abs(coordinate.Longitude - other.Longitude) < MapEpsilon)
            {
                coordinate.Longitude = other.Longitude + (_displace_randomizer.Next(2) == 0 ? MapEpsilon : -MapEpsilon);
            }
            return coordinate;
        }
    }
}