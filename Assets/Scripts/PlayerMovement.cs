using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Timers;
using Assets.Scripts;
using System;

//VERSIONE 2.0
//Struttura del codice:
//  1) Al Player è associato un PlayerState che ne descrive lo dell'animazione corrente.
//  2) Il playerState viene cambiato attraverso il metodo pubblico SetPlayerState(). Usare esclusivamente quel metodo per settare lo stato, senza settarlo direttamente
//     alla proprietà in quanto quel metodo fa eseguire anche il metodo OnPlayerStateChanged, che esegue azioni a seconda del valore dello stato passato e del nuovo.
//  3) IMPORTANTE: Il punto precedente comporta che se è necessario eseguire un'azione su unity allo scadere di un timer, occorre settare un booleano nel corrispondente
//     metodo elapsed e far eseguire l'azione durante l'update o in un altro metodo richiamato direttamente dallo UnityEngine.
//  4) Il SetPlayerState è richiamato praticamente solo dagli script degli stati dell'animator associati al player, in questo modo si ha un'istantanea reale dell'animazione 
//     in atto durante il frame corrente (anche perché il set dei trigger e delle variabili su unity è asincrono).
//  5) Sia per il Player che per i nemici ogni script valuta se il componente relativo sta attaccando e colpendo qualcosa: in caso positivo
//     viene richiamato un metodo pubblico dello script "colpito", che valuta se può essere colpito oppure no ed in caso positivo esegue le relative azioni
//     su se stesso (attivare una animazione, diminuire la vita, ecc.).
//  6) Le azioni che devono essere eseguite una volta sola al cambio del PlayerState sono definite nel metodo OnPlayerStateChanged(); quelle
//     da eseguire ogni frame vanno messe in un metodo richiamato dall'update.
//  7) IMPORTANTE: A quanto mi è sembrato di capire, tutto ciò che interagisce con unity (es. set di variabili, trigger per animazioni, ecc.) può essere eseguito soltanto su un
//     thread inizialmente creato da unity stesso, se è creato internamente non funziona (es. settare un trigger nel metodo di un Timer.Elapsed non va, nello stesso
//     thread del metodo Update si). 
//  8) IMPORTANTE: Il metodo di Update scatta DOPO tutti i vari eventi (OnCollisionEnter, OnFire, OnJump, ecc.)
public class PlayerMovement : MonoBehaviour
{
    public const int MAX_LIVES = 5;
    public const int SHOOT_DAMAGE = 1;
    public const int HIT_DAMAGE = 1;
    public const int SPINE_DAMAGE = 1;

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
    private Timer _hitImmunityTimer = new Timer(1000) { AutoReset = false };
    private Timer _dyingTransitionTimer = new Timer(500) { AutoReset = false };
    private bool _suppressEvents;
    private HealthUiCommands _uiCommands;

    public int CurrentLives { get; private set; } = MAX_LIVES;

    public PlayerStates PlayerState { get; private set; } = PlayerStates.Idle;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleIniziale = myRigidBody.gravityScale;
        myFeetCollider = GetComponent<BoxCollider2D>();
        SetTimers();

        var ui = GameObject.FindWithTag("UI");
        if (ui != null && ui.TryGetComponent<HealthUiCommands>(out var uiCommands))
            _uiCommands = uiCommands;
    }

    void Update()
    {
        if (PlayerState == PlayerStates.Dead)
        {
            myRigidBody.velocity = fermo;
            return;
        }

        if (_suppressEvents)
            return;

        if (IsInstantDeath())
            SetPlayerState(PlayerStates.Dead);

        if (IsCollision(new[] { "Spine" }) && !_hitImmunityTimer.Enabled)
            TakeHit(SPINE_DAMAGE);

        ExecutePlayerActions();
    }

    void OnMove(InputValue value)
    {
        if (_suppressEvents)
            return;

        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) && value.isPressed)
            myRigidBody.velocity += new Vector2(0f, jumpSpeed);
    }

    void OnFire(InputValue value)
    {
        if (_suppressEvents)
            return;

        myAnimator.SetTrigger("IsAttacking");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_suppressEvents)
            return;

        if (collision.gameObject.TryGetComponent<EnemyMovement>(out var enemyMovement) && PlayerState == PlayerStates.Attacking)
            enemyMovement.TakeDamage(HIT_DAMAGE);

        //Debug.Log($"enemyMovement is null = {enemyMovement == null}, count = {_count++}, PlayerState = {PlayerState}");
    }

    public void SetPlayerState(PlayerStates newValue)
    {
        if (newValue == PlayerState)
            return;

        OnPlayerStateChanged(PlayerState, newValue);
        PlayerState = newValue;
    }

    public bool TakeDamage(int damage)
    {
        if (_hitImmunityTimer.Enabled || _suppressEvents)
            return false;

        TakeHit(damage);
        return true;
    }

    public bool Heal(int heartsHealed)
    {
        if (CurrentLives == MAX_LIVES)
            return false;

        CurrentLives = Math.Min(CurrentLives + heartsHealed, MAX_LIVES);
        _uiCommands.GainHeart();
        return true;
    }

    private void OnPlayerStateChanged(PlayerStates oldState, PlayerStates newState)
    {
        switch (oldState, newState)
        {
            case (_, PlayerStates.Dead):
                _suppressEvents = true;
                _uiCommands.IsGameOver = true;
                return;
            case (_, PlayerStates.Dying):
                _dyingTransitionTimer.Enabled = true;
                _suppressEvents = true;
                return;
            case (_, PlayerStates.TakingDamage):
                _hitImmunityTimer.Enabled = true;
                _suppressEvents = false;
                return;
            case (PlayerStates.TakingDamage, _):
                if (CurrentLives <= 0)
                    Die();

                return;
            default:
                _suppressEvents = false;
                return;
        }
    }

    private void ExecutePlayerActions()
    {
        if (_suppressEvents)
            return;

        Run();
        FlipSprite();
        Jump();
        ClimbLadder();
    }

    private bool IsCollision(string[] layerNames)
    {
        return myBodyCollider.IsTouchingLayers(LayerMask.GetMask(layerNames)) || myFeetCollider.IsTouchingLayers(LayerMask.GetMask(layerNames));
    }

    private bool IsInstantDeath()
    {
        return myRigidBody.transform.position.y < -5;
    }

    private void SetTimers()
    {
        _hitImmunityTimer.Elapsed += HitImmunityTimer_Elapsed;
        _dyingTransitionTimer.Elapsed += DyingTransitionTimer_Elapsed;
    }

    private void DyingTransitionTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        SetPlayerState(PlayerStates.Dead);
    }

    private void HitImmunityTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        //Do nothing
    }

    void Jump()
    {
        var isJumping = !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) && !myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Scale"));
        myAnimator.SetBool("isJumping", isJumping);
    }

    private void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * playerSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        var playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
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

    private void TakeHit(int damage)
    {
        myRigidBody.velocity = _hitKnockback;
        CurrentLives -= damage;
        myAnimator.SetTrigger("isTakingDamage");
        _uiCommands.LooseHeart();
    }

    private void Die()
    {
        myRigidBody.velocity = deathKick;
        myAnimator.SetTrigger("Dying");
    }
}
