using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private float Slowness;
    private float currentCounter;
    [SerializeField] private float max;
    [SerializeField] private float min;
    private Light currentLight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLight = GetComponent<Light>();
        currentCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        currentCounter++;
        if(currentCounter > Slowness * 60)
        {
            currentCounter = 0;
            currentLight.intensity = Random.Range(min, max);
        }
    }
}
