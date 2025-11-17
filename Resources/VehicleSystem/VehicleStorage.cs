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
                VehicleAdded?.Invoke(this, new VehicleStorageArgs(vehicle));
                return true;
            }
            return false;
        }

        public bool TryRemoveVehicle(int id)
        {
            if (Vehicles.TryGetValue(id, out Vehicle? old))
            {
                TallyVehicle(old, -1);
                Vehicles.Remove(id);
                VehicleRemoved?.Invoke(this, new VehicleStorageArgs(old));
                return true;
            }
            return false;
        }

        public bool TryEditVehicle(Vehicle vehicle)
        {
            if (Vehicles.TryGetValue(vehicle.ID, out Vehicle? old))
            {
                Vehicles.Remove(old.ID);
                TallyVehicle(old, -1);
                if (Vehicles.TryAdd(vehicle.ID, vehicle))
                {
                    TallyVehicle(old, 1);
                    VehicleUpdated?.Invoke(this, new VehicleStorageArgs(vehicle));
                    return true;
                }
                return false;
            }
            return false;
        }

        public void LoadFromData(StorageData data) 
        {
            Clear();
            Name = data.Name;
            if (data.List.Count > 0)
            {
                foreach (Vehicle vehicle in data.List)
                {
                    Vehicles.Add(vehicle.ID, vehicle);
                    TallyVehicle(vehicle, 1);
                }      
                VehicleFactory.SetVehicleUID(data.NextUID);
            }
        }

        public void Clear()  //We reset ID here to keep this accurate ... maybe not the best
        {
            ResetStats();
            Vehicles = [];
            VehicleFactory.ResetUID();
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

        public StorageData GetSaveData() 
        {
            return new StorageData(Name, [.. Vehicles.Values]) { NextUID = VehicleFactory.VehicleUID };
        }
    }

    public class VehicleStorageArgs(Vehicle v) : EventArgs
    {
        public Vehicle? Vehicle { get; set; } = v; // Passing whole vehicle may be overkill.
    }
}
