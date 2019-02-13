using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class StateMachine<T> {

    private T _context;
#pragma warning disable
    public event Action OnStateChanged;
#pragma warning restore

    public MachineState<T> CurrentState { get { return _currentState; } }
    public MachineState<T> PreviousState;
    public float ElapsedTimeInState { get; private set; }

    private Dictionary<System.Type, MachineState<T>> _states = new Dictionary<System.Type, MachineState<T>>();
    private MachineState<T> _currentState;

    public StateMachine(T context, MachineState<T> initialState) {
        _context = context;
        AddState(initialState);
        _currentState = initialState;
        _currentState.Enter();
    }

    public void AddState(MachineState<T> state) {
        state.setMachineAndContext(this, _context);
        _states[state.GetType()] = state;
    }

    public void UpdateMachine(float deltaTime) {
        ElapsedTimeInState += deltaTime;
        _currentState.Update(deltaTime);
    }

    public R ChangeState<R>() where R : MachineState<T> {
        // avoid changing to the same state
        var newType = typeof (R);
        if (_currentState.GetType() == newType) {
            return _currentState as R;
        }

        // only call end if we have a currentState
        if (_currentState != null) {
            _currentState.Exit();
        }
#if UNITY_EDITOR
        // do a sanity check while in the editor to ensure we have the given state in our state list
        if (!_states.ContainsKey(newType)) {
            var error = GetType() + ": state " + newType +
                        " does not exist. Did you forget to add it by calling addState?";
            Debug.LogError(error);
            throw new Exception(error);
        }
#endif

        // swap states and call begin
        PreviousState = _currentState;
        _currentState = _states[newType];
        _currentState.Enter();
        ElapsedTimeInState = 0f;

        // fire the changed event if we have a listener
        if (OnStateChanged != null) {
            OnStateChanged();
        }
        return _currentState as R;
    }
}


public abstract class MachineState<T> {

    protected StateMachine<T> _machine;
    protected T _context;

    internal void setMachineAndContext(StateMachine<T> machine, T context) {
        _machine = machine;
        _context = context;
        Init();
    }
    public virtual void Init() { }
    public virtual void Enter() { }
    public abstract void Update(float deltaTime);
    public virtual void Exit() { }
    public virtual void OnGizmo() { }
    public abstract string Description { get; }
}