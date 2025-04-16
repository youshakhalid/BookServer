using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    public interface ISerialize
    {

        public List<T> DeserializeToList<T>(string xml)
        {

            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML data is null or empty.");

            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            try
            {
                using (StringReader reader = new StringReader(xml))
                {
                    return (List<T>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                return new List<T>(); // Return an empty list in case of failure
            }
        }
        public List<T> DeserializeToList<T>();
        
        public string ListToSerialize<T>(List<T> list);
       
        public void SaveList<T>(List<T> list);

        public string ObjectSerialize<T>(T model)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, model);
                return writer.ToString();
            }
        }
        
    }
}
