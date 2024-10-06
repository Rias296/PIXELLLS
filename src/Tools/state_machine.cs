using System;
using System.Collections.Generic;

public class StateMachine<T>
{
    // Dictionary to store the valid states
    private Dictionary<T, Action> _states = new Dictionary<T, Action>();

    // Current state of the machine
    private T _currentState;

    // Add a state with an optional action (if you need to trigger something on state change)
    public void AddState(T state, Action onEnterState = null)
    {
        if (!_states.ContainsKey(state))
        {
            _states.Add(state, onEnterState);
        }
    }

    // Method to change the current state
    public void ChangeState(T newState)
    {
        if (_states.ContainsKey(newState))
        {
            _currentState = newState;
            // Invoke the action associated with entering the new state, if there is one
            _states[newState]?.Invoke();
            Console.WriteLine("Changed state to: " + newState);
        }
        else
        {
            Console.WriteLine("State not found: " + newState);
        }
    }

    // Get the current state
    public T GetCurrentState()
    {
        return _currentState;
    }

    // Check if the current state matches a given state
    public bool IsInState(T state)
    {
        return EqualityComparer<T>.Default.Equals(_currentState, state);
    }
}
