/* File: MyFriendJson.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * Jason isn't just my friend, he's your friend too! And he is smart thanks to Jimmy NewtonSoft.
 * Ask him nicely to save and load, then watch him do his best. 
 * If you don't know where to save, don't worry, he's got that covered too.
 */

using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Bongs_Vehicle_Viewer_V2.Resources
{
    public static class MyFriendJson
    {
        public static readonly JsonSerializerSettings jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        public static void SaveThisStorage(VehicleStorage storage, string fileName, string dir)
        {
            StorageData data = storage.GetSaveData();
            string path = Path.Combine(dir, fileName);
            string json = JsonConvert.SerializeObject(data, jsonSettings);
            File.WriteAllText(path, json);
        }

        public static void LoadThisUpPlease(VehicleStorage storage, string filePath)
        {
            var contents = File.ReadAllText(filePath);
            StorageData data = JsonConvert.DeserializeObject<StorageData>(contents, jsonSettings);
            storage.LoadFromData(data);  
        }

        public static string WhereAreMyResource()
        {
            var targetDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.Parent?.Parent?.Parent;
            if (targetDir != null) { return Path.Combine(targetDir.FullName, "Resources"); }
            else { throw new DirectoryNotFoundException("Could not locate your resource directory"); }
        }
    }
}
