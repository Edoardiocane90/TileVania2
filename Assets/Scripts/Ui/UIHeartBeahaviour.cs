using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHeartBeahaviour : MonoBehaviour
{
    [SerializeField] public Sprite SpriteEmpty;
    [SerializeField] public Sprite SpriteFull;

    private Animator _animator;
    private int count = 0;

    public HeartState HeartState 
    {
        get => _heartState;
        set
        {
            if (_heartState == value)
                return;

            _heartState = value;
            _isSpriteToUpdate = true;
        }
    }

    private bool _isSpriteToUpdate;
    private Sprite _sprite;
    private HeartState _heartState = HeartState.Full;

    public void ImEmpty()
    {
        int a = 0;
    }

    public void ImCharging()
    {
        int a = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        HeartState = HeartState.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        count++;

        if (count > 4000)
        {
            _animator.SetTrigger("isCharging");
        }

        if (!_isSpriteToUpdate || count < 2000)
            return;

        var triggerName = HeartState == HeartState.Empty ? "isVanishing" : "isCharging";
        _animator.SetTrigger(triggerName);
        _isSpriteToUpdate = false;
    }
}
