using MNF;

/**
 * @brief ServerSession to communicate with client.
 * @details It can handle events such as client accept, connection termination, etc., and can store the client's state.
 * @ref JsonSession
 */
public class Basic_1_ServerSession : JsonSession
{
	/**
     * @brief Called when Basic_1_ServerSession is successfully accetped to this server.
     * @return Returns 0 if it is normal, otherwise it returns an arbitrary value.
     */
	public override int OnAccept()
	{
		LogManager.Instance.Write("OnAccept : {0}:{1}", this.ToString(), this.GetType());
		return 0;
	}

	/**
     * @brief Called when Basic_1_ServerSession is disconnected from the client.
     * @return Returns 0 if it is normal, otherwise it returns an arbitrary value.
     */
	public override int OnDisconnect()
	{
		LogManager.Instance.Write("OnDisconnect : {0}:{1}", this.ToString(), this.GetType());
		return 0;
	}
}