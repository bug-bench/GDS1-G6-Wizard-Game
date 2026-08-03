// CustomizationOverlay.cs
using UnityEngine;
using UnityEngine.UI;

public class CustomizationOverlay : MonoBehaviour
{
    [System.Serializable]
    public class OptionRow
    {
        public string label;
        public Image leftArrow;
        public Image rightArrow;
    }

    [Header("Rows — order matters: 0=Character, 1=Clothing, 2=Skin")]
    public OptionRow[] rows;

    [Header("Highlight")]
    public Color selectedColor  = Color.white;
    private Color fullAlpha = new Color(1, 1, 1, 1);
    public Color dimmedColor    = new Color(1, 1, 1, 0.3f);
    public Image characterImage;
    public Image headwearImage;
    public Image bodywearImage;

    private int selectedRow = 0;

    public void Show()
    {
        gameObject.SetActive(true);
        selectedRow = 0;
        RefreshHighlight();
    }

    public void Hide()
    {
        SetImageDim(characterImage, false);
        SetImageDim(headwearImage,  false);
        SetImageDim(bodywearImage,  false);
        gameObject.SetActive(false);
    }

    public void MoveRow(int direction)
    {
        selectedRow = (selectedRow + direction + rows.Length) % rows.Length;
        RefreshHighlight();
    }

    public int GetSelectedRow() => selectedRow;

    void RefreshHighlight()
    {
        bool clothingSelected  = selectedRow == 1;
        bool characterSelected = selectedRow == 0 || selectedRow == 2;

        SetImageDim(characterImage, !characterSelected);
        SetImageDim(headwearImage,  !clothingSelected);
        SetImageDim(bodywearImage,  !clothingSelected);

        for (int i = 0; i < rows.Length; i++)
        {
            bool selected = (i == selectedRow);
            Color c = selected ? selectedColor : dimmedColor;
            if (rows[i].leftArrow  != null) rows[i].leftArrow.color  = c;
            if (rows[i].rightArrow != null) rows[i].rightArrow.color = c;
        }
    }

    void SetImageDim(Image img, bool dimmed)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = dimmed ? 0.3f : 1f;
        img.color = c;
    }
}