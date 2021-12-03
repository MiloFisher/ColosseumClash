namespace Photon.Pun
{
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Photon Networking/Photon Game View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonGameView : MonoBehaviour, IPunObservable
    {
        public string playerName;
        public int champion;
        public string championName;
        public int turnPhase;
        public int favor;
        //For combat
        public int defenderId;
        public bool defenderIsPlayer;
        public int roll;
        public bool retreated;
        public bool defended;

        public GameObject defenseBorder;
        
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
                stream.SendNext(champion);
                stream.SendNext(championName);
                stream.SendNext(turnPhase);
                stream.SendNext(favor);
                stream.SendNext(defenderId);
                stream.SendNext(defenderIsPlayer);
                stream.SendNext(roll);
                stream.SendNext(retreated);
                stream.SendNext(defended);
            }
            else
            {
                playerName = (string)stream.ReceiveNext();
                champion = (int)stream.ReceiveNext();
                championName = (string)stream.ReceiveNext();
                turnPhase = (int)stream.ReceiveNext();
                favor = (int)stream.ReceiveNext();
                defenderId = (int)stream.ReceiveNext();
                defenderIsPlayer = (bool)stream.ReceiveNext();
                roll = (int)stream.ReceiveNext();
                retreated = (bool)stream.ReceiveNext();
                defended = (bool)stream.ReceiveNext();
            }
        }

        [PunRPC]
        public void SetTurnPhase(int n)
        {
            turnPhase = n;
        }

        [PunRPC]
        public void WatchCombat(int n, int attackerId)
        {
            SetTurnPhase(n);
            PlayManager playManager = GameObject.FindGameObjectWithTag("Game Arena").GetComponent<PlayManager>();
            playManager.attackerId = attackerId;
        }

        [PunRPC]
        public void DrawDamage(int amount, bool attackSuccess)
        {
            PlayManager playManager = GameObject.FindGameObjectWithTag("Game Arena").GetComponent<PlayManager>();
            playManager.DrawDamage(amount, attackSuccess);
        }

        [PunRPC]
        public void DrawWinner(int id)
        {
            PlayManager playManager = GameObject.FindGameObjectWithTag("Game Arena").GetComponent<PlayManager>();
            playManager.GameOver(id);
        }

        [PunRPC]
        public void GainFavor(int amount)
        {
            favor += amount;
        }

        [PunRPC]
        public void LoseFavor(int amount)
        {
            favor -= amount;
            if (favor < 0)
                favor = 0;
        }

        [PunRPC]
        public void Defended(bool active)
        {
            defended = active;
        }
    }
}