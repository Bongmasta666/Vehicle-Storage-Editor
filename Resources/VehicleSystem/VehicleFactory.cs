using System.Reflection;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public static class VehicleFactory
    {
        public static readonly Dictionary<string, Type> TypeDictonary = GetTypeDictonary(typeof(Vehicle));

        //Ordering by ID and getting higest value sucks. Probably save this.
        private static int VehicleUid = 100200;

        public static Vehicle? NewVehicle(string type)
        {
            if (!TypeDictonary.TryGetValue(type, out Type? vehicle)) { return null; }
            Vehicle v = (Vehicle)Activator.CreateInstance(vehicle);
            return v;
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

        public static int GetVehicleUID() { return VehicleUid++; }

        public static bool SetVehicleUID(int value) 
        {
            if (value >= VehicleUid)
            {
                VehicleUid = value;
                return true;
            }
            return false;   
        }
    }
}
