using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger; 
using System.Linq;
using System.Reflection;

// Utility functions for MoonSharp based on the Unity-Lua library.
// (https://github.com/Semaeopus/Unity-Lua)

public static class LuaUtility
{
    /// <summary>
    // Enum table cache.
    /// </summary>
    private static DynValue m_EnumTables = null;

    /// <summary>
    // Log forwarder hook for LogError.
    /// </summary>
    public static System.Action<string> logHandler = ForwardException;

    /// <summary>
    // Simple forward logger. Reassign logHandler to your error handler
    // or leave it as-is to have it throw exceptions on errors.
    /// </summary>
    public static void LogError(string message) => logHandler.Invoke(message);

     
    /// <summary>
    // Default exception thrower.
    /// </summary>
    private static void ForwardException(string message) => throw new System.Exception(message);

    /// <summary>
    /// Adds a global table.  
    /// <param name="tableName"></param>
    /// </summary>
    public static Table AddGlobalTable(this Script script, string tableName)
    {
        Table table = null;
        if (script.SetGlobal(tableName, DynValue.NewTable(script)))
            table = script.GetGlobal(tableName).Table;
        else
            LogError(string.Format("Failed to add global Lua table {0}", tableName));

        return table;
    }

    /// <summary>
    /// Attempts to set a global variable with key and value  
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>true if value was set successfully</returns>

    public static bool SetGlobal(this Script script,string key, object value)
    {
        bool didSet = false;
        try
        {
            script.Globals[key] = value;
            didSet = true;
        }
        catch (InterpreterException ex)
        {
            LogError(string.Format("Lua SetGlobal error: {0}", ex.DecoratedMessage));
        }
        return didSet;
    }

    /// <summary>
    /// Attempts to retrive a value from the Lua globals
    /// </summary>
    /// <param name="key"></param>
    /// <returns>null if failure occurs, else the requested value as a DynValue</returns>
    /// 
    public static DynValue GetGlobal(this Script script, string key)
    {
        DynValue result = DynValue.Nil;
        try
        {
            result = script.Globals.Get(key);
        }
        catch
        {
            LogError(string.Format("Failed to get Lua global: {0}", key));
        }

        return result;
    }

	/// <summary>
	/// Attempts to retrive a value from the Lua globals, allowing the user to pass parent and children names in
	/// </summary>
	/// <returns>The global.</returns>
	/// <param name="keys">Keys.</param>

	public static DynValue GetGlobal(this Script script, params object[] keys)
	{
        DynValue result = DynValue.Nil;
		try
		{
			result = script.Globals.Get(keys);
		}
        catch
        {
            LogError(string.Format("Failed to get Lua global at '{0}'", 
			    string.Join(", ", Array.ConvertAll(keys, input => input.ToString()))));
		}

		return result;
	}

	/// <summary>
	/// Attempts to retrive a table from the Lua globals
	/// </summary>
	/// <returns>The global table.</returns>
	/// <param name="key">Key.</param>
	public static Table GetGlobalTable(this Script script, string key)
	{
		Table result = null;
		DynValue tableDyn = script.GetGlobal (key);
		if (tableDyn != null)
		{
			if(tableDyn.Type == DataType.Table) 
				result = tableDyn.Table; 
			else
                LogError(string.Format("Lua global {0} is not type table, has type {1}", key, tableDyn.Type.ToString()));
		}
		return result;
	}

	/// <summary>
	/// Attempts to retrive a table from the Lua globals, allowing the user to pass parent and children names in
	/// </summary>
	/// <returns>The global table.</returns>
	/// <param name="keys">Key.</param>
	public static Table GetGlobalTable(this Script script, params object[] keys)
	{
		Table result = null;
		DynValue tableDyn = script.GetGlobal(keys);
		if (tableDyn != null)
		{
			if(tableDyn.Type == DataType.Table)
			{
				result = tableDyn.Table;
			}
			else
			{
                LogError(string.Format("Lua global {0} is not type table, has type {1}", keys, tableDyn.Type.ToString()));
			}
		}
		return result;
	}
    /// <summary>
    // Forward globals table for compatibility with Lua-Unity ports.
    /// </summary>
    public static Table GetGlobalsTable(this Script script) => script.Globals;

    /// <summary>
    /// Attempts to run the string passed in.
    /// </summary>
    /// <param name="command"></param>
    /// <returns>Null if an error occured otherwise will return the result of the executed lua code</returns>
    public static DynValue ExecuteString(this Script script, string command)
    {
        DynValue result = DynValue.Nil; 
        try
        {
            result = script.DoString(command);
        }
        catch (InterpreterException ex)
        {
            LogError(string.Format("Lua interpreter error: {0}", ex.DecoratedMessage));
        }
        catch (Exception ex)
        {
            LogError(string.Format("Lua error: {0}", ex.Message));
        }

        return result;
    }

    /// <summary>
    /// Attempts to run the lua script passed in
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Null if an error occured otherwise will return the result of the executed lua code</returns>
    public static DynValue ExecuteScript(this Script script, string filePath)
    {
        DynValue result = DynValue.Nil;

        try
        {
            result = script.DoFile(filePath);
        }
        catch (InterpreterException ex)
        {
            LogError(string.Format("Lua ExecuteScript error: {0}", ex.DecoratedMessage));
        }
        catch (Exception ex)
        {
            LogError(string.Format("System ExecuteScript error: {0}", ex.Message));
        }

        return result;
    }

    /// <summary>
    /// Attempts to load a lua script
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Null if an error occured otherwise will return the DynValue of the script. This can be passed to Call()</returns>
    public static DynValue LoadScript(this Script script, string filePath)
    {
        DynValue result = DynValue.Nil;

        try
        {
            result = script.LoadFile(filePath);
        }
        catch (InterpreterException ex)
        {
            LogError(string.Format("Lua ExecuteString error: {0}", ex.DecoratedMessage));
        }
        catch (Exception ex)
        {
            LogError(string.Format("System ExecuteString error: {0}", ex.Message));
        }

        return result;
    }
    
    /// <summary>
    /// Attemps to load a string containing lua code
    /// </summary>
    /// <param name="luaString"></param>
    /// <returns>Null if an error occured otherwise will return the DynValue of the script. This can be passed to Call()</returns>
    public static DynValue LoadString(this Script script, string luaString)
    {
        DynValue result = DynValue.Nil;

        try
        {
            result = script.LoadString(luaString);
        }
        catch (InterpreterException ex)
        {
            LogError(string.Format("Lua ExecuteString error: {0}", ex.DecoratedMessage));
        }
        catch (Exception ex)
        {
            LogError(string.Format("System ExecuteString error: {0}", ex.Message));
        }

        return result;
    }

    /// <summary>
    /// Call a lua function via DynValue
    /// </summary>
    /// <param name="luaFunc"></param>
    /// <param name="args"></param>
    /// <returns>Null if call fails of function is invalid, else the result of the function</returns>
    public static DynValue Call(this Script script, DynValue luaFunc, params object[] args)
    {
        DynValue result = DynValue.Nil;

        if (luaFunc.IsNotNil() && luaFunc.Type == DataType.Function)
        {
            try
            {
                result = script.Call(luaFunc, args);
            }
            catch (ScriptRuntimeException ex)
            {
                LogError(string.Format("Lua Call error: {0}", ex.DecoratedMessage));
            }
        }
        else
        {
            LogError("Call: Provided value is not a Lua function");
        }

        return result;
    }

    /// <summary>
    /// Call a Lua function via name
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public static DynValue Call(this Script script, string functionName, params object[] args)
    {
        DynValue result = DynValue.Nil;

        if (!string.IsNullOrEmpty(functionName))
        {
            DynValue func = script.GetGlobal(functionName);
            if (func.Type == DataType.Function)
            {
                try
                {
                    result = script.Call(func, args);
                }
                catch (InterpreterException ex)
                {
                    LogError(string.Format("Lua error calling function {0}: {1}", functionName, ex.DecoratedMessage));
                }
            }
            else
            {
                LogError(string.Format("Failed to find Lua function '{0}'", functionName));
            }
        }
        return result;
    }
     
    /// <summary>
    /// Scan and register anything with the [LuaApiEnum] attribute.
    /// </summary>
    private static void BuildEnumTable()
    { 
            m_EnumTables = DynValue.NewPrimeTable();
            List<Type> luaEnumList = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                luaEnumList.AddRange((assembly.GetTypes()
                    .Where(luaEnumType => Attribute.IsDefined(luaEnumType, typeof(LuaApiEnum)))));
            }

            foreach (Type enumType in luaEnumList)
            {
            // Get the attribute
            LuaApiEnum apiEnumAttrib = (LuaApiEnum)enumType.GetCustomAttributes(typeof(LuaApiEnum), false)[0];

                // Create the table for this enum and get a reference to it 
                m_EnumTables.Table.Set(apiEnumAttrib.name, DynValue.NewPrimeTable());
                Table enumTable = m_EnumTables.Table.Get(apiEnumAttrib.name).Table;

                // Foreach value in the enum list
                foreach (var enumValue in Enum.GetValues(enumType))
                {
                    var memberInfo = enumType.GetMember(enumValue.ToString());
                    var attribute = memberInfo[0].GetCustomAttributes(typeof(LuaApiEnumValue), false);

                    // Double check they've not been flagged as hidden
                    if (attribute.Length > 0 && ((LuaApiEnumValue)attribute[0]).hidden)
                    {
                        continue;
                    }

                    enumTable.Set(enumValue.ToString(), DynValue.NewNumber((int)enumValue));
                }
            }  
    }
    /// <summary>
    /// Attaches enums decorated with the [LuaApiEnum] attribute to this script.
    /// </summary> 
    private static void AttachEnums(this Script script)
    {
        if (m_EnumTables == null)
            BuildEnumTable();

        // Iterate through the enum cache and copy the values into our globals
        foreach (var enumPair in m_EnumTables.Table.Pairs) 
            script.Globals.Set(enumPair.Key, enumPair.Value); 
    }
}
