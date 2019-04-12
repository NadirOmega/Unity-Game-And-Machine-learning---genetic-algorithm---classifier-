﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Misc;
using Hexas;
using Characters;
using AI_Util;
using AI_Class;
using Classifiers;
using Stats;
using static MainMenu;

public class MainGameConsole : MonoBehaviour {
	
	public GameObject textDisplay;
	public GameObject textDisplay2;
	public int nbGames;
	public int currentNbGames;
	
	public int tileW;
	public int tileH;
	public HexaGrid hexaGrid;
	public Character currentCharControlled;
	public int currentCharControlledID;
	public int winner;
	public StatsGame statsGame;
	public List<ActionAIPos> decisionSequence;
	
	public int t1Wins;
	public int t2Wins;
	
    // Start is called before the first frame update
    void Start(){
		CharsDB.initCharsDB();
        // Init game data if it's not (it should be in the main menu)
		if (MainGame.startGameData == null){
			MainGame.startGameData = new StartGameData();
			MainGame.startGameData.loadSave = false;
			MainGame.startGameData.charsTeam1 = new List<CharClass>();
			MainGame.startGameData.charsTeam1.Add(CharClass.VOLEUR);
			MainGame.startGameData.charsTeam1.Add(CharClass.GUERRIER);
			MainGame.startGameData.charsTeam2 = new List<CharClass>();
			MainGame.startGameData.charsTeam2.Add(CharClass.SOIGNEUR);
			MainGame.startGameData.charsTeam2.Add(CharClass.ARCHER);
			MainGame.startGameData.player1Type = PlayerType.AI_HARD;
			MainGame.startGameData.player2Type = PlayerType.AI_EASY;
			MainGame.startGameData.mapChosen = 1;
			MainGame.startGameData.nbGames = 10;
		}
		
		// Init hexa grid
		hexaGrid = new HexaGrid();
		if (MainGame.startGameData.mapChosen == 0){
			hexaGrid.createGridFromFile2("Data/Map/ruins");
			tileW = hexaGrid.w; tileH = hexaGrid.h;
		}else if (MainGame.startGameData.mapChosen >= 1){
			hexaGrid.createRandomRectGrid2(tileW,tileH);
		}
		// Put characters on the grid
		for (int i=0;i<5;i++){
			if (i < MainGame.startGameData.charsTeam1.Count) hexaGrid.addChar2(MainGame.startGameData.charsTeam1[i],tileW/2-4+2+i,2,0);
			if (i < MainGame.startGameData.charsTeam2.Count) hexaGrid.addChar2(MainGame.startGameData.charsTeam2[i],tileW/2-4+2+i,tileH-2,1);
		}
		foreach (Character c in hexaGrid.charList) hexaGrid.getHexa(c.x,c.y).changeType2(HexaType.GROUND);
		currentCharControlledID = 0;
		currentCharControlled = hexaGrid.charList[currentCharControlledID];
		currentNbGames = 0;
		nbGames = MainGame.startGameData.nbGames;
		
		// Init AI
		decisionSequence = new List<ActionAIPos>();
		AI.hexaGrid = hexaGrid;
		// Init AIHard classifiers
		if (AIHard.rules == null){
            AIHard.rules = new ClassifierSystem(DB.connectBDD());
			Debug.Log("Loaded " + AIHard.rules.classifiers.Count + " Classifiers.");
		}
		AIHard.learn = true;
		AIUtil.hexaGrid = hexaGrid;
		statsGame = new StatsGame();
		winner = -1;
		t1Wins = 0;
		t2Wins = 0;
    }

    // Update is called once per frame
    void Update(){
		for (int aaa=0;aaa<1;aaa++){
			if (winner == -1){
				if (aaa == 0) textDisplay.GetComponent<Text>().text = currentNbGames + " / " + nbGames;
				PlayerType currentPlayerType = whoControlsThisChar(currentCharControlled);
				// decide
				if (decisionSequence.Count == 0){
					switch (currentPlayerType){
						case PlayerType.HUMAN :
						case PlayerType.AI_EASY : decisionSequence = AIEasy.decide(currentCharControlledID); break;
						case PlayerType.AI_MEDIUM : decisionSequence = AIMedium.decide(currentCharControlledID); break;
						case PlayerType.AI_HARD : decisionSequence = AIHard.decide(currentCharControlledID,statsGame); break;
					}
				// action
				}else{
					ActionAIPos actionAIPos = decisionSequence[0]; decisionSequence.RemoveAt(0);
					Debug.Log(currentPlayerType + " : " + actionAIPos.action + ((actionAIPos.pos != null) ? (" " + actionAIPos.pos.x + " " + actionAIPos.pos.y) : ""));
					switch (actionAIPos.action){
						case MainGame.ActionType.MOVE : actionMove(hexaGrid.getHexa(actionAIPos.pos)); break;
						case MainGame.ActionType.ATK1 : case MainGame.ActionType.ATK2 : actionUseAttack(actionAIPos.action,hexaGrid.getHexa(actionAIPos.pos)); break;
						case MainGame.ActionType.SKIP : {
							currentCharControlled.PA = 0;
							nextTurn();
						} break;
					}
				}
			// end
			}else if (winner == 10){
				textDisplay.GetComponent<Text>().text = "VICTOIRE DE L'EQUIPE " + ((t1Wins > t2Wins) ? "ROUGE" : "BLEUE") + " (" + ((winner == 0) ? MainGame.startGameData.player1Type : MainGame.startGameData.player2Type) + ")";
				// A Key : Quit
				if (Input.GetKeyDown(KeyCode.A)){
					SceneManager.LoadScene(0);
				}
			// next game (reset)
			}else if (winner == 11){
				statsGame = new StatsGame();
				winner = -1;
				foreach (Character c in hexaGrid.charList) hexaGrid.getHexa(c.x,c.y).charOn = null;
				hexaGrid.charList = new List<Character>();
				// Put characters on the grid
				for (int i=0;i<5;i++){
					if (i < MainGame.startGameData.charsTeam1.Count) hexaGrid.addChar2(MainGame.startGameData.charsTeam1[i],tileW/2-4+2+i,2,0);
					if (i < MainGame.startGameData.charsTeam2.Count) hexaGrid.addChar2(MainGame.startGameData.charsTeam2[i],tileW/2-4+2+i,tileH-2,1);
				}
				foreach (Character c in hexaGrid.charList) hexaGrid.getHexa(c.x,c.y).changeType2(HexaType.GROUND);
				currentCharControlledID = 0;
				currentCharControlled = hexaGrid.charList[currentCharControlledID];
				decisionSequence = new List<ActionAIPos>();
				// A Key : Quit (hold)
				if (Input.GetKey(KeyCode.A)){
					SceneManager.LoadScene(0);
				}
			}else{
				// EVALUATE AI HARD
				statsGame.winner = winner;
				statsGame.evaluateGame();
				if (winner == 0) t1Wins++; else t2Wins++;
				currentNbGames++;
				
				if (currentNbGames == nbGames) winner = 10;
				else winner = 11;
			}
		}
		string s1 = "EQUIPE ROUGE" + " (" + MainGame.startGameData.player1Type + ") : " + t1Wins + " Wins";
		string s2 = "EQUIPE BLEUE" + " (" + MainGame.startGameData.player2Type + ") : " + t2Wins + " Wins";
		textDisplay2.GetComponent<Text>().text = "Nb Classifiers : " + AIHard.rules.classifiers.Count + "\n" + s1 + "\n" + s2;
    }
	
	// ##################################################################################################################################################
	// Functions used in main
	// ##################################################################################################################################################
	
	PlayerType whoControlsThisChar(Character c){
		return (c.team == 0) ? MainGame.startGameData.player1Type : MainGame.startGameData.player2Type;
	}
	
	void actionMove(Hexa hexaDestination){
		if (hexaDestination != null && hexaDestination.type == HexaType.GROUND){
			List<Point> path = hexaGrid.findShortestPath(currentCharControlled.x,currentCharControlled.y,hexaDestination.x,hexaDestination.y,currentCharControlled.PM);
			if (path != null && path.Count > 1){
				currentCharControlled.updatePos2(hexaDestination.x,hexaDestination.y,hexaGrid);
				nextTurn();
			}else Debug.LogWarning("ActionMove : error");
		}else Debug.LogWarning("ActionMove : error");
	}
	
	// must trust the AI to choose right
	void actionMoveNoCheck(Hexa hexaDestination){
		currentCharControlled.updatePos2(hexaDestination.x,hexaDestination.y,hexaGrid);
		nextTurn();
	}
	
	void actionUseAttack(MainGame.ActionType attack,Hexa hexaDestination){
		CharsDB.Attack attackUsed_;
		if (attack == MainGame.ActionType.ATK1) attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].basicAttack;
		else attackUsed_ = CharsDB.list[(int)currentCharControlled.charClass].skill_1;
		if (hexaDestination != null && hexaGrid.hexaInSight(currentCharControlled.x,currentCharControlled.y,hexaDestination.x,hexaDestination.y,attackUsed_.range)){
			if (attack == MainGame.ActionType.ATK2){
				currentCharControlled.skillAvailable = false;
			}
		}else Debug.LogWarning("ActionUseAttack : Error");
		
		List<Character> hits = hexaGrid.getCharWithinRange(hexaDestination.x,hexaDestination.y,attackUsed_.rangeAoE);
		// Filter target(s)
		if (attackUsed_.targetsEnemies == false){
			for (int i=0;i<hits.Count;i++){
				if (hits[i].team != currentCharControlled.team){
					hits.RemoveAt(i); i--;
				}
			}
		}
		if (attackUsed_.targetsAllies == false){
			for (int i=0;i<hits.Count;i++){
				if (hits[i].team == currentCharControlled.team){
					hits.RemoveAt(i); i--;
				}
			}
		}
		if (attackUsed_.targetsSelf == false){
			for (int i=0;i<hits.Count;i++){
				if (hits[i] == currentCharControlled){
					hits.RemoveAt(i); i--;
				}
			}
		}
		foreach (Character c in hits){
			switch (attackUsed_.attackEffect){
				case CharsDB.AttackEffect.DAMAGE  : {
					if (whoControlsThisChar(c) == PlayerType.AI_HARD) statsGame.addToDamageTaken(c,attackUsed_.effectValue);
					if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToDamageDealt(currentCharControlled,attackUsed_.effectValue);
					
					c.HP -= attackUsed_.effectValue;
					// Enemy dies
					if (c.HP <= 0){
						if (whoControlsThisChar(c) == PlayerType.AI_HARD) statsGame.setDead(c,true);
						if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToKills(currentCharControlled,1);
						c.HP = 0;
						hexaGrid.getHexa(c.x,c.y).charOn = null;
						GameObject.Destroy(c.go);
						for (int i=0;i<hexaGrid.charList.Count;i++){
							if (hexaGrid.charList[i] == c){
								hexaGrid.charList.RemoveAt(i);
							}
						}
						// update currentCharControlled ID
						for (int i=0;i<hexaGrid.charList.Count;i++){
							if (hexaGrid.charList[i] == currentCharControlled) currentCharControlledID = i;
						}
						// force AI to make a new decision
						decisionSequence = new List<ActionAIPos>();
						// check if there is a winner
						int nbT1 = 0;
						int nbT2 = 0;
						foreach (Character c2 in hexaGrid.charList){
							if (c2.team == 0) nbT1++;
							else nbT2++;
						}
						if (nbT1 == 0) winner = 1;
						else if (nbT2 == 0) winner = 0;
					}
				} break;
				case CharsDB.AttackEffect.HEAL    : {
					int heal = attackUsed_.effectValue;
					if (heal > c.HPmax - c.HP) heal = c.HPmax - c.HP;
					
					if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToDamageDealt(currentCharControlled,heal);
					
					c.HP += heal;
				} break;
				case CharsDB.AttackEffect.PA_BUFF : {
					if (c.PA == c.getClassData().basePA){
						
						if (whoControlsThisChar(currentCharControlled) == PlayerType.AI_HARD) statsGame.addToDamageDealt(currentCharControlled,attackUsed_.effectValue);
						
						c.PA += attackUsed_.effectValue;
					}
				} break;
			}
		}
		nextTurn();
	}
	
	void nextTurn(){
		currentCharControlled.PA--;
		// Next char turn
		if (currentCharControlled.PA <= 0){
			currentCharControlled.PA = CharsDB.list[(int)currentCharControlled.charClass].basePA;
			do {
				currentCharControlledID = (currentCharControlledID+1)%hexaGrid.charList.Count;
				currentCharControlled = hexaGrid.charList[currentCharControlledID];
			} while (currentCharControlled.HP <= 0);
			PlayerType currentPlayerType = whoControlsThisChar(currentCharControlled);
			if (currentPlayerType == PlayerType.AI_HARD){
				statsGame.nextTurn(currentCharControlled);
			}
			decisionSequence = new List<ActionAIPos>();
		}
	}
}
