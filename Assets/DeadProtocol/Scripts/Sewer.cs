using UnityEngine;

namespace DeadProtocol
{
    // ZONA 4: Alcantarillas (z 300 a 390)
    public static class Sewer
    {
        public const float Z0 = 300f, Z1 = 390f;

        public static void Build()
        {
            Transform root = new GameObject("Escenario4_Alcantarillas").transform;
            float L = Z1 - Z0, cz = (Z0 + Z1) / 2f;
            Texture2D br = U.Bricks(new Color(0.30f, 0.32f, 0.27f), 9);
            Color tint = new Color(0.85f, 0.9f, 0.85f);

            // piso y pasarelas
            U.Prim(PrimitiveType.Cube, root, new Vector3(0f, -0.1f, cz), new Vector3(10.4f, 0.2f, L), U.Tex(br, new Vector2(4f, 37f), tint, 0.3f));
            for (int s = -1; s <= 1; s += 2)
                U.Prim(PrimitiveType.Cube, root, new Vector3(s * 3.525f, 0.2f, cz), new Vector3(2.65f, 0.4f, L), U.Tex(br, new Vector2(1f, 37f), tint, 0.3f));

            // canal de agua verde que fluye
            Material wm = U.Tex(U.Ripple(), new Vector2(2f, 37f), Color.white, 0.95f);
            wm.EnableKeyword("_EMISSION");
            wm.SetColor("_EmissionColor", new Color(0.03f, 0.12f, 0.05f));
            GameObject water = U.Prim(PrimitiveType.Cube, root, new Vector3(0f, 0.05f, cz), new Vector3(4.4f, 0.05f, L), wm);
            water.AddComponent<Scroll>().speed = new Vector2(0f, 0.15f);

            // paredes y boveda de ladrillo
            for (int s = -1; s <= 1; s += 2)
                U.Prim(PrimitiveType.Cube, root, new Vector3(s * 5.1f, 1.2f, cz), new Vector3(0.5f, 2.4f, L), U.Tex(br, new Vector2(37f, 1f), tint, 0.2f), true);
            Material arc = U.Tex(br, new Vector2(0.5f, 37f), tint, 0.2f);
            for (int i = 0; i <= 14; i++)
            {
                float th = Mathf.PI * i / 14f;
                Vector3 pos = new Vector3(Mathf.Cos(th) * 5.1f, 2.2f + Mathf.Sin(th) * 5.1f, cz);
                GameObject g = U.Prim(PrimitiveType.Cube, root, pos, new Vector3(1.25f, 0.5f, L), arc);
                g.transform.localRotation = Quaternion.Euler(0f, 0f, th * Mathf.Rad2Deg + 90f);
            }

            // tuberias oxidadas
            Material pipe = U.Mat(new Color(0.35f, 0.20f, 0.10f), 0f, 0.3f);
            float[,] pp = { { 4.5f, 1.7f }, { -4.5f, 2.3f }, { 2.3f, 6.2f }, { -2.3f, 6.2f } };
            for (int i = 0; i < 4; i++)
            {
                GameObject c = U.Prim(PrimitiveType.Cylinder, root, new Vector3(pp[i, 0], pp[i, 1], cz), new Vector3(0.35f, L / 2f, 0.35f), pipe);
                c.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            for (int z = 305; z < 390; z += 9)
            {
                U.Prim(PrimitiveType.Sphere, root, new Vector3(4.5f, 1.7f, z), new Vector3(0.6f, 0.6f, 0.6f), pipe);
                U.Prim(PrimitiveType.Sphere, root, new Vector3(-4.5f, 2.3f, z + 4f), new Vector3(0.6f, 0.6f, 0.6f), pipe);
            }

            // luces de emergencia verdes (y rojas)
            for (int k = 0; k < 6; k++)
            {
                int side = (k % 2 == 0) ? -1 : 1;
                float z = 312f + k * 15f;
                bool redLamp = (k == 3);
                Color c = redLamp ? new Color(1f, 0.1f, 0.1f) : new Color(0.3f, 1f, 0.4f);
                U.Prim(PrimitiveType.Sphere, root, new Vector3(side * 4.6f, 3.2f, z), new Vector3(0.35f, 0.35f, 0.35f), U.Mat(c, 2f));
                B.PLight(root, new Vector3(side * 4.0f, 3.2f, z), c, 11f, 3f, true);
            }

            // puentes sobre el canal con barandas
            float[] bz = { 325f, 360f };
            foreach (float z in bz)
            {
                B.Box(root, new Vector3(0f, 0.45f, z), new Vector3(4.6f, 0.1f, 1.6f), U.Mat(new Color(0.15f, 0.15f, 0.15f), 0f, 0.4f), false);
                for (int s = -1; s <= 1; s += 2)
                    B.Box(root, new Vector3(s * 2.2f, 1.0f, z), new Vector3(0.06f, 1.0f, 1.6f), U.Mat(new Color(0.1f, 0.1f, 0.1f)), false);
            }

            // escalera de pared, barriles y cajas
            Material iron = U.Mat(new Color(0.1f, 0.1f, 0.1f));
            B.Box(root, new Vector3(4.7f, 1.6f, 344.6f), new Vector3(0.05f, 3.2f, 0.05f), iron, false);
            B.Box(root, new Vector3(4.7f, 1.6f, 345.4f), new Vector3(0.05f, 3.2f, 0.05f), iron, false);
            for (int i = 0; i < 9; i++)
                B.Box(root, new Vector3(4.7f, 0.4f + i * 0.35f, 345f), new Vector3(0.05f, 0.05f, 0.8f), iron, false);
            Material barrel = U.Mat(new Color(0.25f, 0.28f, 0.12f), 0f, 0.3f);
            Material crate = U.Mat(new Color(0.3f, 0.2f, 0.1f));
            float[] dz = { 318f, 338f, 352f, 372f, 383f };
            for (int i = 0; i < dz.Length; i++)
            {
                int s = (i % 2 == 0) ? 1 : -1;
                U.Prim(PrimitiveType.Cylinder, root, new Vector3(s * 3.9f, 0.9f, dz[i]), new Vector3(0.6f, 0.5f, 0.6f), barrel, true);
                B.Box(root, new Vector3(-s * 3.9f, 0.7f, dz[i] + 3f), new Vector3(0.9f, 0.6f, 0.9f), crate, true);
            }

            // goteras
            float[,] dr = { { 1.5f, 330f }, { -2f, 348f }, { 0.5f, 372f } };
            for (int i = 0; i < 3; i++) Drip(root, new Vector3(dr[i, 0], 6.5f, dr[i, 1]));

            B.WallZ(root, Z0 + 0.35f, 5.4f, 3.5f, 7.5f, 0.3f, U.Tex(br, new Vector2(2f, 3f), tint, 0.2f));
        }

        static void Drip(Transform p, Vector3 pos)
        {
            GameObject g = new GameObject("Gotera");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            ParticleSystem ps = g.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 0f;
            main.startSize = 0.06f;
            main.startColor = new Color(0.6f, 0.8f, 0.7f, 0.8f);
            main.gravityModifier = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 1.5f;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(0.05f, 0.05f, 0.05f);
            ParticleSystemRenderer r = g.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Sprites/Default");
            Material m = s != null ? new Material(s) : U.Mat(Color.white);
            m.mainTexture = U.Soft();
            r.sharedMaterial = m;
            ps.Play();
        }
    }
}
