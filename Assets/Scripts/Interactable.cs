using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool top = false;
    public string suit;
    public int value;
    public int row;
    public bool faceUp = false;
    public bool inDeckPile = false;

    private static readonly Dictionary<string, int> valueLookup = new()
    {
        { "A", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
        { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 }, { "10", 10 },
        { "J", 11 }, { "Q", 12 }, { "K", 13 }
    };

    void Start()
    {
        if (CompareTag("Card"))
        {
            ParseCardName(transform.name);
        }
    }

    private void ParseCardName(string cardName)
    {
        suit = cardName[0].ToString();
        string valueStr = cardName.Substring(1);

        if (valueLookup.TryGetValue(valueStr, out int parsedValue))
        {
            value = parsedValue;
        }
        else
        {
            Debug.LogWarning($"Invalid card name: {cardName}");
        }
    }
}
