using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminUI : MonoBehaviour
{
    [Header("各种Button")]
    public Button searchBtn;
    public Button insertBtn;
    public Button modifyBtn;
    public Button returnBtn;
    public Button exitBtn;

    [Header("各种Panel")] 
    public GameObject searchPanel;
    public GameObject insertPanel;
    public GameObject modifyPanel;

	void Start () {
		searchBtn.onClick.AddListener(delegate { searchPanel.SetActive(true);});
		insertBtn.onClick.AddListener(delegate { insertPanel.SetActive(true);});
		modifyBtn.onClick.AddListener(delegate { modifyPanel.SetActive(true);});
        returnBtn.onClick.AddListener(delegate {SceneManager.LoadScene(0);});
        exitBtn.onClick.AddListener(delegate {Application.Quit();});
	}
	
}
