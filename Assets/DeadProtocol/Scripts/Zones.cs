using UnityEngine;

namespace DeadProtocol
{
    public struct Atm { public Color fog, amb; public float dens, sun, rain; }

    public class ZoneInfo
    {
        public string name, sub;
        public float z0, z1, half;
        public Atm a;
        public Color map;
    }

    // Datos del MAPA COMPLETO: 5 zonas conectadas a lo largo del eje Z
    public static class Zones
    {
        public static ZoneInfo[] All;

        static ZoneInfo Z(string n, string s, float z0, float z1, float half, Color map, Color fog, float dens, Color amb, float sun, float rain)
        {
            ZoneInfo zi = new ZoneInfo();
            zi.name = n; zi.sub = s; zi.z0 = z0; zi.z1 = z1; zi.half = half; zi.map = map;
            zi.a = new Atm { fog = fog, dens = dens, amb = amb, sun = sun, rain = rain };
            return zi;
        }

        static Zones()
        {
            All = new ZoneInfo[] {
                Z("Calles en cuarentena", "Ciudad antigua bajo la lluvia: calles adoquinadas, autos abandonados, puente con arco, rejas y barreras. Inicio de la misión.",
                  -12f, 124f, 7f, new Color(0.55f, 0.40f, 0.28f), new Color(0.30f, 0.27f, 0.23f), 0.025f, new Color(0.32f, 0.30f, 0.28f), 0.7f, 1f),
                Z("Plaza de la Fuente", "Gran plaza con fuente contaminada, iglesia con torre del reloj, puesto militar abandonado y árboles muertos.",
                  124f, 210f, 24f, new Color(0.50f, 0.50f, 0.45f), new Color(0.34f, 0.30f, 0.27f), 0.018f, new Color(0.34f, 0.32f, 0.30f), 0.8f, 1f),
                Z("Hospital San Lázaro", "Vestíbulo, pasillo con habitaciones, camillas y sangre. Luces que parpadean. Salida a las alcantarillas.",
                  210f, 300f, 11f, new Color(0.45f, 0.60f, 0.50f), new Color(0.12f, 0.16f, 0.14f), 0.035f, new Color(0.20f, 0.24f, 0.22f), 0f, 0f),
                Z("Alcantarillas", "Túnel de ladrillo con canal de agua verde, tuberías, pasarelas y luces de emergencia.",
                  300f, 390f, 5f, new Color(0.25f, 0.45f, 0.28f), new Color(0.05f, 0.09f, 0.06f), 0.05f, new Color(0.10f, 0.16f, 0.10f), 0f, 0f),
                Z("Laboratorio Zona Cero", "Laboratorio secreto con tanques de especímenes, servidores y la cámara de contención final.",
                  390f, 470f, 14f, new Color(0.40f, 0.50f, 0.75f), new Color(0.10f, 0.13f, 0.18f), 0.03f, new Color(0.30f, 0.34f, 0.42f), 0f, 0f)
            };
        }

        public static float StartZ(int i) { return i == 0 ? -6f : All[i].z0 + 5f; }

        public static int IndexAt(float z)
        {
            for (int i = 0; i < All.Length; i++) if (z < All[i].z1) return i;
            return All.Length - 1;
        }

        static Atm Mix(Atm a, Atm b, float t)
        {
            Atm r = new Atm();
            r.fog = Color.Lerp(a.fog, b.fog, t);
            r.amb = Color.Lerp(a.amb, b.amb, t);
            r.dens = Mathf.Lerp(a.dens, b.dens, t);
            r.sun = Mathf.Lerp(a.sun, b.sun, t);
            r.rain = Mathf.Lerp(a.rain, b.rain, t);
            return r;
        }

        public static Atm Sample(float z)
        {
            for (int i = 1; i < All.Length; i++)
            {
                float b = All[i].z0;
                if (z > b - 8f && z < b + 8f) return Mix(All[i - 1].a, All[i].a, Mathf.InverseLerp(b - 8f, b + 8f, z));
            }
            return All[IndexAt(z)].a;
        }

        public static float HalfAt(float z)
        {
            for (int i = 1; i < All.Length; i++)
                if (Mathf.Abs(z - All[i].z0) < 2.5f) return 3.2f;
            return All[IndexAt(z)].half;
        }

        public static void BuildAll()
        {
            GameObject g = new GameObject("SueloColision");
            BoxCollider bc = g.AddComponent<BoxCollider>();
            bc.size = new Vector3(120f, 1f, 520f);
            g.transform.position = new Vector3(0f, -0.5f, 229f);
            GameObject s = new GameObject("MuroInicio");
            BoxCollider sb = s.AddComponent<BoxCollider>();
            sb.size = new Vector3(20f, 6f, 1f);
            s.transform.position = new Vector3(0f, 3f, -10.5f);

            World.Build();
            Plaza.Build();
            Hospital.Build();
            Sewer.Build();
            Lab.Build();
        }
    }

    // Ayudas de construccion compartidas
    public static class B
    {
        static Texture2D[] fac;
        static readonly Color[] cols = {
            new Color(0.62f, 0.42f, 0.28f), new Color(0.55f, 0.45f, 0.33f),
            new Color(0.50f, 0.33f, 0.25f), new Color(0.60f, 0.55f, 0.42f) };

        public static GameObject Box(Transform p, Vector3 c, Vector3 s, Material m, bool col = true)
        {
            return U.Prim(PrimitiveType.Cube, p, c, s, m, col);
        }

        // Muro atravesado en X, a la altura z, con abertura central
        public static void WallZ(Transform p, float z, float outer, float gap, float h, float th, Material m)
        {
            float w = outer - gap;
            for (int s = -1; s <= 1; s += 2)
                Box(p, new Vector3(s * (gap + w / 2f), h / 2f, z), new Vector3(w, h, th), m, true);
        }

        public static void WallX(Transform p, float x, float z0, float z1, float h, float th, Material m)
        {
            Box(p, new Vector3(x, h / 2f, (z0 + z1) / 2f), new Vector3(th, h, z1 - z0), m, true);
        }

        public static Light PLight(Transform p, Vector3 pos, Color c, float range, float inten, bool flick)
        {
            GameObject g = new GameObject("Luz");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            Light l = g.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.range = range;
            l.intensity = inten;
            l.shadows = LightShadows.None;
            if (flick) g.AddComponent<Flicker>();
            return l;
        }

        // Fila de edificios con fachada (para la plaza)
        public static void Row(Transform p, int side, float faceX, float z0, float z1, int seed, int hMin, int hMax)
        {
            if (fac == null)
            {
                fac = new Texture2D[cols.Length];
                for (int i = 0; i < cols.Length; i++) fac[i] = U.Facade(cols[i], i + 1);
            }
            System.Random r = new System.Random(seed);
            Material iron = U.Mat(new Color(0.10f, 0.10f, 0.10f));
            Material door = U.Mat(new Color(0.20f, 0.12f, 0.07f));
            float z = z0;
            while (z < z1 - 0.01f)
            {
                float w = Mathf.Min(r.Next(7, 12), z1 - z);
                float h = r.Next(hMin, hMax);
                float d = 9f;
                float cz = z + w / 2f;
                Material m = U.Tex(fac[r.Next(0, cols.Length)], new Vector2(w / 6f, h / 7f), Color.white, 0.05f);
                U.Prim(PrimitiveType.Cube, p, new Vector3(side * (faceX + d / 2f), h / 2f, cz), new Vector3(d, h, w), m, true);
                if (r.NextDouble() < 0.5)
                {
                    U.Prim(PrimitiveType.Cube, p, new Vector3(side * (faceX - 0.65f), 4.2f, cz), new Vector3(1.3f, 0.15f, w * 0.55f), iron);
                    U.Prim(PrimitiveType.Cube, p, new Vector3(side * (faceX - 1.25f), 4.7f, cz), new Vector3(0.05f, 0.9f, w * 0.55f), iron);
                }
                U.Prim(PrimitiveType.Cube, p, new Vector3(side * faceX, 1.2f, cz), new Vector3(0.12f, 2.4f, 1.6f), door);
                z += w;
            }
        }

        public static void Puddles(Transform p, int seed, int n, float xHalf, float z0, float z1, float avoidZ, float avoidR)
        {
            System.Random r = new System.Random(seed);
            Material pud = U.Mat(new Color(0.04f, 0.05f, 0.07f), 0f, 0.95f);
            for (int i = 0; i < n; i++)
            {
                float x = (float)(r.NextDouble() * 2.0 - 1.0) * xHalf;
                float z = z0 + (float)r.NextDouble() * (z1 - z0);
                if (avoidR > 0f && Mathf.Sqrt(x * x + (z - avoidZ) * (z - avoidZ)) < avoidR) continue;
                float rad = 0.6f + (float)r.NextDouble() * 1.4f;
                U.Prim(PrimitiveType.Cylinder, p, new Vector3(x, 0.012f, z), new Vector3(rad, 0.004f, rad * 0.7f), pud);
            }
        }
    }
}
