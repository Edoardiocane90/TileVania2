using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public const int SHOOT_DAMAGE = 2;
    public const int HIT_DAMAGE = 1;

    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    CircleCollider2D myTargetCollider;
    Rigidbody2D myRigidBody;
    [SerializeField] float moveSpeed = 1f;
    Vector2 fermo = new Vector2(0f, 0f);

    private Rigidbody2D _playerComponent;
    private System.Timers.Timer _hitImmunityTimer;
    private System.Timers.Timer _dyingTransitionTimer;
    [SerializeField] double hitImmunityTime = 500;
    [SerializeField] double dyingTransitionTime = 500;
    [SerializeField] double verticalAttackRange = 1;
    private bool _iHaveToDie;
    private const int MAX_LIVES = 3;

    public int CurrentLives { get; private set; } = MAX_LIVES;

    public EnemyStates EnemyState { get; private set; } = EnemyStates.Idle;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        SetTimers();
    }


    void Update()
    {
        if (_playerComponent == null)
        {
            Utilities.TryGetValidPlayerComponent(out _playerComponent);
            return;
        }

        if (EnemyState == EnemyStates.Dying || EnemyState == EnemyStates.Dead)
        {
            if (_iHaveToDie)
            {
                _iHaveToDie = false;
                SetEnemyState(EnemyStates.Dead);
            }
            myRigidBody.velocity = fermo;
            return;
        }

        if (Math.Abs(_playerComponent.position.y - myRigidBody.position.y) < verticalAttackRange/2 && IsFacingPlayer())
        {
            Shoot();
        }

        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out var playerMovement))
            playerMovement.TakeDamage(HIT_DAMAGE);
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Rigidbody2D>() == _playerComponent)
            return;

        moveSpeed = -moveSpeed;
        FlipEnemyFacing();
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log($"{myRigidBody.name}: TAKING DAMAGE {DateTime.Now}");
        if (_hitImmunityTimer.Enabled)
            return;

        CurrentLives -= damage;
        myAnimator.SetTrigger("IsTakingDamage");
    }

    public void SetEnemyState(EnemyStates newValue)
    {
        if (EnemyState == newValue)
            return;

        OnEnemyStateChanged(EnemyState, newValue);
        EnemyState = newValue;
    }

    private void SetTimers()
    {
        _hitImmunityTimer = new System.Timers.Timer(hitImmunityTime) { AutoReset = false };
        _dyingTransitionTimer = new System.Timers.Timer(dyingTransitionTime) { AutoReset = false };
        _dyingTransitionTimer.Elapsed += _dyingTransitionTimer_Elapsed;
    }

    private void _dyingTransitionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _iHaveToDie = true;
    }

    private void OnEnemyStateChanged(EnemyStates oldValue, EnemyStates newValue)
    {
        switch(oldValue, newValue)
        {
            case (EnemyStates.TakingDamage, _):
                if (CurrentLives <= 0)
                    Die();

                return;
            case (_, EnemyStates.TakingDamage):
                _hitImmunityTimer.Enabled = true;
                return;
            case (_, EnemyStates.Dying):
                _dyingTransitionTimer.Enabled = true;
                return;
            case (EnemyStates.Dying, EnemyStates.Dead):
                Destroy(gameObject);
                return;
        }
    }

    private void Shoot()
    {

    }

    private void Die()
    {
        myRigidBody.velocity = fermo;
        myAnimator.SetTrigger("Die");
    }    

    private void Move()
    {
        myRigidBody.velocity = new Vector2(moveSpeed, 0f);
    }

    void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
    }

    private bool IsFacingPlayer()
    {
        var isFacingRight = myRigidBody.velocity.x > 0;
        var playerIsAtMyRight = myRigidBody.position.x - _playerComponent.position.x < 0;
        return isFacingRight && playerIsAtMyRight || !isFacingRight && !playerIsAtMyRight;
    }

}
