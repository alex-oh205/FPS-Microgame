using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class KeyHolder : MonoBehaviour
    {
        public event EventHandler OnKeysChanged;

        private List<KeyPickup.KeyType> keyList;

        private void Awake()
        {
            keyList = new List<KeyPickup.KeyType>();
        }

        public List<KeyPickup.KeyType> GetKeyList()
        {
            return keyList;
        }

        public void AddKey(KeyPickup.KeyType keyType)
        {
            // Debug.Log("Added key: " + keyType);
            keyList.Add(keyType);
            OnKeysChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveKey(KeyPickup.KeyType keyType)
        {
            keyList.Remove(keyType);
            OnKeysChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool ContainsKey(KeyPickup.KeyType keyType)
        {
            return keyList.Contains(keyType);
        }
    }
}