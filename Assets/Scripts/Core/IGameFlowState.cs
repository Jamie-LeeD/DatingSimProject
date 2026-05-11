namespace DatingSim.Core
{
    public interface IGameFlowState
    {
        GameFlowStateId Id { get; }
        void Enter(GameFlowStateMachine machine);
        void Exit(GameFlowStateMachine machine);
    }
}
