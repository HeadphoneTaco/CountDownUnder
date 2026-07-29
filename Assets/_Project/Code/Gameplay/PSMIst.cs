using UnityEngine;

public class PSMist : IState
{
    private PlayerController _player;
    private float _mystStep;
    private Vector2 _dashDirection;
    
    public PSMist(PlayerController player)
    {
        _player = player;
    }
    // the inbetween dashing and transforming state
    // give the player a bunch of speed in the direction they were going before (or not if from idle state)
    // do a boxcast to check for victims and if there are go into blood sucking state
    // if the player is still holding space by the end, put the player into bat state
    // if the player stops holding space put them back into falling state
    // if the player alternatively enters from the bat state the player should be put into falling (this may just happen anyway because they are not still holding space)
    // there may need to be a timer to prevent the player from spamming this ability
    public void Enter()
    {
        // make particles
        // change player into myst form
        _player.CanTransform = false;
        EventManager.DIEvent += ChangeDI;
        _mystStep = 0;
        _dashDirection = _player.DirectionalInput;
        Debug.Log("State Entered: Mist");
    }

    public void Execute()
    {
        _mystStep += Time.deltaTime;
        _player.RB.linearVelocity = _dashDirection * _player.MistSpeed;
        if (_mystStep > _player.MistTime)
        {
            if (_player.BatInputHeld && Time.time - _player.LastBatBreakTime > _player.TimeAfterBreakToTransform)
            {
                _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateBat);
            }
            else
            {
                // turn player into vamp form
                _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateFalling);
            }
        }
    }

    public void Exit()
    {
        EventManager.DIEvent += ChangeDI;
        if (_player != null)
        {
            _player.CanTransform = true;
        }
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
    }
}
