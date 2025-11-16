using System.Reflection;
using System.Windows.Media;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public static class VehicleFactory
    {
        public static readonly Dictionary<string, Type> TypeDictonary = GetTypeDictonary(typeof(Vehicle));
        public const int DefaultUID = 100200;

        //Ordering by ID and getting higest value sucks. Probably save this.
        private static int VehicleUID = DefaultUID;

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

        private static BindingFlags PropertyFlags => BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        public static PropertyInfo[]? GetExtendedProps(string className)
        {
            Type? type = TypeDictonary[className].BaseType;
            return type?.GetProperties(PropertyFlags) ?? null;
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

        public static int GetVehicleUID() { return VehicleUID++; }
        public static void ResetUID() => VehicleUID = DefaultUID;

        public static bool SetVehicleUID(int value) 
        {
            if (value >= VehicleUID)
            {
                VehicleUID = value;
                return true;
            }
            return false;   
        }
    }
}
