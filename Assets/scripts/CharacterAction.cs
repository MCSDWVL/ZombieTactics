using UnityEngine;
using System.Collections;

public class CharacterAction : MonoBehaviour 
{
	public string Name = "GenericAction";
	public int Range = 1;
	public int HealthDamageMin = 10;
	public int HealthDamageMax = 10;
	public int InfectionDamageMin = 1;
	public int InfectionDamageMax = 1;
	public bool Pierces = false;
	public bool RequiresLineOfSight = true;
	public int AoeAtTarget = 0;

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
