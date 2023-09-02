using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using TMPro;

public class UiCommands : MonoBehaviour
{
    private const int MAX_COIN_NUMBER = 99;

    private readonly Dictionary<(int, HeartState), GameObject> _heartChildDictionary = new Dictionary<(int, HeartState), GameObject>();
    private GameObject _gameOverSign;
    private GameObject _coinsIndicator;
    private object _lockHealthOperations = new object();

    //l'evento di game over non è settato direttamente quando i cuori vanno a 0 perchè voglio un pelo di ritardo.
    //inoltre non posso settare la visibilità dell'oggetto direttamente perché lo dovrei fare su un thread diverso da quello principale e non verrebbe eseguito.
    public bool IsGameOver { get; set; } = false;

    public int MaxHealth { get; private set; } = 1;

    public int Coins { get; private set; } = 0;

    public void SetMaxHealth(int maxHeartNumber)
    {
        MaxHealth = maxHeartNumber;
        var hearthCount = _heartChildDictionary.Count / 2;
        for (int i = maxHeartNumber + 1; i <= hearthCount; i++)
        {
            foreach (var heartStateValue in Enum.GetValues(typeof(HeartState)))
            {
                var heart = _heartChildDictionary[(i, (HeartState)heartStateValue)];
                heart.SetActive(false);
            }
        }
    }

    public void LooseHeart()
    {
        lock (_lockHealthOperations)
        {
            var lastFullHearth = GetLastFullHeart();
            if (!lastFullHearth.HasValue)
                return;

            lastFullHearth.Value.Value.SetActive(false);
        }
    }

    public void GainHeart()
    {
        lock (_lockHealthOperations)
        {
            var lastFullHearth = GetLastFullHeart();
            var heartToGainIndex = 1;
            if (lastFullHearth.HasValue)
            {
                if (lastFullHearth.Value.Key.Item1 >= MaxHealth)
                    return;

                heartToGainIndex = lastFullHearth.Value.Key.Item1 + 1;
            }

            _heartChildDictionary[(heartToGainIndex, HeartState.Full)].SetActive(true);
        }
    }

    public void AddCoins(int coinNumber)
    {
        if (Coins >= MAX_COIN_NUMBER)
            return;

        Coins += 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetChildren();
        SetGameOverSignVisibility(false);
        SetMaxHealth(PlayerMovement.MAX_LIVES);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGameOver)
            SetGameOverSignVisibility(true);

        var text = _coinsIndicator.GetComponent<TextMeshProUGUI>();
        text.text = Coins.ToString();
    }

    private void GetChildren()
    {
        var transform = this.gameObject.transform;
        var childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            var splitName = child.name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            switch(splitName.Length)
            {
                case 1:
                    if (splitName.First() == "GameOverSign")
                        _gameOverSign = child.gameObject;
                    else if (splitName.First() == "Coins")
                        _coinsIndicator = child.gameObject;

                    continue;
                case 3:
                    var childIndex = int.Parse(splitName.Last());
                    var heartState = Utilities.ParseEnum<HeartState>(splitName[1]);
                    _heartChildDictionary.Add((childIndex, heartState), child.gameObject);
                    break;
                default:
                    continue;
            }
        }
    }

    private KeyValuePair<(int, HeartState), GameObject>? GetLastFullHeart()
    {
        var activeFullHearts = _heartChildDictionary.Where(kvp => kvp.Key.Item2 == HeartState.Full && kvp.Value.activeSelf);
        if (activeFullHearts == null)
            return null;

        return activeFullHearts.OrderBy(kvp => kvp.Key.Item1).Last();
    }

    private void SetGameOverSignVisibility(bool isVisible) => _gameOverSign.SetActive(isVisible);
}
