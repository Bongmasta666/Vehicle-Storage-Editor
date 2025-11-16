/* File: AquaticVehicle.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * This class represents water based vehicles with or without engines and is an abstract extension of <Vehicle>
 */

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    public abstract class AquaticVehicle : Vehicle
    {
        public HullMaterial HullMaterial { get; set; }

        public AquaticVehicle() : base() { }
    }
}
