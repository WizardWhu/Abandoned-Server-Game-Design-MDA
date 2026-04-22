using UnityEngine;

public class NewMaterial : MonoBehaviour
{
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material completedMaterial;
    [SerializeField] private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer.sharedMaterial = startMaterial;
    }

    public void Activate()
    {
        meshRenderer.sharedMaterial = completedMaterial;
    }
}
