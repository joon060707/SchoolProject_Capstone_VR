using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update
    public int seconds;         // ������ �ð�
    public int currentSeconds;  // ���� �귯���� �ð�
    public TMPro.TMP_Text time;

    // controller
    public bool isRunning = false;
    //public bool paused = false;
    public bool shouldStop = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time.text = formatting(currentSeconds);
        /*
        if (paused)
            time.color = Color.gray;
        else
            time.color = Color.white;
        */

       
    }

    private void OnEnable()
    {
        if (isRunning && !shouldStop) StartCoroutine(runTimer(this));   // ����� ���...
    }

    public static string formatting(int sec)
    {
        int min = sec / 60; // over 60min
        int seconds = sec % 60;
        return $"{min:D2}:{seconds:D2}";
    }

    public static IEnumerator runTimer(Timer timer)
    {
        timer.currentSeconds = timer.seconds;
        timer.isRunning = true;

        while(timer.currentSeconds > 0)
        {

            // Ÿ�̸� �Ͻ����� (����)
           // yield return new WaitWhile(() => timer.paused);

            // Ÿ�̸� ����
            if (timer.shouldStop)
            {
                timer.currentSeconds = 0;
                timer.shouldStop = false;
                break;
            }

            timer.currentSeconds -= 1;
            yield return new WaitForSeconds(1);
        }

        timer.isRunning = false;
    }
}
