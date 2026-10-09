using UnityEngine;

namespace DeadProtocol
{
    // Arranca solo al darle Play, en cualquier escena (incluso vacia).
    public static class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (GameObject.Find("DeadProtocol_Juego") != null) return;
            GameObject g = new GameObject("DeadProtocol_Juego");
            g.AddComponent<GameController>();
        }
    }
}
