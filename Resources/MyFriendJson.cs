using System.IO;
using Newtonsoft.Json;

namespace Bongs_Vehicle_Viewer_V2.Resources
{
    public static class MyFriendJson
    {
        //Could maybe use some default variables up here

        public static readonly JsonSerializerSettings jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };
    
        //These two could probably use some error handling.
        public static void SaveThisPlease(object data, string path)
        {
            string json = JsonConvert.SerializeObject(data, jsonSettings);
            File.WriteAllText(path, json);
        }

        public static object GetThisPlease<T>(string path)
        {
            var contents = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<T>(contents, jsonSettings);
            return data;
        }
    }
}
