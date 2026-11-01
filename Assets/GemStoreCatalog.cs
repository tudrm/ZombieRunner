using System;
using UnityEngine;

[Serializable]
public class GemOffer
{
    public string productId;
    public int gems;
    public int euroPrice;
    public string label;
}

[CreateAssetMenu(menuName = "ZombieRunner/Gem Store")]
public class GemStoreCatalog : ScriptableObject
{
    public GemOffer[] offers;
}

// Implement with the platform SDK after products exist in the store console.
// The provider owns receipt verification, persistent transaction deduplication,
// currency fulfillment and store acknowledgement. Never grant from a UI click.
public abstract class GemPurchaseProvider : MonoBehaviour
{
    public abstract bool IsAvailable { get; }
    public abstract string LocalizedPrice(string productId);
    public abstract void Purchase(string productId, Action<bool, string> completed);
}
