using System.IO;
using System.Text.Json;
using System.Reflection;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public static class VehicleFactory
    {
        public readonly static Dictionary<string, Type> TypeDictonary = GetTypeDictonary(typeof(Vehicle));

        public static Dictionary<int, Vehicle> Vehicles { get; private set; } = [];

        //Ordering by ID and getting higest value sucks. Probably save this.
        private static int vehicleUid = 100200;

        public static event EventHandler? VehicleAdded;

        public static Vehicle? NewVehicle(string type)
        {
            if (!TypeDictonary.TryGetValue(type, out Type? vehicle)) { return null; }
            Vehicle v = (Vehicle)Activator.CreateInstance(vehicle);
            return v;
        }

        public static bool AddVehicle(Vehicle vehicle)
        {
            if (vehicle.ID == -1) { vehicle.ID = vehicleUid++; }
            if (Vehicles.TryAdd(vehicle.ID, vehicle))
            {
                TallyVehicle(vehicle);
                return true;
            }
            return false;
        }

        public static bool RemoveVehicle(int id)
        {
            if (Vehicles.TryGetValue(id, out Vehicle? v))
            {
                TallyVehicle(v, true);
                Vehicles.Remove(id);
                return true;
            }
            return false;
        }

        // Where Conditon: IsAssignable gets all sub-classes inclduing itself then we filter out abstracts.
        private static Dictionary<string, Type> GetTypeDictonary(Type classType, bool includeAbstract = false)
        {
            if (!classType.IsClass) { throw new ArgumentException($"Unable to create a Type Dictionary of Type {nameof(classType)}"); }
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => classType.IsAssignableFrom(t) && t.IsAbstract == includeAbstract).ToDictionary(t => t.Name, t => t);
        }

        public static List<string> GetClassNames()
        {
            List<string> names = [];
            foreach (Type t in TypeDictonary.Values) { names.Add(t.Name); }
            return names;
        }

        public static List<int> GetValidYears(int start, int end, bool flip = true)
        {
            List<int> years = [];
            for (int i = start; i < end; i++) { years.Add(i); }
            if (flip) { years.Reverse(); }
            return years;
        }

        //This will probably need a try catch to handle loading or deseralization issues. 
        //We need to know what happened, Gotta get the message to the status bar.
        public static void LoadAllVehicles()
        {
            var contents = File.ReadAllText("vehicles.json");
            var vehicles = JsonSerializer.Deserialize<List<JsonElement>>(contents);
            foreach (var item in vehicles)
            {
                if (item.TryGetProperty("Class", out JsonElement prop))
                {
                    if (TypeDictonary.TryGetValue(prop.ToString(), out Type t))
                    {
                        Vehicle v = (Vehicle)JsonSerializer.Deserialize(item, t);
                        Vehicles.Add(v.ID, v);
                        TallyVehicle(v);
                    }
                }
            }
            vehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID + 1;
        }

        //This is kinda rough as the list scales since saving is done every ADD/REMOVE.
        //Maybe Make a save funtion that possible just appends a line. This applies to removing as well.
        public static void SaveVehicleList()
        {
            var json = JsonSerializer.Serialize(Vehicles.Values);
            File.WriteAllText("vehicles.json", json);
        }

        //String literals suck. Maybe Just compare type with typeof or make enum.
        public static void TallyVehicle(Vehicle vehicle, bool remove = false)
        {
            TotalPrice += (remove) ? -vehicle.Price: vehicle.Price;
            int value = (remove) ? -1 : 1;
            switch (vehicle.GetType().BaseType.Name) // This can maybe be abstracted
            {
                case "AerialVehicle": AerialVehicles += value; break;
                case "AquaticVehicle": AquaticVehicles += value; break;
                case "MotorizedVehicle": MotorizedVehicles += value; break;
                default: break;
            }
        }

        //Kinda Temporary Tracking.
        //Todo: Create variables to store totals and get data when vehicle is added.
        #region TrackerStuff

        public static int VehicleCount => Vehicles.Count;
        public static int MotorizedVehicles { get; private set; } = 0;
        public static int AerialVehicles { get; private set; } = 0;
        public static int AquaticVehicles { get; private set; } = 0;
        public static double TotalPrice { get; private set; } = 0;

        public static double GetTotalPrice() 
        {
            double total = 0.0;
            foreach (Vehicle v in Vehicles.Values) { total += v.Price; }
            return total;
        }     

        #endregion TrackerStuff
    }
}
