using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite markSprite;
    [SerializeField] private SpriteRenderer markParent;


    private int thisCellsX;
    private int thisCellsY;
    private GridMaker gridMaker;

    private bool isMarked = false;

    /// <summary>
    /// GridMaker'da belirlenen Cell'in konumunu kendine tanıtır. Birde GridMaker referansınıda alıyor.
    /// </summary>
    public void Initialize(int x, int y, GridMaker gridMaker)
    {
        thisCellsX = x;
        thisCellsY = y;
        this.gridMaker = gridMaker;
    }

    private void OnEnable()
    {
        //Debug.Log($"Cell at ({thisCellsX}, {thisCellsY}) enabled."); 
        //TO-DO: Init daha geç çağırıldı okuyamadı xle y'yi buna bi bak kontrol sırasında okuyacak mı!
    }

    /// <summary>
    /// Mouse clik ile işaretleme işlemeyi tetikler.
    /// </summary>
    private void OnMouseDown()
    {
        if (!isMarked)
        {
            MarkCell();
        }
    }

    /// <summary>
    /// Hücreyi işaretler. Grid Makerdaki eşleşlme kontrolünü çalıştırır.
    /// </summary>
    private void MarkCell()
    {
        isMarked = true;
        if (markParent != null && markSprite != null)
        {
            markParent.sprite = markSprite;
        }

        gridMaker.CheckMatches(thisCellsX, thisCellsY);
    }

    public bool IsMarked()
    {
        return isMarked;
    }

    public void Clear()
    {
        isMarked = false;
        markParent.sprite = null;
    }

}
