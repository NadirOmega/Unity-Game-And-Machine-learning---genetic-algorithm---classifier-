﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hexas;
using Characters;
using AI_Util;

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
}

	public class Classifier{
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
			threat = 1;
			
			
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
		public InfoChars Copy()
   				 {
      				 return (InfoChars) this.MemberwiseClone();
    			}
		
		
		public InfoChars(){
			classType = (byte)ClassifierAttributes.ClassType.NONE;
			HP = 1+2+4;
			threat = 1+2+4;
			distance = 1+2+4+8;
			
		}
		
		public bool equals(InfoChars i){
			return ((classType == i.classType) && (HP == i.HP) && (threat == i.threat) && (distance == i.distance));
		}
		
		public bool isSimilar(InfoChars i){
			return (((classType & i.classType) > 0) && ((HP & i.HP) > 0) && ((threat & i.threat) > 0) && ((distance & i.distance) > 0));
		}
	}
	
	public List<InfoChars> infoAllies;
	public List<InfoChars> infoEnnemies;
	
	public enum Action : byte {ApproachEnnemy , ApproachAlly , RandomMovement , Flee , Attack , Skill};
	public Action action;
	
	public float fitness;
	public int useCount;
	
	//Used as a clone mehode
	public Classifier (Classifier currentC){
	/* 
		this.charClass = currentC.charClass;
		this.HP=currentC.HP;
		this.PA=currentC.PA;
		this.skillAvailable=currentC.skillAvailable;
		this.threat =currentC.threat;
		this.infoAllies=currentC.infoAllies.Clone();
		this.infoAllies=currentC.infoAllies.Clone();
		this.action=currentC.action;
		this.fitness=currentC.fitness;
*/
	}

	 public Classifier Copy()
    {
       return (Classifier) this.MemberwiseClone();
    }

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
		threat = 1;
		
		// MaxTargets
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
		infoEnnemies = new List<InfoChars>();
		
		// infoAllies / infoEnnemies
		foreach (Character c2 in hexaGrid.charList){
			if (c2 != c){
				
				if (c2.team == c.team){ // Ally
					infoAllies.Add(new InfoChars(c2,false,false));
				}else{ // Ennemy
					bool isWithinAttackRange = hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().basicAttack.range);
					bool isWithinSillRange = hexaGrid.hexaInSight(c.x,c.y,c2.x,c2.y,c.getClassData().skill_1.range);
					if (isWithinAttackRange || isWithinSillRange){
						infoEnnemies.Add(new InfoChars(c2,isWithinAttackRange,isWithinSillRange));
					}
				}
			}
		}
		
		// fill allies/Ennemies with NONE
		for (int i=infoAllies.Count;i<5;i++){
			infoAllies.Add(new InfoChars());
		}
		for (int i=infoEnnemies.Count;i<5;i++){
			infoEnnemies.Add(new InfoChars());
		}
		
		action = Action.RandomMovement;
		useCount = 0;
		fitness = 0.5f;
	}
	
	public bool alliesEquals(Classifier c){
		return (c.infoAllies[0].equals(infoAllies[0]) &&
		c.infoAllies[1].equals(infoAllies[1]) &&
		c.infoAllies[2].equals(infoAllies[2]) &&
		c.infoAllies[3].equals(infoAllies[3]) &&
		c.infoAllies[4].equals(infoAllies[4]));
	}
	
	public bool ennemiesEquals(Classifier c){
		return (c.infoEnnemies[0].equals(infoEnnemies[0]) &&
		c.infoEnnemies[1].equals(infoEnnemies[1]) &&
		c.infoEnnemies[2].equals(infoEnnemies[2]) &&
		c.infoEnnemies[3].equals(infoEnnemies[3]) &&
		c.infoEnnemies[4].equals(infoEnnemies[4]));
	}
	
	public bool alliesSimilar(Classifier c){
		return ((c.infoAllies[0].isSimilar(infoAllies[0])) &&
		(c.infoAllies[1].isSimilar(infoAllies[1])) &&
		(c.infoAllies[2].isSimilar(infoAllies[2])) &&
		(c.infoAllies[3].isSimilar(infoAllies[3])) &&
		(c.infoAllies[4].isSimilar(infoAllies[4])));
	}
	
	public bool ennemiesSimilar(Classifier c){
		return (c.infoEnnemies[0].isSimilar(infoEnnemies[0]) &&
		c.infoEnnemies[1].isSimilar(infoEnnemies[1]) &&
		c.infoEnnemies[2].isSimilar(infoEnnemies[2]) &&
		c.infoEnnemies[3].isSimilar(infoEnnemies[3]) &&
		c.infoEnnemies[4].isSimilar(infoEnnemies[4]));
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
		(ennemiesEquals(c)) && 
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
		(ennemiesSimilar(c)) && 
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
		(ennemiesEquals(c)));
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
		(ennemiesSimilar(c)));
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
		if ((maxTargets & (byte)ClassifierAttributes.MaxTargets.THREE_OR_MORE) > 0) strDisp += "THREE OR MORE ";
		
		strDisp += "\nAllies :";
		foreach (InfoChars i in infoAllies){
			strDisp += "\nClass Type : ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.OTHER) > 0) strDisp += "OTHER ";
			strDisp += "| HP : ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_100_75) > 0) strDisp += "100-75% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_74_40) > 0) strDisp += "74-40% ";
			if ((i.HP & (byte)ClassifierAttributes.HP_.BETWEEN_39_0) > 0) strDisp += "39-0% ";
			
			strDisp += "| Threat : ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.SAFE) > 0) strDisp += "SAFE ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DANGER) > 0) strDisp += "DANGER ";
			if ((i.threat & (byte)ClassifierAttributes.Threat.DEATH) > 0) strDisp += "DEATH ";
		}
		strDisp += "\nEnnemies :";
		foreach (InfoChars i in infoEnnemies){
			strDisp += "\nClass Type : ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
			if ((i.classType & (byte)ClassifierAttributes.ClassType.OTHER) > 0) strDisp += "(GUERRIER/VOLEUR/ARCHER/MAGE) ";
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
			if ((i.distance & (byte)ClassifierAttributes.Distance.ATTACK_AND_SKILL) > 0) strDisp += "ATTACK AND SKILL ";
			if ((i.distance & (byte)ClassifierAttributes.Distance.MOVEMENT) > 0) strDisp += "MOVEMENT ";
		}
		
		strDisp += "\nAction : " + action;
		
		return strDisp;
	}

	
	/*All this methods will be used for mutations  */

	

	public static ClassifierAttributes.CharClass returnRandomCharClassValue(){
		var values = ClassifierAttributes.CharClass.GetValues(typeof(ClassifierAttributes.CharClass));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		ClassifierAttributes.CharClass randomVal = (ClassifierAttributes.CharClass)values.GetValue(nb); 	
		return randomVal;
	}
	public static ClassifierAttributes.HP_ returnRandomHPValue(){
		var values = ClassifierAttributes.HP_.GetValues(typeof(ClassifierAttributes.HP_));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		ClassifierAttributes.HP_ randomVal = (ClassifierAttributes.HP_)values.GetValue(nb); 	
		return randomVal;
	}
	public static ClassifierAttributes.PA_ returnRandomPAValue(){
		var values = ClassifierAttributes.PA_.GetValues(typeof(ClassifierAttributes.PA_));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		ClassifierAttributes.PA_ randomVal = (ClassifierAttributes.PA_)values.GetValue(nb); 	
		return randomVal;
	}

	public static ClassifierAttributes.SkillAvailable returnRandomSkillValue(){
		var values = ClassifierAttributes.SkillAvailable.GetValues(typeof(ClassifierAttributes.SkillAvailable));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		ClassifierAttributes.SkillAvailable randomVal = (ClassifierAttributes.SkillAvailable)values.GetValue(nb); 	
		return randomVal;
	}

	public static ClassifierAttributes.Threat returnRandomThreatValue(){
		var values = ClassifierAttributes.Threat.GetValues(typeof(ClassifierAttributes.Threat));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		ClassifierAttributes.Threat randomVal = (ClassifierAttributes.Threat)values.GetValue(nb); 	
		return randomVal;
	}
	public static Action getRandomActionFromEnum()
	{
		var values = Action.GetValues(typeof(Action));
		System.Random random = new System.Random();
		int nb = random.Next(0,values.Length);
		Action randomVal = (Action)values.GetValue(nb);
		return randomVal;

	}
	/**mutate ally & ennemy*/
	public static ClassifierAttributes.ClassType returnRandomCharTypeValue(byte OldValue)
	{
		var values = ClassifierAttributes.ClassType.GetValues(typeof(ClassifierAttributes.ClassType));
		System.Random random = new System.Random();
		
		ClassifierAttributes.ClassType randomVal;
		int nb ;
		do {
		nb = random.Next(0,values.Length);
		randomVal = (ClassifierAttributes.ClassType)values.GetValue(nb); 	
		}while(((byte)randomVal)==OldValue);

		return randomVal;
		
	}
	/*
	get Random HP Vale different from the old one 
	param=Byte Oldevalue
	return Random Hp Hp Value 
	 */
	public static ClassifierAttributes.HP_ returnRandomHPValue(byte OldValue ){
		var values = ClassifierAttributes.HP_.GetValues(typeof(ClassifierAttributes.HP_));
		System.Random random = new System.Random();
		ClassifierAttributes.HP_ randomVal;
		int nb ;
		do {
		nb = random.Next(0,values.Length);
		randomVal = (ClassifierAttributes.HP_)values.GetValue(nb); 
		}
		while(((byte)randomVal)==OldValue);

		return randomVal;
	}

}

public class ClassifierSystem {
	public List<Classifier> classifiers;
	
	public ClassifierSystem(){
		classifiers = new List<Classifier>();
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
}

}
