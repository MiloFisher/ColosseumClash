namespace Photon.Pun
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [AddComponentMenu("Photon Networking/Photon Event Deck View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonEventDeckView : MonoBehaviour, IPunObservable, IPointerEnterHandler, IPointerExitHandler
    {
        public string[] deck;
        public int iterator;

        private HighlightCard highlightCard;

        private PhotonView m_PhotonView;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(iterator);
            }
            else
            {
                iterator = (int)stream.ReceiveNext();
            }
        }

        [PunRPC]
        public void CreateDeck(int size)
        {
            deck = new string[size];
        }

        [PunRPC]
        public void UpdateDeck(int position, string value)
        {
            deck[position] = value;
        }

        [PunRPC]
        public void IncrementIterator()
        {
            iterator++;
        }

        public string GetEvent()
        {
            if (iterator >= deck.Length || iterator < 0)
            {
                if (iterator >= deck.Length)
                    Debug.Log("Event deck iterator is greater than or equal to deck length: Iterator[" + iterator + "], Deck length[" + deck.Length + "]");
                if (iterator < 0)
                    Debug.Log("Iterator is less than 0: Iterator[" + iterator + "]");
                return "none";
            }
            return deck[iterator];
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightCard = GameObject.FindGameObjectWithTag("Highlight").GetComponent<HighlightCard>();
            highlightCard.EventActivate(GetEvent(), true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightCard = GameObject.FindGameObjectWithTag("Highlight").GetComponent<HighlightCard>();
            highlightCard.EventActivate("none", false);
        }
    }
}