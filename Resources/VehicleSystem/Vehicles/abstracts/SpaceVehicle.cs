/* File: SpaceVehicle.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description:
 * This class represents space based vehicles with engines or rockets, and is an abstract extension of <Vehicle>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    public abstract class SpaceVehicle : Vehicle
    {
        public HullMaterial HullMaterial { get; set; }
        public SpaceVehicle() : base() { }
    }
}
