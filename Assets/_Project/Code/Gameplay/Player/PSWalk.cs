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
        _player.MyAnimator.ChangeState(PlayerAnimationState.RUN);
        EventManager.DIEvent += ChangeDI;
        EventManager.JumpEvent += _player.Jump;
        Debug.Log("State Entered: Walk");
    }

    public void Execute()
    {
        if(!_player.IsGrounded()) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateFalling);
        _player.RB.linearVelocityX = Mathf.SmoothStep(_player.RB.linearVelocity.x, _player.CountInfo.WalkSpeed * _player.DirectionalInput.x, _player.CountInfo.AxelSpeed);
        _player.IncreaseBatTime(1);
    }

    public void Exit()
    {
        EventManager.DIEvent -= ChangeDI;
        EventManager.JumpEvent -= _player.Jump;
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
        if (direction.x == 0) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
    }
    public void FixedUpdate()
    {

    }
}
