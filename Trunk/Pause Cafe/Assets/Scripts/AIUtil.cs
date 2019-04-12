﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;
using Hexas;
using Characters;
using AI_Class;
using static MainGame;

namespace AI_Util {

public class HexaDamage{
	public int x;
	public int y;
	public int value;
	
	public HexaDamage(int x,int y,int value){
		this.x = x;
		this.y = y;
		this.value = value;
	}
}

public class AIUtil{
	public static HexaGrid hexaGrid;
	
	/** Calculates how much damage can potentially be taken on each hexa.
		Returns a list of w*h that contains the data. */
	public static int[] calculateThreat(int team){
		int w_h = hexaGrid.w * hexaGrid.h;
		int[] finalList = new int[w_h];
		int[] list = new int[w_h];
		for (int i=0;i<w_h;i++){
			finalList[i] = 0;
			list[i] = 0;
		}
		
		foreach (Character c in hexaGrid.charList){
			if (c.team != team && c.charClass != CharClass.SOIGNEUR && c.charClass != CharClass.ENVOUTEUR){
				int damage = c.getClassData().basicAttack.effectValue;
				// 0 PM
				List<Point> listHexasInSight = hexaGrid.findHexasInSight2(c.x,c.y,c.getClassData().basicAttack.range);
				foreach (Point p in listHexasInSight){
					int pos = p.x + p.y*hexaGrid.w;
					if (list[pos] < damage*c.PA) list[pos] = damage*c.PA;
				}
				
				// 1+ PM
				for (int i=1;i<c.PA;i++){
					List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
					foreach (Point charpos in listH){
						listHexasInSight = hexaGrid.findHexasInSight2(charpos.x,charpos.y,c.getClassData().basicAttack.range);
						foreach (Point p in listHexasInSight){
							int pos = p.x + p.y*hexaGrid.w;
							if (list[pos] < damage*(c.PA-i)) list[pos] = damage*(c.PA-i);
						}
					}
				}
				// Add to the list
				for (int i=0;i<w_h;i++){
					finalList[i] += list[i];
					list[i] = 0;
				}
			}
		}
		return finalList;
	}
	
	/** Calculates how much damage can potentially be dealt on each hexa.
		Returns a list of w*h that contains the data. */
	public static int[] calculateDamage(int charID){
		Character currentChar = hexaGrid.charList[charID];
		int w_h = hexaGrid.w * hexaGrid.h;
		int[] list = new int[w_h];
		for (int i=0;i<w_h;i++){
			list[i] = 0;
		}
		
		int damage = currentChar.getClassData().basicAttack.effectValue;
		// 0 PM
		foreach (Character c in hexaGrid.charList){
			if (c.team != currentChar.team){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					int value = damage*currentChar.PA;
					if (c.HP <= value) value += 10;
					int pos = currentChar.x + currentChar.y*hexaGrid.w;
					if (list[pos] < value) list[pos] = value;
				}
			}
		}
		// 1+ PM
		for (int i=1;i<currentChar.PA;i++){
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*i);
			foreach (Point charpos in listH){
				foreach (Character c in hexaGrid.charList){
					if (c.team != currentChar.team){
						if (hexaGrid.hexaInSight(charpos.x,charpos.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
							int value = damage*(currentChar.PA-i);
							if (c.HP <= value) value += 10;
							int pos = charpos.x + charpos.y*hexaGrid.w;
							if (list[pos] < value) list[pos] = value;
						}
					}
				}
			}
		}
		
		return list;
	}

	/** Calculates how much healing can potentially be done on each hexa
		assuming the character is a healer.
		Returns a list of w*h that contains the data.*/
	public static int[] calculateHealing(int charID){
		Character currentChar = hexaGrid.charList[charID];
		int w_h = hexaGrid.w * hexaGrid.h;
		int[] list = new int[w_h];
		for (int i=0;i<w_h;i++){
			list[i] = 0;
		}
		
		int healing = currentChar.getClassData().basicAttack.effectValue;
		// 0 PM
		foreach (Character c in hexaGrid.charList){
			if (c.team == currentChar.team){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					int value = healing*currentChar.PA;
					if (c.HP+value > c.HPmax) value = c.HPmax-c.HP;
					int pos = currentChar.x + currentChar.y*hexaGrid.w;
					if (list[pos] < value) list[pos] = value;
				}
			}
		}
		// 1+ PM
		for (int i=1;i<currentChar.PA;i++){
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*i);
			foreach (Point charpos in listH){
				foreach (Character c in hexaGrid.charList){
					if (c.team == currentChar.team){
						if (hexaGrid.hexaInSight(charpos.x,charpos.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
							int value = healing*(currentChar.PA-i);
							if (c.HP+value > c.HPmax) value = c.HPmax-c.HP;
							int pos = charpos.x + charpos.y*hexaGrid.w;
							if (list[pos] < value) list[pos] = value;
						}
					}
				}
			}
		}
		
		return list;
	}
	
	/** Calculates how many PAs can potentially be given on each hexa
		assuming the character is a envouteur.
		Returns a list of w*h that contains the data.*/
	public static int[] calculateBuff(int charID){
		Character currentChar = hexaGrid.charList[charID];
		int w_h = hexaGrid.w * hexaGrid.h;
		int[] list = new int[w_h];
		for (int i=0;i<w_h;i++){
			list[i] = 0;
		}
		
		int healing = currentChar.getClassData().basicAttack.effectValue;
		// 0 PM
		foreach (Character c in hexaGrid.charList){
			if (c.team == currentChar.team){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					int value;
					switch (c.charClass){
						case CharClass.GUERRIER  : value = 3; break;
						case CharClass.VOLEUR    : value = 6; break;
						case CharClass.ARCHER    : value = 4; break;
						case CharClass.MAGE      : value = 5; break;
						case CharClass.SOIGNEUR  : value = 2; break;
						case CharClass.ENVOUTEUR : value = 1; break;
						default : value = 0; break;
					}
					int pos = currentChar.x + currentChar.y*hexaGrid.w;
					if (list[pos] < value) list[pos] = value;
				}
			}
		}
		// 1+ PM
		for (int i=1;i<currentChar.PA;i++){
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*i);
			foreach (Point charpos in listH){
				foreach (Character c in hexaGrid.charList){
					if (c.team == currentChar.team){
						if (hexaGrid.hexaInSight(charpos.x,charpos.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
							int value;
							switch (c.charClass){
								case CharClass.GUERRIER  : value = 3; break;
								case CharClass.VOLEUR    : value = 6; break;
								case CharClass.ARCHER    : value = 4; break;
								case CharClass.MAGE      : value = 5; break;
								case CharClass.SOIGNEUR  : value = 2; break;
								case CharClass.ENVOUTEUR : value = 1; break;
								default : value = 0; break;
							}
							int pos = charpos.x + charpos.y*hexaGrid.w;
							if (list[pos] < value) list[pos] = value;
						}
					}
				}
			}
		}
		
		return list;
	}
	
	/** Returns the minimum number of steps to go from (x1,y1) to (x2,y2). */
	public static int getDistance(int x1,int y1,int x2,int y2){
		int maxSteps = 300;
		if (hexaGrid.getHexa(x1,y1).type == HexaType.WALL || hexaGrid.getHexa(x2,y2).type == HexaType.WALL) return -1;
		List<HexaGrid.HexaTemp> hexaList2 = new List<HexaGrid.HexaTemp>();
		foreach (Hexa hexa in hexaGrid.hexaList){
			hexaList2.Add(new HexaGrid.HexaTemp(hexa.x,hexa.y,maxSteps+1));
		}
		List<HexaGrid.HexaTemp> toCheck = new List<HexaGrid.HexaTemp>();
		toCheck.Add(new HexaGrid.HexaTemp(x1,y1,0));
		hexaList2[x1 + y1*hexaGrid.w].nbSteps = 0;
		int minSteps = maxSteps+1;
		
		while (toCheck.Count > 0){
			HexaGrid.HexaTemp p = toCheck[0];
			toCheck.RemoveAt(0);
			if (p.nbSteps < maxSteps && p.nbSteps < minSteps){
				for (int i=0;i<6;i++){
					HexaDirection hexaDirectionI = (HexaDirection)i;
					Point p2 = HexaGrid.findPos(p.x,p.y,hexaDirectionI);
					Hexa h = hexaGrid.getHexa(p2);
					if (h != null && ((h.x == x2 && h.y == y2) || (h.type == HexaType.GROUND && h.charOn == null)) && hexaList2[p2.x + p2.y*hexaGrid.w].nbSteps > p.nbSteps+1){
						hexaList2[p2.x + p2.y*hexaGrid.w].nbSteps = p.nbSteps+1;
						if (p2.x == x2 && p2.y == y2) minSteps = p.nbSteps+1;
						toCheck.Add(new HexaGrid.HexaTemp(p2.x,p2.y,p.nbSteps+1));
					}
				}
			}
		}
		
		return (hexaList2[x2+y2*hexaGrid.w].nbSteps == maxSteps) ? -1 : hexaList2[x2+y2*hexaGrid.w].nbSteps;
	}

	/** Returns the position of the hexas where the value is at its maximum in the list of possible hexas.
		Use calculateDamage,... to get v.*/
	public static List<Point> findHexasWhereValueIsMax(List<Point> possibleHexas,int[] v){
		// find best value
		int maxValue = v[possibleHexas[0].x+possibleHexas[0].y*hexaGrid.w];
		foreach (Point p in possibleHexas){
			int vmax = v[p.x+p.y*hexaGrid.w];
			if (vmax > maxValue) maxValue = vmax;
		}
		// find all hexas with best value
		List<Point> bestHexas = new List<Point>();
		foreach (Point p in possibleHexas){
			int vmax = v[p.x+p.y*hexaGrid.w];
			if (vmax == maxValue) bestHexas.Add(p);
		}
		return bestHexas;
	}
	
	/** Returns the position of the hexas where the least amount of potential damage will be taken in the list of possible hexas. */
	public static List<Point> findSafestHexas(List<Point> possibleHexas){
		return null; // TO DO
	}
	
	/** Returns the position of the hexas where the character will be closest to the lowest Enemy in the list of possible hexas. */
	public static List<Point> findHexasClosestToLowestEnemy(int charID,List<Point> possibleHexas){
		Character currentChar = hexaGrid.charList[charID];
		int minDistance = 100000;
		Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
		// find best value
		List<int> possibleHexasValues = new List<int>();
		foreach (Point p in possibleHexas){
			int d = AIUtil.getDistance(p.x,p.y,cLowest.x,cLowest.y);
			possibleHexasValues.Add(d);
			if (d != -1) if (d < minDistance) minDistance = d;
		}
		// find all hexas with best value
		List<Point> bestHexas = new List<Point>();
		for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == minDistance) bestHexas.Add(possibleHexas[i]);
		return bestHexas;
	}
	
	/** Returns the position of the hexas where the character will be closest to allies in the list of possible hexas. */
	public static List<Point> findHexasClosestToAllies(int charID,List<Point> possibleHexas){
		Character currentChar = hexaGrid.charList[charID];
		Point charPos = new Point(currentChar.x,currentChar.y);
		List<Point> bestHexas = new List<Point>();
		int closest = 100000;
		// SOIGNEUR
		if (currentChar.charClass == CharClass.SOIGNEUR){
			List<int> possibleHexasValues = new List<int>();
			foreach (Point p in possibleHexas){
				currentChar.updatePos2(p.x,p.y,hexaGrid);
				Character cLowest = findCharToHeal(charID);
				if (cLowest != null){
					possibleHexasValues.Add(cLowest.HP);
					if (cLowest.HP < closest) closest = cLowest.HP;
				}else{
					possibleHexasValues.Add(100000);
				}
			}
			for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == closest) bestHexas.Add(possibleHexas[i]);
			currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
			return bestHexas;
		// OTHERS
		}else{
			List<int> possibleHexasValues = new List<int>();
			foreach (Point p in possibleHexas){
				currentChar.updatePos2(p.x,p.y,hexaGrid);
				int distance = 0;
				foreach (Character c in hexaGrid.charList){
					if (c.team == currentChar.team && c != currentChar) distance += getDistance(p.x,p.y,c.x,c.y);
				}
				possibleHexasValues.Add(distance);
				if (distance < closest) closest = distance;
			}
			for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == closest) bestHexas.Add(possibleHexas[i]);
			currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
			return bestHexas;
		}
	}

	
	public static List<ActionAIPos> findSequencePathToHexa(int charID,int x,int y){
		Character currentChar = hexaGrid.charList[charID];
		List<ActionAIPos> sequence = new List<ActionAIPos>();
		int nbPA = currentChar.PA;
		if (x == currentChar.x && y == currentChar.y){
			
		}else{
			int d = AIUtil.getDistance(currentChar.x,currentChar.y,x,y);
			List<Point> shortestPath = hexaGrid.findShortestPath(currentChar.x,currentChar.y,x,y,d);
			for (int i=0;i<d && nbPA > 0;i+=currentChar.PM){
				Point destination = shortestPath[((i+currentChar.PM) <= d) ? (i+currentChar.PM) : d];
				sequence.Add(new ActionAIPos(MainGame.ActionType.MOVE,new Point(destination.x,destination.y)));
				nbPA--;
			}
		}
		return sequence;
	}
	
	/** Returns the Enemy with the lowest amount of HP. */
	public static Character findLowestEnemy(int myTeam){
		int lowest = 100000;
		Character cLowest = null;
		foreach (Character c in hexaGrid.charList){
			if (c.team != myTeam && c.HP < lowest){
				lowest = c.HP;
				cLowest = c;
			}
		}
		return cLowest;
	}
	
	/** Returns the ID of the Enemy that either will be killed or be lowest after
		being attacked from the current char pos. */
	public static Character findCharToAttack(int myCharID){
		Character currentChar = hexaGrid.charList[myCharID];
		int lowest = 100000;
		Character cLowest = null;
		foreach (Character c in hexaGrid.charList){
			if (c.team != currentChar.team){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					if (c.HP < lowest){
						lowest = c.HP;
						cLowest = c;
					}
				}
			}
		}
		return cLowest;
	}
	
	/** Returns the ID of the Enemy that either will be killed or be lowest after
		being attacked with skill from the current char pos. */
	public static Character findCharToAttackSkill(int myCharID){
		Character currentChar = hexaGrid.charList[myCharID];
		int lowest = 100000;
		Character cLowest = null;
		foreach (Character c in hexaGrid.charList){
			if (c.team != currentChar.team){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().skill_1.range)){
					if (c.HP < lowest){
						lowest = c.HP;
						cLowest = c;
					}
				}
			}
		}
		return cLowest;
	}
	
	/** Returns the position of the hexa that either will allow the mage to hit
		the highest amount of Enemies from the current pos. */
	public static Point findWhereToAttackMage(int myCharID){
		int maxTargets = 0;
		Character currentChar = hexaGrid.charList[myCharID];
		List<int> possibleHexasValues = new List<int>();
		List<Point> possibleHexas = hexaGrid.findHexasInSight2(currentChar.x,currentChar.y,currentChar.getClassData().basicAttack.range);
		foreach (Point p in possibleHexas){
			List<Character> lc = hexaGrid.getCharWithinRange(p.x,p.y,currentChar.getClassData().basicAttack.rangeAoE);
			int nb = 0;
			// filter allies
			foreach (Character c in lc){
				if (c.team != currentChar.team) nb++;
			}
			possibleHexasValues.Add(nb);
			if (nb > maxTargets) maxTargets = nb;
		}
		List<Point> bestHexas = new List<Point>();
		for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == maxTargets) bestHexas.Add(possibleHexas[i]);
		
		if (bestHexas.Count > 0){
			return bestHexas[0]; // Improve by finding the best one to return
		}else{
			return null;
		}
	}
	
	/** Returns the position of the hexa that either will allow the mage to hit
		the highest amount of Enemies from the current pos. */
	public static Point findWhereToAttackMageSkill(int myCharID){
		int maxTargets = 0;
		Character currentChar = hexaGrid.charList[myCharID];
		List<int> possibleHexasValues = new List<int>();
		List<Point> possibleHexas = hexaGrid.findHexasInSight2(currentChar.x,currentChar.y,currentChar.getClassData().skill_1.range);
		foreach (Point p in possibleHexas){
			List<Character> lc = hexaGrid.getCharWithinRange(p.x,p.y,currentChar.getClassData().skill_1.rangeAoE);
			int nb = 0;
			// filter allies
			foreach (Character c in lc){
				if (c.team != currentChar.team) nb++;
			}
			possibleHexasValues.Add(nb);
			if (nb > maxTargets) maxTargets = nb;
		}
		List<Point> bestHexas = new List<Point>();
		for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == maxTargets) bestHexas.Add(possibleHexas[i]);
		
		if (bestHexas.Count > 0){
			return bestHexas[0]; // Improve by finding the best one to return
		}else{
			return null;
		}
	}
	
	/** Returns the position of the hexa that either will allow the soigneur to heal
		the highest amount of allies from the current pos (aoe skill). */
	public static Point findWhereToHealSoigneurSkill(int myCharID){
		int maxTargets = 0;
		Character currentChar = hexaGrid.charList[myCharID];
		List<int> possibleHexasValues = new List<int>();
		List<Point> possibleHexas = hexaGrid.findHexasInSight2(currentChar.x,currentChar.y,currentChar.getClassData().skill_1.range);
		foreach (Point p in possibleHexas){
			List<Character> lc = hexaGrid.getCharWithinRange(p.x,p.y,currentChar.getClassData().skill_1.rangeAoE);
			int nb = 0;
			// filter Enemies/self
			foreach (Character c in lc){
				if (c.team == currentChar.team && c != currentChar) nb++;
			}
			possibleHexasValues.Add(nb);
			if (nb > maxTargets) maxTargets = nb;
		}
		List<Point> bestHexas = new List<Point>();
		for (int i=0;i<possibleHexas.Count;i++) if (possibleHexasValues[i] == maxTargets) bestHexas.Add(possibleHexas[i]);
		
		if (bestHexas.Count > 0){
			return bestHexas[0]; // Improve by finding the best one to return
		}else{
			return null;
		}
	}
	
	/** Returns the ID of the ally that can be healed for the most
		from the current char pos assuming the character is a healer. */
	public static Character findCharToHeal(int myCharID){
		Character currentChar = hexaGrid.charList[myCharID];
		int highest = 0;
		Character cHighest = null;
		foreach (Character c in hexaGrid.charList){
			if (c.team == currentChar.team && c != currentChar){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					if (c.HPmax-c.HP > highest){
						highest = c.HP;
						cHighest = c;
					}
				}
			}
		}
		return cHighest;
	}
	
	/** Returns the ID of the ally that can be buffed for the most
		from the current char pos assuming the character is a envouteur. */
	public static Character findCharToBuff(int myCharID){
		Character currentChar = hexaGrid.charList[myCharID];
		int highest = 0;
		Character cHighest = null;
		foreach (Character c in hexaGrid.charList){
			if (c.team == currentChar.team && c != currentChar){
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,c.x,c.y,currentChar.getClassData().basicAttack.range)){
					int classPrio;
					switch (c.charClass){
						case CharClass.GUERRIER  : classPrio = 3; break;
						case CharClass.VOLEUR    : classPrio = 6; break;
						case CharClass.ARCHER    : classPrio = 4; break;
						case CharClass.MAGE      : classPrio = 5; break;
						case CharClass.SOIGNEUR  : classPrio = 2; break;
						case CharClass.ENVOUTEUR : classPrio = 1; break;
						default : classPrio = 0; break;
					}
					if (classPrio > highest){
						highest = classPrio;
						cHighest = c;
					}
				}
			}
		}
		return cHighest;
	}

	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------
	// Functions used for AI HARD
	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------

	public static int calculateThreatAtHexa(int x,int y,int charID){
		/*Character currentChar = hexaGrid.charList[charID];
		int threat = 0;
		bool stop = false;
		for (int j=charID+1;;j++){
			Character c = hexaGrid.charList[j%(hexaGrid.charList.Count)];
			if (c == currentChar) break;
			
			if (c.team != currentChar.team && c.charClass != CharClass.SOIGNEUR && c.charClass != CharClass.ENVOUTEUR){
				int damage = 0;
				// 0 PM
				if (c.skillAvailable){
					if (hexaGrid.hexaInSight(c.x,c.y,x,y,c.getClassData().skill_1.range)){
						damage = c.getClassData().skill_1.effectValue * c.PA;
					}
					if (hexaGrid.hexaInSight(c.x,c.y,x,y,c.getClassData().basicAttack.range)){
						damage += c.getClassData().basicAttack.effectValue * (c.PA-1);
					}
				}else{
					if (hexaGrid.hexaInSight(c.x,c.y,x,y,c.getClassData().basicAttack.range)){
						damage = c.getClassData().basicAttack.effectValue * c.PA;
					}
				}
				
				// 1+ PM
				for (int i=1;i<c.PA && damage == 0;i++){
					Point realCharPos = new Point(c.x,c.y);
					List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
					foreach (Point charpos in listH){
						c.updatePos2(charPos.x,charPos.y,hexaGrid);
						if (c.skillAvailable){
							if (hexaGrid.hexaInSight(charpos.x,charpos.y,x,y,c.getClassData().skill_1.range)){
								damage = c.getClassData().skill_1.effectValue * c.PA;
							}
							if (hexaGrid.hexaInSight(charpos.x,charpos.y,x,y,c.getClassData().basicAttack.range)){
								damage += c.getClassData().basicAttack.effectValue * (c.PA-1);
							}
							if (damage > 0) break;
						}else{
							if (hexaGrid.hexaInSight(charpos.x,charpos.y,x,y,c.getClassData().basicAttack.range)){
								damage = c.getClassData().basicAttack.effectValue * c.PA;
								break;
							}
						}
					}
					c.updatePos2(realCharPos.x,realCharPos.y,hexaGrid);
				}
				threat -= damage;
				if (currentChar.HP - threat <= 0) return -currentChar.HP - 10;
			}else if (c.team == currentChar.team && c.charClass == CharClass.SOIGNEUR){
				
			}
		}
		return threat;*/
		return 0;
	}
	
	/** return the maximum amount of targets from the current character's position. */
	public static int getNbMaxTargets(int charID){
		int maxTargets = 0;
		Character currentChar = hexaGrid.charList[charID];
		if (currentChar.skillAvailable){
			List<Point> hexas = hexaGrid.findHexasInSight2(currentChar.x,currentChar.y,currentChar.getClassData().skill_1.range);
			foreach (Point p in hexas){
				List<Character> lc = hexaGrid.getCharWithinRange(p.x,p.y,currentChar.getClassData().skill_1.rangeAoE);
				int nb = 0;
				// filter allies / Enemies
				if (currentChar.getClassData().skill_1.targetsEnemies){
					foreach (Character c in lc){
						if (c.team != currentChar.team) nb++;
					}
				}
				if (currentChar.getClassData().skill_1.targetsAllies){
					foreach (Character c in lc){
						if (c.team == currentChar.team && c != currentChar) nb++;
					}
				}
				// Soigneur : filter allies with full hp
				if (currentChar.charClass == CharClass.SOIGNEUR){
					foreach (Character c in lc){
						if (c.team == currentChar.team && c != currentChar && c.HP == c.HPmax) nb--;
					}
				}
				if (nb > maxTargets) maxTargets = nb;
			}
		}else{
			List<Point> hexas = hexaGrid.findHexasInSight2(currentChar.x,currentChar.y,currentChar.getClassData().basicAttack.range);
			foreach (Point p in hexas){
				List<Character> lc = hexaGrid.getCharWithinRange(p.x,p.y,currentChar.getClassData().basicAttack.rangeAoE);
				int nb = 0;
				// filter allies / Enemies
				if (currentChar.getClassData().basicAttack.targetsEnemies){
					foreach (Character c in lc){
						if (c.team != currentChar.team) nb++;
					}
				}
				if (currentChar.getClassData().basicAttack.targetsAllies){
					foreach (Character c in lc){
						if (c.team == currentChar.team && c != currentChar) nb++;
					}
				}
				// Soigneur : filter allies with full hp
				if (currentChar.charClass == CharClass.SOIGNEUR){
					foreach (Character c in lc){
						if (c.team == currentChar.team && c != currentChar && c.HP == c.HPmax) nb--;
					}
				}
				if (nb > maxTargets) maxTargets = nb;
			}
		}
		return maxTargets;
	}
	
	public static bool isCharWithinRangeAttack(int charID,int targetID){
		Character currentChar = hexaGrid.charList[charID];
		Character targetChar = hexaGrid.charList[targetID];
		Point charPos = new Point(currentChar.x,currentChar.y);
		
		if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,targetChar.x,targetChar.y,currentChar.getClassData().basicAttack.range)) return true;
		int i = currentChar.PM * (currentChar.PA-1);
		if (i > 0){
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,i);
			foreach (Point charpos in listH){
				currentChar.updatePos2(charpos.x,charpos.y,hexaGrid);
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,targetChar.x,targetChar.y,currentChar.getClassData().basicAttack.range)){
					currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
					return true;
				}
			}
		}
		currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
		return false;
	}
	
	public static bool isCharWithinRangeSkill(int charID,int targetID){
		Character currentChar = hexaGrid.charList[charID];
		Character targetChar = hexaGrid.charList[targetID];
		Point charPos = new Point(currentChar.x,currentChar.y);
		
		if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,targetChar.x,targetChar.y,currentChar.getClassData().skill_1.range)) return true;
		int i = currentChar.PM * (currentChar.PA-1);
		if (i > 0){
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,i);
			foreach (Point charpos in listH){
				currentChar.updatePos2(charpos.x,charpos.y,hexaGrid);
				if (hexaGrid.hexaInSight(currentChar.x,currentChar.y,targetChar.x,targetChar.y,currentChar.getClassData().skill_1.range)){
					currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
					return true;
				}
			}
		}
		currentChar.updatePos2(charPos.x,charPos.y,hexaGrid);
		return false;
	}
	
	public static List<Point> findHexasWhereDamageIsHighest(int charID,List<Point> possibleHexas){
		Character currentChar = hexaGrid.charList[charID];
		int minDistance = 100000;
		Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
		// find best value
		List<int> possibleHexasValues = new List<int>();
		foreach (Point p in possibleHexas){
			int d = AIUtil.getDistance(p.x,p.y,cLowest.x,cLowest.y);
			possibleHexasValues.Add(d);
			if (d != -1) if (d < minDistance) minDistance = d;
		}
		// find all hexas with best value
		List<Point> bestHexas = new List<Point>();
		for (int i=0;i<possibleHexas.Count;i++){
			if (possibleHexasValues[i] == minDistance){
				bestHexas.Add(possibleHexas[i]);
			}
		}
		return bestHexas;
	}
	
	public class AIHard {
		public static ActionAIPos doApproachEnemy(int myCharID){
			Character currentChar = hexaGrid.charList[myCharID];
			Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM);
			if (listH != null && listH.Count > 0){
				// Find hexas where damage dealt is highest
				List<Point> bestHexas  = AIUtil.findHexasWhereValueIsMax(listH,AIUtil.calculateDamage(myCharID));
				// Find hexas where position to lowest Enemy is lowest
				List<Point> bestHexas2 = AIUtil.findHexasClosestToLowestEnemy(myCharID,bestHexas);
				Point bestHexa = bestHexas2[0];
				if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y) return new ActionAIPos(MainGame.ActionType.SKIP,null);
				else return new ActionAIPos(MainGame.ActionType.MOVE,bestHexa);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
		
		public static ActionAIPos doApproachAlly(int myCharID){
			Character currentChar = hexaGrid.charList[myCharID];
			Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM);
			if (listH != null && listH.Count > 0){
				// Find hexas where position is closest to allies
				List<Point> bestHexas  = AIUtil.findHexasClosestToAllies(myCharID,listH);
				// Find hexas where threat is lowest
				int[] threat = AIUtil.calculateThreat(myCharID);
				for (int i=0;i<threat.Length;i++) threat[i] = - threat[i];
				List<Point> bestHexas2  = AIUtil.findHexasWhereValueIsMax(bestHexas,threat);
				// Find hexas where position to lowest Enemy is lowest
				List<Point> bestHexas3 = AIUtil.findHexasClosestToLowestEnemy(myCharID,bestHexas2);
				Point bestHexa = bestHexas3[0];
				if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y) return new ActionAIPos(MainGame.ActionType.SKIP,null);
				else return new ActionAIPos(MainGame.ActionType.MOVE,bestHexa);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
		
		public static ActionAIPos doRandomMovement(int myCharID){
			return new ActionAIPos(MainGame.ActionType.SKIP,null);
		}
		
		public static ActionAIPos doFlee(int myCharID){
			Character currentChar = hexaGrid.charList[myCharID];
			Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
			List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM);
			if (listH != null && listH.Count > 0){
				// Find hexas where threat is lowest
				int[] threat = AIUtil.calculateThreat(myCharID);
				for (int i=0;i<threat.Length;i++) threat[i] = - threat[i];
				List<Point> bestHexas  = AIUtil.findHexasWhereValueIsMax(listH,threat);
				// Find hexas where position is closest to allies
				List<Point> bestHexas2 = AIUtil.findHexasClosestToAllies(myCharID,bestHexas);
				// Find hexas where position is closest to lowest Enemy
				List<Point> bestHexas3 = AIUtil.findHexasClosestToLowestEnemy(myCharID,bestHexas2);
				Point bestHexa = bestHexas3[0];
				if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y) return new ActionAIPos(MainGame.ActionType.SKIP,null);
				else return new ActionAIPos(MainGame.ActionType.MOVE,bestHexa);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
		
		public static ActionAIPos doAttack(int myCharID,int targetID){
			Character currentChar = hexaGrid.charList[myCharID];
			Point posAttack = null;
			switch (currentChar.charClass){
				case CharClass.GUERRIER :
				case CharClass.VOLEUR :
				case CharClass.ARCHER : {
					Character cAttack = AIUtil.findCharToAttack(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.MAGE : {
					posAttack = AIUtil.findWhereToAttackMage(myCharID);
				} break;
				case CharClass.SOIGNEUR : {
					Character cAttack = AIUtil.findCharToHeal(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.ENVOUTEUR: {
					Character cAttack = AIUtil.findCharToBuff(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
			}
			if (posAttack != null){
				return new  ActionAIPos(MainGame.ActionType.ATK1,posAttack);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
		
		public static ActionAIPos doSkill(int myCharID,int targetID){
			Character currentChar = hexaGrid.charList[myCharID];
			Point posAttack = null;
			switch (currentChar.charClass){
				case CharClass.GUERRIER :
				case CharClass.VOLEUR :
				case CharClass.ARCHER : {
					Character cAttack = AIUtil.findCharToAttackSkill(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.MAGE : {
					posAttack = AIUtil.findWhereToAttackMageSkill(myCharID);
				} break;
				case CharClass.SOIGNEUR : {
					posAttack = AIUtil.findWhereToHealSoigneurSkill(myCharID); // Add skill function
				} break;
				case CharClass.ENVOUTEUR: {
					posAttack = AIUtil.findWhereToHealSoigneurSkill(myCharID); // Add skill function
				} break;
			}
			if (posAttack != null){
				return new ActionAIPos(MainGame.ActionType.ATK2,posAttack);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
	}
	
	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------
	// 2.0
	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	public static bool canTarget(int cX,int cY,int targetX,int targetY,int range,int rangeAoE){
		if (rangeAoE > 0){
			if (hexaGrid.hexaInSight(cX,cY,targetX,targetY,range)){
				return true;
			}else{
				List<Point> hexas = hexaGrid.findHexasInSight2(cX,cY,range);
				foreach (Point h in hexas){
					List<Character> chars = hexaGrid.getCharWithinRange(h.x,h.y,rangeAoE);
					foreach (Character c2 in chars){
						if (c2.x == targetX && c2.y == targetY) return true;
					}
				}
				return false;
			}
		}else{
			return hexaGrid.hexaInSight(cX,cY,targetX,targetY,range);
		}
	}
	
	public static bool canTargetAttack(Character c,Character target){
		switch (c.charClass){
			case CharClass.GUERRIER :
			case CharClass.VOLEUR :
			case CharClass.ARCHER : 
			case CharClass.MAGE :
				return target.team != c.team && canTarget(c.x,c.y,target.x,target.y,c.getClassData().basicAttack.range,c.getClassData().basicAttack.rangeAoE);
			case CharClass.SOIGNEUR :
				return target.team == c.team && target.HP < target.HPmax && canTarget(c.x,c.y,target.x,target.y,c.getClassData().basicAttack.range,c.getClassData().basicAttack.rangeAoE);
			case CharClass.ENVOUTEUR :
				return target.team == c.team && target.PA <= target.getClassData().basePA && canTarget(c.x,c.y,target.x,target.y,c.getClassData().basicAttack.range,c.getClassData().basicAttack.rangeAoE);
			default : return false;
		}
	}
	
	public static bool canTargetSkill(Character c,Character target){
		if (c.skillAvailable){
			switch (c.charClass){
				case CharClass.GUERRIER :
				case CharClass.VOLEUR :
				case CharClass.ARCHER : 
				case CharClass.MAGE :
					return target.team != c.team && canTarget(c.x,c.y,target.x,target.y,c.getClassData().skill_1.range,c.getClassData().skill_1.rangeAoE);
				case CharClass.SOIGNEUR :
					return target.team == c.team && target.HP < target.HPmax && canTarget(c.x,c.y,target.x,target.y,c.getClassData().skill_1.range,c.getClassData().skill_1.rangeAoE);	
				case CharClass.ENVOUTEUR :
					return target.team == c.team && target.PA <= target.getClassData().basePA && canTarget(c.x,c.y,target.x,target.y,c.getClassData().skill_1.range,c.getClassData().skill_1.rangeAoE);
				default : return false;
			}
		}else{
			return false;
		}
	}
	
	public static bool canTargetWithMovementAttack(Character c,Character target){
		int cX = c.x; int cY = c.y;
		for (int i=1;i<c.PA;i++){
			List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
			foreach (Point charPos in listH){
				c.updatePos2(charPos.x,charPos.y,hexaGrid);
				if (canTargetAttack(c,target)){
					c.updatePos2(cX,cY,hexaGrid);
					return true;
				}
			}
		}
		c.updatePos2(cX,cY,hexaGrid);
		return false;
	}
	
	public static bool canTargetWithMovementSkill(Character c,Character target){
		if (c.skillAvailable){
			int cX = c.x; int cY = c.y;
			for (int i=1;i<c.PA;i++){
				List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
				foreach (Point charPos in listH){
					c.updatePos2(charPos.x,charPos.y,hexaGrid);
					if (canTargetSkill(c,target)){
						c.updatePos2(cX,cY,hexaGrid);
						return true;
					}
				}
			}
			c.updatePos2(cX,cY,hexaGrid);
		}
		return false;
	}
	
	/** Calculates the threat if the character ends his turn at hexa (x,y).
		returns a value the indicates an estimation of the amount of HP potentially
		gained until next turn. (negative values indicate damage taken).
		If the value -1000 is returned, the character would die. */
	public static int threatAtHexa(int x,int y,int currentCharID){
		Character currentChar = hexaGrid.charList[currentCharID];
		int currentCharHP = currentChar.HP;
		int currentCharX = currentChar.x;
		int currentCharY = currentChar.y;
		currentChar.updatePos2(x,y,hexaGrid);
		
		int threat = 0;
		for (int j=currentCharID+1;;j++){
			Character c = hexaGrid.charList[j%(hexaGrid.charList.Count)];
			if (c == currentChar) break;
			
			// damage
			if (c.team != currentChar.team && c.charClass != CharClass.SOIGNEUR && c.charClass != CharClass.ENVOUTEUR){
				int damage = 0;
				// 0 PM
				{
					bool skill = canTargetSkill(c,currentChar);
					bool attack = canTargetAttack(c,currentChar);
					damage = ((skill) ? (c.getClassData().skill_1.effectValue) : 0) + ((attack) ? (c.getClassData().basicAttack.effectValue * ((skill) ? (c.PA-1) : (c.PA))) : 0);
				}
				
				// 1+ PM
				int cX = c.x; int cY = c.y;
				for (int i=1;i<c.PA && damage == 0;i++){
					List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
					foreach (Point charPos in listH){
						c.updatePos2(charPos.x,charPos.y,hexaGrid);
						bool skill = canTargetSkill(c,currentChar);
						bool attack = canTargetAttack(c,currentChar);
						damage = ((skill) ? (c.getClassData().skill_1.effectValue) : 0) + ((attack) ? (c.getClassData().basicAttack.effectValue * ((skill) ? (c.PA-i-1) : (c.PA-i))) : 0);
						if (damage > 0) break;
					}
					c.updatePos2(cX,cY,hexaGrid);
				}
				
				currentChar.HP -= damage;
				if (currentChar.HP <= 0) break;
			// heal
			}else if (c.team == currentChar.team && c.charClass == CharClass.SOIGNEUR){
				int heal = 0;
				// 0 PM
				{
					bool skill = canTargetSkill(c,currentChar);
					bool attack = canTargetAttack(c,currentChar);
					heal = ((skill) ? (c.getClassData().skill_1.effectValue) : 0) + ((attack) ? (c.getClassData().basicAttack.effectValue * ((skill) ? (c.PA-1) : (c.PA))) : 0);
				}
				
				// 1+ PM
				int cX = c.x; int cY = c.y;
				for (int i=1;i<c.PA && heal == 0;i++){
					Point realCharPos = new Point(c.x,c.y);
					List<Point> listH = hexaGrid.findAllPaths(c.x,c.y,c.PM*i);
					foreach (Point charPos in listH){
						c.updatePos2(charPos.x,charPos.y,hexaGrid);
						bool skill = canTargetSkill(c,currentChar);
						bool attack = canTargetAttack(c,currentChar);
						heal = ((skill) ? (c.getClassData().skill_1.effectValue) : 0) + ((attack) ? (c.getClassData().basicAttack.effectValue * ((skill) ? (c.PA-i-1) : (c.PA-i))) : 0);
						if (heal > 0) break;
					}
					c.updatePos2(cX,cY,hexaGrid);
				}
				
				currentChar.HP += heal;
				if (currentChar.HP > currentChar.HPmax) currentChar.HP = currentChar.HPmax;
			}
		}
		
		currentChar.updatePos2(currentCharX,currentCharY,hexaGrid);
		if (currentChar.HP <= 0) threat = -1000;
		else threat = currentChar.HP - currentCharHP;
		currentChar.HP = currentCharHP;
		return threat;
	}
	
	public static List<HexaDamage> threatAtHexas(List<Point> hexas,int currentCharID){
		List<HexaDamage> r = new List<HexaDamage>();
		foreach (Point h in hexas) r.Add(new HexaDamage(h.x,h.y,threatAtHexa(h.x,h.y,currentCharID)));
		return r;
	}
	
	public static List<Point> getHexasWhereThreatIsMin(List<Point> hexas,int currentCharID){
		List<Point> hexas2 = new List<Point>();
		if (hexas.Count > 0){
			List<HexaDamage> hexas2_ = threatAtHexas(hexas,currentCharID);
			int best = hexas2_[0].value;
			foreach (HexaDamage hd in hexas2_) if (hd.value >  best) best = hd.value;
			foreach (HexaDamage hd in hexas2_) if (hd.value == best) hexas2.Add(new Point(hd.x,hd.y));
		}
		return hexas2;
	}
	
	/** Can be used to know where an enemy is within range of being attacked.
		Can also be used to know where a SOINGEUR is in range to heal an ally. */
	public static List<Point> getHexasWhereCharIsInRange(List<Point> hexas,Character c,Character target){
		List<Point> hexas2 = new List<Point>();
		if (hexas.Count > 0){
			int cX = c.x; int cY = c.y;
			foreach (Point p in hexas){
				c.updatePos2(p.x,p.y,hexaGrid);
				if (canTargetAttack(c,target) || canTargetSkill(c,target)) hexas2.Add(new Point(p.x,p.y));
			}
			c.updatePos2(cX,cY,hexaGrid);
		}
		return hexas2;
	}
	
	/** Returns the hexas with the lowest amount of PA to use to get to*/
	public static List<Point> getHexasWhereMovementIsLowest(List<Point> hexas,int charID){
		List<Point> r = new List<Point>();
		if (hexas.Count > 0){
			List<int> values = new List<int>();
			foreach (Point h in hexas) values.Add(findSequencePathToHexa(charID,h.x,h.y).Count);
			int lowest = values[0];
			for (int i=0;i<values.Count;i++) if (values[i] < lowest) lowest = values[i];
			for (int i=0;i<values.Count;i++) if (values[i] == lowest) r.Add(new Point(hexas[i].x,hexas[i].y));
		}
		return r;
	}
	
	public static List<Character> getTargetableCharsInRangeAttack(Character c){
		List<Character> r = new List<Character>();
		foreach (Character c2 in hexaGrid.charList) if (canTargetAttack(c,c2)) r.Add(c2);
		return r;
	}
	
	public static List<Character> getTargetableCharsInRangeSkill(Character c){
		List<Character> r = new List<Character>();
		foreach (Character c2 in hexaGrid.charList) if (canTargetSkill(c,c2)) r.Add(c2);
		return r;
	}
	
	public static Character getMainTargetAttack(Character c){
		List<Character> l = getTargetableCharsInRangeAttack(c);
		if (l.Count > 0){
			int minHP = l[0].HP;
			foreach (Character c2 in l) if (c2.HP <  minHP) minHP = c2.HP;
			foreach (Character c2 in l) if (c2.HP == minHP) return c2;
			return l[0];
		}else{
			return null;
		}
	}
	
	public static Character getMainTargetSkill(Character c){
		List<Character> l = getTargetableCharsInRangeSkill(c);
		if (l.Count > 0){
			int minHP = l[0].HP;
			foreach (Character c2 in l) if (c2.HP <  minHP) minHP = c2.HP;
			foreach (Character c2 in l) if (c2.HP == minHP) return c2;
			return l[0];
		}else{
			return null;
		}
	}
	
	/** Returns the hexa that should be targeted to attack. Assumes mainTarget is targetable*/
	public static Point getPosToUseAttack(Character c,Character mainTarget){
		if (c.getClassData().basicAttack.rangeAoE > 0){
			List<Point> hexas = hexaGrid.findHexasInSight2(c.x,c.y,c.getClassData().basicAttack.range);
			HexaDamage pos = new HexaDamage(hexas[0].x,hexas[0].y,0);
			foreach (Point h in hexas){
				List<Character> chars = hexaGrid.getCharWithinRange(h.x,h.y,c.getClassData().basicAttack.rangeAoE);
				
				if (!c.getClassData().basicAttack.targetsEnemies){
					for (int i=0;i<chars.Count;i++){
						if (chars[i].team != c.team){
							chars.RemoveAt(i); i--;
						}
					}
				}
				if (!c.getClassData().basicAttack.targetsAllies){
					for (int i=0;i<chars.Count;i++){
						if (chars[i].team == c.team){
							chars.RemoveAt(i); i--;
						}
					}
				}
				
				foreach (Character c2 in chars){
					if (c2 == mainTarget){
						if (chars.Count > pos.value){
							pos.x = h.x;
							pos.y = h.y;
							pos.value = chars.Count;
						}
					}
				}
			}
			return new Point(pos.x,pos.y);
		}else{
			return new Point(mainTarget.x,mainTarget.y);
		}
	}
	
	/** Returns the hexa that should be targeted to attack (skill). Assumes mainTarget is targetable*/
	public static Point getPosToUseSkill(Character c,Character mainTarget){
		if (c.getClassData().skill_1.rangeAoE > 0){
			List<Point> hexas = hexaGrid.findHexasInSight2(c.x,c.y,c.getClassData().skill_1.range);
			HexaDamage pos = new HexaDamage(hexas[0].x,hexas[0].y,0);
			foreach (Point h in hexas){
				List<Character> chars = hexaGrid.getCharWithinRange(h.x,h.y,c.getClassData().skill_1.rangeAoE);
				
				if (!c.getClassData().skill_1.targetsEnemies){
					for (int i=0;i<chars.Count;i++){
						if (chars[i].team != c.team){
							chars.RemoveAt(i); i--;
						}
					}
				}
				if (!c.getClassData().skill_1.targetsAllies){
					for (int i=0;i<chars.Count;i++){
						if (chars[i].team == c.team){
							chars.RemoveAt(i); i--;
						}
					}
				}
				
				foreach (Character c2 in chars){
					if (c2 == mainTarget){
						if (chars.Count > pos.value){
							pos.x = h.x;
							pos.y = h.y;
							pos.value = chars.Count;
						}
					}
				}
			}
			return new Point(pos.x,pos.y);
		}else{
			return new Point(mainTarget.x,mainTarget.y);
		}
	}
	
	public class AIHard2 {
		public static List<ActionAIPos> doApproachTarget(int myCharID){
			Character currentChar = hexaGrid.charList[myCharID];
			Character target = findLowestEnemy(currentChar.team); // <- TO CHANGE 
			List<Point> hexas1  = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
			List<Point> hexas2 = getHexasWhereCharIsInRange(hexas1,currentChar,target);
			List<Point> hexas3 = getHexasWhereMovementIsLowest(hexas2,myCharID);
			List<Point> hexas4 = getHexasWhereThreatIsMin(hexas3,myCharID);
			
			List<ActionAIPos> sequence;
			if (hexas4.Count > 0){
				sequence = findSequencePathToHexa(myCharID,hexas4[0].x,hexas4[0].y);
				sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
			}else{
				List<Point> bestHexas2 = AIUtil.findHexasClosestToLowestEnemy(myCharID,hexas1);
				sequence = findSequencePathToHexa(myCharID,bestHexas2[0].x,bestHexas2[0].y);
				sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
			}
			return sequence;
		}
		
		public static List<ActionAIPos> doFlee(int myCharID){
			Character currentChar = hexaGrid.charList[myCharID];
			List<Point> hexas1  = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
			List<Point> hexas2 = getHexasWhereThreatIsMin(hexas1,myCharID);
			List<Point> hexas3 = getHexasWhereMovementIsLowest(hexas2,myCharID);
			List<Point> hexas4 = findHexasClosestToAllies(myCharID,hexas3);
			
			List<ActionAIPos> sequence = findSequencePathToHexa(myCharID,hexas4[0].x,hexas4[0].y);
			if (sequence.Count < currentChar.PA){
				// attack before
			}
			sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
			return sequence;
		}
		
		public static ActionAIPos doAttack(int myCharID,int targetID){
			Character currentChar = hexaGrid.charList[myCharID];
			Point posAttack = null;
			switch (currentChar.charClass){
				case CharClass.GUERRIER :
				case CharClass.VOLEUR :
				case CharClass.ARCHER : {
					Character cAttack = AIUtil.findCharToAttack(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.MAGE : {
					posAttack = AIUtil.findWhereToAttackMage(myCharID);
				} break;
				case CharClass.SOIGNEUR : {
					Character cAttack = AIUtil.findCharToHeal(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.ENVOUTEUR: {
					Character cAttack = AIUtil.findCharToBuff(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
			}
			if (posAttack != null){
				return new  ActionAIPos(MainGame.ActionType.ATK1,posAttack);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
		
		public static ActionAIPos doSkill(int myCharID,int targetID){
			Character currentChar = hexaGrid.charList[myCharID];
			Point posAttack = null;
			switch (currentChar.charClass){
				case CharClass.GUERRIER :
				case CharClass.VOLEUR :
				case CharClass.ARCHER : {
					Character cAttack = AIUtil.findCharToAttackSkill(myCharID);
					if (cAttack != null) posAttack = new Point(cAttack.x,cAttack.y);
				} break;
				case CharClass.MAGE : {
					posAttack = AIUtil.findWhereToAttackMageSkill(myCharID);
				} break;
				case CharClass.SOIGNEUR : {
					posAttack = AIUtil.findWhereToHealSoigneurSkill(myCharID); // Add skill function
				} break;
				case CharClass.ENVOUTEUR: {
					posAttack = AIUtil.findWhereToHealSoigneurSkill(myCharID); // Add skill function
				} break;
			}
			if (posAttack != null){
				return new ActionAIPos(MainGame.ActionType.ATK2,posAttack);
			}else{
				return new ActionAIPos(MainGame.ActionType.SKIP,null);
			}
		}
	}
}

}