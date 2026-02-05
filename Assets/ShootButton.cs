using UnityEngine;
using UnityEngine.EventSystems; 

public class ShootButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerShoot playerShootScript; // Aici vom trage Player-ul

    // Cand pui degetul pe ecran
    public void OnPointerDown(PointerEventData eventData)
    {
        if(playerShootScript != null)
        {
            playerShootScript.StartShooting(); // Porneste rafala
        }
    }

    // Cand ridici degetul de pe ecran
    public void OnPointerUp(PointerEventData eventData)
    {
        if(playerShootScript != null)
        {
            playerShootScript.StopShooting(); // Opreste rafala
        }
    }
}