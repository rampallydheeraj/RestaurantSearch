using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace yumi
{
    public class LocationList
    {
        private List<TargetLocation> locations;
        private String Name;
        private int level;

        public LocationList(String nm)
        {
            locations = new List<TargetLocation>();
            Name = nm;
        }

        public String getName() { return Name; }        

        public void addLocation(int code, String loc)
        {
            if(!doesExits(loc, code))
                locations.Add(new TargetLocation(code, loc));
        }


        

        public List<String> getLocationsForSearchEngine(int code)
        {
            List<String> locs = new List<String>();
            foreach (TargetLocation t in locations)
            {
                if (t.belongsToSearchEngine(code))
                {
                    locs.Add(t.getName());
                }
            }

            return locs;
        }

        public String getNeighborhood(int index)
        {
            return locations[index].getName();
        }


        public int getSearchEngineCodeForMapping(int code)
        {
            return locations[code].getSearchEngineCode();
        }
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public Boolean doesExits(string loc, int code)
        {
            foreach (TargetLocation t in locations)
                if (t.getName().Equals(loc) && t.getSearchEngineCode() == code)
                    return true;

            return false;
        }

        public int getLocationsCount()
        {
            return locations.Count();
        }
    }

    [XmlRoot("dictionary")]

    public class SerializableDictionary<TKey, TValue>

        : Dictionary<TKey, TValue>, IXmlSerializable
    {

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {

            return null;
        }



        public void ReadXml(System.Xml.XmlReader reader)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion

    }
}
