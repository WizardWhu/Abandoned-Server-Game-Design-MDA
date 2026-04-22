using UnityEngine;

public class Door : MonoBehaviour
{
    bool isOpened = false;

    public void Open()
    {
        Debug.Log("Door Opened");
        if (isOpened) return;
        isOpened = true;
        gameObject.SetActive(false);
    }
}
