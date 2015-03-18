C# SDK for the EyeTribe Dev Kit
====
<p>

Introduction
----

This is the C# library implementation for the EyeTribe Dev Kit. This reference implementation provides a simple C# interface for communicating with the EyeTribe Server through our open [TET API](http://dev.theeyetribe.com/api/). This allows developers to easily get started and focus their energy on creating innovative and immersive apps using our eye tracking technology. 

This version is to be considered **_beta_**. Feedback and bug fix submissions are welcome.

Please visit our [developer website](http://dev.theeyetribe.com) for documentation and tutorials. Please use our [public forum](http://theeyetribe.com/forum) for questions and support.


Dependencies
----

The implementation is .NET 3.5 compliant for backwards compatibility and [Unity](http://unity3d.com) support. The library uses [Json.NET](http://james.newtonking.com/json) for JSON parsing.


Build
----

To build, open solution file in compliant [Visual Studio](http://www.visualstudio.com/) version and build. 

Note that building for .Net 3.5 compliance may require the installation of [.Net Framwork 3.5 SP1](https://www.microsoft.com/en-us/download/details.aspx?id=22)


Tutorials
----

A simple guide to using this C# SDK is found in the [tutorials section](http://dev.theeyetribe.com/csharp/) of our developer website. More tutorials will be provided in the near future.


Samples
----

Open source samples for windows are available through [GitHub](https://github.com/eyetribe). These samples shows how to calibrate the EyeTribe Server and give examples of how unique user experiences can be created using eye tracking.


Documentation
----
Find documentation of this library at [EyeTribe C# Doc](http://eyetribe.github.io/tet-csharp-client).


API Reference
----

The complete API specification used by the C# SDK to communicate with the server is available on our [developer website](http://dev.theeyetribe.com/api/).


Changelog
----
0.9.56 (2015-03-18)

- Added *GetHashCode(*) implementation for all public data types
- Refactored internal *Collection* types, fixing several race condition bugs
- Improved *ThreadPool* implementation
- Fixing bugs associated to *CalibrationPoint* resampling
- Fixing network initialization bugs
- Clearing *Listener* types now requires explicit call to *GazeManager.deactivate()*
- Minor syntax changes

0.9.49 (2014-12-09)

- Ensured callback order of listener types during activation 
- Ensured thread safety in singletons
- Refactored internal blocking queues
- More consistent console output on errors
- Unified constructors and operators for all data types
- Added utility methods to GazeData class
- Updated Json.NET to 6.0.6

0.9.36 (2014-07-17)

- Fixed bug, causing offset in GazeData timestamp
- Added method GetTimeDeltaNow() to GazeUtils

0.9.35 (2014-05-20)

- Updated license

0.9.34 (2014-05-09)

- Updated project setting to MS Visual Studio 2013
- Improved multithreading and stability in network layer
- Fixed bug related to initialization lock
- Fixed bug related to multi-screen setups
- Fixed bug related to broadcasting calibration updates

0.9.33 (2014-04-15)

- Thread safe GazeManager activation/deactivation
- Added support for listening to EyeTribe Server conneciton state (IConnectionStateListener)
- Minor API timestamp change
- Updated Json.NET to 6.0.2
- Minor refactoring
- Generel bug fixing and optimization

0.9.27 (2014-02-12)

- Fixed tab/space formatting
- New methods to GazeUtils
- Minor internal refactoring

0.9.26 (2014-01-30)

- Redesign and refactoring of main SDK interface GazeManager to support new TET API features
- Added support for listening for EyeTribe Tracker states (ITrackerStateListener & TrackerState)
- Added support for listening for changes in calibration state (ICalibrationResultListener)
- Added option to fetch current FrameRate setting
- Added option to fetch cached CalibrationResult
- Added GazeUtils class that holds methods common to eye tracking apps
- Added documentation

0.9.21 (2013-01-08)

- Initial release


