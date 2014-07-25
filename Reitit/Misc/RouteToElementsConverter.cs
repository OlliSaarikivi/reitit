using Reitit.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;

namespace Reitit
{
    class RouteToElementsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var route = value as CompoundRoute;
            if (route != null)
            {
                var elements = new List<MapElement>();

                foreach (var subRoute in route.Routes)
                {
                    foreach (var leg in subRoute.Legs)
                    {
                        var convertedLocs = from coordinate in leg.Shape
                                            select (BasicGeoposition)coordinate;
                        var polyline = new MapPolyline
                        {
                            StrokeColor = Utils.ColorForType(leg.Type),
                            StrokeThickness = 5,
                            StrokeDashed = leg.Type == "walk",
                            Path = new Geopath(convertedLocs),
                            ZIndex = 0,
                        };
                        elements.Add(polyline);
                        if (leg.Locs.Length > 0 && leg.Shape.Length > 0 && leg.Locs[0].Coord != leg.Shape[0])
                        {
                            var startPolyline = new MapPolyline
                            {
                                StrokeColor = Utils.ColorForType(leg.Type),
                                StrokeThickness = 5,
                                StrokeDashed = true,
                                Path = new Geopath(new BasicGeoposition[] { leg.Locs[0].Coord, leg.Shape[0] }),
                                ZIndex = 0,
                            };
                            elements.Add(startPolyline);
                        }
                        if (leg.Locs.Length > 0 && leg.Shape.Length > 0 && leg.Locs.LastElement().Coord != leg.Shape.LastElement())
                        {
                            var startPolyline = new MapPolyline
                            {
                                StrokeColor = Utils.ColorForType(leg.Type),
                                StrokeThickness = 5,
                                StrokeDashed = true,
                                Path = new Geopath(new BasicGeoposition[] { leg.Shape.LastElement(), leg.Locs.LastElement().Coord }),
                                ZIndex = 0,
                            };
                            elements.Add(startPolyline);
                        }

                        if (leg.Type != "walk")
                        {
                            foreach (var loc in leg.Locs)
                            {
                                var stopIcon = new MapIcon
                                {
                                    Location = loc.Coord,
                                    NormalizedAnchorPoint = new Point(0.5, 0.5),
                                    Title = loc.Name,
                                    Image = Utils.MapIconImageForType(leg.Type),
                                    ZIndex = 10,
                                };
                                elements.Add(stopIcon);
                            }
                        }
                    }
                }

                return elements;
            }
            else
            {
                return new MapElement[0];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
