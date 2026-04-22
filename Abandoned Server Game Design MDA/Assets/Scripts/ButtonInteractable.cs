using UnityEngine;
using UnityEngine.Events;

public class ButtonInteracable : Interactable
{
    public UnityEvent OnPressed;
    public override void Interact(CCPlayer ccPlayer)
    {
        Debug.Log("Button Pressed");
        OnPressed?.Invoke();
    }
}
