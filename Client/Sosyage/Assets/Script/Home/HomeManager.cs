using TMPro;
using UnityEngine;

public class HomeManager : MonoBehaviour
{

    [SerializeField]
    private TMP_Text welcomeText;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        welcomeText.text = $"Welcome, {GameManager.Instance.userData.name}!";
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
