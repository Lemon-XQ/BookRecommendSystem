using System.Collections;
using System.Collections.Generic;
using System.Data;
using DG.Tweening.Plugins;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    ISBN,
    BOOK_NAME,
    PRESS_NAME,
    PRESS_CITY,
    PRESS_YEAR
};
public enum ModifyType
{
    UPDATE,
    INSERT
};

public class ModifyUI : MonoBehaviour {

    [Header("结果文本")]
    public Text resText;
    [Header("确认按钮")]
    public Button confirmBtn;
    [Header("删除按钮")]
    public Button deleteBtn;
    [Header("关闭按钮")]
    public Button closeBtn;
    [Header("record预制体")]
    public GameObject recordPrefab;
    [Header("record容器")]
    public Transform recordContainer;

    private List<int> currentIndexList = new List<int>();

    Dictionary<Record,ModifyType> modifyDic = new Dictionary<Record, ModifyType>(); 
    List<Image> maskList=new List<Image>(); 
    List<Record> recordList=new List<Record>(); 

	void Start () {
        resText.text = "";
        confirmBtn.onClick.AddListener(delegate { OnConfirmBtnClick(); });
        closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });
        deleteBtn.onClick.AddListener(delegate { OnDeleteBtnClick(); });

        ShowAllInfo();
	}

    void ShowAllInfo()
    {
        DataSet ds = DataBase.Instance.QueryAll(Consts.BookView);
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
            Button chooseBtn = record.GetComponentInChildren<Button>();
            Image mask = record.GetComponent<Image>();
            maskList.Add(mask);
            
            ISBNInput.text = dt.Rows[i][0].ToString();
            booknameInput.text = dt.Rows[i][1].ToString();
            pressnameInput.text = dt.Rows[i][2].ToString();
            presscityInput.text = dt.Rows[i][3].ToString();
            pressyearInput.text = dt.Rows[i][4].ToString();

            Record rec = new Record(ISBNInput.text,booknameInput.text,pressnameInput.text,presscityInput.text,pressyearInput.text);
            recordList.Add(rec);
            
            int index = i;
            ISBNInput.onValueChanged.AddListener(delegate { OnValueChanged(index, rec,ItemType.ISBN,ISBNInput.text); });
            booknameInput.onValueChanged.AddListener(delegate { OnValueChanged(index, rec, ItemType.BOOK_NAME,booknameInput.text); });
            pressnameInput.onValueChanged.AddListener(delegate { OnValueChanged(index, rec, ItemType.PRESS_NAME,pressnameInput.text); });
            presscityInput.onValueChanged.AddListener(delegate { OnValueChanged(index,rec, ItemType.PRESS_CITY,presscityInput.text); });
            pressyearInput.onValueChanged.AddListener(delegate { OnValueChanged(index, rec,ItemType.PRESS_YEAR,pressyearInput.text); });
 
            chooseBtn.onClick.AddListener(delegate { OnChooseBtnClick(index); });
        } 
    }

    void OnConfirmBtnClick()
    {
        int modLine = 0;
        foreach (KeyValuePair<int,Record> pair in updateRecordDic)
        {
            Record record = pair.Value;
            string[] bookCols = {"name"};
            string[] bookValues = {record.bookName};
            string[] whereCols = {"ISBN"};
            string[] whereVals = {record.ISBN};
            int res1 = DataBase.Instance.Update(Consts.Book, bookCols, bookValues,whereCols,whereVals);
            
            int id;
            string[,] pressVals = { { record.pressName, record.pressCity, record.pressYear } };
            int res2 = DataBase.Instance.Insert("press(name,city,year)", pressVals, out id);
            
            string[] bpCols = {"PressId"};
            string[] bpVals = {id.ToString() };
            int res3 = DataBase.Instance.Update(Consts.BookPress, bpCols, bpVals,whereCols,whereVals);
            
            modLine += (res1 | res2 | res3 );
        }
        Debug.Log("更新 "+modLine +" 条记录成功！");
        foreach (KeyValuePair<int, Record> pair in insertRecordDic)
        {
            Record record = pair.Value;
            string[] cols = { "ISBN" };
            string[] vals = { deleteRecordDic[pair.Key].ISBN };
            // delete原有记录
            DataBase.Instance.Delete(Consts.Book, cols, vals);
            DataBase.Instance.Delete(Consts.BookPress, cols, vals);
            // insert新记录
            int id;
            string[,] bookVals = { { record.ISBN, record.bookName, "", "沈从文", "" } };
            int res1 = DataBase.Instance.Insert(Consts.Book, bookVals, out id);

            string[,] pressVals = { { record.pressName, record.pressCity, record.pressYear } };
            int res2 = DataBase.Instance.Insert("press(name,city,year)", pressVals, out id);

            string[,] bpVals = { { record.ISBN, id.ToString() } };
            int res3 = DataBase.Instance.Insert(Consts.BookPress, bpVals, out id);

            modLine += (res1 | res2 | res3);
        }
        resText.text = "修改 " + modLine + "条记录成功！";

        // 清空update、delete、insert集合
        updateRecordDic.Clear();
        deleteRecordDic.Clear();
        insertRecordDic.Clear();
    }

    void OnChooseBtnClick(int index)
    {
        currentIndexList.Add(index);
        StartCoroutine("MultiChoose",index);
    }

    IEnumerator MultiChoose(int index)
    {
        while (true)
        {
            if (!Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftControl))
            {
                for (int i = 0; i < maskList.Count; i++)
                {
                    if (i != index)
                        maskList[i].enabled = false;
                    maskList[index].enabled = true; 
                }
                yield break;
            }
            else // 长按Ctrl键可多选
            {
                maskList[index].enabled = true;
                yield break;
            }
        }
    } 

    void OnCloseBtnClick()
    {
        this.gameObject.SetActive(false);
    }

    Dictionary<int,Record> updateRecordDic = new Dictionary<int, Record>();// 需要update的record集
    Dictionary<int, Record> insertRecordDic = new Dictionary<int, Record>(); // 需要insert的record集
    Dictionary<int, Record> deleteRecordDic = new Dictionary<int, Record>(); // 需要delete的record集
    
    void OnValueChanged(int index,Record record,ItemType type,string value)
    {
        switch (type)
        {
            case ItemType.ISBN:
                if (!insertRecordDic.ContainsKey(index))
                {
                    Record originRec = new Record(record.ISBN,record.bookName,record.pressName,record.pressCity,record.pressYear);
                    insertRecordDic.Add(index,record);
                    deleteRecordDic.Add(index,originRec);
                    Debug.Log("原有记录 "+deleteRecordDic[index].ISBN);
                }
                insertRecordDic[index].ISBN = value;
                Debug.Log("更改后记录：" + deleteRecordDic[index].ISBN);
                // 保证update和insert无重复元素
                if (updateRecordDic.ContainsKey(index))
                {
                    updateRecordDic.Remove(index);
                }
                break;

            case ItemType.BOOK_NAME:
                if (insertRecordDic.ContainsKey(index)) // 修改ISBN意味着该条记录加入插入集
                {
                    insertRecordDic[index].bookName = value;
                }
                else if (!updateRecordDic.ContainsKey(index)) 
                {
                    updateRecordDic.Add(index, record);
                }
                else
                {
                    updateRecordDic[index].bookName = value;
                }
                break;
            
            case ItemType.PRESS_CITY:
                if (insertRecordDic.ContainsKey(index)) // 修改ISBN意味着该条记录加入插入集
                {
                    insertRecordDic[index].pressCity = value;
                }
                else if (!updateRecordDic.ContainsKey(index))
                {
                    updateRecordDic.Add(index, record);
                }
                else
                {
                    updateRecordDic[index].pressCity = value;
                }
                break;
            
            case ItemType.PRESS_NAME:
                if (insertRecordDic.ContainsKey(index)) // 修改ISBN意味着该条记录加入插入集
                {
                    insertRecordDic[index].pressName = value;
                }
                else if (!updateRecordDic.ContainsKey(index))
                {
                    updateRecordDic.Add(index, record);
                }
                else
                {
                    updateRecordDic[index].pressName = value;
                }
                break;
            
            case ItemType.PRESS_YEAR:
                if (insertRecordDic.ContainsKey(index)) // 修改ISBN意味着该条记录加入插入集
                {
                    insertRecordDic[index].pressYear = value;
                }
                else if (!updateRecordDic.ContainsKey(index))
                {
                    updateRecordDic.Add(index, record);
                }
                else
                {
                    updateRecordDic[index].pressYear = value;
                }
                break;
        }

    }

    void OnDeleteBtnClick()
    {
        if (currentIndexList.Count > 0)
        {
            int res = 0;
            foreach (int index in currentIndexList)
            {
                string[] cols = {"ISBN"};
                string[] values = {recordList[index].ISBN};
                res += DataBase.Instance.Delete(Consts.Book, cols, values);
                res += DataBase.Instance.Delete(Consts.BookPress, cols, values);
            }           
            resText.text = "删除 " + res/2 + " 条记录成功！";
            resText.text += "删除 " + (currentIndexList.Count-res/2) + " 条记录失败！";
        }
        else
        {
            resText.text = "请先选择一条记录！";
        }
    }
	
}
