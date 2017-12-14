using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        //DataSet ds = DataBase.Instance.QueryAll_Ordered(Consts.BookView,Consts.BookImage);
        DataSet ds = DataBase.Instance.QueryAll(Consts.BookView);
        DataTable dt = ds.Tables[0];
        for (int i = 0; i < recordNum; i++)
        {
            recordList.Add(DataBase.Instance.RowToRecord(dt.Rows[i]));
        }
    }

    void FillData(Transform recordItem,int recordIndex)
    {
        Record record = recordList[recordIndex];
        //Sprite bookImage = Resources.Load<Sprite>("sprite/" + record.bookImage);
        Sprite bookImage = null;

        String path = Application.streamingAssetsPath + "/" + record.bookImage + ".png";
        FileStream fs = File.OpenRead(path); //OpenRead
        int filelength = 0;
        filelength = (int)fs.Length; //获得文件长度 
        Byte[] image = new Byte[filelength]; //建立一个字节数组 
        fs.Read(image, 0, filelength); //按字节流读取      
        System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
        fs.Close();

        MemoryStream ms = new MemoryStream();
        result.Save(ms,System.Drawing.Imaging.ImageFormat.Png);

        Texture2D _tex = new Texture2D(64, 64);
        _tex.LoadImage(ms.ToArray());
        bookImage = Sprite.Create(_tex,new Rect(0,0,250,340),Vector2.zero );
        ms.Close();

        //using (WWW www = new WWW(
        //    "file://"+Application.streamingAssetsPath + "/"+record.bookImage+".png"))
        //{
        //    yield return www;
        //    bookImage = Sprite.Create(www.texture, new Rect(0, 0, 180, 230), Vector2.zero);
        //} 
        //StartCoroutine(LoadImage("file://" + Application.streamingAssetsPath + "/"+record.bookImage+".png", bookImage));
        string pressInfo = record.pressCity + "-" + record.pressName + "，" + record.pressYear;
        
        recordItem.Find("BookImage").GetComponent<Image>().sprite = bookImage;
        recordItem.Find("BookName/Text").GetComponentInChildren<Text>().text = record.bookName;
        recordItem.Find("PressInfo/Text").GetComponent<Text>().text = pressInfo;
        recordItem.Find("ISBN/Text").GetComponent<Text>().text = record.ISBN;
        recordItem.Find("Intro/Viewport/IntroText").GetComponent<Text>().text = record.bookIntro;
        recordItem.Find("Intro").GetComponentInChildren<Scrollbar>().value = 1;
    }

    IEnumerator LoadImage(string url, Sprite image)
    {
        WWW www = new WWW(url);
        //Texture2D tempImage;
        yield return www;
        if (www.isDone && www.error == null)
        {
            image = Sprite.Create(www.texture,new Rect(0,0,180,230),Vector2.zero );
        }
        //if (tempImage != null)
        //{
        //    byte[] data = tempImage.EncodeToPNG();
        //    File.WriteAllBytes(Application.streamingAssetsPath + "/1.png", data);
        //    this.downloadOK = true;
        //}
    }

    void UpdateShow()
    {
        int recordIndex_1 = 2*(page - 1);
        int recordIndex_2 = 2*(page - 1)+1;

        pageText.text = "第 " + page + "/" + maxPageNum + " 页";

        if (recordNum%2 != 0 && page == maxPageNum)
        {
            bookInfo_2.gameObject.SetActive(false);
            FillData(bookInfo_1, recordIndex_1);
            //StartCoroutine(FillData(bookInfo_1,recordIndex_1));
        }
        else
        {
            bookInfo_1.gameObject.SetActive(true);
            bookInfo_2.gameObject.SetActive(true);
            FillData(bookInfo_1, recordIndex_1);
            //StartCoroutine(FillData(bookInfo_1,recordIndex_1));
            FillData(bookInfo_2, recordIndex_2);
            //StartCoroutine(FillData(bookInfo_2,recordIndex_2));
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
