using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexas;
using Misc;
using Characters;
using AI_Util;
using Classifiers;
using Stats;
using static MainGame;

namespace AI_Class {
	
	
public class ActionAIPos{
	public MainGame.ActionType action;
	public Point pos;
	
	public ActionAIPos(MainGame.ActionType action,Point pos){
		this.action = action;
		this.pos = pos;
	}
}

public class AI{
	public static HexaGrid hexaGrid;
	
	
	
	public static List<ActionAIPos> decide(int charID){
		List<ActionAIPos> sequence = new List<ActionAIPos>();
		sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
		return sequence;
	}
}

public class AIEasy : AI {
	new public static List<ActionAIPos> decide(int charID){
		Character currentChar = hexaGrid.charList[charID];
		List<ActionAIPos> sequence = new List<ActionAIPos>();
		
		switch (currentChar.charClass){
			case CharClass.GUERRIER :
			case CharClass.VOLEUR :
			case CharClass.ARCHER :
			case CharClass.MAGE : {
				int[] damage = AIUtil.calculateDamage(charID);
				List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
				if (listH != null && listH.Count > 0){
					// Find hexas where damage dealt is highest
					List<Point> bestHexas  = AIUtil.findHexasWhereValueIsMax(listH,damage);
					// Find hexas where position to lowest Enemy is lowest
					List<Point> bestHexas2 = AIUtil.findHexasClosestToLowestEnemy(charID,bestHexas);
					Point bestHexa = bestHexas2[0];
					// find path to hexa
					sequence = AIUtil.findSequencePathToHexa(charID,bestHexa.x,bestHexa.y);
					// Attack the Enemy
					int nbPA = currentChar.PA - sequence.Count;
					if (nbPA != 0){
						for (int i=0;i<nbPA;i++){
							Character cAttack = AIUtil.findCharToAttack(charID);
							if (cAttack != null){
								sequence.Add(new ActionAIPos(MainGame.ActionType.ATK1,new Point(cAttack.x,cAttack.y)));
							}else if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y){
								sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
							}
						}
					}
				}else{
					sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
				}
			} break;
			case CharClass.SOIGNEUR : {
				int[] healing = AIUtil.calculateHealing(charID);
				List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
				if (listH != null && listH.Count > 0){
					// Find hexas where healing done is highest
					List<Point> bestHexas  = AIUtil.findHexasWhereValueIsMax(listH,healing);
					// Find hexas where position to lowest Enemy is lowest
					List<Point> bestHexas2 = AIUtil.findHexasClosestToLowestEnemy(charID,bestHexas);
					Point bestHexa = bestHexas2[0];
					// find path to hexa
					sequence = AIUtil.findSequencePathToHexa(charID,bestHexa.x,bestHexa.y);
					int nbPA = currentChar.PA - sequence.Count;
					// Heal allies
					if (nbPA != 0){
						for (int i=0;i<nbPA;i++){
							Character cHeal = AIUtil.findCharToHeal(charID);
							if (cHeal != null){
								sequence.Add(new ActionAIPos(MainGame.ActionType.ATK1,new Point(cHeal.x,cHeal.y)));
							}else if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y){
								sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
							}
						}
					}
				}else{
					sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
				}
			} break;
			// TO DO
			case CharClass.ENVOUTEUR : {
				int[] healing = AIUtil.calculateBuff(charID);
				List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
				if (listH != null && listH.Count > 0){
					// Find hexas where healing done is highest
					List<Point> bestHexas  = AIUtil.findHexasWhereValueIsMax(listH,healing);
					// Find hexas where position to lowest Enemy is lowest
					List<Point> bestHexas2 = AIUtil.findHexasClosestToLowestEnemy(charID,bestHexas);
					Point bestHexa = bestHexas2[0];
					// find path to hexa
					sequence = AIUtil.findSequencePathToHexa(charID,bestHexa.x,bestHexa.y);
					int nbPA = currentChar.PA - sequence.Count;
					// Buff allies
					if (nbPA != 0){
						for (int i=0;i<nbPA;i++){
							Character cBuff = AIUtil.findCharToBuff(charID);
							if (cBuff != null){
								sequence.Add(new ActionAIPos(MainGame.ActionType.ATK1,new Point(cBuff.x,cBuff.y)));
							}else if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y){
								sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
							}
						}
					}
				}else{
					sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
				}
			} break;
		}
		
		return sequence;
	}
}

public class AIMedium : AI{
	new public static List<ActionAIPos> decide(int charID){
		Character currentChar = hexaGrid.charList[charID];
		List<ActionAIPos> sequence = new List<ActionAIPos>();
		
		switch (currentChar.charClass){
			case CharClass.GUERRIER :
			case CharClass.VOLEUR :
			case CharClass.ARCHER :
			case CharClass.MAGE : {
				int[] threat = AIUtil.calculateThreat(currentChar.team);
				int[] damage = AIUtil.calculateDamage(charID);
				int[] dif = new int[hexaGrid.w*hexaGrid.h];
				
				for (int i=0;i<dif.Length;i++){
					dif[i] = (int)(1.5f*damage[i] - (float)threat[i]);
				}
				
				List<Point> listH = hexaGrid.findAllPaths(currentChar.x,currentChar.y,currentChar.PM*currentChar.PA);
				if (listH != null && listH.Count > 0){
					// find best value
					int maxValue = dif[listH[0].x+listH[0].y*hexaGrid.w];
					foreach (Point p in listH){
						int v = dif[p.x+p.y*hexaGrid.w];
						if (v > maxValue) maxValue = v;
					}
					// find all hexas with best value
					List<Point> bestHexas = new List<Point>();
					foreach (Point p in listH){
						int v = dif[p.x+p.y*hexaGrid.w];
						if (v == maxValue) bestHexas.Add(p);
					}
					// find the hexa with best value closest to lowest Enemy
					Point bestHexa = null;
					if (bestHexas.Count == 1){
						bestHexa = bestHexas[0];
					}else{
						int minDistance = 100000;
						bestHexa = null;
						Character cLowest = AIUtil.findLowestEnemy(currentChar.team);
						foreach (Point p in bestHexas){
							int d = AIUtil.getDistance(p.x,p.y,cLowest.x,cLowest.y);
							if (d != -1){
								if (d < minDistance){
									minDistance = d;
									bestHexa = p;
								}
							}
						}
					}
					// find path to hexa
					int nbPA = currentChar.PA;
					if (bestHexa != null){
						if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y){
							
						}else{
							int d = AIUtil.getDistance(currentChar.x,currentChar.y,bestHexa.x,bestHexa.y);
							List<Point> shortestPath = hexaGrid.findShortestPath(currentChar.x,currentChar.y,bestHexa.x,bestHexa.y,d);
							for (int i=0;i<=d && nbPA > 0;i+=currentChar.PM){
								Point destination = shortestPath[((i+currentChar.PM) <= d) ? (i+currentChar.PM) : d];
								sequence.Add(new ActionAIPos(MainGame.ActionType.MOVE,new Point(destination.x,destination.y)));
								nbPA--;
							}
						}
						if (nbPA == 0){
							
						}else{
							for (int i=0;i<nbPA;i++){
								Character cAttack = AIUtil.findCharToAttack(charID);
								if (cAttack != null){
									sequence.Add(new ActionAIPos(MainGame.ActionType.ATK1,new Point(cAttack.x,cAttack.y)));
								}else if (bestHexa.x == currentChar.x && bestHexa.y == currentChar.y){
									sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
								}
							}
							
						}
					}else{
						sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
					}
				}else{
					sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
				}
			} break;
			case CharClass.SOIGNEUR : {
				sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
			} break;
			case CharClass.ENVOUTEUR : {
				sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
			} break;
		}
		
		return sequence;
	}
}

public class AIHard : AI{
	public static ClassifierSystem rules;
	public static bool learn = true;
	
	private static bool isValid(Classifier currentSituation,int charID){
		Character currentChar = hexaGrid.charList[charID];

		if (currentSituation.action == Classifier.Action.Skill){
			if (currentSituation.skillAvailable == (byte)ClassifierAttributes.SkillAvailable.NO) return false;
			else if (!currentSituation.isInRangeToUseSkill()) return false;
		}else if (currentSituation.action == Classifier.Action.Attack){
			if (!currentSituation.isInRangeToUseAttack()) return false;
		}else if (currentSituation.action == Classifier.Action.ApproachAlly){
			int nbAllies = 0;
			foreach (Character c in hexaGrid.charList) if (c.team == currentChar.team) nbAllies++;
			if (nbAllies <= 1) return false;
			ActionAIPos temp = AIUtil.AIHard.doApproachAlly(charID);
			if (temp.pos == null) return false;
			//if (temp.pos.x == currentChar.x && temp.pos.y == currentChar.y) return false;
		}else if (currentSituation.action == Classifier.Action.ApproachEnemy){
			ActionAIPos temp = AIUtil.AIHard.doApproachEnemy(charID);
			if (temp.pos == null) return false;
			//if (temp.pos.x == currentChar.x && temp.pos.y == currentChar.y) return false;
		}else if (currentSituation.action == Classifier.Action.Flee){
			ActionAIPos temp = AIUtil.AIHard.doFlee(charID);
			if (temp.pos == null) return false;
			//if (temp.pos.x == currentChar.x && temp.pos.y == currentChar.y) return false;
		}
		return true;
	}
	
	public static List<ActionAIPos> decide(int charID,StatsGame statsGame){
		Character currentChar = hexaGrid.charList[charID];
		List<ActionAIPos> sequence = new List<ActionAIPos>();
		Classifier rule = null;
		
		// Get the current situation
		Classifier currentSituation = new Classifier(hexaGrid,charID);
		Debug.Log(currentSituation.getStringInfo());
		// Find matching classifiers to the current situation in the database 
		List<Classifier> matchingRules = rules.findMatchingClassifiers(currentSituation);
		// Add the classifier to the database if no match is found
			if (matchingRules.Count == 0){
				// Create a classifier for every action (only add one later)
				if (true){
					Classifier cApproachEnemy = currentSituation; cApproachEnemy.action = Classifier.Action.ApproachEnemy;
					Classifier cApproachAlly = new Classifier(currentSituation); cApproachAlly.action = Classifier.Action.ApproachAlly;
					Classifier cFlee = new Classifier(currentSituation); cFlee.action = Classifier.Action.Flee;
					matchingRules.Add(cApproachEnemy);
					matchingRules.Add(cApproachAlly);
					matchingRules.Add(cFlee);
					if (currentSituation.isInRangeToUseAttack()){
						Classifier cAttack = new Classifier(currentSituation); cAttack.action = Classifier.Action.Attack;
						matchingRules.Add(cAttack);
					}
					if (currentSituation.isInRangeToUseSkill()){
						Classifier cSkill = new Classifier(currentSituation); cSkill.action = Classifier.Action.Skill;
						matchingRules.Add(cSkill);
					}
				}else{

				}

				foreach (Classifier c in matchingRules) rules.Add(c);
				Debug.Log("Pas trouvé : on ajoute " + matchingRules.Count + " règle(s)");
			}else{
				Debug.Log("Trouvé " + matchingRules.Count + " règle(s)");
			}

			List<Classifier> validRules = new List<Classifier>();
			// Check matching classifiers validity
			foreach (Classifier c in matchingRules){
				if (isValid(c,charID)){
					validRules.Add(c);
				}
			}

			// Get one classifier from matching/valid ones :
			if (validRules.Count == 0){
				Debug.LogWarning("No valid classifier found !!");
				sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null));
				return sequence;
			}else{
				if (learn) rule = Classifier.getClassifierRouletteWheel(validRules);
				else rule = Classifier.getClassifierElististSelection(validRules);
				statsGame.addToRules(currentChar,rule);
				Debug.Log("Hard AI action : " + rule.action);
			}

			// Convert the action from the classifier to an action that can be executed in game
			switch (rule.action){
			case Classifier.Action.ApproachEnemy : sequence.Add(AIUtil.AIHard.doApproachEnemy(charID)); break;
			case Classifier.Action.ApproachAlly   : sequence.Add(AIUtil.AIHard.doApproachAlly(charID)); break;
			case Classifier.Action.Flee           : sequence.Add(AIUtil.AIHard.doFlee(charID)); break;
			case Classifier.Action.Attack         : sequence.Add(AIUtil.AIHard.doAttack(charID,0)); break;
			case Classifier.Action.Skill          : sequence.Add(AIUtil.AIHard.doSkill(charID,0)); break;
			default : sequence.Add(new ActionAIPos(MainGame.ActionType.SKIP,null)); break;
			}

			return sequence;
		}
}

}
