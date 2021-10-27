using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private Text text;
    [SerializeField] private float _hudRefreshRate = 1f;

    private float _timer;
    // Start is called before the first frame update
    void Start()
    {
        text = this.gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.unscaledTime > _timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            text.text = "FPS: " + fps.ToString();
            _timer = Time.unscaledTime + _hudRefreshRate;
        }
        
    }
}
