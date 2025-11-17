/* File: Enums.cs
 * Author: Michael Millar
 * Date: 16-11-2025
 * Description: A small class containing various vehicle related enums
 */

namespace Bongs_Vehicle_Viewer_V2.Resources.VehicleSystem
{
    //careful with changing enums .. anything previously saved and using enums values may change if stored as intergers.

    public enum VehicleConditon { New, Used, Junk, Restored }
    public enum HullMaterial { Aluminum, Fiberglass, Plastic, Rubber, Steel, Titanium, Wood, Unlisted }
    public enum FuelType { Gas, Diesel, Electric, Hybrid, Hydrogen, JetFuel, Liquor, Methane, Uranium, Unlisted }
}
