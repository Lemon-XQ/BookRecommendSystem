using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemManager : UnitySingleton<SystemManager>
{
    [HideInInspector]
    public User user;

	void Start () {
		DataBase.Instance.Init();
	}
	

	
}
