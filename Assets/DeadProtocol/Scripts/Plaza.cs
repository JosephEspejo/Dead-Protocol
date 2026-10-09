using UnityEngine;

namespace DeadProtocol
{
    // ZONA 2: Plaza de la Fuente (z 124 a 210)
    public static class Plaza
    {
        public const float Z0 = 124f, Z1 = 210f, Half = 24f;

        public static void Build()
        {
            Transform root = new GameObject("Escenario2_PlazaDeLaFuente").transform;
            Material stone = U.Mat(new Color(0.48f, 0.44f, 0.38f), 0f, 0.1f);
            Texture2D cob = U.Cobble();

            U.Prim(PrimitiveType.Cube, root, new Vector3(0f, -0.1f, (Z0 + Z1) / 2f), new Vector3(Half * 2f, 0.2f, Z1 - Z0),
                U.Tex(cob, new Vector2(20f, 36f), new Color(0.9f, 0.9f, 0.95f), 0.45f));
            U.Prim(PrimitiveType.Cylinder, root, new Vector3(0f, 0.015f, 167f), new Vector3(20f, 0.005f, 20f),
                U.Tex(cob, new Vector2(8f, 8f), new Color(1f, 0.95f, 0.9f), 0.5f));

            B.Row(root, -1, Half, Z0, 168f, 21, 10, 18);
            B.Row(root, -1, Half, 202f, Z1, 22, 10, 16);
            B.Row(root, 1, Half, Z0, Z1, 23, 10, 18);

            Fountain(root, new Vector3(0f, 0f, 167f), stone);
            Church(root);

            // bancas alrededor de la fuente
            for (int i = 0; i < 6; i++)
            {
                float ang = i * 60f + 30f, rad = ang * Mathf.Deg2Rad;
                Bench(root, new Vector3(Mathf.Sin(rad) * 8f, 0f, 167f + Mathf.Cos(rad) * 8f), ang + 180f);
            }

            // arboles muertos
            float[,] trees = { { -17f, 140f }, { 17f, 138f }, { -17f, 156f }, { 17f, 178f }, { -17f, 192f }, { 17f, 196f }, { 6f, 203f }, { -8f, 204f } };
            for (int i = 0; i < trees.GetLength(0); i++) Tree(root, new Vector3(trees[i, 0], 0f, trees[i, 1]), i * 7);

            // puesto militar abandonado junto a la entrada
            Sandbags(root, new Vector3(-9f, 0f, 130f), 0f, 6);
            Sandbags(root, new Vector3(9f, 0f, 130f), 0f, 6);
            Tent(root, new Vector3(-17f, 0f, 140f), 90f);
            Tent(root, new Vector3(17f, 0f, 143f), 90f);
            World.Car(root, new Vector3(-13f, 0f, 152f), 35f, new Color(0.22f, 0.27f, 0.18f));
            World.Car(root, new Vector3(15f, 0f, 186f), -20f, new Color(0.22f, 0.27f, 0.18f));
            Material barrel = U.Mat(new Color(0.35f, 0.14f, 0.08f), 0f, 0.3f);
            float[] bx = { -7f, 7f };
            foreach (float x in bx)
            {
                GameObject b = U.Prim(PrimitiveType.Cylinder, root, new Vector3(x, 0.5f, 128f), new Vector3(0.6f, 0.5f, 0.6f), barrel, true);
                World.Fire(root, b.transform.position + Vector3.up * 0.55f);
            }

            // faroles
            float[] lz = { 136f, 154f, 180f, 198f };
            foreach (float z in lz)
            {
                World.Lamp(root, new Vector3(-13f, 0f, z));
                World.Lamp(root, new Vector3(13f, 0f, z));
            }

            B.Puddles(root, 31, 24, 20f, 128f, 207f, 167f, 7f);
        }

        static void Fountain(Transform p, Vector3 c, Material stone)
        {
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 0.4f, new Vector3(10f, 0.4f, 10f), stone, true);
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 0.62f, new Vector3(9.2f, 0.2f, 9.2f), U.Mat(new Color(0.35f, 0.04f, 0.04f), 0.25f, 0.95f));
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 1.9f, new Vector3(1.6f, 1.1f, 1.6f), stone, true);
            U.Prim(PrimitiveType.Cylinder, p, c + Vector3.up * 3.1f, new Vector3(3.2f, 0.15f, 3.2f), stone, false);
            U.Prim(PrimitiveType.Capsule, p, c + Vector3.up * 4.3f, new Vector3(0.8f, 1.2f, 0.8f), stone, false);
            U.Prim(PrimitiveType.Sphere, p, c + Vector3.up * 5.8f, new Vector3(0.6f, 0.6f, 0.6f), stone, false);
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject w = U.Prim(PrimitiveType.Cube, p, c + new Vector3(s * 0.7f, 5.0f, -0.25f), new Vector3(0.1f, 1.6f, 0.9f), stone, false);
                w.transform.rotation = Quaternion.Euler(0f, 0f, -s * 20f);
            }
            B.PLight(p, c + new Vector3(0f, 2.5f, 0f), new Color(1f, 0.25f, 0.2f), 14f, 2f, false);
        }

        static void Church(Transform root)
        {
            Material st = U.Mat(new Color(0.42f, 0.40f, 0.36f), 0f, 0.05f);
            Material roof = U.Mat(new Color(0.20f, 0.14f, 0.12f));
            float x = -(Half + 6f), tx = -(Half + 3.5f);
            B.Box(root, new Vector3(x, 7f, 186f), new Vector3(12f, 14f, 34f), st, true);
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject r = B.Box(root, new Vector3(x + s * 3f * -1f, 16f, 186f), new Vector3(7.2f, 0.4f, 34f), roof, false);
                r.transform.rotation = Quaternion.Euler(0f, 0f, s * 33f);
            }
            B.Box(root, new Vector3(tx, 15f, 172f), new Vector3(7f, 30f, 7f), st, true);
            float[] offs = { -2.6f, -0.87f, 0.87f, 2.6f };
            foreach (float o in offs)
            {
                B.Box(root, new Vector3(tx + 3f, 30.7f, 172f + o), new Vector3(1.2f, 1.3f, 1.2f), st, false);
                B.Box(root, new Vector3(tx - 3f, 30.7f, 172f + o), new Vector3(1.2f, 1.3f, 1.2f), st, false);
                B.Box(root, new Vector3(tx + o, 30.7f, 172f + 3f), new Vector3(1.2f, 1.3f, 1.2f), st, false);
                B.Box(root, new Vector3(tx + o, 30.7f, 172f - 3f), new Vector3(1.2f, 1.3f, 1.2f), st, false);
            }
            GameObject face = U.Prim(PrimitiveType.Cylinder, root, new Vector3(-23.95f, 22f, 172f), new Vector3(4f, 0.1f, 4f), U.Mat(new Color(0.9f, 0.85f, 0.6f), 0.7f));
            face.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Material hand = U.Mat(new Color(0.05f, 0.05f, 0.05f));
            B.Box(root, new Vector3(-23.85f, 22.7f, 172f), new Vector3(0.05f, 1.5f, 0.15f), hand, false);
            B.Box(root, new Vector3(-23.85f, 22f, 172.45f), new Vector3(0.05f, 0.15f, 1.0f), hand, false);

            Color[] wc = { new Color(0.9f, 0.1f, 0.1f), new Color(0.15f, 0.3f, 0.9f), new Color(0.95f, 0.65f, 0.1f), new Color(0.1f, 0.7f, 0.3f) };
            for (int k = 0; k < 6; k++)
                B.Box(root, new Vector3(-23.95f, 8f, 178f + k * 4.4f), new Vector3(0.1f, 5f, 1.4f), U.Mat(wc[k % 4], 1.0f), false);
            B.Box(root, new Vector3(-23.95f, 2.5f, 186f), new Vector3(0.15f, 5f, 3f), U.Mat(new Color(0.15f, 0.08f, 0.04f)), false);
            B.PLight(root, new Vector3(-21.5f, 8f, 181f), new Color(0.9f, 0.3f, 0.2f), 9f, 1.5f, false);
            B.PLight(root, new Vector3(-21.5f, 8f, 195f), new Color(0.3f, 0.4f, 0.95f), 9f, 1.5f, false);
        }

        static void Bench(Transform p, Vector3 pos, float yaw)
        {
            GameObject g = new GameObject("Banca");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Material wood = U.Mat(new Color(0.22f, 0.14f, 0.08f));
            Material iron = U.Mat(new Color(0.08f, 0.08f, 0.08f));
            B.Box(g.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2f, 0.1f, 0.6f), wood, true);
            B.Box(g.transform, new Vector3(0f, 0.85f, -0.27f), new Vector3(2f, 0.45f, 0.08f), wood, false);
            B.Box(g.transform, new Vector3(-0.85f, 0.25f, 0f), new Vector3(0.08f, 0.5f, 0.5f), iron, false);
            B.Box(g.transform, new Vector3(0.85f, 0.25f, 0f), new Vector3(0.08f, 0.5f, 0.5f), iron, false);
        }

        static void Tree(Transform p, Vector3 pos, int seed)
        {
            Material bark = U.Mat(new Color(0.13f, 0.10f, 0.08f), 0f, 0.05f);
            U.Prim(PrimitiveType.Cylinder, p, pos + Vector3.up * 2f, new Vector3(0.35f, 2f, 0.35f), bark, true);
            System.Random r = new System.Random(seed);
            for (int k = 0; k < 5; k++)
            {
                GameObject piv = new GameObject("Rama");
                piv.transform.SetParent(p, false);
                piv.transform.position = pos + Vector3.up * (2.4f + k * 0.4f);
                piv.transform.rotation = Quaternion.Euler(-35f - k * 6f, k * 72f + (float)r.NextDouble() * 30f, 0f);
                U.Prim(PrimitiveType.Cube, piv.transform, new Vector3(0f, 0f, 0.9f), new Vector3(0.12f, 0.12f, 1.8f), bark);
            }
        }

        static void Sandbags(Transform p, Vector3 c, float yaw, int n)
        {
            GameObject g = new GameObject("Sacos");
            g.transform.SetParent(p, false);
            g.transform.position = c;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Material m = U.Mat(new Color(0.45f, 0.40f, 0.28f), 0f, 0.05f);
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < n - row; i++)
                    B.Box(g.transform, new Vector3((i - (n - row - 1) / 2f) * 1.05f, 0.18f + row * 0.34f, 0f), new Vector3(1.0f, 0.33f, 0.5f), m, true);
        }

        static void Tent(Transform p, Vector3 c, float yaw)
        {
            GameObject g = new GameObject("Carpa");
            g.transform.SetParent(p, false);
            g.transform.position = c;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Material m = U.Mat(new Color(0.25f, 0.28f, 0.18f), 0f, 0.05f);
            GameObject a = B.Box(g.transform, new Vector3(-1f, 0.8f, 0f), new Vector3(2.6f, 0.05f, 5f), m, false);
            a.transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
            GameObject b = B.Box(g.transform, new Vector3(1f, 0.8f, 0f), new Vector3(2.6f, 0.05f, 5f), m, false);
            b.transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
            B.Box(g.transform, new Vector3(0f, 0.03f, 0f), new Vector3(3.2f, 0.05f, 5f), U.Mat(new Color(0.12f, 0.12f, 0.1f)), false);
        }
    }
}
