using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public class VehicleStorage
    {
        public string Name { get; set; } = "";
        public Dictionary<int, Vehicle> Vehicles { get; private set; } = [];

        public double TotalValue { get; private set; } = 0.0;
        public int MotorizedVehicles { get; private set; } = 0;
        public int AerialVehicles { get; private set; } = 0;
        public int AquaticVehicles { get; private set; } = 0;

        public event EventHandler? VehicleAdded;
        public event EventHandler? VehicleRemoved;
        public event EventHandler? VehicleUpdated;

        public VehicleStorage(string name) { Name = name; }

        //with events its kinda possible to makes these void and not worry bout returns.. mhmm...
        public bool TryAddVehicle(Vehicle vehicle)
        {
            if (vehicle.ID == -1) { return false; } 
            if (Vehicles.TryAdd(vehicle.ID, vehicle))
            {
                TallyVehicle(vehicle);
                VehicleAdded?.Invoke(this, null); //For now, could pass some shit tho.
                return true;
            }
            return false;
        }

        public bool TryRemoveVehicle(int id)
        {
            if (Vehicles.TryGetValue(id, out Vehicle? v))
            {
                TallyVehicle(v, true);
                Vehicles.Remove(id);
                VehicleRemoved?.Invoke(this, null); //For now as well, could also pass some shit here too.
                return true;
            }
            return false;
        }

        public bool TryEditVehicle(Vehicle vehicle)
        {
            if (TryRemoveVehicle(vehicle.ID))
            {                                   //Both theses will invoke events, could be for the best. 
                if (TryAddVehicle(vehicle))
                {
                    VehicleUpdated?.Invoke(this, null); //Okay, make a custom handler at this point!!
                    return true;
                }
                return false;
            }
            return false;
        }

        public void LoadFromData(StorageData data)
        {
            Vehicles = [];
            Name = data.Name;
            VehicleFactory.ResetUID();

            if (data.List.Count > 0)
            {
                foreach (Vehicle v in data.List)
                {
                    Vehicles.Add(v.ID, v);
                    TallyVehicle(v);
                }

                int VehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID;
                VehicleFactory.SetVehicleUID(VehicleUid + 1);
            }
        }

        //String literals suck. Maybe Just compare type with typeof or make enum.
        private void TallyVehicle(Vehicle vehicle, bool remove = false)
        {
            TotalValue += (remove) ? -vehicle.Price : vehicle.Price;
            int value = (remove) ? -1 : 1;
            switch (vehicle.GetType().BaseType.Name) // This can maybe be abstracted
            {
                case "AerialVehicle": AerialVehicles += value; break;
                case "AquaticVehicle": AquaticVehicles += value; break;
                case "MotorizedVehicle": MotorizedVehicles += value; break;
                default: break;
            }
        }

        //Here if needed
        public double GetTotalPrice()
        {
            double total = 0.0;
            foreach (Vehicle v in Vehicles.Values) { total += v.Price; }
            return total;
        }

        public StorageData GetSaveData() { return new StorageData(Name, [.. Vehicles.Values]); }
    }
}
