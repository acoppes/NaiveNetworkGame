# 0.0.2

## Added

  * Show list of assigned ip addresses and let player select which to use.
  * Multiple server instances (press clear servers list if not working)
  * Server auto restart on client disconnection.
  * Server simulation doesn't start until all players are connected.
  * Wait for other players before showing ui and stuff.
  * Units behave like nav agents obstacle logic, moving away from other units with greater obstacle priority.
  * Units now chase nearest target to attack it.
  * Houses to generate income and to increase unit slots.
  * House spawn and death animation.
  * Added coin icon for player's gold.

## Fixed

  * Start local server was using selected ip to connect with client.
  * Wander logic it wasn't performing the idle between movement and movement.
  * NaN bug when trying to move to current position. 
  * Strange interpolation bug with first created unit.
  * Houses have no income and provide no unit slots while spawning.
  
# 0.0.1

## Added

  * Better sync for attack animation and damage.
  * Units death animation + death bodies instead of destroying entities (temporary).
  * Button to start local game in Client and show IP address.
  * Reload between attacks and changed hit time for different health change feedback.
  * Latency with server in connection debug info.
  * Release notes window with toggle to auto open on start.

## Changed

  * Faster animation for health bars.
  * Latency uses now the packet index (byte) instead of sending the time.
  * Now main scene is 1v1 and increased camera size.

## Fixed

  * Death units don't count for max units.
  * Client units destroyed when no more units in server by sending null game state.
  * Default resolution in Client Windows build.
