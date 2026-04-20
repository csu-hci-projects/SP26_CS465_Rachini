using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class XRITKGestureDetector : MonoBehaviour
{
    private XRHandSubsystem handSubsystem;

    public static bool isPinching = false;
    public static bool pinchThisFrame = false;
    public static Vector3 pinchPosition;

    void Start()
    {
        handSubsystem = XRGeneralSettings.Instance
            .Manager
            .activeLoader
            .GetLoadedSubsystem<XRHandSubsystem>();
    }

    void Update()
    {
        if (handSubsystem == null)
        {
            isPinching = false;
            pinchThisFrame = false;
            return;
        }

        bool wasPinching = isPinching;

        isPinching = false;

        // Check both hands
        CheckHand(handSubsystem.rightHand);
        CheckHand(handSubsystem.leftHand);

        // Detect only the moment pinch starts
        pinchThisFrame = (!wasPinching && isPinching);

        if (pinchThisFrame)
        {
            Debug.Log("PINCH STARTED"); // should appear once per pinch
        }
    }

    bool CheckHand(XRHand hand)
    {
        if (!hand.isTracked) return false;

    if (IsPinch(hand))
    {
        isPinching = true;

        // Save pinch position (midpoint between thumb + index)
        if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbPose) &&
            hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexPose))
        {
            pinchPosition = (thumbPose.position + indexPose.position) / 2f;
        }

        return true;
    }

        return false;
    }

    bool IsPinch(XRHand hand)
    {
        if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbPose) &&
            hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexPose))
        {
            float distance = Vector3.Distance(thumbPose.position, indexPose.position);
            return distance < 0.05f; // FIXED threshold
        }
        return false;
    }
}