namespace Photon.Pun
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Photon Networking/Photon Menu View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonMenuView : MonoBehaviour, IPunObservable
    {
        private ChampionList championList = new ChampionList();

        public InputField inputField;
        public GameObject rightArrow;
        public GameObject leftArrow;
        public GameObject inputBlocker;
        public GameObject keyboardToggle;

        public string playerName;
        public int id;
        public int champion;
        public bool ready;

        public Text healthText;
        public Text attackText;
        public Text defenseText;

        private PhotonView m_PhotonView;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(playerName);
                stream.SendNext(id);
                stream.SendNext(champion);
                stream.SendNext(ready);
            }
            else
            {
                playerName = (string)stream.ReceiveNext();
                id = (int)stream.ReceiveNext();
                champion = (int)stream.ReceiveNext();
                ready = (bool)stream.ReceiveNext();
            }
        }

        public void UpdateName()
        {
            playerName = inputField.text;
        }

        public void CycleChampion(int direction)
        {
            if (direction == 0)//right
            {
                champion++;
                if (champion >= championList.TOTAL_CHAMPIONS)
                    champion = 0;
            }
            else //left
            {
                champion--;
                if (champion < 0)
                    champion = championList.TOTAL_CHAMPIONS - 1;
            }
        }

        [PunRPC]
        public void ReadyUp(bool active)
        {
            ready = active;
        }

        [PunRPC]
        public void CreateChampion(int totalPlayers)
        {
            ready = false;
            NetworkManager networkManager  = GameObject.FindGameObjectWithTag("Network Manager").GetComponent<NetworkManager>();
            networkManager.ActivateMenu(4);
            PlayManager playManager = GameObject.FindGameObjectWithTag("Game Arena").GetComponent<PlayManager>();
            playManager.CreateChampion(playerName, id, champion, totalPlayers);
        }
    }
}