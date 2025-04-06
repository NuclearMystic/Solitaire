using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SolitaireGame : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Array of sprites for all card faces, in deck order.")]
    public Sprite[] cardSuits;
    [Tooltip("Prefab used to instantiate individual card GameObjects.")]
    public GameObject cardPrefab;
    [Tooltip("Button GameObject the player clicks to draw cards from the deck.")]
    public GameObject deckButton;
    [Tooltip("Array of GameObjects representing the 7 bottom piles (tableau).")]
    public GameObject[] tableauSlots;
    [Tooltip("Array of GameObjects representing the 4 top piles (foundations).")]
    public GameObject[] foundationSlots;

    public TMP_Text messageText;

    [Header("Game Data")]
    public static readonly string[] suits = { "C", "D", "H", "S" };
    public static readonly string[] values = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };


    public List<List<string>> tableaus = new();
    public List<List<string>> foundations = new();
    public List<string> cardsOnDisplay = new();
    public List<List<string>> deckDrawGroups = new();

    private readonly List<List<string>> initialTabs = new()
    {
        new(), new(), new(), new(), new(), new(), new()
    };

    public List<string> deck = new();
    public List<string> discardPile = new();

    private int currentDrawIndex;
    private int drawGroupsCount;
    private int remainingCards;

    [Header("Debug")]
    public bool showMoveDebug = false;

    void Start()
    {
        tableaus = initialTabs;
        StartGame();
    }

    public void StartGame()
    {
        deck = LoadDeck();
        Shuffle(deck);

        DealStartingCards();
        StartCoroutine(DealToTableau());
        SortDeckIntoDrawGroups();
    }

    public void ShowHint()
    {
        if (HasValidMoves())
        {
            ShowMessage("There is at least one valid move.");
        }
        else
        {
            ShowMessage("No more valid moves. Draw more cards or restart.");
        }
    }

    public bool HasValidMoves()
    {
        List<GameObject> allCards = GameObject.FindGameObjectsWithTag("Card").ToList();

        foreach (GameObject card in allCards)
        {
            Interactable data = card.GetComponent<Interactable>();

            if (!data.faceUp || card.transform.childCount > 0)
                continue;

            foreach (GameObject foundationSlot in foundationSlots)
            {
                Interactable foundation = foundationSlot.GetComponent<Interactable>();
                if (CanStackToFoundation(data, foundation))
                    Debug.Log($"[Hint Debug] {data.name} can move to foundation slot {foundationSlot.name}");
                return true;
            }

            foreach (GameObject tableauSlot in tableauSlots)
            {
                GameObject top = GetTopCardInSlot(tableauSlot);
                if (top == null) continue;

                Interactable topCard = top.GetComponent<Interactable>();
                if (CanStackToTableau(data, topCard))
                    Debug.Log($"[Hint Debug] {data.name} can stack onto {topCard.name} in tableau.");
                return true;
            }
        }

        if (cardsOnDisplay.Count > 0)
        {
            string topCardName = cardsOnDisplay.Last();
            GameObject topCard = GameObject.Find(topCardName);

            if (topCard != null)
            {
                Interactable drawCard = topCard.GetComponent<Interactable>();

                foreach (GameObject foundationSlot in foundationSlots)
                {
                    Interactable foundation = foundationSlot.GetComponent<Interactable>();
                    if (CanStackToFoundation(drawCard, foundation))
                        return true;
                }

                foreach (GameObject tableauSlot in tableauSlots)
                {
                    GameObject topTableauCard = GetTopCardInSlot(tableauSlot);
                    if (topTableauCard == null) continue;

                    Interactable tableauCard = topTableauCard.GetComponent<Interactable>();
                    if (CanStackToTableau(drawCard, tableauCard))
                        Debug.Log($"[Hint Debug] Draw card {drawCard.name} can stack onto {tableauCard.name}.");
                    return true;
                }
            }
        }


        for (int i = 0; i < tableauSlots.Length; i++)
        {
            List<string> pile = tableaus[i];

            if (pile.Count == 0)
                continue;

            string lastCardName = pile.Last();
            GameObject cardObj = GameObject.Find(lastCardName);
            if (cardObj == null)
                continue;

            Interactable card = cardObj.GetComponent<Interactable>();

            if (!card.faceUp && cardObj.transform.childCount == 0)
            {
                Debug.Log($"[Hint Debug] Card {card.name} can be flipped.");
                return true;
            }
        }


        return false;
    }

    private bool CanStackToFoundation(Interactable card, Interactable foundation)
    {
        if (foundation.value == 0 && card.value == 1 && foundation.suit == null)
            return true;

        if (foundation.suit == card.suit && card.value == foundation.value + 1)
            return true;

        return false;
    }

    private bool CanStackToTableau(Interactable card, Interactable target)
    {
        if (target.value == 0 && card.value == 13)
            return true;

        bool cardRed = card.suit == "H" || card.suit == "D";
        bool targetRed = target.suit == "H" || target.suit == "D";

        if (cardRed != targetRed && card.value == target.value - 1)
            return true;

        return false;
    }

    private GameObject GetTopCardInSlot(GameObject slot)
    {
        Transform current = slot.transform;

        while (current.childCount > 0)
        {
            current = current.GetChild(current.childCount - 1);
        }

        return current.gameObject;
    }

    public static List<string> LoadDeck()
    {
        List<string> newDeck = new();
        foreach (string suit in suits)
            foreach (string value in values)
                newDeck.Add(suit + value);

        return newDeck;
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random rng = new();
        int n = list.Count;

        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    void DealStartingCards()
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = i; j < 7; j++)
            {
                int lastIndex = deck.Count - 1;
                tableaus[j].Add(deck[lastIndex]);
                deck.RemoveAt(lastIndex);
            }
        }
    }

    IEnumerator DealToTableau()
    {
        for (int i = 0; i < 7; i++)
        {
            float yOffset = 0f;
            float zOffset = 0.03f;
            List<string> pile = tableaus[i];

            for (int j = 0; j < pile.Count; j++)
            {
                string card = pile[j];
                yield return new WaitForSeconds(0.03f);

                Vector3 pos = tableauSlots[i].transform.position;
                Vector3 spawnPos = new(pos.x, pos.y - yOffset, pos.z - zOffset);

                GameObject newCard = Instantiate(cardPrefab, spawnPos, Quaternion.identity, tableauSlots[i].transform);
                newCard.name = card;

                var interact = newCard.GetComponent<Interactable>();
                interact.row = i;
                interact.faceUp = (j == pile.Count - 1); 

                yOffset += 0.3f;
                zOffset += 0.03f;
                discardPile.Add(card);
            }
        }

        foreach (string card in discardPile)
            deck.Remove(card);

        discardPile.Clear();
    }

    public void SortDeckIntoDrawGroups()
    {
        deckDrawGroups.Clear();
        drawGroupsCount = deck.Count / 3;
        remainingCards = deck.Count % 3;

        for (int i = 0; i < drawGroupsCount; i++)
        {
            deckDrawGroups.Add(deck.GetRange(i * 3, 3));
        }

        if (remainingCards > 0)
        {
            deckDrawGroups.Add(deck.GetRange(deck.Count - remainingCards, remainingCards));
            drawGroupsCount++;
        }

        currentDrawIndex = 0;
    }

    public void DealFromDeck()
    {
        foreach (Transform child in deckButton.transform)
        {
            if (child.CompareTag("Card"))
            {
                deck.Remove(child.name);
                discardPile.Add(child.name);
                Destroy(child.gameObject);
            }
        }

        if (currentDrawIndex < drawGroupsCount)
        {
            DisplayDrawGroup(deckDrawGroups[currentDrawIndex]);
            currentDrawIndex++;
        }
        else
        {
            RestackDrawPile();
        }
    }

    void DisplayDrawGroup(List<string> drawGroup)
    {
        cardsOnDisplay.Clear();

        float xOffset = 1.5f;
        float zOffset = -0.2f;

        foreach (string card in drawGroup)
        {
            Vector3 basePos = deckButton.transform.position;
            Vector3 spawnPos = new(basePos.x + xOffset, basePos.y, basePos.z + zOffset);

            GameObject newCard = Instantiate(cardPrefab, spawnPos, Quaternion.identity, deckButton.transform);
            newCard.name = card;

            var interact = newCard.GetComponent<Interactable>();
            interact.faceUp = true;
            interact.inDeckPile = true;

            cardsOnDisplay.Add(card);

            xOffset += 0.5f;
            zOffset -= 0.2f;
        }
    }

    void RestackDrawPile()
    {
        deck.Clear();
        deck.AddRange(discardPile);
        discardPile.Clear();
        SortDeckIntoDrawGroups();
    }

    public void CheckForValidMoves()
    {
        if (!HasValidMoves())
        {
            ShowMessage("No more valid moves! Draw more cards or restart.");
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }

    public void CheckForWin()
    {
        GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in allCards)
        {
            Interactable data = card.GetComponent<Interactable>();

            if (!data.top)
                return;
        }

        // All cards are in the foundations
        ShowMessage("You win!");
    }
}
