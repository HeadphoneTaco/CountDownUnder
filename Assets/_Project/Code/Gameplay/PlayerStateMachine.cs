using UnityEngine;

public class PlayerStateMachine
{

    private PlayerController _player;
    private IState _currentState;
    public PSIdle StateIdle;
    public PSWalk StateWalk;
    public PSMist StateMist;
    public PSBat StateBat;
    public PSFalling StateFalling;
    public PSEating StateEating;

    public PlayerStateMachine(PlayerController player)
    {
        _player = player;
        StateIdle = new PSIdle(_player);
        StateWalk = new PSWalk(_player);
        StateMist = new PSMist(_player);
        StateBat = new PSBat(_player);
        StateFalling = new PSFalling(_player);
        StateEating = new PSEating(_player);
    }

    public void Initialize(IState state)
    {
        _currentState = state;
        _currentState.Enter();
    }
    public void Disable()
    {
        _currentState.Exit();
        _currentState = null;
    }
    public void ChangeState(IState state) 
    {
        _currentState.Exit();
        _currentState = state;
        _currentState.Enter();
    }
    public void Execute()
    {
        _currentState?.Execute();
    }
}
