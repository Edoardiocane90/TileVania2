using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    private UiCommands _uiCommands;

    // Start is called before the first frame update
    void Start()
    {
        Utilities.TryGetValidPlayerMovement(out _playerMovement);

        var ui = GameObject.FindWithTag("UI");
        if (ui != null && ui.TryGetComponent<UiCommands>(out var uiCommands))
            _uiCommands = uiCommands;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != _playerMovement.gameObject)
            return;

        _uiCommands.AddCoins(1);
        Destroy(gameObject);
    }
}
