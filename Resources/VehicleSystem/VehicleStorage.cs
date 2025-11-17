/* File: VehicleStorage.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * This files main purpose is to keep track of vehicles and various statistics. Consider its a garage.
 * Its contains a few functions for Adding/Removing/Editing/Saving/Loading Vehicles. Still W.I.P
 */

using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public class VehicleStorage(string name)
    {
        public string Name { get; set; } = name; 
        public Dictionary<int, Vehicle> Vehicles { get; private set; } = [];

        public event EventHandler? VehicleAdded;
        public event EventHandler? VehicleRemoved;
        public event EventHandler? VehicleUpdated;

        //Binding All These Whould Actually Go HARD!!
        #region Statistic Variables
        public int MotorizedVehicles { get; private set; } = 0;
        public int AerialVehicles { get; private set; } = 0;
        public int AquaticVehicles { get; private set; } = 0;
        public double TotalPrice { get; private set; } = 0.0;
        public double TotalMiles { get; private set; } = 0.0;
        #endregion Statistic Variables

        public bool TryAddVehicle(Vehicle vehicle)
        {
            if (vehicle.ID == -1) { return false; } 
            if (Vehicles.TryAdd(vehicle.ID, vehicle))
            {
                TallyVehicle(vehicle, 1);
                VehicleAdded?.Invoke(this, null); //For now, could pass some shit tho.
                return true;
            }
            return false;
        }

        public bool TryRemoveVehicle(int id)
        {
            if (Vehicles.TryGetValue(id, out Vehicle? v))
            {
                TallyVehicle(v, -1);
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

        public void LoadFromData(StorageData data) //Doing all this here is probably not for the best
        {
            ResetStats();
            Vehicles = [];
            Name = data.Name;
            VehicleFactory.ResetUID();

            if (data.List.Count > 0)
            {
                foreach (Vehicle vehicle in data.List)
                {
                    Vehicles.Add(vehicle.ID, vehicle);
                    TallyVehicle(vehicle, 1);
                }
                int VehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID;
                VehicleFactory.SetVehicleUID(VehicleUid + 1);
            }
        }

        private void TallyVehicle(Vehicle vehicle, int step)
        {
            step = Math.Clamp(step, -1, 1);
            TallySubType(vehicle, step);
            TotalPrice += (vehicle.Price * step);
        }

        private void TallySubType(Vehicle vehicle, int step)
        {
            string? type = vehicle.GetType().BaseType?.Name ?? "";
            switch (type)
            {
                case "AerialVehicle":
                    AerialVehicles += step;
                    break;
                case "AquaticVehicle": 
                    AquaticVehicles += step; 
                    break;
                case "MotorizedVehicle": 
                    MotorizedVehicles += step;
                    var v = (MotorizedVehicle)vehicle;
                    TotalMiles += (v.Mileage * step);
                    break;
                default: break;
            }
        }

        public void ResetStats()
        {
            MotorizedVehicles = 0;
            AerialVehicles = 0;
            AquaticVehicles = 0;
            TotalPrice = 0.0;
            TotalMiles = 0.0;
        }

        public double GetTotalPrice()
        {
            double total = 0.0;
            foreach (Vehicle v in Vehicles.Values) { total += v.Price; }
            return total;
        }

        public StorageData GetSaveData() { return new StorageData(Name, [.. Vehicles.Values]); }
    }
}
