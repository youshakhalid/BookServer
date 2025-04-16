using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Server;
using Server.Models;

namespace BookServer.Models
{
    public class LentHistoryModel : ISerialize
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int UserContact { get; set; }
        public int Dues { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.Now;
    

     public List<T> DeserializeToList<T>()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "HistoryModel.xml");
            if (!File.Exists(path))
                return new List<T>();

            var xml = new XmlSerializer(typeof(List<LentHistoryModel>));

            using (FileStream stream = File.OpenRead(path))
            {
                return (List<T>)xml.Deserialize(stream);
            }
        }

        public string ListToSerialize<LentHistoryModel>(List<LentHistoryModel> obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<LentHistoryModel>));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public void SaveList<T>(List<T> list)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "HistoryModel.xml");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<LentHistoryModel>));
            FileStream stream = File.Create(path);
            using (stream)
            {
                xmlSerializer.Serialize(stream, list);
            }
        }
    }
}
