using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Characters;
using AI_Class;
using Classifiers;

public enum PlayerType : byte {HUMAN,AI_EASY,AI_MEDIUM,AI_HARD};

// Data structure used to send game info when switching scenes
public class StartGameData{
	public bool loadSave;
	public List<CharClass> charsTeam1 = new List<CharClass>();
	public List<CharClass> charsTeam2 = new List<CharClass>();
	public PlayerType player1Type;
	public PlayerType player2Type;
	public int mapChosen; // 0 = Ruins, 1 = Random
	public int nbGames;
	public StartGameData(){}
}

public class MainMenu : MonoBehaviour
{
	public GameObject mainMenu;
	public GameObject CharSelectMenu;
	public GameObject advancedOptionsMenu;
	public List<Texture> charCards;
	public GameObject charsTeam1Display;
	public GameObject charsTeam2Display;
	public GameObject teamSelectHighlight;
	public GameObject credits;
	// Main menu buttons
	public Button buttonPlay;
	public Button buttonLoad;
	public Button buttonQuit;
	public Button buttonStats;
	public Button buttonCredits;
	// Char select menu buttons
	public List<Button> buttonCharCards;
	public List<Button> buttonTeam1Cards;
	public List<Button> buttonTeam2Cards;
	public Button buttonBackTeam1;
	public Button buttonBackTeam2;
	public Button buttonReadyTeam1;
	public Button buttonReadyTeam2;
	public Button buttonToAdvancedOptions;
	// Advanced options menu buttons
	public Button buttonToCharSelect;
	public Toggle toggleConsoleMode;
	public Slider sliderNbGames;
	public GameObject textNbGames;
	
	bool buttonPlayPressed = false;
	bool buttonLoadPressed = false;
	bool buttonQuitPressed = false;
	bool buttonStatsPressed = false;
	bool buttonCreditsPressed = false;
	bool[] buttonCharCardsPressed = new bool[6] {false,false,false,false,false,false};
	bool[] buttonTeam1CardsPressed = new bool[5] {false,false,false,false,false};
	bool[] buttonTeam2CardsPressed = new bool[5] {false,false,false,false,false};
	bool buttonBackTeam1Pressed = false;
	bool buttonBackTeam2Pressed = false;
	bool buttonReadyTeam1Pressed = false;
	bool buttonReadyTeam2Pressed = false;
	bool buttonToAdvancedOptionsPressed = false;
	bool buttonToCharSelectPressed = false;
	
	public Dropdown dropdownPlayer1Type;
	public Dropdown dropdownPlayer2Type;
	public Dropdown dropdownMap;
	PlayerType player1Type;
	PlayerType player2Type;
	
	List<CharClass> charsTeam1 = new List<CharClass>();
	List<CharClass> charsTeam2 = new List<CharClass>();
	int currentTeam = 0;
	bool consoleMode;
	int nbGames;
	
    // Start is called before the first frame update
    void Start(){
		mainMenu.SetActive(true);
        buttonPlay.onClick.AddListener(buttonPlayPressed_);
		buttonLoad.onClick.AddListener(buttonLoadPressed_);
		buttonQuit.onClick.AddListener(buttonQuitPressed_);
		buttonStats.onClick.AddListener(buttonStatsPressed_);
		buttonCredits.onClick.AddListener(buttonCreditsPressed_);
		
		charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);
		
		buttonCharCards[0].onClick.AddListener(() => buttonCharCardsPressed_(0));
		buttonCharCards[1].onClick.AddListener(() => buttonCharCardsPressed_(1));
		buttonCharCards[2].onClick.AddListener(() => buttonCharCardsPressed_(2));
		buttonCharCards[3].onClick.AddListener(() => buttonCharCardsPressed_(3));
		buttonCharCards[4].onClick.AddListener(() => buttonCharCardsPressed_(4));
		buttonCharCards[5].onClick.AddListener(() => buttonCharCardsPressed_(5));
		
		buttonTeam1Cards[0].onClick.AddListener(() => buttonTeam1CardsPressed_(0));
		buttonTeam1Cards[1].onClick.AddListener(() => buttonTeam1CardsPressed_(1));
		buttonTeam1Cards[2].onClick.AddListener(() => buttonTeam1CardsPressed_(2));
		buttonTeam1Cards[3].onClick.AddListener(() => buttonTeam1CardsPressed_(3));
		buttonTeam1Cards[4].onClick.AddListener(() => buttonTeam1CardsPressed_(4));
		buttonTeam2Cards[0].onClick.AddListener(() => buttonTeam2CardsPressed_(0));
		buttonTeam2Cards[1].onClick.AddListener(() => buttonTeam2CardsPressed_(1));
		buttonTeam2Cards[2].onClick.AddListener(() => buttonTeam2CardsPressed_(2));
		buttonTeam2Cards[3].onClick.AddListener(() => buttonTeam2CardsPressed_(3));
		buttonTeam2Cards[4].onClick.AddListener(() => buttonTeam2CardsPressed_(4));
		
		buttonBackTeam1.onClick.AddListener(buttonBackTeam1Pressed_);
		buttonBackTeam2.onClick.AddListener(buttonBackTeam2Pressed_);
		buttonReadyTeam1.onClick.AddListener(buttonReadyTeam1Pressed_);
		buttonReadyTeam2.onClick.AddListener(buttonReadyTeam2Pressed_);
		buttonToAdvancedOptions.onClick.AddListener(buttonToAdvancedOptionsPressed_);
		
		buttonToCharSelect.onClick.AddListener(buttonToCharSelectPressed_);
		// Init AIHard classifiers
		if (AIHard.rules == null){
            //AIHard.rules = new ClassifierSystem(DB.connectBDD());
			AIHard.rules = new ClassifierSystem();
			AIHard.rules.loadAllInBinary("Data/Classifiers/test");
			Debug.Log("Loaded " + AIHard.rules.classifiers.Count + " Classifiers.");
		}
		
		consoleMode = true;
		nbGames = 1;
		// Read Options from options file
		if (File.Exists("Data/Options/options")){
			loadOptions();
		}else{
			saveOptions();
		}
		toggleConsoleMode.isOn = consoleMode;
		sliderNbGames.value = Mathf.Sqrt((float)nbGames);
    }

    // Update is called once per frame
    void Update(){
		// MAIN MENU
		
		// Go to character selection menu
		if (buttonPlayPressed){
			credits.SetActive(false);
			mainMenu.SetActive(false);
			CharSelectMenu.SetActive(true);
			initCharSelectMenu();
			buttonPlayPressed = false;
		}
		// Load saved game
		if (buttonLoadPressed){
			if (File.Exists("Data/Save/gameSave")){
				MainGame.startGameData = new StartGameData();
				MainGame.startGameData.loadSave = true;
				SceneManager.LoadScene(1);
			}
			buttonLoadPressed = false;
		}
		// Show credits
		if (buttonCreditsPressed){
			credits.SetActive(!credits.activeInHierarchy);
			buttonCreditsPressed = false;
		}
		if (buttonStatsPressed){
			SceneManager.LoadScene(2);
			buttonStatsPressed = false;
		}
		// Quit
		if (buttonQuitPressed){
            //AIHard.rules.saveInDB();
			AIHard.rules.saveAllInBinary("Data/Classifiers/test");
			Application.Quit();
			Debug.Log("Can't quit");
			buttonQuitPressed = false;
		}
        
		// *************************
		// CHARACTER SELECTION MENU
		// *************************
		if (CharSelectMenu.activeInHierarchy){
			// Back to main menu
			if (buttonBackTeam1Pressed){
				mainMenu.SetActive(true);
				CharSelectMenu.SetActive(false);
				buttonBackTeam1Pressed = false;
			}
			
			// Back to team 1
			if (buttonBackTeam2Pressed){
				charSelectMenuPreviousPlayer();
				buttonBackTeam2Pressed = false;
			}
			
			{
				List<CharClass> charsTeam = (currentTeam == 0) ? charsTeam1 : charsTeam2;
				GameObject charsTeamDisplay = (currentTeam == 0) ? charsTeam1Display : charsTeam2Display;
				bool[] buttonTeamCardsPressed = (currentTeam == 0) ? buttonTeam1CardsPressed : buttonTeam2CardsPressed;
				
				
				// CHARACTER SELECTION
				for (int i=0;i<6;i++){
					if (buttonCharCardsPressed[i]){
						if (charsTeam.Count < 5){
							charsTeam.Add((CharClass)i);
							charsTeamDisplay.transform.GetChild(0).transform.GetChild(charsTeam.Count-1).GetComponent<RawImage>().texture = charCards[i];
						}
						
						buttonCharCardsPressed[i] = false;
					}
				}
			
				// REMOVE CHARACTER FROM TEAM
				for (int i=0;i<5;i++){
					if (buttonTeamCardsPressed[i]){
						if (charsTeam.Count > i){
							charsTeam.RemoveAt(i);
							for (int j=i;j<charsTeam.Count;j++){
								charsTeamDisplay.transform.GetChild(0).transform.GetChild(j).GetComponent<RawImage>().texture = charCards[(int)charsTeam[j]];
							}
							charsTeamDisplay.transform.GetChild(0).transform.GetChild(charsTeam.Count).GetComponent<RawImage>().texture = charCards[6];
						}
						buttonTeamCardsPressed[i] = false;
					}
				}
				
				// READY
				if (charsTeam.Count > 0){
					if (currentTeam == 0) charsTeam1Display.transform.GetChild(2).gameObject.SetActive(true);
					else charsTeam2Display.transform.GetChild(2).gameObject.SetActive(true);
				}else{
					if (currentTeam == 0) charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);
					else charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
				}
				if (buttonReadyTeam1Pressed){
					charSelectMenuNextPlayer();
					buttonReadyTeam1Pressed = false;
				}else if (buttonReadyTeam2Pressed){
					Debug.Log("Start : P1 " + (PlayerType)dropdownPlayer1Type.value + " P2 " + (PlayerType)dropdownPlayer2Type.value);
					// Give Main all the info
					MainGame.startGameData = new StartGameData();
					MainGame.startGameData.loadSave = false;
					MainGame.startGameData.charsTeam1 = charsTeam1;
					MainGame.startGameData.charsTeam2 = charsTeam2;
					MainGame.startGameData.player1Type = (PlayerType)dropdownPlayer1Type.value;
					MainGame.startGameData.player2Type = (PlayerType)dropdownPlayer2Type.value;
					MainGame.startGameData.mapChosen = dropdownMap.value;
					MainGame.startGameData.nbGames = nbGames;
					buttonReadyTeam2Pressed = false;
					if (MainGame.startGameData.player1Type != PlayerType.HUMAN && MainGame.startGameData.player2Type != PlayerType.HUMAN && consoleMode){
						// Load Console Mode scene
						SceneManager.LoadScene(3);
					}else{
						SceneManager.LoadScene(1);
					}
				}
				
				if (buttonToAdvancedOptionsPressed){
					advancedOptionsMenu.SetActive(true);
					CharSelectMenu.SetActive(false);
					buttonToAdvancedOptionsPressed = false;
				}
			}
		}else if (advancedOptionsMenu.activeInHierarchy){
			if (buttonToCharSelectPressed){
				advancedOptionsMenu.SetActive(false);
				CharSelectMenu.SetActive(true);
				buttonToCharSelectPressed = false;
				saveOptions();
			}
			consoleMode = toggleConsoleMode.isOn;
			nbGames = (int)(sliderNbGames.value*sliderNbGames.value);
			textNbGames.GetComponent<Text>().text = "(IA vs IA) nombre de parties : " + nbGames;
		}
    }
	
	void initCharSelectMenu(){
		charsTeam1 = new List<CharClass>();
		charsTeam2 = new List<CharClass>();
		for (int i=0;i<5;i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<RawImage>().texture = charCards[6];
		for (int i=0;i<5;i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<RawImage>().texture = charCards[6];
		
		for (int i=0;i<5;i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;
		for (int i=0;i<5;i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;
		
		charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);
		charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);
		
		charsTeam2Display.transform.GetChild(1).gameObject.SetActive(false);
		charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
	}
	
	void charSelectMenuNextPlayer(){
		currentTeam = 1;
		for (int i=0;i<5;i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;
		for (int i=0;i<5;i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;
		
		charsTeam1Display.transform.GetChild(1).gameObject.SetActive(false);
		charsTeam1Display.transform.GetChild(2).gameObject.SetActive(false);
		
		charsTeam2Display.transform.GetChild(1).gameObject.SetActive(true);
		charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
		
		teamSelectHighlight.transform.localPosition = new Vector3(0,36-183,0);
	}
	
	void charSelectMenuPreviousPlayer(){
		currentTeam = 0;
		for (int i=0;i<5;i++) charsTeam1Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = true;
		for (int i=0;i<5;i++) charsTeam2Display.transform.GetChild(0).transform.GetChild(i).GetComponent<Button>().enabled = false;
		
		charsTeam1Display.transform.GetChild(1).gameObject.SetActive(true);
		charsTeam1Display.transform.GetChild(2).gameObject.SetActive(true);
		
		charsTeam2Display.transform.GetChild(1).gameObject.SetActive(false);
		charsTeam2Display.transform.GetChild(2).gameObject.SetActive(false);
		
		teamSelectHighlight.transform.localPosition = new Vector3(0,36,0);
	}
	
	void saveOptions(){
		using (BinaryWriter writer = new BinaryWriter(File.Open("Data/Options/options",FileMode.Create))){
			writer.Write((int)((consoleMode) ? 1 : 0));
			writer.Write(nbGames);
		}
	}
	
	void loadOptions(){
		using (BinaryReader reader = new BinaryReader(File.Open("Data/Options/options",FileMode.Open))){
			consoleMode = (reader.ReadInt32() == 0) ? false : true;
			nbGames = reader.ReadInt32();
		}
	}
	
	// Events
	void buttonPlayPressed_(){buttonPlayPressed = true;}
	void buttonLoadPressed_(){buttonLoadPressed = true;}
	void buttonQuitPressed_(){buttonQuitPressed = true;}
	void buttonStatsPressed_(){buttonStatsPressed = true;}
	void buttonCreditsPressed_(){buttonCreditsPressed = true;}
	void buttonCharCardsPressed_(int i){buttonCharCardsPressed[i] = true;}
	void buttonTeam1CardsPressed_(int i){buttonTeam1CardsPressed[i] = true;}
	void buttonTeam2CardsPressed_(int i){buttonTeam2CardsPressed[i] = true;}
	void buttonBackTeam1Pressed_(){buttonBackTeam1Pressed = true;}
	void buttonBackTeam2Pressed_(){buttonBackTeam2Pressed = true;}
	void buttonReadyTeam1Pressed_(){buttonReadyTeam1Pressed = true;}
	void buttonReadyTeam2Pressed_(){buttonReadyTeam2Pressed = true;}
	void buttonToAdvancedOptionsPressed_(){buttonToAdvancedOptionsPressed = true;}
	void buttonToCharSelectPressed_(){buttonToCharSelectPressed = true;}
}
