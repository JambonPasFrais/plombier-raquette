using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersListTransform;


	private void Start()
	{
		foreach(var item in _characters)
		{
			GameObject go = Instantiate(_characterUIPrefab, _charactersListTransform);
			go.GetComponent<CharacterUI>().SetVisual(item);
		}
	}
}
