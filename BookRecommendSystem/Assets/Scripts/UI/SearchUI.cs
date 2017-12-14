using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SearchUI : MonoBehaviour {

    [Header("record容器")]
    public Transform recordContainer;
    [Header("record预制体")]
    public GameObject recordPrefab;
    [Header("书名文本")]
    public InputField booknameInput;
    [Header("索书号文本")]
    public InputField ISBNInput;
    [Header("出版城市下拉框")]
    public Dropdown cityDropdown;
    [Header("出版社下拉框")]
    public Dropdown pressDropdown;
    [Header("出版年份文本")]
    public InputField yearInput;
    [Header("模糊查询toggle")]
    public Toggle fuzzySearch;
    [Header("查询按钮")]
    public Button searchBtn;
    [Header("关闭按钮")]
    public Button closeBtn;

    private string pressCity;
    private string pressName;
    private string bookName;
    private string ISBN;
    private string pressYear;

    void Start()
    {
        searchBtn.onClick.AddListener(delegate { OnSearchBtnClick(); });
        closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });
        cityDropdown.onValueChanged.AddListener(delegate { pressCity = cityDropdown.options[cityDropdown.value].text; });
        pressDropdown.onValueChanged.AddListener(delegate { pressName = pressDropdown.options[pressDropdown.value].text; });
        booknameInput.onValueChanged.AddListener(delegate { bookName = booknameInput.text; });
        ISBNInput.onValueChanged.AddListener(delegate { ISBN = ISBNInput.text; });
        yearInput.onValueChanged.AddListener(delegate { pressYear= yearInput.text; });
    
        InitPressDropdown();
    }

    void OnEnable()
    {
        booknameInput.text = "";
        ISBNInput.text = "";
        yearInput.text = "";
        InitPressDropdown();
        // 清空原来的查询结果
        for (int i = 0; i < recordContainer.childCount; i++)
        {
            Destroy(recordContainer.GetChild(i).gameObject);
        } 

    }

    HashSet<string> pressSet = new HashSet<string>();
    HashSet<string> citySet = new HashSet<string>();

    void InitPressDropdown()
    {
        pressDropdown.ClearOptions();
        cityDropdown.ClearOptions();

        DataSet ds = DataBase.Instance.QueryAll(Consts.Press);
        pressSet.Add("");// default
        citySet.Add("");// default
        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        {
            pressSet.Add(ds.Tables[0].Rows[i][1].ToString());
            citySet.Add(ds.Tables[0].Rows[i][2].ToString());
        }

        pressDropdown.AddOptions(pressSet.ToList());
        cityDropdown.AddOptions(citySet.ToList());
    }

    void OnSearchBtnClick()
    {
        // 清空原来的查询结果
        for (int i = 0; i < recordContainer.childCount; i++)
        {
            Destroy(recordContainer.GetChild(i).gameObject);
        } 

        // 数据库查询
        string[] selCols = {"*"};
        string[] tables = {"BookView"};
        string[] cols = { "BookName", "ISBN","PressName","PressCity","PressYear" };
        string[] operations_a = { " = ", " = ", " = ", " = ", " = "};
        string[] operations_f = { " like ", " like ", " like ", " like "," like "};
        string[] values_a = { bookName, ISBN ,pressName,pressCity,pressYear};
        string[] values_f = { "%" + bookName + "%", "%" + ISBN + "%", "%" + pressName + "%", "%" + pressCity + "%", "%" + pressYear + "%"};
        string[] operations;
        string[] values;

        // 开启模糊查询
        if (fuzzySearch.isOn)
        {
            operations = operations_f;
            values = values_f;
        }
        else
        {
            operations = operations_a;
            values = values_a;
        }

        DataSet ds = DataBase.Instance.Query(selCols, tables, cols, operations, values);
        DataTable dt = ds.Tables[0];
        
        // 显示查询结果
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            GameObject record = Instantiate(recordPrefab, recordContainer);
            InputField ISBNInput = record.transform.Find("ISBN").GetComponent<InputField>();
            InputField booknameInput = record.transform.Find("bookname").GetComponent<InputField>();
            InputField pressnameInput = record.transform.Find("pressname").GetComponent<InputField>();
            InputField presscityInput = record.transform.Find("presscity").GetComponent<InputField>();
            InputField pressyearInput = record.transform.Find("pressyear").GetComponent<InputField>();
            ISBNInput.text = dt.Rows[i][0].ToString();
            booknameInput.text = dt.Rows[i][1].ToString();
            pressyearInput.text = dt.Rows[i][2].ToString();
            pressnameInput.text = dt.Rows[i][3].ToString();
            presscityInput.text = dt.Rows[i][4].ToString();
            // 不可编辑
            ISBNInput.enabled = false;
            booknameInput.enabled = false;
            pressnameInput.enabled = false;
            presscityInput.enabled = false;
            pressyearInput.enabled = false;
        }        
    }

    void OnCloseBtnClick()
    {
        this.gameObject.SetActive(false);
    }
	
}
