using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    public abstract class Vehicle
    {
        public string Class => GetType().Name;
        public int ID { get; set; } = -1;
        public int Year { get; set; } = 0;
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public double Price { get; set; } = 0.0;
        public VehicleConditon Condition { get; set; }

        public Vehicle() { }
        
        public override string ToString()
        {
            return $"ID {ID} : {GetType().Name} : {Year} : {Make} {Model}";
        }
    }
}
