using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingHeartBeahaviour : MonoBehaviour
{
    private PlayerMovement _playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        Utilities.TryGetValidPlayerMovement(out _playerMovement);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != _playerMovement.gameObject)
            return;

        _playerMovement.Heal(1);
        Destroy(gameObject);
    }
}
