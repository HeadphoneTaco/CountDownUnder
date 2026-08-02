using System.Collections.Generic;
using UnityEngine;

public enum PlayerAnimationState{ IDLE, RUN, MIST, BAT, FALL, EATING }
public class PlayerAnimator
{
    private GameObject _bat;
    private Animator _anim;
    private PlayerAnimationState _currentState;
    private Dictionary<PlayerAnimationState, string> _stateDictionary = new Dictionary<PlayerAnimationState, string>()
    {
        { PlayerAnimationState.IDLE, "Idle" },
        { PlayerAnimationState.RUN, "Running" },
        { PlayerAnimationState.MIST, "Nothing" },
        { PlayerAnimationState.BAT, "Nothing" },
        { PlayerAnimationState.FALL, "Falling" },
        { PlayerAnimationState.EATING, "Eating" }
    };

    public PlayerAnimator(GameObject bat, Animator anim)
    {
        _bat = bat;
        _anim = anim;
    }
    public void Initialize(PlayerAnimationState initialState)
    {
        _currentState = initialState;
        _anim.SetBool(_stateDictionary[_currentState], true);
    }
    public void ChangeState(PlayerAnimationState newState)
    {
        if (_currentState == newState) return;
        _anim.SetBool(_stateDictionary[_currentState], false);
        if (_currentState == PlayerAnimationState.BAT) _bat.SetActive(false);
        _currentState = newState;
        _anim.SetBool(_stateDictionary[_currentState], true);
        if (_currentState == PlayerAnimationState.BAT) _bat.SetActive(true);
    }
}
