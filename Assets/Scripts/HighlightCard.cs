using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HighlightCard : MonoBehaviour
{
    public GameObject highlightCard;
    public GameObject highlightEventCard;
    public Text[] informations;
    public GameObject target;
    public TurnOptions turnOptions;
    public GameObject championHealth;
    public GameObject championAttack;
    public GameObject championDefense;
    public GameObject combatantHealth;
    public GameObject combatantAttack;
    public GameObject combatantDefense;
    public GameObject defenseBorder;

    private void OnEnable()
    {
        highlightCard.SetActive(false);
        highlightEventCard.SetActive(false);
    }

    private void OnDisable()
    {
        highlightCard.SetActive(false);
        highlightEventCard.SetActive(false);
    }

    public void PlayerActivate(Sprite image, string playerName, int favor, int health, string attack, int defense, GameObject t, bool active)
    {
        if (!turnOptions.confirmation.activeInHierarchy || turnOptions.DRAG_ATTACK)
        {
            if (turnOptions.attacking && turnOptions.DRAG_ATTACK)
                active = false;
            highlightCard.GetComponent<Image>().sprite = image;
            if (health > 0)
            {
                championHealth.GetComponent<Text>().text = health + "";
                if (t.GetComponent<PhotonGameView>().defended)
                {
                    attack = (int.Parse(attack.Substring(0, 1)) - 1) + "-" + (int.Parse(attack.Substring(2, 1)) - 1);
                    defense++;

                    championAttack.GetComponent<Text>().text = "<color=red>" + int.Parse(attack.Substring(0, 1)) + "</color>-<color=red>" + int.Parse(attack.Substring(2, 1)) + "</color>";
                    championDefense.GetComponent<Text>().text = "<color=#00FF00FF>" + defense + "</color>";
                }
                else
                {
                    championAttack.GetComponent<Text>().text = attack;
                    championDefense.GetComponent<Text>().text = defense + "";
                }
            }
            else
            {
                championHealth.GetComponent<Text>().text = "";
                championAttack.GetComponent<Text>().text = "";
                championDefense.GetComponent<Text>().text = "";
            }

            informations[0].text = " -<color=#00FF00FF>" + playerName + "</color>";
            informations[1].text = " -Favor: <color=yellow>" + favor + "</color>";
            informations[2].text = " -Health: <color=red>" + health + "</color>";
            informations[3].text = " -Attack: <color=#FF9B00FF>" + attack + "</color>";
            informations[4].text = " -Defense: <color=#0000FFFF>" + defense + "</color>";
            defenseBorder.SetActive(t && t.GetComponent<PhotonGameView>().defended);
            highlightCard.SetActive(active);
            if (t && (t.GetComponent<PhotonGameView>().retreated || t.GetComponent<PhotonTargetView>().dead))
                target = null;
            else
                target = t;
        }
    }

    public void CombatantActivate(Sprite image, int health, string attack, int defense, GameObject t, bool active)
    {
        if (!turnOptions.confirmation.activeInHierarchy || turnOptions.DRAG_ATTACK)
        {
            if (turnOptions.attacking && turnOptions.DRAG_ATTACK)
                active = false;
            highlightCard.GetComponent<Image>().sprite = image;
            if (health > 0)
            {
                combatantHealth.GetComponent<Text>().text = health + "";
                combatantAttack.GetComponent<Text>().text = attack;
                combatantDefense.GetComponent<Text>().text = defense + "";
            }
            else
            {
                combatantHealth.GetComponent<Text>().text = "";
                combatantAttack.GetComponent<Text>().text = "";
                combatantDefense.GetComponent<Text>().text = "";
            }

            informations[0].text = " -Health: <color=red>" + health + "</color>";
            informations[1].text = " -Attack: <color=#FF9B00FF>" + attack + "</color>";
            informations[2].text = " -Defense: <color=#0000FFFF>" + defense + "</color>";
            informations[3].text = "";
            informations[4].text = "";
            defenseBorder.SetActive(false);
            highlightCard.SetActive(active);
            target = t;
        }
    }

    public void EventActivate(string eventCard, bool active)
    {
        if (eventCard == "none")
            active = false;
        else
            highlightEventCard.GetComponent<Image>().sprite = Resources.Load<Sprite>("Events/" + eventCard);
        highlightEventCard.SetActive(active);
    }
}
