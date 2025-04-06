using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    [Tooltip("The front face sprite of this specific card.")]
    public Sprite cardFace;

    [Tooltip("The generic back sprite used for all facedown cards.")]
    public Sprite cardBack;

    private SpriteRenderer spriteRenderer;
    private Interactable interactable;
    private SolitaireGame solitaire;
    private InputHandler inputHandler;

    void Start()
    {
        solitaire = FindObjectOfType<SolitaireGame>();
        inputHandler = FindObjectOfType<InputHandler>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        interactable = GetComponent<Interactable>();

        int index = SolitaireGame.LoadDeck().IndexOf(name);
        if (index < 0 || index >= solitaire.cardSuits.Length)
            return;

        cardFace = solitaire.cardSuits[index];
    }

    void Update()
    {
        spriteRenderer.sprite = interactable.faceUp ? cardFace : cardBack;

        if (inputHandler.selectedCard)
        {
            spriteRenderer.color = (name == inputHandler.selectedCard.name)
                ? Color.yellow
                : Color.white;
        }
    }
}
