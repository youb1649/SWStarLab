using MNF;

public class CustomMessage
{
	public SessionBase session;
	public object messageData;
}

public class DBRegist
{
    // request
	public string ID { get; set; }
	public string PWD { get; set; }

	// response
	public bool IsSuccess { get; set; }
}

public class DBLogin
{
    // request
	public string ID { get; set; }
	public string PWD { get; set; }

	// response
	public bool IsSuccess { get; set; }
	public int DB_IDX { get; set; }
}

public class DBCustomQuery
{
	// request
	public string QUERY { get; set; }

	// response
	public string RESULT { get; set; }
}

public class DBSelect
{
    // request
    public string ID { get; set; }
    public string PWD { get; set; }

    // response
    public int DB_IDX { get; set; }
}