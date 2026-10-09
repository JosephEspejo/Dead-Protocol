using UnityEngine;

namespace DeadProtocol
{
    public class Spin : MonoBehaviour
    {
        public float speed = 90f;
        void Update() { transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.World); }
    }

    public class Scroll : MonoBehaviour
    {
        public Vector2 speed;
        Renderer r;
        void Start() { r = GetComponent<Renderer>(); }
        void Update() { r.sharedMaterial.mainTextureOffset += speed * Time.deltaTime; }
    }

    public class Blink : MonoBehaviour
    {
        public float period = 1f;
        public Color on = Color.green;
        Renderer r; float o;
        void Start() { r = GetComponent<Renderer>(); o = Random.value * 5f; }
        void Update()
        {
            bool b = Mathf.Sin((Time.time + o) * 6.28318f / period) > 0f;
            r.sharedMaterial.SetColor("_EmissionColor", b ? on * 2f : Color.black);
        }
    }
}
