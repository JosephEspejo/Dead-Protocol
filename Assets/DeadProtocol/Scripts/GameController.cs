using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadProtocol
{
    // DEAD PROTOCOL: menu (personaje y mapa), juego en primera persona con armas e infectados.
    public partial class GameController : MonoBehaviour
    {
        public static GameController I;
        public enum S { Title, Character, MapSel, Playing, Paused, Dead, Won }
        public S state = S.Title;
        public bool Live { get { return state == S.Playing && !showMap; } }
        public Transform CamT { get { return camT; } }

        static readonly int[] ZCount = { 46, 70, 60, 50, 56 };
        static readonly float[] ZFat = { 0.12f, 0.20f, 0.20f, 0.25f, 0.30f };

        struct Ind { public float ang, t; }

        Camera cam; Transform camT, player, fxRoot;
        CharacterController cc; Light torch, spotSel;
        AudioSource sfx, rainSrc;
        AudioClip thunder, hurtClip, groanA, groanB, stepClip, killClip, pickClip;
        Texture2D thumb, vig, softTex;
        GameObject[] models;
        Vector3 showPos = new Vector3(600f, 0f, 0f);
        Vector2 pointer;
        Atm cur;
        List<Pickup> pickups = new List<Pickup>();
        List<Ind> inds = new List<Ind>();
        WView[] views = new WView[4];
        int[] mag = new int[4], res = new int[4];

        int agentId = 0, mapSel = 5, curZone = -1, wIdx, kills, zoneStart;
        float hp = 100f, maxHp = 100f, yaw, pitch, bobT, shake, recoilPitch, hurtT, deadT, hitMark, hitKill;
        float reloadT, reloadDur = 1f, nextFire, flashT, kickAnim, bannerT, toastT, startHint, playTime, menuT;
        float zombieTick, checkTick, prevStep, tourZ, fovCur = 65f;
        bool locked, torchOn, showMap, tour, explore, flashing, reloading, moving, aiming;
        string toast = "";

        AgentDef Ag { get { return Agents.All[agentId]; } }

        void Awake()
        {
            I = this;
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
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
            camT = cg.transform;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 40f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 220f;

            Zones.BuildAll();
            World.Rain(camT);
            fxRoot = new GameObject("Efectos").transform;
            FxPool.Init(fxRoot);

            GameObject tg = new GameObject("Linterna");
            tg.transform.SetParent(camT, false);
            torch = tg.AddComponent<Light>();
            torch.type = LightType.Spot;
            torch.range = 30f; torch.spotAngle = 45f; torch.intensity = 4f;
            torch.color = new Color(1f, 0.95f, 0.85f);
            torch.enabled = false;

            rainSrc = gameObject.AddComponent<AudioSource>();
            rainSrc.clip = U.RainLoop(); rainSrc.loop = true; rainSrc.volume = 0.2f; rainSrc.spatialBlend = 0f;
            rainSrc.Play();
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.spatialBlend = 0f;
            thunder = U.Noise(2.5f, 45f, 0.7f, 1.4f);
            hurtClip = U.Noise(0.25f, 110f, 0.45f, 10f);
            groanA = U.Noise(0.8f, 62f, 0.15f, 3f);
            groanB = U.Noise(1.0f, 88f, 0.25f, 2.5f);
            stepClip = U.Noise(0.07f, 90f, 0.9f, 45f);
            killClip = U.Noise(0.3f, 55f, 0.3f, 7f);
            pickClip = U.Noise(0.15f, 900f, 0.1f, 18f);
            thumb = Resources.Load<Texture2D>("zona1_calle");
            vig = U.Vignette();
            softTex = U.Soft();

            BuildShowroom();
            for (int i = 0; i < 4; i++) { mag[i] = Weapons.All[i].mag; res[i] = Weapons.All[i].reserve; }
            state = S.Title;
            StartCoroutine(Storm());
        }

        // ---------- Sala de seleccion de personajes ----------
        void BuildShowroom()
        {
            Transform r = new GameObject("SalaPersonajes").transform;
            r.position = showPos;
            U.Prim(PrimitiveType.Cylinder, r, new Vector3(0f, -0.05f, 0f), new Vector3(16f, 0.05f, 16f), U.Mat(new Color(0.16f, 0.16f, 0.18f), 0f, 0.5f));
            U.Prim(PrimitiveType.Cube, r, new Vector3(0f, 5f, 6f), new Vector3(34f, 12f, 0.5f), U.Mat(new Color(0.20f, 0.21f, 0.24f)));
            models = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                models[i] = Agents.BuildModel(i);
                models[i].transform.SetParent(r, false);
                models[i].transform.localPosition = new Vector3((i - 1.5f) * 1.9f, 0f, 0f);
                models[i].transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                U.Prim(PrimitiveType.Cylinder, r, new Vector3((i - 1.5f) * 1.9f, 0.0f, 0f), new Vector3(1.4f, 0.02f, 1.4f), U.Mat(new Color(0.3f, 0.05f, 0.07f), 0.6f));
            }
            B.PLight(r, showPos + new Vector3(-4f, 3f, -4f), new Color(1f, 0.95f, 0.9f), 16f, 3f, false);
            B.PLight(r, showPos + new Vector3(4f, 3f, -4f), new Color(0.85f, 0.9f, 1f), 16f, 3f, false);
            GameObject sg = new GameObject("FocoSeleccion");
            sg.transform.SetParent(r, false);
            spotSel = sg.AddComponent<Light>();
            spotSel.type = LightType.Spot;
            spotSel.range = 14f; spotSel.spotAngle = 38f; spotSel.intensity = 9f;
            spotSel.color = new Color(1f, 0.9f, 0.8f);
        }

        IEnumerator Storm()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(6f, 15f));
                if (cur.rain < 0.5f || state == S.Title || state == S.Character) continue;
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

        void Atmo(float z, bool showroom)
        {
            if (showroom)
            {
                RenderSettings.fog = false;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.40f);
                cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
                if (World.Sun != null) World.Sun.intensity = 0.3f;
                if (World.RainPS != null) { ParticleSystem.EmissionModule e0 = World.RainPS.emission; e0.rateOverTime = 0f; }
                rainSrc.volume = 0.04f;
                cur = new Atm();
                return;
            }
            cur = Zones.Sample(z);
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
            rainSrc.volume = 0.3f * cur.rain + 0.04f;
        }

        void Teleport(Vector3 p)
        {
            cc.enabled = false;
            player.position = p;
            cc.enabled = !tour;
        }

        public void PlayAt(AudioClip c, Vector3 p, float v)
        {
            if (c != null) AudioSource.PlayClipAtPoint(c, p, v);
        }

        public void Groan(Vector3 p)
        {
            PlayAt(Random.value < 0.5f ? groanA : groanB, p, 0.7f);
        }

        // ---------- Iniciar / terminar partida ----------
        void ClearWorldObjects()
        {
            foreach (Zombie z in new List<Zombie>(Zombie.All))
                if (z != null) DestroyImmediate(z.gameObject);
            Zombie.All.Clear();
            foreach (Pickup p in pickups)
                if (p != null) DestroyImmediate(p.gameObject);
            pickups.Clear();
        }

        void BuildViews()
        {
            for (int i = 0; i < views.Length; i++)
                if (views[i] != null && views[i].root != null) Destroy(views[i].root);
            AgentDef a = Ag;
            for (int i = 0; i < 4; i++) views[i] = Weapons.BuildView(camT, i, a.suit, a.skin);
        }

        void StartGame()
        {
            Time.timeScale = 1f;
            ClearWorldObjects();
            AgentDef a = Ag;
            maxHp = hp = explore ? 100f : a.hp;
            kills = 0; playTime = 0f; hurtT = 0f; shake = 0f; recoilPitch = 0f; deadT = 0f;
            hitMark = 0f; hitKill = 0f; flashT = 0f; kickAnim = 0f;
            inds.Clear();
            reloading = false; wIdx = a.startWeapon;
            for (int i = 0; i < 4; i++) { mag[i] = Weapons.All[i].mag; res[i] = Weapons.All[i].reserve; }
            BuildViews();
            zoneStart = (mapSel == 5 || explore) ? 0 : mapSel;
            tour = false;
            Teleport(new Vector3(0f, 0f, Zones.StartZ(zoneStart)));
            yaw = 0f; pitch = 0f; curZone = -1; fovCur = 65f;
            cam.fieldOfView = 65f;
            if (!explore)
            {
                Physics.SyncTransforms();
                Zombie.Target = player;
                PopulateZombies();
                PlacePickups();
            }
            state = S.Playing;
            showMap = false; locked = true;
            toast = ""; toastT = 0f; startHint = 14f;
        }

        void ToMenu()
        {
            ClearWorldObjects();
            Time.timeScale = 1f;
            explore = false; locked = false; showMap = false;
            state = S.Title;
        }

        void Pause()
        {
            state = S.Paused;
            Time.timeScale = 0f;
            locked = false;
        }

        void Resume()
        {
            state = S.Playing;
            Time.timeScale = 1f;
            locked = true;
        }

        void Win()
        {
            state = S.Won; locked = false; deadT = 0f;
        }

        void Die()
        {
            state = S.Dead; locked = false; deadT = 0f;
            reloading = false;
        }

        // ---------- Infectados y objetos por todo el mapa ----------
        bool Spot(int z, bool forZombie, out Vector3 p)
        {
            for (int t = 0; t < 30; t++)
            {
                float x = 0f, zz = 0f;
                switch (z)
                {
                    case 0:
                        x = Random.Range(-4.3f, 4.3f); zz = Random.Range(8f, 120f);
                        break;
                    case 1:
                        x = Random.Range(-21f, 21f); zz = Random.Range(130f, 206f);
                        break;
                    case 2:
                        {
                            float r = Random.value;
                            if (r < 0.28f) { x = Random.Range(-9.5f, 9.5f); zz = Random.Range(213f, 233f); }
                            else if (r < 0.6f) { x = Random.Range(-2.3f, 2.3f); zz = Random.Range(239f, 289f); }
                            else
                            {
                                int k = Random.Range(0, 5);
                                float sd = Random.value < 0.5f ? -1f : 1f;
                                x = sd * Random.Range(5f, 10f); zz = 236f + k * 11f + Random.Range(2f, 9f);
                            }
                        }
                        break;
                    case 3:
                        x = Random.Range(-3.6f, 3.6f); zz = Random.Range(306f, 386f);
                        break;
                    default:
                        x = Random.Range(-12f, 12f); zz = Random.Range(396f, 467f);
                        break;
                }
                p = new Vector3(x, 0.02f, zz);
                if (z == 1 && new Vector2(x, zz - 167f).magnitude < 8f) continue;
                if (z == 4 && Mathf.Abs(x) < 3.2f && Mathf.Abs(zz - 459f) < 3.2f) continue;
                if (forZombie && Vector3.Distance(p, player.position) < 16f) continue;
                if (Physics.CheckSphere(p + Vector3.up * 0.95f, 0.45f)) continue;
                return true;
            }
            p = Vector3.zero;
            return false;
        }

        void PopulateZombies()
        {
            int from = mapSel == 5 ? 0 : mapSel, to = mapSel == 5 ? 4 : mapSel;
            for (int z = from; z <= to; z++)
            {
                for (int i = 0; i < ZCount[z]; i++)
                {
                    Vector3 p;
                    if (!Spot(z, true, out p)) continue;
                    int k = Random.value < ZFat[z] ? 2 : (Random.value < 0.4f ? 1 : 0);
                    Zombie zb = Zombie.Spawn(k, p);
                    zb.gameObject.SetActive(Vector3.Distance(p, player.position) < 70f);
                }
            }
        }

        void PlacePickups()
        {
            int from = mapSel == 5 ? 0 : mapSel, to = mapSel == 5 ? 4 : mapSel;
            for (int z = from; z <= to; z++)
            {
                for (int i = 0; i < 9; i++)
                {
                    Vector3 p;
                    if (Spot(z, false, out p)) pickups.Add(Pickup.Create(i < 6 ? 0 : 1, p));
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector3 p;
                    if (Spot(z, false, out p)) pickups.Add(Pickup.Create(1, p));
                }
            }
        }

        void ManageZombies()
        {
            Vector3 pp = player.position;
            for (int i = Zombie.All.Count - 1; i >= 0; i--)
            {
                Zombie z = Zombie.All[i];
                if (z == null) { Zombie.All.RemoveAt(i); continue; }
                float dx = z.transform.position.x - pp.x, dz = z.transform.position.z - pp.z;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                bool act = z.gameObject.activeSelf;
                if (act && d > 85f) z.gameObject.SetActive(false);
                else if (!act && d < 72f) z.gameObject.SetActive(true);
            }
        }

        void AlertZombies(Vector3 p, float r)
        {
            float r2 = r * r;
            foreach (Zombie z in Zombie.All)
            {
                if (z == null || !z.gameObject.activeSelf) continue;
                Vector3 d = z.transform.position - p;
                d.y = 0f;
                if (d.sqrMagnitude < r2) z.aggro = true;
            }
        }

        void CheckWin()
        {
            if (state != S.Playing || explore) return;
            if (mapSel == 5)
            {
                if (curZone != 4) return;
                foreach (Zombie z in Zombie.All)
                    if (z != null && z.transform.position.z > 390f) return;
                Win();
            }
            else if (Zombie.All.Count == 0) Win();
        }

        void UpdatePickups()
        {
            Vector3 pp = player.position;
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                Pickup p = pickups[i];
                if (p == null) { pickups.RemoveAt(i); continue; }
                Vector3 d = p.transform.position - pp;
                d.y = 0f;
                if (d.sqrMagnitude > 1.4f * 1.4f) continue;
                if (p.kind == 0)
                {
                    for (int w = 0; w < 4; w++)
                        res[w] = Mathf.Min(Weapons.All[w].reserveMax, res[w] + Mathf.Max(2, (int)(Weapons.All[w].mag * 1.5f)));
                    toast = "+ MUNICIÓN"; toastT = 2f;
                }
                else
                {
                    if (hp >= maxHp) continue;
                    hp = Mathf.Min(maxHp, hp + 40f);
                    toast = "+ SALUD"; toastT = 2f;
                }
                sfx.pitch = 1f;
                sfx.PlayOneShot(pickClip, 0.8f);
                Destroy(p.gameObject);
                pickups.RemoveAt(i);
            }
        }

        // ---------- Dano al jugador y muerte de infectados ----------
        public void PlayerHit(float dmg, Vector3 from)
        {
            if (state != S.Playing || explore) return;
            hp -= dmg * Ag.takenMul;
            hurtT = 0.7f;
            shake = Mathf.Max(shake, 0.8f);
            sfx.pitch = 1f;
            sfx.PlayOneShot(hurtClip, 1f);
            Vector3 f = camT.forward;
            f.y = 0f;
            Vector3 to = from - player.position;
            to.y = 0f;
            if (f.sqrMagnitude > 0.001f && to.sqrMagnitude > 0.001f)
            {
                Ind n;
                n.ang = Vector3.SignedAngle(f.normalized, to.normalized, Vector3.up);
                n.t = 1.3f;
                inds.Add(n);
            }
            if (hp <= 0f) { hp = 0f; Die(); }
        }

        public void OnKill(Zombie z, bool head)
        {
            kills++;
            hitKill = 0.35f;
            PlayAt(killClip, z.transform.position, 0.8f);
        }

        // ---------- Armas ----------
        void SwitchTo(int i)
        {
            if (i < 0 || i > 3 || i == wIdx) return;
            wIdx = i;
            reloading = false;
            nextFire = Time.time + 0.2f;
        }

        void StartReload()
        {
            WeaponDef w = Weapons.All[wIdx];
            if (reloading || mag[wIdx] >= w.mag || res[wIdx] <= 0) return;
            reloading = true;
            reloadDur = w.reload * Ag.reloadMul;
            reloadT = reloadDur;
            sfx.pitch = 1f;
            sfx.PlayOneShot(Weapons.ReloadClip, 0.8f);
        }

        void FinishReload()
        {
            reloading = false;
            WeaponDef w = Weapons.All[wIdx];
            int take = Mathf.Min(w.mag - mag[wIdx], res[wIdx]);
            mag[wIdx] += take;
            res[wIdx] -= take;
        }

        void Fire(WeaponDef w)
        {
            if (reloading) return;
            if (mag[wIdx] <= 0)
            {
                sfx.pitch = 1f;
                sfx.PlayOneShot(Weapons.EmptyClip, 0.8f);
                nextFire = Time.time + 0.3f;
                StartReload();
                return;
            }
            mag[wIdx]--;
            nextFire = Time.time + w.rate;
            sfx.pitch = Random.Range(0.93f, 1.07f);
            sfx.PlayOneShot(w.clip, 0.9f);
            flashT = 0.05f;
            kickAnim = 1f;
            recoilPitch += w.kick * (aiming ? 0.6f : 1f);
            shake = Mathf.Max(shake, w.shake * 8f);
            AlertZombies(player.position, 45f);

            AgentDef a = Ag;
            WView v = views[wIdx];
            Vector3 muz = v != null ? v.muzzle.position : camT.position;
            float sp = w.spread * (moving ? 1.6f : 1f) * (aiming ? 0.5f : 1f);
            for (int i = 0; i < w.pellets; i++)
            {
                Quaternion q = camT.rotation * Quaternion.Euler(Random.Range(-sp, sp), Random.Range(-sp, sp), 0f);
                Vector3 dir = q * Vector3.forward;
                RaycastHit h;
                Vector3 end;
                if (Physics.Raycast(camT.position, dir, out h, w.range, ~0, QueryTriggerInteraction.Ignore))
                {
                    end = h.point;
                    Zombie z = h.collider.GetComponentInParent<Zombie>();
                    if (z != null && !z.dead)
                    {
                        bool head = (h.point.y - z.transform.position.y) > z.headY;
                        float dmg = w.dmg * a.dmgMul * (head ? (w.hsMul + a.hsBonus) : 1f);
                        if (w.pellets > 1) dmg *= Mathf.Clamp01(1.25f - h.distance / w.range);
                        z.Hit(dmg, head, h.point, dir);
                        hitMark = 0.18f;
                    }
                    else FxPool.Spark(h.point, h.normal);
                }
                else end = camT.position + dir * w.range;
                if (i % 3 == 0) FxPool.Tracer(muz, end);
            }
            if (mag[wIdx] <= 0 && res[wIdx] > 0) StartReload();
        }

        void HandleWeapons(float dt)
        {
            int nk = U.NumKey();
            if (nk >= 1 && nk <= 4) SwitchTo(nk - 1);
            float sc = U.Wheel();
            if (sc > 0f) SwitchTo((wIdx + 1) % 4);
            else if (sc < 0f) SwitchTo((wIdx + 3) % 4);
            if (U.Reload()) StartReload();
            WeaponDef w = Weapons.All[wIdx];
            if (reloading)
            {
                reloadT -= dt;
                if (reloadT <= 0f) FinishReload();
            }
            bool want = w.auto ? U.ClickHeld() : U.Click();
            if (want && Time.time >= nextFire) Fire(w);
            aiming = U.RightHeld() && !reloading;
        }

        void UpdateWeaponView(float dt)
        {
            for (int i = 0; i < 4; i++)
                if (views[i] != null && views[i].root != null)
                    views[i].root.SetActive(i == wIdx && (state == S.Playing || state == S.Paused) && !explore);
            WView v = views[wIdx];
            if (v == null || v.root == null) return;
            kickAnim = Mathf.MoveTowards(kickAnim, 0f, dt * 8f);
            float rl = reloading ? Mathf.Sin(Mathf.Clamp01(1f - reloadT / reloadDur) * Mathf.PI) : 0f;
            float sway = moving ? Mathf.Sin(bobT * 0.5f) * 0.012f : Mathf.Sin(Time.time * 1.3f) * 0.004f;
            float ax = aiming ? -0.18f : 0f, ay = aiming ? 0.07f : 0f;
            v.root.transform.localPosition = new Vector3(0.26f + sway + ax, -0.26f - rl * 0.2f + kickAnim * 0.015f + Mathf.Abs(sway) + ay, 0.6f - kickAnim * 0.09f);
            v.root.transform.localRotation = Quaternion.Euler(-kickAnim * 8f + rl * 40f, 0f, rl * 10f);
            bool fl = flashT > 0f;
            flashT -= dt;
            v.flash.SetActive(fl);
            v.flashLight.enabled = fl;
        }

        // ---------- Bucle principal ----------
        void ApplyCursor()
        {
            bool free = state == S.Playing && locked && !showMap;
            Cursor.lockState = free ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !free;
        }

        void Update()
        {
            pointer = U.Pointer();
            float udt = Time.unscaledDeltaTime;
            menuT += udt;
            switch (state)
            {
                case S.Title:
                case S.Character:
                case S.MapSel:
                    MenuUpdate();
                    break;
                case S.Playing:
                    PlayUpdate(Time.deltaTime);
                    break;
                case S.Paused:
                    if (U.EscKey()) Resume();
                    else HandleBtn(ClickedBtn());
                    break;
                default:
                    deadT += udt;
                    if (deadT > 1.0f) HandleBtn(ClickedBtn());
                    break;
            }
            ApplyCursor();
            CameraUpdate(Time.deltaTime, udt);
            UpdateWeaponView(Time.deltaTime);
            toastT = Mathf.Max(0f, toastT - udt);
        }

        void MenuUpdate()
        {
            int ak = U.ArrowKey();
            if (state == S.Character && ak != 0) agentId = (agentId + ak + 4) % 4;
            if (state == S.MapSel && ak != 0) mapSel = (mapSel + ak + 6) % 6;
            if (U.EnterKey())
            {
                if (state == S.Title) HandleBtn(1);
                else if (state == S.Character) HandleBtn(4);
                else HandleBtn(30);
                return;
            }
            if (U.EscKey()) { HandleBtn(3); return; }
            HandleBtn(ClickedBtn());
        }

        int ClickedBtn()
        {
            if (!U.Click()) return 0;
            Vector2 g = new Vector2(pointer.x, Screen.height - pointer.y);
            foreach (BtnRec b in btns)
                if (b.r.Contains(g)) return b.id;
            return 0;
        }

        void HandleBtn(int id)
        {
            if (id == 0) return;
            if (id >= 10 && id < 14) { agentId = id - 10; return; }
            if (id >= 20 && id < 26) { mapSel = id - 20; return; }
            switch (id)
            {
                case 1: explore = false; state = S.Character; break;
                case 2: explore = true; agentId = 0; mapSel = 5; StartGame(); break;
                case 3:
                    if (state == S.Character) state = S.Title;
                    else if (state == S.MapSel) state = S.Character;
                    break;
                case 4: state = S.MapSel; break;
                case 30: StartGame(); break;
                case 40: Resume(); break;
                case 41: ToMenu(); break;
                case 42: StartGame(); break;
                case 43:
                    ClearWorldObjects();
                    Time.timeScale = 1f;
                    explore = false;
                    state = S.Character;
                    break;
            }
        }

        void BoundsFix()
        {
            Vector3 p = player.position;
            float hw = Zones.HalfAt(p.z);
            float zmin = -9f, zmax = 468f;
            if (!explore && mapSel != 5)
            {
                ZoneInfo zi = Zones.All[mapSel];
                zmin = mapSel == 0 ? -9f : zi.z0 + 1.2f;
                zmax = zi.z1 - 1.2f;
            }
            float cx = Mathf.Clamp(p.x, -hw, hw), cz = Mathf.Clamp(p.z, zmin, zmax);
            if (cx != p.x || cz != p.z || p.y > 0.5f) Teleport(new Vector3(cx, 0f, cz));
        }

        void PlayUpdate(float dt)
        {
            if (U.EscKey())
            {
                if (showMap) { showMap = false; Time.timeScale = 1f; locked = true; }
                else Pause();
                return;
            }
            if (U.MapKey())
            {
                showMap = !showMap;
                Time.timeScale = showMap ? 0f : 1f;
                locked = !showMap;
                return;
            }
            if (showMap) return;
            if (!locked) { if (U.Click()) locked = true; return; }

            playTime += dt;
            startHint = Mathf.Max(0f, startHint - dt);
            hurtT = Mathf.Max(0f, hurtT - dt);
            hitMark = Mathf.Max(0f, hitMark - dt);
            hitKill = Mathf.Max(0f, hitKill - dt);
            bannerT = Mathf.Max(0f, bannerT - dt);
            for (int i = inds.Count - 1; i >= 0; i--)
            {
                Ind n = inds[i];
                n.t -= dt;
                if (n.t <= 0f) inds.RemoveAt(i); else inds[i] = n;
            }
            if (U.LightKey()) { torchOn = !torchOn; torch.enabled = torchOn; }

            if (explore && U.TourKey()) { tour = !tour; if (tour) tourZ = player.position.z; cc.enabled = !tour; }
            int nk = explore ? U.NumKey() : 0;
            if (nk > 0) { Teleport(new Vector3(0f, 0f, Zones.StartZ(nk - 1))); yaw = 0f; tourZ = player.position.z; }

            if (tour)
            {
                tourZ += 4f * dt;
                if (tourZ > 455f) tourZ = -6f;
                float f = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(145f, 155f, tourZ)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(180f, 190f, tourZ)));
                player.position = new Vector3(Mathf.Sin(Time.time * 0.3f) * 0.8f + 11f * f, 0f, tourZ);
                yaw = Mathf.Sin(Time.time * 0.25f) * 18f - 25f * f;
                pitch = 2f + Mathf.Sin(Time.time * 0.2f) * 4f;
                bobT += dt * 9f;
                moving = true;
            }
            else
            {
                Vector2 md = U.MouseDelta();
                float sens = aiming ? 0.07f : 0.12f;
                yaw += md.x * sens;
                pitch = Mathf.Clamp(pitch - md.y * sens, -85f, 85f);
                Vector2 a = U.Axis();
                bool sprint = U.Shift() && a.y > 0.1f;
                float sp = explore ? (U.Shift() ? 7f : 3.6f) : Ag.speed * (sprint ? 1.7f : 1f) * (aiming ? 0.7f : 1f);
                Vector3 fwd = new Vector3(Mathf.Sin(yaw * Mathf.Deg2Rad), 0f, Mathf.Cos(yaw * Mathf.Deg2Rad));
                Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);
                Vector3 mv = fwd * a.y + right * a.x;
                if (mv.sqrMagnitude > 1f) mv.Normalize();
                Vector3 d = mv * sp * dt;
                d.y = -0.1f;
                if (cc.enabled) cc.Move(d);
                BoundsFix();
                moving = mv.sqrMagnitude > 0.01f;
                if (moving)
                {
                    bobT += dt * (sprint ? 12f : 9f);
                    float ph = Mathf.Sin(bobT);
                    if (prevStep < 0f && ph >= 0f) { sfx.pitch = Random.Range(0.9f, 1.1f); sfx.PlayOneShot(stepClip, 0.25f); }
                    prevStep = ph;
                }
            }

            int z = Zones.IndexAt(player.position.z);
            if (z != curZone) { curZone = z; bannerT = 4f; }

            if (!explore)
            {
                HandleWeapons(dt);
                UpdatePickups();
                zombieTick -= dt;
                if (zombieTick <= 0f) { zombieTick = 0.3f; ManageZombies(); }
                checkTick -= dt;
                if (checkTick <= 0f) { checkTick = 0.5f; CheckWin(); }
            }
        }

        void CameraUpdate(float dt, float udt)
        {
            if (state == S.Title || state == S.Character || state == S.MapSel)
            {
                cam.fieldOfView = 40f;
                if (state == S.MapSel)
                {
                    int zi = mapSel == 5 ? 0 : mapSel;
                    float z0 = Zones.StartZ(zi);
                    camT.position = new Vector3(Mathf.Sin(menuT * 0.2f) * 1.2f, 2.0f, z0 + Mathf.Sin(menuT * 0.15f) * 2f);
                    camT.rotation = Quaternion.Euler(4f, Mathf.Sin(menuT * 0.25f) * 25f, 0f);
                    Atmo(z0, false);
                }
                else
                {
                    bool ch = state == S.Character;
                    float sel = ch ? (agentId - 1.5f) * 1.9f * 0.5f : Mathf.Sin(menuT * 0.3f) * 0.6f;
                    Vector3 tgt = showPos + new Vector3(sel, ch ? 0.1f : 0.6f, 0f);
                    Vector3 pos = showPos + new Vector3(sel, ch ? 1.1f : 1.4f, ch ? -6.4f : -7.2f);
                    if ((camT.position - pos).sqrMagnitude > 2500f) camT.position = pos;
                    else camT.position = Vector3.Lerp(camT.position, pos, 1f - Mathf.Exp(-4f * udt));
                    camT.rotation = Quaternion.LookRotation(tgt - camT.position);
                    Atmo(0f, true);
                    for (int i = 0; i < 4; i++)
                    {
                        bool s = ch && i == agentId;
                        models[i].transform.localRotation = Quaternion.Euler(0f, 180f + (s ? Mathf.Sin(menuT * 1.2f) * 18f : 0f), 0f);
                        models[i].transform.localScale = Vector3.one * (s ? 1.04f : 1f);
                    }
                    spotSel.intensity = ch ? 9f : 0f;
                    spotSel.transform.position = showPos + new Vector3((agentId - 1.5f) * 1.9f, 5f, -2.5f);
                    spotSel.transform.LookAt(showPos + new Vector3((agentId - 1.5f) * 1.9f, 1f, 0f));
                }
                return;
            }

            Atmo(player.position.z, false);
            if (state == S.Dead) deadT = Mathf.Max(deadT, 0f);
            float bob = moving ? Mathf.Sin(bobT) * 0.03f : Mathf.Sin(Time.time * 1.2f) * 0.01f;
            Vector3 eye = player.position + new Vector3(0f, 1.65f + bob, 0f);
            float roll = 0f;
            if (state == S.Dead)
            {
                float k = Mathf.Clamp01(deadT / 0.8f);
                eye.y = 1.65f - k * 1.2f;
                roll = k * 65f;
            }
            if (shake > 0f)
            {
                eye += Random.insideUnitSphere * shake * 0.08f;
                shake = Mathf.Max(0f, shake - dt * 2.5f);
            }
            recoilPitch = Mathf.MoveTowards(recoilPitch, 0f, dt * 14f);
            camT.position = eye;
            camT.rotation = Quaternion.Euler(pitch - recoilPitch, yaw, roll);
            float tf = (aiming && !explore) ? Weapons.All[wIdx].zoom : 65f;
            fovCur = Mathf.Lerp(fovCur, tf, 1f - Mathf.Exp(-12f * udt));
            cam.fieldOfView = fovCur;
        }
    }
}
