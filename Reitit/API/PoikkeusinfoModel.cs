using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Reitit.API
{
    static class PoikkeusinfoModelUtils
    {
        public static string ParseInfoText(this XElement ele)
        {
            return (from info in ele.Descendants("INFO")
                    from text in info.Descendants("TEXT")
                    select text.Value).First();
        }
    }

    [DataContract]
    public class Disruptions
    {
        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }
        [DataMember]
        public string _info;
        public List<AdvanceDisruption> AdvanceDisruptions
        {
            get { return _advanceDisruptions; }
            set { _advanceDisruptions = value; }
        }
        [DataMember]
        public List<AdvanceDisruption> _advanceDisruptions;
        public List<SuddenDisruption> SuddenDisruptions
        {
            get { return _suddenDisruptions; }
            set { _suddenDisruptions = value; }
        }
        [DataMember]
        public List<SuddenDisruption> _suddenDisruptions;

        public static Disruptions Parse(XElement ele)
        {
            var infoText = ele.ParseInfoText();
            var advanceDisruptions =
                from disruption in ele.Descendants("DISRUPTION")
                where disruption.Attribute("type").Value == "1"
                select AdvanceDisruption.Parse(disruption);
            var suddenDisruptions =
                from disruption in ele.Descendants("DISRUPTION")
                where disruption.Attribute("type").Value == "2"
                select SuddenDisruption.Parse(disruption);
            return new Disruptions
            {
                Info = infoText,
                AdvanceDisruptions = new List<AdvanceDisruption>(advanceDisruptions),
                SuddenDisruptions = new List<SuddenDisruption>(suddenDisruptions),
            };
        }
    }

    [DataContract]
    public abstract class Disruption
    {
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        [DataMember]
        public int _id;
        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }
        [DataMember]
        public string _info;
        public Validity Validity
        {
            get { return _validity; }
            set { _validity = value; }
        }
        [DataMember]
        public Validity _validity;
    }

    [DataContract]
    public class AdvanceDisruption : Disruption
    {
        public List<int> TransportTypeIds
        {
            get { return _transportTypeIds; }
            set { _transportTypeIds = value; }
        }
        [DataMember]
        public List<int> _transportTypeIds;

        public static AdvanceDisruption Parse(XElement ele)
        {
            var typeIds = from targets in ele.Descendants("TARGETS")
                          from linetype in targets.Descendants("LINETYPE")
                          select int.Parse(linetype.Attribute("id").Value);
            return new AdvanceDisruption
            {
                Id = int.Parse(ele.Attribute("id").Value),
                Info = ele.ParseInfoText(),
                Validity = (from validity in ele.Descendants("VALIDITY")
                            select Validity.Parse(validity)).First(),
                TransportTypeIds = new List<int>(typeIds),
            };
        }
    }

    [DataContract]
    public class SuddenDisruption : Disruption
    {
        public List<Line> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }
        [DataMember]
        public List<Line> _lines;

        public static SuddenDisruption Parse(XElement ele)
        {
            var lines = from targets in ele.Descendants("TARGETS")
                        from lineEle in targets.Descendants("LINE")
                        let name = lineEle.Attribute("id").Value
                        select ModelCache.GetOrCreate(name, () => new Line(null, name));
            return new SuddenDisruption
            {
                Id = int.Parse(ele.Attribute("id").Value),
                Info = ele.ParseInfoText(),
                Validity = (from validity in ele.Descendants("VALIDITY")
                            select Validity.Parse(validity)).First(),
                Lines = new List<Line>(lines),
            };
        }
    }

    [DataContract]
    public class Validity
    {
        public DateTime From
        {
            get { return _from; }
            set { _from = value; }
        }
        [DataMember]
        public DateTime _from;
        public DateTime To
        {
            get { return _to; }
            set { _to = value; }
        }
        [DataMember]
        public DateTime _to;

        public static Validity Parse(XElement ele)
        {
            return new Validity
            {
                From = DateTime.Parse(ele.Attribute("from").Value),
                To = DateTime.Parse(ele.Attribute("to").Value),
            };
        }
    }
}
