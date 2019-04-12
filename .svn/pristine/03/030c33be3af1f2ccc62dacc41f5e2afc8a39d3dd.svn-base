using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classifiers;


public class AlgoGen 
{
	/**FAIRE ATTENTION  */


	public static void MutateClassTypeCurrentChar (Classifier currentClassifier){
		
		/*TO DO  */
		/* 
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.HP =(byte)Classifier.returnRandomHPValue();
		tempClassifier.display();*/

		/*END */
	}

	public static void MutateHpCurrentChar (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.HP =(byte)Classifier.returnRandomHPValue();
		tempClassifier.display();
	}

	public static void MutatePACurrentChar (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.PA =(byte)Classifier.returnRandomPAValue();
		tempClassifier.display();
	}
	

	public static void MutateActionCurrentChar (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.action =Classifier.getRandomActionFromEnum();
		tempClassifier.display();
	}
		public static void MutateThreatCurrentChar (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.threat =(byte)Classifier.returnRandomThreatValue();
		tempClassifier.display();
	}

	public static void MutateHpAlly (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		tempClassifier.infoAllies = new List<Classifier.InfoChars>();

		for(int i=0; i<currentClassifier.infoAllies.Count ;i++) {

			if ((currentClassifier.infoAllies[i].classType)==(byte)8)
			{
				//DO NOTHING
			}
			else  {
			tempClassifier.infoAllies.Add(currentClassifier.infoAllies[i].Copy());
			tempClassifier.infoAllies[i].HP=(byte)Classifier.returnRandomCharTypeValue((byte)currentClassifier.infoAllies[i].HP);
			
			}
		}
		currentClassifier.display();

		tempClassifier.display();
	}
	

	
	public static void MutateCharTypeAlly (Classifier currentClassifier){
		Classifier tempClassifier= currentClassifier.Copy();
		tempClassifier.infoAllies = new List<Classifier.InfoChars>();

		for(int i=0; i<currentClassifier.infoAllies.Count ;i++) {
			//Verifier si Classtype != NONE 
			if ((currentClassifier.infoAllies[i].classType)==(byte)8)
			{
				//DO NOTHING
			}
			else  {
			tempClassifier.infoAllies.Add(currentClassifier.infoAllies[i].Copy());
			tempClassifier.infoAllies[i].classType=(byte)Classifier.returnRandomCharTypeValue((byte)currentClassifier.infoAllies[i].classType);
			
			}

		}
		currentClassifier.display();

		tempClassifier.display();
	}

	public static void insertRule(Classifier c)
	{
			
	}
}