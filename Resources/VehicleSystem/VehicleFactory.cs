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

        private static Dictionary<int, Vehicle> Vehicles = [];

        public static event EventHandler? VehicleAdded;

        public static Vehicle NewVehicle(int typeIndex)
        {
            Vehicle v = (Vehicle)Activator.CreateInstance(VehicleTypes[typeIndex]);
            v.ID = vehicleUid;
            return v;
        }

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

        public static List<Vehicle> GetVehicleList() => [.. Vehicles.Values]; //Simplified ToList().
    }
}
