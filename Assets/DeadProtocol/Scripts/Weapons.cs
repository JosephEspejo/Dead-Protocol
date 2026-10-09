using UnityEngine;

namespace DeadProtocol
{
    public class WeaponDef
    {
        public string name, desc;
        public float dmg, rate, reload, spread, range, hsMul, kick, shake, zoom;
        public int pellets = 1, mag, reserve, reserveMax;
        public bool auto;
        public AudioClip clip;
    }

    public class WView
    {
        public GameObject root, flash;
        public Light flashLight;
        public Transform muzzle;
    }

    // ARMAS: pistola, escopeta, subfusil y rifle
    public static class Weapons
    {
        public static WeaponDef[] All;
        public static AudioClip HitClip, EmptyClip, ReloadClip;

        static Weapons()
        {
            All = new WeaponDef[] {
                new WeaponDef { name = "PISTOLA M9", desc = "Precisa y rápida de recargar", dmg = 28f, rate = 0.25f, mag = 12, reserve = 48, reserveMax = 144, reload = 1.1f, spread = 0.5f, range = 70f, hsMul = 2.5f, kick = 2.5f, shake = 0.02f, zoom = 52f, auto = false, clip = U.Noise(0.18f, 140f, 0.75f, 22f) },
                new WeaponDef { name = "ESCOPETA", desc = "9 perdigones, devastadora de cerca", dmg = 13f, pellets = 9, rate = 0.85f, mag = 6, reserve = 18, reserveMax = 48, reload = 2.0f, spread = 4.5f, range = 30f, hsMul = 1.8f, kick = 8f, shake = 0.08f, zoom = 56f, auto = false, clip = U.Noise(0.35f, 70f, 0.85f, 12f) },
                new WeaponDef { name = "SUBFUSIL", desc = "Fuego automático", dmg = 13f, rate = 0.075f, mag = 30, reserve = 90, reserveMax = 270, reload = 1.6f, spread = 1.8f, range = 55f, hsMul = 2.0f, kick = 1.2f, shake = 0.012f, zoom = 52f, auto = true, clip = U.Noise(0.1f, 200f, 0.8f, 40f) },
                new WeaponDef { name = "RIFLE", desc = "Un disparo, gran daño, largo alcance", dmg = 85f, rate = 0.75f, mag = 8, reserve = 24, reserveMax = 72, reload = 2.1f, spread = 0.15f, range = 120f, hsMul = 3f, kick = 5f, shake = 0.04f, zoom = 28f, auto = false, clip = U.Noise(0.3f, 100f, 0.7f, 16f) }
            };
            HitClip = U.Noise(0.08f, 180f, 0.6f, 30f);
            EmptyClip = U.Noise(0.05f, 1200f, 0.2f, 60f);
            ReloadClip = U.Noise(0.2f, 400f, 0.5f, 15f);
        }

        static GameObject Cyl(Transform p, Vector3 lp, float r, float len, Material m)
        {
            GameObject g = U.Prim(PrimitiveType.Cylinder, p, lp, new Vector3(r, len / 2f, r), m);
            g.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            return g;
        }

        public static WView BuildView(Transform cam, int id, Color sleeve, Color skin)
        {
            WView v = new WView();
            GameObject root = new GameObject("Arma_" + All[id].name);
            root.transform.SetParent(cam, false);
            root.transform.localPosition = new Vector3(0.26f, -0.26f, 0.6f);
            Transform t = root.transform;
            Material metal = U.Mat(new Color(0.08f, 0.08f, 0.09f), 0f, 0.5f);
            Material grey = U.Mat(new Color(0.25f, 0.25f, 0.27f), 0f, 0.5f);
            Material wood = U.Mat(new Color(0.25f, 0.15f, 0.08f), 0f, 0.2f);
            Material sl = U.Mat(sleeve, 0f, 0.1f);
            Material sk = U.Mat(skin, 0f, 0.1f);
            float mz = 0.45f;
            if (id == 0)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.09f, 0.34f), metal);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0.06f, 0.02f), new Vector3(0.06f, 0.04f, 0.32f), grey);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.13f, -0.1f), new Vector3(0.06f, 0.16f, 0.08f), metal);
                Cyl(t, new Vector3(0f, 0.02f, 0.24f), 0.035f, 0.16f, grey);
                mz = 0.4f;
            }
            else if (id == 1)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.1f, 0.5f), metal);
                Cyl(t, new Vector3(0f, 0.05f, 0.55f), 0.04f, 0.8f, grey);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.04f, 0.45f), new Vector3(0.08f, 0.07f, 0.22f), wood);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.04f, -0.35f), new Vector3(0.06f, 0.12f, 0.35f), wood);
                mz = 0.97f;
            }
            else if (id == 2)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.12f, 0.5f), metal);
                Cyl(t, new Vector3(0f, 0.03f, 0.4f), 0.03f, 0.3f, grey);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.17f, 0.05f), new Vector3(0.05f, 0.25f, 0.08f), grey);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.02f, -0.35f), new Vector3(0.05f, 0.08f, 0.3f), metal);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0.09f, 0.1f), new Vector3(0.03f, 0.04f, 0.1f), grey);
                mz = 0.58f;
            }
            else
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.11f, 0.9f), metal);
                Cyl(t, new Vector3(0f, 0.1f, 0.1f), 0.05f, 0.35f, grey);
                U.Prim(PrimitiveType.Sphere, t, new Vector3(0f, 0.1f, 0.28f), new Vector3(0.06f, 0.06f, 0.02f), U.Mat(new Color(0.3f, 0.7f, 1f), 1f));
                Cyl(t, new Vector3(0f, 0.03f, 0.8f), 0.03f, 0.7f, grey);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.05f, -0.6f), new Vector3(0.06f, 0.14f, 0.4f), wood);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, -0.14f, 0.1f), new Vector3(0.05f, 0.14f, 0.08f), grey);
                mz = 1.15f;
            }
            // brazo derecho y mano
            GameObject fa = U.Prim(PrimitiveType.Cube, t, new Vector3(0.03f, -0.2f, -0.25f), new Vector3(0.1f, 0.1f, 0.55f), sl);
            fa.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            U.Prim(PrimitiveType.Sphere, t, new Vector3(0.0f, -0.1f, -0.05f), new Vector3(0.1f, 0.1f, 0.1f), sk);
            // mano izquierda de apoyo
            if (id != 0)
            {
                GameObject fl = U.Prim(PrimitiveType.Cube, t, new Vector3(-0.12f, -0.17f, 0.15f), new Vector3(0.09f, 0.09f, 0.5f), sl);
                fl.transform.localRotation = Quaternion.Euler(-10f, 15f, 0f);
                U.Prim(PrimitiveType.Sphere, t, new Vector3(-0.04f, -0.08f, 0.4f), new Vector3(0.09f, 0.09f, 0.09f), sk);
            }
            // fogonazo
            GameObject mg = new GameObject("Boca");
            mg.transform.SetParent(t, false);
            mg.transform.localPosition = new Vector3(0f, 0.03f, mz);
            GameObject fl2 = U.Prim(PrimitiveType.Sphere, mg.transform, Vector3.zero, new Vector3(0.14f, 0.14f, 0.2f), U.Mat(new Color(1f, 0.8f, 0.3f), 3f));
            fl2.SetActive(false);
            GameObject lg = new GameObject("LuzFogonazo");
            lg.transform.SetParent(mg.transform, false);
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.8f, 0.4f);
            l.range = 8f;
            l.intensity = 8f;
            l.shadows = LightShadows.None;
            l.enabled = false;
            v.root = root; v.muzzle = mg.transform; v.flash = fl2; v.flashLight = l;
            root.SetActive(false);
            return v;
        }
    }
}
