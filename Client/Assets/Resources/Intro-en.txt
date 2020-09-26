# Naive Network Game

This is just a prototype to help evaluate if it is possible to make a simplified RTS game using a Client/Server architecture where almost all the simulation runs on the Server.

# Objectives

* Prove that this architecture is good for this kind of games
* Find which restrictions the game should have to work over this architecture.
* Learn about UDP and ECS during the process. 

## Conclusions (for now)

* Player only performs indirect actions that doesn't require instant reaction.
* Client interpolates game states from Server. 
* Client anticipates some action to show feedbacks as soon as possible. 
* Client can predict part of the game state.
* There is a lot of state that is not important to the game and can be simulated in each Client (sfx, vfx, etc). 