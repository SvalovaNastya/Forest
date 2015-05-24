using System.Xml.Serialization;

namespace Server
{
    [XmlRoot(Namespace = "http://localhost", IsNullable = false)]
    public class Config
    {
        public string Filename;
        [XmlArrayAttribute("players")]
        public ConfigPoints[] Points;
    }

    public class ConfigPoints
    {
        public int StartPointX;
        public int StartPointY;
        public int TargetX;
        public int TargetY;
    }
}
