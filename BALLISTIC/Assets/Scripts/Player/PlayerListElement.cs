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
    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private float chatDisplayDuration;

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

    private Coroutine displayChat;
    private Vector2 originalPos;

    void Awake()
    {
        originalPos = playerIcon.GetComponent<RectTransform>().anchoredPosition;
    }

    public void SetChat(string message)
    {
        if (displayChat != null)
        {
            StopCoroutine(displayChat);
        }
        displayChat = StartCoroutine(DisplayChat(message));
    }

    IEnumerator DisplayChat(string message)
    {
        chatText.gameObject.SetActive(true);
        chatText.text = message;
        float timer = chatDisplayDuration;

        var pos = playerIcon.GetComponent<RectTransform>();

        while (timer > 0)
        {
            if (NetworkPlayerManager.Instance.PlayerListDisplayed)
            {
                pos.anchoredPosition = originalPos;
            }
            else
            {
                pos.anchoredPosition = new Vector2(-305, pos.anchoredPosition.y);
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        chatText.gameObject.SetActive(false);
        pos.anchoredPosition = originalPos;
    }
}
