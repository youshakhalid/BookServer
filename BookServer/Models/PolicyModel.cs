using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Server;
using Server.Models;

namespace BookServer.Models
{
    public class PolicyModel : ISerialize
    {
        public int MaxBurrowLimit { get; set; }
        public int BaseDue{ get; set; }
        public int IncrementAmount { get; set; }

        public List<T> DeserializeToList<T>()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "PolicyModel.xml");
            if (!File.Exists(path))
                return new List<T>();

            var xml = new XmlSerializer(typeof(List<PolicyModel>));

            using (FileStream stream = File.OpenRead(path))
            {
                return (List<T>)xml.Deserialize(stream);
            }
        }

        public string ListToSerialize<PolicyModel>(List<PolicyModel> list)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PolicyModel>));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, list);
                return writer.ToString();
            }
        }

        public void SaveList<T>(List<T> list)
        {

            string path = Path.Combine(Environment.CurrentDirectory, "PolicyModel.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PolicyModel>));
            FileStream stream = File.Create(path);
            using (stream)
            {
                xmlSerializer.Serialize(stream, list);
            }
        }
    }
}
