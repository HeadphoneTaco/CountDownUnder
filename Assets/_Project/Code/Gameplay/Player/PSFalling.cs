using UnityEngine;

public class PSFalling : IState
{
    private PlayerController _player;
    
    public PSFalling(PlayerController player)
    {
        _player = player;
    }
    // if the player presses space go into mist form
    // if the player hits the ground they should either go into walk or idle depending on if their di is 0 or not
    // if the player hits a victim, suck their blood
    public void Enter()
    {
        _player.MyAnimator.ChangeState(PlayerAnimationState.FALL);
        EventManager.DIEvent += ChangeDI;
        Debug.Log("State Entered: falling");
    }

    public void Execute()
    {
        _player.IncreaseBatTime(_player.BatInfo.FallingRefillReduce);
        _player.RB.linearVelocityX += _player.CountInfo.FallMoveSpeed * Time.deltaTime * _player.DirectionalInput.x;
        if (_player.IsGrounded())
        {
            if (_player.DirectionalInput.x == 0) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
            else _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateWalk);
        }
    }

    public void Exit()
    {
        EventManager.DIEvent -= ChangeDI;
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
    }
    public void FixedUpdate()
    {

    }

}
