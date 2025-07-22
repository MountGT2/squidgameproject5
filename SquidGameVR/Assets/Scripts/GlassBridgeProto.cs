using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GlassBridgeProto : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class Pair
    {
        public GameObject part1;
        public GameObject part2;
    }

    public List<Pair> pairs = new List<Pair>();

    private int[] disabledIndices;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
                ChooseDisabledColliders();
            else
                RequestStateFromMaster();
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            ChooseDisabledColliders();
        else
            RequestStateFromMaster();
    }

    void ChooseDisabledColliders()
    {
        disabledIndices = new int[pairs.Count];
        for (int i = 0; i < pairs.Count; i++)
            disabledIndices[i] = Random.Range(0, 2);

        ApplyDisabledColliders();
        photonView.RPC(nameof(RPC_ReceiveDisabledIndices), RpcTarget.OthersBuffered, disabledIndices);
    }

    void ApplyDisabledColliders()
    {
        for (int i = 0; i < pairs.Count; i++)
        {
            if (pairs[i].part1 != null)
                SetColliderEnabled(pairs[i].part1, disabledIndices[i] != 0);
            if (pairs[i].part2 != null)
                SetColliderEnabled(pairs[i].part2, disabledIndices[i] != 1);
        }
    }

    void SetColliderEnabled(GameObject obj, bool enabled)
    {
        var cols = obj.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
            col.enabled = enabled;
    }

    void RequestStateFromMaster()
    {
        photonView.RPC(nameof(RPC_RequestState), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    void RPC_RequestState(Player requester)
    {
        if (disabledIndices == null)
            ChooseDisabledColliders();
        photonView.RPC(nameof(RPC_ReceiveDisabledIndices), requester, disabledIndices);
    }

    [PunRPC]
    void RPC_ReceiveDisabledIndices(int[] indices)
    {
        disabledIndices = indices;
        ApplyDisabledColliders();
    }
}
