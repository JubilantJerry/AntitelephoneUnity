using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectControl : MonoBehaviour
{

	enum StatusCode
	{
		CONNECTING,
		CONNECTED,
		DISCONNECTED,
	};

	enum MenuMode
	{
		SP_SETUP,
		MP_INIT,
		MP_INPUTROOMNAME,
		MP_CREATEROOM,
		MP_WAITING,
		MP_BIN_CHOICE,
		MP_MONO_CHOICE
	}
	;

	class MenuChoice
	{
		public delegate void Choose(bool choice);
		public string message, firstOption, secondOption;
		public Choose onChoice;
	}

	StatusCode status;
	MenuMode mode;
	MenuChoice customChoice;

	Dictionary<string, object> matchParameters, debugSettings1, debugSettings2;
	string roomName;
	string oppUsername;
	bool matchUp;

	public GameObject roundOverviewObj; // Set via inspector

	// Use this for initialization
	void Start()
	{
		Master.getInstance().Log("Level select scene starts");
		if (Master.getInstance().multiplayer) {
			AppWarp.connectionStatusEvent += connectionStatusHandler;
			AppWarp.initialize();
			status = StatusCode.CONNECTING;
			mode = MenuMode.MP_WAITING;
		}
		else {
			status = StatusCode.CONNECTED;
			mode = MenuMode.SP_SETUP;
		}
		customChoice = new MenuChoice();
	}

	void OnApplicationQuit()
	{
		if (Master.getInstance().multiplayer) {
			AppWarp.deleteCurrentRoom();
			AppWarp.exit();
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(400, 10, 300, 600), status.ToString());
		string text = "";
		switch (mode) {
			case MenuMode.SP_SETUP:
				text = "1. Begin game with debugSetup1\n2. Begin game with debugSetup2\n3. Back to menu";
				break;
			case MenuMode.MP_INIT:
				text = "1: Named Room\n2: Matchup\n3. Back to menu";
				break;
			case MenuMode.MP_INPUTROOMNAME:
				text = "1: Use name \"debugRoom1\"\n2. Use name \"debugRoom2\"\n3. Back";
				break;
			case MenuMode.MP_CREATEROOM:
				text = "1. Begin game with debugSetup1\n2. Begin game with debugSetup2\n3. Back";
				break;
			case MenuMode.MP_WAITING:
				text = "Please wait...";
				break;
			case MenuMode.MP_BIN_CHOICE:
				if (!string.IsNullOrEmpty(customChoice.message)) {
					text = customChoice.message + "\n1. " + customChoice.firstOption + "\n2. " + customChoice.secondOption;
				}
				break;
			case MenuMode.MP_MONO_CHOICE:
				if (!string.IsNullOrEmpty(customChoice.message)) {
					text = customChoice.message + "\n1. " + customChoice.firstOption;
				}
				break;
		}
		GUI.Label(new Rect(400, 50, 300, 600), text);
	}


	void Update()
	{
		int choice = 0;
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			choice = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			choice = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			choice = 3;
		}
		
		switch (mode) {
			case MenuMode.SP_SETUP:
				switch (choice) {
					case 1:
						debugSettings1 = new Dictionary<string, object>() {
							{"Lives", 5}, 
							{"First player first facility", (int)(WorldData.Facility.REDIRECTOR)},
							{"Second player first facility", (int)(WorldData.Facility.SEISMOMETER)}};
						matchParameters = debugSettings1;
						oppUsername = "AI";
						matchParameters.Add("First player", "true");
						startGame();
						break;
					case 2:
						debugSettings2 = new Dictionary<string, object>() {
							{"Lives", 10}, 
							{"First player first facility", (int)(WorldData.Facility.REDIRECTOR)},
							{"Second player first facility", (int)(WorldData.Facility.SEISMOMETER)}};
						matchParameters = debugSettings2;
						oppUsername = "AI";
						matchParameters.Add("First player", "false");
						startGame();
						break;
					case 3:
						Application.LoadLevel("Menu");
						break;
				}
				break;
			case MenuMode.MP_INIT:
				switch (choice) {
					case 1:
						matchUp = false;
						mode = MenuMode.MP_INPUTROOMNAME;
						break;
					case 2:
						matchUp = true;
						AppWarp.roomFoundEvent += roomFoundEventHandler;
						roomName = AppWarp.MATCHUP_ROOM_NAME;
						AppWarp.getRoom(roomName);
						mode = MenuMode.MP_WAITING;
						break;
					case 3:
						AppWarp.exit();
						Application.LoadLevel("Menu");
						break;
				}
				break;
			case MenuMode.MP_INPUTROOMNAME:
				switch (choice) {
					case 1:
						roomName = "debugRoom1";
						AppWarp.roomFoundEvent += roomFoundEventHandler;
						AppWarp.getRoom(roomName);
						mode = MenuMode.MP_WAITING;
						break;
					case 2:
						roomName = "debugRoom2";
						AppWarp.roomFoundEvent += roomFoundEventHandler;
						AppWarp.getRoom(roomName);
						mode = MenuMode.MP_WAITING;
						break;
					case 3:
						mode = MenuMode.MP_INIT;
						break;
				}
				break;
			case MenuMode.MP_CREATEROOM:
				switch (choice) {
					case 1:
						debugSettings1 = new Dictionary<string, object>() {{"Lives", "5"}};
						matchParameters = debugSettings1;
						AppWarp.oppJoinedEvent += oppJoinedEventHandler;
						AppWarp.createRoom(roomName, matchParameters);
						customChoice.message = "Room created, waiting for opponent";
						customChoice.firstOption = "Cancel";
						customChoice.onChoice = cancelRoomWaitChoice;
						mode = MenuMode.MP_MONO_CHOICE;
						break;
					case 2:
						debugSettings2 = new Dictionary<string, object>() {{"Lives", "10"}};
						matchParameters = debugSettings2;
						AppWarp.oppJoinedEvent += oppJoinedEventHandler;
						AppWarp.createRoom(roomName, matchParameters);
						customChoice.message = "Room created, waiting for opponent";
						customChoice.firstOption = "Cancel";
						customChoice.onChoice = cancelRoomWaitChoice;
						mode = MenuMode.MP_MONO_CHOICE;
						break;
					case 3:
						mode = MenuMode.MP_INIT;
						break;
				}
				break;
			case MenuMode.MP_BIN_CHOICE:
				if (customChoice.onChoice != null) {
					switch (choice) {
						case 1:
							customChoice.onChoice(true);
							break;
						case 2:
							customChoice.onChoice(false);
							break;
					}
				}
				break;
			case MenuMode.MP_MONO_CHOICE:
				if (customChoice.onChoice != null) {
					switch (choice) {
						case 1:
							customChoice.onChoice(true);
							break;
					}
				}
				break;
		}
	}

	void connectionStatusHandler(AppWarp.ResultCode result)
	{
		switch (result) {
			case AppWarp.ResultCode.SUCCESS:
				status = StatusCode.CONNECTED;
				mode = MenuMode.MP_INIT;
				AppWarp.associateUserName();
				break;
			default: 
				customChoice.message = "Disconnected from server";
				customChoice.firstOption = "Return to main menu";
				customChoice.onChoice = connectionErrorChoice;
				mode = MenuMode.MP_MONO_CHOICE;
				status = StatusCode.DISCONNECTED;
				break;
		}
	}

	void connectionErrorChoice(bool b)
	{
		AppWarp.exit();
		Application.LoadLevel("Menu");
	}

	void roomDeletedErrorChoice(bool b)
	{
		mode = MenuMode.MP_INIT;
	}

	void noRoomsFoundChoice(bool choice)
	{
		if (choice) {
			mode = MenuMode.MP_CREATEROOM;
		}
		else {
			mode = MenuMode.MP_INIT;
		}
	}

	void roomsFoundChoice(bool choice)
	{
		if (choice) {
			AppWarp.roomJoinedEvent += roomJoinedEventHandler;
			AppWarp.joinFoundRoom();
		}
		else {
			if (matchUp) {
				mode = MenuMode.MP_CREATEROOM;
			}
			else {
				mode = MenuMode.MP_INIT;
			}
		}
	}

	void cancelRoomWaitChoice(bool b)
	{
		AppWarp.deleteCurrentRoom();
		mode = MenuMode.MP_INIT;
	}

	void roomFoundEventHandler(Dictionary<string, object> parameters)
	{
		AppWarp.roomFoundEvent -= roomFoundEventHandler;
		if (parameters == null) {
			customChoice.message = "No rooms found, create room?";
			customChoice.firstOption = "Create Room";
			customChoice.secondOption = "Return to menu";
			customChoice.onChoice = noRoomsFoundChoice;
		}
		else {
			matchParameters = parameters;
			oppUsername = (string)parameters ["Room owner"];
			customChoice.message = "Matchup rooms exist. Opponent is " + oppUsername;
			customChoice.firstOption = "Join Room";
			if (matchUp) {
				customChoice.secondOption = "Create Room";
			}
			else {
				customChoice.secondOption = "Return to menu";
			}
			customChoice.onChoice = roomsFoundChoice;
		}
		mode = MenuMode.MP_BIN_CHOICE;
	}

	void roomJoinedEventHandler(AppWarp.ResultCode result)
	{
		if (result == AppWarp.ResultCode.SUCCESS) {
			matchParameters.Add("First player", "false");
			startGame();
		}
		else {
			customChoice.message = "Room already deleted";
			customChoice.firstOption = "Return to main menu";
			customChoice.onChoice = roomDeletedErrorChoice;
			mode = MenuMode.MP_MONO_CHOICE;
		}
	}

	void oppJoinedEventHandler(string username)
	{
		AppWarp.oppJoinedEvent -= oppJoinedEventHandler;
		oppUsername = username;
		matchParameters.Add("First player", "true");
		startGame();
	}

	void startGame()
	{
		Master.getInstance().matchParameters = matchParameters;
		Master.getInstance().Log("Master object match parameters configured: Lives = " + matchParameters ["Lives"]);
		roundOverviewObj.SetActive(true);
		RoundOverview roundOverview = roundOverviewObj.GetComponent<RoundOverview>();
		roundOverview.myUsername = Master.getInstance().username;
		roundOverview.oppUsername = oppUsername;
		roundOverview.matchParameters = matchParameters;
		customChoice.message = "";
		customChoice.onChoice = null;
		mode = MenuMode.MP_MONO_CHOICE;
	}
}
