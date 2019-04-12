using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classifiers;


public class AlgoGen 
{
	/**FAIRE ATTENTION  */
	// MUTATION
	public static void MutateClassTypeCurrentChar (Classifier currentClassifier){
		
		/*TO DO  */
		/* 
		Classifier tempClassifier= currentClassifier.Copy();
		currentClassifier.display();
		tempClassifier.HP =(byte)Classifier.returnRandomHPValue();
		tempClassifier.display();*/

		/*END */
	}

	public static void MutateHpCurrentChar (Classifier classifier){
		classifier.HP = (byte)ClassifierAttributes.getRandomHPValue();
	}

	public static void MutatePACurrentChar (Classifier classifier){
		classifier.PA = (byte)ClassifierAttributes.getRandomPAValue();
	}
	
	public static void MutateActionCurrentChar (Classifier classifier){
		classifier.action = Classifier.getRandomAction();
	}
	
	public static void MutateThreatCurrentChar (Classifier classifier){
		classifier.threat = (byte)ClassifierAttributes.getRandomThreatValue();
	}

	public static void MutateHpAlly (Classifier classifier){
		for(int i=0; i<classifier.infoAllies.Count ;i++) {
			if (classifier.infoAllies[i].classType != (byte)ClassifierAttributes.ClassType.NONE){
				classifier.infoAllies[i].HP = (byte)ClassifierAttributes.getRandomCharTypeValue((byte)classifier.infoAllies[i].HP);
			}
		}
	}

	public static void MutateCharTypeAlly (Classifier classifier){
		for(int i=0; i<classifier.infoAllies.Count ;i++) {
			if (classifier.infoAllies[i].classType != (byte)ClassifierAttributes.ClassType.NONE){
				classifier.infoAllies[i].classType = (byte)ClassifierAttributes.getRandomCharTypeValue((byte)classifier.infoAllies[i].classType);
			}
		}
	}


    // CROSSOVER

    //Croisement du personnage courant d'une régle extraite d'une BDD avec la régle courante 
    public static void CrossOverCurrentChar(Classifier currentClassifier, Classifier classifierFromDB)
    {
        Classifier tempClassifier1 = new Classifier(currentClassifier);
        Classifier tempClassifier2 = new Classifier(classifierFromDB);

        // le premier nombre aléatoire sert a definir le nombre d'attribus a modifier
        System.Random rnd = new System.Random();
        int randomNumber = rnd.Next(0, 5);

        List<Classifier.InfoChars> info1 = tempClassifier1.infoAllies;
        List<Classifier.InfoChars> info2 = tempClassifier2.infoAllies;


        for (int i = 0; i < randomNumber; i++)
        {
            // le 2eme nombre aleatoire definit l'attribut a modifier
            int randomNumber2 = rnd.Next(0, 6);
            switch (randomNumber2)
            {
                case 0:
                    tempClassifier1.charClass = classifierFromDB.charClass;
                    tempClassifier2.charClass = currentClassifier.charClass;
                    break;

                case 1:
                    tempClassifier1.HP = classifierFromDB.HP;
                    tempClassifier2.HP = currentClassifier.HP;
                    break;

                case 2:
                    tempClassifier1.PA = classifierFromDB.PA;
                    tempClassifier2.PA = currentClassifier.PA;
                    break;

                case 3:
                    tempClassifier1.skillAvailable = classifierFromDB.skillAvailable;
                    tempClassifier2.skillAvailable = currentClassifier.skillAvailable;
                    break;

                case 4:
                    tempClassifier1.threat = classifierFromDB.threat;
                    tempClassifier2.threat = currentClassifier.threat;
                    break;

                case 5:
                    tempClassifier1.action = classifierFromDB.action;
                    tempClassifier2.action = currentClassifier.action;
                    break;

                default:
                    Debug.Log("Erreur nb aleatoire");
                    break;
            }
        }
    }

    // Croisement sur les alliés d'une régle extraite de la BDD avec la régle courante
    public static void CrossOverAlly(Classifier currentClassifier, Classifier ClassifierFromDB)
    {

        Classifier tempClassifier1 = new Classifier(currentClassifier);
        Classifier tempClassifier2 = new Classifier(ClassifierFromDB);

        System.Random rnd = new System.Random();
        int randomNumber = rnd.Next(0, 4);

        List<Classifier.InfoChars> info1 = tempClassifier1.infoAllies;
        List<Classifier.InfoChars> info2 = tempClassifier2.infoAllies;
        // swap allies info

        for (int i = 0; i < randomNumber; i++)
        {
            // le 2eme nombre aleatoire definit l'attribut a modifier
            int randomNumber2 = rnd.Next(0, 4);
            switch (randomNumber2)
            {


                case 0:
                    tempClassifier1.infoAllies[1].classType = info2[1].classType;
                    tempClassifier2.infoAllies[1].classType = info1[1].classType;
                    break;

                case 1:
                    tempClassifier1.infoAllies[2].HP = info2[2].HP;
                    tempClassifier2.infoAllies[2].HP = info1[2].HP;
                    break;

                case 2:
                    tempClassifier1.infoAllies[3].threat = info2[3].threat;
                    tempClassifier2.infoAllies[3].threat = info1[3].threat;
                    break;

                case 3:
                    tempClassifier1.infoAllies[4].distance = info2[4].distance;
                    tempClassifier2.infoAllies[4].distance = info1[4].distance;
                    break;

                default:
                    Debug.Log("Erreur");
                    break;
            }
        }



            /*for (int i = 0; i < currentClassifier.infoAllies.Count; i++)
            {
                if (currentClassifier.infoAllies[i].classType != (byte)ClassifierAttributes.ClassType.NONE){
                    //tempClassifier.HP = 4;
                    tempClassifier2.infoAllies.Add(new InfoChars(currentClassifier.infoAllies[i]));
                }
            }

            for (int i = 0; i < ClassifierFromDB.infoEnemies.Count; i++)
            {
                if ((currentClassifier.infoAllies[i].classType) == (byte)8)
                {
                }
                else
                {
                    tempClassifier.infoAllies.Add(currentClassifier.infoAllies[i].Copy());
                }
            }*/
            //Debug.Log("Affichage Algogen");
            Debug.Log(tempClassifier1.getStringInfo());
        Debug.Log(tempClassifier2.getStringInfo());
    }

    // Croisement sur les Enemies d'une régle extraite de la BDD avec la régle courante
    public static void CrossOverEnemy(Classifier currentClassifier, Classifier ClassifierFromDB)
    {
        Classifier tempClassifier1 = new Classifier(currentClassifier);
        Classifier tempClassifier2 = new Classifier(ClassifierFromDB);

        List<Classifier.InfoChars> info1 = tempClassifier1.infoEnemies;
        List<Classifier.InfoChars> info2 = tempClassifier2.infoEnemies;


        System.Random rnd = new System.Random();
        int randomNumber = rnd.Next(0, 5);
        // swap Enemies info
        for (int i = 0; i < randomNumber; i++)
        {
            // le 2eme nombre aleatoire definit l'attribut a modifier
            int randomNumber2 = rnd.Next(0, 4);
            switch (randomNumber2)
            {
                case 0:
                    tempClassifier1.infoEnemies[1].classType = info2[1].classType;
                    tempClassifier2.infoEnemies[1].classType = info1[1].classType;
                    break;

                case 1:
                    tempClassifier1.infoEnemies[2].HP = info2[2].HP;
                    tempClassifier2.infoEnemies[2].HP = info1[2].HP;
                    break;

                case 2:
                    tempClassifier1.infoEnemies[3].threat = info2[3].threat;
                    tempClassifier2.infoEnemies[3].threat = info1[3].threat;
                    break;

                case 3:
                    tempClassifier1.infoEnemies[4].distance = info2[4].distance;
                    tempClassifier2.infoEnemies[4].distance = info1[4].distance;
                    break;

                default:
                    Debug.Log("Erreur");
                    break;
            }

        }


            /*for (int i = 0; i < currentClassifier.infoEnemies.Count; i++)
            {
                //Verifier si Classtype != NONE 
                if ((currentClassifier.infoEnemies[i].classType) == (byte)8)
                {
                }
                else
                {
                    tempClassifier2.infoEnemies.Add(currentClassifier.infoEnemies[i].Copy());
                }
            }
            for (int i = 0; i < ClassifierFromDB.infoEnemies.Count; i++)
            {
                //Verifier si Classtype != NONE 
                if ((currentClassifier.infoEnemies[i].classType) == (byte)8)
                {
                }
                else
                {
                    tempClassifier.infoAllies.Add(currentClassifier.infoEnemies[i].Copy());
                }
            }*/
            Debug.Log(tempClassifier1.getStringInfo());
        Debug.Log(tempClassifier2.getStringInfo());
    }


    /* // croisement sur les fitness des deux regles 
     public static void CrossOverFitness(Classifier currentClassifier, Classifier ClassifierFromDB)
     {
         Classifier tempClassifier = currentClassifier.Copy();
         tempClassifier.infoEnemies = new List<Classifier.InfoChars>();
         Classifier tempClassifier2 = ClassifierFromDB.Copy();
         tempClassifier2.infoEnemies = new List<Classifier.InfoChars>();

         tempClassifier.fitness = ClassifierFromDB.fitness;
         tempClassifier2.fitness = currentClassifier.fitness;
         //  Debug.Log(currentClassifier.fitness);
         //  Debug.Log(ClassifierFromDB.fitness);      
         tempClassifier.display();
         tempClassifier2.display();

     }*/
}