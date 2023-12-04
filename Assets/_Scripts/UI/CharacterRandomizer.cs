using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRandomizer : MonoBehaviour
{
    private static CharacterRandomizer _instance;

	private void Awake()
	{
		if (_instance == null)
			_instance = this;
	}

	public List<CharacterData> ReturnRandomCharacter(List<CharacterData> availableCharacters, int nbOfCharactersToReturn)
	{
		List<CharacterData> charactersToReturn= new List<CharacterData>();
		CharacterData data = null;
		int currentIndex;

		System.Random rand = new System.Random();

		for (int i = 0; i < nbOfCharactersToReturn; i++)
		{
			currentIndex = rand.Next(charactersToReturn.Count);
			data = availableCharacters[currentIndex];
			availableCharacters.RemoveAt(currentIndex);
			charactersToReturn.Add(data);
		}

		return charactersToReturn;
	}
}
