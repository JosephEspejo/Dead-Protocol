using UnityEngine;

namespace DeadProtocol
{
    public class AgentDef
    {
        public string name, nick, role, trait, weaponName;
        public float hp, speed, takenMul = 1f, reloadMul = 1f, dmgMul = 1f, hsBonus, bulk = 1f;
        public int startWeapon;
        public bool radar, female, tie;
        public Color suit, shirt, accent, skin, hair;
    }

    // PERSONAJES del informe: el escuadron de 4 agentes
    public static class Agents
    {
        public static AgentDef[] All = new AgentDef[] {
            new AgentDef { name = "Marcos Rivera", nick = "Halcón", role = "Líder del escuadrón", hp = 100f, speed = 3.4f, dmgMul = 1.1f, startWeapon = 0, weaponName = "Pistola M9", tie = true,
                trait = "LÍDER: +10% de daño con todas las armas. Equilibrado en todo.",
                suit = new Color(0.07f, 0.07f, 0.09f), shirt = new Color(0.9f, 0.9f, 0.92f), accent = new Color(0.7f, 0.05f, 0.08f), skin = new Color(0.85f, 0.65f, 0.52f), hair = new Color(0.08f, 0.06f, 0.05f) },
            new AgentDef { name = "Sofía Mendoza", nick = "Víbora", role = "Experta en armas ligeras", hp = 85f, speed = 4.1f, reloadMul = 0.7f, startWeapon = 2, weaponName = "Subfusil", female = true, bulk = 0.88f,
                trait = "VÍBORA: la más rápida y recarga en un instante. Poca vida.",
                suit = new Color(0.14f, 0.14f, 0.18f), shirt = new Color(0.85f, 0.85f, 0.9f), accent = new Color(0.5f, 0.1f, 0.35f), skin = new Color(0.8f, 0.6f, 0.5f), hair = new Color(0.25f, 0.14f, 0.08f) },
            new AgentDef { name = "Daniel Cruz", nick = "Muro", role = "Pesado con chaleco antibalas", hp = 160f, speed = 2.8f, takenMul = 0.7f, reloadMul = 1.2f, startWeapon = 1, weaponName = "Escopeta", bulk = 1.3f,
                trait = "MURO: gran resistencia, recibe 30% menos daño. Lento.",
                suit = new Color(0.6f, 0.63f, 0.7f), shirt = new Color(0.65f, 0.68f, 0.74f), accent = new Color(0.12f, 0.16f, 0.28f), skin = new Color(0.75f, 0.55f, 0.45f), hair = new Color(0.75f, 0.55f, 0.45f) },
            new AgentDef { name = "Tomás Quiroga", nick = "Radar", role = "Técnico y francotirador", hp = 90f, speed = 3.3f, hsBonus = 0.3f, startWeapon = 3, weaponName = "Rifle", radar = true,
                trait = "RADAR: ve a los infectados cercanos en el radar y +30% de daño a la cabeza.",
                suit = new Color(0.2f, 0.2f, 0.22f), shirt = new Color(0.7f, 0.72f, 0.75f), accent = new Color(0.15f, 0.15f, 0.17f), skin = new Color(0.7f, 0.5f, 0.4f), hair = new Color(0.05f, 0.05f, 0.05f) }
        };

        // Modelo 3D para la vista previa del menu
        public static GameObject BuildModel(int id)
        {
            AgentDef a = All[id];
            GameObject g = new GameObject("Agente_" + a.nick);
            Transform t = g.transform;
            float b = a.bulk;
            Material suit = U.Mat(a.suit, 0f, 0.2f), shirt = U.Mat(a.shirt), skin = U.Mat(a.skin, 0f, 0.1f);
            Material hair = U.Mat(a.hair), dark = U.Mat(new Color(0.03f, 0.03f, 0.03f), 0f, 0.7f), acc = U.Mat(a.accent);
            Material pants = U.Mat(new Color(a.suit.r * 0.8f, a.suit.g * 0.8f, a.suit.b * 0.85f), 0f, 0.15f);

            for (int s = -1; s <= 1; s += 2)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(s * 0.12f * b, 0.46f, 0f), new Vector3(0.17f * b, 0.92f, 0.2f * b), pants);
                U.Prim(PrimitiveType.Cube, t, new Vector3(s * 0.12f * b, 0.04f, 0.05f), new Vector3(0.18f * b, 0.08f, 0.3f), dark);
            }
            U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.22f, 0f), new Vector3(0.52f * b, 0.72f, 0.28f * b), suit);
            U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.25f, 0.145f * b), new Vector3(0.2f * b, 0.62f, 0.02f), shirt);
            if (a.tie) U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.2f, 0.16f * b), new Vector3(0.05f, 0.45f, 0.02f), acc);
            if (id == 2)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.2f, 0.15f * b), new Vector3(0.46f * b, 0.55f, 0.05f), acc);
                for (int s = -1; s <= 1; s += 2) U.Prim(PrimitiveType.Cube, t, new Vector3(s * 0.3f * b, 1.52f, 0f), new Vector3(0.16f * b, 0.1f, 0.3f * b), acc);
            }
            if (id == 3)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.2f, 0.15f * b), new Vector3(0.4f * b, 0.5f, 0.04f), acc);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0.2f * b, 0.88f, 0.15f * b), new Vector3(0.1f, 0.07f, 0.03f), U.Mat(new Color(0.2f, 0.9f, 1f), 1.5f));
                U.Prim(PrimitiveType.Cube, t, new Vector3(0.13f, 1.74f, 0.02f), new Vector3(0.04f, 0.1f, 0.04f), dark);
            }
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject piv = new GameObject("Brazo");
                piv.transform.SetParent(t, false);
                piv.transform.localPosition = new Vector3(s * 0.31f * b, 1.52f, 0f);
                piv.transform.localRotation = Quaternion.Euler(0f, 0f, s * 6f);
                U.Prim(PrimitiveType.Cube, piv.transform, new Vector3(0f, -0.3f, 0f), new Vector3(0.12f * b, 0.6f, 0.13f * b), suit);
                U.Prim(PrimitiveType.Sphere, piv.transform, new Vector3(0f, -0.65f, 0f), new Vector3(0.1f, 0.1f, 0.1f), skin);
            }
            U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.6f, 0f), new Vector3(0.1f, 0.1f, 0.1f), skin);
            U.Prim(PrimitiveType.Sphere, t, new Vector3(0f, 1.74f, 0f), new Vector3(0.24f, 0.27f, 0.24f), skin);
            U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.76f, 0.11f), new Vector3(0.25f, 0.06f, 0.05f), dark);
            if (a.female)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 1.66f, -0.09f), new Vector3(0.27f, 0.4f, 0.12f), hair);
                U.Prim(PrimitiveType.Sphere, t, new Vector3(0f, 1.82f, -0.01f), new Vector3(0.27f, 0.18f, 0.27f), hair);
            }
            else if (id != 2)
                U.Prim(PrimitiveType.Sphere, t, new Vector3(0f, 1.85f, -0.01f), new Vector3(0.25f, 0.13f, 0.25f), hair);
            return g;
        }
    }
}
