﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hexas;
using Characters;
using AI_Util;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Text;

namespace Classifiers {

public class ClassifierAttributes {
	public enum CharClass : byte {GUERRIER = 1,VOLEUR = 2,ARCHER = 4,MAGE = 8,SOIGNEUR = 16,ENVOUTEUR = 32};
	public enum HP_ : byte {BETWEEN_100_75 = 1,BETWEEN_74_40 = 2,BETWEEN_39_0 = 4};
	public enum PA_ : byte {ONE = 1, TWO_OR_MORE = 2};
	public enum SkillAvailable : byte {YES = 1,NO = 2};
	public enum Threat : byte {SAFE = 1,DANGER = 2,DEATH = 4};
	public enum MaxTargets : byte {NONE = 1,ONE = 2,TWO = 4,THREE_OR_MORE = 8};
	
	public enum ClassType : byte {SOIGNEUR = 1,ENVOUTEUR = 2,OTHER = 4,NONE = 8};
	public enum Distance : byte {ATTACK = 1,SKILL = 2,ATTACK_AND_SKILL = 4,MOVEMENT = 8};
	
	public static CharClass getRandomCharClassValue(){
		var values = CharClass.GetValues(typeof(CharClass));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		CharClass randomVal = (CharClass)values.GetValue(nb); 	
		return randomVal;
	}
	public static HP_ getRandomHPValue(){
		var values = HP_.GetValues(typeof(HP_));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		HP_ randomVal = (HP_)values.GetValue(nb); 	
		return randomVal;
	}
	public static PA_ getRandomPAValue(){
		var values = PA_.GetValues(typeof(PA_));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		PA_ randomVal = (PA_)values.GetValue(nb); 	
		return randomVal;
	}

	public static SkillAvailable getRandomSkillValue(){
		var values = SkillAvailable.GetValues(typeof(SkillAvailable));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		SkillAvailable randomVal = (SkillAvailable)values.GetValue(nb); 	
		return randomVal;
	}

	public static Threat getRandomThreatValue(){
		var values = Threat.GetValues(typeof(Threat));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		Threat randomVal = (Threat)values.GetValue(nb); 	
		return randomVal;
	}
	
	/**mutate ally & Enemy*/
	public static ClassType getRandomCharTypeValue(byte OldValue)
	{
		var values = ClassType.GetValues(typeof(ClassType));
		System.Random random = new System.Random();
		
		ClassType randomVal;
		int nb ;
		do {
		nb = random.Next(0,values.Length);
		randomVal = (ClassType)values.GetValue(nb); 	
		}while(((byte)randomVal)==OldValue);

		return randomVal;
	}
}

public class Classifier {
    public int id;
    public bool modified;
	public byte charClass;
	public byte HP; 
	public byte PA;
	public byte skillAvailable;
	public byte threat;
	public byte maxTargets;
	
	public class InfoChars{
		public byte classType; // soigneur / envouteur / autres
		public byte HP;
		public byte threat;
		public byte distance;
		
		public InfoChars(Character c,bool isWithinAttackRange,bool isWithinSkillRange){
			// ClassType
			switch (c.charClass){
				case CharClass.GUERRIER  : 
				case CharClass.VOLEUR    :
				case CharClass.ARCHER    : 
				case CharClass.MAGE      : classType = (byte)ClassifierAttributes.ClassType.OTHER; break;
				case CharClass.SOIGNEUR  : classType = (byte)ClassifierAttributes.ClassType.SOIGNEUR; break;
				case CharClass.ENVOUTEUR : classType = (byte)ClassifierAttributes.ClassType.ENVOUTEUR; break;
			}
			
			// HP
			float HPprc = ((float)c.HP)/c.HPmax;
			if (HPprc >= 0.75){
				HP = (byte)ClassifierAttributes.HP_.BETWEEN_100_75;
			}else if (HPprc >= 0.4){
				HP = (byte)ClassifierAttributes.HP_.BETWEEN_74_40;
			}else{
				HP = (byte)ClassifierAttributes.HP_.BETWEEN_39_0;
			}
			
			// Threat
			int threat_int = AIUtil.calculateThreatAtHexa(c.x,c.y,c.team);
			if (c.HP - threat_int <= 0){
				threat = (byte)ClassifierAttributes.Threat.DEATH;
			}else if (threat_int <= 2){
				threat = (byte)ClassifierAttributes.Threat.SAFE;
			}else{
				threat = (byte)ClassifierAttributes.Threat.DANGER;
			}
			
			// Distance
			if (isWithinAttackRange){
				if (isWithinSkillRange){
					distance = (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL;
				}else{
					distance = (byte)ClassifierAttributes.Distance.ATTACK;
				}
			}else if (isWithinSkillRange){
				distance = (byte)ClassifierAttributes.Distance.SKILL;
			}else{
				distance = (byte)ClassifierAttributes.Distance.MOVEMENT;
			}
		}
		
		public InfoChars(){
			classType = (byte)ClassifierAttributes.ClassType.NONE;
			HP = 1+2+4;
			threat = 1+2+4;
			distance = 1+2+4+8;
		}
		
		public InfoChars(InfoChars i){
			this.classType = i.classType;
			this.HP = i.HP;
			this.threat = i.threat;
			this.distance = i.distance;
		}
		
		public bool equals(InfoChars i){
			return ((classType == i.classType) && (HP == i.HP) && (threat == i.threat) && (distance == i.distance));
		}
		
		public bool isSimilar(InfoChars i){
			return (((classType & i.classType) > 0) && ((HP & i.HP) > 0) && ((threat & i.threat) > 0) && ((distance & i.distance) > 0));
		}
	}
	
	public List<InfoChars> infoAllies;
	public List<InfoChars> infoEnemies;
	
	public enum Action : byte {ApproachEnemy , ApproachAlly , Flee , Attack , Skill};
	public Action action;
	
	public float fitness;
	public int useCount;
	
	/** Init the classifier from a character's situation. */
	public Classifier(HexaGrid hexaGrid,int charID){
		Character c = hexaGrid.charList[charID];
		
		// ClassType
		switch (c.charClass){
			case CharClass.GUERRIER  : charClass = (byte)ClassifierAttributes.CharClass.GUERRIER; break;
			case CharClass.VOLEUR    : charClass = (byte)ClassifierAttributes.CharClass.VOLEUR; break;
			case CharClass.ARCHER    : charClass = (byte)ClassifierAttributes.CharClass.ARCHER; break;
			case CharClass.MAGE      : charClass = (byte)ClassifierAttributes.CharClass.MAGE; break;
			case CharClass.SOIGNEUR  : charClass = (byte)ClassifierAttributes.CharClass.SOIGNEUR; break;
			case CharClass.ENVOUTEUR : charClass = (byte)ClassifierAttributes.CharClass.ENVOUTEUR; break;
		}
		
		// HP
		float HPprc = ((float)c.HP)/c.HPmax;
		if (HPprc >= 0.75){
			HP = (byte)ClassifierAttributes.HP_.BETWEEN_100_75;
		}else if (HPprc >= 0.4){
			HP = (byte)ClassifierAttributes.HP_.BETWEEN_74_40;
		}else{
			HP = (byte)ClassifierAttributes.HP_.BETWEEN_39_0;
		}
		
		// PA
		PA = (c.PA == 1) ? (byte)ClassifierAttributes.PA_.ONE : (byte)ClassifierAttributes.PA_.TWO_OR_MORE;
		
		// SkillAvailable
		skillAvailable = (c.skillAvailable) ? (byte)ClassifierAttributes.SkillAvailable.YES : (byte)ClassifierAttributes.SkillAvailable.NO;
		
		// Threat
		int threat_int = AIUtil.calculateThreatAtHexa(c.x,c.y,c.team); //Debug.Log("Threat : " + threat_int);
		if (c.HP - threat_int <= 0){
			threat = (byte)ClassifierAttributes.Threat.DEATH;
		}else if (threat_int <= 2){
			threat = (byte)ClassifierAttributes.Threat.SAFE;
		}else{
			threat = (byte)ClassifierAttributes.Threat.DANGER;
		}
		
		// MaxTargets (0-1 for everyone, 0-3+ for mage)
		int nb = AIUtil.getNbMaxTargets(charID);
		if (nb == 0){
			maxTargets = (byte)ClassifierAttributes.MaxTargets.NONE;
		}else if (nb == 1){
			maxTargets = (byte)ClassifierAttributes.MaxTargets.ONE;
		}else if (nb == 2){
			maxTargets = (byte)ClassifierAttributes.MaxTargets.TWO;
		}else{
			maxTargets = (byte)ClassifierAttributes.MaxTargets.THREE_OR_MORE;
		}
		
		
		infoAllies   = new List<InfoChars>();
		infoEnemies = new List<InfoChars>();
		
		// infoAllies / infoEnemies
		int char2ID = 0;
		foreach (Character c2 in hexaGrid.charList){
			if (c2 != c){
				if (c2.team == c.team){ // Ally
					// I am GUERRIER / VOLEUR / ARCHER / MAGE : 
					switch (c.charClass){
						case CharClass.GUERRIER :
						case CharClass.VOLEUR :
						case CharClass.ARCHER :
						case CharClass.MAGE : {
							switch (c2.charClass){
								// ally is GUERRIER / VOLEUR / ARCHER / MAGE : Is Ally able to attack the Enemy that I want to attack ?
								case CharClass.GUERRIER :
								case CharClass.VOLEUR :
								case CharClass.ARCHER :
								case CharClass.MAGE : {
									if (maxTargets != (byte)ClassifierAttributes.MaxTargets.NONE){
										Character cAttack = AIUtil.findCharToAttack(charID);
										if (cAttack != null){
											bool isWithinAttackRange = hexaGrid.hexaInSight(c2.x,c2.y,cAttack.x,cAttack.y,c2.getClassData().basicAttack.range);
											bool isWithinSkillRange = c2.skillAvailable && hexaGrid.hexaInSight(c2.x,c2.y,cAttack.x,cAttack.y,c2.getClassData().skill_1.range);
											if (isWithinAttackRange || isWithinSkillRange){
												infoAllies.Add(new InfoChars(c2,isWithinAttackRange,isWithinSkillRange));
											}else{
												// Get Enemy ID
												int cAttackID = 0;
												for (int i=0;i<hexaGrid.charList.Count;i++){
													if (hexaGrid.charList[i] == cAttack){
														//Debug.Log(i);
														cAttackID = i; i = hexaGrid.charList.Count;
													}
												}
												if (AIUtil.isCharWithinRangeAttack(char2ID,cAttackID) ||(c.skillAvailable && AIUtil.isCharWithinRangeSkill(char2ID,cAttackID))){
													infoAllies.Add(new InfoChars(c2,false,false));
												}
											}
										}
									}
								} break;
								// ally is SOIGNEUR / ENVOUTEUR : Is Ally able to heal/buff me ?
								case CharClass.SOIGNEUR : 
								case CharClass.ENVOUTEUR : {
									bool isWithinAttackRange = hexaGrid.hexaInSight(c2.x,c2.y,c.x,c.y,c2.getClassData().basicAttack.range);
									bool isWithinSkillRange = c2.skillAvailable && hexaGrid.hexaInSight(c2.x,c2.y,c.x,c.y,c2.getClassData().skill_1.range);
									if (isWithinAttackRange || isWithinSkillRange){
										infoAllies.Add(new InfoChars(c2,isWithinAttackRange,isWithinSkillRange));
									}else{
										if (AIUtil.isCharWithinRangeAttack(char2ID,charID) ||(c.skillAvailable && AIUtil.isCharWithinRangeSkill(char2ID,charID))){
											infoAllies.Add(new InfoChars(c2,false,false));
										}
									}
								} break;
							}
						} break;
						// I am SOIGNEUR / ENVOUTEUR : Can I heal/buff my ally ?
						case CharClass.SOIGNEUR : 
						case CharClass.ENVOUTEUR : {
							if (c.charClass == CharClass.SOIGNEUR && c2.HP == c2.HPmax){
							}else{
								bool isWithinAttackRange = hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().basicAttack.range);
								bool isWithinSkillRange = c.skillAvailable && hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().skill_1.range);
								if (isWithinAttackRange || isWithinSkillRange){
									infoAllies.Add(new InfoChars(c2,isWithinAttackRange,isWithinSkillRange));
								}else{
									if (AIUtil.isCharWithinRangeAttack(charID,char2ID) ||(c.skillAvailable && AIUtil.isCharWithinRangeSkill(charID,char2ID))){
										infoAllies.Add(new InfoChars(c2,false,false));
									}
								}
							}
						}break;
					}
						
				}else{ // Enemy : Can I reach the Enemy (directly or with movement)
					switch (c.charClass){
						case CharClass.GUERRIER :
						case CharClass.VOLEUR :
						case CharClass.ARCHER :
						case CharClass.MAGE : {
							bool isWithinAttackRange = hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().basicAttack.range);
							bool isWithinSkillRange = c.skillAvailable && hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().skill_1.range);
							if (isWithinAttackRange || isWithinSkillRange){
								infoEnemies.Add(new InfoChars(c2,isWithinAttackRange,isWithinSkillRange));
							}else{
								if (AIUtil.isCharWithinRangeAttack(charID,char2ID) ||(c.skillAvailable && AIUtil.isCharWithinRangeSkill(charID,char2ID))){
									infoEnemies.Add(new InfoChars(c2,false,false));
								}
							}
						} break;
						case CharClass.SOIGNEUR : 
						case CharClass.ENVOUTEUR : {
							// don't add anything to the list because it doesn't matter.
						} break;
					}
				}
			}
			char2ID++;
		}
		
		// fill allies/Enemies with NONE
		/*for (int i=infoAllies.Count;i<5;i++){
			infoAllies.Add(new InfoChars());
		}
		for (int i=infoEnemies.Count;i<5;i++){
			infoEnemies.Add(new InfoChars());
		}*/
		
		action = Action.ApproachEnemy;
		useCount = 0;
		fitness = 0.5f;
        id = 0;
        modified = true;
	}
	
	/** Copies the given classifier c. */
	public Classifier(Classifier c){
		this.charClass = c.charClass;
		this.HP = c.HP;
		this.PA = c.PA;
		this.skillAvailable = c.skillAvailable;
		this.threat = c.threat;
		this.maxTargets = c.maxTargets;
		
		this.infoAllies = new List<InfoChars>();
		this.infoEnemies = new List<InfoChars>();
		foreach (InfoChars i in c.infoAllies) this.infoAllies.Add(new InfoChars(i));
		foreach (InfoChars i in c.infoEnemies) this.infoEnemies.Add(new InfoChars(i));
		
		this.action = c.action;
		this.fitness = c.fitness;
		this.useCount = c.useCount;
        this.id = 0;
        this.modified = true;
	}
	
	/** Loads a classifier from a file. */
	public Classifier(BinaryReader reader){
		this.charClass = reader.ReadByte();
		this.HP = reader.ReadByte();
		this.PA = reader.ReadByte();
		this.skillAvailable = reader.ReadByte();
		this.threat = reader.ReadByte();
		this.maxTargets = reader.ReadByte();
		int nbAllies = reader.ReadByte();
		this.infoAllies = new List<InfoChars>();
		for (int i=0;i<nbAllies;i++){
			InfoChars temp = new InfoChars();
			temp.classType = reader.ReadByte();
			temp.HP = reader.ReadByte();
			temp.threat = reader.ReadByte();
			temp.distance = reader.ReadByte();
			this.infoAllies.Add(temp);
		}
		int nbEnemies = reader.ReadByte();
		this.infoEnemies = new List<InfoChars>();
		for (int i=0;i<nbEnemies;i++){
			InfoChars temp = new InfoChars();
			temp.classType = reader.ReadByte();
			temp.HP = reader.ReadByte();
			temp.threat = reader.ReadByte();
			temp.distance = reader.ReadByte();
			this.infoEnemies.Add(temp);
		}
		this.action = (Action)reader.ReadByte();
		this.fitness = reader.ReadSingle();
		this.useCount = reader.ReadInt32();
	}
	
	/** -1 <= n <= 1 */
	public void addToFitness(float n){
		if (n > 0.0f && n <= 1.0f){
			fitness += (1.0f-fitness) * n;
		}else if (n < 0.0f && n >= -1.0f){
			fitness += fitness * n;
		}
	}
	
	/** converts the number into one between -1 and 1 */
	public void addToFitness2(float n){
		addToFitness((n >= 0) ? (1.0f-(1.0f/(1.0f+n*0.01f))) : -(1.0f-(1.0f/(1.0f-n*0.01f))));
	}
	
	/** String to Rule */
	public Classifier(string str){
            //separateurs
           
        string[] separatingChars = { "*IA*" };
		string[] separatingChars2 = { "*IE*" };
		string[] separatingChars3 = { "/" };
		string[] separatingChars4 = { "//" };
		string[] W = str.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
	   // Debug.Log(W[0]);
		// exemple affichage 2/1/2/1/1/1
		string[] attributes = str.Split(separatingChars3, System.StringSplitOptions.RemoveEmptyEntries);

		this.charClass = byte.Parse(attributes[0]);
		this.HP = byte.Parse(attributes[1]);
		this.PA = byte.Parse(attributes[2]);
		this.skillAvailable = byte.Parse(attributes[3]);
		this.threat = byte.Parse(attributes[4]);
		this.maxTargets = byte.Parse(attributes[5]);

		this.infoAllies = new List<InfoChars>();
		this.infoEnemies = new List<InfoChars>();
		//Info Allies
		//display 
		//for exemple 8/7/7/15/
	   // Debug.Log(W[1]);

		for (int cmp = 0; cmp < 5; cmp++)
		{
			string strr = W[1].Substring(0, 9);
			string[] attributes2 = strr.Split(separatingChars3, System.StringSplitOptions.RemoveEmptyEntries);
		  //  Debug.Log(attributes2[0]);
		   // Debug.Log(attributes2[1]);
			InfoChars i = new InfoChars();
			i.classType = byte.Parse(attributes2[0]);
			i.HP = byte.Parse(attributes2[1]);
			i.threat = byte.Parse(attributes2[2]);
			i.distance = byte.Parse(attributes2[3]);
			this.infoAllies.Add(new InfoChars(i));
			//display         
		  //  Debug.Log("classType= " + i.classType);
		}

		//Debug.Log("W=  " + W[2]);
		string[] W2 = str.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
		//info Enemy
		for (int cmp = 0; cmp < 5; cmp++)
		{
			string strr = W2[1].Substring(0, 9);
			string[] attributes3 = strr.Split(separatingChars3, System.StringSplitOptions.RemoveEmptyEntries);

			InfoChars i = new InfoChars();
			i.classType = byte.Parse(attributes3[0]);
			i.HP = byte.Parse(attributes3[1]);
			i.threat = byte.Parse(attributes3[2]);
			i.distance = byte.Parse(attributes3[3]);
			this.infoEnemies.Add(new InfoChars(i));

		  //  Debug.Log("classType= " + i.classType);
		}
		string[] W4 = str.Split(separatingChars4, System.StringSplitOptions.RemoveEmptyEntries);
		switch (W4[1]){ //ApproachEnemy , ApproachAlly , Flee , Attack , Skill
			case "ApproachEnemy" : this.action = Action.ApproachEnemy ; break;
			case "ApproachAlly" : this.action = Action.ApproachAlly ; break;
			case "Flee" : this.action = Action.Flee ; break;
			case "Attack" : this.action = Action.Attack ; break;
			case "Skill" : this.action = Action.Skill ; break;
			default : Debug.LogWarning("OUPS"); break;
		}
            this.modified = false;

		
		    
	}
	
	public string RuleToString(){   //concatination des infos du personnage 
		string c = string.Concat(this.charClass, "/", this.HP, "/", this.PA, "/", this.skillAvailable, "/", this.threat, "/", this.maxTargets, "/");
		StringBuilder builder = new StringBuilder();

		builder.Append(c);
		//Ajout des informations des allies au stringBuilder
		builder.Append("*IA*/");
		for (int i = 0; i < this.infoAllies.Count; i++)
		{
			c = string.Concat(this.infoAllies[i].classType, "/", this.infoAllies[i].HP,
				"/", this.infoAllies[i].threat, "/", this.infoAllies[i].distance, "/");
			builder.Append(c);
		}
		builder.Append("*IA*/");
		//fin des informations sur les allies  
		//Ajout des informations des ennemis 
		builder.Append("*IE*/");
		for (int i = 0; i < this.infoEnemies.Count; i++)
		{
			c = string.Concat(this.infoEnemies[i].classType, "/", this.infoEnemies[i].HP,
				"/", this.infoEnemies[i].threat, "/", this.infoEnemies[i].distance, "/");
			builder.Append(c);
		}
		builder.Append("*IE*//");
		//fin des infos des ennemis 
		//builder.Append(this.action.ToString());
		return builder.ToString();
	}
	
	public bool isInRangeToUseAttack(){
		if ((charClass & (byte)ClassifierAttributes.CharClass.GUERRIER) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		if ((charClass & (byte)ClassifierAttributes.CharClass.VOLEUR) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		if ((charClass & (byte)ClassifierAttributes.CharClass.ARCHER) > 0)  foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		if ((charClass & (byte)ClassifierAttributes.CharClass.MAGE) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		if ((charClass & (byte)ClassifierAttributes.CharClass.SOIGNEUR) > 0) foreach (InfoChars ic in infoAllies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		if ((charClass & (byte)ClassifierAttributes.CharClass.ENVOUTEUR) > 0) foreach (InfoChars ic in infoAllies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.ATTACK || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		return false;
	}
	
	public bool isInRangeToUseSkill(){
		if (skillAvailable == (byte)ClassifierAttributes.SkillAvailable.YES){
			if ((charClass & (byte)ClassifierAttributes.CharClass.GUERRIER) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
			if ((charClass & (byte)ClassifierAttributes.CharClass.VOLEUR) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
			if ((charClass & (byte)ClassifierAttributes.CharClass.ARCHER) > 0)  foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
			if ((charClass & (byte)ClassifierAttributes.CharClass.MAGE) > 0) foreach (InfoChars ic in infoEnemies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
			if ((charClass & (byte)ClassifierAttributes.CharClass.SOIGNEUR) > 0) foreach (InfoChars ic in infoAllies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
			if ((charClass & (byte)ClassifierAttributes.CharClass.ENVOUTEUR) > 0) foreach (InfoChars ic in infoAllies) if (ic.classType != (byte)ClassifierAttributes.ClassType.NONE &&  (ic.distance == (byte)ClassifierAttributes.Distance.SKILL || ic.distance == (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL)) return true;
		}
		return false;
	}
	
	public bool alliesEquals(Classifier c){
		if (infoAllies.Count == c.infoAllies.Count){
			for (int i=0;i<infoAllies.Count;i++) if (!(c.infoAllies[0].equals(infoAllies[0]))) return false;
			return true;
		}else{
			return false;
		}
	}
	
	public bool EnemiesEquals(Classifier c){
		if (infoEnemies.Count == c.infoEnemies.Count){
			for (int i=0;i<infoEnemies.Count;i++) if (!(c.infoEnemies[0].equals(infoEnemies[0]))) return false;
			return true;
		}else{
			return false;
		}
	}
	
	public bool alliesSimilar(Classifier c){
		if (infoAllies.Count == c.infoAllies.Count){
			for (int i=0;i<infoAllies.Count;i++) if (!(c.infoAllies[0].isSimilar(infoAllies[0]))) return false;
			return true;
		}else{
			return false;
		}
	}
	
	public bool EnemiesSimilar(Classifier c){
		if (infoEnemies.Count == c.infoEnemies.Count){
			for (int i=0;i<infoEnemies.Count;i++) if (!(c.infoEnemies[0].isSimilar(infoEnemies[0]))) return false;
			return true;
		}else{
			return false;
		}
	}
	
	public bool equals(Classifier c){
		return ((charClass == c.charClass) &&
		(HP == c.HP) &&
		(PA == c.PA) &&
		(PA == c.PA) &&
		(skillAvailable == c.skillAvailable) &&
		(threat == c.threat) &&
		(maxTargets == c.maxTargets) &&
		(alliesEquals(c)) &&
		(EnemiesEquals(c)) && 
		(action == c.action));
	}
	
	public bool isSimilar(Classifier c){
		return (((charClass & c.charClass) > 0) &&
		((HP & c.HP) > 0) &&
		((PA & c.PA) > 0) &&
		((PA & c.PA) > 0) &&
		((skillAvailable & c.skillAvailable) > 0) &&
		((threat & c.threat) > 0) &&
		((maxTargets & c.maxTargets) > 0) &&
		(alliesSimilar(c)) &&
		(EnemiesSimilar(c)) && 
		(action == c.action));
	}
	
	// These don't check for action match (Used to matching situations)
	public bool equals2(Classifier c){
		return ((charClass == c.charClass) &&
		(HP == c.HP) &&
		(PA == c.PA) &&
		(PA == c.PA) &&
		(skillAvailable == c.skillAvailable) &&
		(threat == c.threat) &&
		(maxTargets == c.maxTargets) &&
		(alliesEquals(c)) &&
		(EnemiesEquals(c)));
	}
	
	public bool isSimilar2(Classifier c){
		return (((charClass & c.charClass) > 0) &&
		((HP & c.HP) > 0) &&
		((PA & c.PA) > 0) &&
		((PA & c.PA) > 0) &&
		((skillAvailable & c.skillAvailable) > 0) &&
		((threat & c.threat) > 0) &&
		((maxTargets & c.maxTargets) > 0) && 
		(alliesSimilar(c)) &&
		(EnemiesSimilar(c)));
	}
	
	/** Writes the classifier in binary */
	public void saveInBinary(BinaryWriter writer){
		writer.Write(this.charClass);
		writer.Write(this.HP);
		writer.Write(this.PA);
		writer.Write(this.skillAvailable);
		writer.Write(this.threat);
		writer.Write(this.maxTargets);
		writer.Write((byte)this.infoAllies.Count);
		foreach (Classifier.InfoChars ic in this.infoAllies){
			writer.Write(ic.classType);
			writer.Write(ic.HP);
			writer.Write(ic.threat);
			writer.Write(ic.distance);
		}
		writer.Write((byte)this.infoEnemies.Count);
		foreach (Classifier.InfoChars ic in this.infoEnemies){
			writer.Write(ic.classType);
			writer.Write(ic.HP);
			writer.Write(ic.threat);
			writer.Write(ic.distance);
		}
		writer.Write((byte)this.action);
		writer.Write(this.fitness);
		writer.Write(this.useCount);
	}
	
	public string getStringInfo(){
		string strDisp = "Char class : ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.GUERRIER) > 0) strDisp += "GUERRIER ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.VOLEUR) > 0) strDisp += "VOLEUR ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.ARCHER) > 0) strDisp += "ARCHER ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.MAGE) > 0) strDisp += "MAGE ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
		if ((charClass & (byte)ClassifierAttributes.CharClass.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
		
		strDisp += "| HP : ";
		if ((HP & (byte)ClassifierAttributes.HP_.BETWEEN_100_75) > 0) strDisp += "100-75% ";
		if ((HP & (byte)ClassifierAttributes.HP_.BETWEEN_74_40) > 0) strDisp += "74-40% ";
		if ((HP & (byte)ClassifierAttributes.HP_.BETWEEN_39_0) > 0) strDisp += "39-0% ";
		
		strDisp += "| PA : ";
		if ((PA & (byte)ClassifierAttributes.PA_.ONE) > 0) strDisp += "ONE ";
		if ((PA & (byte)ClassifierAttributes.PA_.TWO_OR_MORE) > 0) strDisp += "TWO+ ";
		
		strDisp += "| Skill : ";
		if ((skillAvailable & (byte)ClassifierAttributes.SkillAvailable.YES) > 0) strDisp += "YES ";
		if ((skillAvailable & (byte)ClassifierAttributes.SkillAvailable.NO) > 0) strDisp += "NO ";
		
		strDisp += "| Threat : ";
		if ((threat & (byte)ClassifierAttributes.Threat.SAFE) > 0) strDisp += "SAFE ";
		if ((threat & (byte)ClassifierAttributes.Threat.DANGER) > 0) strDisp += "DANGER ";
		if ((threat & (byte)ClassifierAttributes.Threat.DEATH) > 0) strDisp += "DEATH ";
		
		strDisp += "| Max Targets : ";
		if ((maxTargets & (byte)ClassifierAttributes.MaxTargets.NONE) > 0) strDisp += "NONE ";
		if ((maxTargets & (byte)ClassifierAttributes.MaxTargets.ONE) > 0) strDisp += "ONE ";
		if ((maxTargets & (byte)ClassifierAttributes.MaxTargets.TWO) > 0) strDisp += "TWO ";
		if ((maxTargets & (byte)ClassifierAttributes.MaxTargets.THREE_OR_MORE) > 0) strDisp += "THREE+ ";
		
		if (infoAllies.Count > 0) strDisp += "\nAllies :";
		foreach (InfoChars i in infoAllies){
			strDisp += "\nClass Type : ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.OTHER) > 0) strDisp += "OTHER ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.NONE) > 0) strDisp += "NONE ";
			strDisp += "| HP : ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_100_75) > 0) strDisp += "100-75% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_74_40) > 0) strDisp += "74-40% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_39_0) > 0) strDisp += "39-0% ";
			
			strDisp += "| Threat : ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.SAFE) > 0) strDisp += "SAFE ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DANGER) > 0) strDisp += "DANGER ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DEATH) > 0) strDisp += "DEATH ";
			
			strDisp += "| Distance : ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.ATTACK) > 0) strDisp += "ATTACK ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.SKILL) > 0) strDisp += "SKILL ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL) > 0) strDisp += "ATTACK/SKILL ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.MOVEMENT) > 0) strDisp += "MOVEMENT ";
		}
		if (infoEnemies.Count > 0) strDisp += "\nEnemies :";
		foreach (InfoChars i in infoEnemies){
			strDisp += "\nClass Type : ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.OTHER) > 0) strDisp += "(GUERRIER/VOLEUR/ARCHER/MAGE) ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.NONE) > 0) strDisp += "NONE ";
			strDisp += "| HP : ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_100_75) > 0) strDisp += "100-75% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_74_40) > 0) strDisp += "74-40% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_39_0) > 0) strDisp += "39-0% ";
			
			strDisp += "| Threat : ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.SAFE) > 0) strDisp += "SAFE ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DANGER) > 0) strDisp += "DANGER ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DEATH) > 0) strDisp += "DEATH ";
			
			strDisp += "| Distance : ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.ATTACK) > 0) strDisp += "ATTACK ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.SKILL) > 0) strDisp += "SKILL ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL) > 0) strDisp += "ATTACK/SKILL ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.MOVEMENT) > 0) strDisp += "MOVEMENT ";
		}
		
		strDisp += "\nAction : " + action + " | Fitness : " + fitness + " | Use count : " + useCount;
		
		return strDisp;
	}
	
	/** used for mutations  */
	public static Action getRandomAction(){
		var values = Action.GetValues(typeof(Action));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		Action randomVal = (Action)values.GetValue(nb);
		return randomVal;
	}
	
	/// <summary>
    /// get classifier from list of classifier with RouletteWheel Selection
	/// <param name="classifiers">: the list of matching classifiers
	/// <returns>  selected classifier  </returns>  
    /// </summary>
	public static Classifier getClassifierRouletteWheel(List<Classifier> classifiers){
		float total = 0.0f;
		foreach (Classifier c in classifiers) total += c.fitness;
		float r = UnityEngine.Random.Range(0.0f,total);
		float n = 0.0f;
		foreach (Classifier c in classifiers){
			if (r >= n && r <= c.fitness + n) return c;
			else n += c.fitness;
		}
		return null;
	}
	/// <summary>
    /// get classifier from list of classifier with Elitist Selection
	/// <param name="classifiers">: the list of matching classifiers
	/// <returns>  selected classifier  </returns>  
    /// </summary>
	public static Classifier getClassifierElististSelection(List<Classifier> classifiers){
		if (classifiers.Count > 0){
			float maxFitness = classifiers[0].fitness;
			List<Classifier> maxClassifiers = new List<Classifier>();
			// Find max value
			for (int i=1;i<classifiers.Count;i++) if (classifiers[i].fitness > maxFitness) maxFitness = classifiers[i].fitness;
			// Get all classifiers with max value
			for (int i=0;i<classifiers.Count;i++) if (classifiers[i].fitness == maxFitness) maxClassifiers.Add(classifiers[i]);
			return getRandomClassifier(maxClassifiers);
		}else{
			return null;
		}
	}

		public static Classifier getClassifierBestAction(List<Classifier> classifiers,Classifier currentSituation){
			if (classifiers.Count > 0){
				Debug.Log("je suis la getClassifierBestAction ");
				if (currentSituation.isInRangeToUseSkill()){
					for (int i=1;i<classifiers.Count;i++)
					{
						if (classifiers[i].action == Classifier.Action.Skill) 
							return  classifiers[i];
					}
				}
				else if (currentSituation.isInRangeToUseAttack()){
					for (int i=1;i<classifiers.Count;i++)
					{
						if (classifiers[i].action == Classifier.Action.Attack) 
							return  classifiers[i];
					}
				}
				else{ 
					if (((byte)currentSituation.charClass & (byte)ClassifierAttributes.ClassType.OTHER) > 0)
					{
						Debug.Log("je suis la OTHER ");
						for (int i=1;i<classifiers.Count;i++)
						{
							if (classifiers[i].action == Classifier.Action.ApproachEnemy) 
								return  classifiers[i];
						}
					}
					if (((byte)currentSituation.charClass & (byte)ClassifierAttributes.ClassType.ENVOUTEUR) > 0) 
					{
						Debug.Log("je suis la Envouteur ");
						for (int i=1;i<classifiers.Count;i++)
						{
							if (classifiers[i].action == Classifier.Action.ApproachAlly) 
								return  classifiers[i];
						}

					}
					if (((byte)currentSituation.charClass & (byte)ClassifierAttributes.ClassType.SOIGNEUR) > 0) 
					{
						for (int i=1;i<classifiers.Count;i++)
						{
							if (classifiers[i].action == Classifier.Action.ApproachAlly) 
								return  classifiers[i];
						}

					}
				}
			}
			else{
				return null;
			}
			return null;
		}

	/// <summary>
    /// get classifier from list of classifier randomly
	/// <param name="classifiers">: the list of matching classifiers
	/// <returns>  selected classifier  </returns>  
    /// </summary>
	public static Classifier getRandomClassifier(List<Classifier> classifiers){
		return (classifiers.Count > 0) ? classifiers[UnityEngine.Random.Range(0,classifiers.Count)] : null;
	}
	
	/// <summary>
    /// get classifier from list of classifier with the tournament selection
	/// <param name="classifiers">: the list of matching classifiers
	/// <returns>  selected classifier  </returns>  
    /// </summary>
	public static Classifier getClassifierTournamentSelection(List<Classifier> classifiers){
		if (classifiers.Count > 0){
			if (classifiers.Count==1 || classifiers.Count==2){
				return getClassifierElististSelection(classifiers);
			}else{
				Classifier cl1=getRandomClassifier(classifiers);
				Classifier cl2=getRandomClassifier(classifiers);
				int nb=0;
				while(cl2.action==cl1.action && nb <10){
					cl2=getRandomClassifier(classifiers);
					nb++;
				}
				if (cl1.fitness>cl2.fitness) return cl1;
				else return cl2;
			}
		}else{
			return null;
		}
	}
	
	private static void sort2(List<Classifier> t,int debutT1,int debutT2,int endT2){
		for (int i=debutT1;i<debutT2 && debutT2 <= endT2;i++){
			if (t[i].fitness > t[debutT2].fitness){
				Classifier temp = t[debutT2];
				for (int j=debutT2;j>i;j--){
					t[j] = t[j-1];
				}
				t[i] = temp;
				debutT2++;
			}
		}
	}

	public static void sortByFitness(List<Classifier> t){
		int nb = t.Count;
		if (nb > 1){
			for (int i=0;i<nb;i+=2){
				if (i+1 < nb){
					if (t[i].fitness > t[i+1].fitness){
						Classifier temp = t[i];
						t[i] = t[i+1];
						t[i+1] = temp;
					}
				}

			}

			for (int j=2;j<nb;j<<=1){
				for (int i=0;i<nb;i+=j<<1){
					if (i+j < nb){
						sort2(t,0+i,j+i,(((j<<1)-1)+i >= nb) ? nb-1 : ((j<<1)-1)+i); //(4-1) .. (7-1) .. (16-1)
					}
				}
			}
		}
	}
	
	private static void sort3(List<Classifier> t,int debutT1,int debutT2,int endT2){
		for (int i=debutT1;i<debutT2 && debutT2 <= endT2;i++){
			if (t[i].fitness < t[debutT2].fitness){
				Classifier temp = t[debutT2];
				for (int j=debutT2;j>i;j--){
					t[j] = t[j-1];
				}
				t[i] = temp;
				debutT2++;
			}
		}
	}

	public static void sortByFitness2(List<Classifier> t){
		int nb = t.Count;
		if (nb > 1){
			for (int i=0;i<nb;i+=2){
				if (i+1 < nb){
					if (t[i].fitness < t[i+1].fitness){
						Classifier temp = t[i];
						t[i] = t[i+1];
						t[i+1] = temp;
					}
				}

			}

			for (int j=2;j<nb;j<<=1){
				for (int i=0;i<nb;i+=j<<1){
					if (i+j < nb){
						sort3(t,0+i,j+i,(((j<<1)-1)+i >= nb) ? nb-1 : ((j<<1)-1)+i); //(4-1) .. (7-1) .. (16-1)
					}
				}
			}
		}
	}

}

public class ClassifierSystem {
	public List<Classifier> classifiers;
	
	public ClassifierSystem(){
		classifiers = new List<Classifier>();
	}
	
	/// <summary>
    /// Constructor of the classifier List from a databse instance , 
	/// DEPENDENCY TO DO : DATABASE get Instance function  , Constructur of the Classifier  that use  a STRING as parmeter 
    /// </summary>
	public ClassifierSystem(IDbConnection dbconn ){
	Debug.Log("Load From Database");
		classifiers = new List<Classifier>();
		
		IDbCommand dbcmd = dbconn.CreateCommand();
		string sqlQuery = "SELECT * FROM Rules ";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader();
     	 
		while (reader.Read())
     	{
            int id = reader.GetInt32(0);
			string situation = reader.GetString(1);
			string action = reader.GetString(2);
            float fitness = reader.GetFloat(3);
			StringBuilder builder = new StringBuilder();
			builder.Append(situation);
			builder.Append(action);
			string result = builder.ToString();
			Classifier tempClassifier = new Classifier(result);
            tempClassifier.id = id;
            tempClassifier.fitness = fitness;
			classifiers.Add(tempClassifier);	
    	}

		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbconn.Close();
		dbconn = null;
    }

    public void saveInDB(){
        foreach (Classifier c in classifiers){
            if (c.id == 0){
                DB.addRule(c);
            }else if (c.modified){
                DB.UpdateRuleFitness(c);
            }
        }
    }
	
	public void Add(Classifier classifier){
		classifiers.Add(classifier);
	}
	
	public List<Classifier> findMatchingClassifiers(Classifier classifier){
		List<Classifier> r = new List<Classifier>();
		foreach (Classifier c in classifiers){
			if (c.isSimilar2(classifier)){
				r.Add(c);
			}
		}
		return r;
	}
	
	public string getStringInfo(){
		string str = "";
		for (int i=0;i<classifiers.Count;i++){
			str += i+1 + " :\n" + classifiers[i].getStringInfo() + "\n--------------------------------------------------------------------------------------------------------------\n";
		}
		return str;
	}
	
	public void dispInFile(string filePath){
		System.IO.File.WriteAllText(filePath,getStringInfo());
	}
	
	/** Creates a file that contains all the classifiers. */
	public void saveAllInBinary(string filePath){
		using (BinaryWriter writer = new BinaryWriter(File.Open(filePath,FileMode.Create))){
			writer.Write(classifiers.Count);
			foreach (Classifier c in classifiers){
				c.saveInBinary(writer);
			}
		}
	}
	
	public void loadAllInBinary(string filePath){
		if (File.Exists(filePath)){
			using (BinaryReader reader = new BinaryReader(File.Open(filePath,FileMode.Open))){
				int nbClassifiers = reader.ReadInt32();
				for (int i=0;i<nbClassifiers;i++){
					classifiers.Add(new Classifier(reader));
				}
			}
		}
	}
	
}

}