using UnityEngine;
using UnityEngine.UI;
using MNF_Common;
using MNF;

public class LoginClientMessageDispatcher : DefaultDispatchHelper<LoginClientSession, LoginMessageDefine, LoginMessageDefine.ENUM_SC_>
{
	int onSC_REGIST(LoginClientSession session, object message)
	{
		var regist = (LoginMessageDefine.PACK_SC_REGIST)message;
        LogManager.Instance.Write("Regist Respone : {0}:{1}:{2}", regist.id, regist.pwd, regist.isSuccess);

		var queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
        if (queryResult != null)
            queryResult.text = string.Format("Regist Respone : {0}:{1}:{2}", regist.id, regist.pwd, regist.isSuccess);
        
		return 0;
	}

	int onSC_LOGIN(LoginClientSession session, object message)
	{
		var login = (LoginMessageDefine.PACK_SC_LOGIN)message;
		LogManager.Instance.Write("Login Respone : {0}:{1}:{2}:{3}", login.id, login.pwd, login.isSuccess, login.db_idx);

		var queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
		if (queryResult != null)
			queryResult.text = string.Format("Login Respone : {0}:{1}:{2}:{3}", login.id, login.pwd, login.isSuccess, login.db_idx);

        return 0;
	}
}