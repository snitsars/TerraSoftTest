using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace CountryProgect3.Models
{
    public interface ICountriesRepository
    {
        bool Add(string itemName);
        IEnumerable<CountryItem> GetAll();
        CountryItem Find(string key);
        CountryItem Remove(string key);
        void Update(string countryId);
    }

    public class CountriesRepository : ICountriesRepository
    {
        // Строка адреса BPMonline сервиса OData.
        private static readonly string serverUri =
            "https://serhiisnitsarenko.bpmonline.com/0/ServiceModel/EntityDataService.svc/";

        private static readonly string authServiceUtri =
            "https://serhiisnitsarenko.bpmonline.com/ServiceModel/AuthService.svc/Login";

        // Ссылки на пространства имен XML.
        private static readonly XNamespace ds = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly XNamespace dsmd = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private static readonly XNamespace atom = "http://www.w3.org/2005/Atom";

        private CookieContainer _bpmCookieContainer = null;

        public bool Add(string itemName)
        {
            Autorization(out _bpmCookieContainer);
            bool result = false;
            // Создание сообщения xml, содержащего данные о создаваемом объекте.
            var content = new XElement(dsmd + "properties",
                new XElement(ds + "Name", itemName));
            var entry = new XElement(atom + "entry",
                new XElement(atom + "content",
                    new XAttribute("type", "application/xml"), content));

            // Создание запроса к сервису, который будет добавлять новый объект в коллекцию контактов.
            var request = (HttpWebRequest) WebRequest.Create(serverUri + "CountryCollection/");
            //request.Credentials = new NetworkCredential(userName, userPassword);
            request.Method = "POST";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            if (_bpmCookieContainer != null) request.CookieContainer = _bpmCookieContainer;
            // Запись xml-сообщения в поток запроса.
            using (var writer = XmlWriter.Create(request.GetRequestStream()))
            {
                entry.WriteTo(writer);
            }
            // Получение ответа от сервиса о результате выполнения операции.
            using (var response = request.GetResponse())
            {
                if (((HttpWebResponse) response).StatusCode == HttpStatusCode.Created)
                {
                    result = true;
                }
            }
            return result;
        }

        public IEnumerable<CountryItem> GetAll()
        {
            IEnumerable<CountryItem> countries = new CountryItem[] {};            
            Autorization(out _bpmCookieContainer);
            // Получение ответа от сервера. Если аутентификация проходит успешно, в объекте _bpmCookieContainer будут 
            // помещены cookie, которые могут быть использованы для последующих запросов.


            // Создание запроса на получение данных от сервиса OData.
            //var dataRequest = HttpWebRequest.Create(serverUri + "ContactCollection?select=Id, Name")
            var dataRequest = WebRequest.Create(serverUri + "CountryCollection?select")
                as HttpWebRequest;
            // Для получения данных используется HTTP-метод GET.
            dataRequest.Method = "GET";
            // Добавление полученных ранее аутентификационных cookie в запрос на получение данных.
            if (_bpmCookieContainer != null) dataRequest.CookieContainer = _bpmCookieContainer;
            // Получение ответа от сервера.
            using (var dataResponse = (HttpWebResponse) dataRequest.GetResponse())
            {
                // Загрузка ответа сервера в xml-документ для дальнейшей обработки.
                var xmlDoc = XDocument.Load(dataResponse.GetResponseStream());
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
                    /*Console.WriteLine(contact.Id + " " + contact.Name);*/
                    countries = countries.Concat(new[] {new CountryItem(contact.Id, contact.Name)});
                }
            }
            return countries;
        }

        public CountryItem Find(string key)
        {
            throw new NotImplementedException();
        }

        public CountryItem Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Update(string countryId)
        {
            
            Autorization(out _bpmCookieContainer);
            // Id записи объекта, который необходимо изменить.
            
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
            request.CookieContainer = _bpmCookieContainer;
            //request.Credentials = new NetworkCredential("Сницаренко Сергей", "tqn5496O");
            request.Method = "PUT";
            request.Accept = "application/atom+xml";
            request.ContentType = "application/atom+xml;type=entry";
            // Запись сообщения xml в поток запроса.
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

        private static void Autorization(out CookieContainer bpmCookieContainer)
        {
            // Создание запроса на аутентификацию.
            var authRequest = WebRequest.Create(authServiceUtri) as HttpWebRequest;
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
                                ""UserName"":""" + "Сницаренко Сергей" + @""",
                                ""UserPassword"":""" + "tqn5496O" + @""",
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
    }

    public class CountryItem
    {
        public CountryItem(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public CountryItem(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        [Required]
        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}