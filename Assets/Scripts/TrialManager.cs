using UnityEngine;
using TMPro;
using System.IO;

public class TrialManager : MonoBehaviour
{
    public Transform targetSphere;
    public TextMeshProUGUI trialText;
    public TextMeshProUGUI methodText;
    public TextMeshProUGUI resultText;
    public float trialTimeLimit = 3f;
    public AudioSource audioSource;
    public AudioClip missSound;    

    // Experiment parameters
    float[] distances = { 0.3f, 0.6f };
    float[] sizes = { 0.1f, 0.18f };
    int[] directions = { -1, 1 };
    float A;
    float W;
    int dir;
    bool trialEnded = false;

    int currentTrial = 0;
    int totalTrials = 32;
    float startTime;
    string path = Application.dataPath + "/YourName_Outputfile.csv";
    public bool usePinch;

    public bool useAudioFeedback;

    void Start()
    {
        NextTrial();
        File.WriteAllText(path, "Trial,A,W,Direction,Method,Feedback,MT,ID,Result\n");
    }

    public void NextTrial()
    {
        if (currentTrial >= totalTrials)
        {
            trialText.text = "Experiment Complete!";
            return;
        }

        // Determine condition
        int dIndex = currentTrial % distances.Length;
        int sIndex = (currentTrial / distances.Length) % sizes.Length;
        int dirIndex = (currentTrial / (distances.Length * sizes.Length)) % directions.Length;

        A = distances[dIndex];
        W = sizes[sIndex];
        dir = directions[dirIndex];

        // Apply size
        targetSphere.localScale = new Vector3(W, W, W);

        Transform cam = Camera.main.transform;

        float angle = dir * 20f;

        Vector3 direction = Quaternion.AngleAxis(angle, cam.up) * cam.forward;

        direction = Quaternion.AngleAxis(Random.Range(-5f, 5f), cam.right) * direction;

        Vector3 offset = direction.normalized * A;

        targetSphere.position = cam.position + offset;

        Debug.Log($"Trial {currentTrial + 1}: A={A}, W={W}, Dir={dir}");

        startTime = Time.time;

        trialText.text = "Trial " + (currentTrial + 1);

        int conditionBlock = currentTrial / (totalTrials / 4);
        // 4 blocks:
        // 0 = Touch + Visual
        // 1 = Touch + Audio
        // 2 = Pinch + Visual
        // 3 = Pinch + Audio
        usePinch = (conditionBlock >= 2);
        useAudioFeedback = (conditionBlock % 2 == 1);

        methodText.text = (usePinch ? "Pinch" : "Touch") + " | " + (useAudioFeedback ? "Audio+Visual" : "Visual");
        resultText.text = "";

        TargetObject target = targetSphere.GetComponent<TargetObject>();
        target.enabled = true;
        target.ResetSelection();
        target.ForceClearHover();

        trialEnded = false;
    }

    public void RegisterHit()
    {
        if (trialEnded) return;   // Prevent double hits

        trialEnded = true;

        float MT = Time.time - startTime;
        Debug.Log("MT: " + MT);

        resultText.text = "HIT";
        resultText.color = Color.green;
        Invoke(nameof(NextTrial), 0.5f);

        float ID = Mathf.Log((A / W) + 1, 2);

        File.AppendAllText(path,
            $"{currentTrial},{A},{W},{dir},{(usePinch ? "Pinch" : "Touch")},{(useAudioFeedback ? "Audio" : "Visual")},{MT},{ID},HIT\n");

        currentTrial++;
    }

    void Update()
    {
        if (trialEnded) return;

        if (Time.time - startTime > trialTimeLimit)
        {
            trialEnded = true;

            Debug.Log("MISS");

            float MT = Time.time - startTime;

            if (resultText != null)
            {
                resultText.text = "MISS";
                resultText.color = Color.red;
            }

            if (useAudioFeedback && audioSource && missSound)
            {
                audioSource.PlayOneShot(missSound);
            }

            float ID = Mathf.Log((A / W) + 1, 2);

            File.AppendAllText(path,
                $"{currentTrial},{A},{W},{dir},{(usePinch ? "Pinch" : "Touch")},{MT},{ID},MISS\n");

            currentTrial++;
            Invoke(nameof(NextTrial), 0.5f);
        }
    }
}
