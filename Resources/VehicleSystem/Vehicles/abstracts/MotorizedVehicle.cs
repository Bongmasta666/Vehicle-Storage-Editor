using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    //This Class Represents Land Based Vehicles With Engines
    public abstract class MotorizedVehicle : Vehicle
    {
        public double Mileage { get; set; }
        public FuelType FuelType { get; set; }

        public MotorizedVehicle() : base() { }
    }
}
