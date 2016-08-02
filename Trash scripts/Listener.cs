using UnityEngine;

using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client.message;
using com.shephertz.app42.gaming.multiplayer.client.transformer;

using System;
using System.Collections.Generic;

public class Listener : ConnectionRequestListener, LobbyRequestListener, ZoneRequestListener,
RoomRequestListener, ChatRequestListener, UpdateRequestListener, NotifyListener
{
	public String[] resultCodes = {
		"SUCCESS",
		"AUTH_ERROR",
		"RESOURCE_NOT_FOUND",
		"RESOURCE_MOVED",
		"BAD_REQUEST",
		"CONNECTION_ERR",
		"UNKNOWN_ERROR",
		"RESULT_SIZE_ERROR",
		"SUCCESS_RECOVERED",
		"CONNECTION_ERROR_RECOVERABLE"
	};

	public void Log(string msg, byte resultCode)
	{
		if (resultCode == 0) {
			Master.getInstance().Log(msg + resultCodes [resultCode]);
		}
		else {
			Master.getInstance().LogWarning(msg + resultCodes [resultCode]);
		}
	}
		
	public void Log(string msg)
	{
		Master.getInstance().Log(msg);
	}
		
#region ConnectionRequestListener
	public void onConnectDone(ConnectEvent eventObj)
	{
		Log("onConnectDone : ", eventObj.getResult());
		WarpClient.GetInstance().GetRoomsInRange(1, 1);
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
		
#region LobbyRequestListener
	public void onJoinLobbyDone(LobbyEvent eventObj)
	{
		Log("onJoinLobbyDone : ", eventObj.getResult());
	}
		
	public void onLeaveLobbyDone(LobbyEvent eventObj)
	{
		Log("onLeaveLobbyDone : ", eventObj.getResult());
	}
		
	public void onSubscribeLobbyDone(LobbyEvent eventObj)
	{
		Log("onSubscribeLobbyDone : ", eventObj.getResult());
	}
		
	public void onUnSubscribeLobbyDone(LobbyEvent eventObj)
	{
		Log("onUnSubscribeLobbyDone : ", eventObj.getResult());
	}
		
	public void onGetLiveLobbyInfoDone(LiveRoomInfoEvent eventObj)
	{
		Log("onGetLiveLobbyInfoDone : ", eventObj.getResult());
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
		if (eventObj.getResult() == 0) {
			AppWarp.roomID = eventObj.getData().getId();
			Log(AppWarp.roomID);
			WarpClient.GetInstance().JoinRoom(eventObj.getData().getId());
		}
	}
		
	public void onGetOnlineUsersDone(AllUsersEvent eventObj)
	{
		Log("onGetOnlineUsersDone : ", eventObj.getResult());
	}
		
	public void onGetLiveUserInfoDone(LiveUserInfoEvent eventObj)
	{
		Log("onGetLiveUserInfoDone : ", eventObj.getResult());
		Log("Password = " + eventObj.getCustomData());
	}
		
	public void onSetCustomUserDataDone(LiveUserInfoEvent eventObj)
	{
		Log("onSetCustomUserDataDone : ", eventObj.getResult());
		WarpClient.GetInstance().GetLiveUserInfo(Master.getInstance().username);
	}
		
	public void onGetMatchedRoomsDone(MatchedRoomsEvent eventObj)
	{
		Log("onGetMatchedRooms: ", eventObj.getResult());
		if (eventObj.getResult() == 0) {
			RoomData[] rooms = eventObj.getRoomsData();
			;
			foreach (RoomData  room in rooms) {
				if (room.getName() == Master.getInstance().roomName) {
					AppWarp.roomID = room.getId();
					break;
				}
			}
			if (AppWarp.roomID != null) {
				WarpClient.GetInstance().JoinRoom(AppWarp.roomID);
				Log(AppWarp.roomID);
			}
			else {
				WarpClient.GetInstance().CreateRoom(Master.getInstance().roomName, Master.getInstance().username, 2, null);
			}
		}
	}
#endregion		
		
#region RoomRequestListener
	public void onSubscribeRoomDone(RoomEvent eventObj)
	{
		Log("onSubscribeRoomDone : ", eventObj.getResult());
	}
		
	public void onUnSubscribeRoomDone(RoomEvent eventObj)
	{
		Log("onUnSubscribeRoomDone : ", eventObj.getResult());
	}
		
	public void onJoinRoomDone(RoomEvent eventObj)
	{
		Log("onJoinRoomDone : ", eventObj.getResult());
		WarpClient.GetInstance().SubscribeRoom(AppWarp.roomID);	
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
		
#region UpdateRequestListener
	public void onSendUpdateDone(byte result)
	{
		Log("onSendUpdateDone : ");
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
	}
		
	public void onUserJoinedRoom(RoomData eventObj, string username)
	{
		Log("onUserJoinedRoom : " + username);
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
		if (eventObj.getSender() != Master.getInstance().username) {
			AppWarp.receiveMove(eventObj.getMessage());
		}
	}
		
	public void onUpdatePeersReceived(UpdateEvent eventObj)
	{
		Log("onUpdatePeersReceived");
	}
		
	public void onUserChangeRoomProperty(RoomData roomData, string sender, Dictionary<String, System.Object> properties)
	{
		Log("onUserChangeRoomProperty");
		Log(roomData.getId());
		Log(sender);
		foreach (KeyValuePair<String, System.Object> entry in properties) {
			Log("KEY:" + entry.Key);
			Log("VALUE:" + entry.Value.ToString());
		}
	}
		
	public void onUserPaused(string a, bool b, string c)
	{
		Log("onUserPaused : ");
	}
		
	public void onUserResumed(string a, bool b, string c)
	{
		Log("onUserResumed : ");
	}
		
	public void onGameStarted(string a, string b, string c)
	{
		Log("onGameStarted : ");
	}
		
	public void onGameStopped(string a, string b)
	{
		Log("onGameStopped : ");
	}
#endregion
}


