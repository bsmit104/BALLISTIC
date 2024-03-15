using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerListElement : MonoBehaviour
{
    private PlayerRef player;

    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Color deadColor;

    public void SetPlayer(PlayerRef player)
    {
        this.player = player;
        playerIcon.sprite = NetworkPlayerManager.Instance.GetColor(player).icon;
        playerIcon.color = NetworkPlayerManager.Instance.GetColor(player).color;
        nameText.text = NetworkPlayerManager.Instance.GetColor(player).colorName;
    }

    public void PlayerDied()
    {
        nameText.color = deadColor;
    }

    public void PlayerAlive()
    {
        nameText.color = Color.white;
    }
}
