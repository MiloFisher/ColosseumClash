using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class TurnOptions : MonoBehaviour
{
    public bool DRAG_ATTACK = false;

    public GameObject advance;
    public GameObject defend;
    public GameObject retreat;
    public GameObject hide;
    public GameObject inputBlocker;
    public GameObject targetLine;
    public GameObject pointer;
    public GameObject confirmation;
    public HighlightCard highlightCard;
    public PlayManager playManager;
    public int retreatStreak;
    public GameObject cannotRetreat;
    public GameObject[] cannotRetreatMessages;
    public Text cost;
    public GameObject attackMessage;

    public bool canAttack;
    public bool attacking;

    private GameObject target;

    private void OnEnable()
    {
        target = null;
        playManager.Undefend();
        if (playManager.Unretreat())
            retreatStreak++;
        else if (playManager.IsEssedarius())
            retreatStreak = 0;
        else
            retreatStreak = 1;
        cost.text = "<color=black>Cost:</color>" + retreatStreak + " Favor";
        cannotRetreat.SetActive(playManager.HasEnoughFavor(retreatStreak) > 0);
        if (playManager.HasEnoughFavor(retreatStreak) == 1)
        {
            cannotRetreatMessages[0].SetActive(true);
            cannotRetreatMessages[1].SetActive(false);
            cannotRetreatMessages[2].SetActive(false);
        }
        else if (playManager.HasEnoughFavor(retreatStreak) == 2)
        {
            cannotRetreatMessages[0].SetActive(false);
            cannotRetreatMessages[1].SetActive(true);
            cannotRetreatMessages[2].SetActive(false);
        }
        else if (playManager.HasEnoughFavor(retreatStreak) == 3)
        {
            cannotRetreatMessages[0].SetActive(false);
            cannotRetreatMessages[1].SetActive(false);
            cannotRetreatMessages[2].SetActive(true);
        }
        else
        {
            cannotRetreatMessages[0].SetActive(false);
            cannotRetreatMessages[1].SetActive(false);
            cannotRetreatMessages[2].SetActive(false);
        }
        advance.SetActive(true);
        defend.SetActive(true);
        retreat.SetActive(true);
        inputBlocker.SetActive(true);
        hide.SetActive(true);
        confirmation.SetActive(false);
        attackMessage.SetActive(false);

        if (playManager.IsDead())
            playManager.SkipTurn();
    }

    private void OnDisable()
    {
        attackMessage.SetActive(false);
    }

    private void Update()
    {
        if (DRAG_ATTACK)
        {
            //If you can attack, and you are targeting something
            if (canAttack && highlightCard.target)
            {
                //If your target is you
                if (highlightCard.target.GetComponent<PhotonGameView>() && highlightCard.target.GetComponent<PhotonTargetView>().id == playManager.myId + 1)
                {
                    //Start click sets attacking true
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        attacking = true;
                    }

                    //End click with yourself as the target sets attacking false
                    if (Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        attacking = false;
                        target = null;
                    }
                }
                //Target is someone else
                else
                {
                    //End click with target sets attacking false and assigns target
                    if (Input.GetKeyUp(KeyCode.Mouse0) && attacking)
                    {
                        attacking = false;
                        target = highlightCard.target;
                    }
                }
            }
            //If you cannot attack or you are not targeting something
            else
            {
                //End click with no target sets attacking false
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    attacking = false;
                }
            }


            //Escape cancels selected target
            if (Input.GetKeyDown(KeyCode.Escape) && canAttack)
            {
                target = null;
            }

            if (attacking)
            {
                targetLine.SetActive(true);
                float x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
                float y = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
                pointer.transform.localPosition = new Vector3(x, y, 0);

                Vector3 targ = new Vector3(0, -150, 0);
                Vector3 objectPos = pointer.transform.position;
                targ.x -= objectPos.x;
                targ.y -= objectPos.y;
                float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
                pointer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                targetLine.GetComponent<LineRenderer>().SetPosition(1, new Vector3(x, y, -1));
            }
            else if (target)
            {
                targetLine.SetActive(true);
                float x = target.transform.position.x;
                float y = target.transform.position.y;
                pointer.transform.localPosition = new Vector3(x, y, 0);

                Vector3 targ = new Vector3(0, -150, 0);
                Vector3 objectPos = pointer.transform.position;
                targ.x -= objectPos.x;
                targ.y -= objectPos.y;
                float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
                pointer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                targetLine.GetComponent<LineRenderer>().SetPosition(1, new Vector3(x, y, -1));

                confirmation.SetActive(true);
            }
            else
            {
                targetLine.SetActive(false);
                confirmation.SetActive(false);
            }

            attackMessage.SetActive(canAttack);
        }
        else
        {
            //Escape cancels selected target
            if (Input.GetKeyDown(KeyCode.Escape) && canAttack)
            {
                Cancel();
            }
        }
    }

    public void SelectTarget(Sprite image, string playerName, int favor, int health, string attack, int defense, GameObject t)
    {
        target = t;
        if (target.GetComponent<PhotonGameView>())
        {
            if (target.GetComponent<PhotonGameView>().retreated || target.GetComponent<PhotonTargetView>().dead || playManager.IsYou(t))
            {
                target = null;
                return;
            }
            highlightCard.PlayerActivate(image, playerName, favor, health, attack, defense, t, true);
        }
        else
            highlightCard.CombatantActivate(image, health, attack, defense, t, true);
        confirmation.SetActive(true);
    }

    public void Hide()
    {
        advance.SetActive(!advance.activeInHierarchy);
        defend.SetActive(!defend.activeInHierarchy);
        retreat.SetActive(!retreat.activeInHierarchy);
        inputBlocker.SetActive(!inputBlocker.activeInHierarchy);
    }

    public void Advance()
    {
        advance.SetActive(false);
        defend.SetActive(false);
        retreat.SetActive(false);
        inputBlocker.SetActive(false);
        hide.SetActive(false);
        canAttack = true;
        playManager.SetAttackMarkers();
    }

    public void Attack()
    {
        targetLine.SetActive(false);
        canAttack = false;
        playManager.WatchCombat(target);
        confirmation.SetActive(false);
        if (target.GetComponent<PhotonGameView>())
            highlightCard.PlayerActivate(null, null, 0, 0, null, 0, null, false);
        else
            highlightCard.CombatantActivate(null, 0, null, 0, null, false);
        playManager.ClearAttackMarkers();
    }

    public void Cancel()
    {
        confirmation.SetActive(false);
        if (target.GetComponent<PhotonGameView>())
            highlightCard.PlayerActivate(null, null, 0, 0, null, 0, null, false);
        else
            highlightCard.CombatantActivate(null, 0, null, 0, null, false);
        target = null;
    }

    public void Retreat()
    {
        if (!cannotRetreat.activeInHierarchy)
        {
            playManager.Retreat(retreatStreak);
        }
    }

    public void Defend()
    {
        playManager.Defend();
    }
}
