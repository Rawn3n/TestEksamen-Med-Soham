using UnityEngine;
using PurrNet;

public class TestNetwork : NetworkIdentity
{
    [SerializeField] private NetworkIdentity _networkIdentity;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (!isServer)
        {
            return;
        }


        Instantiate(_networkIdentity, Vector3.zero, Quaternion.identity);
    }



}
