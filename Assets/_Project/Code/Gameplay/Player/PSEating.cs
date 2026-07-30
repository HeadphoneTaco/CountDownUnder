using UnityEngine;

public class PSEating : IState
{
    private PlayerController _player;
    private bool _targetDead;
    private float _bloodSucked;
    
    public PSEating(PlayerController player)
    {
        _player = player;
    }
    // play the eating animation
    // exit into Idle
    // 
    public void Enter()
    {
        _player.CanTransform = false;
        _player.transform.position = _player.Food.transform.position;
        _player.RB.linearVelocity = Vector2.zero;
        if (_player.Food == null||_player.Food.GetBit())
        {
            Debug.Log("food is missing or dead");
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
            return;
        }
    }

    public void Execute()
    {
        if (_player.Food == null)
        {
            Debug.Log("food is missing");
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
            return;
        }
        _player.IncreaseBatTime(1);
        _player.RB.linearVelocity = Vector2.zero;
        DrainBlood();
    }

    public void Exit()
    {
        if (_player != null)
        {
            _player.CanTransform = true;
            _player.Food = null;
        }
    }
    public void FixedUpdate()
    {

    }
    private void DrainBlood()
    {
        (_targetDead, _bloodSucked) =_player.Food.DrainBlood(_player.HitInfo.BloodDrainRate * Time.deltaTime);
        if (_targetDead)
        {
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
        }
    }
}
