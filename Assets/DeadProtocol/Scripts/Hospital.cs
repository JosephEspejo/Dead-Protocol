using UnityEngine;

namespace DeadProtocol
{
    // ZONA 3: Hospital San Lazaro (z 210 a 300)
    public static class Hospital
    {
        public const float Z0 = 210f, Z1 = 300f, HX = 11f, H = 4.6f;

        public static void Build()
        {
            Transform root = new GameObject("Escenario3_HospitalSanLazaro").transform;
            Material wall = U.Mat(new Color(0.55f, 0.58f, 0.52f), 0f, 0.1f);
            Material dark = U.Mat(new Color(0.10f, 0.10f, 0.11f));
            Material stone = U.Mat(new Color(0.46f, 0.42f, 0.36f), 0f, 0.05f);
            Material blood = U.Mat(new Color(0.32f, 0.02f, 0.02f), 0f, 0.8f);
            Material red = U.Mat(new Color(1f, 0.05f, 0.05f), 1.8f);
            float L = Z1 - Z0, cz = (Z0 + Z1) / 2f;

            // Fachada hacia la plaza con cruz roja
            B.WallZ(root, Z0, Plaza.Half + 1f, 6f, 14f, 1.5f, stone);
            B.Box(root, new Vector3(0f, 10f, Z0), new Vector3(14f, 8f, 1.5f), stone, false);
            B.Box(root, new Vector3(0f, 10f, Z0 - 0.8f), new Vector3(3.4f, 1.0f, 0.1f), red, false);
            B.Box(root, new Vector3(0f, 10f, Z0 - 0.8f), new Vector3(1.0f, 3.4f, 0.1f), red, false);
            float[] cx = { -11.5f, -7.8f, 7.8f, 11.5f };
            foreach (float x in cx)
                U.Prim(PrimitiveType.Cylinder, root, new Vector3(x, 3f, Z0 - 1.6f), new Vector3(1f, 3f, 1f), stone, true);

            // Paredes, techo y piso
            B.WallX(root, -HX, Z0, Z1, H, 0.5f, wall);
            B.WallX(root, HX, Z0, Z1, H, 0.5f, wall);
            B.Box(root, new Vector3(0f, H + 0.15f, cz), new Vector3(22.5f, 0.3f, L), dark, false);
            B.Box(root, new Vector3(0f, -0.1f, cz), new Vector3(22f, 0.2f, L),
                U.Tex(U.Checker(new Color(0.62f, 0.66f, 0.60f), new Color(0.45f, 0.50f, 0.45f)), new Vector2(5.5f, 22.5f), Color.white, 0.35f), false);

            // ---- Vestibulo (210-236)
            B.Box(root, new Vector3(-5.5f, 0.55f, 224f), new Vector3(5f, 1.1f, 1.2f), wall, true);
            B.Box(root, new Vector3(-5.5f, 1.15f, 224f), new Vector3(5.4f, 0.1f, 1.4f), dark, false);
            System.Random r = new System.Random(77);
            for (int row = 0; row < 2; row++)
                for (int i = 0; i < 4; i++)
                    Chair(root, new Vector3(5f + i * 1.3f, 0f, 216f + row * 2.2f), (float)(r.NextDouble() * 40.0 - 20.0), r.NextDouble() < 0.25);
            Wheelchair(root, new Vector3(-3f, 0f, 218f), 25f);
            Wheelchair(root, new Vector3(2f, 0f, 229f), -40f);
            Wheelchair(root, new Vector3(-8f, 0f, 231f), 110f);
            Panel(root, new Vector3(-5f, H - 0.05f, 216f)); Panel(root, new Vector3(5f, H - 0.05f, 216f));
            Panel(root, new Vector3(-5f, H - 0.05f, 228f)); Panel(root, new Vector3(5f, H - 0.05f, 228f));
            B.PLight(root, new Vector3(0f, 4f, 216f), new Color(0.8f, 1f, 0.9f), 13f, 2f, false);
            B.PLight(root, new Vector3(0f, 4f, 229f), new Color(0.8f, 1f, 0.9f), 13f, 2f, true);
            B.WallZ(root, 236f, HX, 3.5f, H, 0.5f, wall);

            // ---- Pasillo con habitaciones (236-291)
            Material door = U.Mat(new Color(0.25f, 0.2f, 0.15f));
            for (int side = -1; side <= 1; side += 2)
                for (int k = 0; k < 5; k++)
                {
                    float zs = 236f + k * 11f, ze = zs + 11f, zc = zs + 5.5f;
                    float wx = side * 3.5f;
                    B.Box(root, new Vector3(wx, H / 2f, (zs + zc - 1.2f) / 2f), new Vector3(0.3f, H, zc - 1.2f - zs), wall, true);
                    B.Box(root, new Vector3(wx, H / 2f, (zc + 1.2f + ze) / 2f), new Vector3(0.3f, H, ze - zc - 1.2f), wall, true);
                    B.Box(root, new Vector3(wx, 3f + (H - 3f) / 2f, zc), new Vector3(0.3f, H - 3f, 2.4f), wall, false);
                    B.Box(root, new Vector3(side * 7.25f, H / 2f, ze), new Vector3(7.5f, H, 0.3f), wall, true);
                    // contenido de la habitacion
                    Bed(root, new Vector3(side * 8.4f, 0f, zc), 90f, (k + side) % 2 == 0 ? new Color(0.55f, 0.1f, 0.1f) : new Color(0.7f, 0.72f, 0.7f));
                    B.Box(root, new Vector3(side * 10.1f, 0.25f, zc + 1.1f), new Vector3(0.5f, 0.5f, 0.5f), dark, true);
                    IV(root, new Vector3(side * 7.2f, 0f, zc - 1.2f));
                    Blood(root, blood, side * (5f + (float)r.NextDouble() * 3f), zc + (float)r.NextDouble() * 2f - 1f, 0.5f + (float)r.NextDouble());
                    B.PLight(root, new Vector3(side * 7f, 3.8f, zc), new Color(0.7f, 1f, 0.8f), 6.5f, 1.5f, (k + side + 5) % 3 == 0);
                }
            for (int k = 0; k < 5; k++)
            {
                float z = 241.5f + k * 11f;
                Panel(root, new Vector3(0f, H - 0.05f, z));
                B.PLight(root, new Vector3(0f, 4f, z), new Color(0.75f, 1f, 0.85f), 12f, 1.8f, k == 1 || k == 3);
            }
            // rastro de sangre y camilla en el pasillo
            for (int i = 0; i < 14; i++)
                Blood(root, blood, Mathf.Sin(i * 0.8f) * 1.2f, 246f + i * 3.2f, 0.25f + (i % 3) * 0.12f);
            Bed(root, new Vector3(-1.6f, 0f, 262f), 20f, new Color(0.7f, 0.72f, 0.7f));
            for (int i = 0; i < 12; i++)
                B.Box(root, new Vector3((float)(r.NextDouble() * 4.0 - 2.0), 0.012f, 238f + (float)r.NextDouble() * 52f), new Vector3(0.3f, 0.01f, 0.22f), U.Mat(new Color(0.85f, 0.85f, 0.8f)), false).transform.rotation = Quaternion.Euler(0f, (float)r.NextDouble() * 180f, 0f);

            // ---- Sala final y salida a las alcantarillas
            Bed(root, new Vector3(-6f, 0f, 294f), 20f, new Color(0.6f, 0.1f, 0.1f));
            Bed(root, new Vector3(7f, 0f, 296f), -35f, new Color(0.7f, 0.72f, 0.7f));
            Panel(root, new Vector3(0f, H - 0.05f, 295f));
            B.PLight(root, new Vector3(0f, 4f, 294f), new Color(0.7f, 1f, 0.8f), 12f, 1.6f, true);
            B.WallZ(root, Z1, HX, 3.5f, H, 0.6f, wall);
            B.Box(root, new Vector3(0f, 3.2f + (H - 3.2f) / 2f, Z1), new Vector3(7.2f, H - 3.2f, 0.6f), wall, false);
            B.Box(root, new Vector3(0f, 3.3f, Z1 - 0.4f), new Vector3(0.6f, 0.25f, 0.1f), U.Mat(new Color(0.2f, 1f, 0.35f), 2f), false);
            B.Box(root, new Vector3(0f, 1.2f, Z1 - 0.35f), new Vector3(7f, 0.1f, 0.02f), U.Mat(new Color(0.95f, 0.8f, 0.1f), 0.4f), false);
            B.PLight(root, new Vector3(0f, 3.4f, Z1 - 1.5f), new Color(0.3f, 1f, 0.4f), 9f, 3f, true);
        }

        static void Panel(Transform p, Vector3 pos)
        {
            B.Box(p, pos, new Vector3(2.5f, 0.08f, 0.5f), U.Mat(new Color(0.85f, 1f, 0.9f), 1.5f), false);
        }

        static void Blood(Transform p, Material m, float x, float z, float r)
        {
            U.Prim(PrimitiveType.Cylinder, p, new Vector3(x, 0.012f, z), new Vector3(r, 0.004f, r * 0.7f), m);
        }

        static void Chair(Transform p, Vector3 pos, float yaw, bool tipped)
        {
            GameObject g = new GameObject("Silla");
            g.transform.SetParent(p, false);
            g.transform.position = tipped ? pos + Vector3.up * 0.25f : pos;
            g.transform.rotation = Quaternion.Euler(tipped ? 80f : 0f, yaw, 0f);
            Material m = U.Mat(new Color(0.2f, 0.25f, 0.3f));
            B.Box(g.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.08f, 0.5f), m, false);
            B.Box(g.transform, new Vector3(0f, 0.75f, -0.23f), new Vector3(0.5f, 0.5f, 0.06f), m, false);
            B.Box(g.transform, new Vector3(0f, 0.21f, 0f), new Vector3(0.4f, 0.42f, 0.4f), U.Mat(new Color(0.08f, 0.08f, 0.08f)), !tipped);
        }

        static void Wheelchair(Transform p, Vector3 pos, float yaw)
        {
            GameObject g = new GameObject("SillaDeRuedas");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Material m = U.Mat(new Color(0.1f, 0.1f, 0.12f), 0f, 0.4f);
            B.Box(g.transform, new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.06f, 0.5f), m, true);
            B.Box(g.transform, new Vector3(0f, 0.85f, -0.24f), new Vector3(0.5f, 0.7f, 0.05f), m, false);
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject w = U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(s * 0.3f, 0.3f, -0.1f), new Vector3(0.6f, 0.03f, 0.6f), m);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        static void Bed(Transform p, Vector3 pos, float yaw, Color blanket)
        {
            GameObject g = new GameObject("Cama");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Material metal = U.Mat(new Color(0.2f, 0.2f, 0.22f), 0f, 0.5f);
            B.Box(g.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1.0f, 0.1f, 2.1f), metal, true);
            B.Box(g.transform, new Vector3(0f, 0.65f, -1.05f), new Vector3(1.0f, 0.9f, 0.08f), metal, false);
            B.Box(g.transform, new Vector3(0f, 0.43f, 0f), new Vector3(0.95f, 0.16f, 2.0f), U.Mat(new Color(0.8f, 0.82f, 0.78f)), false);
            B.Box(g.transform, new Vector3(0f, 0.53f, 0.35f), new Vector3(0.96f, 0.1f, 1.2f), U.Mat(blanket), false);
            B.Box(g.transform, new Vector3(0f, 0.54f, -0.8f), new Vector3(0.5f, 0.1f, 0.3f), U.Mat(new Color(0.9f, 0.9f, 0.88f)), false);
        }

        static void IV(Transform p, Vector3 pos)
        {
            GameObject g = new GameObject("Suero");
            g.transform.SetParent(p, false);
            g.transform.position = pos;
            Material m = U.Mat(new Color(0.6f, 0.6f, 0.62f), 0f, 0.5f);
            U.Prim(PrimitiveType.Cylinder, g.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.04f, 0.95f, 0.04f), m);
            B.Box(g.transform, new Vector3(0f, 1.95f, 0f), new Vector3(0.4f, 0.04f, 0.04f), m, false);
            B.Box(g.transform, new Vector3(0.15f, 1.75f, 0f), new Vector3(0.12f, 0.22f, 0.05f), U.Mat(new Color(0.9f, 0.85f, 0.4f), 0.4f), false);
            B.Box(g.transform, new Vector3(0f, 0.03f, 0f), new Vector3(0.5f, 0.05f, 0.5f), m, false);
        }
    }
}
