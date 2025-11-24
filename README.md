# Vehicle Storage Editor

Key Features:
  - Add/Edit/Remove Vehicles 
  - Search Vehicles by ID
  - Save & Load Different Storages
  - Storage Data Tracking

Highlights:
  - Json seralizing to make external editing easy.
  - Dynamic property and class handling using System.Reflection.

# Adding Custom Vehicle Types

Heirarchy: Base -> Extended -> Concrete

An Extended vehicle class should be an abstract class that inherits from Vehicle. 
Its role is to better define Vehicle categories and to make sharing properties easier. 
Pre-defined Examples: MotorizedVehicle, SpaceVehicle, AquaticVehicle, AerialVehicle

To add a new Vehicle type you can inherit from one of the pre-defined Extensions or create your own.
You can add properties to an extended class or concrete class and they will automatically be handled.

! Property handling is currently limited and should only be of type Int, Double, or String !
