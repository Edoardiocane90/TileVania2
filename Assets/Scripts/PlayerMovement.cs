using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Timers;

public enum PlayerStates
{
    Ok,
    Hit,
    Dying,
    Dead
}

//VERSIONE 2.0
public class PlayerMovement : MonoBehaviour
{
    private const int MAX_LIVES = 5;

    Vector2 moveInput;
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    //[SerializeField] bool isGrounded = false;
    CapsuleCollider2D myBodyCollider; //bisogna richiamare gli oggetti che poi uso nel codice
    BoxCollider2D myFeetCollider;
    [SerializeField] float playerSpeed = 1f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathKick = new Vector2(0f, 10f);
    [SerializeField] Vector2 fermo = new Vector2(0f, 0f);
    [SerializeField] Vector2 _hitKnockback = new Vector2(0f, 15f);
    float gravityScaleIniziale;
    private PlayerStates _playerState = PlayerStates.Ok;
    private Timer _hitImmunityTimer = new Timer(1000) { AutoReset = false };
    private bool _isElapsed;
    private Timer _dyingTransitionTimer = new Timer(1000) { AutoReset = false };
    private int _count;

    public int CurrentLives { get; private set; } = MAX_LIVES;

    /*

    NON MI SERVE PIU QUESTA PARTE DI CODICE CHE MI RILEVAVA COMUNQUE SE TOCCAVO O MENO IL GROUND
    L HO SOSTITUITA CON IL PEZZO DI CODICE PIU ELEGANTE CHE UTILIZZA IsTouchingLayers E ALLA FINE FA LO STESSO
    GENERA UN BOOL CHE E TRUE SE TOCCO, FALSE SE NON TOCCO. 

    // queste due mi dicono se sto entrando o uscendo da una collisione una collisione col Ground
     void OnCollisionEnter2D(Collision2D collision)
        {
            //Check for a match with the specific tag on any GameObject that collides with your GameObject
            if (collision.gameObject.tag  == "Ground")
            {
                Debug.Log("miao");
                isGrounded = true;
            }
        }
        void OnCollisionExit2D(Collision2D collision)
        {
            //Check for a match with the specific tag on any GameObject that collides with your GameObject
            if (collision.gameObject.tag  == "Ground")
            {
                Debug.Log("miao");
                isGrounded = false;
            }
        }

    */


    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleIniziale = myRigidBody.gravityScale;
        myFeetCollider = GetComponent<BoxCollider2D>();
        SetTimers();
    }

    void Update()
    {
        switch (_playerState)
        {
            case PlayerStates.Dead:
                myRigidBody.velocity = fermo;
                return;
            case PlayerStates.Dying:
                if (!_dyingTransitionTimer.Enabled)
                {
                    _dyingTransitionTimer.Enabled = true;
                    Die();
                }

                return;
            case PlayerStates.Hit:
                if (!_hitImmunityTimer.Enabled)
                {
                    _hitImmunityTimer.Enabled = true;
                    TakeHit();
                }

                ExecutePlayerActions();
                return;
            case PlayerStates.Ok:
                if (IsInstantDeath())
                {
                    _playerState = PlayerStates.Dead;
                    return;
                }

                ExecutePlayerActions();
                _playerState = IsEnemyCollision() ? PlayerStates.Hit : PlayerStates.Ok;
                return;
        }
    }

    private void ExecutePlayerActions()
    {
        Run();
        FlipSprite();
        Jump();
        ClimbLadder();
    }

    private bool IsEnemyCollision()
    {
        return myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Spine")) || myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Spine"));
    }

    private bool IsInstantDeath()
    {
        return myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Mare"));
    }

    private void SetTimers()
    {
        _hitImmunityTimer.Elapsed += HitImmunityTimer_Elapsed;
        _dyingTransitionTimer.Elapsed += DyingTransitionTimer_Elapsed;
    }

    private void DyingTransitionTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _playerState = PlayerStates.Dead;
    }

    private void HitImmunityTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _playerState = CurrentLives <= 0 ? PlayerStates.Dying : PlayerStates.Ok;
        //Debug.Log($"Hit immunity elapsed: new player state is {_playerState}");
        _isElapsed = true;
        //Debug.Log($"I set IsTakingDamage false");
    }

    void Jump()
    {
        var isJumping = !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) && !myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Scale"));
        myAnimator.SetBool("isJumping", isJumping);
    }



    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) && value.isPressed)
            myRigidBody.velocity += new Vector2(0f, jumpSpeed);
    }

    void OnFire(InputValue value)
    {
        //Debug.Log("Fire");
        myAnimator.SetTrigger("IsAttacking");
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        var clipName = myAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        if (collision.gameObject.TryGetComponent<EnemyMovement>(out var enemyMovement) && clipName == "PlayerAttack")
            enemyMovement.TakeDamage();

        Debug.Log($"clipName = {clipName}, enemyMovement is null = {enemyMovement == null}, count = {_count++}");
    }

    private void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * playerSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        var playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
        // questo sopra e' un trick per evitare un  ciclo if: metto direttamente il booleano invece di true o false, 
        //tanto lui cambia gia in base al movimento del personaggio              

    }

    private void FlipSprite()
    {
        var playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }
    }


    private void ClimbLadder()
    {
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Scale"))) //se il mio capsule collider tocca il layer chiamato "Scale" allora esci da questo metodo! cosi non salta
        {
            myRigidBody.gravityScale = gravityScaleIniziale;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, moveInput.y * climbSpeed);
        myRigidBody.velocity = climbVelocity;
        myRigidBody.gravityScale = 0f;

        var playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);

    }

    IEnumerator Waiter(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        myRigidBody.velocity = fermo;
    }

    private void TakeHit()
    {
        myRigidBody.velocity = _hitKnockback;
        --CurrentLives;
        //Debug.Log($"timer is {_hitImmunityTimer.Enabled} and I have {CurrentLives} lives");
        myAnimator.SetTrigger("isTakingDamage");
    }

    private void Die()
    {
        myRigidBody.velocity = deathKick;
        myAnimator.SetTrigger("Dying");

    }


}
