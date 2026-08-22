using System;

namespace SubjectZero.Core
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public event Action<IState> OnStateChanged;

        public void ChangeState(IState newState)
        {
            if (newState == CurrentState) return;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
            OnStateChanged?.Invoke(newState);
        }

        public void Tick() => CurrentState?.Tick();
        public void FixedTick() => CurrentState?.FixedTick();
    }
}