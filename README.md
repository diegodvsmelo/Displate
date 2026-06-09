# Paws&Plates

**Paws&Plates** is a tactical restaurant management game in development with Unity, centered around dispatching staff to time-sensitive tasks across a growing restaurant.

Instead of relying on passive preparation timers as the main interaction, the game turns restaurant operation into an active decision-making system: tasks appear on specific restaurant structures, the player evaluates the demand, assigns employees manually, manages stamina, and expands the restaurant while keeping the operation under control.

## Core Concept

The player manages a restaurant through tactical staff dispatch.

Tasks can represent:

- taking customer orders;
- preparing food;
- cleaning tables;
- washing dishes;
- repairing equipment;
- solving operational issues.

Each task appears as a **TaskPin** over the structure that generated it, such as a table, cashier, grill, oven, sink, or other station.

## Current Gameplay Flow

Current implemented/prototyped flow:

1. A TaskPin spawns over a compatible restaurant structure.
2. The player clicks the TaskPin.
3. The Task Team Selection screen opens.
4. The task timer pauses while the player evaluates the task.
5. The player drags employee cards into task slots.
6. TeamStats updates the selected team’s total attributes.
7. The player dispatches the team.
8. The task enters execution.
9. When execution finishes, the task becomes ready to collect.
10. Clicking the finished TaskPin applies rewards or penalties.

## Main Systems

### Tasks

Tasks are defined through `TaskData`.

Each TaskData can define:

- task name;
- description;
- task type;
- outcome flow;
- expiration time;
- execution time;
- max employee slots;
- difficulty points;
- required attributes;
- stamina cost;
- money reward;
- reputation reward;
- reputation penalty;
- XP rewards;
- task icon and color.

The runtime state of a task is handled separately by `TaskInstance`.

### Task States

Tasks currently support these states:

- Available;
- In Progress;
- Ready To Collect;
- Expired;
- Completed.

### Task Pins

TaskPins are the visual and clickable representation of tasks in the scene.

Current behavior:

- Available tasks count down until expiration.
- Dispatching a team starts execution.
- Execution has its own timer.
- Finished tasks become ready to collect.
- Expired tasks apply penalties when collected.
- Ready tasks apply rewards when collected.

### Task Generator Structures

Tasks spawn on specific restaurant structures.

Each structure can hold only one task at a time.

Examples:

- Cashier → Service tasks;
- Table → Service / Operational tasks;
- Grill / Stove / Oven → Cooking / Operational tasks;
- Sink → Operational tasks.

### Task Team Selection

When the player clicks an available TaskPin, the Task Team Selection screen opens.

It currently supports:

- task name and description display;
- dynamic generation of employee slots based on `TaskData.maxSlots`;
- expanded employee cards;
- drag and drop into task slots;
- TeamStats visual update;
- dispatching selected employees.

### Employees

Employees are defined through ScriptableObjects and have:

- Cooking;
- Service;
- Operational;
- Agility;
- stamina;
- XP;
- level;
- skill points;
- optional trait;
- status icons.

Employee attributes range from 1 to 10.

When employees are assigned to a task, their attributes are summed into team attributes, capped at 10 per attribute.

Example:

- Employee A: Service 6
- Employee B: Service 8
- Team Service: 10

### Employee Cards

There are currently two employee card formats:

- compact cards for normal sidebar display;
- expanded cards for task team selection.

Expanded cards show:

- portrait;
- name;
- level;
- trait;
- stamina;
- XP;
- status icons;
- Cooking;
- Service;
- Operational;
- Agility.

Attributes are displayed as 10-square bars with colored filled squares.

### TeamStats

TeamStats displays the total attributes of the currently selected team during task selection.

It updates whenever a card is placed, removed, or swapped in a task slot.

### Stamina

In the design target, stamina should be consumed when the task result is collected.

In the current prototype, stamina is consumed on Dispatch to validate UI feedback and employee card updates earlier.

### Reputation

Reputation is displayed through a star-based UI with partial progress.

The fill is hidden when:

- reputation is zero;
- reputation is exactly at a star threshold;
- reputation is at maximum.

## Built With

- Unity
- C#
- Unity Input System
- ScriptableObjects for data-driven configuration

## Current Development Status

Implemented / prototyped:

- Main menu structure;
- Options menu with audio sliders;
- AudioManager base;
- camera drag and zoom;
- ResourceManager with observer events;
- MoneyUI and ReputationUI;
- TaskData structure;
- TaskInstance runtime structure;
- TaskPin state/timer flow;
- Task spawning on world structures;
- TaskGeneratorStructure;
- one active task per structure;
- TaskTeamSelectionUI;
- dynamic employee task slots;
- employee drag and drop;
- compact and expanded employee cards;
- visual employee attributes;
- TeamStats attribute display;
- basic reward and penalty collection;
- stamina consumption on dispatch for prototype testing.

## Roadmap

### Phase 1 — Current Task Selection Foundation

- [x] Define new TaskData structure
- [x] Separate TaskData from TaskInstance
- [x] Implement TaskPin states
- [x] Implement TaskPin countdown slider
- [x] Spawn TaskPins on world structures
- [x] Restrict structures to one active task
- [x] Open Task Team Selection from TaskPin
- [x] Generate task slots based on maxSlots
- [x] Drag employee cards into task slots
- [x] Show expanded employee cards
- [x] Display TeamStats from selected employees
- [x] Consume stamina on dispatch for prototype feedback
- [x] Finalize TeamStats refresh on every drag/drop edge case
- [x] Improve visual feedback for selected / occupied employees

### Phase 2 — Task Resolution

- [x] Calculate task success chance from TaskData requirements
- [x] Display final success chance in Task Result Popup
- [x] Store calculated chance in TaskInstance
- [x] Roll success/failure when collecting result
- [x] Apply money reward on success
- [x] Apply reputation reward or penalty
- [x] Apply XP to assigned employees
- [x] Apply stamina cost at final intended timing
- [ ] Add critical / 100% chance reward behavior

### Phase 3 — Employee Availability and Rest

- [x] Prevent unavailable or exhausted employees from being assigned
- [x] Mark assigned employees as occupied
- [x] Prevent occupied employees from appearing as available
- [x] Release employees only after result collection
- [x] Add manual rest command
- [x] Add passive stamina recovery
- [x] Add forced full rest at zero stamina

### Phase 4 — Order Flow

- [x] Implement RestaurantOrder runtime object
- [x] Service task generates order
- [x] Order identifies required cooking structure
- [x] Cooking task spawns on correct structure
- [x] Cooking task resolves order delivery
- [x] Table enters eating state
- [x] Table generates cleaning task or new service request

### Phase 5 — Operational Flow

- [x] Tables can become dirty
- [x] Cleaning tasks restore dirty tables to available state

### Phase 6 — Daily Loop and Progression

- [x] Stop new tasks when day timer ends
- [x] Let active tasks finish before ending day
- [x] Generate end-of-day report
- [x] Connect revenue and payroll
- [x] Add reputation tiers
- [ ] Unlock team size / stations / hiring by progression
- [ ] Implement hiring cadence
- [ ] Implement shop cadence

### Phase 7 — Structure Wear and Malfunctions

- [ ] Cooking tasks can generate sink cleaning tasks
- [ ] Operational tasks can repair structures
- [ ] Add wear accumulation to cooking structures
- [ ] Roll malfunction chance after cooking tasks
- [ ] Implement temporary structure block
- [ ] Implement penalty to next tasks
- [ ] Generate repair tasks
- [ ] Balance malfunction frequency and severity

### Phase 9 — 1.0 Systems

- [ ] Trait system integration
- [ ] Debuff system
- [ ] Buff / reward for 100% chance
- [ ] Save/load
- [ ] Final critic day
- [ ] Game feel pass
- [ ] UI polish
- [ ] Audio feedback polish

## Planned Scope for 1.0

The current 1.0 target includes:

- complete daily gameplay loop;
- task dispatch and resolution;
- manual team selection;
- employee stamina and progression;
- reputation and prestige tiers;
- restaurant expansion through purchasable stations;
- hiring;
- basic buffs and debuffs;
- initial trait system;
- final critic/endgame objective.

Advanced systems such as emergent traits, complex random events, free layout placement, and deep decoration customization are considered future expansion possibilities.

## Installation

This repository contains a Unity project.

To open it:

1. Clone the repository.
2. Open the project with the correct Unity version.
3. Let Unity import dependencies.
4. Open the main scene.
5. Run the project from the editor.

## Status

Work in progress.

## Author

Developed by Diego Davis.