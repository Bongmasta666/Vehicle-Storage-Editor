using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    //Thanks to Newtonsoft, Vehicles can be saved as abstracts like this to Json.
    //As long as the settings include TypeNameHandling = Auto during both saving and loading.
    public struct StorageData {
        public string Name { get; set; }
        public List<Vehicle> List { get; set; }

        public StorageData(string name, List<Vehicle> list)
        {
            Name = name;
            List = list;
        }
    }
}
