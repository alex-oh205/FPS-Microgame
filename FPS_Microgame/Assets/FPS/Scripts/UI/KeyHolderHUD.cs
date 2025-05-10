using System.Collections.Generic;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class KeyHolderHUD : MonoBehaviour
    {
        [Tooltip("Container that holds the current keys in inventory")]
        public Transform container;

        private KeyHolder keyHolder;
        private Transform keyTemplate;

        private void Awake()
        {
            keyTemplate = container.Find("KeyTemplate");
            keyTemplate.gameObject.SetActive(false);

            keyHolder = FindObjectOfType<KeyHolder>();
        }

        private void Start()
        {
            if (keyHolder != null)
            {
                keyHolder.OnKeysChanged += KeyHolder_OnKeysChanged;
            }
        }

        private void KeyHolder_OnKeysChanged(object sender, System.EventArgs e)
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (keyHolder != null)
            {
                // Clean up old keys
                foreach (Transform child in container)
                {
                    if (child == keyTemplate) continue;
                    Destroy(child.gameObject);
                }

                // Instantiate current key list
                List<KeyPickup.KeyType> keyList = keyHolder.GetKeyList();
                for (int i = 0; i < keyList.Count; i++)
                {
                    KeyPickup.KeyType keyType = keyList[i];
                    Transform keyTransform = Instantiate(keyTemplate, container);
                    keyTransform.gameObject.SetActive(true);
                    Image keyImage = keyTransform.GetComponentInChildren<Image>();
                    switch (keyType)
                    {
                        default:

                        case KeyPickup.KeyType.Red:
                            keyImage.color = Color.red;
                            break;

                        case KeyPickup.KeyType.Green:
                            keyImage.color = Color.green;
                            break;

                        case KeyPickup.KeyType.Blue:
                            keyImage.color = Color.cyan;
                            break;
                    }
                }
            }
        }
    }
}