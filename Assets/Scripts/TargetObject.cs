using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public Material hoverMat;
    public Material activeMat;
    public Material inactiveMat;
    public TrialManager trialManager;
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip hitSound;

    private Renderer rend;
    private bool hasSelected = false;
    private bool isHovering = false;
    private Transform handTransform;
    private Transform handRootTransform;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // Default to inactive at startup
        rend.material = inactiveMat;

        GameObject fingerObj = GameObject.FindGameObjectWithTag("Hand");

        if (fingerObj != null)
        {
            handTransform = fingerObj.transform;
        }

        GameObject rootObj =
            GameObject.FindGameObjectWithTag("HandRoot");

        if (rootObj != null)
        {
            handRootTransform = rootObj.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;

        // Only allow active target interaction
        if (transform != trialManager.GetCurrentTarget()) return;

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

    public void SetHover(bool hovering)
    {
        // Prevent repeated calls
        if (isHovering == hovering) return;

        isHovering = hovering;

        if (hovering)
        {
            rend.material = hoverMat;

            if (trialManager.useAudioFeedback &&
                audioSource &&
                hoverSound)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }
        else
        {
            bool isActive =
                (transform == trialManager.GetCurrentTarget());

            rend.material =
                isActive ? activeMat : inactiveMat;
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

        trialManager.RegisterHit(transform);
    }   

    public void ResetSelection()
    {
        hasSelected = false;
        isHovering = false;

        SetHover(false);
    }
    
    public void SetActiveState(bool isActive)
    {
        if (isActive)
            rend.material = activeMat;
        else
            rend.material = inactiveMat;
    }   

    void Update()
    {
        // Snap Assistance
        if (trialManager.useSnapAssistance &&
            transform == trialManager.GetCurrentTarget() &&
            handTransform != null)
        {
            float dist = Vector3.Distance(handTransform.position, transform.position);

            // Pull hand toward target
            if (dist < trialManager.snapRadius)
            {
                if (trialManager.usePinch) {
                    Vector3 snapOffset = (transform.position - handTransform.position);
                    handRootTransform.position += snapOffset * Time.deltaTime * 4f;
                } else {
                    Vector3 snapOffset = (transform.position - handTransform.position).normalized;
                    handRootTransform.position += snapOffset * Time.deltaTime * 4f;
                }
                /*Vector3.Lerp(
                    handRootTransform.position,
                    handRootTransform.position + snapOffset,
                    Time.deltaTime * 8f);*/

                // Force hover state once close enough
                if (!isHovering && dist < trialManager.snapRadius)
                {
                    isHovering = true;
                    SetHover(true);

                    // Auto-touch selection
                    if (!trialManager.usePinch)
                    {
                        Select();
                    }
                }
            }
        }

        // Only for pinch trials
        if (!trialManager.usePinch) return;

        // Only allow active target interaction
        if (transform != trialManager.GetCurrentTarget()) return;
        
        // Allow selection if hovering AND currently pinching
        if (isHovering && XRITKGestureDetector.isPinching) Select();
    }
}