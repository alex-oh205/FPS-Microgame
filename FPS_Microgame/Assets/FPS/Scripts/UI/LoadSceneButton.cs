using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        [Tooltip("Scene to load when clicking this button. Specifying \"-1\" implies the previous loaded scene.")]
        public string SceneName = "";

        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject
                && Input.GetButtonDown(GameConstants.k_ButtonNameSubmit))
            {
                LoadTargetScene();
            }
        }

        public void LoadTargetScene()
        {
            LoadSceneEvent evt = Events.LoadSceneEvent;
            evt.NewScene = SceneName;
            evt.LoadPreviousScene = SceneName == "-1";
            EventManager.Broadcast(evt);
        }
    }
}