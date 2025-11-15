using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public class VehicleStorage
    {
        public string Name { get; set; } = "";
        public Dictionary<int, Vehicle> Vehicles { get; } = [];

        public double TotalValue { get; private set; } = 0.0;
        public int MotorizedVehicles { get; private set; } = 0;
        public int AerialVehicles { get; private set; } = 0;
        public int AquaticVehicles { get; private set; } = 0;

        public VehicleStorage(string name) { Name = name; }

        public bool TryAddVehicle(Vehicle vehicle)
        {
            if (vehicle.ID == -1) { return false; } 
            if (Vehicles.TryAdd(vehicle.ID, vehicle))
            {
                TallyVehicle(vehicle);
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
                return true;
            }
            return false;
        }

        public bool TryEditVehicle(Vehicle vehicle)
        {
            if (TryRemoveVehicle(vehicle.ID))
            {
                if (TryAddVehicle(vehicle))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void LoadFromData(StorageData data)
        {
            foreach (Vehicle v in data.List)
            {
                Vehicles.Add(v.ID, v);
                TallyVehicle(v);
            }

            //Maybe Abstract
            int VehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID + 1;
            VehicleFactory.SetVehicleUID(VehicleUid);
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
    }
}
