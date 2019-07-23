using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using MNF;

public enum DB_MESSAGE_TYPE
{
	DB_MESSAGE_REGIST,
	DB_MESSAGE_LOGIN,
    DB_MESSAGE_CUSTOM_QUERY,
    DB_MESSAGE_INIT_DB,
	DB_MESSAGE_SELECT,
}

public class DBMessageDispatcher : CustomDispatchHelper<CustomMessage>
{
    Dictionary<DB_MESSAGE_TYPE, MySqlCommand> commands;

    protected override bool OnInit()
	{
		if (ExportFunctionFromEnum<DB_MESSAGE_TYPE>() == false)
			return false;

        if (InitDB() == false)
            return false;

		return true;
	}

    bool InitDB()
    {
		commands = new Dictionary<DB_MESSAGE_TYPE, MySqlCommand>();

        // regist
        var registCmd = DBDirector.Instance.createMySqlCommand(
			"insert into account (id, pwd) value (@id, @pwd)");
		registCmd.Parameters.Add("id", MySqlDbType.VarChar);
		registCmd.Parameters.Add("pwd", MySqlDbType.VarChar);
        commands.Add(DB_MESSAGE_TYPE.DB_MESSAGE_REGIST, registCmd);

        // login
        var loginCmd = DBDirector.Instance.createMySqlCommand(
            "select idx from account where id=@id and pwd=@pwd");
		loginCmd.Parameters.Add("id", MySqlDbType.VarChar);
        loginCmd.Parameters.Add("pwd", MySqlDbType.VarChar);
		commands.Add(DB_MESSAGE_TYPE.DB_MESSAGE_LOGIN,loginCmd);

        // select
        var selectCmd = DBDirector.Instance.createMySqlCommand(
            "select idx from account where id=@id and pwd=@pwd");
        selectCmd.Parameters.Add("id", MySqlDbType.VarChar);
        selectCmd.Parameters.Add("pwd", MySqlDbType.VarChar);
        commands.Add(DB_MESSAGE_TYPE.DB_MESSAGE_SELECT, selectCmd);

		// verify db commands
		var dict = Utility.EnumDictionary<DB_MESSAGE_TYPE>();
		foreach (var enumMessage in dict)
		{
            switch ((DB_MESSAGE_TYPE)enumMessage.Key)
            {
                case DB_MESSAGE_TYPE.DB_MESSAGE_CUSTOM_QUERY:
                case DB_MESSAGE_TYPE.DB_MESSAGE_INIT_DB:
                    continue;
            }
            
            MySqlCommand mysqlCommand;
            if (commands.TryGetValue((DB_MESSAGE_TYPE)enumMessage.Key, out mysqlCommand) == false)
				return false;

            if (mysqlCommand == null)
                return false;
		}

        return true;
    }

	int onDB_MESSAGE_REGIST(CustomMessage dbMessage)
	{
		var message = dbMessage.messageData as DBRegist;
		if (message == null)
			return -1;

        var cmd = commands[DB_MESSAGE_TYPE.DB_MESSAGE_REGIST];
		try
		{
            // db query
            cmd.Parameters[0].Value = message.ID;
            cmd.Parameters[1].Value = message.PWD;

            // db response
            var result = cmd.ExecuteNonQuery();
            if (result == 1)
                message.IsSuccess = true;

            LogManager.Instance.Write("session:{0} regist({1}:{2}) result:{3}", dbMessage.session, message.ID, message.PWD, result);
		}
		catch (Exception e)
		{
            LogManager.Instance.WriteException(e, "Query failed : {0}", cmd.CommandText);
		}

		TcpHelper.Instance.RequestInterMessage((int)INTER_MESSAGE_TYPE.INTER_MESSAGE_REGIST, dbMessage);
	
        return 0;
	}

	int onDB_MESSAGE_LOGIN(CustomMessage dbMessage)
	{
		var message = dbMessage.messageData as DBLogin;
        if (message == null)
			return -1;

		var cmd = commands[DB_MESSAGE_TYPE.DB_MESSAGE_LOGIN];
		try
		{
            // db query
			cmd.Parameters[0].Value = message.ID;
            cmd.Parameters[1].Value = message.PWD;

			// db response
			using (var reader = cmd.ExecuteReader())
			{
                if (reader.Read() == true)
				{
					message.DB_IDX = reader.GetInt32(0);
					message.IsSuccess = true;
				}
			}

            LogManager.Instance.Write("session:{0} login({1}:{2}:{3})", dbMessage.session, message.ID, message.PWD, message.DB_IDX);
		}
		catch (Exception e)
		{
            LogManager.Instance.WriteException(e, "Query failed : {0}", cmd.CommandText);
		}

		TcpHelper.Instance.RequestInterMessage((int)INTER_MESSAGE_TYPE.INTER_MESSAGE_LOGIN, dbMessage);

        return 0;
	}

	int onDB_MESSAGE_CUSTOM_QUERY(CustomMessage dbMessage)
	{
		var message = dbMessage.messageData as DBCustomQuery;
		if (message == null)
			return -1;

        message.RESULT = DBDirector.Instance.Query(message.QUERY).ToString();

		bool requestResult = TcpHelper.Instance.RequestInterMessage(
            (int)INTER_MESSAGE_TYPE.INTER_MESSAGE_CUSTOM_QUERY, dbMessage);

		if (requestResult == false)
			return -3;

		LogManager.Instance.Write("query({0}) success!", message.QUERY);

		return 0;
    }

    int onDB_MESSAGE_INIT_DB(CustomMessage dbMessage)
    {
        string[] initQuery = {
            "DROP DATABASE `mnf_db`",
            "CREATE DATABASE `mnf_db`",
            "USE `mnf_db`",
            "CREATE TABLE `account` (`idx` INT NOT NULL AUTO_INCREMENT, `id` VARCHAR(32) NOT NULL, `pwd` VARCHAR(32), PRIMARY KEY(`idx`, `id`), UNIQUE KEY(`id`))"
        };

        foreach(var query in initQuery)
        {
            LogManager.Instance.Write("Run Query : {0}", query);
            DBDirector.Instance.Query(query);
        }

        LogManager.Instance.Write("init db success!");

        return 0;
    }

    int onDB_MESSAGE_SELECT(CustomMessage dbMessage)
    {
        var message = dbMessage.messageData as DBSelect;
        if (message == null)
            return -1;

        var cmd = commands[DB_MESSAGE_TYPE.DB_MESSAGE_SELECT];
        try
        {
            // db query
            cmd.Parameters[0].Value = message.ID;
            cmd.Parameters[1].Value = message.PWD;

            // db response
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read() == true)
                {
                    message.DB_IDX = reader.GetInt32(0);
                }
            }

            LogManager.Instance.Write("session:{0} login({1}:{2}:{3})", dbMessage.session, message.ID, message.PWD, message.DB_IDX);
        }
        catch (Exception e)
        {
            LogManager.Instance.WriteException(e, "Query failed : {0}", cmd.CommandText);
        }

        TcpHelper.Instance.RequestInterMessage((int)INTER_MESSAGE_TYPE.INTER_MESSAGE_SELECT, dbMessage);

        return 0;
    }
}