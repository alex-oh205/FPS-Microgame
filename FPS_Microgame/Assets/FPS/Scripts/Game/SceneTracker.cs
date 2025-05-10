using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Game
{
    public class SceneTracker : MonoBehaviour
    {
        private string m_PreviousScene = "";

        void Awake()
        {
            EventManager.AddListener<LoadSceneEvent>(OnLoadScene);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnLoadScene(LoadSceneEvent evt)
        {
            if (evt.LoadPreviousScene)
            {
                LoadPreviousScene();
            }
            else
            {
                LoadScene(evt.NewScene);
            }
        }

        public void LoadScene(string newScene)
        {
            m_PreviousScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(newScene);
        }

        public void LoadPreviousScene()
        {
            if (!string.IsNullOrEmpty(m_PreviousScene))
            {
                LoadScene(m_PreviousScene);
            }
        }
    }
}