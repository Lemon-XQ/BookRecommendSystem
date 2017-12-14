using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using System.Linq;

public class UserInsertUI : MonoBehaviour {

	[Header("结果文本")]
	public Text resText;
	[Header("用户名文本")]
	public InputField usernameInput;
	[Header("密码文本")]
	public InputField passwordInput;

	[Header("确认按钮")]
	public Button confirmBtn;
	[Header("关闭按钮")]
	public Button closeBtn;

	private string username = "";
	private string password = "";

	void Start()
	{
		resText.text = "";
		confirmBtn.onClick.AddListener(delegate { OnConfirmBtnClick(); });
		closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });

		usernameInput.onValueChanged.AddListener(delegate { username = usernameInput.text; });
		passwordInput.onValueChanged.AddListener(delegate { password = passwordInput.text; });
	}

	void OnEnable()
	{
		resText.text = "";
		usernameInput.text = "";
		passwordInput.text = "";
	}

	void OnConfirmBtnClick()
	{
		resText.text = "";
		// 检测输入是否合理
		if (username == "" || password == "")
		{
			if (username == "")
				resText.text += "用户名 ";
			if (password == "")
				resText.text += "密码 ";
			resText.text += "不能为空！";
		}
		else
		{
			string[] selectcols = {"username"};
			string[] tables = {Consts.User};
			string[] values = {username};
			string[] operations = {"="};
			DataSet ds = DataBase.Instance.Query(selectcols, tables, selectcols, operations, values);
			if (ds.Tables[0].Rows.Count > 0)
			{
				resText.text = "该用户已存在！";
			}
			else
			{
				// 数据库插入
				int id;
				string[,] values_user = { { username, password, "0" } };
				int res1 = DataBase.Instance.Insert(Consts.User, values_user, out id);
				resText.text = "插入" + res1 + "条记录成功！";
			}
		}
	}

	void OnCloseBtnClick()
	{
		this.gameObject.SetActive(false);
	}


}
