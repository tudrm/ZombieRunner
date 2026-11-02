using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only. Selection and purchases still go through ShopManager.
public sealed class ShopWeaponCard : MonoBehaviour
{
    [System.NonSerialized] public WeaponDefinition weapon;
    public Image frame, badge;
    public TextMeshProUGUI ownership;
    public void Refresh(WeaponCatalog catalog, WeaponDefinition selected)
    {
        if(weapon==null)return;
        bool equipped=catalog.Equipped==weapon, owned=catalog.IsOwned(weapon);
        GetComponent<Image>().color=Color.white;
        frame.gameObject.SetActive(selected==weapon);
        badge.gameObject.SetActive(owned);
        badge.color=equipped?ShopPresentation.Lime:new Color(.14f,.19f,.13f);
        ownership.text=equipped?"EQUIPPED":owned?"OWNED":
            "<color=#FFBE4D>"+weapon.price.ToString("N0")+" Coins</color>\n<color=#F34B65>"+weapon.gemPrice.ToString("N0")+" Gems</color>";
        ownership.color=equipped?ShopPresentation.Ink:owned?ShopPresentation.Lime:ShopPresentation.White;
        ownership.alignment=owned?TextAlignmentOptions.Center:TextAlignmentOptions.MidlineLeft;
        var r=ownership.rectTransform;
        r.anchorMin=new Vector2(.48f,.08f);r.anchorMax=new Vector2(.96f,owned?.39f:.54f);
        r.offsetMin=r.offsetMax=Vector2.zero;
    }
}
