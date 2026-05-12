using UnityEngine;
using TMPro;
using System.IO;

public class TrialManager : MonoBehaviour
{
    public Transform targetA;
    public Transform targetB;

    private Transform currentTarget;
    private Transform nextTarget;

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
    public bool useSnapAssistance;

    [Header("Snap Assistance")]
    public float snapRadius = 0.50f;

    int hitsThisTrial = 0;
    int hitsPerTrial = 5;

    void Start()
    {
        File.WriteAllText(path, "Trial,Hit,A,W,Direction,Method,Snap,MT,ID,Result\n");
        SetupTargets();
        NextTrial();
    }

    void SetupTargets()
    {
        currentTarget = targetA;
        nextTarget = targetB;
        // Enable both colliders
        targetA.gameObject.SetActive(true);
        targetB.gameObject.SetActive(true);
    }

    public void NextTrial()
    {
        if (currentTrial >= totalTrials)
        {
            trialText.text = "Experiment Complete!";
            return;
        }

        hitsThisTrial = 0;

        // Determine condition
        int dIndex = currentTrial % distances.Length;
        int sIndex = (currentTrial / distances.Length) % sizes.Length;
        int dirIndex = (currentTrial / (distances.Length * sizes.Length)) % directions.Length;

        A = distances[dIndex];
        W = sizes[sIndex];
        dir = directions[dirIndex];

        float angle = dir * 20f;

        Transform cam = Camera.main.transform;

        // Base forward direction
        Vector3 forward = cam.forward;

        // Left/right offset direction
        Vector3 right = cam.right;

        // Distance from user
        Vector3 center = cam.position + forward * A;

        // Spread targets left/right (half amplitude each side)
        float spread = A / 2f;

        currentTarget.position = center + right * spread;
        nextTarget.position = center - right * spread;

        //Apply size to both targets
        currentTarget.localScale = new Vector3(W, W, W);
        nextTarget.localScale = new Vector3(W, W, W);

        Debug.Log($"Trial {currentTrial + 1}: A={A}, W={W}, Dir={dir}");

        startTime = Time.time;

        trialText.text = "Trial " + (currentTrial + 1);

        int condition = currentTrial % 4;

        switch (condition)
        {
            case 0:
                usePinch = false;
                useSnapAssistance = false;
                break;

            case 1:
                usePinch = false;
                useSnapAssistance = true;
                break;

            case 2:
                usePinch = true;
                useSnapAssistance = false;
                break;

            case 3:
                usePinch = true;
                useSnapAssistance = true;
                break;
        }

        useAudioFeedback = true;

        methodText.text = "Method: " + (usePinch ? "Pinch" : "Touch") + (useSnapAssistance ? " + Assistance" : "");
        resultText.text = "";

        // Reset both targets
        ResetTarget(currentTarget);
        ResetTarget(nextTarget);
        
        currentTarget.GetComponent<TargetObject>().SetActiveState(true);   // active
        nextTarget.GetComponent<TargetObject>().SetActiveState(false);     // inactive
                
        Renderer currentRend = currentTarget.GetComponent<Renderer>();
        Renderer nextRend = nextTarget.GetComponent<Renderer>();

        trialEnded = false;
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    void ResetTarget(Transform t)
    {
        TargetObject obj = t.GetComponent<TargetObject>();
        obj.enabled = true;
        obj.ResetSelection();
        obj.ForceClearHover();
    }

    public void RegisterHit(Transform hitTarget)
    {
        // Prevent double hits
        if (trialEnded) return;

        if (hitTarget != currentTarget)
        {
            File.AppendAllText(path,
                $"{currentTrial},{hitsThisTrial},{A},{W},{dir},{(usePinch ? "Pinch" : "Touch")},0,0,ERROR\n");
            return;
        }   

        trialEnded = true;

        float MT = Time.time - startTime;
        Debug.Log("MT: " + MT);

        resultText.text = "HIT";
        resultText.color = Color.green;

        float ID = Mathf.Log((A / W) + 1, 2);
        
        hitsThisTrial++;

        File.AppendAllText(path,
            //$"{currentTrial},{hitsThisTrial},{A},{W},{dir},{(usePinch ? "Pinch" : "Touch")},{MT},{ID},HIT\n");
            $"{currentTrial},{hitsThisTrial},{A},{W},{dir}," +
            $"{(usePinch ? "Pinch" : "Touch")}," +
            $"{(useSnapAssistance ? "Snap" : "Normal")}," +
            $"{MT},{ID},HIT\n");
      
        // Switch targets
        Transform temp = currentTarget;
        currentTarget = nextTarget;
        nextTarget = temp;

        // Reset both targets
        currentTarget.GetComponent<TargetObject>().ResetSelection();
        nextTarget.GetComponent<TargetObject>().ResetSelection();

        // Update visuals
        currentTarget.GetComponent<TargetObject>().SetActiveState(true);   // active
        nextTarget.GetComponent<TargetObject>().SetActiveState(false);     // inactive 

        // Check if trial is done
        if (hitsThisTrial >= hitsPerTrial)
        {
            currentTrial++;
            Invoke(nameof(NextTrial), 0.5f);
        }
        else
        {
            // Continue same trial
            trialEnded = false;
            startTime = Time.time;
        }       

        Renderer currentRend = currentTarget.GetComponent<Renderer>();
        Renderer nextRend = nextTarget.GetComponent<Renderer>();
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
                //$"{currentTrial},{hitsThisTrial},{A},{W},{dir},{(usePinch ? "Pinch" : "Touch")},{MT},{ID},MISS\n");
                $"{currentTrial},{hitsThisTrial},{A},{W},{dir}," +
                $"{(usePinch ? "Pinch" : "Touch")}," +
                $"{(useSnapAssistance ? "Snap" : "Normal")}," +
                $"{MT},{ID},MISS\n");

            currentTrial++;
            Invoke(nameof(NextTrial), 0.5f);
        }
    }
}
