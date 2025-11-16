/* File: MotorizedVehicle.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: 
 * This class represents land based vehicles with engines and is an abstract extension of <Vehicle>
 */

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{    
    public abstract class MotorizedVehicle : Vehicle
    {
        public double Mileage { get; set; }

        public MotorizedVehicle() : base() { }
    }
}
