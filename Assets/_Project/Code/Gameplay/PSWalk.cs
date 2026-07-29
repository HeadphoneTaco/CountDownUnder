using UnityEngine;

public class PSWalk : IState
{
    private PlayerController _player;
    
    public PSWalk(PlayerController player)
    {
        _player = player;
    }
    // take a DI and walk left or right
    // enter mist state on space press 
    public void Enter()
    {
        // play walk animation
        EventManager.DIEvent += ChangeDI;
        Debug.Log("State Entered: Walk");
    }

    public void Execute()
    {
        if(!_player.IsGrounded()) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateFalling);
        _player.RB.linearVelocityX = _player.WalkSpeed * _player.DirectionalInput.x; 
        _player.IncreaseBatTime();
    }

    public void Exit()
    {
        // end walk animation
        EventManager.DIEvent -= ChangeDI;
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
        if (direction.x == 0) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
    }
}
