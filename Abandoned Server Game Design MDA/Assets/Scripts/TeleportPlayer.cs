using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    public void Teleport()
    {
        player.position = target.position;
    }
}
