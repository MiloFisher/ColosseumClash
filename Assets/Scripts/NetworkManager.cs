using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject[] menuScreens;
    public InputField matchNameHost;
    public InputField matchNameJoin;
    public GameObject rules;
    private RoomOptions roomOptions;

    void Awake()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "usw";
        PhotonNetwork.ConnectUsingSettings();
        ActivateMenu(0);
    }

    private void Update()
    {
        if (menuScreens[1].activeInHierarchy && Input.GetKeyDown(KeyCode.Return) && matchNameHost.text != "")
            Host();
        if (menuScreens[3].activeInHierarchy && Input.GetKeyDown(KeyCode.Return) && matchNameJoin.text != "")
            Join();
        if (!menuScreens[2].activeInHierarchy && Input.GetKeyDown(KeyCode.R))
            Rules(!rules.activeInHierarchy);
        if (Input.GetKeyDown(KeyCode.Escape))
            Rules(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to the " + PhotonNetwork.CloudRegion + " server, version: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Host()
    {
        roomOptions = new RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 5;
        roomOptions.PlayerTtl = 60000; //wait 60 sec
        roomOptions.EmptyRoomTtl = 1000; //wait 1 sec
        roomOptions.CleanupCacheOnLeave = false;
        PhotonNetwork.CreateRoom(matchNameHost.text, roomOptions);
    }

    public void Join()
    {
        PhotonNetwork.JoinRoom(matchNameJoin.text);
    }

    public void Leave()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
    }

    public void OnDisconnectedFromMasterServer()
    {

    }

    public override void OnJoinedRoom()
    {
        ActivateMenu(2);
        var player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player (Menu)"), Vector3.zero, Quaternion.identity);
        player.GetComponent<PhotonMenuView>().id = PhotonNetwork.CurrentRoom.PlayerCount;
        player.GetComponent<PhotonMenuView>().playerName = "Player " + PhotonNetwork.CurrentRoom.PlayerCount;
        player.GetComponent<PhotonMenuView>().inputField.text = player.GetComponent<PhotonMenuView>().playerName;
        player.GetComponent<PhotonMenuView>().inputField.interactable = true;
        player.GetComponent<PhotonMenuView>().rightArrow.SetActive(true);
        player.GetComponent<PhotonMenuView>().leftArrow.SetActive(true);
        player.GetComponent<PhotonMenuView>().inputBlocker.SetActive(true);
        player.GetComponent<PhotonMenuView>().keyboardToggle.SetActive(true);
    }

    public override void OnLeftRoom()
    {

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Could not join game");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room");
        Debug.LogError("Error " + returnCode + ": " + message);
        PhotonNetwork.CreateRoom(matchNameHost.text, roomOptions);
    }

    public void ActivateMenu(int id)
    {
        for (int i = 0; i < menuScreens.Length; i++)
            menuScreens[i].SetActive(false);
        menuScreens[id].SetActive(true);
    }

    public void Quit()
    {
        Application.Quit(1);
    }

    public void Rules(bool active)
    {
        rules.SetActive(active);
    }
}
