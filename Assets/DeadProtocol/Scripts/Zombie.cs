using System.Collections.Generic;
using UnityEngine;

namespace DeadProtocol
{
    // ENEMIGO: Infectado. Tres variantes segun tu informe:
    // 0 = delgado sin camisa, 1 = delgado con camisa abierta, 2 = grande (camiseta amarilla, panza con llagas)
    public class Zombie : MonoBehaviour
    {
        public static List<Zombie> All = new List<Zombie>();
        public static Transform Target;
        static Zombie[] protos = new Zombie[3];
        static Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public int kind;
        public bool dead, aggro;
        public float hp, maxHp, headY;
        public Transform vis, armL, armR, legL, legR, bar, barFill;
        public CharacterController cc;

        const float AttackDur = 0.75f;
        float speed, dmg, reach, cooldown, detect = 20f;
        float cd, attackT, stun, sideT, stuckT, walkPhase, hitKick, deathT, groanT, barT, seed, armAng;
        bool hitDone, moving;
        Vector3 side;
        Material fillMat;

        static Material M(string key, Color c, float emis)
        {
            Material m;
            if (!cache.TryGetValue(key, out m) || m == null)
            {
                m = U.Mat(c, emis, 0.05f);
                cache[key] = m;
            }
            return m;
        }

        static GameObject P(PrimitiveType t, Transform par, Vector3 lp, Vector3 ls, Material m)
        {
            return U.Prim(t, par, lp, ls, m, false);
        }

        static Zombie Proto(int k)
        {
            if (protos[k] != null) return protos[k];
            GameObject go = new GameObject(k == 2 ? "Infectado_Grande" : (k == 1 ? "Infectado_Camisa" : "Infectado"));
            go.SetActive(false);
            go.transform.position = new Vector3(0f, -500f, 0f);
            Zombie z = go.AddComponent<Zombie>();
            z.kind = k;
            z.Build();
            protos[k] = z;
            return z;
        }

        void Build()
        {
            bool fat = kind == 2;
            vis = new GameObject("Vis").transform;
            vis.SetParent(transform, false);
            Transform v = vis;

            Material skin = fat ? M("skinF", new Color(0.70f, 0.54f, 0.50f), 0f) : M("skinT", new Color(0.62f, 0.56f, 0.44f), 0f);
            Material skinD = M("skinD", new Color(0.45f, 0.36f, 0.28f), 0f);
            Material blood = M("blood", new Color(0.48f, 0.04f, 0.04f), 0f);
            Material bloodD = M("bloodD", new Color(0.24f, 0.02f, 0.02f), 0f);
            Material eye = M("eye", new Color(1f, 0.1f, 0.1f), 2.5f);
            Material dark = M("dark", new Color(0.05f, 0.01f, 0.01f), 0f);
            Material teeth = M("teeth", new Color(0.75f, 0.70f, 0.55f), 0f);
            Material pants = fat ? M("pantsF", new Color(0.45f, 0.30f, 0.38f), 0f) : M("pantsT", new Color(0.27f, 0.23f, 0.18f), 0f);
            Material shirt = fat ? M("shirtF", new Color(0.88f, 0.68f, 0.10f), 0f) : M("shirtT", new Color(0.66f, 0.66f, 0.60f), 0f);

            if (!fat)
            {
                headY = 1.52f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Transform leg = new GameObject(s < 0 ? "PiernaI" : "PiernaD").transform;
                    leg.SetParent(v, false);
                    leg.localPosition = new Vector3(s * 0.12f, 0.62f, 0f);
                    P(PrimitiveType.Cube, leg, new Vector3(0f, -0.31f, 0f), new Vector3(0.17f, 0.62f, 0.20f), pants);
                    P(PrimitiveType.Cube, leg, new Vector3(0f, -0.60f, 0.04f), new Vector3(0.16f, 0.07f, 0.28f), skinD);
                    if (s < 0) legL = leg; else legR = leg;
                }
                P(PrimitiveType.Cube, v, new Vector3(0f, 0.82f, 0f), new Vector3(0.38f, 0.20f, 0.22f), pants);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.2f, 0f), new Vector3(0.44f, 0.62f, 0.24f), skin);
                P(PrimitiveType.Sphere, v, new Vector3(0.10f, 1.28f, 0.12f), new Vector3(0.16f, 0.14f, 0.05f), blood);
                P(PrimitiveType.Sphere, v, new Vector3(-0.09f, 1.10f, 0.12f), new Vector3(0.13f, 0.11f, 0.05f), blood);
                P(PrimitiveType.Sphere, v, new Vector3(0.05f, 0.95f, 0.12f), new Vector3(0.10f, 0.10f, 0.05f), bloodD);
                P(PrimitiveType.Sphere, v, new Vector3(-0.12f, 1.38f, -0.12f), new Vector3(0.14f, 0.12f, 0.05f), blood);
                if (kind == 1)
                {
                    P(PrimitiveType.Cube, v, new Vector3(-0.2f, 1.2f, 0.13f), new Vector3(0.12f, 0.64f, 0.04f), shirt);
                    P(PrimitiveType.Cube, v, new Vector3(0.2f, 1.2f, 0.13f), new Vector3(0.12f, 0.64f, 0.04f), shirt);
                    P(PrimitiveType.Cube, v, new Vector3(0f, 1.2f, -0.13f), new Vector3(0.46f, 0.64f, 0.04f), shirt);
                }
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.52f, 0f), new Vector3(0.10f, 0.10f, 0.10f), skin);
                GameObject head = P(PrimitiveType.Sphere, v, new Vector3(0f, 1.7f, 0f), new Vector3(0.25f, 0.29f, 0.26f), skin);
                head.name = "Head";
                P(PrimitiveType.Sphere, v, new Vector3(-0.06f, 1.73f, 0.105f), new Vector3(0.09f, 0.09f, 0.05f), dark);
                P(PrimitiveType.Sphere, v, new Vector3(0.06f, 1.73f, 0.105f), new Vector3(0.09f, 0.09f, 0.05f), dark);
                P(PrimitiveType.Sphere, v, new Vector3(-0.06f, 1.73f, 0.125f), new Vector3(0.05f, 0.05f, 0.04f), eye);
                P(PrimitiveType.Sphere, v, new Vector3(0.06f, 1.73f, 0.125f), new Vector3(0.05f, 0.05f, 0.04f), eye);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.62f, 0.115f), new Vector3(0.09f, 0.07f, 0.03f), dark);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.652f, 0.128f), new Vector3(0.07f, 0.014f, 0.02f), teeth);
                P(PrimitiveType.Sphere, v, new Vector3(0.09f, 1.78f, 0.09f), new Vector3(0.08f, 0.06f, 0.04f), blood);
                for (int s = -1; s <= 1; s += 2)
                {
                    Transform arm = new GameObject(s < 0 ? "BrazoI" : "BrazoD").transform;
                    arm.SetParent(v, false);
                    arm.localPosition = new Vector3(s * 0.29f, 1.46f, 0f);
                    P(PrimitiveType.Cube, arm, new Vector3(0f, -0.2f, 0f), new Vector3(0.10f, 0.40f, 0.10f), skin);
                    if (kind == 1) P(PrimitiveType.Cube, arm, new Vector3(0f, -0.12f, 0f), new Vector3(0.13f, 0.26f, 0.13f), shirt);
                    P(PrimitiveType.Cube, arm, new Vector3(0f, -0.5f, 0f), new Vector3(0.085f, 0.30f, 0.085f), skinD);
                    P(PrimitiveType.Sphere, arm, new Vector3(0f, -0.68f, 0f), new Vector3(0.10f, 0.12f, 0.06f), skin);
                    if (s < 0) armL = arm; else armR = arm;
                }
                cc = gameObject.AddComponent<CharacterController>();
                cc.height = 1.85f; cc.radius = 0.32f; cc.center = new Vector3(0f, 0.93f, 0f);
            }
            else
            {
                headY = 1.62f;
                for (int s = -1; s <= 1; s += 2)
                {
                    Transform leg = new GameObject(s < 0 ? "PiernaI" : "PiernaD").transform;
                    leg.SetParent(v, false);
                    leg.localPosition = new Vector3(s * 0.2f, 0.7f, 0f);
                    P(PrimitiveType.Cube, leg, new Vector3(0f, -0.35f, 0f), new Vector3(0.30f, 0.70f, 0.32f), pants);
                    P(PrimitiveType.Cube, leg, new Vector3(0f, -0.68f, 0.06f), new Vector3(0.28f, 0.08f, 0.40f), skin);
                    if (s < 0) legL = leg; else legR = leg;
                }
                P(PrimitiveType.Sphere, v, new Vector3(0f, 1.40f, 0f), new Vector3(0.95f, 0.62f, 0.62f), shirt);
                P(PrimitiveType.Sphere, v, new Vector3(0f, 0.98f, 0.05f), new Vector3(0.90f, 0.80f, 0.72f), skin);
                float[,] sores = { { -0.22f, 1.00f }, { 0.10f, 1.10f }, { 0.25f, 0.92f }, { -0.05f, 0.88f }, { 0.0f, 1.0f }, { -0.28f, 0.80f }, { 0.20f, 0.75f }, { -0.15f, 1.15f } };
                for (int i = 0; i < sores.GetLength(0); i++)
                {
                    float sx = sores[i, 0], sy = sores[i, 1];
                    float q = 1f - (sx * sx) / (0.45f * 0.45f) - ((sy - 0.98f) * (sy - 0.98f)) / (0.40f * 0.40f);
                    float sz = 0.05f + 0.36f * Mathf.Sqrt(Mathf.Max(0.05f, q));
                    P(PrimitiveType.Sphere, v, new Vector3(sx, sy, sz), new Vector3(0.14f, 0.12f, 0.06f), i % 3 == 0 ? bloodD : blood);
                }
                GameObject head = P(PrimitiveType.Sphere, v, new Vector3(0f, 1.85f, 0.02f), new Vector3(0.34f, 0.36f, 0.34f), skin);
                head.name = "Head";
                P(PrimitiveType.Sphere, v, new Vector3(-0.08f, 1.9f, 0.15f), new Vector3(0.11f, 0.11f, 0.06f), dark);
                P(PrimitiveType.Sphere, v, new Vector3(0.08f, 1.9f, 0.15f), new Vector3(0.11f, 0.11f, 0.06f), dark);
                P(PrimitiveType.Sphere, v, new Vector3(-0.08f, 1.9f, 0.18f), new Vector3(0.06f, 0.06f, 0.04f), eye);
                P(PrimitiveType.Sphere, v, new Vector3(0.08f, 1.9f, 0.18f), new Vector3(0.06f, 0.06f, 0.04f), eye);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.74f, 0.16f), new Vector3(0.17f, 0.13f, 0.05f), dark);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.795f, 0.185f), new Vector3(0.14f, 0.025f, 0.02f), teeth);
                P(PrimitiveType.Cube, v, new Vector3(0f, 1.69f, 0.185f), new Vector3(0.11f, 0.02f, 0.02f), teeth);
                for (int s = -1; s <= 1; s += 2)
                {
                    Transform arm = new GameObject(s < 0 ? "BrazoI" : "BrazoD").transform;
                    arm.SetParent(v, false);
                    arm.localPosition = new Vector3(s * 0.5f, 1.5f, 0.02f);
                    P(PrimitiveType.Cube, arm, new Vector3(0f, -0.2f, 0f), new Vector3(0.20f, 0.40f, 0.20f), skin);
                    P(PrimitiveType.Cube, arm, new Vector3(0f, -0.1f, 0f), new Vector3(0.24f, 0.22f, 0.24f), shirt);
                    P(PrimitiveType.Cube, arm, new Vector3(0f, -0.5f, 0f), new Vector3(0.16f, 0.30f, 0.16f), skin);
                    P(PrimitiveType.Sphere, arm, new Vector3(0f, -0.68f, 0f), new Vector3(0.16f, 0.14f, 0.10f), skin);
                    if (s < 0) armL = arm; else armR = arm;
                }
                cc = gameObject.AddComponent<CharacterController>();
                cc.height = 1.95f; cc.radius = 0.55f; cc.center = new Vector3(0f, 0.98f, 0f);
            }
            cc.stepOffset = 0.1f; cc.skinWidth = 0.04f; cc.minMoveDistance = 0f;

            // barra de vida sobre la cabeza (se muestra al recibir dano)
            bar = new GameObject("BarraVida").transform;
            bar.SetParent(transform, false);
            bar.localPosition = new Vector3(0f, headY + (fat ? 0.75f : 0.6f), 0f);
            P(PrimitiveType.Cube, bar, Vector3.zero, new Vector3(1.0f, 0.13f, 0.02f), U.Flat(new Color(0.04f, 0.04f, 0.04f, 0.95f)));
            GameObject f = P(PrimitiveType.Cube, bar, new Vector3(0f, 0f, -0.012f), new Vector3(0.96f, 0.09f, 0.02f), U.Flat(new Color(0.2f, 0.9f, 0.2f, 1f)));
            barFill = f.transform;
            bar.gameObject.SetActive(false);
        }

        void Init()
        {
            bool fat = kind == 2;
            maxHp = hp = fat ? 220f : 70f;
            speed = fat ? Random.Range(0.9f, 1.3f) : (kind == 1 ? Random.Range(1.4f, 2.2f) : Random.Range(1.6f, 2.5f));
            dmg = fat ? 20f : 10f;
            reach = fat ? 1.9f : 1.5f;
            cooldown = fat ? 1.6f : 1.1f;
            cd = Random.Range(0.2f, 0.8f);
            seed = Random.value * 10f;
            groanT = Random.Range(2f, 9f);
            aggro = false; dead = false;
            armAng = -28f;
        }

        // Crea un infectado (queda inactivo: el administrador lo activa segun la distancia)
        public static Zombie Spawn(int k, Vector3 pos)
        {
            Zombie pr = Proto(k);
            GameObject g = (GameObject)Object.Instantiate(pr.gameObject);
            g.name = pr.gameObject.name;
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Zombie z = g.GetComponent<Zombie>();
            z.Init();
            All.Add(z);
            return z;
        }

        void Face(Vector3 to, float dt)
        {
            if (to.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 8f * dt);
        }

        void MoveCC(Vector3 dir, float sp, float dt)
        {
            Vector3 before = transform.position;
            Vector3 vel = dir * sp;
            vel.y = -3f;
            cc.Move(vel * dt);
            Vector3 delta = transform.position - before;
            delta.y = 0f;
            float exp = sp * dt;
            if (exp > 0.0001f && delta.magnitude < exp * 0.3f)
            {
                stuckT += dt;
                if (stuckT > 0.3f)
                {
                    stuckT = 0f; sideT = 0.8f;
                    side = (Random.value < 0.5f ? 1f : -1f) * Vector3.Cross(Vector3.up, dir);
                }
            }
            else stuckT = 0f;
            walkPhase += dt * sp * 3.4f;
            moving = true;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            if (dead) { DeathUpdate(dt); return; }
            if (hitKick > 0f) hitKick = Mathf.Max(0f, hitKick - dt * 5f);
            GameController gc = GameController.I;
            bool live = gc != null && gc.Live && Target != null;
            moving = false;
            if (live)
            {
                Vector3 to = Target.position - transform.position;
                to.y = 0f;
                float d = to.magnitude;
                if (!aggro && d < detect) aggro = true;
                cd -= dt;
                stun -= dt;
                if (attackT > 0f)
                {
                    attackT += dt;
                    Face(to, dt);
                    if (!hitDone && attackT >= AttackDur * 0.45f)
                    {
                        hitDone = true;
                        if (d < reach + 0.5f) gc.PlayerHit(dmg, transform.position);
                    }
                    if (attackT >= AttackDur) attackT = 0f;
                }
                else if (aggro && stun <= 0f)
                {
                    if (d > reach * 0.8f)
                    {
                        Vector3 dir = to / Mathf.Max(d, 0.001f);
                        if (sideT > 0f) { sideT -= dt; dir = (dir + side * 0.9f).normalized; }
                        Face(dir, dt);
                        MoveCC(dir, speed, dt);
                    }
                    else
                    {
                        Face(to, dt);
                        if (cd <= 0f) { attackT = 0.001f; hitDone = false; cd = cooldown; }
                    }
                }
                groanT -= dt;
                if (groanT <= 0f)
                {
                    groanT = Random.Range(3.5f, 9f);
                    if (d < 28f) gc.Groan(transform.position);
                }
            }
            Animate(dt);
            if (bar.gameObject.activeSelf)
            {
                barT -= dt;
                if (barT <= 0f) bar.gameObject.SetActive(false);
            }
        }

        void Animate(float dt)
        {
            float swing = moving ? Mathf.Sin(walkPhase) : 0f;
            legL.localRotation = Quaternion.Euler(swing * 32f, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-swing * 32f, 0f, 0f);
            float baseAng = aggro ? (kind == 2 ? -55f : -82f) : -28f;
            if (attackT > 0f)
            {
                float t = Mathf.Clamp01(attackT / AttackDur);
                armAng = Mathf.Lerp(-125f, -10f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.5f)));
            }
            else armAng = Mathf.Lerp(armAng, baseAng, 6f * dt);
            float sway = Mathf.Sin(Time.time * 2f + seed) * 4f;
            armL.localRotation = Quaternion.Euler(armAng + sway + swing * 8f, 0f, 6f);
            armR.localRotation = Quaternion.Euler(armAng - sway - swing * 8f, 0f, -6f);
            float lean = (aggro ? 10f : 3f) + Mathf.Sin(Time.time * 1.5f + seed) * 2f - hitKick * 14f;
            vis.localRotation = Quaternion.Euler(lean, 0f, Mathf.Sin(Time.time * 1.1f + seed) * 2.5f);
            vis.localPosition = new Vector3(0f, moving ? Mathf.Abs(Mathf.Sin(walkPhase)) * 0.04f : 0f, 0f);
        }

        void LateUpdate()
        {
            if (dead || !bar.gameObject.activeSelf) return;
            GameController gc = GameController.I;
            if (gc == null) return;
            Vector3 dirToCam = bar.position - gc.CamT.position;
            if (dirToCam.sqrMagnitude > 0.001f) bar.rotation = Quaternion.LookRotation(dirToCam);
        }

        void ShowBar()
        {
            bar.gameObject.SetActive(true);
            barT = 4f;
            float pct = Mathf.Clamp01(hp / maxHp);
            barFill.localScale = new Vector3(0.96f * Mathf.Max(0.001f, pct), 0.09f, 0.02f);
            barFill.localPosition = new Vector3(-0.48f * (1f - pct), 0f, -0.012f);
            if (fillMat == null) fillMat = barFill.GetComponent<Renderer>().material;
            fillMat.color = Color.Lerp(new Color(0.95f, 0.1f, 0.1f), new Color(0.2f, 0.9f, 0.2f), pct);
        }

        public void Hit(float amount, bool head, Vector3 p, Vector3 dir)
        {
            if (dead) return;
            hp -= amount;
            aggro = true;
            stun = kind == 2 ? 0.03f : 0.12f;
            hitKick = 1f;
            FxPool.Blood(p, -dir);
            if (head) FxPool.Blood(p, Vector3.up);
            ShowBar();
            if (hp <= 0f) { Die(head); return; }
            if (kind != 2 && cc.enabled)
            {
                Vector3 k = dir;
                k.y = 0f;
                if (k.sqrMagnitude > 0.001f) cc.Move(k.normalized * 0.15f);
            }
        }

        void Die(bool head)
        {
            dead = true;
            deathT = 0f;
            All.Remove(this);
            if (cc != null) cc.enabled = false;
            bar.gameObject.SetActive(false);
            FxPool.Blood(transform.position + Vector3.up * 1.2f, Vector3.up);
            if (GameController.I != null) GameController.I.OnKill(this, head);
        }

        void DeathUpdate(float dt)
        {
            deathT += dt;
            float k = Mathf.Clamp01(deathT / 0.55f);
            vis.localRotation = Quaternion.Euler(-90f * k, 0f, 0f);
            vis.localPosition = new Vector3(0f, 0.12f * k, 0f);
            armL.localRotation = Quaternion.Euler(-150f * k, 0f, 20f);
            armR.localRotation = Quaternion.Euler(-150f * k, 0f, -20f);
            if (deathT > 7f) transform.position += Vector3.down * dt * 0.3f;
            if (deathT > 11f) Destroy(gameObject);
        }

        void OnDestroy() { All.Remove(this); }
    }
}
