# Genetic Euchre
![Genetic Euchre Screenshot](/EuchreCapture.PNG)


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
	I begin by generating an initial population equal to two times the Population Cap. I have tried two initial population generation methods; one, generate a random value 0-10 for each gene; two, modify the recommended strategy by +/-5 randomly for each gene (floor at 0). The fitness for each gene sequence is determined by playing a number of hands (not games) equal the Fitness Test Trials and calculating a percentage win rate. 
	The population is then culled down to the population cap by removing the worst performing individuals. Then, ‘children’ are generated. In this implementation, two ‘parents’ are selected and the child has a chance to inherit each gene randomly from either parent. A mutation chance may modify a random gene of the child by +/- 1. To test for a population that has converged to a solution, the program tests for when there are two or fewer genes that have variations across the whole population.
	The fitness test plays hands of euchre where one team uses the recommended bidding strategy specified above and the other team uses a generated strategy from the population. A hand win for that team is reported as a win to the genetic algorithm and that generated strategy is tested a number of times equal to Fitness Test Trials to generated a percentage win rate.
  
	Some results are avialable in the [Detailed Development Report](https://docs.google.com/document/d/1MAJRUl7Eo8Jm62nuJoKyQ0patQ5Oqda2RF0S1OCGQQM/edit?usp=sharing)
