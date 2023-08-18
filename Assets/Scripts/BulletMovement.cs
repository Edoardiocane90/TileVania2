using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    private const int DAMAGE = 1;

    public EnemyMovement ParentScript;

    [SerializeField] double timeOfExistence = 500;

    private bool _isFirstTimeInstance = true;
    private System.Timers.Timer TimerOfExistence;

    // Start is called before the first frame update
    void Start()
    {
        TimerOfExistence = new System.Timers.Timer(timeOfExistence) { AutoReset = false, Enabled = true };
    }

    // Update is called once per frame
    void Update()
    {
        if (_isFirstTimeInstance)
        {
            _isFirstTimeInstance = false;
            transform.localScale = new Vector2((int)ParentScript.Facing, 1f);
        }

        if (TimerOfExistence.Enabled == false)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            playerMovement.TakeDamage(DAMAGE);
            Destroy(gameObject);
        }
    }
}
