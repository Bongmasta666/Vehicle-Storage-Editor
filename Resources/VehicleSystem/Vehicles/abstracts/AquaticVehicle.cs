using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    //This Class Represents Water Based Vehicles With Engines
    public abstract class AquaticVehicle : Vehicle
    {
        public HullMaterial HullMaterial { get; set; }

        public AquaticVehicle() : base() { }
    }
}
