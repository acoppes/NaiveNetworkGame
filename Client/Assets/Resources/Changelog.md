# 0.0.1

## Added

  * Button to start local game in Client and show IP address.
  * Reload between attacks and changed hit time for different health change feedback.
  * Latency with server in connection debug info.
  * Release notes window with toggle to auto open on start.

## Changed

  * Latency uses now the packet index (byte) instead of sending the time.
  * Now main scene is 1v1 and increased camera size.

## Fixed

  * Client units destroyed when no more units in server by sending null game state.
  * Default resolution in Client Windows build.

## Removed