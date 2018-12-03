using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Web;

namespace AuthCodeFlow
{
    public static class HtmlExtensions
    {
        public static IHtmlString SerializeObjectToJson(this object value)
        {
            if (value == null)
            {
                value = new { };
            }
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter)
            {
                QuoteName = false // We don't want quotes around object names
            })
            {
                var serializer = new JsonSerializer
                {
                    // Let's use camelCasing as is common practice in JavaScript
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                serializer.Serialize(jsonWriter, value);

                return new HtmlString(stringWriter.ToString());
            }
        }
    }
}