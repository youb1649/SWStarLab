using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MNF;

public class LoginServerButtonTrigger : MonoBehaviour
{
	public InputField serverIP;
	public InputField serverPort;
	public InputField querySend;
	public InputField queryResult;

	public InputField db_server;
	public InputField db_database;
	public InputField db_uid;
	public InputField db_pwd;

    public void Awake()
    {
		LogManager.Instance.SetLogWriter(new UnityLogWriter());
		if (LogManager.Instance.Init() == false)
			Debug.Log("LogWriter init failed");

        if (DBDirector.Instance.readDBConnInfo() == true)
        {
			db_server.text = DBDirector.Instance.DBConnInfo.server;
			db_database.text = DBDirector.Instance.DBConnInfo.database;
			db_uid.text = DBDirector.Instance.DBConnInfo.uid;
			db_pwd.text = DBDirector.Instance.DBConnInfo.pwd;
        }
	}

	void Release()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
		IPPulse.Instance.Stop();
		TcpHelper.Instance.StopAccept();
		TcpHelper.Instance.Stop();
		LogManager.Instance.Release();
	}

	void OnDestroy()
	{
		Release();
	}

	void OnApplicationQuit()
	{
		Release();
	}

	void Update()
	{
		TcpHelper.Instance.DipatchNetworkInterMessage();
	}

    public void StartServer()
    {
        if (TcpHelper.Instance.Start(false) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.run() failed");
			return;
		}

		TcpHelper.Instance.RegistDBMsgDispatcher<DBMessageDispatcher>();
		TcpHelper.Instance.RegistInterMsgDispatcher<InterMessageDispatcher>();

        if (TcpHelper.Instance.StartAccept<LoginServerSession, LoginServerMessageDispatcher>(serverPort.text, 500) == false)
		{
			LogManager.Instance.WriteError("TcpHelper.Instance.StartAccept<LoginServerSession, LoginServerMessageDispatcher>() failed");
			return;
		}
		LogManager.Instance.Write("Start Server Success");
	}

    public void OnDBConnectionTest()
    {
        DBDirector.Instance.DBConnInfo.server = db_server.text;
        DBDirector.Instance.DBConnInfo.database = db_database.text;
        DBDirector.Instance.DBConnInfo.uid = db_uid.text;
        DBDirector.Instance.DBConnInfo.pwd = db_pwd.text;

        if (DBDirector.Instance.connectToDB() == true)
        {
			queryResult.text = "Connect Success!";
			DBDirector.Instance.writeDBConnInfo();

            List<string> getPrivateIPList;
            List<string> getPrivateIPDescList;
            Utility.GetPrivateIPList(out getPrivateIPList, out getPrivateIPDescList);
            serverIP.text = string.Join(",", getPrivateIPList);

            foreach (var ip in getPrivateIPDescList)
                LogManager.Instance.Write(ip);

            if (IPPulse.Instance.Start(getPrivateIPList, serverPort.text, true) == false)
            {
                LogManager.Instance.WriteError("IPPulse start failed");
                return;
            }
            IPPulse.Instance.Pause();

            StartServer();
        }
        else
        {
			queryResult.text = "Connect Failed!";
		}
	}

	public void OnSendQuery()
	{
        var dbSelect = new DBSelect();
        dbSelect.ID = "2";
        dbSelect.PWD = "2";

        var dbMessage = new CustomMessage();
        dbMessage.session = new LoginServerSession();
        dbMessage.messageData = dbSelect;

        TcpHelper.Instance.RequestDBMessage(
            (int)DB_MESSAGE_TYPE.DB_MESSAGE_SELECT, dbMessage);
    }

    public void OnSelectQuery()
    {
        var dbCustomQuery = new DBCustomQuery();
        dbCustomQuery.QUERY = querySend.text;
        dbCustomQuery.RESULT = "";

        var dbMessage = new CustomMessage();
        dbMessage.messageData = dbCustomQuery;

        TcpHelper.Instance.RequestDBMessage(
            (int)DB_MESSAGE_TYPE.DB_MESSAGE_CUSTOM_QUERY, dbMessage);

        LogManager.Instance.Write("Request Query : {0}", querySend.text);
    }

    public void OnTestQuery()
    {
        var session = new LoginServerSession();
        {
			var dbRegist = new DBRegist();
			dbRegist.ID = "2";
			dbRegist.PWD = "2";

			var dbMessage = new CustomMessage();
			dbMessage.session = session;
			dbMessage.messageData = dbRegist;

			TcpHelper.Instance.RequestDBMessage(
				(int)DB_MESSAGE_TYPE.DB_MESSAGE_REGIST, dbMessage);
		}

        {
			var dbLogin = new DBLogin();
			dbLogin.ID = "2";
			dbLogin.PWD = "2";

			var dbMessage = new CustomMessage();
			dbMessage.session = session;
			dbMessage.messageData = dbLogin;

			TcpHelper.Instance.RequestDBMessage(
				(int)DB_MESSAGE_TYPE.DB_MESSAGE_LOGIN, dbMessage);    
        }
    }

    public void OnInitDB()
    {
        TcpHelper.Instance.RequestDBMessage(
            (int)DB_MESSAGE_TYPE.DB_MESSAGE_INIT_DB, new CustomMessage());
    }
}
