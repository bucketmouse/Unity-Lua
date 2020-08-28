# LuaUtility

Quick genericized port of [Unity-Lua](https://github.com/Semaeopus/Unity-Lua) that provides most of its functionality as static extension methods to Moonsharp's Script class.  
This is mostly intended as minimal glue for integrating external bruteforce testing/generation tools that need to share code with your Lua wrapper.

Currently doesn't support documentation generation functionality or debugger attaching. Just use Lua-Unity if you need those for now.

#
# Installation
Include LuaUtility.cs somewhere in your project. If the project isn't already using Unity-Lua include LuaAttributes.cs as well. 

Whenever you create a script, call AttachEnums() on it to mirror your decorated C# enums.

Assign LuaUtility.logHandler to some function to handle errors if you'd like it to do something aside from throwing an exception when an error occurs.

e.g
```C#
LuaUtility.logHandler = (message) => Unity.Log(message);
```

# Dependancies
None.

# Documentation

See the original Unity-Lua documentation.
