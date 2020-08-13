using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zauzi.utils.csharp.Networking
{
    public class JsonUtils
    {
        private static class JsonConvert
        {
            private static JsonSerializerSettings setting()
            {
                JsonSerializerSettings result = new JsonSerializerSettings();
                result.MissingMemberHandling = MissingMemberHandling.Ignore;
                return result;
            }
            public static T DeserializeObject<T>(string json)
            {

                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, setting());
            }

            public static string SerializeObject<T>(T obj)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }

        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string SerializeObject(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
