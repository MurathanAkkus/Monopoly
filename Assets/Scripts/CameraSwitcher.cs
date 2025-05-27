using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher instance;

    [SerializeField] CinemachineVirtualCamera topDownCamera;
    [SerializeField] CinemachineVirtualCamera diceCamera;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;

    CinemachineVirtualCamera currentCam;
    CinemachineVirtualCamera lastCam;

    void Awake()
    {
        instance = this;
    }

    void SwitchTo(CinemachineVirtualCamera targetCam)
    {
        if (currentCam != null)
        {
            currentCam.Priority = 0;
            lastCam = currentCam;
        }

        currentCam = targetCam;
        currentCam.Priority = 10;
    }

    public void SwitchToTopDown() => SwitchTo(topDownCamera);
    public void SwitchToDice() => SwitchTo(diceCamera);

    public void SwitchToPlayer(Transform followTarget)
    {
        playerFollowCamera.Follow = followTarget;
        playerFollowCamera.LookAt = followTarget;
        SwitchTo(playerFollowCamera);
    }

    // VIEWBUTTON İÇİN KAMERA GEÇİŞİ
    public void SwitchToTemporaryTopDown()
    {
        if (currentCam == null)
        {
            currentCam = (playerFollowCamera.Priority > diceCamera.Priority)
                ? playerFollowCamera
                : diceCamera;
        }

        lastCam = currentCam;
        topDownCamera.Priority = 20;
        currentCam = topDownCamera;
    }

    public void ReturnToLastCam()
    {
        if (lastCam != null)
        {
            lastCam.Priority = 20;
            topDownCamera.Priority = 0;
            currentCam = lastCam;
            lastCam = null;
        }
    }
}