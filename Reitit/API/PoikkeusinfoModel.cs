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
        [DataMember]
        public string Info;
        [DataMember]
        public List<AdvanceDisruption> AdvanceDisruptions;
        [DataMember]
        public List<SuddenDisruption> SuddenDisruptions;

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
        [DataMember]
        public int Id;
        [DataMember]
        public string Info;
        [DataMember]
        public Validity Validity;
    }

    [DataContract]
    public class AdvanceDisruption : Disruption
    {
        [DataMember]
        public List<int> TransportTypeIds;

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
        [DataMember]
        public List<Line> Lines;

        public static SuddenDisruption Parse(XElement ele)
        {
            var lines = from targets in ele.Descendants("TARGETS")
                        from lineEle in targets.Descendants("LINE")
                        let code = lineEle.Attribute("id").Value
                        select ModelCache.GetOrCreate(code, () => new Line(code));
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
        [DataMember]
        public DateTime From;
        [DataMember]
        public DateTime To;

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
