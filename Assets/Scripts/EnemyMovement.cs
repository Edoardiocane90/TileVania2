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
    private System.Timers.Timer _attackRecoveryTimer;
    [SerializeField] double hitImmunityTime = 500;
    [SerializeField] double attackRecoveryTime = 500;
    [SerializeField] double dyingTransitionTime = 1000;
    [SerializeField] double verticalAttackRange = 1;
    private bool _iHaveToDie;
    private const int MAX_LIVES = 3;

    public GameObject Bullet;
    private bool _isBulletTime;

    public int CurrentLives { get; private set; } = MAX_LIVES;

    public EnemyStates EnemyState { get; private set; } = EnemyStates.Idle;

    public XorloxFacing Facing { get; private set; } = XorloxFacing.Right;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        SetTimers();
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

        UpdateEnemyFacing();

        if (_isBulletTime)
        {
            //Instantiate(Bullet, transform);
            _isBulletTime = false;
        }
        else if (Math.Abs(_playerComponent.position.y - myRigidBody.position.y) < verticalAttackRange/2 && IsFacingPlayer())
            Shoot();

        if (EnemyState == EnemyStates.Moving)
            Move();
        else
            myRigidBody.velocity = new Vector2(0f, 0f);
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

    private void SetTimers()
    {
        _hitImmunityTimer = new System.Timers.Timer(hitImmunityTime) { AutoReset = false };
        _dyingTransitionTimer = new System.Timers.Timer(dyingTransitionTime) { AutoReset = false };
        _attackRecoveryTimer = new System.Timers.Timer(attackRecoveryTime) { AutoReset = false };
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
            case (EnemyStates.LongRangeAttack, _):
                _attackRecoveryTimer.Enabled = true;
                _isBulletTime = true;
                return;
            case (_, EnemyStates.Dying):
                _dyingTransitionTimer.Enabled = true;
                return;
            case (EnemyStates.Dying, EnemyStates.Dead):
                Destroy(gameObject);
                return;
        }
    }

    private void UpdateEnemyFacing()
    {
        Facing = (XorloxFacing)Math.Sign(transform.localScale.x);
    }

    private void Shoot()
    {
        if (_attackRecoveryTimer.Enabled)
            return;

        myAnimator.SetTrigger("IsAttacking");
    }

    private void Die()
    {
        myRigidBody.velocity = fermo;
        myAnimator.SetTrigger("Dying");
    }    

    private void Move()
    {
        myRigidBody.velocity = new Vector2(moveSpeed, 0f);
    }

    void FlipEnemyFacing()
    {
        var newFacing = -1 * (int)Facing;
        transform.localScale = new Vector2(newFacing, 1f);
    }

    private bool IsFacingPlayer()
    {
        var playerIsAtMyRight = myRigidBody.position.x - _playerComponent.position.x < 0;
        return Facing == XorloxFacing.Right && playerIsAtMyRight || Facing == XorloxFacing.Left && !playerIsAtMyRight;
    }

}
