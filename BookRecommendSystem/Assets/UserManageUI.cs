using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using System.Linq;

public class UserManageUI : MonoBehaviour {

	[Header("结果文本")]
	public Text resText;
	[Header("record容器")]
	public Transform recordContainer;
	[Header("record预制体")]
	public GameObject recordPrefab;
	[Header("用户名文本")]
	public InputField usernameInput;
	[Header("模糊查询toggle")]
	public Toggle fuzzySearch;
	[Header("查询按钮")]
	public Button searchBtn;
	[Header("关闭按钮")]
	public Button closeBtn;
	[Header("修改按钮")]
	public Button modifyBtn;
	[Header("删除按钮")]
	public Button deleteBtn;

	private string userName;

	private List<int> currentIndexList = new List<int>();

	Dictionary<User, ModifyType> modifyDic = new Dictionary<User, ModifyType>();
	List<Image> maskList = new List<Image>();
	List<User> recordList = new List<User>(); 
	List<GameObject> recordObjs = new List<GameObject>(); 

	void Start()
	{
		searchBtn.onClick.AddListener(delegate { OnSearchBtnClick(); });
		closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });
		modifyBtn.onClick.AddListener(delegate { OnModifyBtnClick(); });
		deleteBtn.onClick.AddListener(delegate { OnDeleteBtnClick(); });
		usernameInput.onValueChanged.AddListener(delegate { userName = usernameInput.text; });
	}

	void OnEnable()
	{
		usernameInput.text = "";

		resText.text = "";
		maskList.Clear();
		currentIndexList.Clear();
		recordList.Clear();
		isMulti = false;
		recordObjs.Clear();

		//InitPressDropdown();
		// 清空原来的查询结果
		for (int i = 0; i < recordContainer.childCount; i++)
		{
			Destroy(recordContainer.GetChild(i).gameObject);
		}
	}
		
	void OnSearchBtnClick()
	{
		// 清空原来的查询结果
		for (int i = 0; i < recordContainer.childCount; i++)
		{
			Destroy(recordContainer.GetChild(i).gameObject);
		}
		resText.text = "";
		maskList.Clear();
		currentIndexList.Clear();
		recordList.Clear();
		isMulti = false;
		recordObjs.Clear();

		// 数据库查询
		string[] selCols = { "*" };
		string[] tables = { "user" };
		string[] cols = { "username" };
		string[] operations_a = { " = " };
		string[] operations_f = { " like " };
		string[] values_a = { userName };
		string[] values_f = { "%" + userName + "%" };
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
			recordObjs.Add(record);
			InputField userNameInput = record.transform.Find("UserName").GetComponent<InputField>();
			InputField passwordInput = record.transform.Find("Password").GetComponent<InputField>();

			userNameInput.text = dt.Rows[i][0].ToString();
			passwordInput.text = dt.Rows[i][1].ToString();

			// 填充遮罩列表（蓝色选择条）
			Image mask = record.GetComponent<Image>();
			maskList.Add(mask);

			User user = new User (userNameInput.text, passwordInput.text);
			recordList.Add(user);

			// 选择按钮及各个文本框绑定事件
			int index = i;
			userNameInput.onValueChanged.AddListener(delegate { OnValueChanged(index, user, ItemType.USERNAME, userNameInput.text); });
			passwordInput.onValueChanged.AddListener(delegate { OnValueChanged(index, user, ItemType.PASSWORD, passwordInput.text); });

			Button chooseBtn = record.GetComponentInChildren<Button>();
			chooseBtn.onClick.AddListener(delegate { OnChooseBtnClick(index); });

		}
	}

	void OnCloseBtnClick()
	{
		this.gameObject.SetActive(false);
	}

	void OnModifyBtnClick()
	{
		int modLine = 0;
		// 未改动USERNAME的项——更新旧记录
		foreach (KeyValuePair<int, User> pair in updateRecordDic)
		{
			// 更新user表
			User record = pair.Value;
			string[] userCols = { "password" };
			string[] userValues = { record.password };
			string[] whereCols = { "username" };
			string[] whereVals = { record.username };
			int res1 = DataBase.Instance.Update(Consts.User, userCols, userValues, whereCols, whereVals);

			modLine += res1;
		}
		Debug.Log("更新 " + modLine + " 条记录成功！");
		// 改动USERNAME的项——插入新记录
		foreach (KeyValuePair<int, User> pair in insertRecordDic)
		{
			User record = pair.Value;
			string[] cols = { "username" };
			string[] vals = { deleteRecordDic[pair.Key].username };
			// delete原有记录
			DataBase.Instance.Delete(Consts.User, cols, vals);
			// insert新记录
			int id;
			string[,] userVals = { { record.username, record.password, "0" } };
			int res1 = DataBase.Instance.Insert(Consts.User, userVals, out id);

			modLine += res1;
		}
		resText.text = "修改 "+ modLine +" 条记录成功！";

		// 清空update、delete、insert集合
		updateRecordDic.Clear();
		deleteRecordDic.Clear();
		insertRecordDic.Clear();
	}

	private bool isMulti = false;
	void OnChooseBtnClick(int index)
	{
		// 非多选时先清空选中的index集合
		if(!currentIndexList.Contains(index) && !isMulti) currentIndexList.Clear();
		currentIndexList.Add(index);
		StartCoroutine("MultiChoose", index);
	}

	IEnumerator MultiChoose(int index)
	{
		while (true)
		{
			if (!Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftControl))
			{
				for (int i = 0; i < maskList.Count; i++)
				{
					isMulti = false;
					if (i != index)
						maskList[i].enabled = false;
					maskList[index].enabled = true;
				}
				yield break;
			}
			else // 长按Ctrl键可多选
			{
				isMulti = true;
				maskList[index].enabled = true;
				yield break;
			}
		}
	}

	Dictionary<int, User> updateRecordDic = new Dictionary<int, User>();// 需要update的record集
	Dictionary<int, User> insertRecordDic = new Dictionary<int, User>(); // 需要insert的record集
	Dictionary<int, User> deleteRecordDic = new Dictionary<int, User>(); // 需要delete的record集

	void OnValueChanged(int index, User record, ItemType type, string value)
	{
		switch (type)
		{
		case ItemType.USERNAME:
			if (!insertRecordDic.ContainsKey(index))
			{
				User originRec = new User(record.username, record.password);
				insertRecordDic.Add(index, record);
				deleteRecordDic.Add(index, originRec);
				Debug.Log("原有记录 " + deleteRecordDic[index].username);
			}
			insertRecordDic[index].username = value;
			Debug.Log("更改后记录：" + deleteRecordDic[index].username);
			// 保证update和insert无重复元素
			if (updateRecordDic.ContainsKey(index))
			{
				updateRecordDic.Remove(index);
			}
			break;

		case ItemType.PASSWORD:
			if (insertRecordDic.ContainsKey(index)) // 修改ISBN意味着该条记录加入插入集
			{
				insertRecordDic[index].password = value;
			}
			else if (!updateRecordDic.ContainsKey(index))
			{
				updateRecordDic.Add(index, record);
			}
			else
			{
				updateRecordDic[index].password = value;
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
				string[] cols = { "username" };
				string[] values = { recordList[index].username };
				res += DataBase.Instance.Delete(Consts.User, cols, values);
				Destroy(recordObjs[index]);
				recordObjs.RemoveAt(index);
			}
			resText.text = "删除 " + res + " 条记录成功！";
			//resText.text += "删除 " + (currentIndexList.Count - res / 2) + " 条记录失败！";
		}
		else
		{
			resText.text = "请先选择一条记录！";
		}
	}
}
