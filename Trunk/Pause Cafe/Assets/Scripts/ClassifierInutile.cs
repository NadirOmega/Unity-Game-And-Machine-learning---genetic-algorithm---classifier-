using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters;

namespace AI_Class {
	
	public enum Action {ATTACK,SKILL,GO_FORWARD,FLEE,DEFEND};
	
	public enum HP {
		BELOW_ONE_QUARTER    = 1,
		BELOW_HALF           = 2,
		BELOW_THREE_QUARTERS = 4,
		ABOVE_THREE_QUARTERS = 8
	};
	
	public enum PA {
		ONE   = 1,
		TWO   = 2,
		THREE = 4,
		FOUR  = 8
	};
	
	public enum CanBeHealed {
		YES           = 1,
		NO            = 2,
		ONLY_IF_FLEE  = 4,
		ONLY_IF_STILL = 8
	}
	
	public enum Threat {
		NONE               = 1,
		NONE_IF_FLEE       = 2,
		DEATH              = 4,
		DEATH_IF_APPROACH  = 8,
		DAMAGE             = 16,
		DAMAGE_IF_APPROACH = 32
	}
	
	public enum ClosestSupport {
		NONE     = 1,
		SOIGNEUR = 2,
		GUERRIER = 4
	}
	
	public enum SkillAvailable {
		YES = 1,
		NO  = 2
	}
	
	public enum CanAttack {
		WILL_TAKE_MORE_DAMAGE = 1,
		WILL_TAKE_LESS_DAMAGE = 2,
		WILL_DIE              = 4,
		WILL_KILL             = 8,
		WILL_KILL_AND_DIE     = 16,
		NO                    = 32
	}
	
	// GUERRIER
	public enum CanDefend {
		WITH_NO_PA_LEFT = 1,
		YES             = 2,
		NO              = 4
	}
	
	// VOLEUR
	
	// ARCHER
	
	// MAGE
	
	// ENVOUTEUR / SOIGNEUR
	public enum ClosestAlly {
		DIRECT    = 1,
		MUST_MOVE = 2,
		NONE      = 4
	}
	
	// ##############################################################################################################################################
	// SITUATION
	// ##############################################################################################################################################
	
	public class Situation{
		public HP hp;
		public PA pa;
		public CanBeHealed canBeHealed;
		public Threat threat;
		public ClosestSupport closestSupport;
		public SkillAvailable skillAvailable;
		
		public Situation(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f){
			hp = a;
			pa = b;
			canBeHealed = c;
			threat = d;
			closestSupport = e;
			skillAvailable = f;
		}
	}
	
	
	public class SituationGuerrier : Situation {
		public SituationGuerrier(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	public class SituationVoleur : Situation {
		public SituationVoleur(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	public class SituationArcher : Situation {
		public SituationArcher(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	public class SituationMage : Situation {
		public SituationMage(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	public class SituationSoigneur : Situation {
		public SituationSoigneur(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	public class SituationEnvouteur : Situation {
		public SituationEnvouteur(HP a,PA b,CanBeHealed c,Threat d,ClosestSupport e,SkillAvailable f) : base(a,b,c,d,e,f) {
			
		}
	}
	
	// ##############################################################################################################################################
	// CONDITION
	// ##############################################################################################################################################
		
	public class Condition{
		public static int[] nbConditionsPerClass = {6,6,6,6,6,6};
		public static int[][] nbBitsPerCondition = new int[][]{
			new int[]{4,4,4,6,3,2},
			new int[]{4,4,4,6,3,2},
			new int[]{4,4,4,6,3,2},
			new int[]{4,4,4,6,3,2},
			new int[]{4,4,4,6,3,2},
			new int[]{4,4,4,6,3,2}
		};
		
		public CharClass charClass;
		public byte[] conditions;
		
		public Condition(CharClass charClass){
			this.charClass = charClass;
			int nbConditions = nbConditionsPerClass[(int)charClass];
			conditions = new byte[nbConditions];
			for (int i=0;i<nbConditions;i++){
				conditions[i] = (byte)generateRandomBinary(nbBitsPerCondition[(int)charClass][i],0.25f);
			}
		}
		
		/*public static Condition createFrom(Situation s){
			
		}*/
		
		public static uint generateRandomBinary(int nbBits,float likelihood){
			uint r = 0;
			if (nbBits > 32) nbBits = 32;
			for (int i=0;i<nbBits;i++) r |= (uint)((Random.Range(0.0f,1.0f) < r) ? (1<<i) : 0);
			return r;
		}
	}
	
	/*public class Classifier{
		public Condition condition;
		public Action action;
		
		public int matches;
		public int correctMatches;
		public float fitness;
		
		private Classifier(){
			
		}
		
		public static Classifier createFrom(Situation s,Action a){
			
		}
		
		public bool checkMatch(Situation s){
			return true;
		}
	}
	
	public class Classifiers{
		public List<Classifier> classifier;
		
		public Classifiers(){
			classifier = new List<Classifier>();
		}
		
		public void createClassifierFrom(Situation s,Action a){
			classifier.Add(Classifier.createFrom(s,a));
		}
		
		public List<Classifier> checkCondition(Situation s){
			List<Classifier> cMatches = new List<Classifier>();
			foreach (Classifier c in classifier){
				if (c.checkMatch(s)) cMatches.Add(c);
			}
			return cMatches;
		}
	}*/
	
}
