using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite markSprite;
    [SerializeField] private SpriteRenderer markParent;

    private bool isMarked = false;

    private void OnMouseDown()
    {
        if (!isMarked)
        {
            MarkCell();
        }
    }


    private void MarkCell()
    {
        isMarked = true;
        if (markParent != null && markSprite != null)
        {
            markParent.sprite = markSprite;
        }
    }

    public bool IsMarked()
    {
        return isMarked;
    }

    public void Clear()
    {
        isMarked = false;
        if (markParent != null)
        {
            markParent.sprite = null;
        }
    }
}
