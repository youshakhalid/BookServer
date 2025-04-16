using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Models 
{
   public class BookModel : ISerialize
    {
        public int Id { get; set; }
        public string IBAN { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int CopiesAvailable { get; set; }

        public List<T> DeserializeToList<T>()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "BookModel.xml");
            if (!File.Exists(path))
                return new List<T>(); // Return an empty list if the file doesn't exist

            var xml = new XmlSerializer(typeof(List<T>));

            using (FileStream stream = File.OpenRead(path))
            {
                return (List<T>)xml.Deserialize(stream);
            }
        }

        public string ListToSerialize<BookModel>(List<BookModel> obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<BookModel>));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public void SaveList<T>(List<T> list)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "BookModel.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<BookModel>));
            FileStream stream = File.Create(path);
            using (stream)
            {
                xmlSerializer.Serialize(stream, list);
            }
        }
    }
}
