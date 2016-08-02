using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client.message;
using com.shephertz.app42.gaming.multiplayer.client.transformer;

public static class AppWarp
{
	public delegate void SimpleNotification();
	public delegate void ResultNotification(ResultCode resultCode);
	public delegate void MoveNotification(WorldData.MoveData move);
	public delegate void UserNotification(string username);
	public delegate void RoomNotification(Dictionary<string, object> roomInfo);

	public static event SimpleNotification oppLeftEvent, oppPauseEvent, oppResumeEvent;
	public static event ResultNotification connectionStatusEvent, roomJoinedEvent;
	public static event MoveNotification moveReceivedEvent;
	public static event UserNotification oppJoinedEvent;
	public static event RoomNotification roomFoundEvent;

	static string apiKey = "9d074573d8ae28d12c88a93446c19826a7096aed7ede12e16bd2243529786c20";
	static string secretKey = "039978662a3ad7c98c5ca0880462e2f5ad6d47e3d55c454d56ed94dc79ae9fc8";
	static string currentRoom_ID;
	static string sessionName;
	static bool evenNumberedMoveSent;
	static bool evenNumberedMoveReceived;

	public const string MATCHUP_ROOM_NAME = "*matchUpRoom*";
	public const int RECOVERY_TIME_SECS = 90;

	static Listener listen = new Listener();

	public enum ResultCode
	{
		SUCCESS,
		AUTH_ERROR,
		RESOURCE_NOT_FOUND,
		RESOURCE_MOVED,
		BAD_REQUEST,
		CONNECTION_ERR,
		UNKNOWN_ERROR,
		RESULT_SIZE_ERROR,
		SUCCESS_RECOVERED,
		CONNECTION_ERROR_RECOVERABLE,
	}
	;

// External methods
	public static void initialize()
	{
		WarpClient.initialize(apiKey, secretKey);
		WarpClient.setRecoveryAllowance(RECOVERY_TIME_SECS);
		WarpClient.GetInstance().AddConnectionRequestListener(listen);
		WarpClient.GetInstance().AddChatRequestListener(listen);
		WarpClient.GetInstance().AddNotificationListener(listen);
		WarpClient.GetInstance().AddRoomRequestListener(listen);
		WarpClient.GetInstance().AddZoneRequestListener(listen);
		MonoBehaviour.DontDestroyOnLoad(WarpClient.GetInstance());
		evenNumberedMoveSent = true;
		evenNumberedMoveReceived = true;

		sessionName = System.DateTime.Now.Ticks.ToString();
		// Returns connection result in connectionStatusEvent
		WarpClient.GetInstance().Connect(sessionName);
	}

	public static void associateUserName()
	{
		Log("Username associated");
		WarpClient.GetInstance().SetCustomUserData(sessionName, Master.getInstance().username);
	}

	public static void reconnect()
	{
		Log("Reconnect attempted");
		// Returns reconnect result in connectionStatusEvent
		WarpClient.GetInstance().RecoverConnection();
	}
	
	public static void exit()
	{
		if (WarpClient.GetInstance() != null) {
			Log("Multiplayer disconnect requested");
			WarpClient.GetInstance().Disconnect();
		}
		MonoBehaviour.DestroyObject(WarpClient.GetInstance());
	}

	public static void getRoom(string name)
	{
		// returns a MatchParameter from roomFoundEvent
		Log("Attempted to join room with name: " + name);
		listen.roomNameFilter = name;
		WarpClient.GetInstance().GetRoomsInRange(1, 1);
	}

	public static void joinFoundRoom()
	{
		WarpClient.GetInstance().JoinRoom(currentRoom_ID);
	}

	public static void createRoom(string roomName, Dictionary<string, object> properties)
	{
		Log("Creating room with name: " + roomName);
		properties.Add("Room owner", (object)Master.getInstance().username);
		WarpClient.GetInstance().CreateRoom(roomName, sessionName, 2, properties);
	}

	public static void deleteCurrentRoom()
	{
		Log("Room disconnect requested");
		if (string.IsNullOrEmpty(currentRoom_ID)) {
			WarpClient.GetInstance().DeleteRoom(currentRoom_ID);
			currentRoom_ID = null;
		}
	}

	public static void submitMove(WorldData.MoveData move)
	{
		evenNumberedMoveSent = !evenNumberedMoveSent;
		Log("Move send requested. Even numbered: " + evenNumberedMoveSent.ToString());
		WarpClient.GetInstance().SendChat(serialize(move));
	}

	public static void reSubmitMove(WorldData.MoveData move)
	{
		Log("Move resend requested. Even numbered: " + evenNumberedMoveSent.ToString());
		WarpClient.GetInstance().SendChat(serialize(move));
	}

// Internal tools
	static void Log(string msg)
	{
		Master.getInstance().Log(msg);
	}
	
	static void Log(string msg, byte resultCode)
	{
		string logMsg = msg + ((ResultCode)resultCode).ToString();
		if ((ResultCode)resultCode == ResultCode.SUCCESS) {
			Master.getInstance().Log(logMsg);
		}
		else {
			Master.getInstance().LogWarning(logMsg);
		}
	}
	
	static string serialize(WorldData.MoveData move)
	{
		string[] arr = {
		evenNumberedMoveSent ? "1" : "0", "|",
		((int)move.location).ToString(), "|",
		move.energyExpended.ToString(), "|",
			move.timeDist.ToString()};
		return string.Concat(arr);
	}
	
	static bool getPacketEvenParity(string packet)
	{
		return (packet.Substring(0, 1) == "1");
	}

	static WorldData.MoveData deSerialize(string encodedMove)
	{
		WorldData.Facility facility;
		string[] packetContents = encodedMove.Split('|');
		facility = (WorldData.Facility)(int.Parse(packetContents [1]));
		int energyExpended = int.Parse(packetContents [2]);
		int timeDist = int.Parse(packetContents [3]);
		return new WorldData.MoveData(facility, energyExpended, timeDist);
	}

#region Listener
	private class Listener : ConnectionRequestListener, ZoneRequestListener,
	RoomRequestListener, ChatRequestListener, NotifyListener
	{
		public string roomNameFilter = AppWarp.MATCHUP_ROOM_NAME;
		#region ConnectionRequestListener
		public void onConnectDone(ConnectEvent eventObj)
		{
			Log("onConnectDone : ", eventObj.getResult());
			if (connectionStatusEvent != null) {
				connectionStatusEvent((ResultCode)eventObj.getResult());
			}
		}
		
		public void onInitUDPDone(byte res)
		{
			Log("onInitUDPDone : ");
		}
		
		public void onDisconnectDone(ConnectEvent eventObj)
		{
			Log("onDisconnectDone : ", eventObj.getResult());
		}
		#endregion
		
		#region ZoneRequestListener
		public void onDeleteRoomDone(RoomEvent eventObj)
		{
			Log("onDeleteRoomDone : ", eventObj.getResult());
		}
		
		public void onGetAllRoomsDone(AllRoomsEvent eventObj)
		{
			Log("onGetAllRoomsDone : ", eventObj.getResult());
		}
		
		public void onCreateRoomDone(RoomEvent eventObj)
		{
			Log("onCreateRoomDone : ", eventObj.getResult());
			if ((ResultCode)eventObj.getResult() == ResultCode.SUCCESS) {
				AppWarp.currentRoom_ID = eventObj.getData().getId();
				WarpClient.GetInstance().JoinRoom(currentRoom_ID);
				Log(AppWarp.currentRoom_ID);
			}
		}
		
		public void onGetOnlineUsersDone(AllUsersEvent eventObj)
		{
			Log("onGetOnlineUsersDone : ", eventObj.getResult());
		}
		
		public void onGetLiveUserInfoDone(LiveUserInfoEvent eventObj)
		{
			Log("onGetLiveUserInfoDone : ", eventObj.getResult());
			Log("Data = " + eventObj.getCustomData());
			if (oppJoinedEvent != null) {
				oppJoinedEvent(eventObj.getCustomData());
			}
		}
		
		public void onSetCustomUserDataDone(LiveUserInfoEvent eventObj)
		{
			Log("onSetCustomUserDataDone : ", eventObj.getResult());
		}
		
		public void onGetMatchedRoomsDone(MatchedRoomsEvent eventObj)
		{
			Log("onGetMatchedRooms: ", eventObj.getResult());
			if ((ResultCode)eventObj.getResult() == ResultCode.SUCCESS) {
				string matchedRoomID = null;
				RoomData[] rooms = eventObj.getRoomsData();
				foreach (RoomData room in rooms) {
					Log(room.getName() + " vs. " + roomNameFilter);
					if (room.getName() == (roomNameFilter)) {
						Log("Found appropriate room");
						matchedRoomID = room.getId();
						break;
					}
				}
				if (string.IsNullOrEmpty(matchedRoomID)) {
					Log("No appropriate room found");
					if (roomFoundEvent != null) {
						roomFoundEvent(null);
					}
				}
				else {
					currentRoom_ID = matchedRoomID;
					WarpClient.GetInstance().GetLiveRoomInfo(matchedRoomID);
				}
			}
		}
		#endregion		
		
		#region RoomRequestListener
		public void onSubscribeRoomDone(RoomEvent eventObj)
		{
			Log("onSubscribeRoomDone : ", eventObj.getResult());
			if ((ResultCode)eventObj.getResult() == ResultCode.SUCCESS) {
				if (roomJoinedEvent != null) {
					roomJoinedEvent((ResultCode)eventObj.getResult());
				}
			}
		}
		
		public void onUnSubscribeRoomDone(RoomEvent eventObj)
		{
			Log("onUnSubscribeRoomDone : ", eventObj.getResult());
		}
		
		public void onJoinRoomDone(RoomEvent eventObj)
		{
			Log("onJoinRoomDone : ", eventObj.getResult());
			if ((ResultCode)eventObj.getResult() == ResultCode.SUCCESS) {
				WarpClient.GetInstance().SubscribeRoom(currentRoom_ID);
			}
		}
		
		public void onLockPropertiesDone(byte result)
		{
			Log("onLockPropertiesDone :", result);
		}
		
		public void onUnlockPropertiesDone(byte result)
		{
			Log("onUnlockPropertiesDone :", result);
		}
		
		public void onLeaveRoomDone(RoomEvent eventObj)
		{
			Log("onLeaveRoomDone : ", eventObj.getResult());
		}

		public void onGetLiveRoomInfoDone(LiveRoomInfoEvent eventObj)
		{
			Log("onGetLiveRoomInfoDone : ", eventObj.getResult());
			if ((ResultCode)eventObj.getResult() == ResultCode.SUCCESS) {
				if (roomFoundEvent != null) {
					roomFoundEvent(eventObj.getProperties());
				}
			}
		}
		
		public void onSetCustomRoomDataDone(LiveRoomInfoEvent eventObj)
		{
			Log("onSetCustomRoomDataDone : ", eventObj.getResult());
		}
		
		public void onUpdatePropertyDone(LiveRoomInfoEvent eventObj)
		{
			Log("OnUpdatePropertyDone :", eventObj.getResult());
		}
		#endregion
		
		#region ChatRequestListener
		public void onSendChatDone(byte result)
		{
			Log("onSendChatDone result :", result);
		}
		
		public void onSendPrivateChatDone(byte result)
		{
			Log("onSendPrivateChatDone :", result);
		}
		#endregion
		
		#region NotifyListener
		public void onRoomCreated(RoomData eventObj)
		{
			Log("onRoomCreated");
		}
		
		public void onRoomDestroyed(RoomData eventObj)
		{
			Log("onRoomDestroyed");
		}
		
		public void onUserLeftRoom(RoomData eventObj, string username)
		{
			Log("onUserLeftRoom : " + username);
			if (username != (sessionName)) {
				if (oppLeftEvent != null) {
					oppLeftEvent();
				}
			}
		}
		
		public void onUserJoinedRoom(RoomData eventObj, string username)
		{
			Log("onUserJoinedRoom : " + username);
			WarpClient.GetInstance().GetLiveUserInfo(username);
		}
		
		public void onUserLeftLobby(LobbyData eventObj, string username)
		{
			Log("onUserLeftLobby : " + username);
		}
		
		public void onUserJoinedLobby(LobbyData eventObj, string username)
		{
			Log("onUserJoinedLobby : " + username);
		}
		
		public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, object> properties, Dictionary<string, string> lockedPropertiesTable)
		{
			Log("onUserChangeRoomProperty : " + sender);
		}
		
		public void onPrivateChatReceived(string sender, string message)
		{
			Log("onPrivateChatReceived : " + sender);
		}
		
		public void onMoveCompleted(MoveEvent move)
		{
			Log("onMoveCompleted by : " + move.getSender());
		}
		
		public void onChatReceived(ChatEvent eventObj)
		{
			Log(eventObj.getSender() + " sent " + eventObj.getMessage());
			if (eventObj.getSender() != (sessionName)) {
				string packet = eventObj.getMessage();
				if (getPacketEvenParity(packet) != evenNumberedMoveReceived) {
					evenNumberedMoveReceived = !evenNumberedMoveReceived;
					if (moveReceivedEvent != null) {
						moveReceivedEvent(deSerialize(packet));
					}
				}
			}
		}
		
		public void onUpdatePeersReceived(UpdateEvent eventObj)
		{
			Log("onUpdatePeersReceived");
		}
		
		public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<string, System.Object> properties)
		{
			Log("onUserChangeRoomProperty");
			Log(roomData.getId());
			Log(sender);
			foreach (KeyValuePair<string, System.Object> entry in properties) {
				Log("KEY:" + entry.Key);
				Log("VALUE:" + entry.Value.ToString());
			}
		}
		
		public void onUserPaused(string locID, bool isLobby, string username)
		{
			Log("onUserPaused : ");
			if (username != (sessionName)) {
				if (oppPauseEvent != null) {
					oppPauseEvent();
				}
			}
		}
		
		public void onUserResumed(string locID, bool isLobby, string username)
		{
			Log("onUserResumed : ");
			if (username != (sessionName)) {
				if (oppResumeEvent != null) {
					oppResumeEvent();
				}
			}
		}
		
		public void onGameStarted(string a, string b, string c)
		{
			Log("onGameStarted : ");
		}
		
		public void onGameStopped(string a, string b)
		{
			Log("onGameStopped : ");
		}

		public void onPrivateUpdateReceived(string s, byte[] b, bool c)
		{
			Log("onPrivateUpdateReceived : ");
		}

		#endregion
	
	}
	#endregion

}
