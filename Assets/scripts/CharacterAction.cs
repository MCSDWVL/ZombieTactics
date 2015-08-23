using UnityEngine;
using System.Collections;

public class CharacterAction : MonoBehaviour 
{
	public string Name = "GenericAction";
	public string Description = "A generic description of an attack; You should use this on babies";
	public string NameStatsDescription 
	{ 
		get 
		{ 
			var damageString = HealthDamageMin == HealthDamageMax ? string.Format("{0} damage", HealthDamageMin) : string.Format("{0}-{1} damage", HealthDamageMin, HealthDamageMax);
			var infectionString = InfectionDamageMin == InfectionDamageMax ? string.Format("{0} infection", InfectionDamageMin) : string.Format("{0}-{1} infection", InfectionDamageMin, InfectionDamageMax);

			var effectString = "";
			if(HealthDamageMax > 0 && InfectionDamageMax > 0)
				effectString = " (" + damageString + "/" + infectionString + ")";
			else if(HealthDamageMax > 0)
				effectString = " (" + damageString + ")";
			else if (InfectionDamageMax > 0)
				effectString = " (" + infectionString + ")";

			return string.Format("{0}{1}: {2}", Name, effectString, Description); 
		}
	}
	public int Range = 1;
	public int HealthDamageMin = 10;
	public int HealthDamageMax = 10;
	public int InfectionDamageMin = 1;
	public int InfectionDamageMax = 1;
	public bool Pierces = false;
	public bool ThroughWalls = true;
	public int AoeAtTarget = 0;
	
	public int SelfHealthDamageMin = 0;
	public int SelfHealthDamageMax = 0;

	public int SelfInfectionDamageMin = 0;
	public int SelfInfectionDamageMax = 0;

	public string TriggerAnimName = "Attack";

	public int GetSelfHealthDamage()
	{
		return UnityEngine.Random.Range(SelfHealthDamageMin, SelfHealthDamageMax);
	}

	public int GetSelfInfectionDamage()
	{
		return UnityEngine.Random.Range(SelfInfectionDamageMin, SelfInfectionDamageMax);
	}

	public int GetHealthDamage()
	{
		return UnityEngine.Random.Range(HealthDamageMin, HealthDamageMax);
	}

	public int GetInfectionDamage()
	{
		return UnityEngine.Random.Range(InfectionDamageMin, InfectionDamageMax);
	}

	public bool IsNoOp()
	{
		return HealthDamageMax == 0 && HealthDamageMin == 0 && InfectionDamageMax == 0 && InfectionDamageMin == 0;
	}
}
