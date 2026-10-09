using System.Collections;
using UnityEngine;

namespace DeadProtocol
{
    // MAPA COMPLETO: 5 escenarios conectados, recorrido libre con colisiones, mapa (M), tormenta y atmosferas por zona.
    public class SceneController : MonoBehaviour
    {
        Camera cam; Transform camT, player; CharacterController cc; Light torch;
        AudioSource rain, sfx; AudioClip thunder;
        float yaw, pitch, bobT, tourZ = -6f, bannerT, flashT;
        bool tour, locked, torchOn, showMap, flashing;
        int curZone = -1;
        Atm cur;
        GUIStyle st, stw;
        Texture2D thumb;

        void Awake()
        {
            foreach (Camera c in Camera.allCameras) c.gameObject.SetActive(false);
            foreach (Light l in Resources.FindObjectsOfTypeAll<Light>())
                if (l.gameObject.scene.IsValid()) l.gameObject.SetActive(false);

            player = new GameObject("Jugador").transform;
            player.position = new Vector3(0f, 0f, -6f);
            cc = player.gameObject.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.1f; cc.skinWidth = 0.05f;

            GameObject cg = new GameObject("DP_Camara");
            cg.tag = "MainCamera";
            cg.transform.SetParent(player, false);
            cg.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
            camT = cg.transform;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 220f;

            Zones.BuildAll();
            World.Rain(camT);

            GameObject tg = new GameObject("Linterna");
            tg.transform.SetParent(camT, false);
            torch = tg.AddComponent<Light>();
            torch.type = LightType.Spot;
            torch.range = 30f;
            torch.spotAngle = 45f;
            torch.intensity = 4f;
            torch.color = new Color(1f, 0.95f, 0.85f);
            torch.enabled = false;

            rain = gameObject.AddComponent<AudioSource>();
            rain.clip = U.RainLoop();
            rain.loop = true;
            rain.volume = 0.35f;
            rain.spatialBlend = 0f;
            rain.Play();
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.spatialBlend = 0f;
            thunder = U.Noise(2.5f, 45f, 0.7f, 1.4f);
            thumb = Resources.Load<Texture2D>("zona1_calle");

            ApplyAtmosphere();
            StartCoroutine(Storm());
        }

        IEnumerator Storm()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(6f, 15f));
                if (cur.rain < 0.5f) continue;
                int n = Random.Range(1, 4);
                for (int i = 0; i < n; i++)
                {
                    flashing = true;
                    yield return new WaitForSeconds(0.06f);
                    flashing = false;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
                }
                yield return new WaitForSeconds(Random.Range(0.4f, 2.2f));
                if (cur.rain > 0.5f) sfx.PlayOneShot(thunder, Random.Range(0.6f, 1f));
            }
        }

        void ApplyAtmosphere()
        {
            cur = Zones.Sample(player.position.z);
            Color fog = cur.fog, amb = cur.amb;
            float sun = cur.sun;
            if (flashing && cur.rain > 0.5f)
            {
                fog = new Color(0.65f, 0.67f, 0.75f);
                amb = new Color(0.75f, 0.78f, 0.90f);
                sun = 3.2f;
            }
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fog;
            RenderSettings.fogDensity = cur.dens;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = amb;
            cam.backgroundColor = fog;
            if (World.Sun != null) World.Sun.intensity = sun;
            if (World.RainPS != null)
            {
                ParticleSystem.EmissionModule em = World.RainPS.emission;
                em.rateOverTime = 2200f * cur.rain;
            }
            rain.volume = 0.35f * cur.rain + 0.05f;
        }

        void Teleport(Vector3 p)
        {
            cc.enabled = false;
            player.position = p;
            cc.enabled = !tour;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (U.TourKey()) { tour = !tour; if (tour) tourZ = player.position.z; else cc.enabled = true; }
            if (U.LightKey()) { torchOn = !torchOn; torch.enabled = torchOn; }
            if (U.MapKey()) showMap = !showMap;
            int nk = U.NumKey();
            if (nk > 0) { Teleport(new Vector3(0f, 0f, Zones.StartZ(nk - 1))); yaw = 0f; tourZ = player.position.z; }
            if (U.EscKey()) { locked = false; showMap = false; }
            if (!locked && !showMap && U.Click()) locked = true;
            bool free = locked && !showMap;
            Cursor.lockState = free ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !free;

            float bob = 0f;
            if (tour)
            {
                if (cc.enabled) cc.enabled = false;
                tourZ += 4f * dt;
                if (tourZ > 455f) tourZ = -6f;
                float f = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(145f, 155f, tourZ)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(180f, 190f, tourZ)));
                float x = Mathf.Sin(Time.time * 0.3f) * 0.8f + 11f * f;
                player.position = new Vector3(x, 0f, tourZ);
                yaw = Mathf.Sin(Time.time * 0.25f) * 18f + (f > 0.05f ? -25f * f : 0f);
                pitch = 2f + Mathf.Sin(Time.time * 0.2f) * 4f;
                bobT += dt * 9f;
                bob = Mathf.Sin(bobT) * 0.03f;
            }
            else
            {
                if (free)
                {
                    Vector2 md = U.MouseDelta();
                    yaw += md.x * 0.12f;
                    pitch = Mathf.Clamp(pitch - md.y * 0.12f, -80f, 80f);
                }
                Vector2 a = free ? U.Axis() : Vector2.zero;
                float sp = U.Shift() ? 6.5f : 3.2f;
                Vector3 fwd = new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(yaw * Mathf.Deg2Rad));
                Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);
                Vector3 mv = fwd * a.y + right * a.x;
                if (mv.sqrMagnitude > 1f) mv.Normalize();
                Vector3 d = mv * sp * dt;
                d.y = -0.05f;
                if (cc.enabled) cc.Move(d);
                Vector3 p = player.position;
                float hw = Zones.HalfAt(p.z);
                if (p.z < -9f || p.z > 468f || Mathf.Abs(p.x) > hw || p.y > 0.5f)
                    Teleport(new Vector3(Mathf.Clamp(p.x, -hw, hw), 0f, Mathf.Clamp(p.z, -9f, 468f)));
                if (mv.sqrMagnitude > 0.01f) { bobT += dt * (U.Shift() ? 12f : 9f); bob = Mathf.Sin(bobT) * 0.03f; }
            }
            player.rotation = Quaternion.Euler(0f, yaw, 0f);
            camT.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            camT.localPosition = new Vector3(0f, 1.65f + bob, 0f);

            int z = Zones.IndexAt(player.position.z);
            if (z != curZone) { curZone = z; bannerT = 4f; }
            bannerT = Mathf.Max(0f, bannerT - dt);
            ApplyAtmosphere();
        }

        // ---------- Interfaz ----------
        void L(GUIStyle s, string t, float x, float y, float w, float h, int size, TextAnchor an, Color c)
        {
            s.fontSize = Mathf.RoundToInt(size * Screen.height / 720f);
            s.alignment = an;
            s.normal.textColor = new Color(0f, 0f, 0f, c.a);
            GUI.Label(new Rect(x + 2f, y + 2f, w, h), t, s);
            s.normal.textColor = c;
            GUI.Label(new Rect(x, y, w, h), t, s);
        }

        void R(float x, float y, float w, float h, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (st == null)
            {
                st = new GUIStyle(GUI.skin.label); st.fontStyle = FontStyle.Bold;
                stw = new GUIStyle(GUI.skin.label); stw.fontStyle = FontStyle.Bold; stw.wordWrap = true;
            }
            float W = Screen.width, H = Screen.height, s = H / 720f;
            TextAnchor C = TextAnchor.MiddleCenter;
            ZoneInfo zi = Zones.All[Mathf.Max(0, curZone)];

            string t = "DEAD PROTOCOL  -  Zona " + (curZone + 1) + ": " + zi.name + "\n" +
                       "WASD mover  ·  Shift correr  ·  Mouse mirar  ·  F linterna  ·  M mapa  ·  1-5 ir a una zona  ·  C recorrido automático  ·  Esc soltar mouse";
            if (!locked && !tour && !showMap) t += "\n>> Haz CLIC para controlar la cámara <<";
            L(st, t, 20f * s, 12f * s, W, 100f * s, 17, TextAnchor.UpperLeft, Color.white);

            // mini barra de progreso del mapa
            float bw = 420f * s, bx = (W - bw) / 2f, by = H - 34f * s, total = 482f;
            for (int i = 0; i < Zones.All.Length; i++)
            {
                ZoneInfo q = Zones.All[i];
                R(bx + (q.z0 + 12f) / total * bw, by, (q.z1 - q.z0) / total * bw - 2f, 12f * s, new Color(q.map.r, q.map.g, q.map.b, i == curZone ? 1f : 0.5f));
            }
            R(bx + (player.position.z + 12f) / total * bw - 4f * s, by - 5f * s, 8f * s, 22f * s, Color.white);

            if (bannerT > 0f && !showMap)
            {
                float al = Mathf.Clamp01(bannerT);
                L(st, "ZONA " + (curZone + 1), 0f, H * 0.22f, W, 50f * s, 28, C, new Color(1f, 0.85f, 0.4f, al));
                L(st, zi.name.ToUpper(), 0f, H * 0.22f + 45f * s, W, 80f * s, 56, C, new Color(1f, 0.95f, 0.9f, al));
            }
            if (showMap) DrawMap(W, H, s);
        }

        void DrawMap(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter;
            R(0f, 0f, W, H, new Color(0f, 0f, 0f, 0.88f));
            L(st, "MAPA DEL JUEGO  -  DEAD PROTOCOL", 0f, 14f * s, W, 50f * s, 36, C, new Color(1f, 0.85f, 0.4f));
            float x0 = 60f * s, w = W - 120f * s, y0 = 80f * s, h = H * 0.34f, cy = y0 + h / 2f;
            float total = 482f, sx = w / total, sy = h / 52f;
            for (int i = 0; i < Zones.All.Length; i++)
            {
                ZoneInfo q = Zones.All[i];
                float rx = x0 + (q.z0 + 12f) * sx, rw = (q.z1 - q.z0) * sx, rh = 2f * q.half * sy;
                R(rx - 2f, cy - rh / 2f - 2f, rw + 4f, rh + 4f, Color.black);
                R(rx, cy - rh / 2f, rw, rh, q.map);
                L(st, (i + 1).ToString(), rx, cy - 30f * s, rw, 60f * s, 40, C, Color.white);
                if (i > 0) R(rx - 4f * s, cy - 3.5f * sy, 8f * s, 7f * sy, new Color(1f, 1f, 1f, 0.9f));
            }
            float px = x0 + (player.position.z + 12f) * sx, py = cy + player.position.x * sy;
            R(px - 6f * s, py - 6f * s, 12f * s, 12f * s, new Color(1f, 0.9f, 0.1f));
            L(st, "TÚ", px - 30f * s, py - 30f * s, 60f * s, 20f * s, 16, C, new Color(1f, 0.9f, 0.1f));

            float cw = (w - 4f * 10f * s) / 5f, ch = H * 0.36f, cyy = H - ch - 40f * s;
            for (int i = 0; i < Zones.All.Length; i++)
            {
                ZoneInfo q = Zones.All[i];
                float cx = x0 + i * (cw + 10f * s);
                R(cx, cyy, cw, ch, new Color(0.12f, 0.12f, 0.14f, 1f));
                R(cx, cyy, cw, 8f * s, q.map);
                float ty = cyy + 14f * s;
                if (i == 0 && thumb != null)
                {
                    GUI.DrawTexture(new Rect(cx + 6f * s, ty, cw - 12f * s, ch * 0.34f), thumb, ScaleMode.ScaleAndCrop);
                    ty += ch * 0.34f + 4f * s;
                }
                L(stw, (i + 1) + ". " + q.name, cx + 8f * s, ty, cw - 16f * s, 50f * s, 18, TextAnchor.UpperLeft, Color.white);
                L(stw, q.sub, cx + 8f * s, ty + 44f * s, cw - 16f * s, ch, 14, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.85f));
            }
            L(st, "M: cerrar mapa   ·   Teclas 1-5: ir a cada zona", 0f, H - 34f * s, W, 30f * s, 18, C, Color.white);
        }
    }
}
