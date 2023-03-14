using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum EnemyStates
{
    Idle,
    Patroling,
    Attacking,
    Hit,
    Dead
}

public class EnemyMovement : MonoBehaviour
{
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    CircleCollider2D myTargetCollider;
    Rigidbody2D myRigidBody;
    [SerializeField] float moveSpeed = 1f;
    Vector2 fermo = new Vector2(0f, 0f);

    private EnemyStates _state = EnemyStates.Patroling;
    private Rigidbody2D _playerComponent;
    private PlayerMovement _playerMovement;
    private System.Timers.Timer _hitImmunityTimer;
    [SerializeField] double hitImmunityTime = 500;
    [SerializeField] double verticalAttackRange = 1;
    private bool _isAlreadyDead;

    private const int MAX_LIVES = 3;

    public int CurrentLives { get; private set; } = MAX_LIVES;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        var player = GameObject.FindWithTag("Player");
        _playerComponent = player.GetComponent<Rigidbody2D>();
        _playerMovement = player.GetComponent<PlayerMovement>();
        _hitImmunityTimer = new System.Timers.Timer(hitImmunityTime) { AutoReset = false };
    }


    void Update()
    {
        if (CurrentLives <= 0)
        {
            Die();
            return;
        }


        if (Math.Abs(_playerComponent.position.y - myRigidBody.position.y) < verticalAttackRange/2)
        {
            
        }


        Move();

    }

    public void TakeDamage()
    {
        Debug.Log($"{myRigidBody.name}: TAKING DAMAGE {DateTime.Now}");
        if (_hitImmunityTimer.Enabled)
            return;

        _hitImmunityTimer.Enabled = true;

        --CurrentLives;
        myAnimator.SetTrigger("IsTakingDamage");
    }

    private void Die()
    {
        myRigidBody.velocity = fermo;
        if (_isAlreadyDead)
            return;

        _isAlreadyDead = true;
        myAnimator.SetTrigger("Die");

    }    

    private void Move()
    {
        myRigidBody.velocity = new Vector2(moveSpeed, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.rigidbody == _playerComponent)
        //    Debug.Log("I GOT GINARDO!");
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Rigidbody2D>() == _playerComponent)
            return;

        moveSpeed = -moveSpeed;
        FlipEnemyFacing();
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
