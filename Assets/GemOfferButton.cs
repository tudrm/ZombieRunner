using TMPro;
using UnityEngine;

public class GemOfferButton : MonoBehaviour
{
    public ShopManager shop;
    public int offerIndex;
    public TextMeshProUGUI priceLabel;
    void OnEnable()
    {
        if (shop == null || shop.purchaseProvider == null || !shop.purchaseProvider.IsAvailable) return;
        string price = shop.purchaseProvider.LocalizedPrice(shop.gemStore.offers[offerIndex].productId);
        if (!string.IsNullOrEmpty(price)) priceLabel.text = price;
    }
    public void Purchase() { shop.BuyGemPack(offerIndex); }
}
