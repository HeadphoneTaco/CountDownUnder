using UnityEngine;

public class PSBat : IState
{
    private PlayerController _player;
    
    public PSBat(PlayerController player)
    {
        _player = player;
    }
    // the player is a bat and can fly in 8 directions
    // drain the bat meter, if it is empty: change back into mist
    public void Enter()
    {
        // transform into a bat
        Debug.Log("State Entered: Bat");
        _player.RB.gravityScale = 0;
    }

    public void Execute()
    {
        if (_player.ReduceBatTime()) _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateMist);
        _player.RB.linearVelocity += _player.FlySpeed * Time.deltaTime * _player.DirectionalInput;
    }

    public void Exit()
    {
        if (_player != null)
        {
            // transform out of a bat if player isnt null
            _player.RB.gravityScale = _player.DefaultGravity;
        }
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
    }
}
