using UnityEngine;

namespace DeadProtocol
{
    // ZONA 5: Laboratorio Zona Cero (z 390 a 470)
    public static class Lab
    {
        public const float Z0 = 390f, Z1 = 470f, HX = 14f, H = 6f;

        public static void Build()
        {
            Transform root = new GameObject("Escenario5_LaboratorioZonaCero").transform;
            Material wall = U.Mat(new Color(0.72f, 0.74f, 0.77f), 0f, 0.3f);
            Material dark = U.Mat(new Color(0.12f, 0.13f, 0.15f), 0f, 0.4f);
            Material metal = U.Mat(new Color(0.25f, 0.27f, 0.3f), 0f, 0.6f);
            float L = Z1 - Z0, cz = (Z0 + Z1) / 2f;

            B.Box(root, new Vector3(0f, -0.1f, cz), new Vector3(28f, 0.2f, L),
                U.Tex(U.Checker(new Color(0.78f, 0.80f, 0.82f), new Color(0.62f, 0.64f, 0.68f)), new Vector2(7f, 20f), Color.white, 0.55f), false);
            B.Box(root, new Vector3(0f, H + 0.15f, cz), new Vector3(29f, 0.3f, L), dark, false);
            B.WallX(root, -HX - 0.25f, Z0, Z1, H, 0.5f, wall);
            B.WallX(root, HX + 0.25f, Z0, Z1, H, 0.5f, wall);
            B.Box(root, new Vector3(0f, H / 2f, Z1 + 0.4f), new Vector3(29f, H, 0.8f), wall, true);

            // pared de entrada (desde las alcantarillas), con compuerta
            B.WallZ(root, Z0, HX + 0.5f, 3.5f, H + 1.5f, 0.8f, wall);
            B.Box(root, new Vector3(0f, 5.75f, Z0), new Vector3(7.2f, 3.5f, 0.8f), wall, false);
            for (int s = -1; s <= 1; s += 2)
                B.Box(root, new Vector3(s * 3.6f, 1.9f, Z0 + 0.45f), new Vector3(0.2f, 3.8f, 0.1f), U.Mat(new Color(1f, 0.7f, 0.1f), 1.2f), false);

            // luces del techo
            for (int i = 0; i < 8; i++)
            {
                float z = 396f + i * 10f;
                B.Box(root, new Vector3(0f, H - 0.05f, z), new Vector3(6f, 0.08f, 1.2f), U.Mat(new Color(0.85f, 0.95f, 1f), 1.6f), false);
                B.PLight(root, new Vector3(0f, H - 0.8f, z), new Color(0.75f, 0.9f, 1f), 14f, 2f, i == 2 || i == 5);
            }

            // lineas guia en el piso
            for (int s = -1; s <= 1; s += 2)
                B.Box(root, new Vector3(s * 3.8f, 0.012f, 419f), new Vector3(0.2f, 0.01f, 58f), U.Mat(s < 0 ? new Color(0.2f, 0.5f, 1f) : new Color(1f, 0.8f, 0.1f), 0.6f), false);

            // tanques de especimenes (dos filas)
            for (int k = 0; k < 5; k++)
            {
                Tank(root, new Vector3(-9f, 0f, 414f + k * 8f), k);
                Tank(root, new Vector3(9f, 0f, 418f + k * 8f), k + 3);
            }

            // servidores con luces intermitentes
            for (int k = 0; k < 10; k++)
            {
                int s = (k % 2 == 0) ? -1 : 1;
                float z = 408f + (k / 2) * 8f;
                B.Box(root, new Vector3(s * 13.2f, 1.2f, z), new Vector3(1.2f, 2.4f, 1.6f), dark, true);
                GameObject led = B.Box(root, new Vector3(s * 12.58f, 1.8f, z), new Vector3(0.04f, 0.5f, 1.2f), U.Mat(Color.green, 1.5f), false);
                Blink bl = led.AddComponent<Blink>();
                bl.period = 0.7f + k * 0.13f;
                bl.on = (k % 3 == 0) ? new Color(1f, 0.5f, 0.1f) : new Color(0.2f, 1f, 0.3f);
            }

            // escritorios con monitores
            for (int k = 0; k < 6; k++)
            {
                int s = (k % 2 == 0) ? -1 : 1;
                float z = 420f + (k / 2) * 11f;
                B.Box(root, new Vector3(s * 5.5f, 0.45f, z), new Vector3(3f, 0.9f, 1.2f), metal, true);
                B.Box(root, new Vector3(s * 5.5f, 1.2f, z - 0.25f), new Vector3(0.9f, 0.55f, 0.05f), U.Mat(new Color(0.3f, 0.8f, 1f), 1.2f), false);
            }

            Arena(root, new Vector3(0f, 0f, 459f), metal);

            // franja de peligro antes de la camara de contencion
            for (int i = 0; i < 24; i++)
            {
                bool y = i % 2 == 0;
                B.Box(root, new Vector3(-13.8f + i * 1.2f, 0.015f, 447f), new Vector3(1.2f, 0.01f, 0.8f),
                    U.Mat(y ? new Color(0.9f, 0.7f, 0.05f) : new Color(0.05f, 0.05f, 0.05f), y ? 0.5f : 0f), false);
            }
        }

        static void Tank(Transform p, Vector3 pos, int seed)
        {
            GameObject g = new GameObject("Tanque");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            Material metal = U.Mat(new Color(0.2f, 0.22f, 0.25f), 0f, 0.6f);
            Material glow = U.Mat(new Color(0.2f, 1f, 0.4f), 1.4f);
            U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(0f, 0.15f, 0f), new Vector3(1.9f, 0.15f, 1.9f), metal, true);
            U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(0f, 3.15f, 0f), new Vector3(1.9f, 0.15f, 1.9f), metal, false);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(Mathf.Cos(a) * 0.85f, 1.65f, Mathf.Sin(a) * 0.85f), new Vector3(0.08f, 1.5f, 0.08f), metal);
            }
            U.Prim(PrimitiveType.Sphere, g.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 0.3f, 0.3f), glow);
            U.Prim(PrimitiveType.Sphere, g.transform, new Vector3(0f, 2.7f, 0f), new Vector3(0.3f, 0.3f, 0.3f), glow);
            // silueta de infectado dentro del tanque
            Material sk = U.Mat(new Color(0.25f, 0.35f, 0.25f), 0.3f);
            U.Prim(PrimitiveType.Capsule, g.transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.5f, 0.75f, 0.4f), sk);
            U.Prim(PrimitiveType.Sphere, g.transform, new Vector3(0f, 2.4f, 0f), new Vector3(0.3f, 0.34f, 0.3f), sk);
            if (seed % 2 == 0) B.PLight(g.transform, pos + new Vector3(0f, 1.6f, 0f), new Color(0.2f, 1f, 0.4f), 7f, 1.6f, seed % 4 == 0);
        }

        static void Arena(Transform p, Vector3 c, Material metal)
        {
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 0.03f, new Vector3(18.8f, 0.03f, 18.8f), U.Mat(new Color(1f, 0.1f, 0.05f), 1.2f));
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 0.045f, new Vector3(17.6f, 0.03f, 17.6f), U.Mat(new Color(0.1f, 0.1f, 0.12f), 0f, 0.6f));
            // jaula de contencion con nucleo brillante
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 0.2f, new Vector3(5f, 0.2f, 5f), metal, true);
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 5.8f, new Vector3(5f, 0.2f, 5f), metal, false);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                U.Prim(PrimitiveType.Cylinder, p, c + new Vector3(Mathf.Cos(a) * 2.2f, 3f, Mathf.Sin(a) * 2.2f), new Vector3(0.24f, 2.8f, 0.24f), metal, true);
            }
            U.Prim(PrimitiveType.Sphere, p, c + Vector3.up * 2.8f, new Vector3(2.6f, 2.6f, 2.6f), U.Mat(new Color(1f, 0.25f, 0.1f), 2f));
            B.PLight(p, c + Vector3.up * 2.8f, new Color(1f, 0.25f, 0.1f), 16f, 4f, true);
            // alarmas giratorias
            GameObject piv = new GameObject("AlarmaGiratoria");
            piv.transform.SetParent(p, false);
            piv.transform.position = c + Vector3.up * 5.3f;
            piv.AddComponent<Spin>().speed = 120f;
            GameObject lg = new GameObject("Foco");
            lg.transform.SetParent(piv.transform, false);
            lg.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = new Color(1f, 0.1f, 0.1f);
            l.range = 22f;
            l.spotAngle = 40f;
            l.intensity = 6f;
            GameObject lg2 = new GameObject("Foco2");
            lg2.transform.SetParent(piv.transform, false);
            lg2.transform.localRotation = Quaternion.Euler(18f, 180f, 0f);
            Light l2 = lg2.AddComponent<Light>();
            l2.type = LightType.Spot;
            l2.color = new Color(1f, 0.1f, 0.1f);
            l2.range = 22f;
            l2.spotAngle = 40f;
            l2.intensity = 6f;
        }
    }
}
