using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldData : MonoBehaviour
{
	public enum Facility
	{
		NONE,
		SERVER,
		POWER,
		REDIRECTOR,
		SEISMOMETER}
	;

	public delegate void UpdateNotification();
	public static event UpdateNotification worldUpdate;
	MoveData playerMove;
	MoveData oppMove;
	MoveData resendBackupMove;

	public TimeNode currentNode;

	// Use this for initialization
	void Start()
	{
		SimulationInput.moveMadeEvent += moveMadeEventHandler;
		if (Master.getInstance().multiplayer) {
			AppWarp.moveReceivedEvent += moveReceivedEventHandler;
		}
		else {
			AIScript.AIMoveCreatedEvent += moveReceivedEventHandler;
			AIScript.subscribeToUpdates();
		}
		GameControl.pauseEvent += onPause;
		applySettings(Master.getInstance().matchParameters);

		if (worldUpdate != null) {
			worldUpdate();
		}
	}

	void moveMadeEventHandler(MoveData move)
	{
		Master.getInstance().Log("Make move method called");
		if (playerMove == null) {
			resendBackupMove = move;
			playerMove = move;
			if (Master.getInstance().multiplayer) {
				AppWarp.submitMove(move);
			}
			if (oppMove != null) {
				finalizeMoves();
			}
		}
	}

	void moveReceivedEventHandler(MoveData move)
	{
		Master.getInstance().Log("Move receive method called");
		if (oppMove == null) {
			resendBackupMove = null;
			oppMove = move;
			if (playerMove != null) {
				finalizeMoves();
			}
		}
	}

	public void finalizeMoves()
	{
		bool antitelephoneUsed = (playerMove.timeDist == 0 && oppMove.timeDist == 0);
		WorldState newPlayerWorld, newOppWorld;
		bool reportSent;


		int playerTimeDist = playerMove.timeDist;
		int oppTimeDist = playerMove.timeDist;
		Facility playerNewLoc = playerMove.location;
		Facility oppNewLoc = oppMove.location;
		
		//assuming no antitelephone
		newPlayerWorld = currentNode.playerWorld.makeDuplicate();
		newOppWorld = currentNode.oppWorld.makeDuplicate();

		int playerFightEnergy = (playerMove.energyExpended == 0) ? newPlayerWorld.Energy : 0;
		int oppFightEnergy = (oppMove.energyExpended == 0) ? newOppWorld.Energy : 0;
		int energyDiff = playerFightEnergy - oppFightEnergy;
		
		if (playerNewLoc != oppNewLoc || (playerFightEnergy == 0 && oppFightEnergy == 0)) {
			updateWorld(playerNewLoc, playerMove.energyExpended, newPlayerWorld);
			updateWorld(oppNewLoc, oppMove.energyExpended, newOppWorld);
			newPlayerWorld.updateAllFacilities();
			newOppWorld.updateAllFacilities();
		}
		else {
			if (energyDiff > 0) {
				newPlayerWorld.Energy = energyDiff;
				newPlayerWorld.Location = playerNewLoc;
				newOppWorld.Energy = 0;
				newOppWorld.Location = Facility.NONE;
				if (energyDiff > 10) {
					newOppWorld.Lives -= 1;
				}
			}
			else if (energyDiff < 0) {
				energyDiff = -energyDiff;
				newPlayerWorld.Energy = 0;
				newPlayerWorld.Location = Facility.NONE;
				newOppWorld.Energy = energyDiff;
				newOppWorld.Location = oppNewLoc;
				if (energyDiff > 10) {
					newPlayerWorld.Lives -= 1;
				}
			}
			else {
				newPlayerWorld.Energy = 0;
				newPlayerWorld.Location = Facility.NONE;
				newOppWorld.Energy = 0;
				newOppWorld.Location = Facility.NONE;
			}
			newPlayerWorld.updateAllFacilities();
			newOppWorld.updateAllFacilities();
		}
		currentNode.child = new TimeNode(currentNode, newPlayerWorld, newOppWorld);
		currentNode = currentNode.child;

		Master.getInstance().Log(playerMove.ToString() + " & " + oppMove.ToString());
		playerMove = null;
		oppMove = null;
		if (worldUpdate != null) {
			worldUpdate();
		}
	}

	void updateWorld(Facility newLoc, int energyExpended, WorldState world)
	{
		if (world.Location == newLoc) {
			world.Energy -= energyExpended;
			world.FacilityDatas [newLoc].inputEnergy(energyExpended);
			if (newLoc == Facility.POWER && energyExpended != -(world.FacilityDatas [Facility.POWER].energyProperty)) {
				world.FacilityDatas [Facility.POWER].state = WorldState.FacilityData.FacilityState.IDLE;
			}
		}
		else {
			world.Location = newLoc;
		}
	}
	
	void applySettings(Dictionary<string, object> properties)
	{
		int initialLives = int.Parse(properties ["Lives"].ToString());
		Facility p1Facility = (Facility)(int.Parse(properties ["First player first facility"].ToString()));
		Facility p2Facility = (Facility)(int.Parse(properties ["Second player first facility"].ToString()));
		WorldState p1World = new WorldState(initialLives, p1Facility);
		WorldState p2World = new WorldState(initialLives, p2Facility);
		if (properties ["First player"].ToString() == "true") {
			currentNode = new TimeNode(null, p1World, p2World);
		}
		else {
			currentNode = new TimeNode(null, p2World, p1World);
		}
		Master.getInstance().Log("Room initialized. Lives = " + initialLives);
	}

	void onPause(bool paused)
	{
		this.gameObject.SetActive(!paused);
		if (!paused && Master.getInstance().multiplayer && resendBackupMove != null) {
			Master.getInstance().Log("Move resent");
			AppWarp.reSubmitMove(resendBackupMove);
		}
	}
	#region TimeNode
	public class TimeNode
	{
		public TimeNode parent;
		public TimeNode child;
		public TimeNode branchChild;
		public WorldState playerWorld;
		public WorldState oppWorld;

		public TimeNode(TimeNode parentNode, WorldState playerWorldObj, WorldState oppWorldObj)
		{
			parent = parentNode;
			playerWorld = playerWorldObj;
			oppWorld = oppWorldObj;
		}

		public bool isBranch()
		{
			return (branchChild != null);
		}

		public bool isHead()
		{
			if (parent != null) {
				return parent.branchChild == this;
			}
			else {
				return true;
			}
		}

		public TimeNode getParallelNode()
		{
			int dist = 0;
			TimeNode tempNode = this;
			while (!tempNode.isHead()) {
				tempNode = tempNode.parent;
				dist++;
			}
			if (tempNode.parent == null) {
				return null;
			}
			else {
				tempNode = tempNode.parent;
				dist++;
				while (dist > 0) {
					tempNode = tempNode.child;
					if (tempNode == null) {
						return null;
					}
				}
				return tempNode;
			}
		}
	}
	#endregion

	#region WorldState
	public class WorldState
	{
		public Facility Location;
		public int Energy;
		public int Lives;
		public Dictionary<Facility, FacilityData> FacilityDatas;
		public bool TimelineReportSent;
		
		const int INITIAL_ENERGY_LEVEL = 0;
		const int SERVER_REQUIREMENT_1 = 128;
		const int SERVER_REQUIREMENT_2 = 256;
		const int POWER_STORED_INITIAL = 10;
		const int POWER_INCREMENT = 10;
		const int REDIRECTOR_COST = 2;
		const int SEISMOMETER_COST = 10;
		Facility NextUnlockedFacility;

		public WorldState(int initialLives, Facility nextUnlocked)
		{
			Location = Facility.NONE;
			Energy = INITIAL_ENERGY_LEVEL;
			Lives = initialLives;
			FacilityDatas = makeInitialFacilityDatas();
			NextUnlockedFacility = nextUnlocked;
			TimelineReportSent = false;
		}

		WorldState()
		{
		}

		public WorldState makeDuplicate()
		{
			WorldState output = new WorldState();
			output.Location = Location;
			output.Energy = Energy;
			output.Lives = Lives;
			output.FacilityDatas = duplicateFacilityDatas(FacilityDatas);
			output.NextUnlockedFacility = NextUnlockedFacility;
			output.TimelineReportSent = false;// Not to be duplicated
			return output;
		}

		Dictionary<Facility, FacilityData> makeInitialFacilityDatas()
		{
			Dictionary<Facility, FacilityData> tempFacilityDatas = new Dictionary<Facility, FacilityData>();
			tempFacilityDatas = new Dictionary<Facility, FacilityData>();
			tempFacilityDatas.Add(Facility.SERVER, 
				new FacilityData(FacilityData.FacilityState.ACTIVE, 0, SERVER_REQUIREMENT_1));
			tempFacilityDatas.Add(Facility.POWER, 
		 		new FacilityData(FacilityData.FacilityState.IDLE, POWER_STORED_INITIAL, POWER_STORED_INITIAL / POWER_INCREMENT + 1));
			tempFacilityDatas.Add(Facility.REDIRECTOR, 
				new FacilityData(FacilityData.FacilityState.LOCKED, 0, REDIRECTOR_COST));
			tempFacilityDatas.Add(Facility.SEISMOMETER, 
			 	new FacilityData(FacilityData.FacilityState.LOCKED, 0, SEISMOMETER_COST));
			return tempFacilityDatas;
		}

		Dictionary<Facility, FacilityData> duplicateFacilityDatas(Dictionary<Facility, FacilityData> input)
		{
			Dictionary<Facility, FacilityData> output = new Dictionary<Facility, FacilityData>();
			foreach (KeyValuePair<Facility, FacilityData> entry in input) {
				output.Add(entry.Key, entry.Value.makeDuplicate());
			}
			return output;
		}

		public void updateAllFacilities()
		{
			bool facilityUnlocked = false;
			foreach (KeyValuePair<Facility, FacilityData> entry in FacilityDatas) {
				updateFacility(entry.Key, entry.Value);
				if (entry.Key == Facility.SERVER && entry.Value.energyProperty <= 0) {
					entry.Value.storedEnergy = 0;
					entry.Value.energyProperty = SERVER_REQUIREMENT_2;
					facilityUnlocked = true;
				}
			}
			if (facilityUnlocked) {
				FacilityDatas [NextUnlockedFacility].state = FacilityData.FacilityState.IDLE;
				if (NextUnlockedFacility == Facility.SEISMOMETER) {
					FacilityDatas [Facility.SERVER].state = FacilityData.FacilityState.LOCKED;
				}
				NextUnlockedFacility = Facility.SEISMOMETER;
			}
		}

		void updateFacility(Facility facility, FacilityData data)
		{
			switch (facility) {
				case Facility.SERVER:
					data.energyProperty -= data.storedEnergy;
					break;
				case Facility.POWER:
					if (data.state == FacilityData.FacilityState.ACTIVE) {
						data.storedEnergy += data.energyProperty;
						data.state = FacilityData.FacilityState.IDLE;
					}
					data.energyProperty = (int)(data.storedEnergy / POWER_INCREMENT) + 1;
					break;
				case Facility.REDIRECTOR:
					if (data.state == FacilityData.FacilityState.LOCKED) {
						break;
					}
					if (data.storedEnergy >= data.energyProperty) {
						data.storedEnergy -= data.energyProperty;
					}
					else {
						data.state = FacilityData.FacilityState.IDLE;
					}
					break;
				case Facility.SEISMOMETER:
					if (data.state == FacilityData.FacilityState.LOCKED) {
						break;
					}
					if (data.storedEnergy == data.energyProperty) {
						data.storedEnergy = 0;
					}
					else {
						data.state = FacilityData.FacilityState.IDLE;
					}
					break;
			}
		}
		
		public override string ToString()
		{
			string[] labelItems = {
				"Lives: ", 
				Lives.ToString(),
				"\nEnergy: ", 
				Energy.ToString(),
				"\nLocation: ", 
				Location.ToString(),
				"\nServer:", 
				FacilityDatas [Facility.SERVER].ToString(), 
				"\nPower:", 
				FacilityDatas [Facility.POWER].ToString(), 
				"\nRedirector:", 
				FacilityDatas [Facility.REDIRECTOR].ToString(),
				"\nSeismometer:", 
				FacilityDatas [Facility.SEISMOMETER].ToString(),
				"\nNext to be unlocked:", 
				NextUnlockedFacility.ToString(), 
			};
			return string.Concat(labelItems);
		}
		
		public class FacilityData
		{
			public enum FacilityState
			{
				LOCKED,
				IDLE,
				ACTIVE,
			}
			;
			public FacilityState state;
			public int storedEnergy;
			public int energyProperty;

			public FacilityData(FacilityState newState, int newEnergy, int newProperty)
			{
				state = newState;
				storedEnergy = newEnergy;
				energyProperty = newProperty;
			}

			public FacilityData makeDuplicate()
			{
				return new FacilityData(state, storedEnergy, energyProperty);
			}

			public void inputEnergy(int input)
			{
				if (state != FacilityState.LOCKED && input != 0) {
					storedEnergy += input;
					state = FacilityState.ACTIVE;
				}
			}

			public override string ToString()
			{
				return state.ToString() + " " + storedEnergy.ToString() + "/" + energyProperty.ToString();
			}
		}
	}
	#endregion
		
	#region MoveData
	public class MoveData
	{
		public Facility location;
		public int energyExpended;
		public int timeDist;
		
		public MoveData(Facility loc, int op, int t)
		{
			location = loc;
			energyExpended = op;
			timeDist = t;
		}
		
		public MoveData(Facility loc)
		{
			location = loc;
			energyExpended = 0;
			timeDist = 0;
		}
		
		public override string ToString()
		{
			return (Facility)location + ", " + energyExpended + ", " + timeDist;
		}
	}
	#endregion
}

