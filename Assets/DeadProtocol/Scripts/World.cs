using UnityEngine;

namespace DeadProtocol
{
    // ESCENARIO: "Calles en cuarentena" - calle antigua adoquinada, lluvia, niebla, faroles, autos, edificios.
    public static class World
    {
        public const float Len = 130f;
        public const float Half = 5f;
        public static Light Sun;
        public static ParticleSystem RainPS;

        public static void Build()
        {
            Transform root = new GameObject("Escenario_CallesEnCuarentena").transform;

            // Ambiente: nublado, lluvia, niebla
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.025f;
            RenderSettings.fogColor = new Color(0.30f, 0.27f, 0.23f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.30f, 0.28f);

            GameObject sunGo = new GameObject("Luz_Nublado");
            sunGo.transform.SetParent(root, false);
            sunGo.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.75f, 0.70f, 0.62f);
            sun.intensity = 0.7f;
            sun.shadows = LightShadows.Soft;
            Sun = sun;

            // Suelo adoquinado y veredas
            Texture2D cob = U.Cobble();
            U.Prim(PrimitiveType.Cube, root, new Vector3(0f, -0.1f, 56f), new Vector3(Half * 2f, 0.2f, 136f),
                U.Tex(cob, new Vector2(4f, 57f), Color.white, 0.45f));
            for (int s = -1; s <= 1; s += 2)
                U.Prim(PrimitiveType.Cube, root, new Vector3(s * (Half + 1.5f), 0.06f, 56f), new Vector3(3f, 0.25f, 136f),
                    U.Tex(cob, new Vector2(1.2f, 57f), new Color(0.8f, 0.8f, 0.85f), 0.3f));

            // Edificios a ambos lados
            System.Random rnd = new System.Random(11);
            Color[] walls = {
                new Color(0.62f, 0.42f, 0.28f), new Color(0.55f, 0.45f, 0.33f),
                new Color(0.50f, 0.33f, 0.25f), new Color(0.60f, 0.55f, 0.42f) };
            Texture2D[] fac = new Texture2D[walls.Length];
            for (int i = 0; i < walls.Length; i++) fac[i] = U.Facade(walls[i], i + 1);
            Material iron = U.Mat(new Color(0.10f, 0.10f, 0.10f));
            Material wood = U.Mat(new Color(0.20f, 0.12f, 0.07f));

            for (int side = -1; side <= 1; side += 2)
            {
                float z = -12f;
                while (z < 123.99f)
                {
                    float w = Mathf.Min(rnd.Next(7, 12), 124f - z);
                    float h = rnd.Next(9, 16);
                    float d = 9f;
                    float face = side * (Half + 3f);
                    float cx = side * (Half + 3f + d / 2f);
                    float cz = z + w / 2f;
                    Material m = U.Tex(fac[rnd.Next(0, walls.Length)], new Vector2(w / 6f, h / 7f), Color.white, 0.05f);
                    U.Prim(PrimitiveType.Cube, root, new Vector3(cx, h / 2f, cz), new Vector3(d, h, w), m, true);

                    if (rnd.NextDouble() < 0.6)
                    {
                        U.Prim(PrimitiveType.Cube, root, new Vector3(side * (Half + 3f - 0.65f), 4.2f, cz), new Vector3(1.3f, 0.15f, w * 0.55f), iron);
                        U.Prim(PrimitiveType.Cube, root, new Vector3(side * (Half + 3f - 1.25f), 4.7f, cz), new Vector3(0.05f, 0.9f, w * 0.55f), iron);
                    }
                    U.Prim(PrimitiveType.Cube, root, new Vector3(face, 1.2f, cz), new Vector3(0.12f, 2.4f, 1.6f), wood);
                    if (rnd.NextDouble() < 0.45)
                    {
                        Color sc = rnd.NextDouble() < 0.5 ? new Color(0.15f, 0.38f, 0.22f) : new Color(0.55f, 0.15f, 0.12f);
                        U.Prim(PrimitiveType.Cube, root, new Vector3(side * (Half + 3f - 0.4f), 3.4f, cz - 1f), new Vector3(0.8f, 0.06f, 0.06f), iron);
                        U.Prim(PrimitiveType.Cube, root, new Vector3(side * (Half + 3f - 0.5f), 3.05f, cz - 1f), new Vector3(0.8f, 0.55f, 0.05f), U.Mat(sc));
                    }
                    z += w;
                }
            }

            // Faroles
            for (float lz = 8f; lz < 120f; lz += 16f)
            {
                int side = (((int)(lz / 16f)) % 2 == 0) ? -1 : 1;
                Lamp(root, new Vector3(side * (Half + 0.8f), 0f, lz));
            }

            // Autos abandonados
            float[,] cars = { { -3.5f, 22f, 8f }, { 3.5f, 44f, -6f }, { -3.4f, 66f, 170f }, { 3.6f, 88f, 10f }, { -3.5f, 106f, -12f } };
            Color[] cc = { new Color(0.45f, 0.12f, 0.10f), new Color(0.20f, 0.25f, 0.30f), new Color(0.50f, 0.45f, 0.30f), new Color(0.15f, 0.20f, 0.15f), new Color(0.40f, 0.40f, 0.42f) };
            for (int i = 0; i < 5; i++) Car(root, new Vector3(cars[i, 0], 0f, cars[i, 1]), cars[i, 2], cc[i]);

            // Cajas / barricadas
            float[,] crates = { { 3.8f, 12f }, { -4.0f, 34f }, { 3.9f, 56f }, { -3.9f, 78f }, { 4.0f, 100f } };
            for (int i = 0; i < 5; i++)
            {
                GameObject c = U.Prim(PrimitiveType.Cube, root, new Vector3(crates[i, 0], 0.5f, crates[i, 1]), new Vector3(1f, 1f, 1f), U.Mat(new Color(0.35f, 0.24f, 0.12f)), true);
                c.transform.rotation = Quaternion.Euler(0f, i * 31f, 0f);
                GameObject c2 = U.Prim(PrimitiveType.Cube, root, new Vector3(crates[i, 0] * 0.9f, 1.4f, crates[i, 1] + 0.3f), new Vector3(0.8f, 0.8f, 0.8f), U.Mat(new Color(0.30f, 0.20f, 0.10f)), true);
                c2.transform.rotation = Quaternion.Euler(0f, i * 53f + 20f, 0f);
            }

            Decor(root);
            Bridge(root, 48f);
            Fence(root, -1, 0f, 30f);
            Fence(root, 1, 66f, 100f);
            CityGate(root, 124f);
        }

        public static void Lamp(Transform p, Vector3 pos)
        {
            GameObject g = new GameObject("Farol");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            Material iron = U.Mat(new Color(0.08f, 0.08f, 0.08f));
            U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(0f, 2.2f, 0f), new Vector3(0.14f, 2.2f, 0.14f), iron, true);
            U.Prim(PrimitiveType.Sphere, g.transform, new Vector3(0f, 4.6f, 0f), new Vector3(0.5f, 0.5f, 0.5f), U.Mat(new Color(1f, 0.8f, 0.45f), 1.6f));
            GameObject lg = new GameObject("Luz");
            lg.transform.SetParent(g.transform, false);
            lg.transform.localPosition = new Vector3(0f, 4.4f, 0f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.78f, 0.45f);
            l.range = 13f;
            l.intensity = 3f;
            l.shadows = LightShadows.None;
            lg.AddComponent<Flicker>();
        }

        public static void Car(Transform p, Vector3 pos, float yaw, Color col)
        {
            GameObject g = new GameObject("Auto");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            U.Prim(PrimitiveType.Cube, g.transform, new Vector3(0f, 0.6f, 0f), new Vector3(1.9f, 0.7f, 4.3f), U.Mat(col, 0f, 0.4f), true);
            U.Prim(PrimitiveType.Cube, g.transform, new Vector3(0f, 1.2f, -0.2f), new Vector3(1.7f, 0.6f, 2.3f), U.Mat(new Color(col.r * 0.5f, col.g * 0.5f, col.b * 0.5f), 0f, 0.5f), true);
            Material tire = U.Mat(new Color(0.05f, 0.05f, 0.05f));
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    GameObject w = U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(sx * 0.95f, 0.35f, sz * 1.3f), new Vector3(0.7f, 0.12f, 0.7f), tire);
                    w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
        }

        // Porton final: ZONA CERO
        static void Gate(Transform p, float z)
        {
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 3.5f, z), new Vector3(Half * 2f + 6f, 7f, 1.2f), U.Mat(new Color(0.22f, 0.22f, 0.24f)), true);
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 1.7f, z - 0.65f), new Vector3(3.2f, 3.4f, 0.2f), U.Mat(new Color(0.12f, 0.13f, 0.15f), 0f, 0.5f));
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 3.6f, z - 0.7f), new Vector3(3.2f, 0.12f, 0.22f), U.Mat(new Color(1f, 0.1f, 0.1f), 2f));
            GameObject lg = new GameObject("Alarma");
            lg.transform.SetParent(p, false);
            lg.transform.position = new Vector3(0f, 4f, z - 3f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.1f, 0.1f);
            l.range = 14f;
            l.intensity = 4f;
            lg.AddComponent<Flicker>();
        }

        // Charcos, barriles (algunos con fuego), basura, barreras de cuarentena y vapor de alcantarilla
        static void Decor(Transform root)
        {
            System.Random r = new System.Random(5);
            Material pud = U.Mat(new Color(0.04f, 0.05f, 0.07f), 0f, 0.95f);
            for (int i = 0; i < 16; i++)
            {
                float rad = 0.6f + (float)r.NextDouble() * 1.2f;
                U.Prim(PrimitiveType.Cylinder, root, new Vector3(-3.8f + (float)r.NextDouble() * 7.6f, 0.012f, 4f + i * 7.5f), new Vector3(rad, 0.004f, rad * 0.7f), pud);
            }

            Material barrel = U.Mat(new Color(0.35f, 0.14f, 0.08f), 0f, 0.3f);
            for (int i = 0; i < 10; i++)
            {
                int side = (i % 2 == 0) ? -1 : 1;
                GameObject b = U.Prim(PrimitiveType.Cylinder, root, new Vector3(side * (Half + 1.2f), 0.5f, 10f + i * 11f), new Vector3(0.6f, 0.5f, 0.6f), barrel, true);
                if (i == 3 || i == 8) Fire(root, b.transform.position + Vector3.up * 0.55f);
            }

            Material bag = U.Mat(new Color(0.05f, 0.05f, 0.06f), 0f, 0.5f);
            for (int i = 0; i < 14; i++)
            {
                int side = r.NextDouble() < 0.5 ? -1 : 1;
                U.Prim(PrimitiveType.Sphere, root, new Vector3(side * (Half + 0.6f + (float)r.NextDouble() * 1.6f), 0.2f, 3f + i * 8.5f), new Vector3(0.55f, 0.38f, 0.5f), bag);
            }

            float[] bz = { 16f, 60f, 92f };
            foreach (float z in bz)
            {
                Barrier(root, new Vector3(-2.7f, 0f, z), 10f);
                Barrier(root, new Vector3(2.7f, 0f, z), -10f);
                U.Prim(PrimitiveType.Cube, root, new Vector3(0f, 1.15f, z), new Vector3(5.4f, 0.1f, 0.01f), U.Mat(new Color(0.95f, 0.8f, 0.1f), 0.4f));
            }

            Vent(root, new Vector3(1.4f, 0f, 26f));
            Vent(root, new Vector3(-1.5f, 0f, 72f));
        }

        public static void Barrier(Transform p, Vector3 pos, float yaw)
        {
            GameObject g = new GameObject("Barrera");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            U.Prim(PrimitiveType.Cube, g.transform, new Vector3(0f, 0.55f, 0f), new Vector3(2.6f, 0.9f, 0.35f), U.Mat(new Color(0.85f, 0.85f, 0.85f)), true);
            U.Prim(PrimitiveType.Cube, g.transform, new Vector3(0f, 0.75f, 0f), new Vector3(2.62f, 0.25f, 0.37f), U.Mat(new Color(0.75f, 0.1f, 0.1f)));
        }

        public static void Fire(Transform p, Vector3 pos)
        {
            GameObject g = new GameObject("Fuego");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            U.Prim(PrimitiveType.Sphere, g.transform, Vector3.zero, new Vector3(0.45f, 0.35f, 0.45f), U.Mat(new Color(1f, 0.45f, 0.1f), 2.2f));
            Light l = g.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.5f, 0.15f);
            l.range = 10f;
            l.intensity = 4f;
            g.AddComponent<Flicker>();
        }

        static void Vent(Transform p, Vector3 pos)
        {
            U.Prim(PrimitiveType.Cylinder, p, new Vector3(pos.x, 0.012f, pos.z), new Vector3(0.9f, 0.01f, 0.9f), U.Mat(new Color(0.06f, 0.06f, 0.06f), 0f, 0.3f));
            GameObject g = new GameObject("Vapor");
            g.transform.SetParent(p, false);
            g.transform.position = pos + Vector3.up * 0.05f;
            g.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            ParticleSystem ps = g.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.startLifetime = 3.5f;
            main.startSpeed = 0.9f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
            main.maxParticles = 200;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 14f;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle = 12f;
            sh.radius = 0.2f;
            ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 2.2f));
            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gr = new Gradient();
            gr.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                       new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(gr);
            ParticleSystemRenderer r = g.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Sprites/Default");
            Material m = s != null ? new Material(s) : U.Mat(Color.white);
            m.mainTexture = U.Soft();
            r.sharedMaterial = m;
            ps.Play();
        }

        // Reja de hierro con pilares de piedra (como en la imagen de referencia)
        static void Fence(Transform p, int side, float z0, float z1)
        {
            Material iron = U.Mat(new Color(0.06f, 0.06f, 0.06f));
            Material stone = U.Mat(new Color(0.5f, 0.47f, 0.4f));
            float x = side * (Half + 2.4f), mid = (z0 + z1) / 2f, len = z1 - z0;
            for (float z = z0; z <= z1; z += 0.9f)
                U.Prim(PrimitiveType.Cube, p, new Vector3(x, 0.9f, z), new Vector3(0.05f, 1.8f, 0.05f), iron, true);
            U.Prim(PrimitiveType.Cube, p, new Vector3(x, 1.6f, mid), new Vector3(0.05f, 0.06f, len), iron, true);
            U.Prim(PrimitiveType.Cube, p, new Vector3(x, 0.35f, mid), new Vector3(0.05f, 0.06f, len), iron, true);
            U.Prim(PrimitiveType.Cube, p, new Vector3(x, 1.1f, z0), new Vector3(0.6f, 2.2f, 0.6f), stone, true);
            U.Prim(PrimitiveType.Cube, p, new Vector3(x, 1.1f, z1), new Vector3(0.6f, 2.2f, 0.6f), stone, true);
        }

        // Puente con arco entre los edificios
        static void Bridge(Transform p, float z)
        {
            Material stone = U.Mat(new Color(0.5f, 0.42f, 0.32f), 0f, 0.05f);
            float r = 5f;
            for (int i = 0; i <= 12; i++)
            {
                float th = Mathf.PI * i / 12f;
                Vector3 pos = new Vector3(Mathf.Cos(th) * r, 3f + Mathf.Sin(th) * r, z);
                GameObject g = U.Prim(PrimitiveType.Cube, p, pos, new Vector3(1.3f, 0.9f, 3.4f), stone);
                g.transform.localRotation = Quaternion.Euler(0f, 0f, th * Mathf.Rad2Deg + 90f);
            }
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 8.9f, z), new Vector3(2f * (Half + 3f), 1.2f, 3.4f), stone);
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 9.8f, z - 1.6f), new Vector3(2f * (Half + 3f), 0.8f, 0.25f), stone);
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 9.8f, z + 1.6f), new Vector3(2f * (Half + 3f), 0.8f, 0.25f), stone);
            for (int s = -1; s <= 1; s += 2)
                U.Prim(PrimitiveType.Cube, p, new Vector3(s * 6.5f, 5.65f, z), new Vector3(3f, 5.3f, 3.4f), stone);
        }

        // Torre del reloj al fondo, con almenas
        static void Tower(Transform p, float z)
        {
            Material stone = U.Mat(new Color(0.45f, 0.40f, 0.33f), 0f, 0.05f);
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 15f, z), new Vector3(10f, 30f, 10f), stone);
            for (int i = -2; i <= 2; i++)
                for (int sgn = -1; sgn <= 1; sgn += 2)
                {
                    U.Prim(PrimitiveType.Cube, p, new Vector3(i * 2.25f, 30.7f, z + sgn * 4.5f), new Vector3(1.2f, 1.4f, 1.2f), stone);
                    if (i > -2 && i < 2) U.Prim(PrimitiveType.Cube, p, new Vector3(sgn * 4.5f, 30.7f, z + i * 2.25f), new Vector3(1.2f, 1.4f, 1.2f), stone);
                }
            GameObject face = U.Prim(PrimitiveType.Cylinder, p, new Vector3(0f, 24f, z - 5.05f), new Vector3(4f, 0.1f, 4f), U.Mat(new Color(0.9f, 0.85f, 0.6f), 0.8f));
            face.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Material hand = U.Mat(new Color(0.05f, 0.05f, 0.05f));
            U.Prim(PrimitiveType.Cube, p, new Vector3(0f, 24.5f, z - 5.2f), new Vector3(0.15f, 1.4f, 0.05f), hand);
            GameObject h2 = U.Prim(PrimitiveType.Cube, p, new Vector3(0.45f, 24f, z - 5.2f), new Vector3(1.0f, 0.15f, 0.05f), hand);
            h2.transform.rotation = Quaternion.Euler(0f, 0f, 20f);
        }

        // Puerta de la ciudad: muro con abertura de 10 m hacia la plaza
        public static void CityGate(Transform p, float z)
        {
            Material stone = U.Mat(new Color(0.40f, 0.36f, 0.30f), 0f, 0.05f);
            B.WallZ(p, z, 25f, 5f, 8f, 2f, stone);
            B.Box(p, new Vector3(0f, 8.2f, z), new Vector3(14f, 2.4f, 2.4f), stone, false);
            for (int s = -1; s <= 1; s += 2)
                B.Box(p, new Vector3(s * 8f, 6f, z), new Vector3(4f, 12f, 4f), stone, true);
            B.Box(p, new Vector3(0f, 7.2f, z - 1.25f), new Vector3(8f, 0.12f, 0.1f), U.Mat(new Color(1f, 0.1f, 0.1f), 2f), false);
            B.PLight(p, new Vector3(0f, 6f, z - 4f), new Color(1f, 0.15f, 0.1f), 14f, 4f, true);
        }

        // Lluvia que sigue a la camara
        public static void Rain(Transform cam)
        {
            GameObject g = new GameObject("Lluvia");
            g.transform.SetParent(cam, false);
            g.transform.localPosition = new Vector3(0f, 9f, 10f);
            ParticleSystem ps = g.AddComponent<ParticleSystem>();
            RainPS = ps;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.startLifetime = 0.6f;
            main.startSpeed = 0f;
            main.startSize = 0.04f;
            main.startColor = new Color(0.75f, 0.80f, 0.90f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 4000;

            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 2200f;

            ParticleSystem.ShapeModule shp = ps.shape;
            shp.shapeType = ParticleSystemShapeType.Box;
            shp.scale = new Vector3(26f, 1f, 34f);

            ParticleSystem.VelocityOverLifetimeModule vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.space = ParticleSystemSimulationSpace.World;
            vol.x = new ParticleSystem.MinMaxCurve(0f);
            vol.y = new ParticleSystem.MinMaxCurve(-22f);
            vol.z = new ParticleSystem.MinMaxCurve(0f);

            ParticleSystemRenderer r = g.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.lengthScale = 1f;
            r.velocityScale = 0.06f;
            Shader s = Shader.Find("Sprites/Default");
            r.sharedMaterial = s != null ? new Material(s) : U.Mat(Color.white);
            ps.Play();
        }
    }
}
