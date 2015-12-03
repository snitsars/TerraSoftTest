using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace TestApi
{
    class Program
    {
        // Строка адреса BPMonline сервиса OData.
        private const string serverUri = "https://serhiisnitsarenko.bpmonline.com/0/ServiceModel/EntityDataService.svc/";
        private const string authServiceUtri = "https://serhiisnitsarenko.bpmonline.com/ServiceModel/AuthService.svc/Login";

        // Ссылки на пространства имен XML.
        private static readonly XNamespace ds = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace dsmd = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private static readonly XNamespace atom = "http://www.w3.org/2005/Atom";

        private static void Autorization(string userName, string userPassword, out CookieContainer bpmCookieContainer)
        {
            // Создание запроса на аутентификацию.
            var authRequest = HttpWebRequest.Create(authServiceUtri) as HttpWebRequest;
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";
            bpmCookieContainer = new CookieContainer();
            // Включение использования cookie в запросе.
            authRequest.CookieContainer = bpmCookieContainer;
            // Получение потока, ассоциированного с запросом на аутентификацию.
            using (var requesrStream = authRequest.GetRequestStream())
            {
                // Запись в поток запроса учетных данных пользователя BPMonline и дополнительных параметров запроса.
                using (var writer = new StreamWriter(requesrStream))
                {
                    writer.Write(@"{
                                ""UserName"":""" + userName + @""",
                                ""UserPassword"":""" + userPassword + @""",
                                ""SolutionName"":""TSBpm"",
                                ""TimeZoneOffset"":-120,
                                ""Language"":""Ru-ru""
                                }");
                }
            }
            using (var response = (HttpWebResponse) authRequest.GetResponse())
            {
            }
        }

        public static void GetOdataCollectionByAuthByHttpExample(string userName, string userPassword)
        {
            CookieContainer bpmCookieContainer;
            Autorization(userName, userPassword, out bpmCookieContainer);
            // Получение ответа от сервера. Если аутентификация проходит успешно, в объекте bpmCookieContainer будут 
            // помещены cookie, которые могут быть использованы для последующих запросов.
            

                // Создание запроса на получение данных от сервиса OData.
                //var dataRequest = HttpWebRequest.Create(serverUri + "ContactCollection?select=Id, Name")
                var dataRequest = HttpWebRequest.Create(serverUri + "CountryCollection?select")
                    as HttpWebRequest;
                // Для получения данных используется HTTP-метод GET.
                dataRequest.Method = "GET";
                // Добавление полученных ранее аутентификационных cookie в запрос на получение данных.
                dataRequest.CookieContainer = bpmCookieContainer;
                // Получение ответа от сервера.
                using (var dataResponse = (HttpWebResponse)dataRequest.GetResponse())
                {
                    // Загрузка ответа сервера в xml-документ для дальнейшей обработки.
                    XDocument xmlDoc = XDocument.Load(dataResponse.GetResponseStream());
                    // Получение коллекции объектов контактов, соответствующих условию запроса.
                    var contacts = from entry in xmlDoc.Descendants(atom + "entry")
                                   select new
                                   {
                                       Id = new Guid(entry.Element(atom + "content")
                                           .Element(dsmd + "properties")
                                           .Element(ds + "Id").Value),
                                       Name = entry.Element(atom + "content")
                                           .Element(dsmd + "properties")
                                           .Element(ds + "Name").Value
                                   };
                    foreach (var contact in contacts)
                    {
                        Console.WriteLine(contact.Id + " " + contact.Name);

                    }
                }
        }

        // Строка запроса:
        // POST <Адрес приложения BPMonline>/0/ServiceModel/EntityDataService.svc/ContactCollection/

        public static void CreateBpmEntityByOdataHttpExample(string userName, string userPassword)
        {
            CookieContainer bpmCookieContainer;
            Autorization(userName, userPassword, out bpmCookieContainer);

            // Создание сообщения xml, содержащего данные о создаваемом объекте.
            var content = new XElement(dsmd + "properties",
                          new XElement(ds + "Name", "USA"));
            var entry = new XElement(atom + "entry",
                        new XElement(atom + "content",
                        new XAttribute("type", "application/xml"), content));
            Console.WriteLine("=============================");
            Console.WriteLine(entry.ToString());
            Console.WriteLine("=============================");
            // Создание запроса к сервису, который будет добавлять новый объект в коллекцию контактов.
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUri + "CountryCollection/");
            //request.Credentials = new NetworkCredential(userName, userPassword);
            request.Method = "POST";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            request.CookieContainer = bpmCookieContainer;
            // Запись xml-сообщения в поток запроса.
            using (var writer = XmlWriter.Create(request.GetRequestStream()))
            {
                entry.WriteTo(writer);
            }
            // Получение ответа от сервиса о результате выполнения операции.
            using (WebResponse response = request.GetResponse())
            {
                if (((HttpWebResponse)response).StatusCode == HttpStatusCode.Created)
                {
                    Console.WriteLine("Country created");
                }
            }
        }

        // Строка запроса:
        // PUT <Адрес приложения BPMonline>/0/ServiceModel/EntityDataService.svc/ContactCollection(guid'00000000-0000-0000-0000-000000000000')
        
        public static void UpdateExistingBpmEnyityByOdataHttpExample(string userName, string userPassword)
        {
            CookieContainer bpmCookieContainer;
            Autorization(userName, userPassword, out bpmCookieContainer);
            // Id записи объекта, который необходимо изменить.
            string countryId = "a470b005-e8bb-df11-b00f-001d60e938c6";
            // Создание сообщения xml, содержащего данные об изменяемом объекте.
            var content = new XElement(dsmd + "properties",
                    new XElement(ds + "Name", "Ukraine")
            );
            var entry = new XElement(atom + "entry",
                    new XElement(atom + "content",
                            new XAttribute("type", "application/xml"),
                            content)
                    );
            // Создание запроса к сервису, который будет изменять данные объекта.
            var request = (HttpWebRequest)HttpWebRequest.Create(serverUri
                    + "CountryCollection(guid'" + countryId + "')");
            request.CookieContainer = bpmCookieContainer;
            //request.Credentials = new NetworkCredential("Сницаренко Сергей", "tqn5496O");
            request.Method = "PUT";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            // Запись сообщения xml в поток запроса.
            Console.WriteLine("=============================");
            Console.WriteLine(entry.ToString());
            Console.WriteLine("=============================");
            using (var writer = XmlWriter.Create(request.GetRequestStream()))
            {
                entry.WriteTo(writer);
            }
            // Получение ответа от сервиса о результате выполнения операции.
            using (WebResponse response = request.GetResponse())
            {
                // Обработка результата выполнения операции.
            }
        }
        
        static void Main(string[] args)
        {
            GetOdataCollectionByAuthByHttpExample("Сницаренко Сергей", "tqn5496O");
            //CreateBpmEntityByOdataHttpExample("Сницаренко Сергей", "tqn5496O");
            UpdateExistingBpmEnyityByOdataHttpExample("Сницаренко Сергей", "tqn5496O");
            GetOdataCollectionByAuthByHttpExample("Сницаренко Сергей", "tqn5496O");

            Console.ReadLine();
        }
    }
}
