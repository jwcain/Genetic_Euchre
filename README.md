# Genetic Euchre
![Genetic Euchre Screenshot](/EuchreCapture.png)


A human playable euchre game where the AI has been augmented using a genetic algorithm. You can read a more [Detailed Development Report](https://docs.google.com/document/d/1MAJRUl7Eo8Jm62nuJoKyQ0patQ5Oqda2RF0S1OCGQQM/edit?usp=sharing) at that link or there is a summary below. 


## Summary

This project had two goals. One, to produce a human-playable euchre game with AI players. Second, to develop a system that allowed me to modify the bidding (or, trump declaration) strategy using a genetic algorithm. You can follow this link to [a web-hosted version of my game](https://jwcain.github.io/Euchre_Play/) (while the game has been reported to work on mobile, it has not been developed for mobile and may have some unknown behaviour).

The project was developed in C# on the Unity3D engine. The game code is broken down into the following categories, with various helper code omitted:
-State Machine
-Behavioural States
-Game Memory
-Notification Center
-Human Interaction Handler
-Animation Handler
-AI Handler

The AI Handler is simplistic in design, it is a static class with methods that take a reference to the game state and player for which to make a decision. The three decisions the AI needs to make are:
-What card to play during the Play round.
-Whether or not to call trump.
-What card to discard as the dealer.
For more information on these processes, please read the [Detailed Development Report](https://docs.google.com/document/d/1MAJRUl7Eo8Jm62nuJoKyQ0patQ5Oqda2RF0S1OCGQQM/edit?usp=sharing)

## Genetic Algorithm

The README for this project is still underconstruction. (Last Updated 10/9/19)


