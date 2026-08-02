using UnityEngine;

public class PSIdle : IState
{
    private PlayerController _player;
    
    public PSIdle(PlayerController player)
    {
        _player = player;
    }
    // play idle animation
    // listen for inputs to go into walking and the non moving mist
    public void Enter()
    {
        _player.MyAnimator.ChangeState(PlayerAnimationState.IDLE);
        EventManager.DIEvent += ChangeDI;
        EventManager.JumpEvent += _player.Jump;
        Debug.Log("State Entered: IDLE");
    }

    public void Execute()
    {
        if (!_player.IsGrounded) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateFalling);
        _player.RB.linearVelocityX = Mathf.SmoothStep(_player.RB.linearVelocity.x, 0, _player.CountInfo.AxelSpeed);
        _player.IncreaseBatTime(1);
    }

    public void Exit()
    {
        EventManager.DIEvent -= ChangeDI;
        EventManager.JumpEvent = _player.Jump;
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
        if (direction.x != 0) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateWalk);
    }
    
    public void FixedUpdate()
    {

    }
}
