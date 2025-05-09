using UnityEngine;

public class cam : MonoBehaviour
{
    void Start()
    {
        if (RenderSettings.sun == null)
        {
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            RenderSettings.sun = light;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
