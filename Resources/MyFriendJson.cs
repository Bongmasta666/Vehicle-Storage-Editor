/* File: MyFriendJson.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * Jason isn't just my friend, he's your friend too! And he is smart thanks to Jimmy NewtonSoft.
 * Ask him nicely to save and load, then watch him do his best. 
 * If you don't know where to save, don't worry, he's got that covered too.
 */

using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Bongs_Vehicle_Viewer_V2.Resources
{
    public static class MyFriendJson
    {
        public static readonly string ResourcesDir = WhereAreMyResource();

        public static readonly string ImagesDir = Path.Combine(ResourcesDir, "Images");
        public static readonly string DefaultSaveDir = Path.Combine(ResourcesDir, "SaveData");

        public static readonly JsonSerializerSettings jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        public static void SaveThisPlease<T>(object data, string dir, string file) where T : class
        {
            var path = Path.Combine(dir, file);
            if (!File.Exists(path)) { throw new FileNotFoundException($"[Dir: {dir}] [File: {file}]"); }

            var contents = JsonConvert.SerializeObject(data, typeof(T), jsonSettings);
            File.WriteAllText(path, contents);
        }

        public static T? GetThisForMePlease<T>(string dir, string file) where T : class 
        {
            var path = Path.Combine(dir, file);
            if (!File.Exists(path)) { throw new FileNotFoundException($"[Dir: {dir}] [File: {file}]"); }

            var contents = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(contents, jsonSettings);
        }

        public static string WhereAreMyResource()
        {
            var targetDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.Parent?.Parent?.Parent;
            if (targetDir != null) { return Path.Combine(targetDir.FullName, "Resources"); }
            else { throw new DirectoryNotFoundException("Could not locate your resource directory"); } // this will have to change
        }
    }
}
