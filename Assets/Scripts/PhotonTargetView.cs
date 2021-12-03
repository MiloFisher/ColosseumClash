namespace Photon.Pun
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [AddComponentMenu("Photon Networking/Photon Target View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    [RequireComponent(typeof(PhotonView))]
    public class PhotonTargetView : MonoBehaviour, IPunObservable, IPointerEnterHandler, IPointerExitHandler
    {
        private HighlightCard highlightCard;
        private TurnOptions turnOptions;
        private PhotonGameView gameView;

        public int id;
        public int uid;
        public int health;
        public int startingHealth;
        public string attack;
        public int defense;
        public string effect;
        public int stars;
        public bool dead;
        public bool revived;
        public bool snared;
        public Text healthText;
        public Text attackText;
        public Text defenseText;

        private PhotonView m_PhotonView;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
            gameView = GetComponent<PhotonGameView>();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(id);
                stream.SendNext(uid);
                stream.SendNext(health);
                stream.SendNext(startingHealth);
                stream.SendNext(attack);
                stream.SendNext(defense);
                stream.SendNext(effect);
                stream.SendNext(stars);
                stream.SendNext(dead);
                stream.SendNext(revived);
                stream.SendNext(snared);
            }
            else
            {
                id = (int)stream.ReceiveNext();
                uid = (int)stream.ReceiveNext();
                health = (int)stream.ReceiveNext();
                startingHealth = (int)stream.ReceiveNext();
                attack = (string)stream.ReceiveNext();
                defense = (int)stream.ReceiveNext();
                effect = (string)stream.ReceiveNext();
                stars = (int)stream.ReceiveNext();
                dead = (bool)stream.ReceiveNext();
                revived = (bool)stream.ReceiveNext();
                snared = (bool)stream.ReceiveNext();
            }
        }

        public void Select()
        {
            if (!GameObject.FindGameObjectWithTag("Turn Options"))
                return;
            turnOptions = GameObject.FindGameObjectWithTag("Turn Options").GetComponent<TurnOptions>();
            if (turnOptions.canAttack)
            {
                if (gameView)
                    turnOptions.SelectTarget(GetComponent<Image>().sprite, gameView.playerName, gameView.favor, health, attack, defense, gameObject);
                else
                    turnOptions.SelectTarget(GetComponent<Image>().sprite, "", 0, health, attack, defense, gameObject);
            }
        }
    

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightCard = GameObject.FindGameObjectWithTag("Highlight").GetComponent<HighlightCard>();
            if(gameView)
                highlightCard.PlayerActivate(GetComponent<Image>().sprite, gameView.playerName, gameView.favor, health, attack, defense, gameObject, true);
            else
                highlightCard.CombatantActivate(GetComponent<Image>().sprite, health, attack, defense, gameObject, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightCard = GameObject.FindGameObjectWithTag("Highlight").GetComponent<HighlightCard>();
            if (gameView)
                highlightCard.PlayerActivate(null, null, 0, 0, null, 0, null, false);
            else
                highlightCard.CombatantActivate(null, 0, null, 0, null, false);
        }

        [PunRPC]
        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                //Gallus Effect 1/1
                if (effect == "Grit" && !revived)
                {
                    health = 1;
                    revived = true;
                    GetComponent<PhotonGameView>().favor += 3;
                }
                else
                {
                    health = 0;
                    dead = true;
                    if (GetComponent<PhotonGameView>())
                    {
                        GetComponent<PhotonGameView>().retreated = false;
                        GetComponent<PhotonGameView>().defended = false;
                    }
                }
            }
        }

        [PunRPC]
        public void Heal(int amount)
        {
            health += amount;
            if (health > startingHealth)
                health = startingHealth;
        }

        [PunRPC]
        public void Snare(bool active)
        {
            snared = active;
        }
    }
}