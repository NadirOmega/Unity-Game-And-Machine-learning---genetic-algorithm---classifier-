﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexas;

namespace Characters {
	
public enum CharClass : byte {GUERRIER,VOLEUR,ARCHER,MAGE,SOIGNEUR,ENVOUTEUR};

public class CharsDB {
	
	public enum AttackEffect : byte {DAMAGE,HEAL,PA_BUFF};
	
	// Attack 
	public class Attack {
		public int range;
		public int rangeAoE;
		public bool targetsEnemies;
		public bool targetsAllies;
		public bool targetsSelf;
		public AttackEffect attackEffect;
		public int effectValue;
		
		public Attack(int range,int rangeAoE,bool targetsEnemies,bool targetsAllies,bool targetsSelf,AttackEffect attackEffect,int effectValue){
			this.range = range;
			this.rangeAoE = rangeAoE;
			this.targetsEnemies = targetsEnemies;
			this.targetsAllies = targetsAllies;
			this.targetsSelf = targetsSelf;
			this.attackEffect = attackEffect;
			this.effectValue = effectValue;
		}
	}
	// Char base stats per class and attacks
	public class CharacterDB {
		public int maxHP;
		public int basePA;
		public int basePM;
		public Attack basicAttack;
		public Attack skill_1;
		
		public CharacterDB(int maxHP,int basePA,int basePM,Attack basicAttack,Attack skill_1){
			this.maxHP  = maxHP;
			this.basePA = basePA;
			this.basePM = basePM;
			this.basicAttack = basicAttack;
			this.skill_1 = skill_1;
		}
	}
	// base stats list
	public static List<CharacterDB> list;
	
	public static void initCharsDB(){
		list = new List<CharacterDB>();
		list.Add(new CharacterDB(17,2,3,new Attack( 1,0,true ,false,false,AttackEffect.DAMAGE ,3),new Attack( 1,0,true ,false,false,AttackEffect.DAMAGE ,5))); // GUERRIER (competence ?)
		list.Add(new CharacterDB(12,3,3,new Attack( 1,0,true ,false,false,AttackEffect.DAMAGE ,2),new Attack( 1,0,true ,false,false,AttackEffect.DAMAGE ,4))); // VOLEUR (competence ?)
		list.Add(new CharacterDB(13,2,3,new Attack( 7,0,true ,false,false,AttackEffect.DAMAGE ,2),new Attack(10,0,true ,false,false,AttackEffect.DAMAGE ,5))); // ARCHER
		list.Add(new CharacterDB(10,2,3,new Attack( 3,1,true ,false,false,AttackEffect.DAMAGE ,3),new Attack( 5,2,true ,false,false,AttackEffect.DAMAGE ,3))); // MAGE (attaques ne marchent pas)
		list.Add(new CharacterDB(13,2,3,new Attack( 4,0,false,true ,false,AttackEffect.HEAL   ,3),new Attack( 5,1,false,true ,false,AttackEffect.HEAL   ,3))); // SOIGNEUR (competence ?)
		list.Add(new CharacterDB(11,2,3,new Attack( 4,0,false,true ,false,AttackEffect.PA_BUFF,1),new Attack( 4,2,false,true ,false,AttackEffect.PA_BUFF,1))); // ENVOUTEUR
	}
}

public class Character {
	public static GameObject characterTemplate;
	public static List<GameObject> characterTemplateModels;
	public static GameObject charactersFolder ;
	public static Color TEAM_1_COLOR = new Color(1,0.125f,0);
	public static Color TEAM_2_COLOR = new Color(0.125f,0.125f,1);
	
	public CharClass charClass;
	public int team;
	public int HPmax;
	public int HP;
	public int PA;
	public int PM;
	public int x;
	public int y;
	public bool skillAvailable;
	public HexaDirection directionFacing;
	public GameObject go;
	
	public Character(CharClass charClass,int x,int y,int team){
		this.charClass = charClass;
		CharsDB.CharacterDB myCharClass = CharsDB.list[(int)charClass];
		HPmax = myCharClass.maxHP; HP = HPmax;
		PA = myCharClass.basePA;
		PM = myCharClass.basePM;
		
		this.x = x;
		this.y = y;
		this.team = team;
		this.skillAvailable = true;
		
		this.go = GameObject.Instantiate(characterTemplate,charactersFolder.transform);
		this.go.SetActive(true);
		this.go.transform.position = Hexa.hexaPosToReal(x,y,0);
		this.go.GetComponent<CharacterGO>().character = this;
		this.setColorByClass();
		this.setDirection(HexaDirection.DOWN);
	}
	
	// No GameObject (console mode)
	public Character(CharClass charClass,int x,int y,int team,bool a){
		this.charClass = charClass;
		CharsDB.CharacterDB myCharClass = CharsDB.list[(int)charClass];
		HPmax = myCharClass.maxHP; HP = HPmax;
		PA = myCharClass.basePA;
		PM = myCharClass.basePM;
		
		this.x = x;
		this.y = y;
		this.team = team;
		this.skillAvailable = true;
		
		this.go = null;
	}
	
	public void updatePos(int newX,int newY,HexaGrid hexaGrid){
		hexaGrid.getHexa(x,y).charOn = null;
		x = newX;
		y = newY;
		hexaGrid.getHexa(x,y).charOn = this;
		this.go.transform.position = Hexa.hexaPosToReal(x,y,0);
	}
	
	// Console mode
	public void updatePos2(int newX,int newY,HexaGrid hexaGrid){
		hexaGrid.getHexa(x,y).charOn = null;
		x = newX;
		y = newY;
		hexaGrid.getHexa(x,y).charOn = this;
	}
	
	public void setColorByClass(){
		/*switch (this.charClass){
			case CharClass.GUERRIER  : this.go.GetComponent<Renderer>().material.color = new Color(1,0,0); break; // GUERRIER : red
			case CharClass.VOLEUR    : this.go.GetComponent<Renderer>().material.color = new Color(0.4f,0.4f,0.4f); break; // VOLEUR : gray
			case CharClass.ARCHER    : this.go.GetComponent<Renderer>().material.color = new Color(1,1,0); break; // ARCHER : orange
			case CharClass.MAGE      : this.go.GetComponent<Renderer>().material.color = new Color(0.5f,0,1); break; // MAGE : purple
			case CharClass.SOIGNEUR  : this.go.GetComponent<Renderer>().material.color = new Color(1,0.5f,0.7f); break; // SOIGNEUR : pink
			case CharClass.ENVOUTEUR : this.go.GetComponent<Renderer>().material.color = new Color(1,0,1); break; // ENVOUTEUR : ugly magenta
		}*/
		switch (team){
			case 0 : this.go.transform.GetChild(0).GetComponent<Renderer>().material.color = TEAM_1_COLOR; break;
			case 1 : this.go.transform.GetChild(0).GetComponent<Renderer>().material.color = TEAM_2_COLOR; break;
			default : break;
		}
		GameObject.Instantiate(characterTemplateModels[(int)this.charClass],go.transform);
	}
	
	public void setDirection(HexaDirection newDirection){
		this.directionFacing = newDirection;
		Transform charModel = this.go.transform.GetChild(1);
		if (charModel) charModel.eulerAngles = new Vector3(0,(int)newDirection * 60,0);
	}
	
	public string getName(){
		switch (this.charClass){
			case CharClass.GUERRIER  : return "Guerrier";
			case CharClass.VOLEUR    : return "Voleur";
			case CharClass.ARCHER    : return "Archer";
			case CharClass.MAGE      : return "Mage";
			case CharClass.SOIGNEUR  : return "Soigneur";
			case CharClass.ENVOUTEUR : return "Envouteur";
			default : return "None";
		}
	}
	
	public CharsDB.CharacterDB getClassData(){
		return CharsDB.list[(int)charClass];
	}
}
	
}