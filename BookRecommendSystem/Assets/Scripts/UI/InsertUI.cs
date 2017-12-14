using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InsertUI : MonoBehaviour {

    [Header("结果文本")]
    public Text resText;
    [Header("书名文本")]
    public InputField booknameInput;
    [Header("索书号文本")]
    public InputField ISBNInput;
    [Header("出版城市下拉框")]
    public Dropdown cityDropdown;
    [Header("出版城市文本")]
    public InputField cityInput;
    [Header("出版社下拉框")]
    public Dropdown pressDropdown;
    [Header("出版社文本")]
    public InputField pressInput;
    [Header("出版年份文本")]
    public InputField yearInput;
    [Header("书目简介文本")]
    public InputField introInput;
    [Header("封面图id文本")]
    public InputField imageInput;
    [Header("确认按钮")]
    public Button confirmBtn;
    [Header("关闭按钮")]
    public Button closeBtn;

    private string pressCity = "";
    private string pressName = "";
    private string bookName = "";
    private string ISBN = "";
    private string pressYear="";
    private string bookIntro = "";
    private string imageId = "";

    void Start()
    {
        resText.text = "";
        confirmBtn.onClick.AddListener(delegate { OnConfirmBtnClick(); });
        closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });

        cityDropdown.onValueChanged.AddListener(delegate { OnCityValueChanged(0); });
        cityInput.onValueChanged.AddListener(delegate { OnCityValueChanged(1); });

        pressDropdown.onValueChanged.AddListener(delegate { OnPressValueChanged(0); });
        pressInput.onValueChanged.AddListener(delegate {OnPressValueChanged(1);});

        booknameInput.onValueChanged.AddListener(delegate { bookName = booknameInput.text; });
        ISBNInput.onValueChanged.AddListener(delegate { ISBN = ISBNInput.text; });
        yearInput.onValueChanged.AddListener(delegate { pressYear = yearInput.text; });
        introInput.onValueChanged.AddListener(delegate { bookIntro = introInput.text; });
        imageInput.onValueChanged.AddListener(delegate { imageId = imageInput.text; });

        InitPressDropdown();
    }

    void OnEnable()
    {
        resText.text = "";
        booknameInput.text = "";
        ISBNInput.text = "";
        yearInput.text = "";
        pressInput.text = "";
        cityInput.text = "";
        imageInput.text = "";
        introInput.text = "";
        InitPressDropdown();
    }

    HashSet<string> pressSet=new HashSet<string>();
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

    void OnConfirmBtnClick()
    {
        resText.text = "";
        // 检测输入是否合理
        if (bookName == "" || ISBN == "" || pressName == "" || pressCity == "" || pressYear == "")
        {
            if (bookName == "")
                resText.text += "书名 ";
            if (ISBN == "")
                resText.text += "索书号 ";
            if (pressName == "" || pressCity == "")
                resText.text += "出版社 ";
            if (pressYear == "")
                resText.text += "出版年份 ";
            resText.text += "不能为空！";
        }
        else
        {
            string[] selectcols = {"ISBN"};
            string[] tables = {Consts.BookView};
            string[] values = {ISBN};
            string[] operations = {"="};
            DataSet ds = DataBase.Instance.Query(selectcols, tables, selectcols, operations, values);
            if (ds.Tables[0].Rows.Count > 0)
            {
                resText.text = "该索书号已存在！";
            }
            else
            {
                // 数据库插入
                int pressId;
                if (imageId == "")
                    imageId = "-1";// 默认封面图
                string[,] values_book = { { ISBN, bookName, bookIntro, "沈从文", imageId, pressYear } };
                string[,] values_press = { { pressName, pressCity } };
                int res1 = DataBase.Instance.Insert("book", values_book, out pressId);
                int res2 = DataBase.Instance.Insert("press(name,city)", values_press, out pressId);
                string[,] values_bp = { { ISBN, pressId.ToString() } };
                int res3 = DataBase.Instance.Insert("bp", values_bp, out pressId);
                if (res1 == 1 && res3 == 1)
                    resText.text = "插入" + res1 + "条记录成功！";
                else
                    resText.text = "插入记录失败（SQL ERROR）";
           }
        }
    }

    void OnCloseBtnClick()
    {
        this.gameObject.SetActive(false);
    }

    void OnCityValueChanged(int type)
    {
        switch (type)
        {
            case 0:// 下拉框
                pressCity = cityDropdown.options[cityDropdown.value].text;
                if (cityDropdown.value != 0)
                {
                    cityInput.text = "";
                    cityDropdown.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    cityInput.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
                break;

            case 1:// 输入框
                pressCity = cityInput.text;
                if (cityInput.text != "")
                {
                    cityDropdown.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    cityInput.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    cityDropdown.value = 0;
                }
                break;
        }
    }

    void OnPressValueChanged(int type)
    {
        switch (type)
        {
            case 0:// 下拉框
                pressName = pressDropdown.options[pressDropdown.value].text;
                if (pressDropdown.value != 0)
                {
                    pressInput.text = "";
                    pressDropdown.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    pressInput.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                }
                break;

            case 1:// 输入框
                pressName = pressInput.text;
                if (pressInput.text != "")
                {
                    pressDropdown.transform.Find("Label").GetComponent<Text>().text = "";
                    pressDropdown.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    pressInput.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    pressDropdown.value = 0;
                }
                break;
        }
    }
}
