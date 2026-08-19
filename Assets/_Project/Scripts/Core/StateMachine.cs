namespace SubjectZero.Core
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            if (newState == CurrentState) return;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Tick() => CurrentState?.Tick();
        public void FixedTick() => CurrentState?.FixedTick();
    }
}