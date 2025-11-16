/* File: MyFriendJson.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * Jason isn't just my friend, he's your friend too! And he is smart thanks to Jimmy NewtonSoft.
 * Ask him nicely to save and load, then watch him do his best. 
 * If you don't know where to save, don't worry, he's got that covered too.
 */

using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;

namespace Bongs_Vehicle_Viewer_V2.Resources
{
    public static class MyFriendJson
    {
        public static readonly JsonSerializerSettings jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };
    
        public static void SaveThisPlease(object data, string path)
        {
            if (!File.Exists(path)) { throw new ArgumentException($"{path} is invalid or does not exist."); }
            if (data.GetType() != typeof(StorageData)) { throw new ArgumentException("Data Must be of type <StorageData>"); }
            string json = JsonConvert.SerializeObject(data, jsonSettings);
            File.WriteAllText(path, json);
        }

        public static object? GetThisPlease<T>(string path)
        {
            if (!File.Exists(path)) { throw new ArgumentException($"{path} is invalid or does not exist."); }
            var contents = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<T>(contents, jsonSettings);
            return data;
        }

        public static string WhereIsShouldISave()
        {
            var targetDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.Parent?.Parent;
            if (targetDir != null)
            {
                return Path.Combine(targetDir.FullName, "Resources", "SaveData");
            }
            else { throw new DirectoryNotFoundException("Json Could not locate target directory"); }
        }
    }
}
