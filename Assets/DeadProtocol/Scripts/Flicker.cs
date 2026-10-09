using UnityEngine;

namespace DeadProtocol
{
    // Parpadeo de faroles y luces de alarma.
    public class Flicker : MonoBehaviour
    {
        Light l; float b; float o;
        void Start() { l = GetComponent<Light>(); b = l.intensity; o = Random.value * 100f; }
        void Update()
        {
            float n = Mathf.PerlinNoise(Time.time * 6f, o);
            float drop = Mathf.PerlinNoise(Time.time * 0.7f, o + 9f) > 0.82f ? 0.25f : 1f;
            l.intensity = b * (0.75f + 0.25f * n) * drop;
        }
    }
}
