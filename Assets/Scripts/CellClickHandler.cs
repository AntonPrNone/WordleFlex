using UnityEngine;
using UnityEngine.EventSystems;

public class CellClickHandler : MonoBehaviour, IPointerClickHandler
{
    private WordleGame wordleGame;
    private int cellIndex;

    public void Setup(WordleGame game, int index)
    {
        wordleGame = game;
        cellIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        wordleGame.OnCellClicked(cellIndex);
    }
}
