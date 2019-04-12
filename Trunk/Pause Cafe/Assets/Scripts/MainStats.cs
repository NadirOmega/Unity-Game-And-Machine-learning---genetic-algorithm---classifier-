using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Classifiers;
using AI_Class;

public class MainStats : MonoBehaviour {
	
	public GameObject classifierStatsGO;
	public GameObject classifierContainerGO;
	public GameObject displayClassifierGO;
	public Button buttonBack;
	bool buttonBackPressed = false;
	
	public int scrollAmount;
	public bool forceUpdate;
	public List<Classifier> classifierList;
	
	public List<GameObject> classifierContainerList;
	
    // Start is called before the first frame update
    void Start(){
        buttonBack.onClick.AddListener(buttonBackPressed_);
		scrollAmount = 0;
		for (int i=0;i<20;i++){
			GameObject go = GameObject.Instantiate(classifierStatsGO,classifierContainerGO.transform);
			classifierContainerList.Add(go);
			go.transform.position = new Vector3(go.transform.position.x,go.transform.position.y-i*24,go.transform.position.z);
		}
		forceUpdate = true;
		
		// Copy the list
		classifierList = new List<Classifier>();
		if (AIHard.rules != null){
			foreach (Classifier c in AIHard.rules.classifiers){
				classifierList.Add(c);
			}
		}
    }

    // Update is called once per frame
    void Update(){
		
		if (Input.GetKeyDown(KeyCode.A)){
			Classifier.sortByFitness2(classifierList);
			forceUpdate = true;
		}
		if (Input.GetKeyDown(KeyCode.Z)){
			Classifier.sortByFitness(classifierList);
			forceUpdate = true;
		}
		if (Input.GetAxis("Mouse ScrollWheel") != 0.0f || forceUpdate){
			forceUpdate = false;
			scrollAmount -= (int)(Input.GetAxis("Mouse ScrollWheel")*10.0);
			for (int i=0;i<20;i++){
				if (i+scrollAmount >= 0 && i+scrollAmount < classifierList.Count){
					classifierContainerList[i].SetActive(true);
					setClassifierStats(classifierContainerList[i],classifierList[i+scrollAmount]);
				}else{
					classifierContainerList[i].SetActive(false);
				}
			}
		}
        if (buttonBackPressed){
			SceneManager.LoadScene(0);
			buttonBackPressed = false;
		}
		
		if (Input.GetMouseButton(0)){
			Vector3 mousePos = Input.mousePosition;
			for (int i=0;i<20;i++){
				GameObject go = classifierContainerList[i];
				if (go.activeInHierarchy){
					Vector3 pos = go.transform.position;
					if (mousePos.x >= pos.x-500 && mousePos.y < pos.y && mousePos.x <= pos.x+500 && mousePos.y >= pos.y-24){
						displayClassifierGO.SetActive(true);
						displayClassifierGO.transform.GetChild(0).GetComponent<Text>().text = classifierList[i+scrollAmount].getStringInfo();
						displayClassifierGO.GetComponent<Image>().color = go.GetComponent<Image>().color;
					}
				}
			}
		}else{
			displayClassifierGO.SetActive(false);
		}
    }
	
	void setClassifierStats(GameObject classifierStatsGO,Classifier classifier){
		if (classifier != null){
			string strDisp;
			strDisp = "";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.GUERRIER) > 0) strDisp += "GUERRIER ";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.VOLEUR) > 0) strDisp += "VOLEUR ";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.ARCHER) > 0) strDisp += "ARCHER ";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.MAGE) > 0) strDisp += "MAGE ";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.SOIGNEUR) > 0) strDisp += "SOIGNEUR ";
			if ((classifier.charClass & (byte)ClassifierAttributes.CharClass.ENVOUTEUR) > 0) strDisp += "ENVOUTEUR ";
			classifierStatsGO.transform.GetChild(0).GetComponent<Text>().text = strDisp;
			
			strDisp = "";
			if ((classifier.HP & (byte)ClassifierAttributes.HP_.BETWEEN_100_75) > 0) strDisp += "100-75% ";
			if ((classifier.HP & (byte)ClassifierAttributes.HP_.BETWEEN_74_40) > 0) strDisp += "74-40% ";
			if ((classifier.HP & (byte)ClassifierAttributes.HP_.BETWEEN_39_0) > 0) strDisp += "39-0% ";
			classifierStatsGO.transform.GetChild(1).GetComponent<Text>().text = strDisp;
			
			strDisp = "";
			if ((classifier.PA & (byte)ClassifierAttributes.PA_.ONE) > 0) strDisp += "ONE ";
			if ((classifier.PA & (byte)ClassifierAttributes.PA_.TWO_OR_MORE) > 0) strDisp += "TWO+ ";
			classifierStatsGO.transform.GetChild(2).GetComponent<Text>().text = strDisp;
			
			strDisp = "";
			if ((classifier.skillAvailable & (byte)ClassifierAttributes.SkillAvailable.YES) > 0) strDisp += "YES ";
			if ((classifier.skillAvailable & (byte)ClassifierAttributes.SkillAvailable.NO) > 0) strDisp += "NO ";
			classifierStatsGO.transform.GetChild(3).GetComponent<Text>().text = strDisp;
			
			strDisp = "";
			if ((classifier.threat & (byte)ClassifierAttributes.Threat.SAFE) > 0) strDisp += "SAFE ";
			if ((classifier.threat & (byte)ClassifierAttributes.Threat.DANGER) > 0) strDisp += "DANGER ";
			if ((classifier.threat & (byte)ClassifierAttributes.Threat.DEATH) > 0) strDisp += "DEATH ";
			classifierStatsGO.transform.GetChild(4).GetComponent<Text>().text = strDisp;
			
			strDisp = "";
			if ((classifier.maxTargets & (byte)ClassifierAttributes.MaxTargets.NONE) > 0) strDisp += "NONE ";
			if ((classifier.maxTargets & (byte)ClassifierAttributes.MaxTargets.ONE) > 0) strDisp += "ONE ";
			if ((classifier.maxTargets & (byte)ClassifierAttributes.MaxTargets.TWO) > 0) strDisp += "TWO ";
			if ((classifier.maxTargets & (byte)ClassifierAttributes.MaxTargets.THREE_OR_MORE) > 0) strDisp += "THREE+ ";
			classifierStatsGO.transform.GetChild(5).GetComponent<Text>().text = strDisp;

			classifierStatsGO.transform.GetChild(6).GetComponent<Text>().text = "" + classifier.infoAllies.Count;

			classifierStatsGO.transform.GetChild(7).GetComponent<Text>().text = "" + classifier.infoEnemies.Count;
			
			classifierStatsGO.transform.GetChild(8).GetComponent<Text>().text = "" + classifier.action;
			
			classifierStatsGO.transform.GetChild(9).GetComponent<Text>().text = "" + classifier.fitness + "," + classifier.useCount;
			if (classifier.fitness < 0.5f){
				classifierStatsGO.GetComponent<Image>().color = new Color(1.0f,(classifier.fitness)*2,(classifier.fitness)*2);
			}else{
				classifierStatsGO.GetComponent<Image>().color = new Color(1.0f-((classifier.fitness-0.5f)*2),1.0f,1.0f-((classifier.fitness-0.5f)*2));
			}
		}
	}
	
	// Events
	void buttonBackPressed_(){buttonBackPressed = true;}
}
