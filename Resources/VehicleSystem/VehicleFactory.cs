using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public static class VehicleFactory
    {
        // Where Conditon: IsAssignable gets all sub-classes inclduing itself then we filter out abstracts.
        public readonly static List<Type> VehicleTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(Vehicle).IsAssignableFrom(t) && !t.IsAbstract).ToList();

        private static int vehicleUid = 100200;

        public static event EventHandler? VehicleAdded;

        public static Vehicle NewVehicle(int typeIndex)
        {
            Vehicle v = (Vehicle)Activator.CreateInstance(VehicleTypes[typeIndex]);
            v.ID = vehicleUid;
            return v;
        }

        //At this point its possible to get values needed for tracking and update tracker if added.
        public static bool AddVehicle(Vehicle vehicle)
        {
            if (Vehicles.TryAdd(vehicle.ID, vehicle))
            {
                vehicleUid++;
                VehicleAdded?.Invoke(null, new EventArgs());
                return true;
            }
            return false;
        }

        //At this point its possible to get values needed for tracking and update tracker if added.
        public static bool RemoveVehicle(int id)
        {
            if (Vehicles.TryGetValue(id, out Vehicle? v))
            {
                Vehicles.Remove(id);
                return true;
            }
            return false;
        }

        //Kinda Temporary Tracking.
        //Todo: Create variables to store totals and get data when vehicle is added.
        #region TrackerStuff

        private static Dictionary<int, Vehicle> Vehicles = [];
        public static int VehicleCount => Vehicles.Count;
        public static int MotorizedVehicles { get; private set; } = 0;
        public static int AerialVehicles { get; private set; } = 0;
        public static int AquaticVehicles { get; private set; } = 0;
        public static double TotalPrice { get; private set; } = 0;

        //This is just for convience/validation.
        public static double GetTotalPrice() 
        {
            double total = 0.0;
            foreach (Vehicle v in Vehicles.Values) { total += v.Price; }
            return total;
        } 

        public static void UpdateStats()
        {
            foreach (Vehicle v in Vehicles.Values)
            {
                TotalPrice += v.Price;
                switch (v.GetType().BaseType.Name)
                {
                    //String literals suck. Maybe Just compare type with typeof or make enum.
                    case "AerialVehicle": AerialVehicles++; break;
                    case "AquaticVehicle": AquaticVehicles++; break;
                    case "MotorizedVehicle": MotorizedVehicles++; break;
                    default: break;
                }
            }
        }

        public static int GetVehiclesCount(string type)
        {
            int total = 0;
            foreach (Vehicle v in Vehicles.Values) 
            {
                if (v.GetType().BaseType.Name == type)
                {
                    total++;
                }
            }
            return total;
        }

        public static List<Vehicle> GetVehicleList() => [.. Vehicles.Values];

        #endregion TrackerStuff
    }
}
