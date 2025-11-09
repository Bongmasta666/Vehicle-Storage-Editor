using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    public static class VehicleFactory
    {
        public static Vehicle NewVehicle<T>() where T : Vehicle, new()
        {
            return new T();
        }
    }
}
