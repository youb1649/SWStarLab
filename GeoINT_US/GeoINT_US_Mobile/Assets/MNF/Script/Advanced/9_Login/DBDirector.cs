using System;
using System.IO;
using MySql.Data.MySqlClient;
using MNF;

class DBConnectionInfo
{
	public string server;
	public string database;
	public string uid;
	public string pwd;

	public override string ToString()
	{
		return string.Format(
			"server={0};database={1};uid={2};pwd={3};Allow User Variables=True",
			server, database, uid, pwd);
	}
}

class DBDirector : Singleton<DBDirector>
{
    string dbConnectionInfo = UnityUtility.GetDataPath("DBInfo.inf");
	public DBConnectionInfo DBConnInfo { get; set; }
	public string ErrorString { get; set; }
	public MySqlConnection Connection {	get; private set; }

	#region DBConnectionInfo
	public bool readDBConnInfo()
	{
		try
		{
			DBConnInfo = new DBConnectionInfo();
			using (StreamReader sr = new StreamReader(dbConnectionInfo))
			{
				DBConnInfo.server = sr.ReadLine();
				DBConnInfo.database = sr.ReadLine();
				DBConnInfo.uid = sr.ReadLine();
				DBConnInfo.pwd = sr.ReadLine();
			}
			return true;
		}
		catch (Exception e)
		{
			LogManager.Instance.WriteException(e, "DB Connection info read failed");
			return false;
		}
	}

	public bool writeDBConnInfo()
	{
		try
		{
			// save option string
			using (StreamWriter sw = new StreamWriter(dbConnectionInfo))
			{
				sw.WriteLine(DBConnInfo.server);
				sw.WriteLine(DBConnInfo.database);
				sw.WriteLine(DBConnInfo.uid);
				sw.WriteLine(DBConnInfo.pwd);
			}
			return true;
		}
		catch (Exception e)
		{
			LogManager.Instance.WriteException(e, "DB Connection info write failed");
			return false;
		}
	}
	#endregion DBConnectionInfo

	#region DBSetting
	public bool connectToDB()
	{
		if (DBConnInfo == null)
		{
			if (readDBConnInfo() == false)
				return false;
		}

		try
		{
			Connection = new MySqlConnection();
			Connection.ConnectionString = DBConnInfo.ToString();
			Connection.Open();

			writeDBConnInfo();
		}
		catch (MySqlException e)
		{
			switch (e.Number)
			{
				case 0:
					LogManager.Instance.WriteException(
                        e, "Cannot connect to server. Contact administrator : {0}, {1}", DBConnInfo.ToString(), e.Number);
					break;

				case 1045:
					LogManager.Instance.WriteException(
						e, "Invalid username/password, please try again : {0}, {1}", DBConnInfo.ToString(), e.Number);
					break;

				default:
					LogManager.Instance.WriteException(
						e, "Cannot connect to server : {0}, {1}", DBConnInfo.ToString(), e.Number);
					break;
			}
			return false;
		}
		return true;
	}

	public MySqlCommand createMySqlCommand(string query)
	{
        
		try
		{
            var cmd = new MySqlCommand(query, Connection);
		    cmd.Prepare();
			return cmd;
		}
		catch (Exception e)
		{
            LogManager.Instance.WriteException(e, "DB mysql command create failed : {0}", query);
			return null;
		}
	}
	#endregion DBSetting

    public object Query(string query)
    {
		try
		{
            MySqlCommand cmd = new MySqlCommand(query, Connection);
            return cmd.ExecuteScalar();
		}
		catch (Exception e)
		{
            LogManager.Instance.WriteException(e, "DB mysql query failed : {0}", query);
            return null;
		}
    }
}