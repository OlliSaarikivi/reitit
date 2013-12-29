using System;
using System.Text;
using System.Device.Location;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Threading;

namespace ReittiAPI
{
    public enum ReittiAPIExceptionKind
    {
        ParseException,
        HttpRequestException
    }

    public class ReittiAPIException : Exception
    {
        public ReittiAPIException(ReittiAPIExceptionKind kind, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            Kind = kind;
        }

        public ReittiAPIExceptionKind Kind { get; set; }
    }

    public class GeocodeResults
    {
        public GeocodeResults()
        {
            Pois = new List<Poi>();
            ConnectedStopsList = new List<ConnectedStops>();
            Addresses = new List<Address>();
            Streets = new List<Location>();
            OtherAddresses = new List<Location>();
            Others = new List<Location>();
        }

        public List<Poi> Pois { get; set; }
        public List<ConnectedStops> ConnectedStopsList { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Location> Streets { get; set; }
        public List<Location> OtherAddresses { get; set; }
        public List<Location> Others { get; set; }

        public IEnumerable<Location> GetAllLocations()
        {
            foreach (var poi in Pois)
                yield return poi;
            foreach (var address in Addresses)
                yield return address;
            foreach (var street in Streets)
                yield return street;
            foreach (var otherAddress in OtherAddresses)
                yield return otherAddress;
            foreach (var other in Others)
                yield return other;
        }
    }

    public enum LocationLanguage
    {
        Fi,
        Sv,
    }

    public class ReittiAPIClient
    {
        private const string CoordinateSystems = "&epsg_out=4326&epsg_in=4326";
        private readonly Uri BaseUri = new Uri("http://api.reittiopas.fi/hsl/1_1_3/");

        private HttpClient _client;

        public string User { private get; set; }
        public string Pass { private get; set; }

        public LocationLanguage LocationLanguage { get; set; }

        public ReittiAPIClient(string user, string pass)
        {
            User = user;
            Pass = pass;
            LocationLanguage = ReittiAPI.LocationLanguage.Fi;
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            _client = new HttpClient(handler);
        }

        private string GetAddressLang()
        {
            switch (LocationLanguage)
            {
                case LocationLanguage.Fi:
                default:
                    return "fi";
                case LocationLanguage.Sv:
                    return "sv";
            }
        }

        private string GetPoiLang()
        {
            switch (LocationLanguage)
            {
                case LocationLanguage.Fi:
                default:
                    return "fi";
                case LocationLanguage.Sv:
                    return "sv";
            }
        }

        private StringBuilder GetCommonParameters(string addressLang, string poiLang, bool poiSpecific)
        {
            var builder = new StringBuilder();

            builder.Append("user=").Append(User);
            builder.Append("&pass=").Append(Pass);
            if (poiSpecific)
            {
                builder.Append("&lang=").Append(poiLang);
            }
            else
            {
                builder.Append("&lang=").Append(addressLang);
            }
            builder.Append(CoordinateSystems);

            return builder;
        }

        public async Task<GeocodeResults> GeocodeAsync(
            string key,
            IEnumerable<string> cities = null,
            IEnumerable<string> locTypes = null,
            bool? disableErrorCorrection = null,
            bool? disableUniqueStopNames = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=geocode");
            query.Append("&key=").Append(key);
            if (cities != null)
            {
                query.Append("&cities=").AppendList(cities);
            }
            if (locTypes != null)
            {
                query.Append("&loc_types=").AppendList(locTypes);
            }
            if (disableErrorCorrection != null)
            {
                query.Append("&disable_error_correction=").Append(disableErrorCorrection.Value ? 1 : 0);
            }
            if (disableUniqueStopNames != null)
            {
                query.Append("&disable_unique_stop_names=").Append(disableUniqueStopNames.Value ? 1 : 0);
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }
                
                GeocodeResults result = new GeocodeResults();
                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    foreach (var token in json)
                    {
                        ParseGeocodeLocation(token, addressLang, result);
                    }
                }
                return result;
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }

        public async Task<GeocodeResults> ReverseGeocodeAsync(
            ReittiCoordinate coordinate,
            int? limit = null,
            int? radius = null,
            IEnumerable<string> resultContains = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=reverse_geocode");
            query.Append("&coordinate=").Append(coordinate);
            if (limit != null)
            {
                if (limit > 0)
                {
                    query.Append("&limit=").Append(limit);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("limit");
                }
            }
            if (radius != null)
            {
                if (1 <= radius && radius <= 1000)
                {
                    query.Append("&radius=").Append(radius);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("radius");
                }
            }
            if (resultContains != null)
            {
                query.Append("&result_contains=").AppendList(resultContains);
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }

                GeocodeResults result = new GeocodeResults();
                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    foreach (var token in json)
                    {
                        ParseGeocodeLocation(token, addressLang, result);
                    }
                }
                return result;
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }

        private static void ParseGeocodeLocation(JToken token, string responseLang, GeocodeResults eventArgs)
        {
            int locTypeId = token.Value<int>("locTypeId");
            if (locTypeId < 10 || locTypeId == 1008)
            {
                var poi = new Poi();
                eventArgs.Pois.Add(poi);
                poi.UpdateFromGeoResponse(token, responseLang);
            }
            else if (locTypeId == 10)
            {
                string code = token["details"].Value<string>("code");
                var stop = ModelCache.GetOrCreate(code, () => new Stop(code));
                stop.UpdateFromGeoResponse(token, responseLang);

                eventArgs.ConnectedStopsList.AddStop(stop);
            }
            else if (locTypeId == 900)
            {
                string locType = token.Value<string>("locType");
                if (locType == "address")
                {
                    var address = new Address();
                    eventArgs.Addresses.Add(address);
                    address.UpdateFromGeoResponse(token, responseLang);
                }
                else if (locType == "street")
                {
                    var street = new Location();
                    eventArgs.Streets.Add(street);
                    street.UpdateFromGeoResponse(token, responseLang);
                }
                else
                {
                    var location = new Location();
                    eventArgs.OtherAddresses.Add(location);
                    location.UpdateFromGeoResponse(token, responseLang);
                }
            }
            else
            {
                var location = new Location();
                eventArgs.Others.Add(location);
                location.UpdateFromGeoResponse(token, responseLang);
            }
        }

        public async Task<Stop> StopInformationAsync(
            string code,
            DateTime? dateTime = null,
            TimeSpan? timeLimit = null,
            int? depLimit = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=stop");
            query.Append("&code=").Append(code);

            if (dateTime == null)
            {
                dateTime = DateTime.Now;
            }
            var value = dateTime.Value;
            string date = value.ToString("yyyyMMdd");
            string time = value.ToString("HHmm");

            query.Append("&date=").Append(date);
            query.Append("&time=").Append(time);

            if (timeLimit != null)
            {
                int minutes = (int)timeLimit.Value.TotalMinutes;
                if (0 <= minutes && minutes <= 360)
                {
                    query.Append("&time_limit=").Append(minutes);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("timeLimit");
                }
            }
            if (depLimit != null)
            {
                if (1 <= depLimit && depLimit <= 20)
                {
                    query.Append("&dep_limit=").Append(depLimit);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("depLimit");
                }
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }

                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    if (json.Count > 0)
                    {
                        JToken source = json[0];

                        string receivedCode = source.Value<string>("code");
                        Stop stop = ModelCache.GetOrCreate(receivedCode, () => new Stop(receivedCode));
                        stop.UpdateFromStopResponse(source);
                        return stop;
                    }
                }
                return null; // TODO: is this an error?
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }

        public async Task<IEnumerable<Stop>> StopsInAreaAsyc(
            ReittiCoordinate centerCoordinate,
            int? limit = null,
            int? diameter = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=stops_area");
            query.Append("&center_coordinate=").Append(centerCoordinate);
            if (limit != null)
            {
                if (limit > 0)
                {
                    query.Append("&limit=").Append(limit);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("limit");
                }
            }
            if (diameter != null)
            {
                if (0 < diameter && diameter <= 5000)
                {
                    query.Append("&diameter=").Append(diameter);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("diameter");
                }
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }

                var stopsList = new List<Stop>();

                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    foreach (var token in json)
                    {
                        string receivedCode = token.Value<string>("code");
                        Stop stop = ModelCache.GetOrCreate(receivedCode, () => new Stop(receivedCode));
                        stop.UpdateFromStopsInAreaResponse(token, addressLang);

                        stopsList.Add(stop);
                    }
                }
                return stopsList;
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }

        public async Task<IEnumerable<ConnectedLines>> LineInformationAsync(
            IEnumerable<string> queryStrings,
            IEnumerable<int> transportType = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=lines");
            query.Append("&query=").AppendList(queryStrings);
            if (transportType != null)
            {
                query.Append("&transport_type=").AppendList(transportType);
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }

                var connectedLinesByCode = new Dictionary<string, ConnectedLines>();
                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    foreach (var token in json)
                    {
                        string code = token.Value<string>("code");

                        Line line = ModelCache.GetOrCreate<Line>(code, () => new Line(code));
                        line.UpdateFromLineResponse(token, addressLang);

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

                        connectedLines.Lines.Insert(0, line);
                    }
                }

                foreach (var lines in connectedLinesByCode.Values)
                {
                    lines.Lines.Sort((l1, l2) =>
                    {
                        if (l1.LineStops != null)
                        {
                            if (l2.LineStops != null)
                            {
                                if (l1.LineStops.Length > l2.LineStops.Length)
                                {
                                    return -1;
                                }
                                else if (l1.LineStops.Length == l2.LineStops.Length)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            if (l2.LineStops != null)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    });
                }
                return connectedLinesByCode.Values;
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }

        public async Task<List<CompoundRoute>> RouteAsync(
            ReittiCoordinate from,
            ReittiCoordinate to,
            ReittiCoordinate via = null,
            TimeSpan? viaTime = null,
            DateTime? dateTime = null,
            string timetype = null,
            string zone = null,
            IEnumerable<string> transportTypes = null,
            IDictionary<int, double> modeCostsByTransportTypeId = null,
            string optimize = null,
            int? changeMargin = null,
            int? changeCost = null,
            int? waitCost = null,
            int? walkCost = null,
            int? walkSpeed = null,
            string detail = null,
            int? show = null,
            CancellationToken? cancellationToken = null)
        {
            string addressLang = GetAddressLang();
            string poiLang = GetPoiLang();

            var query = GetCommonParameters(addressLang, poiLang, poiSpecific: false);
            query.Append("&request=route");

            query.Append("&from=").Append(from);

            query.Append("&to=").Append(to);

            if (via != null)
            {
                query.Append("&via=").Append(via);
            }

            if (viaTime != null)
            {
                query.Append("&via_time=").Append((int)Math.Ceiling(viaTime.Value.TotalMinutes));
            }

            if (dateTime != null)
            {
                query.Append("&date=").Append(dateTime.Value.ToString("yyyyMMdd"));
                query.Append("&time=").Append(dateTime.Value.ToString("HHmm"));
            }

            if (timetype != null)
            {
                query.Append("&timetype=").Append(timetype);
            }

            if (zone != null)
            {
                query.Append("&zone=").Append(zone);
            }

            if (transportTypes != null)
            {
                query.Append("&transport_types=").AppendList(transportTypes);
            }

            if (modeCostsByTransportTypeId != null)
            {
                foreach (var entry in modeCostsByTransportTypeId)
                {
                    query.Append("&mode_cost_").Append(entry.Key).Append('=');
                    if (entry.Value < 0)
                    {
                        query.Append("-1");
                    }
                    else
                    {
                        query.Append(entry.Value.ToString("0.0"));
                    }
                }
            }

            if (optimize != null)
            {
                query.Append("&optimize=").Append(optimize);
            }

            if (changeMargin != null)
            {
                query.Append("&change_margin=").Append(changeMargin);
            }

            if (changeCost != null)
            {
                query.Append("&change_cost=").Append(changeCost);
            }

            if (waitCost != null)
            {
                query.Append("&wait_cost=").Append(waitCost);
            }

            if (walkCost != null)
            {
                query.Append("&walk_cost=").Append(walkCost);
            }

            if (walkSpeed != null)
            {
                query.Append("&walk_speed=").Append(walkSpeed);
            }

            if (detail != null)
            {
                query.Append("&detail=").Append(detail);
            }

            if (show != null)
            {
                query.Append("&show=").Append(show);
            }

            var uriBuilder = new UriBuilder(BaseUri);
            uriBuilder.Query = query.ToString();

            try
            {
                string response;
                if (cancellationToken.HasValue)
                {
                    response = await (await _client.GetAsync(uriBuilder.Uri, cancellationToken.Value)).Content.ReadAsStringAsync();
                    cancellationToken.Value.ThrowIfCancellationRequested();
                }
                else
                {
                    response = await _client.GetStringAsync(uriBuilder.Uri);
                }

                var routeResults = new List<CompoundRoute>();
                if (response.Trim().Length != 0)
                {
                    JArray json = JArray.Parse(response);

                    foreach (var token in json)
                    {
                        var routeArray = token as JArray;
                        if (routeArray != null)
                        {
                            var compoundRoute = new CompoundRoute { Routes = new Route[routeArray.Count] };
                            for (int i = 0; i < compoundRoute.Routes.Length; ++i)
                            {
                                compoundRoute.Routes[i] = new Route(routeArray[i]);
                            }
                            routeResults.Add(compoundRoute);
                        }
                    }
                    routeResults.Sort((r1, r2) =>
                    {
                        return r1.Routes[0].Legs[0].Locs[0].DepTime.CompareTo(r2.Routes[0].Legs[0].Locs[0].DepTime);
                    });
                }
                return routeResults;
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (JsonReaderException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }
    }
}
