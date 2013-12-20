using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ReittiAPI
{
    public enum PoikkeusinfoLanguage
    {
        Fi, En, Se
    }

    public class PoikkeusinfoAPIClient
    {
        private const string BaseUri = "http://www.poikkeusinfo.fi/xml/v2/";

        private HttpClient _client;

        public PoikkeusinfoLanguage Language { get; set; }

        public PoikkeusinfoAPIClient()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            _client = new HttpClient(handler);
        }

        public async Task<Disruptions> GetAsync()
        {
            string languageString;
            switch (Language)
            {
                case PoikkeusinfoLanguage.Se:
                    languageString = "se";
                    break;
                case PoikkeusinfoLanguage.Fi:
                    languageString = "fi";
                    break;
                case PoikkeusinfoLanguage.En:
                    languageString = "en";
                    break;
                default:
                    languageString = "en";
                    break;

            }
            Uri localizedUri = new Uri(BaseUri + languageString);

            try
            {
                string response = await _client.GetStringAsync(localizedUri);
                var doc = XDocument.Parse(response);
                try
                {
                    return Disruptions.Parse(doc.Root);
                }
                catch (Exception e)
                {
                    throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
                }
            }
            catch (HttpRequestException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.HttpRequestException, e.Message, e);
            }
            catch (XmlException e)
            {
                throw new ReittiAPIException(ReittiAPIExceptionKind.ParseException, "Could not parse server response. Please contact the app creator.", e);
            }
        }
    }
}
