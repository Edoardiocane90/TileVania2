using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;

public class HealthUiCommands : MonoBehaviour
{
    private readonly Dictionary<(int, HeartState), GameObject> _childDictionary = new Dictionary<(int, HeartState), GameObject>();
    private object _lockHealthOperations = new object();

    public int MaxHealth { get; private set; } = 1;

    public void SetMaxHealth(int maxHeartNumber)
    {
        MaxHealth = maxHeartNumber;
        var hearthCount = _childDictionary.Count / 2;
        for (int i = maxHeartNumber + 1; i <= hearthCount; i++)
        {
            foreach (var heartStateValue in Enum.GetValues(typeof(HeartState)))
            {
                var heart = _childDictionary[(i, (HeartState)heartStateValue)];
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

            _childDictionary[(heartToGainIndex, HeartState.Full)].SetActive(true);
        }
    }

    private KeyValuePair<(int, HeartState), GameObject>? GetLastFullHeart()
    {
        var activeFullHearts = _childDictionary.Where(kvp => kvp.Key.Item2 == HeartState.Full && kvp.Value.activeSelf);
        if (activeFullHearts == null)
            return null;

        return activeFullHearts.OrderBy(kvp => kvp.Key.Item1).Last();
    }

    // Start is called before the first frame update
    void Start()
    {
        var transform = this.gameObject.transform;
        var childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            var splitName = child.name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (splitName.Length != 3 || splitName.First() != "Heart")
                continue;

            var childIndex = int.Parse(splitName.Last());
            var heartState = Utilities.ParseEnum<HeartState>(splitName[1]);
            _childDictionary.Add((childIndex, heartState), child.gameObject);
        }

        SetMaxHealth(PlayerMovement.MAX_LIVES);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
