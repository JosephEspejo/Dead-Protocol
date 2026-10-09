using System.Collections.Generic;
using UnityEngine;

namespace DeadProtocol
{
    // Efectos de disparo (particulas y trazos) reutilizables
    public class TracerFx : MonoBehaviour
    {
        float t; LineRenderer lr;
        public void Show(Vector3 a, Vector3 b)
        {
            if (lr == null) lr = GetComponent<LineRenderer>();
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            t = 0.06f;
            gameObject.SetActive(true);
        }
        void Update() { t -= Time.deltaTime; if (t <= 0f) gameObject.SetActive(false); }
    }

    public static class FxPool
    {
        static ParticleSystem blood, spark, dust;
        static TracerFx[] tr;
        static int ti;

        static ParticleSystem MakePS(Transform root, string name, Color c, float s0, float s1, float grav, float size, float life)
        {
            GameObject g = new GameObject(name);
            g.transform.SetParent(root, false);
            ParticleSystem ps = g.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(s0, s1);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startColor = c;
            main.gravityModifier = grav;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;
            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.angle = 35f;
            sh.radius = 0.02f;
            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gr = new Gradient();
            gr.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                       new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(gr);
            ParticleSystemRenderer r = g.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Sprites/Default");
            Material m = s != null ? new Material(s) : U.Mat(Color.white);
            m.mainTexture = U.Soft();
            r.sharedMaterial = m;
            ps.Play();
            return ps;
        }

        public static void Init(Transform root)
        {
            blood = MakePS(root, "SangreFx", new Color(0.6f, 0.02f, 0.02f, 1f), 2f, 5f, 0.8f, 0.12f, 0.6f);
            spark = MakePS(root, "ChispasFx", new Color(1f, 0.85f, 0.3f, 1f), 3f, 7f, 0.6f, 0.07f, 0.3f);
            dust = MakePS(root, "PolvoFx", new Color(0.65f, 0.65f, 0.65f, 0.7f), 0.5f, 2f, 0.1f, 0.2f, 0.6f);
            tr = new TracerFx[12];
            Shader s = Shader.Find("Sprites/Default");
            for (int i = 0; i < tr.Length; i++)
            {
                GameObject g = new GameObject("Trazo");
                g.transform.SetParent(root, false);
                LineRenderer lr = g.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.03f;
                lr.endWidth = 0.01f;
                lr.useWorldSpace = true;
                lr.sharedMaterial = s != null ? new Material(s) : U.Mat(Color.yellow);
                lr.startColor = new Color(1f, 0.9f, 0.5f, 0.9f);
                lr.endColor = new Color(1f, 0.7f, 0.3f, 0.2f);
                tr[i] = g.AddComponent<TracerFx>();
                g.SetActive(false);
            }
        }

        static void Emit(ParticleSystem ps, Vector3 p, Vector3 n, int c)
        {
            if (ps == null) return;
            ps.transform.position = p;
            ps.transform.rotation = Quaternion.LookRotation(n.sqrMagnitude < 0.001f ? Vector3.up : n);
            ps.Emit(c);
        }

        public static void Blood(Vector3 p, Vector3 n) { Emit(blood, p, n, 18); }
        public static void Spark(Vector3 p, Vector3 n) { Emit(spark, p, n, 8); Emit(dust, p, n, 4); }
        public static void Tracer(Vector3 a, Vector3 b) { if (tr == null) return; tr[ti].Show(a, b); ti = (ti + 1) % tr.Length; }
    }

    // Cajas de municion y botiquines
    public class Pickup : MonoBehaviour
    {
        public int kind; // 0 municion, 1 botiquin
        Vector3 basePos; float t0;

        public static Pickup Create(int kind, Vector3 pos)
        {
            GameObject g = new GameObject(kind == 0 ? "CajaMunicion" : "Botiquin");
            g.transform.position = pos;
            Transform t = g.transform;
            if (kind == 0)
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.3f, 0.32f), U.Mat(new Color(0.2f, 0.35f, 0.15f), 0f, 0.2f));
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0.01f, 0f), new Vector3(0.52f, 0.06f, 0.34f), U.Mat(new Color(1f, 0.8f, 0.1f), 1.2f));
                for (int i = -1; i <= 1; i++)
                    U.Prim(PrimitiveType.Cylinder, t, new Vector3(i * 0.12f, 0.2f, 0f), new Vector3(0.05f, 0.07f, 0.05f), U.Mat(new Color(0.9f, 0.7f, 0.2f), 0.3f, 0.6f));
            }
            else
            {
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0f, 0f), new Vector3(0.45f, 0.3f, 0.45f), U.Mat(new Color(0.9f, 0.9f, 0.9f), 0f, 0.3f));
                Material red = U.Mat(new Color(1f, 0.05f, 0.05f), 1.8f);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0.16f, 0f), new Vector3(0.3f, 0.03f, 0.09f), red);
                U.Prim(PrimitiveType.Cube, t, new Vector3(0f, 0.16f, 0f), new Vector3(0.09f, 0.03f, 0.3f), red);
            }
            return g.AddComponent<Pickup>();
        }

        void Start() { basePos = transform.position; t0 = Random.value * 6f; }

        void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            transform.position = new Vector3(basePos.x, basePos.y + 0.55f + Mathf.Sin(Time.time * 2f + t0) * 0.08f, basePos.z);
        }
    }
}
