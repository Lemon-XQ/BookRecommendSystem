using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour {

    [Header("下一页按钮")]
    public Button nextBtn;
    [Header("上一页按钮")]
    public Button preBtn;
    [Header("关闭按钮")]
    public Button closeBtn;
    [Header("页码文本")]
    public Text pageText;
    [Header("列表项")]
    public Transform bookInfo_1;
    public Transform bookInfo_2;

    private int page = 1;
    private int recordNum;
    private int maxPageNum;
    private List<Record> recordList = new List<Record>(); 
    
	void Start () {
        recordNum = DataBase.Instance.CalTotalRecordNum(Consts.BookView);
        maxPageNum = (int)(recordNum / 2.0+0.5);
        Debug.Log("recordNum:"+recordNum);
        Debug.Log("maxPageNum:"+maxPageNum);

        nextBtn.onClick.AddListener(delegate { OnNextBtnClick(); });
        preBtn.onClick.AddListener(delegate { OnPreBtnClick(); });
        closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });

        InitData();
        UpdateShow();
	}

    void InitData()
    {
        DataSet ds = DataBase.Instance.QueryAll_Ordered(Consts.BookView,Consts.BookImage);
        DataTable dt = ds.Tables[0];
        for (int i = 0; i < recordNum; i++)
        {
            recordList.Add(DataBase.Instance.RowToRecord(dt.Rows[i]));
        }
    }

    void FillData(Transform recordItem,int recordIndex)
    {
        Record record = recordList[recordIndex];
        Sprite bookImage = Resources.Load<Sprite>("sprite/" + record.bookImage);
        string pressInfo = record.pressCity + "-" + record.pressName + "，" + record.pressYear;
        
        recordItem.Find("BookImage").GetComponent<Image>().sprite = bookImage;
        recordItem.Find("BookName/Text").GetComponentInChildren<Text>().text = record.bookName;
        recordItem.Find("PressInfo/Text").GetComponent<Text>().text = pressInfo;
        recordItem.Find("ISBN/Text").GetComponent<Text>().text = record.ISBN;
        recordItem.Find("Intro/Viewport/IntroText").GetComponent<Text>().text = record.bookIntro;
        recordItem.Find("Intro").GetComponentInChildren<Scrollbar>().value = 1;
    }

    void UpdateShow()
    {
        int recordIndex_1 = 2*(page - 1);
        int recordIndex_2 = 2*(page - 1)+1;

        pageText.text = "第 " + page + "/" + maxPageNum + " 页";

        if (recordNum%2 != 0 && page == maxPageNum)
        {
            bookInfo_2.gameObject.SetActive(false);
            FillData(bookInfo_1,recordIndex_1);
        }
        else
        {
            bookInfo_1.gameObject.SetActive(true);
            bookInfo_2.gameObject.SetActive(true);
            FillData(bookInfo_1,recordIndex_1);
            FillData(bookInfo_2,recordIndex_2);
        }
    }

    void OnNextBtnClick()
    {
        page++;
        if (page > maxPageNum)
        {
            page = maxPageNum;
        }
        UpdateShow();
    }

    void OnPreBtnClick()
    {
        page--;
        if (page < 1)
        {
            page = 1;
        }
        UpdateShow();
    }

    void OnCloseBtnClick()
    {
        this.gameObject.SetActive(false);
    }
}
