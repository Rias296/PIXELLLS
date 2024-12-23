using System;
using System.Collections.Generic;
using Godot;

public class CharacterStateMachine
{
	// Dictionary to store the valid states
	private Dictionary<Character_Constant.CharacterStates, Action> _states = new Dictionary<Character_Constant.CharacterStates, Action>();

	// Current state of the machine
	private Character_Constant.CharacterStates _currentState;

	// Add a state with an optional action (if you need to trigger something on state change)
	public void AddState(Character_Constant.CharacterStates state, Action onEnterState = null)
	{
		if (!_states.ContainsKey(state))
		{
			_states.Add(state, onEnterState);
		}
	}

	// Method to change the current state
	public void ChangeState(Character_Constant.CharacterStates newState)
	{
		if (_currentState != newState)
	{
		_currentState = newState; // Update the current state
		// GD.Print("State changed to: " + _currentState); // Print the new state for debugging

		if (_states.TryGetValue(newState, out Action onEnterState) && onEnterState != null){
			onEnterState();
		}
	}
	}

	// Get the current state
	public Character_Constant.CharacterStates GetCurrentState()
	{
		return _currentState;
	}

	// Check if the current state matches a given state
	public bool IsInState(Character_Constant.CharacterStates state)
	{
		return EqualityComparer<Character_Constant.CharacterStates>.Default.Equals(_currentState, state);
	}
}
