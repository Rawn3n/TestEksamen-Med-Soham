using System.Globalization;
using PurrNet;
using UnityEngine;

public class PickandShooting : NetworkBehaviour
{


    //throw settings
    public float throwForce;
    public float throwCooldown;

    //references
    public Transform holdPoint;
    public Camera playerCamera;
    public LineRenderer trajectoryLine;

    private float lastThrowTime;

    void Update()
    {

    }
}
