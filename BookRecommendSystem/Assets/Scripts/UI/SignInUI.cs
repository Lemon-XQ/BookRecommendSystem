using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class SignInUI : MonoBehaviour {

    [Header("登录按钮")]
    public Button signInBtn;
	[Header("注册按钮")]
	public Button registerBtn;
    [Header("关闭按钮")] 
    public Button closeBtn;
    [Header("用户名文本")]
    public Text userName;
    [Header("密码文本")]
    public InputField password;
    [Header("错误提示")] 
    public Text errorText;
    [Header("管理员主面板")]
    public GameObject adminPanel;
    [Header("用户主面板")]
    public GameObject userPanel;

	void Start ()
	{
	    signInBtn.onClick.AddListener(delegate { OnSignInBtnClick(); });
		registerBtn.onClick.AddListener(delegate { OnRegisterBtnClick(); });
	    closeBtn.onClick.AddListener(delegate { OnCloseBtnClick(); });
	}

    void OnSignInBtnClick()
    {
        string[] selCols = {"*"};
        string[] tables = {"user"};
        string[] cols = {"username","password"};
        string[] operations = {"=","="};
        string[] values = {userName.text,password.text};

        DataSet ds = DataBase.Instance.Query(selCols, tables, cols, operations, values);
        DataTable dt = ds.Tables[0];
        if (ds == null || dt.Rows.Count==0)
        {
            errorText.enabled = true;
            errorText.text = "用户不存在或密码错误！";
        }
        else
        {
            errorText.enabled = false;
            User user = new User(dt.Rows[0][0].ToString(),dt.Rows[0][1].ToString(),dt.Rows[0][2].ToString().ToString());
            SystemManager.Instance.user = user;
            if(user.isAdmin=="1")
                adminPanel.SetActive(true);
            else
                userPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

	void OnRegisterBtnClick()
	{
		string username = userName.text;
		string pwd = password.text;
		// 检测输入是否合理
		if (username == null || pwd == null || username == "" || pwd == "") {
			errorText.enabled = true;
			errorText.text = "";
			if (username == "")
				errorText.text = "用户名 ";
			if (pwd == "")
				errorText.text += "密码 ";
			errorText.text += "不能为空！";
		} else {
			string[] selCols = {"*"};
			string[] tables = {"user"};
			string[] cols = {"username","password"};
			string[] operations = {"=","="};
			string[] values = {username, pwd};

			DataSet ds = DataBase.Instance.Query(selCols, tables, cols, operations, values);
			DataTable dt = ds.Tables[0];
			if (dt.Rows.Count != 0)
			{
				errorText.enabled = true;
				errorText.text = "该用户已存在！";
			}
			else
			{
				errorText.enabled = false;
				// 数据库插入
				int id;
				string[,] values_user = { { username, pwd, "0" } };
				DataBase.Instance.Insert(Consts.User, values_user, out id);
				errorText.enabled = true;
				errorText.text = "注册成功！";
			}
		}

	}

    void OnCloseBtnClick()
    {
        Application.Quit();
    }
}
