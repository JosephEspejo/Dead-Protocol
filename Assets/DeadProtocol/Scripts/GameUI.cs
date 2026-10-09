using System.Collections.Generic;
using UnityEngine;

namespace DeadProtocol
{
    // Interfaz: menus, HUD, pausa y pantallas finales (OnGUI, sin Canvas)
    public partial class GameController
    {
        struct BtnRec { public int id; public Rect r; }
        List<BtnRec> btns = new List<BtnRec>();
        List<BtnRec> btnsNext = new List<BtnRec>();
        GUIStyle st, stw;
        static readonly Color RED = new Color(0.9f, 0.12f, 0.14f);
        static readonly Color YEL = new Color(1f, 0.85f, 0.3f);
        static readonly Color GRN = new Color(0.3f, 1f, 0.45f);
        static readonly int[] Diff = { 2, 3, 4, 4, 5, 5 };

        void L(GUIStyle sty, string t, float x, float y, float w, float h, int size, TextAnchor an, Color c)
        {
            sty.fontSize = Mathf.RoundToInt(size * Screen.height / 720f);
            sty.alignment = an;
            sty.normal.textColor = new Color(0f, 0f, 0f, c.a);
            GUI.Label(new Rect(x + 2f, y + 2f, w, h), t, sty);
            sty.normal.textColor = c;
            GUI.Label(new Rect(x, y, w, h), t, sty);
        }

        void R(float x, float y, float w, float h, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void Reg(int id, float x, float y, float w, float h)
        {
            BtnRec b;
            b.id = id;
            b.r = new Rect(x, y, w, h);
            btnsNext.Add(b);
        }

        bool Hover(float x, float y, float w, float h)
        {
            return new Rect(x, y, w, h).Contains(new Vector2(pointer.x, Screen.height - pointer.y));
        }

        void Button(int id, float x, float y, float w, float h, string text, int size, bool primary)
        {
            Reg(id, x, y, w, h);
            bool hov = Hover(x, y, w, h);
            Color bg = primary
                ? (hov ? new Color(0.95f, 0.15f, 0.18f, 0.97f) : new Color(0.70f, 0.08f, 0.10f, 0.94f))
                : (hov ? new Color(0.38f, 0.38f, 0.44f, 0.97f) : new Color(0.14f, 0.14f, 0.17f, 0.94f));
            R(x - 2f, y - 2f, w + 4f, h + 4f, new Color(0f, 0f, 0f, 0.85f));
            R(x, y, w, h, bg);
            L(st, text, x, y, w, h, size, TextAnchor.MiddleCenter, Color.white);
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            if (st == null)
            {
                st = new GUIStyle(GUI.skin.label); st.fontStyle = FontStyle.Bold;
                stw = new GUIStyle(GUI.skin.label); stw.fontStyle = FontStyle.Bold; stw.wordWrap = true;
            }
            btnsNext.Clear();
            float W = Screen.width, H = Screen.height, s = H / 720f;

            switch (state)
            {
                case S.Title: DrawTitle(W, H, s); break;
                case S.Character: DrawCharacter(W, H, s); break;
                case S.MapSel: DrawMapSel(W, H, s); break;
                default:
                    if (showMap) DrawMap(W, H, s);
                    else
                    {
                        DrawHud(W, H, s);
                        if (state == S.Paused) DrawPause(W, H, s);
                        else if (state == S.Dead || state == S.Won) DrawEnd(W, H, s);
                    }
                    break;
            }
            List<BtnRec> t = btns;
            btns = btnsNext;
            btnsNext = t;
        }

        // ---------- Pantalla de titulo ----------
        void DrawTitle(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter;
            R(0f, 0f, W, H * 0.24f, new Color(0f, 0f, 0f, 0.55f));
            L(st, "DEAD PROTOCOL", 0f, H * 0.02f, W, 90f * s, 78, C, RED);
            L(st, "Agentes de élite contra una ciudad infectada", 0f, H * 0.02f + 84f * s, W, 34f * s, 24, C, Color.white);
            float bw = 440f * s, bh = 56f * s, bx = (W - bw) / 2f, by = H * 0.68f;
            Button(1, bx, by, bw, bh, "JUGAR", 32, true);
            Button(2, bx, by + bh + 12f * s, bw, bh * 0.8f, "RECORRIDO LIBRE (sin infectados)", 20, false);
            R(0f, H - 74f * s, W, 74f * s, new Color(0f, 0f, 0f, 0.62f));
            L(st, "WASD mover  ·  Shift correr  ·  Mouse apuntar  ·  Clic disparar  ·  Clic derecho mira  ·  R recargar  ·  1-4 armas  ·  F linterna  ·  M mapa  ·  Esc pausa",
                0f, H - 70f * s, W, 28f * s, 15, C, new Color(1f, 1f, 1f, 0.85f));
            L(st, "Yoann Sebastian Cachi Felipe  ·  Joshep Klaws Espejo Mamani  ·  Carlos Jesús Guevara Apaza",
                0f, H - 38f * s, W, 28f * s, 16, C, new Color(1f, 0.85f, 0.3f, 0.9f));
        }

        // ---------- Seleccion de personaje ----------
        void Stat(string label, float v, float x, float y, float w, float s)
        {
            L(st, label, x, y, 90f * s, 20f * s, 13, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f));
            float bx = x + 92f * s, bw = w - 92f * s;
            R(bx, y + 4f * s, bw, 12f * s, new Color(0.25f, 0.25f, 0.28f));
            R(bx, y + 4f * s, bw * Mathf.Clamp01(v), 12f * s, Color.Lerp(new Color(0.9f, 0.5f, 0.1f), GRN, Mathf.Clamp01(v)));
        }

        void DrawCharacter(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter, Lf = TextAnchor.UpperLeft;
            R(0f, 0f, W, 66f * s, new Color(0f, 0f, 0f, 0.62f));
            L(st, "ELIGE TU PERSONAJE", 0f, 8f * s, W, 50f * s, 36, C, YEL);
            float gap = 14f * s, cw = (W - 5f * gap) / 4f, ch = 255f * s, cy = H - 320f * s;
            for (int i = 0; i < 4; i++)
            {
                AgentDef a = Agents.All[i];
                float cx = gap + i * (cw + gap);
                bool sel = i == agentId;
                Reg(10 + i, cx, cy, cw, ch);
                bool hov = Hover(cx, cy, cw, ch);
                R(cx - 3f * s, cy - 3f * s, cw + 6f * s, ch + 6f * s, sel ? RED : (hov ? new Color(0.5f, 0.5f, 0.55f) : new Color(0f, 0f, 0f, 0.85f)));
                R(cx, cy, cw, ch, new Color(0.09f, 0.09f, 0.11f, 0.95f));
                R(cx, cy, cw, 6f * s, Color.Lerp(a.accent, Color.white, 0.3f));
                L(st, a.name, cx + 10f * s, cy + 10f * s, cw - 20f * s, 28f * s, 22, Lf, Color.white);
                L(st, "\"" + a.nick + "\"", cx + 10f * s, cy + 38f * s, cw - 20f * s, 26f * s, 20, Lf, new Color(1f, 0.35f, 0.35f));
                L(stw, a.role, cx + 10f * s, cy + 64f * s, cw - 20f * s, 22f * s, 15, Lf, new Color(0.75f, 0.75f, 0.78f));
                Stat("VIDA", a.hp / 160f, cx + 10f * s, cy + 92f * s, cw - 20f * s, s);
                Stat("VELOCIDAD", a.speed / 4.2f, cx + 10f * s, cy + 116f * s, cw - 20f * s, s);
                Stat("DEFENSA", (1f / a.takenMul) / 1.45f, cx + 10f * s, cy + 140f * s, cw - 20f * s, s);
                L(stw, "Arma inicial: " + a.weaponName, cx + 10f * s, cy + 168f * s, cw - 20f * s, 22f * s, 15, Lf, YEL);
                L(stw, a.trait, cx + 10f * s, cy + 192f * s, cw - 20f * s, 62f * s, 13, Lf, new Color(0.85f, 0.85f, 0.85f));
            }
            Button(3, 20f * s, H - 56f * s, 200f * s, 44f * s, "ATRÁS", 22, false);
            Button(4, W - 280f * s, H - 56f * s, 260f * s, 44f * s, "CONTINUAR", 24, true);
            L(st, "← →  o clic para elegir  ·  Enter: continuar", 0f, H - 54f * s, W, 40f * s, 16, C, new Color(1f, 1f, 1f, 0.7f));
        }

        // ---------- Seleccion de mapa ----------
        void DrawMapSel(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter, Lf = TextAnchor.UpperLeft;
            R(0f, 0f, W, 66f * s, new Color(0f, 0f, 0f, 0.62f));
            L(st, "ELIGE EL MAPA", 0f, 8f * s, W, 50f * s, 36, C, YEL);
            L(st, "Personaje: " + Ag.name + " \"" + Ag.nick + "\"", 0f, 46f * s, W, 22f * s, 15, C, new Color(1f, 1f, 1f, 0.8f));
            float gap = 10f * s, cw = (W - 7f * gap) / 6f, ch = H * 0.54f, cy = H * 0.31f;
            for (int i = 0; i < 6; i++)
            {
                bool camp = i == 5;
                float cx = gap + i * (cw + gap);
                bool sel = i == mapSel;
                Color col = camp ? new Color(0.8f, 0.15f, 0.15f) : Zones.All[i].map;
                Reg(20 + i, cx, cy, cw, ch);
                bool hov = Hover(cx, cy, cw, ch);
                R(cx - 3f * s, cy - 3f * s, cw + 6f * s, ch + 6f * s, sel ? RED : (hov ? new Color(0.5f, 0.5f, 0.55f) : new Color(0f, 0f, 0f, 0.85f)));
                R(cx, cy, cw, ch, new Color(0.09f, 0.09f, 0.11f, 0.95f));
                R(cx, cy, cw, 6f * s, col);
                float ty = cy + 12f * s, ih = ch * 0.30f;
                if (i == 0 && thumb != null) GUI.DrawTexture(new Rect(cx + 6f * s, ty, cw - 12f * s, ih), thumb, ScaleMode.ScaleAndCrop);
                else
                {
                    R(cx + 6f * s, ty, cw - 12f * s, ih, new Color(col.r * 0.55f, col.g * 0.55f, col.b * 0.55f));
                    L(st, camp ? "1-5" : (i + 1).ToString(), cx, ty, cw, ih, camp ? 44 : 56, C, new Color(1f, 1f, 1f, 0.9f));
                }
                ty += ih + 6f * s;
                string nm = camp ? "CAMPAÑA COMPLETA" : (i + 1) + ". " + Zones.All[i].name;
                string ds = camp ? "Las 5 zonas conectadas, de principio a fin. Llega al laboratorio y elimina a los infectados de la Zona Cero." : Zones.All[i].sub;
                int n = 0;
                if (camp) { for (int k = 0; k < 5; k++) n += ZCount[k]; } else n = ZCount[i];
                L(stw, nm, cx + 8f * s, ty, cw - 16f * s, 44f * s, 16, Lf, Color.white);
                L(stw, ds, cx + 8f * s, ty + 44f * s, cw - 16f * s, ch * 0.34f, 12, Lf, new Color(0.82f, 0.82f, 0.82f));
                float by = cy + ch - 56f * s;
                L(st, "Infectados: " + n, cx + 8f * s, by, cw - 16f * s, 20f * s, 14, Lf, new Color(1f, 0.45f, 0.4f));
                L(st, "Dificultad", cx + 8f * s, by + 22f * s, 80f * s, 20f * s, 12, Lf, new Color(0.85f, 0.85f, 0.85f));
                for (int d = 0; d < 5; d++)
                    R(cx + 86f * s + d * 14f * s, by + 28f * s, 11f * s, 11f * s, d < Diff[i] ? YEL : new Color(0.3f, 0.3f, 0.33f));
            }
            Button(3, 20f * s, H - 56f * s, 200f * s, 44f * s, "ATRÁS", 22, false);
            Button(30, W - 280f * s, H - 56f * s, 260f * s, 44f * s, "¡A JUGAR!", 26, true);
            L(st, "← →  o clic para elegir  ·  Enter: jugar", 0f, H - 54f * s, W, 40f * s, 16, C, new Color(1f, 1f, 1f, 0.7f));
        }

        // ---------- HUD ----------
        void Cross(float x, float y, float s, Color c, float gapBoost)
        {
            float g = (6f + gapBoost) * s, l = 12f * s, t = 2f * s;
            R(x - g - l, y - t / 2f, l, t, c);
            R(x + g, y - t / 2f, l, t, c);
            R(x - t / 2f, y - g - l, t, l, c);
            R(x - t / 2f, y + g, t, l, c);
            R(x - t, y - t, t * 2f, t * 2f, c);
        }

        void DrawHud(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter, Lf = TextAnchor.MiddleLeft, Rt = TextAnchor.MiddleRight;
            AgentDef a = Ag;
            float cx = W / 2f, cy = H / 2f;

            if (explore)
            {
                L(st, "RECORRIDO LIBRE  -  Zona " + (curZone + 1) + ": " + Zones.All[Mathf.Max(0, curZone)].name, 20f * s, 12f * s, W, 28f * s, 20, Lf, Color.white);
                L(st, "WASD mover · Shift correr · F linterna · M mapa · 1-5 ir a una zona · C recorrido automático · Esc pausa", 20f * s, 38f * s, W, 24f * s, 15, Lf, new Color(1f, 1f, 1f, 0.85f));
                DrawBanner(W, H, s);
                return;
            }

            // efecto de dano: vineta roja + flechas de direccion
            float va = Mathf.Clamp01(hurtT / 0.7f) * 0.95f;
            if (state == S.Playing && hp / maxHp < 0.3f) va = Mathf.Max(va, 0.35f + 0.15f * Mathf.Sin(Time.unscaledTime * 6f));
            if (state == S.Dead) va = 0.9f;
            if (va > 0.01f)
            {
                GUI.color = new Color(1f, 1f, 1f, va);
                GUI.DrawTexture(new Rect(0f, 0f, W, H), vig);
                GUI.color = Color.white;
            }
            Matrix4x4 keep = GUI.matrix;
            foreach (Ind n in inds)
            {
                GUI.matrix = keep;
                GUIUtility.RotateAroundPivot(n.ang, new Vector2(cx, cy));
                R(cx - 40f * s, cy - 190f * s, 80f * s, 14f * s, new Color(1f, 0.1f, 0.1f, Mathf.Clamp01(n.t)));
            }
            GUI.matrix = keep;

            // mira y marcador de impacto
            if (state == S.Playing && !aiming)
            {
                float spread = (moving ? 8f : 0f) + kickAnim * 10f;
                Cross(cx, cy, s, Color.white, spread);
            }
            else if (state == S.Playing) R(cx - 2f * s, cy - 2f * s, 4f * s, 4f * s, new Color(1f, 0.2f, 0.2f));
            if (hitMark > 0f || hitKill > 0f)
            {
                Color hc = hitKill > 0f ? RED : Color.white;
                GUIUtility.RotateAroundPivot(45f, new Vector2(cx, cy));
                R(cx - 20f * s, cy - 1f * s, 11f * s, 2f * s, hc);
                R(cx + 9f * s, cy - 1f * s, 11f * s, 2f * s, hc);
                R(cx - 1f * s, cy - 20f * s, 2f * s, 11f * s, hc);
                R(cx - 1f * s, cy + 9f * s, 2f * s, 11f * s, hc);
                GUI.matrix = keep;
            }

            // arriba: infectados restantes y objetivo
            L(st, "INFECTADOS: " + Zombie.All.Count, 0f, 10f * s, W, 34f * s, 26, C, Color.white);
            L(st, mapSel == 5 ? "Objetivo: llega al Laboratorio y elimina a los infectados de la Zona Cero" : "Objetivo: elimina a todos los infectados de la zona",
                0f, 42f * s, W, 22f * s, 14, C, new Color(1f, 1f, 1f, 0.8f));
            if (startHint > 0f)
                L(st, "WASD mover · Shift correr · Clic disparar · Clic derecho apuntar · R recargar · 1-4 armas · F linterna · M mapa",
                    20f * s, 12f * s, W * 0.5f, 60f * s, 14, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, Mathf.Clamp01(startHint)));

            // vida (abajo izquierda)
            float hx = 24f * s, hy = H - 66f * s, hw = 340f * s, hh = 28f * s;
            float pct = Mathf.Clamp01(hp / maxHp);
            L(st, a.nick.ToUpper() + "  -  SALUD", hx, hy - 24f * s, hw, 22f * s, 16, Lf, Color.white);
            R(hx - 3f * s, hy - 3f * s, hw + 6f * s, hh + 6f * s, new Color(0f, 0f, 0f, 0.85f));
            R(hx, hy, hw, hh, new Color(0.18f, 0.05f, 0.05f));
            R(hx, hy, hw * pct, hh, Color.Lerp(RED, GRN, pct));
            L(st, Mathf.CeilToInt(hp) + " / " + Mathf.RoundToInt(maxHp), hx, hy, hw, hh, 18, C, Color.white);

            // arma y municion (abajo derecha)
            WeaponDef w = Weapons.All[wIdx];
            L(st, w.name, W - 330f * s, H - 150f * s, 310f * s, 26f * s, 20, Rt, YEL);
            L(st, mag[wIdx].ToString(), W - 330f * s, H - 128f * s, 200f * s, 54f * s, 46, Rt, mag[wIdx] == 0 ? RED : Color.white);
            L(st, "/ " + res[wIdx], W - 128f * s, H - 112f * s, 108f * s, 36f * s, 26, Rt, new Color(0.8f, 0.8f, 0.8f));
            if (reloading)
            {
                float rp = 1f - reloadT / reloadDur;
                R(W - 330f * s, H - 70f * s, 310f * s, 8f * s, new Color(0.2f, 0.2f, 0.2f, 0.9f));
                R(W - 330f * s, H - 70f * s, 310f * s * Mathf.Clamp01(rp), 8f * s, YEL);
                L(st, "RECARGANDO...", W - 330f * s, H - 98f * s, 310f * s, 24f * s, 17, Rt, Color.white);
            }
            for (int i = 0; i < 4; i++)
            {
                float bx = W - 20f * s - (4 - i) * 84f * s, by = H - 56f * s;
                bool sel = i == wIdx;
                R(bx - 2f * s, by - 2f * s, 80f * s, 30f * s, sel ? YEL : new Color(0f, 0f, 0f, 0.8f));
                R(bx, by, 76f * s, 26f * s, sel ? new Color(0.35f, 0.28f, 0.05f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.9f));
                string nm = Weapons.All[i].name;
                int sp = nm.IndexOf(' ');
                if (sp > 0) nm = nm.Substring(0, sp);
                L(st, (i + 1) + " " + nm, bx, by, 76f * s, 26f * s, 11, C, Color.white);
            }

            if (a.radar) DrawRadar(W, H, s);
            if (toastT > 0f) L(st, toast, 0f, H * 0.62f, W, 40f * s, 28, C, new Color(0.5f, 1f, 0.6f, Mathf.Clamp01(toastT)));
            DrawBanner(W, H, s);
        }

        void DrawBanner(float W, float H, float s)
        {
            if (bannerT <= 0f || curZone < 0) return;
            float al = Mathf.Clamp01(bannerT);
            TextAnchor C = TextAnchor.MiddleCenter;
            L(st, "ZONA " + (curZone + 1), 0f, H * 0.17f, W, 50f * s, 28, C, new Color(1f, 0.85f, 0.4f, al));
            L(st, Zones.All[curZone].name.ToUpper(), 0f, H * 0.17f + 45f * s, W, 80f * s, 54, C, new Color(1f, 0.95f, 0.9f, al));
        }

        void DrawRadar(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter;
            float R0 = 85f * s, cx = W - R0 - 24f * s, cy = R0 + 78f * s;
            GUI.color = new Color(0f, 0.12f, 0.06f, 0.8f);
            GUI.DrawTexture(new Rect(cx - R0, cy - R0, 2f * R0, 2f * R0), softTex);
            GUI.color = Color.white;
            R(cx - R0, cy - 0.5f, 2f * R0, 1f, new Color(0.3f, 1f, 0.45f, 0.25f));
            R(cx - 0.5f, cy - R0, 1f, 2f * R0, new Color(0.3f, 1f, 0.45f, 0.25f));
            float yr = yaw * Mathf.Deg2Rad;
            Vector2 f = new Vector2(Mathf.Sin(yr), Mathf.Cos(yr));
            Vector2 r = new Vector2(Mathf.Cos(yr), -Mathf.Sin(yr));
            foreach (Zombie z in Zombie.All)
            {
                if (z == null || !z.gameObject.activeSelf) continue;
                float dx = z.transform.position.x - player.position.x, dz = z.transform.position.z - player.position.z;
                if (dx * dx + dz * dz > 900f) continue;
                float fx = dx * f.x + dz * f.y, rx = dx * r.x + dz * r.y;
                float px = cx + rx / 30f * R0, py = cy - fx / 30f * R0;
                float sz = (z.kind == 2 ? 8f : 5f) * s;
                R(px - sz / 2f, py - sz / 2f, sz, sz, RED);
            }
            R(cx - 3f * s, cy - 3f * s, 6f * s, 6f * s, GRN);
            L(st, "RADAR", cx - R0, cy - R0 - 22f * s, 2f * R0, 20f * s, 14, C, GRN);
        }

        // ---------- Pausa y pantallas finales ----------
        void DrawPause(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter;
            R(0f, 0f, W, H, new Color(0f, 0f, 0f, 0.74f));
            L(st, "PAUSA", 0f, H * 0.10f, W, 80f * s, 64, C, YEL);
            float bw = 460f * s, bh = 52f * s, bx = (W - bw) / 2f, by = H * 0.28f;
            Button(40, bx, by, bw, bh, "CONTINUAR", 26, true);
            Button(42, bx, by + 66f * s, bw, bh, "REINICIAR PARTIDA", 22, false);
            Button(43, bx, by + 132f * s, bw, bh, "CAMBIAR PERSONAJE / MAPA", 22, false);
            Button(41, bx, by + 198f * s, bw, bh, "MENÚ PRINCIPAL", 22, false);
            L(st, "WASD mover · Shift correr · Clic disparar · Clic derecho apuntar · R recargar · 1-4 / rueda armas · F linterna · M mapa",
                0f, H - 60f * s, W, 30f * s, 15, C, new Color(1f, 1f, 1f, 0.8f));
        }

        void DrawEnd(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter;
            bool win = state == S.Won;
            R(0f, 0f, W, H, win ? new Color(0f, 0.08f, 0.02f, 0.72f) : new Color(0.12f, 0f, 0f, 0.62f));
            L(st, win ? "¡MISIÓN CUMPLIDA!" : "HAS MUERTO", 0f, H * 0.14f, W, 100f * s, 70, C, win ? YEL : RED);
            int secs = Mathf.FloorToInt(playTime);
            L(st, "Infectados eliminados: " + kills + "   ·   Tiempo: " + (secs / 60) + ":" + (secs % 60).ToString("00") + "   ·   " + Ag.nick,
                0f, H * 0.14f + 100f * s, W, 40f * s, 24, C, Color.white);
            if (deadT > 1f)
            {
                float bw = 460f * s, bh = 52f * s, bx = (W - bw) / 2f, by = H * 0.42f;
                Button(42, bx, by, bw, bh, win ? "JUGAR DE NUEVO" : "REINTENTAR", 26, true);
                Button(43, bx, by + 66f * s, bw, bh, "CAMBIAR PERSONAJE / MAPA", 22, false);
                Button(41, bx, by + 132f * s, bw, bh, "MENÚ PRINCIPAL", 22, false);
            }
        }

        // ---------- Mapa del juego (tecla M) ----------
        void DrawMap(float W, float H, float s)
        {
            TextAnchor C = TextAnchor.MiddleCenter, Lf = TextAnchor.UpperLeft;
            R(0f, 0f, W, H, new Color(0f, 0f, 0f, 0.9f));
            L(st, "MAPA DEL JUEGO  -  DEAD PROTOCOL", 0f, 14f * s, W, 50f * s, 36, C, YEL);
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
            R(px - 6f * s, py - 6f * s, 12f * s, 12f * s, YEL);
            L(st, "TÚ", px - 30f * s, py - 30f * s, 60f * s, 20f * s, 16, C, YEL);

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
                L(stw, (i + 1) + ". " + q.name, cx + 8f * s, ty, cw - 16f * s, 50f * s, 18, Lf, Color.white);
                L(stw, q.sub, cx + 8f * s, ty + 44f * s, cw - 16f * s, ch, 14, Lf, new Color(0.85f, 0.85f, 0.85f));
            }
            L(st, "M o Esc: cerrar el mapa", 0f, H - 34f * s, W, 30f * s, 18, C, Color.white);
        }
    }
}
