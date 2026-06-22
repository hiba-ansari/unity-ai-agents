# **Games and Artificial Intelligence Techniques<br />COSC2527 / COSC3144<br />Semester 1 2025<br />Assignment 2**

This is the README file for Assignment 2 in Games and Artificial Intelligence Techniques.

## **Project Contributors:**

| Name                 | Main Contribution                                                                   |
| ------------------- | ----------------------------------------------------------------------------------- |
| Team-member 1| Implemented Basic A\*, Decision Tree, and Monte Carlo Tree Search agent (MCTSAgent) |
| Hiba Ansari   | Developed Path Smoothing, Varying Terrain, and Monte Carlo agent (MCAgent)          |
| Team-member 2  | Added Varying Heuristics, Dynamic Obstacles, and Rule-Based agent (RuleBasedAgent)  |

## **Overview:**

**Game Genre:** 2D Strategy.

**AI Features:**

- A\* Pathfinding.
- Decision Tree.
- Monte Carlo Agent.
- Monte Carlo Tree Search Agent.
- Rule-Based Agent.

## **Installation Steps:**

1. Clone the repository to your local machine.
2. Open the project in Unity Hub.
3. Let Unity import all necessary packages and assets automatically.

## **Frog Game:**

### **Overview:**

In the game, the player controls a frog with the objective of eating flies while avoiding snakes, all within a grid-based environment.A variety of AI techniques are demonstrated in the game, such as Finite State Machines (FSM) for behavioral control of non-player characters, A\* Pathfinding for movement, and Decision Trees to direct the frog's AI decisions.The gameplay is improved by features like dynamic obstacles, path smoothing, different terrain types (mud and water), and several heuristic strategies. Players can experiment with various AI configurations, which provides a rich and adaptable experience.

### **Demonstration Controls:**

| Mode            | Action                                      |
| --------------- | ------------------------------------------- |
| Human           | Click to move the frog                      |
| AI              | Frog auto-navigates with optimal heuristic  |
| Bubble Shooting | Press `Spacebar`                            |
| Restart Game    | Press `R` key when Game Over panel is shown |

### **Execution Steps:**

1. **Press Play** in Unity.
2. **Check the box next to `Human Play`** on top right corner of the game screen to turn the game from AI mode to Human mode.
3. **Use mouse right-click** to move the frog (Human mode only).
4. **Select Heuristic Type** (only in Human mode) from the dropdown list at top right corner of the game screen .
5. **Press Spacebar** to shoot bubbles.
6. **Game End Conditions:**

- **Frog health = 0** → _"You Died!"_
- **Fly count = 10** → _"You Won!"_

7. **Various fruits** appears randomly on screen and when the frog eats them:
- **Red** revives a frog’s life.
- **Orange** temporarily increases the snake’s aggro range.
- **Yellow** temporarily slows the snake down.
9. **Press `R`** to restart the game after it ends.

### **Testing Tips:**

- **Use `AStarGrid.cs`** to visualize the A\* pathfinding grid.
- **Test dynamic obstacles** by observing how the frog reroutes when snakes block its path.
- **Verify FSM transitions** by watching console sprite color changes during state changes (e.g., Snake Attack, Flies Flee).
- **Test varying terrain** (Mud, Water, Normal) and observe how terrain affects path cost and movement speed.
- **Experiment with heuristic types** (Euclidean, Manhattan, Diagonal) in Human mode and see how they influence A\* path behavior.
- **Check camera movement** to ensure it smoothly follows the frog using the implemented camera follow script.
- **Test fruit collection** (Various fruits appear randomly on the screen during gameplay):

  - **Red**: Revives frog’s life (restores health).
  - **Orange**: Temporarily increases snake’s aggro range for 3 seconds.
  - **Yellow**: Temporarily slows the snake down for 3 seconds.

## **Connect4 Game:**

### **Overview:**

This game shows how to incorporate AI methods into a traditional strategy game. Each of the three difficulty settings-Easy, Medium, and Hard - is associated with distinct AI tactic. For both Yellow and Red disks, players have the option of HumanAgent or several AI agents, including RandomAgent, MonteCarloAgent, MCTSAgent, and RuleBasedAgent. Strategic AI-versus-AI or human-versus-AI gameplay is made possible by the game. Every AI exhibits distinct decision-making techniques and acts differently depending on the search depth, number of simulations, or rule logic.

### **Demonstration Controls:**

| Mode  | Action                                   |
| ----- | ---------------------------------------- |
| Human | Click on column to place the disc        |
| AI    | Automatically plays based on agent logic |

### **Execution Steps:**

1. **Assign Agents** in `GameController.cs` - set `yellowAgent` and `redAgent` to one of the following:  
   `HumanAgent`, `RandomAgent`, `MonteCarloAgent`, `MCTSAgent`, or `RuleBasedAgent`.
2. **Press Play** in Unity.
3. **Select Difficulty** on the Mode Panel (Easy, Medium, Hard).
4. **Watch or Interact** depending on agent type:

- **`HumanAgent`:** Player clicks to drop discs manually.
- **`RandomAgent`:** Chooses random valid moves.
- **`MonteCarloAgent`:** Uses simulations to evaluate moves.
- **`MCTSAgent`:** Builds and searches a tree of future states.
- **`RuleBasedAgent`:** Uses logic to block, win, or fork.

5. **Game Ends When**:

- **Yellow connects 4** → _“Yellow wins!”_
- **Red connects 4** → _“Red wins!”_
- **Board is full** → _“Match drawn!”_

### **Testing Tips:**

- **Agent Selection**: Assign different agents (e.g. HumanAgent, MCTSAgent, MonteCarloAgent, RandomAgent, RuleBasedAgent) in `GameController.cs` for Yellow and Red players.
- **AI Behavior**: Test AI performance across difficulty levels.
  Compare strategies between Monte Carlo (averaged simulations), MCTS (tree search), and Rule-Based logic.
- **Game Result Detection**: Verify win/draw conditions are triggered correctly and messages display as expected.
- **Tuning Parameters**: Adjusted `totalSims` and `c` in `MCTSAgent.cs` and `MonteCarloAgent.cs` to observe impact on decision-making.
- **UI & Controls**: Test mode selection and toggle handling.
- **Rule-Based Agent**: Use `Debug.Log()` to observe:
  - Winning move detection.
  - Blocking opponent’s win.
  - Fork creation (if `EnableForks` is true).
- **Human Input**: Confirm click-to-place behavior and correct turn handling for `HumanAgent.cs`.
