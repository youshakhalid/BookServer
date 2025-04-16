using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Models
{
    public class UserModel : ISerialize
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } 
        public string Password { get; set; }
        public int ContactNo { get; set; }

        public List<T> DeserializeToList<T>()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "UserModel.xml");
            if (!File.Exists(path))
                return new List<T>(); // Return an empty list if the file doesn't exist

            var xml = new XmlSerializer(typeof(List<UserModel>));

            using (FileStream stream = File.OpenRead(path))
            {
                return (List<T>)xml.Deserialize(stream);
            }
        }

        public string ListToSerialize<UserModel>(List<UserModel> obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<UserModel>));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public void SaveList<T>(List<T> list)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "UserModel.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserModel>));
            FileStream stream = File.Create(path);
            using (stream)
            {
                xmlSerializer.Serialize(stream, list);
            }
        }

       
    }
}
