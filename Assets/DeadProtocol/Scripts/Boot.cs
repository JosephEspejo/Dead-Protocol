using UnityEngine;

namespace DeadProtocol
{
    // Arranca solo al darle Play, en cualquier escena (incluso vacia).
    public static class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (GameObject.Find("DeadProtocol_Escena") != null) return;
            GameObject g = new GameObject("DeadProtocol_Escena");
            g.AddComponent<SceneController>();
        }
    }
}
