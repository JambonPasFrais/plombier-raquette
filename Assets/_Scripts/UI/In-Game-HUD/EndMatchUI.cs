using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMatchUI : MonoBehaviour
{
	[SerializeField] private Transform _singleMatchParent;
	[SerializeField] private Transform _doubleMatchParent;

	[SerializeField] private Transform _scoreLocation;
	[SerializeField] private GameObject _scoreDisplayReference;

	private List<Transform> _modelLocationsSingle = new List<Transform>();
	private List<Transform> _modelLocationsDouble = new List<Transform>();

	private void InitPositions()
	{
		foreach(Transform item in _singleMatchParent)
			_modelLocationsSingle.Add(item);

		foreach(Transform item in _doubleMatchParent) 
			_modelLocationsDouble.Add(item);

		_singleMatchParent.gameObject.SetActive(false);
		_doubleMatchParent.gameObject.SetActive(false);
	}

	public void Init(int winnerIndex)
	{
		InitPositions();

		gameObject.SetActive(true);

		if(!GameParameters.Instance.IsDouble)
		{
			_singleMatchParent.gameObject.SetActive(true);
			InstantiateCharacter(winnerIndex, _modelLocationsSingle[0]);
			InstantiateCharacter((winnerIndex + 1) % 2, _modelLocationsSingle[1]);
		}
		else
		{
			int winnerSideCpt = 0;
			int loserSideCpt = 2;

			for(int i = 0; i < 4; i++)
			{
				if (i % 2 == winnerIndex)
				{
					InstantiateCharacter(i, _modelLocationsDouble[winnerSideCpt]);
					winnerSideCpt++;
				}

				else
				{
					InstantiateCharacter(i, _modelLocationsDouble[loserSideCpt]);
					loserSideCpt++;
				}
			}

			_doubleMatchParent.gameObject.SetActive(true);
		}

		_scoreDisplayReference.transform.SetParent(_scoreLocation);
		_scoreDisplayReference.transform.localPosition = Vector3.zero;
		_scoreDisplayReference.transform.localScale = new Vector3(2, 2, 2);
		_scoreDisplayReference.transform.localRotation = Quaternion.identity;
	}

	private void InstantiateCharacter(int characterIndex, Transform location)
	{
		GameObject go = Instantiate(GameParameters.Instance.PlayersCharacter[characterIndex].BasicModel, location);
		go.transform.localScale = new Vector3(20, 20, 20);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler(0, 180, 0);
	}
}
