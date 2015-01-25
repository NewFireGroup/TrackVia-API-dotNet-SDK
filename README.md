# TrackVia-API-dotNet-SDK
A Microsoft .NET Framework SDK for accessing your TrackVia application data.

## Features

1. Simple client to access the Trackvia API

## Installing

NuGet Package: https://www.nuget.org/packages/InfiNet.TrackVia/

`PM> Install-Package InfiNet.TrackVia`

 
## API Access and The User Key

Obtain a user key by enabling the API at:

  https://go.trackvia.com/#/my-info

Note, the API is only available for Enterprise level accounts

## Usage

First instantiate a TrackViaClient object

```csharp
TrackViaClient client = new TrackViaClient(hostName, email, password, userKey);
```
