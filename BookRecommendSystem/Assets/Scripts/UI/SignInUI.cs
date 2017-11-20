using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class SignInUI : MonoBehaviour {

    [Header("登录按钮")]
    public Button signInBtn;
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

    void OnCloseBtnClick()
    {
        Application.Quit();
    }
}
