using UnityEngine;
using UnityEngine.UI;

// Layout and visual framing only; all equipment operations live in WeaponCatalog.
public sealed class LockerPresentation : MonoBehaviour
{
    public LockerMenu locker;
    public RectTransform safeArea, header, page, details, action, equipped, collectionHeader, cards, footer, empty;
    public Image background;
    public GridLayoutGroup cardLayout;

    public void Layout()
    {
        Canvas.ForceUpdateCanvases();
        float safeWidth = safeArea.rect.width;
        float width = Mathf.Min(880, safeWidth - 40), left = (safeWidth - width) * .5f;
        ShopManager.Top(header, left, 8, width, 112);
        var viewport = locker.scroll.viewport;
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(left, 20); viewport.offsetMax = new Vector2(-left, -136);
        Canvas.ForceUpdateCanvases();
        float heroHeight = Mathf.Clamp(safeArea.rect.height * .31f, 350, 460);
        ShopManager.Top(details, 0, 0, width, heroHeight);
        ShopManager.Top(action, 0, heroHeight + 20, width, 88);
        ShopManager.Top(equipped, 0, heroHeight + 120, width, 40);
        float collectionY = heroHeight + 188;
        ShopManager.Top(collectionHeader, 0, collectionY, width, 48);
        float cardHeight = 120, gap = 14;
        cardLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardLayout.constraintCount = 1;
        cardLayout.cellSize = new Vector2(width, cardHeight);
        cardLayout.spacing = new Vector2(0, gap);
        float listHeight = Mathf.Max(0, locker.Cards.Count * (cardHeight + gap) - gap);
        ShopManager.Top(cards, 0, collectionY + 62, width, listHeight);
        ShopManager.Top(empty, 0, collectionY + 62, width, 120);
        float footerY = collectionY + 62 + Mathf.Max(listHeight, locker.Cards.Count == 0 ? 120 : 0) + 28;
        ShopManager.Top(footer, 0, footerY, width, 64);
        page.sizeDelta = new Vector2(0, footerY + 84);

        // The original city image is shared with Main Menu and is never stretched.
        var canvasRect = (RectTransform)GetComponent<Canvas>().transform;
        float scale = Mathf.Max(canvasRect.rect.width / background.sprite.rect.width, canvasRect.rect.height / background.sprite.rect.height);
        var r = background.rectTransform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, .5f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = background.sprite.rect.size * scale;
        Canvas.ForceUpdateCanvases();
    }
}
