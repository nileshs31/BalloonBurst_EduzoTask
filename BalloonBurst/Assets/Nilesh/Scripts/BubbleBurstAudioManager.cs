using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Eduzo.Games.GameName
{
   
    public class BubbleBurstAudioManager : MonoBehaviour
    {
        public static BubbleBurstAudioManager Instance;
        // Start is called before the first frame update
        void Start()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
