using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    public GameObject attackingPlayer;
    public GameObject defendingEntity;

    public int MAX_FAVOR;

    public int myId;
    public int totalPlayers;

    public GameObject[] champions;
    public GameObject[] combatants;
    public float DISTANCE;

    public GameObject turnOptions;
    public GameObject turnMarker;
    public GameObject[] attackMarkers;

    //Combat arena components
    public GameObject combatArea;
    public int attackerId;
    public GameObject attacker;
    public GameObject attackerDamage;
    public Text attackerDamageText;
    public GameObject defender;
    public GameObject defenderDamage;
    public Text defenderDamageText;
    public GameObject target;
    public GameObject rollButton;
    public Text roll;
    public GameObject success;
    public GameObject fail;
    public Sprite championCardBack;
    public Sprite combatantCardBack;
    public GameObject winnerCard;
    public GameObject winnerPlayer;
    public Text winnerHealth;
    public Text winnerAttack;
    public Text winnerDefense;
    public NetworkManager networkManager;
    public Sprite winnerImage;
    public Sprite loserImage;
    public Text attackerName;
    public Text defenderName;
    public Text attackerHealthText;
    public Text attackerAttackText;
    public Text attackerDefenseText;
    public Text defenderChampionHealthText;
    public Text defenderChampionAttackText;
    public Text defenderChampionDefenseText;
    public Text defenderCombatantHealthText;
    public Text defenderCombatantAttackText;
    public Text defenderCombatantDefenseText;
    public GameObject attackerDefenseBorder;
    public GameObject defenderDefenseBorder;

    private GameObject eventDeck; 

    private bool gameOver;
    private bool gameCanEnd;

    private Vector3 pos0;
    private Vector3 play2pos1;

    private Vector3 play3pos1;
    private Vector3 play3pos2;

    private Vector3 play4pos1;
    private Vector3 play4pos2;
    private Vector3 play4pos3;

    private Vector3 play5pos1;
    private Vector3 play5pos2;
    private Vector3 play5pos3;
    private Vector3 play5pos4;

    private float[] rotations = new float[5];

    private ChampionList championListScript = new ChampionList();
    private List<Champion> championList = new List<Champion>();
    private CombatantList combatantListScript = new CombatantList();
    private List<Combatant> combatantList = new List<Combatant>();
    private List<Combatant> combatantDeck;
    private int combatantCount = 0;

    private void OnEnable()
    {
        myId = -1;
        gameCanEnd = false;
        gameOver = false;
        winnerCard.SetActive(false);
        turnMarker.SetActive(false);
        for (int i = 0; i < 10; i++)
            attackMarkers[i].SetActive(false);
        CalculatePositions();
        championList = championListScript.Champions();
        combatantList = combatantListScript.Combatants();
        if (PhotonNetwork.IsMasterClient)
        {
            MakeCombatantDeck();
            MakeEventDeck();
            StartCoroutine(CountdownToStart());
        }
    }

    private void OnDisable()
    {
        turnOptions.SetActive(false);
    }

    public void CreateChampion(string playerName, int id, int champion, int totalPlayers)
    {
        this.totalPlayers = totalPlayers;
        var player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player (Game)"), Vector3.zero, Quaternion.identity);
        player.GetComponent<PhotonTargetView>().id = id;
        player.GetComponent<PhotonGameView>().playerName = playerName;
        player.GetComponent<PhotonGameView>().champion = champion;
        Champion thisChampion = championList[champion];
        player.GetComponent<PhotonGameView>().championName = thisChampion.name;
        player.GetComponent<PhotonTargetView>().health = thisChampion.health;
        player.GetComponent<PhotonTargetView>().startingHealth = thisChampion.health;
        player.GetComponent<PhotonTargetView>().attack = thisChampion.attack;
        player.GetComponent<PhotonTargetView>().defense = thisChampion.defense;
        player.GetComponent<PhotonTargetView>().effect = thisChampion.effect;
        player.GetComponent<PhotonTargetView>().stars = thisChampion.stars;
        StartCoroutine(GetMyId(id,totalPlayers));
    }

    IEnumerator GetMyId(int id, int totalPlayers)
    {
        do
        {
            champions = GameObject.FindGameObjectsWithTag("Champion");
            yield return new WaitForSeconds(.2f);
        }
        while (champions.Length < totalPlayers);
        yield return new WaitForSeconds(1);
        champions = GameObject.FindGameObjectsWithTag("Champion");
        SortById(champions);
        for (int i = 0; i < champions.Length; i++)
        {
            if (champions[i].GetComponent<PhotonTargetView>().id == id)
                myId = i;
        }
    }

    private void Update()
    {
        if (!winnerCard.activeInHierarchy && myId > -1)
        {
            DisplayChampions();
            DisplayCombatants();
            DisplayEvents();
            if (champions.Length > myId)
                turnOptions.SetActive(champions[myId].GetComponent<PhotonGameView>().turnPhase == 1);
            DrawCombatArea();
        }
        if (PhotonNetwork.IsMasterClient && gameCanEnd && myId > -1)
        {
            for (int i = 0; i < combatants.Length; i++)
            {
                if(combatants[i].GetComponent<PhotonTargetView>().dead)
                    PhotonNetwork.Destroy(combatants[i]);
            }

            if (combatants.Length == 0)
            {
                if (combatantDeck.Count > 0)
                {
                    DrawEvent();
                    DrawCombatants(champions.Length);
                }
            }

            int alive = 0;
            int aliveId = 0;
            for (int i = 0; i < champions.Length; i++)
            {
                if (!champions[i].GetComponent<PhotonTargetView>().dead)
                {
                    alive++;
                    aliveId = i;
                }
            }

            if (alive <= 1 && !gameOver)
            {
                for (int i = 0; i < champions.Length; i++)
                {
                    champions[i].GetComponent<PhotonView>().RPC("DrawWinner", champions[i].GetComponent<PhotonView>().Owner, aliveId);
                }
            }

            int highestFavor = 0;
            int favorId = 0;
            for (int i = 0; i < champions.Length; i++)
            {
                if (champions[i].GetComponent<PhotonGameView>().favor > highestFavor)
                {
                    highestFavor = champions[i].GetComponent<PhotonGameView>().favor;
                    favorId = i;
                }
            }

            if (highestFavor >= MAX_FAVOR && !gameOver)
            {
                for (int i = 0; i < champions.Length; i++)
                {
                    champions[i].GetComponent<PhotonView>().RPC("DrawWinner", champions[i].GetComponent<PhotonView>().Owner, favorId);
                }
            }
        }
    }

    private void DisplayEvents()
    {
        eventDeck = GameObject.FindGameObjectWithTag("Event Deck");
        if (!eventDeck)
            return;
        eventDeck.transform.SetParent(transform);
        eventDeck.transform.localPosition = new Vector3(225, -150, 0);
        string card = eventDeck.GetComponent<PhotonEventDeckView>().GetEvent();
        if (card == "none")
            eventDeck.GetComponent<Image>().enabled = false;
        else
        {
            eventDeck.GetComponent<Image>().enabled = true;
            eventDeck.GetComponent<Image>().sprite = Resources.Load<Sprite>("Events/" + card);
        }
    }

    private void DisplayChampions()
    {
        champions = GameObject.FindGameObjectsWithTag("Champion");
        SortById(champions);
        for (int i = 0; i < champions.Length; i++)
        {
            champions[i].transform.SetParent(transform);
            if (champions[i].GetComponent<PhotonTargetView>().dead)
            {
                champions[i].GetComponent<Image>().sprite = championCardBack;
            }
            else
            {
                champions[i].GetComponent<Image>().sprite = championList[champions[i].GetComponent<PhotonGameView>().champion].image;
            }

            if (champions[i].GetComponent<PhotonGameView>().retreated)
                rotations[i] = -90;
            else
                rotations[i] = 0;

            if (champions[i].GetComponent<PhotonGameView>().turnPhase == 1)
            {
                turnMarker.SetActive(true);
                turnMarker.transform.localPosition = champions[i].transform.localPosition;
                turnMarker.transform.rotation = champions[i].transform.rotation;
            }

            champions[i].GetComponent<PhotonGameView>().defenseBorder.SetActive(champions[i].GetComponent<PhotonGameView>().defended);

            if (!champions[i].GetComponent<PhotonTargetView>().dead)
            {
                champions[i].GetComponent<PhotonTargetView>().healthText.text = champions[i].GetComponent<PhotonTargetView>().health + "";
                if (champions[i].GetComponent<PhotonGameView>().defended)
                {
                    champions[i].GetComponent<PhotonTargetView>().attackText.text = "<color=red>" + (int.Parse(champions[i].GetComponent<PhotonTargetView>().attack.Substring(0, 1)) - 1) + "</color>-<color=red>" + (int.Parse(champions[i].GetComponent<PhotonTargetView>().attack.Substring(2, 1)) - 1) + "</color>";
                    champions[i].GetComponent<PhotonTargetView>().defenseText.text = "<color=#00FF00FF>" + (champions[i].GetComponent<PhotonTargetView>().defense + 1) + "</color>";
                }
                else
                {
                    champions[i].GetComponent<PhotonTargetView>().attackText.text = champions[i].GetComponent<PhotonTargetView>().attack + "";
                    champions[i].GetComponent<PhotonTargetView>().defenseText.text = champions[i].GetComponent<PhotonTargetView>().defense + "";
                }
            }
            else
            {
                champions[i].GetComponent<PhotonTargetView>().healthText.text = "";
                champions[i].GetComponent<PhotonTargetView>().attackText.text = "";
                champions[i].GetComponent<PhotonTargetView>().defenseText.text = "";
            }
        }

        switch (champions.Length)
        {
            case 2:
                //Rotations
                champions[(myId + 0) % 2].transform.localEulerAngles = new Vector3(0, 0, rotations[(myId + 0) % 2]);
                champions[(myId + 1) % 2].transform.localEulerAngles = new Vector3(0, 0, 180 + rotations[(myId + 1) % 2]);
                //Positions
                champions[(myId + 0) % 2].transform.localPosition = pos0;
                champions[(myId + 1) % 2].transform.localPosition = play2pos1;
                break;
            case 3:
                //Rotations
                champions[(myId + 0) % 3].transform.localEulerAngles = new Vector3(0, 0, rotations[(myId + 0) % 3]);
                champions[(myId + 1) % 3].transform.localEulerAngles = new Vector3(0, 0, 120 + rotations[(myId + 1) % 3]);
                champions[(myId + 2) % 3].transform.localEulerAngles = new Vector3(0, 0, -120 + rotations[(myId + 2) % 3]);
                //Positions
                champions[(myId + 0) % 3].transform.localPosition = pos0;
                champions[(myId + 1) % 3].transform.localPosition = play3pos1;
                champions[(myId + 2) % 3].transform.localPosition = play3pos2;
                break;
            case 4:
                //Rotations
                champions[(myId + 0) % 4].transform.localEulerAngles = new Vector3(0, 0, rotations[(myId + 0) % 4]);
                champions[(myId + 1) % 4].transform.localEulerAngles = new Vector3(0, 0, 90 + rotations[(myId + 1) % 4]);
                champions[(myId + 2) % 4].transform.localEulerAngles = new Vector3(0, 0, 180 + rotations[(myId + 2) % 4]);
                champions[(myId + 3) % 4].transform.localEulerAngles = new Vector3(0, 0, -90 + rotations[(myId + 3) % 4]);
                //Positions
                champions[(myId + 0) % 4].transform.localPosition = pos0;
                champions[(myId + 1) % 4].transform.localPosition = play4pos1;
                champions[(myId + 2) % 4].transform.localPosition = play4pos2;
                champions[(myId + 3) % 4].transform.localPosition = play4pos3;
                break;
            case 5:
                //Rotations
                champions[(myId + 0) % 5].transform.localEulerAngles = new Vector3(0, 0, rotations[(myId + 0) % 5]);
                champions[(myId + 1) % 5].transform.localEulerAngles = new Vector3(0, 0, 72 + rotations[(myId + 1) % 5]);
                champions[(myId + 2) % 5].transform.localEulerAngles = new Vector3(0, 0, 144 + rotations[(myId + 2) % 5]);
                champions[(myId + 3) % 5].transform.localEulerAngles = new Vector3(0, 0,-144 + rotations[(myId + 3) % 5]);
                champions[(myId + 4) % 5].transform.localEulerAngles = new Vector3(0, 0, -72 + rotations[(myId + 4) % 5]);
                //Positions
                champions[(myId + 0) % 5].transform.localPosition = pos0;
                champions[(myId + 1) % 5].transform.localPosition = play5pos1;
                champions[(myId + 2) % 5].transform.localPosition = play5pos2;
                champions[(myId + 3) % 5].transform.localPosition = play5pos3;
                champions[(myId + 4) % 5].transform.localPosition = play5pos4;
                break;
        }
    }

    private void DisplayCombatants()
    {
        combatants = GameObject.FindGameObjectsWithTag("Combatant");
        for (int i = 0; i < combatants.Length; i++)
        {
            combatants[i].transform.SetParent(transform);
            combatants[i].GetComponent<Image>().sprite = combatantList[combatants[i].GetComponent<PhotonTargetView>().id].image;

            combatants[i].GetComponent<PhotonTargetView>().healthText.text = combatants[i].GetComponent<PhotonTargetView>().health + "";
            combatants[i].GetComponent<PhotonTargetView>().attackText.text = combatants[i].GetComponent<PhotonTargetView>().attack + "";
            combatants[i].GetComponent<PhotonTargetView>().defenseText.text = combatants[i].GetComponent<PhotonTargetView>().defense + "";
        }

        int widthOffset = 60;
        int heightOffset = 80;

        switch (combatants.Length)
        {
            case 1:
                //Positions
                combatants[0].transform.localPosition = new Vector3(0, 0, 0);
                break;
            case 2:
                //Positions
                combatants[0].transform.localPosition = new Vector3(-widthOffset/2f, 0, 0);
                combatants[1].transform.localPosition = new Vector3(widthOffset/2f, 0, 0);
                break;
            case 3:
                //Positions
                combatants[0].transform.localPosition = new Vector3(-widthOffset,0,0);
                combatants[1].transform.localPosition = new Vector3(0, 0, 0);
                combatants[2].transform.localPosition = new Vector3(widthOffset, 0, 0);
                break;
            case 4:
                //Positions
                combatants[0].transform.localPosition = new Vector3(-widthOffset/2f, heightOffset / 2f, 0);
                combatants[1].transform.localPosition = new Vector3(widthOffset/2f, heightOffset / 2f, 0);
                combatants[2].transform.localPosition = new Vector3(-widthOffset/2f, -heightOffset / 2f, 0);
                combatants[3].transform.localPosition = new Vector3(widthOffset/2f, -heightOffset / 2f, 0);
                break;
            case 5:
                //Positions
                combatants[0].transform.localPosition = new Vector3(-widthOffset, heightOffset / 2f, 0);
                combatants[1].transform.localPosition = new Vector3(0, heightOffset / 2f, 0);
                combatants[2].transform.localPosition = new Vector3(widthOffset, heightOffset / 2f, 0);
                combatants[3].transform.localPosition = new Vector3(-widthOffset / 2f, -heightOffset / 2f, 0);
                combatants[4].transform.localPosition = new Vector3(widthOffset / 2f, -heightOffset / 2f, 0);
                break;
            case 6:
                //Positions
                combatants[0].transform.localPosition = new Vector3(-widthOffset, heightOffset / 2f, 0);
                combatants[1].transform.localPosition = new Vector3(0, heightOffset / 2f, 0);
                combatants[2].transform.localPosition = new Vector3(widthOffset, heightOffset / 2f, 0);
                combatants[3].transform.localPosition = new Vector3(-widthOffset, -heightOffset / 2f, 0);
                combatants[4].transform.localPosition = new Vector3(0, -heightOffset / 2f, 0);
                combatants[5].transform.localPosition = new Vector3(widthOffset, -heightOffset / 2f, 0);
                break;
        }
    }

    private void CalculatePositions()
    {
        pos0 = new Vector3(0,-DISTANCE,0);
        play2pos1 = new Vector3(0, DISTANCE, 0);

        play3pos1 = new Vector3(Mathf.Cos(30 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(30 * Mathf.Deg2Rad) * DISTANCE, 0);
        play3pos2 = new Vector3(Mathf.Cos(150 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(150 * Mathf.Deg2Rad) * DISTANCE, 0);

        play4pos1 = new Vector3(DISTANCE, 0, 0);
        play4pos2 = new Vector3(0, DISTANCE, 0);
        play4pos3 = new Vector3(-DISTANCE, 0, 0);

        play5pos1 = new Vector3(Mathf.Cos(342 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(342 * Mathf.Deg2Rad) * DISTANCE, 0);
        play5pos2 = new Vector3(Mathf.Cos(54 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(54 * Mathf.Deg2Rad) * DISTANCE, 0);
        play5pos3 = new Vector3(Mathf.Cos(126 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(126 * Mathf.Deg2Rad) * DISTANCE, 0);
        play5pos4 = new Vector3(Mathf.Cos(198 * Mathf.Deg2Rad) * DISTANCE, Mathf.Sin(198 * Mathf.Deg2Rad) * DISTANCE, 0);
    }

    private void SortById(GameObject[] champions)
    {
        for (int p = 0; p < champions.Length - 1; p++)
        {
            for (int i = 0; i < champions.Length - 1; i++)
            {
                if (champions[i].GetComponent<PhotonTargetView>().id > champions[i + 1].GetComponent<PhotonTargetView>().id)
                {
                    GameObject t = champions[i + 1];
                    champions[i + 1] = champions[i];
                    champions[i] = t;
                }
            }
        }
    }

    private void DrawCombatArea()
    {
        bool active = champions[myId].GetComponent<PhotonGameView>().turnPhase > 1;
        combatArea.SetActive(active);

        if (active)
        {
            attackingPlayer = GetCharacterFromId(attackerId, true);
            defendingEntity = GetCharacterFromId(attackingPlayer.GetComponent<PhotonGameView>().defenderId, attackingPlayer.GetComponent<PhotonGameView>().defenderIsPlayer);

            attacker.GetComponent<Image>().sprite = attackingPlayer.GetComponent<Image>().sprite;
            attackerName.text = attackingPlayer.GetComponent<PhotonGameView>().playerName;
            attackerDefenseBorder.SetActive(attackingPlayer.GetComponent<PhotonGameView>().defended);
            if (!attackingPlayer.GetComponent<PhotonTargetView>().dead)
            {
                attackerHealthText.GetComponent<Text>().text = attackingPlayer.GetComponent<PhotonTargetView>().health + "";
                attackerAttackText.GetComponent<Text>().text = attackingPlayer.GetComponent<PhotonTargetView>().attack + "";
                attackerDefenseText.GetComponent<Text>().text = attackingPlayer.GetComponent<PhotonTargetView>().defense + "";
            }

            if (defendingEntity)
            {
                if(!defendingEntity.GetComponent<PhotonTargetView>().dead)
                    defender.GetComponent<Image>().sprite = defendingEntity.GetComponent<Image>().sprite;
                else
                    defender.GetComponent<Image>().sprite = combatantCardBack;
                if (attackingPlayer.GetComponent<PhotonGameView>().defenderIsPlayer)
                {
                    defenderName.text = defendingEntity.GetComponent<PhotonGameView>().playerName;
                    defenderDefenseBorder.SetActive(defendingEntity.GetComponent<PhotonGameView>().defended);
                }
                else
                {
                    defenderName.text = "";
                    defenderDefenseBorder.SetActive(false);
                }
                if (!defendingEntity.GetComponent<PhotonTargetView>().dead)
                {
                    if (defendingEntity.GetComponent<PhotonGameView>())
                    {
                        if (defendingEntity.GetComponent<PhotonGameView>().defended)
                        {
                            defenderChampionHealthText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().health + "";
                            defenderChampionAttackText.GetComponent<Text>().text = "<color=red>" + (int.Parse(defendingEntity.GetComponent<PhotonTargetView>().attack.Substring(0, 1)) - 1) + "</color>-<color=red>" + (int.Parse(defendingEntity.GetComponent<PhotonTargetView>().attack.Substring(2, 1)) - 1) + "</color>";
                            defenderChampionDefenseText.GetComponent<Text>().text = "<color=#00FF00FF>" + (defendingEntity.GetComponent<PhotonTargetView>().defense + 1) + "</color>";

                            defenderCombatantHealthText.GetComponent<Text>().text = "";
                            defenderCombatantAttackText.GetComponent<Text>().text = "";
                            defenderCombatantDefenseText.GetComponent<Text>().text = "";
                        }
                        else
                        {
                            defenderChampionHealthText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().health + "";
                            defenderChampionAttackText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().attack + "";
                            defenderChampionDefenseText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().defense + "";

                            defenderCombatantHealthText.GetComponent<Text>().text = "";
                            defenderCombatantAttackText.GetComponent<Text>().text = "";
                            defenderCombatantDefenseText.GetComponent<Text>().text = "";
                        }
                    }
                    else
                    {
                        defenderCombatantHealthText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().health + "";
                        defenderCombatantAttackText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().attack + "";
                        defenderCombatantDefenseText.GetComponent<Text>().text = defendingEntity.GetComponent<PhotonTargetView>().defense + "";

                        defenderChampionHealthText.GetComponent<Text>().text = "";
                        defenderChampionAttackText.GetComponent<Text>().text = "";
                        defenderChampionDefenseText.GetComponent<Text>().text = "";
                    }
                }
                else
                {
                    defenderChampionHealthText.GetComponent<Text>().text = "";
                    defenderChampionAttackText.GetComponent<Text>().text = "";
                    defenderChampionDefenseText.GetComponent<Text>().text = "";

                    defenderCombatantHealthText.GetComponent<Text>().text = "";
                    defenderCombatantAttackText.GetComponent<Text>().text = "";
                    defenderCombatantDefenseText.GetComponent<Text>().text = "";
                }

                if (attackingPlayer.GetComponent<PhotonGameView>().defenderIsPlayer && defendingEntity.GetComponent<PhotonGameView>().roll > attackingPlayer.GetComponent<PhotonGameView>().roll)
                    roll.text = defendingEntity.GetComponent<PhotonGameView>().roll + "";
                else
                    roll.text = attackingPlayer.GetComponent<PhotonGameView>().roll + "";

                if (attackingPlayer.GetComponent<PhotonGameView>().turnPhase != 4)
                {
                    int reduction = 0;
                    //Dimachaerus Effect 1/2
                    if (attackingPlayer.GetComponent<PhotonTargetView>().effect == "Killer")
                        reduction += 1;

                    //Blinding Sun Effect (1) 1/2
                    if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BlindingSun1" && attackingPlayer.GetComponent<PhotonGameView>())
                    {
                        if (champions[(myId + champions.Length - 1) % champions.Length].GetComponent<PhotonTargetView>().id == attackingPlayer.GetComponent<PhotonTargetView>().id)
                            reduction -= 1;
                    }

                    //Blinding Sun Effect (2) 1/2
                    if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BlindingSun2" && attackingPlayer.GetComponent<PhotonGameView>())
                    {
                        if (champions[(myId + 1) % champions.Length].GetComponent<PhotonTargetView>().id == attackingPlayer.GetComponent<PhotonTargetView>().id)
                            reduction -= 1;
                    }

                    if (attackingPlayer.GetComponent<PhotonGameView>() && attackingPlayer.GetComponent<PhotonGameView>().defended)
                        reduction -= 1;

                    if (attackingPlayer.GetComponent<PhotonGameView>().roll == 0 || defenderDamage.activeInHierarchy)
                    {
                        success.SetActive(false);
                        fail.SetActive(false);
                    }
                    else if (attackingPlayer.GetComponent<PhotonGameView>().roll > defendingEntity.GetComponent<PhotonTargetView>().defense - reduction)
                    {
                        success.SetActive(true);
                        fail.SetActive(false);
                    }
                    else
                    {
                        success.SetActive(false);
                        fail.SetActive(true);
                    }
                }
                else
                {
                    success.SetActive(false);
                    fail.SetActive(false);
                }
            }
        }

        rollButton.SetActive(champions[myId].GetComponent<PhotonGameView>().turnPhase == 2 || champions[myId].GetComponent<PhotonGameView>().turnPhase == 4 || champions[myId].GetComponent<PhotonGameView>().turnPhase == 5);
    }

    public void Roll()
    {
        //Roll to hit
        if (champions[myId].GetComponent<PhotonGameView>().turnPhase == 2)
        {
            int r = Random.Range(1, 7);
            champions[myId].GetComponent<PhotonGameView>().roll = r;
            champions[myId].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[myId].GetComponent<PhotonView>().Owner, 6);
            StartCoroutine(ProcessRoll(r));
        }
        //Roll for damage (attacker)
        else if (champions[myId].GetComponent<PhotonGameView>().turnPhase == 4)
        {
            int damageMin = int.Parse(champions[myId].GetComponent<PhotonTargetView>().attack.Substring(0, 1));
            int damageMax = int.Parse(champions[myId].GetComponent<PhotonTargetView>().attack.Substring(2, 1)) + 1;
            int r = Random.Range(damageMin, damageMax);
            champions[myId].GetComponent<PhotonGameView>().roll = r;
            champions[myId].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[myId].GetComponent<PhotonView>().Owner, 6);

            int favorGain = 1;

            //Provocator Effect 1/1
            if (champions[myId].GetComponent<PhotonTargetView>().effect == "Glory")
                favorGain = 2;

            if (target.GetComponent<PhotonTargetView>().effect == "Object")
                favorGain = 0;

            champions[myId].GetComponent<PhotonGameView>().favor += favorGain;

            //Beastiarius Effect 1/2
            if (champions[myId].GetComponent<PhotonTargetView>().effect == "Tamer" && target.GetComponent<PhotonTargetView>().effect == "Beast")
                r += 2;

            //Target taking damage makes them defended afterwards
            if (target.GetComponent<PhotonGameView>())
                target.GetComponent<PhotonView>().RPC("Defended", RpcTarget.All, true);

            target.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, r);

            //check to see if killed target
            if (target.GetComponent<PhotonTargetView>().dead)
            {
                champions[myId].GetComponent<PhotonView>().RPC("GainFavor", RpcTarget.All, target.GetComponent<PhotonTargetView>().stars);
            }

            //Laquearius Effect 1/1
            if (champions[myId].GetComponent<PhotonTargetView>().effect == "Snare")
                target.GetComponent<PhotonView>().RPC("Snare", RpcTarget.All, true);

            //Booing Crowd Effect 1/3
            if(eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BooingCrowd" && target.GetComponent<PhotonGameView>() && r > 0)
                target.GetComponent<PhotonView>().RPC("LoseFavor", RpcTarget.All, 1);

            for (int i = 0; i < champions.Length; i++)
            {
                champions[i].GetComponent<PhotonView>().RPC("DrawDamage", champions[i].GetComponent<PhotonView>().Owner, r, true);
            }
            StartCoroutine(EndTurn());
        }
        //Roll for damage (defender)
        else if (champions[myId].GetComponent<PhotonGameView>().turnPhase == 5)
        {
            int damageMin = int.Parse(champions[myId].GetComponent<PhotonTargetView>().attack.Substring(0, 1));
            int damageMax = int.Parse(champions[myId].GetComponent<PhotonTargetView>().attack.Substring(2, 1)) + 1;

            if (champions[myId].GetComponent<PhotonGameView>().defended)
            {
                damageMin--;
                damageMax--;
            }

            int r = Random.Range(damageMin, damageMax);
            champions[myId].GetComponent<PhotonGameView>().roll = r;
            champions[myId].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[myId].GetComponent<PhotonView>().Owner, 3);
            GetCharacterFromId(attackerId, true).GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, r);

            //check to see if killed target
            if (GetCharacterFromId(attackerId, true).GetComponent<PhotonTargetView>().dead)
            {
                champions[myId].GetComponent<PhotonView>().RPC("GainFavor", RpcTarget.All, GetCharacterFromId(attackerId, true).GetComponent<PhotonTargetView>().stars);
            }

            //Booing Crowd Effect 2/3
            if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BooingCrowd" && r > 0)
                GetCharacterFromId(attackerId, true).GetComponent<PhotonView>().RPC("LoseFavor", RpcTarget.All, 1);

            for (int i = 0; i < champions.Length; i++)
            {
                champions[i].GetComponent<PhotonView>().RPC("DrawDamage", champions[i].GetComponent<PhotonView>().Owner, r, false);
            }
            StartCoroutine(EndTurn());
        }
    }


    IEnumerator ProcessRoll(int roll)
    {
        yield return new WaitForSeconds(1);
        champions[myId].GetComponent<PhotonGameView>().roll = 0;

        int reduction = 0;
        //Dimachaerus Effect 2/2
        if (champions[myId].GetComponent<PhotonTargetView>().effect == "Killer")
            reduction++;

        //Blinding Sun Effect (1) 2/2
        if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BlindingSun1" && target.GetComponent<PhotonGameView>())
        {
            if (champions[(myId + champions.Length - 1) % champions.Length].GetComponent<PhotonTargetView>().id == target.GetComponent<PhotonTargetView>().id)
                reduction--;
        }

        //Blinding Sun Effect (2) 2/2
        if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BlindingSun2" && target.GetComponent<PhotonGameView>())
        {
            if (champions[(myId + 1) % champions.Length].GetComponent<PhotonTargetView>().id == target.GetComponent<PhotonTargetView>().id)
                reduction--;
        }

        if (target.GetComponent<PhotonGameView>() && target.GetComponent<PhotonGameView>().defended)
            reduction--;


        //Debug.LogError("Roll[" + roll + "], Combined Defense[" + (target.GetComponent<PhotonTargetView>().defense - reduction) + "], Defense[" + target.GetComponent<PhotonTargetView>().defense + "], Reduction[" + reduction + "]");
        if (roll > target.GetComponent<PhotonTargetView>().defense - reduction)
        {
            champions[myId].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[myId].GetComponent<PhotonView>().Owner, 4);
            //Debug.LogError("Attack Succeeded!");
        }
        else
        {
            //Debug.LogError("Attack Failed!");
            if (target.GetComponent<PhotonGameView>())
            {
                int favorGain = 0;

                //Scutarius Effect 1/1
                if (target.GetComponent<PhotonTargetView>().effect == "Bulwark")
                    favorGain = 1;

                target.GetComponent<PhotonView>().RPC("GainFavor",RpcTarget.All,favorGain);
            }

            //Sagittarius Effect 1/1
            if (champions[myId].GetComponent<PhotonTargetView>().effect == "Sniper")
                StartCoroutine(EndTurn());
            else if (champions[myId].GetComponent<PhotonGameView>().defenderIsPlayer)
            {
                target.GetComponent<PhotonView>().RPC("SetTurnPhase", target.GetComponent<PhotonView>().Owner, 5);
            }
            else
            {
                int damage = int.Parse(target.GetComponent<PhotonTargetView>().attack);

                //Beastiarius Effect 2/2
                if (champions[myId].GetComponent<PhotonTargetView>().effect == "Tamer" && target.GetComponent<PhotonTargetView>().effect == "Beast")
                {
                    damage -= 2;
                    if (damage < 0)
                        damage = 0;
                }

                //Booing Crowd Effect 3/3
                if (eventDeck.GetComponent<PhotonEventDeckView>().GetEvent() == "BooingCrowd" && damage > 0)
                    champions[myId].GetComponent<PhotonView>().RPC("LoseFavor", RpcTarget.All, 1);

                champions[myId].GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
                for (int i = 0; i < champions.Length; i++)
                {
                    champions[i].GetComponent<PhotonView>().RPC("DrawDamage", champions[i].GetComponent<PhotonView>().Owner, damage, false);
                }
                StartCoroutine(EndTurn());
            }
        }
    }

    IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        bool someonePhase6;
        do
        {
            someonePhase6 = false;
            for (int i = 0; i < champions.Length; i++)
            {
                if (champions[i].GetComponent<PhotonGameView>().turnPhase == 6)
                    someonePhase6 = true;
            }
            if(!someonePhase6)
                yield return new WaitForSeconds(.2f);
        }
        while (!someonePhase6);

        int id = -1;
        for (int i = 0; i < champions.Length; i++)
        {
            if (champions[i].GetComponent<PhotonGameView>().turnPhase == 6)
            {
                id = champions[i].GetComponent<PhotonTargetView>().id;
            }
        }
        for (int i = 0; i < champions.Length; i++)
        {
            champions[i].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[i].GetComponent<PhotonView>().Owner, 0);
        }
        champions[myId].GetComponent<PhotonGameView>().roll = 0;
        champions[myId].GetComponent<PhotonTargetView>().snared = false;
        target = null;
        GetCharacterFromId((id % champions.Length) + 1, true).GetComponent<PhotonView>().RPC("SetTurnPhase", GetCharacterFromId((id % champions.Length) + 1, true).GetComponent<PhotonView>().Owner, 1);
    }

    private GameObject GetCharacterFromId(int id, bool isPlayer)
    {
        if (isPlayer)
        {
            for (int i = 0; i < champions.Length; i++)
            {
                if (champions[i].GetComponent<PhotonTargetView>().id == id)
                    return champions[i];
            }
        }
        else
        {
            for (int i = 0; i < combatants.Length; i++)
            {
                if (combatants[i].GetComponent<PhotonTargetView>().uid == id)
                    return combatants[i];
            }
        }
        return null;
    }

    private void MakeCombatantDeck()
    {
        combatantDeck = new List<Combatant>();
        for (int i = 0; i < combatantListScript.TOTAL_COMBATANTS; i++)
        {
            Combatant combatant = Resources.Load<Combatant>("Combatants/" + i);
            for(int j = 0; j < combatant.amountInDeck; j++)
                combatantDeck.Add(combatant);
        }
        Shuffle(combatantDeck);
        for (int i = 0; i < combatantDeck.Count; i++)
            Debug.Log("[" + i + "] " + combatantDeck[i].combatantName);
    }

    private void MakeEventDeck()
    {
        //Event Deck Settings
        int copiesInDeck = 2;
        int numberOfEvents = 8;
        int cardsInDeck = numberOfEvents * copiesInDeck;

        string[] eventDeck = new string[cardsInDeck];

        for (int i = 0; i < cardsInDeck; i += numberOfEvents)
        {
            eventDeck[i + 0] = "BooingCrowd";
            eventDeck[i + 1] = "BooingCrowd";

            eventDeck[i + 2] = "FairConditions";
            eventDeck[i + 3] = "FairConditions";
            eventDeck[i + 4] = "FairConditions";
            eventDeck[i + 5] = "FairConditions";

            eventDeck[i + 6] = "BlindingSun1";
            eventDeck[i + 7] = "BlindingSun2";
        }
        Shuffle(eventDeck);
        var deck = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Event Deck"), Vector3.zero, Quaternion.identity);
        deck.GetComponent<PhotonView>().RPC("CreateDeck", RpcTarget.All, cardsInDeck);
        for (int i = 0; i < cardsInDeck; i++)
        {
            deck.GetComponent<PhotonView>().RPC("UpdateDeck", RpcTarget.All, i, eventDeck[i]);
        }
        deck.GetComponent<PhotonEventDeckView>().iterator = -1;
    }

    private void DrawCombatants(int playersInGame)
    {
        for (int i = 0; i <= playersInGame; i++)
        {
            if (combatantDeck.Count > 0)
            {
                var combatant = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Combatant"), Vector3.zero, Quaternion.identity);
                combatant.GetComponent<PhotonTargetView>().id = GetCombatantId(combatantDeck[0].combatantName);
                combatant.GetComponent<PhotonTargetView>().uid = combatantCount++;
                combatant.GetComponent<PhotonTargetView>().health = combatantDeck[0].health;
                combatant.GetComponent<PhotonTargetView>().attack = combatantDeck[0].attack;
                combatant.GetComponent<PhotonTargetView>().defense = combatantDeck[0].defense;
                combatant.GetComponent<PhotonTargetView>().effect = combatantDeck[0].effect;
                combatant.GetComponent<PhotonTargetView>().stars = combatantDeck[0].stars;
                combatantDeck.Remove(combatantDeck[0]);
            }
        }
    }

    private void DrawEvent()
    {
        eventDeck = GameObject.FindGameObjectWithTag("Event Deck");
        eventDeck.GetComponent<PhotonView>().RPC("IncrementIterator",RpcTarget.All);
    }

    IEnumerator CountdownToStart()
    {
        do
        {
            champions = GameObject.FindGameObjectsWithTag("Champion");
            yield return new WaitForSeconds(.2f);
        }
        while (champions.Length < totalPlayers);

        DrawEvent();
        DrawCombatants(champions.Length);
        yield return new WaitForSeconds(1f);
        int startingPlayer = Random.Range(0,champions.Length);
        champions[startingPlayer].GetComponent<PhotonView>().RPC("SetTurnPhase",RpcTarget.All,1);
        gameCanEnd = true;
    }

    private void Shuffle<T>(List<T> alpha)
    {
        for (int i = 0; i < alpha.Count; i++)
        {
            T temp = alpha[i];
            int randomIndex = Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
    }

    private void Shuffle<T>(T[] alpha)
    {
        for (int i = 0; i < alpha.Length; i++)
        {
            T temp = alpha[i];
            int randomIndex = Random.Range(i, alpha.Length);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
    }

    private int GetCombatantId(string combatantName)
    {
        for (int i = 0; i < combatantList.Count; i++)
        {
            if (combatantName == combatantList[i].combatantName)
                return i;
        }
        return -1;
    }

    public void WatchCombat(GameObject target)
    {
        this.target = target;
        //If Target is player
        bool isplayer = target.GetComponent<PhotonGameView>() != null;
        champions[myId].GetComponent<PhotonGameView>().defenderIsPlayer = isplayer;
        if(isplayer)
            champions[myId].GetComponent<PhotonGameView>().defenderId = target.GetComponent<PhotonTargetView>().id;
        else
            champions[myId].GetComponent<PhotonGameView>().defenderId = target.GetComponent<PhotonTargetView>().uid;
        champions[myId].GetComponent<PhotonGameView>().roll = 0;

        champions[myId].GetComponent<PhotonView>().RPC("WatchCombat", champions[myId].GetComponent<PhotonView>().Owner, 2, myId + 1);
        for (int i = 0; i < champions.Length; i++)
        {
            if(i != myId)
                champions[i].GetComponent<PhotonView>().RPC("WatchCombat", champions[i].GetComponent<PhotonView>().Owner, 3, myId + 1);
        }
    }

    public void DrawDamage(int amount, bool attackSuccess)
    {
        StartCoroutine(DamageEffect(amount, attackSuccess));
    }

    IEnumerator DamageEffect(int amount, bool attackSuccess)
    {
        if (attackSuccess)
        {
            defenderDamage.SetActive(true);
            defenderDamageText.text = "-" + amount + " ";
        }
        else
        {
            attackerDamage.SetActive(true);
            attackerDamageText.text = "-" + amount + " ";
        }
        yield return new WaitForSeconds(1);
        champions[myId].GetComponent<PhotonGameView>().roll = 0;
        attackerDamage.SetActive(false);
        defenderDamage.SetActive(false);
    }

    public bool Unretreat()
    {
        if (champions[myId].GetComponent<PhotonGameView>().retreated)
        {
            champions[myId].GetComponent<PhotonGameView>().retreated = false;
            return true;
        }
        return false;
    }

    public void Retreat(int cost)
    {
        //Thraex effect 2/2
        for (int i = 0; i < champions.Length; i++)
        {
            if (champions[i].GetComponent<PhotonTargetView>().effect == "Warrior")
            {
                champions[i].GetComponent<PhotonView>().RPC("GainFavor", RpcTarget.All, 1);
            }
        }

        champions[myId].GetComponent<PhotonView>().RPC("Heal", RpcTarget.All, 1);

        champions[myId].GetComponent<PhotonGameView>().favor -= cost;
        champions[myId].GetComponent<PhotonGameView>().retreated = true;
        for (int i = 0; i < champions.Length; i++)
        {
            champions[i].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[i].GetComponent<PhotonView>().Owner, 0);
        }
        GetCharacterFromId(((myId + 1) % champions.Length) + 1, true).GetComponent<PhotonView>().RPC("SetTurnPhase", GetCharacterFromId(((myId+1) % champions.Length) + 1, true).GetComponent<PhotonView>().Owner, 1);
    }

    public bool Undefend()
    {
        if (champions[myId].GetComponent<PhotonGameView>().defended)
        {
            champions[myId].GetComponent<PhotonGameView>().defended = false;
            return true;
        }
        return false;
    }

    public void Defend()
    {
        champions[myId].GetComponent<PhotonGameView>().defended = true;
        for (int i = 0; i < champions.Length; i++)
        {
            champions[i].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[i].GetComponent<PhotonView>().Owner, 0);
        }
        GetCharacterFromId(((myId + 1) % champions.Length) + 1, true).GetComponent<PhotonView>().RPC("SetTurnPhase", GetCharacterFromId(((myId + 1) % champions.Length) + 1, true).GetComponent<PhotonView>().Owner, 1);
    }

    public int HasEnoughFavor(int cost)
    {
        //0 - Has enough
        //1 - Does not have enough
        //2 - Snared
        //3 - Thraex

        //Thraex effect 1/2
        if (champions[myId].GetComponent<PhotonTargetView>().effect == "Warrior")
            return 3;
        if (champions[myId].GetComponent<PhotonTargetView>().snared)
            return 2;
        if (champions[myId].GetComponent<PhotonGameView>().favor >= cost)
            return 0;
        return 1;
    }

    public void GameOver(int id)
    {
        if (myId == id)
        {
            winnerCard.GetComponent<Image>().sprite = winnerImage;
        }
        else
        {
            winnerCard.GetComponent<Image>().sprite = loserImage;
        }
        gameOver = true;
        winnerCard.SetActive(true);
        winnerHealth.text = champions[myId].GetComponent<PhotonTargetView>().health + "";
        winnerAttack.text = champions[myId].GetComponent<PhotonTargetView>().attack + "";
        winnerDefense.text = champions[myId].GetComponent<PhotonTargetView>().defense + "";
        combatArea.SetActive(false);
        winnerPlayer.GetComponent<Image>().sprite = championList[champions[myId].GetComponent<PhotonGameView>().champion].image;
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < champions.Length; i++)
            {
                if (i != myId)
                    champions[i].GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);
            }
        }
        yield return new WaitForSeconds(2);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(eventDeck);
            for (int i = 0; i < combatants.Length; i++)
            {
                PhotonNetwork.Destroy(combatants[i]);
            }
            for (int i = 0; i < champions.Length; i++)
            {
                PhotonNetwork.Destroy(champions[i]);
            }
        }
        winnerCard.SetActive(false);
        networkManager.ActivateMenu(2);
    }

    public bool IsDead()
    {
        return champions[myId].GetComponent<PhotonTargetView>().dead;
    }

    public bool IsYou(GameObject t)
    {
        if (t.GetComponent<PhotonGameView>())
        {
            return t.GetComponent<PhotonTargetView>().id == champions[myId].GetComponent<PhotonTargetView>().id;
        }
        return false;
    }

    public void SkipTurn()
    {
        for (int i = 0; i < champions.Length; i++)
        {
            champions[i].GetComponent<PhotonView>().RPC("SetTurnPhase", champions[i].GetComponent<PhotonView>().Owner, 0);
        }
        champions[myId].GetComponent<PhotonGameView>().roll = 0;
        int id = myId + 1;
        GetCharacterFromId((id % champions.Length) + 1, true).GetComponent<PhotonView>().RPC("SetTurnPhase", GetCharacterFromId((id % champions.Length) + 1, true).GetComponent<PhotonView>().Owner, 1);
    }

    public bool IsEssedarius()
    {
        //Essedarius Effect 1/1
        return champions[myId].GetComponent<PhotonTargetView>().effect == "Rider";
    }

    public void SetAttackMarkers()
    {
        int counter = 0;
        for (int i = 0; i < champions.Length; i++)
        {
            //check that champion is not you, not retreated, and not dead
            if (myId != i && !champions[i].GetComponent<PhotonGameView>().retreated && !champions[i].GetComponent<PhotonTargetView>().dead)
            {
                attackMarkers[counter].SetActive(true);
                attackMarkers[counter].transform.localPosition = champions[i].transform.localPosition;
                attackMarkers[counter].transform.rotation = champions[i].transform.rotation;
                counter++;
            }
        }
        int counter2 = counter;
        for (int i = counter2; i < counter2 + combatants.Length; i++)
        {
            attackMarkers[counter].SetActive(true);
            attackMarkers[counter].transform.localPosition = combatants[i-counter2].transform.localPosition;
            attackMarkers[counter].transform.rotation = combatants[i-counter2].transform.rotation;
            counter++;
        }
        for (int i = counter; i < 10; i++)
        {
            attackMarkers[i].SetActive(false);
        }
    }

    public void ClearAttackMarkers()
    {
        for (int i = 0; i < 10; i++)
        {
            attackMarkers[i].SetActive(false);
        }
    }
}
