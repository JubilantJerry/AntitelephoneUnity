using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour
	// Deals with top-level GUI, AppWarp, initialization behavior
{
	// Toggles GUI behavior
	enum StatusCode
	{
		RECONNECTING,
		OPP_RECONNECTING,
		OPERATIONAL,
		DISCONNECTED,
		OPP_LEFT,
		USER_PAUSED,
		GAME_END_VICTORY,
		GAME_END_DEFEAT}
	;
	StatusCode status;

	public delegate void PauseNotification(bool paused);
	public static event PauseNotification pauseEvent;
	bool currentPauseState;

	void Start()
	{
		Master.getInstance().Log("Game scene starts");
		currentPauseState = false;
		status = StatusCode.OPERATIONAL;
		if (Master.getInstance().multiplayer) {
			AppWarp.connectionStatusEvent += connectionStatusHandler;
			AppWarp.oppLeftEvent += opponentLeftHandler;
			AppWarp.oppPauseEvent += opponentPauseHandler;
			AppWarp.oppResumeEvent += opponentResumeHandler;
		}
	}

	void OnDestroy()
	{
		AppWarp.connectionStatusEvent -= connectionStatusHandler;
		AppWarp.oppLeftEvent -= opponentLeftHandler;
		AppWarp.oppPauseEvent -= opponentPauseHandler;
		AppWarp.oppResumeEvent -= opponentResumeHandler;
		AppWarp.exit();
	}

	void OnGUI()
	{
		GUI.Label(new Rect(500, 10, 300, 600), status.ToString());
	}

	void pauseGame(bool paused)
	{
		if (pauseEvent != null && currentPauseState != paused) {
			currentPauseState = paused;
			pauseEvent(paused);
			Master.getInstance().Log("Game pause state set to " + paused.ToString());
		}
	}

	void endGame(bool winner)
	{
		if (winner) {
			status = StatusCode.GAME_END_VICTORY;
		}
		else {
			status = StatusCode.GAME_END_DEFEAT;
			AppWarp.deleteCurrentRoom();
		}
	}

	void connectionStatusHandler(AppWarp.ResultCode result)
	{
		switch (result) {
			case AppWarp.ResultCode.SUCCESS_RECOVERED:
				status = StatusCode.OPERATIONAL;
				pauseGame(false);
				break;
			case AppWarp.ResultCode.CONNECTION_ERROR_RECOVERABLE:
				pauseGame(true);
				Invoke("reconnect", 5f);
				status = StatusCode.RECONNECTING;
				break;
			case AppWarp.ResultCode.CONNECTION_ERR:
				pauseGame(true);
				status = StatusCode.DISCONNECTED;
				break;
		}
	}

	void reconnect()
	{
		AppWarp.reconnect();
	}

	void opponentPauseHandler()
	{
		pauseGame(true);
		status = StatusCode.OPP_RECONNECTING;
	}

	void opponentResumeHandler()
	{
		pauseGame(false);
		status = StatusCode.OPERATIONAL;
	}

	void opponentLeftHandler()
	{
		pauseGame(true);
		AppWarp.deleteCurrentRoom();
		status = StatusCode.OPP_LEFT;
	}
}
