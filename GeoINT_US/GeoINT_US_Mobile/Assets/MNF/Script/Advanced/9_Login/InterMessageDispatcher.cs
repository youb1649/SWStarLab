using UnityEngine;
using UnityEngine.UI;
using MNF;
using MNF_Common;

public enum INTER_MESSAGE_TYPE
{
	INTER_MESSAGE_REGIST,
	INTER_MESSAGE_LOGIN,
    INTER_MESSAGE_CUSTOM_QUERY,
    INTER_MESSAGE_SELECT,
}

public class InterMessageDispatcher : CustomDispatchHelper<CustomMessage>
{
	protected override bool OnInit()
	{
		if (ExportFunctionFromEnum<INTER_MESSAGE_TYPE>() == false)
			return false;

		return true;
	}

	int onINTER_MESSAGE_REGIST(CustomMessage interMessage)
	{
		var message = interMessage.messageData as DBRegist;
		if (message == null)
			return -1;

        InputField queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
        if (queryResult != null)
        {
            queryResult.text = string.Format("session : {0}, Regist : {1}",
                                             interMessage.session, message.IsSuccess == true ? "true" : "false");
        }

		var regist = new LoginMessageDefine.PACK_SC_REGIST();
        regist.id = message.ID;
        regist.pwd = message.PWD;
        regist.isSuccess = message.IsSuccess;

        var session = (LoginServerSession)interMessage.session;
        session.AsyncSend((int)LoginMessageDefine.ENUM_SC_.SC_REGIST, regist);
        
		return 0;
	}

	int onINTER_MESSAGE_LOGIN(CustomMessage interMessage)
	{
		var message = interMessage.messageData as DBLogin;
		if (message == null)
			return -1;

        InputField queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
        if (queryResult != null)
        {
            queryResult.text = string.Format("session : {0}, Login : {1}",
                                             interMessage.session, message.IsSuccess == true ? "true" : "false");
        }

		var login = new LoginMessageDefine.PACK_SC_LOGIN();
		login.id = message.ID;
		login.pwd = message.PWD;
        login.db_idx = message.DB_IDX;
		login.isSuccess = message.IsSuccess;

		var session = (LoginServerSession)interMessage.session;
		session.AsyncSend((int)LoginMessageDefine.ENUM_SC_.SC_LOGIN, login);
        
		return 0;
	}

	int onINTER_MESSAGE_CUSTOM_QUERY(CustomMessage interMessage)
	{
		var message = interMessage.messageData as DBCustomQuery;
		if (message == null)
			return -1;

        InputField queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
        if (queryResult != null)
            queryResult.text = message.RESULT;

        return 0;
    }

    int onINTER_MESSAGE_SELECT(CustomMessage interMessage)
    {
        var message = interMessage.messageData as DBSelect;
        if (message == null)
            return -1;

        InputField queryResult = GameObject.FindWithTag("Tag_QueryResult").GetComponent<InputField>();
        if (queryResult != null)
        {
            queryResult.text = string.Format("session : {0}, DB_IDX : {1}",
                                             interMessage.session, message.DB_IDX);
        }

        return 0;
    }
}