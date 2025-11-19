/* File: StorageData.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: A small struct used for saving vehicle data to Json
 * Thanks to Newtonsoft, Vehicles can be saved as abstracts like this to Json.
 * As long as the settings include TypeNameHandling = Auto during both saving and loading.
 */

using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public class StorageData(string name, List<Vehicle> list)
    {
        public string Name { get; set; } = name;
        public int NextUID { get; set; } = 0;
        public List<Vehicle> List { get; set; } = list;
    }
}
