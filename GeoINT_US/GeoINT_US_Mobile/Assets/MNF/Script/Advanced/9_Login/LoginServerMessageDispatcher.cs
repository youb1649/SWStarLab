using MNF_Common;
using MNF;

public class LoginServerMessageDispatcher : DefaultDispatchHelper<LoginServerSession, LoginMessageDefine, LoginMessageDefine.ENUM_CS_>
{
	int onCS_REGIST(LoginServerSession session, object message)
	{
        var regist = (LoginMessageDefine.PACK_CS_REGIST)message;

        var dbRegist = new DBRegist();
		dbRegist.ID = regist.id;
        dbRegist.PWD = regist.pwd;

		var dbMessage = new CustomMessage();
		dbMessage.session = session;
		dbMessage.messageData = dbRegist;

		TcpHelper.Instance.RequestDBMessage(
            (int)DB_MESSAGE_TYPE.DB_MESSAGE_REGIST, dbMessage);
        
		return 0;
	}

	int onCS_LOGIN(LoginServerSession session, object message)
	{
        var login = (LoginMessageDefine.PACK_CS_LOGIN)message;

		var dbLogin = new DBLogin();
		dbLogin.ID = login.id;
		dbLogin.PWD = login.pwd;

		var dbMessage = new CustomMessage();
		dbMessage.session = session;
		dbMessage.messageData = dbLogin;

		TcpHelper.Instance.RequestDBMessage(
			(int)DB_MESSAGE_TYPE.DB_MESSAGE_LOGIN, dbMessage);
        
		return 0;
	}
}