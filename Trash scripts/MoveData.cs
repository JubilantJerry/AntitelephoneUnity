//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
public class MoveData
{
	public static string[] facilities = {"Server", "Power", "Redirector", "Seismometer"};
	public int location;
	public int options;
		
	public MoveData(int loc, int op)
	{
		location = loc;
		options = op;
	}
		
	public MoveData(int loc)
	{
		location = loc;
		options = 0;
	}
		
	public override string ToString()
	{
		return facilities [location] + ", " + options;
	}
}

