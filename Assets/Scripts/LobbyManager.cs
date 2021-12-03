using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;

public class LobbyManager : MonoBehaviour
{
    public GameObject[] players;
    public GameObject startGameButton;
    public GameObject readyButton;
    public GameObject unreadyButton;

    private ChampionList championListScript = new ChampionList();
    private List<Champion> championList = new List<Champion>();
    private GameObject myPlayer;
    private bool allReady;

    private void OnEnable()
    {
        championList = championListScript.Champions();
        readyButton.SetActive(true);
        unreadyButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        allReady = true;
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.SetParent(transform);
            players[i].transform.position = GetPosition(players[i].GetComponent<PhotonMenuView>().id, players.Length);
            players[i].GetComponent<Image>().sprite = championList[players[i].GetComponent<PhotonMenuView>().champion].image;
            players[i].GetComponent<PhotonMenuView>().healthText.text = championList[players[i].GetComponent<PhotonMenuView>().champion].health + "";
            players[i].GetComponent<PhotonMenuView>().attackText.text = championList[players[i].GetComponent<PhotonMenuView>().champion].attack;
            players[i].GetComponent<PhotonMenuView>().defenseText.text = championList[players[i].GetComponent<PhotonMenuView>().champion].defense + "";
            if (!players[i].GetComponent<PhotonMenuView>().inputField.interactable)
                players[i].GetComponent<PhotonMenuView>().inputField.text = players[i].GetComponent<PhotonMenuView>().playerName;
            else
                myPlayer = players[i];

            players[i].transform.GetChild(3).gameObject.SetActive(players[i].GetComponent<PhotonMenuView>().ready);
            players[i].transform.GetChild(8).position = new Vector3(0, players[i].transform.GetChild(8).position.y, 0);
            if (!players[i].GetComponent<PhotonMenuView>().ready)
                allReady = false;
        }
        startGameButton.SetActive(players.Length > 1 && allReady && PhotonNetwork.IsMasterClient);
    }

    private Vector3 GetPosition(int id, int players)
    {
        switch (players)
        {
            case 1: return Vector3.zero;
            case 2:
                if(id == 1)
                    return new Vector3(-55, 0, 0);
                else
                    return new Vector3(55, 0, 0);
            case 3:
                if (id == 1)
                    return new Vector3(-110, 0, 0);
                else if (id == 2)
                    return Vector3.zero;
                else
                    return new Vector3(110, 0, 0);
            case 4:
                if (id == 1)
                    return new Vector3(-165, 0, 0);
                else if (id == 2)
                    return new Vector3(-55, 0, 0);
                else if (id == 3)
                    return new Vector3(55, 0, 0);
                else
                    return new Vector3(165, 0, 0);
            case 5:
                if (id == 1)
                    return new Vector3(-220, 0, 0);
                else if (id == 2)
                    return new Vector3(-110, 0, 0);
                else if (id == 3)
                    return Vector3.zero;
                else if (id == 4)
                    return new Vector3(110, 0, 0);
                else
                    return new Vector3(220, 0, 0);
        }
        return Vector3.zero;
    }

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;

        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<PhotonView>().RPC("CreateChampion", players[i].GetComponent<PhotonView>().Owner, players.Length);
        }
    }

    public void Ready()
    {
        if (myPlayer)
        {
            myPlayer.GetComponent<PhotonView>().RPC("ReadyUp", RpcTarget.All, true);
            readyButton.SetActive(false);
            unreadyButton.SetActive(true);
        }
    }

    public void Unready()
    {
        if (myPlayer)
        {
            myPlayer.GetComponent<PhotonView>().RPC("ReadyUp", RpcTarget.All, false);
            readyButton.SetActive(true);
            unreadyButton.SetActive(false);
        }
    }
}
