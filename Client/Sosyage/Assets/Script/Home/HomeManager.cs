using TMPro;
using UnityEngine;

public class HomeManager : MonoBehaviour
{

    [SerializeField]
    private TMP_Text welcomeText;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	async void Start()
    {
        welcomeText.text = $"Welcome, {GameManager.Instance.userData.name}!\r\n";
		var res = await APIRequest.SendRequesr<LoginBonusClaimResponse>("login-bonus/claim", "{\"id\":\"" + GameManager.Instance.userData.id + "\"}");

		welcomeText.text+= res.period.start + "から"+res.period.end + "までログインボーナス！\r\n";
		welcomeText.text += "今日は" + res.currentDay + "日目！\r\n";
		
		foreach(var bonus in res.dailyBonuses)
		{
			foreach(var item in bonus.bonuses)
			{
				welcomeText.text +="["+ item.id + " x" + item.quantity + "]";
			}
			welcomeText.text += "\r\n";
		}

	}

}
