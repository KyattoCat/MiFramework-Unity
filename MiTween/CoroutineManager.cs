using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiFramework.Tween
{
    public class CoroutineManager
    {
        private class CoroutineHandler : MonoBehaviour { }

        public static CoroutineManager Instance => Singleton<CoroutineManager>.Instance;

        public Dictionary<int, Coroutine> allCoroutine = new Dictionary<int, Coroutine>();

        private int UniqueID = 0;

        private GameObject root;
        private CoroutineHandler handler;

        public CoroutineManager()
        {
            InitManager();
        }

        public int StartCoroutine(IEnumerator method)
        {
            allCoroutine[UniqueID] = handler.StartCoroutine(method);
            return UniqueID;
        }

        public void StopCoroutine(int uniqueID)
        {
            if (uniqueID == 0)
                return;

            if (allCoroutine.TryGetValue(uniqueID, out var coroutine))
            {
                handler.StopCoroutine(coroutine);
                allCoroutine.Remove(uniqueID);
            }
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