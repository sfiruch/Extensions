[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/sfiruch/Extensions/workflows/.NET/badge.svg)](https://github.com/sfiruch/Extensions/actions)
[![NuGet Package](https://img.shields.io/nuget/v/sfiruch.Extensions.svg)](https://www.nuget.org/packages/sfiruch.Extensions/)

# Extensions
Collection of useful classes and extensions.

## Installation
Add a reference to the NuGet Package [sfiruch.Extensions](https://www.nuget.org/packages/sfiruch.Extensions/).

## Features
- Console output
  - VT-100 formatting
  - Histograms
- LINQ extensions
- .NET6 xoshiro256** Random
- Vector3 extensions

# Examples
```csharp
Log.Progress = 0;

var aLogLine = Log.AddStatusLine("Ahoy");
var aSecondLogLine = Log.AddStatusLine("Progress", "Group B");

ProcessA();
Console.WriteLine("This is important".StyleBrightRed());

aSecondLogLine.Progress = 0.5;

using(var l = Log.AddStatusLine("Wowza"))
{
	l.Progress = 0;
	ProcessB();
	l.Progress = 0.33;
	ProcessC();
	l.Progress = 0.66;
	ProcessD();
	l.Progress = 1;
}

aLogLine.Remove();
```