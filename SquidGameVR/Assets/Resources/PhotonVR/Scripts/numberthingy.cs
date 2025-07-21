using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class numberthingy : MonoBehaviourPunCallbacks, IPunObservable
{
    public List<TextMeshPro> numberDisplays = new List<TextMeshPro>();
    private static HashSet<int> usedNumbers = new HashSet<int>();
    private int myNumber;

    void Start()
    {
        if (photonView.IsMine)
        {
            GenerateUniqueNumber();
            photonView.RPC("SetNumber", RpcTarget.AllBuffered, myNumber);
        }
    }

    void GenerateUniqueNumber()
    {
        int tries = 0;
        do
        {
            myNumber = Random.Range(1, 457);
            tries++;
            if (tries > 1000) break;
        } while (usedNumbers.Contains(myNumber));
        usedNumbers.Add(myNumber);
    }

    [PunRPC]
    void SetNumber(int num)
    {
        myNumber = num;
        string displayText = myNumber.ToString("D3");
        foreach (var display in numberDisplays)
        {
            if (display != null)
                display.text = displayText;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(myNumber);
        else
        {
            myNumber = (int)stream.ReceiveNext();
            string displayText = myNumber.ToString("D3");
            foreach (var display in numberDisplays)
                if (display != null) display.text = displayText;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (photonView.IsMine && usedNumbers.Contains(myNumber))
            usedNumbers.Remove(myNumber);
    }
}
