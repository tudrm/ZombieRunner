using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LockerWeaponCard : MonoBehaviour
{
    public Image thumbnail, selectedFrame;
    public TextMeshProUGUI title, stats, state;
    public Button button;
    public WeaponDefinition Weapon { get; private set; }

    public void Bind(LockerMenu owner, WeaponDefinition weapon)
    {
        Weapon = weapon;
        thumbnail.sprite = weapon.thumbnail;
        thumbnail.preserveAspect = true;
        title.text = weapon.displayName;
        stats.text = weapon.category.ToString().ToUpperInvariant() + "  •  " + weapon.damage + " DMG / " + weapon.roundsPerMinute + " RPM";
        button.onClick.AddListener(() => owner.SelectWeapon(Weapon));
    }

    public void Refresh(WeaponDefinition selected, WeaponDefinition equipped)
    {
        bool isSelected = Weapon == selected, isEquipped = Weapon == equipped;
        selectedFrame.gameObject.SetActive(isSelected);
        title.color = isSelected ? ShopPresentation.Lime : ShopPresentation.White;
        state.text = isEquipped ? "EQUIPPED" : isSelected ? "SELECTED" : "OWNED";
        state.color = isEquipped || isSelected ? ShopPresentation.Lime : ShopPresentation.Muted;
    }
}
