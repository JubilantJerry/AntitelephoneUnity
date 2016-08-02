using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.user;

public static class App42
{
	public static string apiKey = "e7a39b2cd6b1ef0687c012992fdbfc201404a3cc6580ea72387a228863ec4f43";
	public static string secretKey = "d814cc1cebec848e66cb0b515dbf7cb69ebdf85033a09b8c82ea24a2a4524672";

	public static double skillLevel;

	private static ServiceAPI serviceAPI;
	private static UserService userService;
	private static CallBack callback;

	// Use this for initialization
	static void initiate()
	{
		App42API.Initialize(apiKey, secretKey);
		userService = App42API.BuildUserService();
		callback = new CallBack();
		;
	}

	private class CallBack: App42CallBack
	{
		public void OnSuccess(object response)
		{  
			User user = (User)response;  
			App42Log.Console("userName is " + user.GetUserName());  
			App42Log.Console("emailId is " + user.GetEmail());   
		}  
		public void OnException(Exception e)
		{  
			App42Exception exception = (App42Exception)e;  
			int appErrorCode = exception.GetAppErrorCode();  
			if (appErrorCode == 2001) {

			}
			App42Log.Console("Exception : " + e);  
		}  
	}
}
