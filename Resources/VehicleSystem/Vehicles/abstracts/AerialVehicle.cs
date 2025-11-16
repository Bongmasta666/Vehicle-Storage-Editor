
namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem.Vehicles.abstracts
{
    //This Class Represents Air Based Vehicles With Engines
    public abstract class AerialVehicle : Vehicle
    {
        public double MaxAltitude { get; set; }

        public AerialVehicle() : base() { }
    }
}
