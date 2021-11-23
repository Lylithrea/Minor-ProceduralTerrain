using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    private Text text;
    [SerializeField] private float _hudRefreshRate = 1f;

    private float _timer;
    public int samples = 100;
    public float totalTime;

    // Start is called before the first frame update
    void Start()
    {
        text = this.gameObject.GetComponent<Text>();
        _timer = samples;
        totalTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= 1;
        totalTime += Time.deltaTime;

        if (_timer <= 0)
        {
            float fps = samples / totalTime;
            text.text = "FPS: " + fps.ToString();
            totalTime = 0f;
            _timer = samples;
        }


 /*       if (Time.unscaledTime > _timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            text.text = "FPS: " + fps.ToString();
            _timer = Time.unscaledTime + _hudRefreshRate;
        }*/
        
    }
}
