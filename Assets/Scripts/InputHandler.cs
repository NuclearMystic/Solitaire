using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputHandler : MonoBehaviour
{
    public GameObject selectedCard;
    private SolitaireGame solitaireScript;
    private float timer;
    private float doubleClickTime = 0.3f;
    private int clickCount = 0;

    void Start()
    {
        solitaireScript = FindObjectOfType<SolitaireGame>();
        selectedCard = gameObject;
    }

    void Update()
    {
        GetMouseClick();
    }

    void GetMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (!hit) return;

        string tag = hit.collider.tag;
        GameObject target = hit.collider.gameObject;

        switch (tag)
        {
            case "Deck": solitaireScript.DealFromDeck(); break;
            case "Card": HandleCardClick(target); break;
            case "Top": HandleTopClick(target); break;
            case "Bottom": HandleBottomClick(target); break;
        }
    }

    void HandleCardClick(GameObject selected)
    {
        Interactable interact = selected.GetComponent<Interactable>();

        if (!interact.faceUp)
        {
            if (!IsBlocked(selected))
            {
                interact.faceUp = true;
                selectedCard = gameObject;
            }
            return;
        }

        if (interact.inDeckPile)
        {
            if (IsBlocked(selected)) return;

            if (selectedCard == selected)
            {
                if (DoubleClick()) AutoStack(selected);
            }
            else
            {
                selectedCard = selected;
            }

            return;
        }

        if (selectedCard == gameObject)
        {
            selectedCard = selected;
        }
        else if (selectedCard == selected)
        {
            if (DoubleClick()) AutoStack(selected);
        }
        else if (CanStackOnto(selectedCard, selected))
        {
            Stack(selected);
        }
        else
        {
            selectedCard = selected;
        }
    }

    void HandleTopClick(GameObject target)
    {
        if (!selectedCard.CompareTag("Card")) return;

        if (selectedCard.GetComponent<Interactable>().value == 1)
        {
            Stack(target);
        }
    }

    void HandleBottomClick(GameObject target)
    {
        if (!selectedCard.CompareTag("Card")) return;

        if (selectedCard.GetComponent<Interactable>().value == 13)
        {
            Stack(target);
        }
    }

    bool CanStackOnto(GameObject source, GameObject destination)
    {
        Interactable s1 = source.GetComponent<Interactable>();
        Interactable s2 = destination.GetComponent<Interactable>();

        if (s2.inDeckPile) return false;

        if (s2.top)
        {
            if (s1.suit != s2.suit && !(s1.value == 1 && s2.suit == null)) return false;
            return s1.value == s2.value + 1;
        }

        if (s1.value != s2.value - 1) return false;

        bool isS1Red = s1.suit == "H" || s1.suit == "D";
        bool isS2Red = s2.suit == "H" || s2.suit == "D";

        return isS1Red != isS2Red;
    }

    void Stack(GameObject selected)
    {
        Interactable s1 = selectedCard.GetComponent<Interactable>();
        Interactable s2 = selected.GetComponent<Interactable>();
        float yOffset = (s2.top || s1.value == 13) ? 0f : 0.3f;

        selectedCard.transform.position = selected.transform.position + new Vector3(0, -yOffset, -0.01f);
        selectedCard.transform.parent = selected.transform;

        if (s1.inDeckPile)
        {
            solitaireScript.cardsOnDisplay.Remove(selectedCard.name);
        }
        else if (s1.top)
        {
            var topStack = solitaireScript.foundationSlots[s1.row].GetComponent<Interactable>();
            topStack.value = s2.top && s1.value == 1 ? 0 : s1.value - 1;
            topStack.suit = s2.top && s1.value == 1 ? null : topStack.suit;
        }
        else
        {
            solitaireScript.tableaus[s1.row].Remove(selectedCard.name);
        }

        s1.inDeckPile = false;
        s1.row = s2.row;
        s1.top = s2.top;

        if (s2.top)
        {
            var newTop = solitaireScript.foundationSlots[s1.row].GetComponent<Interactable>();
            newTop.value = s1.value;
            newTop.suit = s1.suit;
        }

        selectedCard = gameObject;
        solitaireScript.CheckForValidMoves();
        solitaireScript.CheckForWin();
    }

    bool IsBlocked(GameObject selected)
    {
        Interactable s2 = selected.GetComponent<Interactable>();

        if (s2.inDeckPile)
            return s2.name != solitaireScript.cardsOnDisplay.Last();

        return s2.name != solitaireScript.tableaus[s2.row].Last();
    }

    bool DoubleClick()
    {
        return timer < doubleClickTime && clickCount == 2;
    }

    void AutoStack(GameObject selected)
    {
        Interactable selectedData = selected.GetComponent<Interactable>();

        for (int i = 0; i < solitaireScript.foundationSlots.Length; i++)
        {
            Interactable targetStack = solitaireScript.foundationSlots[i].GetComponent<Interactable>();

            if (selectedData.value == 1 && targetStack.value == 0)
            {
                selectedCard = selected;
                Stack(solitaireScript.foundationSlots[i]);
                break;
            }

            if (targetStack.suit == selectedData.suit &&
                targetStack.value == selectedData.value - 1 &&
                HasNoChildren(selectedCard))
            {
                selectedCard = selected;
                string lastCardName = targetStack.suit + CardValueToString(targetStack.value);
                GameObject lastCard = GameObject.Find(lastCardName);
                Stack(lastCard);
                break;
            }
        }
    }

    bool HasNoChildren(GameObject card)
    {
        return card.transform.childCount == 0;
    }

    string CardValueToString(int value)
    {
        return value switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => value.ToString()
        };
    }
}
