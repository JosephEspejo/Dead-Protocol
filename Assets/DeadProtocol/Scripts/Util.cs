using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DeadProtocol
{
    // Utilidades: materiales (sin rosa en URP/Built-in), texturas y sonidos hechos por codigo, entrada.
    public static class U
    {
        static Shader sh;

        public static Material Mat(Color c, float emission = 0f, float gloss = 0.1f)
        {
            if (sh == null)
            {
                string[] names = { "Universal Render Pipeline/Lit", "Standard", "Unlit/Color", "Sprites/Default" };
                foreach (string n in names)
                {
                    sh = Shader.Find(n);
                    if (sh != null) break;
                }
            }
            Material m = new Material(sh);
            m.color = c;
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", gloss);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", gloss);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (emission > 0f && m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * emission);
            }
            return m;
        }

        public static Material Tex(Texture2D t, Vector2 tiling, Color tint, float gloss = 0.1f)
        {
            Material m = Mat(tint, 0f, gloss);
            m.mainTexture = t;
            m.mainTextureScale = tiling;
            return m;
        }

        public static GameObject Prim(PrimitiveType type, Transform parent, Vector3 lp, Vector3 ls, Material m, bool collider = false)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = lp;
            g.transform.localScale = ls;
            g.GetComponent<Renderer>().sharedMaterial = m;
            if (!collider)
            {
                Collider c = g.GetComponent<Collider>();
                if (c != null) Object.Destroy(c);
            }
            return g;
        }

        static float H(int x, int y)
        {
            return Mathf.Abs(Mathf.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f) % 1f;
        }

        // Adoquines de la calle
        public static Texture2D Cobble()
        {
            int n = 128, cell = 16;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, true);
            Color[] px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int row = y / cell;
                    int xs = (row % 2 == 1) ? (x + cell / 2) % n : x;
                    int cx = xs / cell;
                    float b = 0.55f + H(cx, row) * 0.45f;
                    bool gap = (xs % cell) < 2 || (y % cell) < 2;
                    float v = gap ? 0.10f : b * 0.5f;
                    px[y * n + x] = new Color(v * 1.05f, v * 0.95f, v * 0.85f, 1f);
                }
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return t;
        }

        // Fachada con ventanas (2 pisos por repeticion)
        public static Texture2D Facade(Color wall, int seed)
        {
            int w = 128, h = 256;
            Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, true);
            Color[] px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int fl = y / 128, yy = y % 128, wx = x % 64;
                    float g = 0.82f + H(x / 2 + seed, y / 2) * 0.18f;
                    Color c = new Color(wall.r * g, wall.g * g, wall.b * g, 1f);
                    bool win = wx >= 14 && wx <= 50 && yy >= 40 && yy <= 100;
                    bool frame = wx >= 11 && wx <= 53 && yy >= 37 && yy <= 103 && !win;
                    if (win)
                    {
                        bool on = H(x / 64 + seed * 3, fl * 7 + seed) > 0.8f;
                        c = on ? new Color(0.9f, 0.68f, 0.28f, 1f) : new Color(0.06f, 0.08f, 0.10f, 1f);
                        if (wx == 32 || yy == 70) c = new Color(wall.r * 0.4f, wall.g * 0.4f, wall.b * 0.4f, 1f);
                    }
                    else if (frame) c = new Color(wall.r * 0.5f, wall.g * 0.5f, wall.b * 0.5f, 1f);
                    if (yy < 8) c = new Color(wall.r * 0.55f, wall.g * 0.55f, wall.b * 0.55f, 1f);
                    px[y * w + x] = c;
                }
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return t;
        }

        // Sonidos generados por codigo (sin archivos de audio)
        public static AudioClip Noise(float dur, float freq, float noise, float decay)
        {
            int sr = 22050, n = (int)(sr * dur);
            float[] d = new float[n];
            System.Random rnd = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * decay);
                float s = Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - noise) + ((float)rnd.NextDouble() * 2f - 1f) * noise;
                d[i] = s * env * 0.6f;
            }
            AudioClip c = AudioClip.Create("gen", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        public static AudioClip RainLoop()
        {
            int sr = 22050, n = sr * 3;
            float[] d = new float[n];
            System.Random rnd = new System.Random(3);
            float v = 0f;
            for (int i = 0; i < n; i++)
            {
                float r = (float)rnd.NextDouble() * 2f - 1f;
                v = v * 0.6f + r * 0.4f;
                d[i] = v * 0.5f;
            }
            AudioClip c = AudioClip.Create("lluvia", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // Textura suave (circulo difuso) para el vapor
        public static Texture2D Soft()
        {
            int n = 32;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            Color[] px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x - n / 2f) / (n / 2f), dy = (y - n / 2f) / (n / 2f);
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    px[y * n + x] = new Color(1f, 1f, 1f, a * a);
                }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        // Entrada compatible con el Input System nuevo y el antiguo
        public static Vector2 MouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f;
#endif
        }

        public static Vector2 Axis()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return Vector2.zero;
            return new Vector2((k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f),
                               (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f));
#else
            return new Vector2((Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
                               (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f));
#endif
        }

        public static bool Shift()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif
        }

        public static bool Click()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool TourKey()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.C);
#endif
        }

        public static bool LightKey()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F);
#endif
        }

        public static Texture2D Checker(Color a, Color b)
        {
            int n = 64, sq = 8;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, true);
            Color[] px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    Color c = (((x / sq) + (y / sq)) % 2 == 0) ? a : b;
                    if (x % sq == 0 || y % sq == 0) c = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 1f);
                    px[y * n + x] = c;
                }
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return t;
        }

        public static Texture2D Bricks(Color c0, int seed)
        {
            int n = 64;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, true);
            Color[] px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int row = y / 8;
                    int xs = (row % 2 == 1) ? (x + 8) % n : x;
                    bool mortar = (xs % 16) < 1 || (y % 8) < 1;
                    float g = 0.7f + H(xs / 16 + seed, row) * 0.5f;
                    px[y * n + x] = mortar ? new Color(0.12f, 0.12f, 0.11f, 1f) : new Color(c0.r * g, c0.g * g, c0.b * g, 1f);
                }
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return t;
        }

        public static Texture2D Ripple()
        {
            int n = 64;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, true);
            Color[] px = new Color[n * n];
            Color dk = new Color(0.04f, 0.13f, 0.07f, 1f), lt = new Color(0.12f, 0.32f, 0.16f, 1f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = x / (float)n * 6.2832f, v = y / (float)n * 6.2832f;
                    float val = 0.5f + 0.25f * Mathf.Sin(u * 3f + Mathf.Sin(v * 2f) * 1.5f) + 0.25f * Mathf.Sin(v * 4f + u);
                    px[y * n + x] = Color.Lerp(dk, lt, val);
                }
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.Apply();
            return t;
        }

        public static bool MapKey()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.M);
#endif
        }

        public static int NumKey()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return 0;
            if (k.digit1Key.wasPressedThisFrame) return 1;
            if (k.digit2Key.wasPressedThisFrame) return 2;
            if (k.digit3Key.wasPressedThisFrame) return 3;
            if (k.digit4Key.wasPressedThisFrame) return 4;
            if (k.digit5Key.wasPressedThisFrame) return 5;
            return 0;
#else
            for (int i = 0; i < 5; i++)
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) return i + 1;
            return 0;
#endif
        }

        public static bool EscKey()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
