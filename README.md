# RB3EHelper

A simple C# .NET 6.0 program to fetch data from the [RB3Enhanced](https://rb3e.rbenhanced.rocks) mod for Rock Band 3 over the network.

Currently, this can output both to a Discord status and to text files (for use with OBS layouts, and such).

Licensed under the MIT license.

## Usage

Download the latest version from the [releases page](https://github.com/InvoxiPlayGames/RB3EHelper/releases) and make sure you have an up-to-date version of RB3Enhanced ([actions build](https://github.com/RBEnhanced/RB3Enhanced/actions) 0.5.1-21*-g4ec1649* or later) installed on your console.

Make sure the following section is present in the `rb3.ini` file on your console:

```ini
[Events]
EnableEvents = true
```

Optionally, you can add `BroadcastTarget = your.computers.LOCAL.ipv4` to the Events section to have the console *only* send state to your computer.
