using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;

public class DataBase : Singleton<DataBase>
{
    const string host = "localhost";
    const string database = "BookRecommend";
    const string id = "root";
    const string pwd = "azazaz";

    MySqlConnection connection;
    MySqlDataAdapter dataAdapter;

    public void Init()
    {
        // Connect
        try
        {
            string constr = String.Format("data Source={0};database={1};user id={2};pwd={3}",host,database,id,pwd);
            connection = new MySqlConnection(constr);
            connection.Open();
        }
        catch (Exception e)
        {
            throw new Exception("服务器连接失败，请重新检查是否打开MySql服务。" + e.Message.ToString()); 
        }
        
    }

    public DataSet Query(string[] selectCols, string[] tables, string[] cols, string[] operations, string[] values)
    {
        string cmd = "select ";
        if (selectCols.Length > 0)
        {
            cmd += selectCols[0];
            for (int i = 1; i < selectCols.Length; ++i)
            {
                cmd += "," + selectCols[i];
            }
            if (tables.Length > 0)
            {
                cmd += " from " + tables[0];
                for (int i = 1; i < tables.Length; i++)
                {
                    cmd += "," + tables[i];
                }
            }
            
            if (cols.Length > 0 && cols.Length == values.Length && cols.Length==operations.Length)
            {
                cmd += " where " + cols[0] + operations[0] + "'"+ values[0] + "'";
                for (int i = 1; i < cols.Length; ++i)
                {
                    cmd += " and " + cols[i] +  operations[i] + "'" + values[i] + "'";
                }
            }
            cmd += ";";
            Debug.Log(cmd);
        }
        else
            return null;

        dataAdapter = new MySqlDataAdapter(cmd, connection);

        //实例化数据集，并写入查询到的数据   
        DataSet ds = new DataSet();
        dataAdapter.Fill(ds);
        return ds;
    }
    
    public DataSet QueryAll(string table)
    {
        string cmd = "select * from " + table + ";";
        dataAdapter = new MySqlDataAdapter(cmd, connection);

        //实例化数据集，并写入查询到的数据   
        DataSet ds = new DataSet();
        dataAdapter.Fill(ds);
        return ds;
    }
    
    public DataSet QueryAll_Ordered(string table,string orderItem)
    {
        string cmd = "select * from " + table +" order by "+ orderItem +";";
        dataAdapter = new MySqlDataAdapter(cmd, connection);

        //实例化数据集，并写入查询到的数据   
        DataSet ds = new DataSet();
        dataAdapter.Fill(ds);
        return ds;
    }

    public int Insert(string table, string[,] values,out int insertId)
    {
        string cmdText = "insert into " + table + " values ";
        if (values.Length > 0)
        {
            for (int i = 0; i < values.GetLength(0); ++i)
            {
                cmdText += "(";
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    cmdText += "'" + values[i,j] + "'";
                    if (values.GetLength(1) > j + 1)
                        cmdText += ",";
                }
                cmdText += ")";
                if (values.GetLength(0) > i + 1)
                    cmdText += ",";
            }
            cmdText += ";";
            Debug.Log(cmdText);
        }
        else
        {
            insertId = 0;
            return 0;
        }
        // 处理table的特殊情况，如press(name,city,year)
        if (table.Contains("("))
        {
            // 提取insert的属性和表名 vals={name,city,year}
            string[] cols = table.Substring(table.IndexOf('(') + 1, table.IndexOf(')') - table.IndexOf('(') - 1).Split(',');
            table = table.Substring(0, table.IndexOf('('));
            // 校验是否已存在相同内容的记录
            string[] selectCols = {"*"};
            string[] tables = {Consts.Press};
            string[] ops = new string[cols.Length];
            string[] vals=new string[values.GetLength(1)];
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    vals[j] = values[i, j];
                    ops[j] = "=";
                }
                DataSet ds = Query(selectCols, tables, cols, ops, vals);
                if (ds.Tables[0].Rows.Count > 0)// 已存在
                {
                    Debug.Log("已存在相同记录");
                    insertId=int.Parse(ds.Tables[0].Rows[0][0].ToString());
                    return 0;
                }
            }           
        }
 
        MySqlCommand cmd=new MySqlCommand(cmdText,connection);
        int resLine = cmd.ExecuteNonQuery();// 返回受影响的行数
        // 返回自增长的id值
        cmd.CommandText = "select last_insert_id() from "+table+" limit 1";
        MySqlDataReader reader = cmd.ExecuteReader();
        reader.Read();
        insertId=reader.GetInt16(0);
        reader.Close();

        return resLine;
    }
    
    public int Update(string table, string[] cols,string[] values,string[] whereCols=null,string[] whereValues=null)
    {
        string cmdText = "update " + table + " set ";
        if (cols.Length > 0 && values.Length == cols.Length)
        {
            for (int i = 0; i < cols.Length; ++i)
            {
                cmdText += cols[i] + "=" + "'" + values[i] + "'";
                if (values.Length > i + 1)
                    cmdText += ",";
            }
            if (whereCols!=null && whereValues!=null && whereCols.Length > 0 && whereCols.Length == whereValues.Length)
            {
                cmdText += " where ";
                for (int i = 0; i < whereCols.Length; ++i)
                {
                    cmdText += whereCols[i] + "=" + "'" + whereValues[i] + "'";
                    if (whereCols.Length > i + 1)
                        cmdText += " and ";
                }
            }           
            cmdText += ";";
            Debug.Log(cmdText);
        }
        else
        {
            return 0;
        }

        MySqlCommand cmd = new MySqlCommand(cmdText, connection);
        int resLine = cmd.ExecuteNonQuery();// 返回受影响的行数
        //int resLine = 1;
        return resLine;
    }

    public int Delete(string table, string[] cols, string[] values)
    {
        string cmdText = "delete from " + table;
        if (cols.Length > 0 && cols.Length == values.Length)
        {
            cmdText += " where " + cols[0] + "=" + "'" + values[0] + "'";
            for (int i = 1; i < cols.Length; ++i)
            {
                cmdText += " and " + cols[i] + "=" + "'" + values[i] + "'";
            }
        }
        else
        {
            return 0;
        }
        cmdText += ";";
        Debug.Log(cmdText);     

        MySqlCommand cmd = new MySqlCommand(cmdText, connection);
        int resLine = cmd.ExecuteNonQuery();// 返回受影响的行数
        return resLine;
    }

    public int CalTotalRecordNum(string table)
    {
        string cmd = "select count(*) from " + table + ";";
        dataAdapter = new MySqlDataAdapter(cmd, connection);

        //实例化数据集，并写入查询到的数据   
        DataSet ds = new DataSet();
        dataAdapter.Fill(ds);
        return int.Parse(ds.Tables[0].Rows[0][0].ToString());
    }

    public Record RowToRecord(DataRow row)
    {
        Record record = new Record();
        record.ISBN = row[0].ToString();
        record.bookName = row[1].ToString();
        record.pressName = row[2].ToString();
        record.pressCity = row[3].ToString();
        record.pressYear = row[4].ToString();
        record.bookIntro = row[5].ToString();
        record.bookImage = int.Parse(row[6].ToString());
        return record;
    }

    public void Close()
    {
        if (connection != null)
        {
            connection.Clone();
            connection.Dispose();
            connection = null;
        }
    }

}
