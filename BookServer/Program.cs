//Establishing and setting up Connection
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using BookServer.Models;
using Server;
using Server.Models;



internal class Program
{
    static List<TcpClient> clients = new List<TcpClient>(); // Store connected clients
    static int port = 42101;

    private static void Main(string[] args)
    {
        IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress iPAddress = iPHostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? IPAddress.Loopback;
        TcpListener listener = new TcpListener(iPAddress, port);
        TcpClient client;
       
        ISerialize serialize = new UserModel();
        ISerialize bookSerialize = new BookModel();
        ISerialize lendingSerialize = new LendingModel();
        ISerialize historySerialize = new LentHistoryModel();
        ISerialize policySerialize = new PolicyModel();

        List<UserModel> userList = serialize.DeserializeToList<UserModel>();
        List<BookModel> bookList = bookSerialize.DeserializeToList<BookModel>();
        List<PolicyModel> policyList = policySerialize.DeserializeToList<PolicyModel>();
        List<LentHistoryModel> historyList = historySerialize.DeserializeToList<LentHistoryModel>();
        List<LendingModel> lendingList = lendingSerialize.DeserializeToList<LendingModel>();

        UserModel user = new UserModel();
        BookModel book = new BookModel();
        LendingModel lend = new LendingModel();
        LentHistoryModel history = new LentHistoryModel();
        PolicyModel policy = new PolicyModel();


        try
        {
           
            listener.Start();
            Console.WriteLine("Server listening on port " + port);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                listener.Stop();
                Environment.Exit(0);
            };

            while (true)
            {
                client = listener.AcceptTcpClient();
                
                Console.WriteLine($"Client connected!");

                // Handle client in a separate task
                _ = Task.Run(() =>
             HandleClient(client, userList, bookList, lendingList, policyList, historyList,
                 serialize, bookSerialize, lendingSerialize, policySerialize, historySerialize));
            }
        }
        
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }



        static void HandleClient(TcpClient client, List<UserModel> userList, List<BookModel> bookList,
            List<LendingModel> lendingList, List<PolicyModel> policyList, List<LentHistoryModel> historyList,
            ISerialize serialize, ISerialize bookSerialize, ISerialize lendingSerialize, ISerialize policySerialize, ISerialize historySerialize)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("Client disconnected.");
                            break;
                        }

                        string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received request: {request}");

                        string responseXml = ProcessRequest(request, userList, bookList, lendingList, policyList, historyList, serialize, bookSerialize, lendingSerialize, policySerialize, historySerialize);

                        byte[] responseData = Encoding.UTF8.GetBytes(responseXml);
                        stream.Write(responseData, 0, responseData.Length);
                        stream.Flush();
                        Console.WriteLine($"Sent response to client.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client disconnected: " + ex.Message);
            }
            finally
            {
                
                client.Close();
            }
        }


        static string ProcessRequest(string request, List<UserModel> userList, List<BookModel> bookList,
            List<LendingModel> lendingList, List<PolicyModel> policyList, List<LentHistoryModel> historyList,
            ISerialize serialize, ISerialize bookSerialize, ISerialize lendingSerialize, ISerialize policySerialize,
            ISerialize historySerialize)
        {
            string responseXml = "";
           
                switch (request)
                {
                    case "LOGIN":
                    responseXml =  "RESPONSE_LOGIN|" + serialize.ListToSerialize(userList);
                    break;
                    case "BOOKS":
                    responseXml = "RESPONSE_BOOK|" + bookSerialize.ListToSerialize(bookList);
                    break;
                    case "ASSIGN":
                    responseXml = "RESPONSE_LEND|"+ lendingSerialize.ListToSerialize(lendingList);
                    break;
                    case "POLICY":
                    responseXml = "RESPONSE_POLICY|"+policySerialize.ListToSerialize(policyList);
                    break;
                    case "HISTORY":
                        responseXml = "RESPONSE_HISTORY|"+historySerialize.ListToSerialize(historyList);
                        break;
                    case "ACK":
                       
                        break;
                    case "PING":
                    return "PONG";
                default:
                    if (request.StartsWith("SAVE_USERS|"))
                    {
                        string xmlData = request.Substring("SAVE_USERS|".Length);
                        List<UserModel> updatedList = DeserializeFromXml<UserModel>(xmlData);
                        userList.Clear();
                        userList.AddRange(updatedList);
                        serialize.SaveList(userList);
                        responseXml = "ACK_USERS";

                    }
                    else if (request.StartsWith("SAVE_BOOKS|"))
                    {
                        string xmlData = request.Substring("SAVE_BOOKS|".Length);
                        List<BookModel> updatedList = DeserializeFromXml<BookModel>(xmlData);
                        bookList.Clear();
                        bookList.AddRange(updatedList);
                        bookSerialize.SaveList(bookList);
                        responseXml = "ACK_BOOK";

                    }
                    else if (request.StartsWith("SAVE_POLICY|"))
                    {
                        string xmlData = request.Substring("SAVE_POLICY|".Length);
                        List<PolicyModel> updatedList = DeserializeFromXml<PolicyModel>(xmlData);
                        policyList.Clear();
                        policyList.AddRange(updatedList);
                        policySerialize.SaveList(policyList);
                        responseXml = "ACK_POLICY";

                    }
                    else if (request.StartsWith("SAVE_LEND|"))
                    {
                        string xmlData = request.Substring("SAVE_LEND|".Length);
                        List<LendingModel> updatedList = DeserializeFromXml<LendingModel>(xmlData);
                        lendingList.Clear();
                        lendingList.AddRange(updatedList);
                        lendingSerialize.SaveList(lendingList);
                        responseXml = "ACK_LEND";

                    }
                    else if (request.StartsWith("SAVE_HISTORY|"))
                    {
                        string xmlData = request.Substring("SAVE_HISTORY|".Length);
                        List<LentHistoryModel> updatedList = DeserializeFromXml<LentHistoryModel>(xmlData);
                        historyList.Clear();
                        historyList.AddRange(updatedList);
                        historySerialize.SaveList(historyList);
                        responseXml = "ACK_HISTORY";

                    }
                    else if (request.StartsWith("ADD_USER|"))
                    {
                        string xmlData = request.Substring("ADD_USER|".Length);

                        UserModel addedUser;
                        XmlSerializer serializer = new XmlSerializer(typeof(UserModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            addedUser = (UserModel)serializer.Deserialize(reader);
                        }
                        if (addedUser != null)
                        {
                            userList.Add(addedUser);
                            serialize.SaveList(userList);
                        }



                        string addListXml = serialize.ListToSerialize(userList);
                        responseXml = "ADDED_LIST_USER|" + addListXml;
                    }
                    else if (request.StartsWith("ADD_BOOK|"))
                    {
                        string xmlData = request.Substring("ADD_BOOK|".Length);

                        BookModel addedBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(BookModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            addedBook = (BookModel)serializer.Deserialize(reader);
                        }
                        if (addedBook != null)
                        {
                            bookList.Add(addedBook);
                            bookSerialize.SaveList(bookList);
                        }



                        string addListXml = bookSerialize.ListToSerialize(bookList);
                        responseXml = "ADDED_LIST_BOOK|" + addListXml;
                    }

                    else if (request.StartsWith("ADD_LEND|"))
                    {
                        string xmlData = request.Substring("ADD_LEND|".Length);

                        LendingModel assignedBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(LendingModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            assignedBook = (LendingModel)serializer.Deserialize(reader);
                        }
                        if (assignedBook != null)
                        {
                            lendingList.Add(assignedBook);
                            lendingSerialize.SaveList(lendingList);
                        }



                        string addListXml = lendingSerialize.ListToSerialize(lendingList);
                        responseXml = "ADDED_LIST_LEND|" + addListXml;
                    }
                    else if (request.StartsWith("ADD_HISTORY|"))
                    {
                        string xmlData = request.Substring("ADD_HISTORY|".Length);

                        LentHistoryModel historyBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(LentHistoryModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            historyBook = (LentHistoryModel)serializer.Deserialize(reader);
                        }
                        if (historyBook != null)
                        {
                            historyList.Add(historyBook);
                            historySerialize.SaveList(historyList);
                        }



                        string addListXml = historySerialize.ListToSerialize(historyList);
                        responseXml = "ADDED_LIST_HISTORY|" + addListXml;
                    }

                    else if (request.StartsWith("UPDATE_USER|"))
                    {
                        string xmlData = request.Substring("UPDATE_USER|".Length);

                        UserModel updatedUser;
                        XmlSerializer serializer = new XmlSerializer(typeof(UserModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedUser = (UserModel)serializer.Deserialize(reader);
                        }

                        var userInList = userList.FirstOrDefault(u => u.Id == updatedUser.Id);
                        if (userInList != null)
                        {
                            int index = userList.IndexOf(userInList);
                            userList[index] = updatedUser;
                            serialize.SaveList(userList);
                        }

                        string updatedListXml = serialize.ListToSerialize(userList);
                        responseXml = "UPDATED_LIST_USER|" + updatedListXml;
                    }
                    else if (request.StartsWith("UPDATE_BOOK|"))
                    {
                        string xmlData = request.Substring("UPDATE_BOOK|".Length);

                        BookModel updatedBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(BookModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedBook = (BookModel)serializer.Deserialize(reader);
                        }

                        var bookInList = bookList.FirstOrDefault(u => u.Id == updatedBook.Id);
                        if (bookInList != null)
                        {
                            int index = bookList.IndexOf(bookInList);
                            bookList[index] = updatedBook;
                            bookSerialize.SaveList(bookList);
                        }
                        var existingBook = bookList.FirstOrDefault(b => b.Id == updatedBook.Id);
                        if (existingBook != null)
                        {
                            // Update the CopiesAvailable value
                            existingBook.CopiesAvailable = updatedBook.CopiesAvailable;

                            bookSerialize.SaveList(bookList); // Save to XML
                        }

                        string updatedListXml = bookSerialize.ListToSerialize(bookList);
                        responseXml = "UPDATED_LIST_BOOK|" + updatedListXml;
                    }
                    else if (request.StartsWith("UPDATE_LEND|"))
                    {
                        string xmlData = request.Substring("UPDATE_LEND|".Length);

                        LendingModel updatedLendUser;
                        XmlSerializer serializer = new XmlSerializer(typeof(LendingModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedLendUser = (LendingModel)serializer.Deserialize(reader);
                        }

                        var userInList = userList.FirstOrDefault(u => u.Id == updatedLendUser.UserId);
                        var bookInList = bookList.FirstOrDefault(u => u.Id == updatedLendUser.BookId);

                        if (userInList != null)
                        {
                            var lendingsToUpdate = lendingList.Where(l => l.UserId == userInList.Id).ToList();

                            foreach (var lend in lendingsToUpdate)
                            {
                                lend.UserName = userInList.Name;
                                lend.UserContact = userInList.ContactNo;
                            }

                            lendingSerialize.SaveList(lendingList);

                            string updatedXml = lendingSerialize.ListToSerialize(lendingList);
                            responseXml = "UPDATED_LIST_LEND|" + updatedXml;
                        }

                        if (bookInList != null)
                        {
                            var lendingsToUpdate = lendingList.Where(l => l.BookId == bookInList.Id).ToList();

                            foreach (var lend in lendingsToUpdate)
                            {
                                lend.BookName = bookInList.Title;
                            }

                            lendingSerialize.SaveList(lendingList);

                            string updatedXml = lendingSerialize.ListToSerialize(lendingList);
                            responseXml = "UPDATED_LIST_LEND|" + updatedXml;
                        }
                        else
                        {
                            responseXml = "<Error>Lending record or user not found</Error>";
                        }
                    }
                    else if (request.StartsWith("UPDATE_HISTORY|"))
                    {
                        string xmlData = request.Substring("UPDATE_HISTORY|".Length);

                        LentHistoryModel updatedHistory;
                        XmlSerializer serializer = new XmlSerializer(typeof(LentHistoryModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedHistory = (LentHistoryModel)serializer.Deserialize(reader);
                        }

                        var userInList = userList.FirstOrDefault(u => u.Id == updatedHistory.UserId);
                        var bookInList = bookList.FirstOrDefault(u => u.Id == updatedHistory.BookId);

                        if (userInList != null)
                        {
                            var HistoryUpdate = historyList.Where(l => l.UserId == userInList.Id).ToList();

                            foreach (var history in HistoryUpdate)
                            {
                                history.UserName = userInList.Name;
                                history.UserContact = userInList.ContactNo;
                            }

                            historySerialize.SaveList(historyList);

                            string updatedXml = historySerialize.ListToSerialize(historyList);
                            responseXml = "UPDATED_LIST_HISTORY|" + updatedXml;
                        }
                        if (bookInList != null)
                        {
                            var HistoryUpdate = historyList.Where(l => l.BookId == bookInList.Id).ToList();

                            foreach (var history in HistoryUpdate)
                            {
                                history.BookName = bookInList.Title;
                            }

                            historySerialize.SaveList(historyList);

                            string updatedXml = historySerialize.ListToSerialize(historyList);
                            responseXml = "UPDATED_LIST_HISTORY|" + updatedXml;
                        }
                        else
                        {
                            responseXml = "<Error>Lending record or user not found</Error>";
                        }
                    }
                    else if (request.StartsWith("UPDATE_POLICY|"))
                    {
                        string xmlData = request.Substring("UPDATE_POLICY|".Length);

                        PolicyModel updatedPolicy;
                        XmlSerializer serializer = new XmlSerializer(typeof(PolicyModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedPolicy = (PolicyModel)serializer.Deserialize(reader);
                        }

                        policyList.Clear();
                        if (updatedPolicy != null)
                        {
                            policyList.Add(updatedPolicy);

                            policySerialize.SaveList(policyList);
                        }

                        string updatedListXml = policySerialize.ListToSerialize(policyList);
                        responseXml = "UPDATED_LIST_POLICY|" + updatedListXml;
                    }
                    else if (request.StartsWith("DELETE_USER|"))
                    {
                        string xmlData = request.Substring("DELETE_USER|".Length);

                        UserModel deletedUser;
                        XmlSerializer serializer = new XmlSerializer(typeof(UserModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            deletedUser = (UserModel)serializer.Deserialize(reader);
                        }

                        var userInList = userList.FirstOrDefault(u => u.Id == deletedUser.Id);
                        if (userInList != null)
                        {
                            int index = userList.IndexOf(userInList);
                            userList.RemoveAt(index);
                            serialize.SaveList(userList);
                        }

                        string updatedListXml = serialize.ListToSerialize(userList);
                        responseXml = "DELETE_LIST_USER|" + updatedListXml;
                    }

                    else if (request.StartsWith("DELETE_BOOK|"))
                    {
                        string xmlData = request.Substring("DELETE_BOOK|".Length);

                        BookModel updatedBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(BookModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            updatedBook = (BookModel)serializer.Deserialize(reader);
                        }

                        var bookInList = bookList.FirstOrDefault(u => u.Id == updatedBook.Id);
                        if (bookInList != null)
                        {
                            int index = bookList.IndexOf(bookInList);
                            bookList.RemoveAt(index);
                            bookSerialize.SaveList(bookList);
                        }

                        string updatedListXml = bookSerialize.ListToSerialize(bookList);
                        responseXml = "DELETE_LIST_BOOK|" + updatedListXml;
                    }

                    else if (request.StartsWith("DELETE_LEND|"))
                    {
                        string xmlData = request.Substring("DELETE_LEND|".Length);

                        LendingModel assignedBook;
                        XmlSerializer serializer = new XmlSerializer(typeof(LendingModel));
                        using (StringReader reader = new StringReader(xmlData))
                        {
                            assignedBook = (LendingModel)serializer.Deserialize(reader);
                        }

                        var assignBookInList = lendingList.FirstOrDefault(u => u.BorrowDate == assignedBook.BorrowDate);

                        if (assignBookInList != null)
                        {
                            int index = lendingList.IndexOf(assignBookInList);
                            lendingList.RemoveAt(index);
                            lendingSerialize.SaveList(lendingList);
                        }

                        string updatedListXml = lendingSerialize.ListToSerialize(lendingList);
                        responseXml = "DELETE_LIST_LEND|" + updatedListXml;
                    }

                    break;

                }
                if (string.IsNullOrEmpty(responseXml))
                {
                    responseXml = "<Error>Invalid request</Error>";
                }
            
            return responseXml;

        }
    }

    

    static List<T> DeserializeFromXml<T>(string xmlData)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (StringReader textReader = new StringReader(xmlData))
            {
                return (List<T>)serializer.Deserialize(textReader);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Deserialization failed: " + ex.Message);
            return new List<T>(); // Return an empty list to avoid crashes
        }
    }

    

   
}