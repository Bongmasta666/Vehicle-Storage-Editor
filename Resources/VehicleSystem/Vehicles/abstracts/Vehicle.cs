using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    public abstract class Vehicle
    {
        public long? ID { get; set; }
        public int? Year { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public double? Price { get; set; }
        public VehicleConditon Condition { get; set; }

        public Vehicle() { }
        
        public override string ToString()
        {
            return $"{GetType().Name} : {ID} : {Year} : {Make} : {Model}";
        }
    }
}
