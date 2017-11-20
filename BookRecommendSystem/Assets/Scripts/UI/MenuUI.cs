using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("各种Button")]
    public Button bookInfoBtn;
    public Button searchBtn;
    public Button authorInfoBtn;
    public Button returnBtn;
    public Button exitBtn;

    [Header("各种Panel")]
    public GameObject bookInfoPanel;
    public GameObject searchPanel;
    public GameObject authorInfoPanel;

    void Start()
    {
        searchBtn.onClick.AddListener(delegate { searchPanel.SetActive(true); });
        bookInfoBtn.onClick.AddListener(delegate { bookInfoPanel.SetActive(true); });
        authorInfoBtn.onClick.AddListener(delegate { authorInfoPanel.SetActive(true); });
        returnBtn.onClick.AddListener(delegate { SceneManager.LoadScene(0); });
        exitBtn.onClick.AddListener(delegate { Application.Quit(); });
    }


}
