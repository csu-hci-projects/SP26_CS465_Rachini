using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public Material defaultMat;
    public Material hoverMat;
    public TrialManager trialManager;
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip hitSound;

    private Renderer rend;
    private bool canSelect = true;
    private bool hasSelected = false;
    private bool isHovering = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = defaultMat;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;

        isHovering = true;
        SetHover(true);

        // TOUCH selection only
        if (!trialManager.usePinch)
        {
            Select();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Hand")) return;

        isHovering = false;
        SetHover(false);
    }

    public void SetHover(bool isHovering)
    {
        if (isHovering)
        {
            rend.material = hoverMat;

            if (trialManager.useAudioFeedback && audioSource && hoverSound)
                audioSource.PlayOneShot(hoverSound);
        }
        else
        {
            rend.material = defaultMat;
        }
    }

    public void ForceClearHover()
    {
        isHovering = false;
        SetHover(false);
    }

    public void Select()
    {
        if (hasSelected) return;

        hasSelected = true;

    if (trialManager.useAudioFeedback && audioSource && hitSound)
        audioSource.PlayOneShot(hitSound);

        trialManager.RegisterHit();
    }   

    public void ResetSelection()
    {
        hasSelected = false;
        isHovering = false;

        SetHover(false);
    }   

    void Update()
    {
        // Only for pinch trials
        if (!trialManager.usePinch) return;

    // Allow selection if hovering AND currently pinching
    if (isHovering && XRITKGestureDetector.isPinching)
        {
            Select();
        }
    }
}