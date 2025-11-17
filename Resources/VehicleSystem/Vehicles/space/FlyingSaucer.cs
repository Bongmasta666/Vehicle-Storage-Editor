/* File: FlyingSaucer.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * This class is a concrete class of <SpaceVehicle> and contains various FlyingSaucer functions and properties
 */

using Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts;

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.space
{
    class FlyingSaucer : SpaceVehicle
    {
        public double BeamRadius { get; set; } = 0.0;
        public FlyingSaucer() : base() { }
    }
}
