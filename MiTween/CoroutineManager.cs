using System.Collections;
using UnityEngine;

namespace MiFramework.Tween
{
    public class CoroutineManager
    {
        private class CoroutineHandler : MonoBehaviour { }

        public static CoroutineManager Instance => Singleton<CoroutineManager>.Instance;

        private GameObject root;
        private CoroutineHandler handler;

        public CoroutineManager()
        {
            InitManager();
        }

        public void StartCoroutine(IEnumerator method)
        {
            handler.StartCoroutine(method);
        }

        public void StopCoroutine(IEnumerator method)
        {
            handler.StopCoroutine(method);
        }

        private void InitManager()
        {
            if (handler == null)
            {
                root = new GameObject("[CoroutineManager]");
                handler = root.SafeGetComponent<CoroutineHandler>();
                Object.DontDestroyOnLoad(root);
            }
        }
    }
}