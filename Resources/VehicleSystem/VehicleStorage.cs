using System.IO;
using System.Text.Json;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.motorized;

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



        //Probably abstract these, but leave for now.
        public void SaveVehicleList()
        {
            StorageData data = new(Name, [.. Vehicles.Values]);
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText("vehicle_data.json", json);
        }

        public void LoadAllVehicles()
        {
            var typeDict = VehicleFactory.TypeDictonary;
            var contents = File.ReadAllText("vehicle_data.json");
            var doc = JsonDocument.Parse(contents);
            var name = doc.RootElement.GetProperty("Name").ToString();
            var list = doc.RootElement.GetProperty("List").EnumerateArray();

            foreach (var item in list)
            {
                if (item.TryGetProperty("Class", out JsonElement prop))
                {
                    if (typeDict.TryGetValue(prop.ToString(), out Type t))
                    {
                        Vehicle v = (Vehicle)JsonSerializer.Deserialize(item, t);
                        Vehicles.Add(v.ID, v);
                        TallyVehicle(v);
                    }
                }
            }
            int VehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID + 1;
            VehicleFactory.SetVehicleUID(VehicleUid);
        }

        public class StorageData
        {
            public string Name { get; set; }
            public List<Vehicle> List { get; set; }

            public StorageData(string name, List<Vehicle> list)
            {
                Name = name;
                List = list;
            }
        }

        //Old Save & Load

        //public void SaveAllVehicles()
        //{
        //    string json = JsonSerializer.Serialize(Vehicles.Values);
        //    File.WriteAllText("vehicles.json", json);
        //}

        //public void LoadAllVehicles()
        //{
        //    var typeDict = VehicleFactory.TypeDictonary;
        //    var contents = File.ReadAllText("vehicles.json");
        //    var vehicles = JsonSerializer.Deserialize<List<JsonElement>>(contents);
        //    foreach (var item in vehicles)
        //    {
        //        if (item.TryGetProperty("Class", out JsonElement prop))
        //        {
        //            if (typeDict.TryGetValue(prop.ToString(), out Type t))
        //            {
        //                Vehicle v = (Vehicle)JsonSerializer.Deserialize(item, t);
        //                Vehicles.Add(v.ID, v);
        //                TallyVehicle(v);
        //            }
        //        }
        //    }
        //    int VehicleUid = Vehicles.OrderBy(v => v.Value.ID).Last().Value.ID + 1;
        //    VehicleFactory.SetVehicleUID(VehicleUid);
        //}

    }
}
