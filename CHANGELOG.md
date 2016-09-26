# Change Log #
---

Version 0.9.77.3 (2016-09-26)
---
- Added support for listening to any calibration state change (ICalibrationStateListener)
- Runtime 'debug mode' now controlled through GazeManager.DebugMode 
- Default states for all GazeManager enum types
- Unittests now using NUnit
- Updated Json.NET to 9.0.1

Version 0.9.77.2 (2016-06-05)
---
- Fix listener bug

Version 0.9.77 (2016-05-17)
---
- Extensive rewrite of core classes
- New support for .Net 3.5, 4.0 & 4.5 frameworks
- Added support for methods of async nature in *GazeManager*
- Added support for debug mode via *GazeManagerCore.IS\_DEBUG\_MODE*
- *Point2D* & *Point3D* are now 'struct' types
- Moving all data types from 64-bit to 32-bit floating point precision
- Added calibration evaluation class *CalibUtils*
- Added support for listening to active screen index changes (*IScreenStateListener*)
- Adapting to EyeTribe API changes, deprecating obsolete *GazeManager* method calls 
- Added Unit Tests
- Updated Json.NET to 8.0.3

Version 0.9.56 (2015-03-18)
---
- Added *GetHashCode(*) implementation for all public data types
- Refactored internal *Collection* types, fixing several race condition bugs
- Improved *ThreadPool* implementation
- Fixing bugs associated to *CalibrationPoint* resampling
- Fixing network initialization bugs
- Clearing *Listener* types now requires explicit call to *GazeManager.deactivate()*
- Minor syntax changes

Version 0.9.49 (2015-12-09)
---
- Ensured callback order of listener types during activation 
- Ensured thread safety in singletons
- Refactored internal blocking queues
- More consistent console output on errors
- Unified constructors and operators for all data types
- Added utility methods to GazeData class
- Updated Json.NET to 6.0.6

Version 0.9.36 (2015-07-17)
---

- Fixed bug, causing offset in GazeData timestamp
- Added method GetTimeDeltaNow() to GazeUtils

Version 0.9.35 (2014-05-20)
---

- Updated license

Version 0.9.34 (2014-05-09)
---

- Updated project setting to MS Visual Studio 2013
- Improved multithreading and stability in network layer
- Fixed bug related to initialization lock
- Fixed bug related to multi-screen setups
- Fixed bug related to broadcasting calibration updates

Version 0.9.33 (2014-04-15)
---

- Thread safe GazeManager activation/deactivation
- Added support for listening to EyeTribe Server conneciton state (IConnectionStateListener)
- Minor API timestamp change
- Updated Json.NET to 6.0.2
- Minor refactoring
- Generel bug fixing and optimization

Version 0.9.27 (2014-02-12)
---

- Fixed tab/space formatting
- New methods to GazeUtils
- Minor internal refactoring

Version 0.9.26 (2014-01-30)
---

- Redesign and refactoring of main SDK interface GazeManager to support new TET API features
- Added support for listening for EyeTribe Tracker states (ITrackerStateListener & TrackerState)
- Added support for listening for changes in calibration state (ICalibrationResultListener)
- Added option to fetch current FrameRate setting
- Added option to fetch cached CalibrationResult
- Added GazeUtils class that holds methods common to eye tracking apps
- Added documentation

Version 0.9.21 (2014-01-30)
---

- Initial release