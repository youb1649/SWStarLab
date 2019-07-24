using MNF;

/**
 * @brief Client to communicate with server.
 * @details
 * It can handle events such as client connection, connection failure, connection termination, etc., and can store the client's state.
 * @ref JsonSession
 */
public class Basic_1_ClientSession : JsonSession
{
	/**
     * @brief Called when Basic_1_ClientSession is successfully connected to the server.
     * @return Returns 0 if it is normal, otherwise it returns an arbitrary value.
     */
	public override int OnConnectSuccess()
	{
		LogManager.Instance.Write("onConnectSuccess : {0}:{1}", ToString(), GetType());

		// Add connected client.
		TcpHelper.Instance.AddClientSession(GetHashCode(), this);
		return 0;
	}

	/**
     * @brief Called when Basic_1_ClientSession fails to connect to the server.
     * @return Returns 0 if it is normal, otherwise it returns an arbitrary value.
     */
	public override int OnConnectFail()
	{
		LogManager.Instance.Write("onConnectFail : {0}:{1}", ToString(), GetType());
		return 0;
	}

	/**
     * @brief Called when Basic_1_ClientSession is disconnected from the server.
     * @return Returns 0 if it is normal, otherwise it returns an arbitrary value.
     */
	public override int OnDisconnect()
	{
		LogManager.Instance.Write("onDisconnect : {0}:{1}", ToString(), GetType());

		// Remove disconnected client.
		TcpHelper.Instance.RemoveClientSession(GetHashCode());
		return 0;
	}
}