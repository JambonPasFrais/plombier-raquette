using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMatchUI : MonoBehaviour
{
	[SerializeField] private Transform _singleMatchParent;
	[SerializeField] private Transform _doubleMatchParent;

	private List<Transform> _modelLocationsSingle = new List<Transform>();
	private List<Transform> _modelLocationsDouble = new List<Transform>();

	private void Start()
	{
		foreach(Transform item in _singleMatchParent)
			_modelLocationsSingle.Add(item);

		foreach(Transform item in _doubleMatchParent) 
			_modelLocationsDouble.Add(item);

		_singleMatchParent.gameObject.SetActive(false);
		_doubleMatchParent.gameObject.SetActive(false);

		Init(0);
	}

	public void Init(int winnerIndex)
	{
		gameObject.SetActive(true);

		if(GameParameters.Instance.NumberOfPlayerBySide == 0)
		{
			_singleMatchParent.gameObject.SetActive(true);
			InstantiateCharacter(winnerIndex, _modelLocationsSingle[0]);
			InstantiateCharacter((winnerIndex + 1) % 2, _modelLocationsSingle[1]);
		}
		else
		{
			InstantiateCharacter(winnerIndex * 2, _modelLocationsDouble[0]);
			InstantiateCharacter(winnerIndex * 2 + 1, _modelLocationsDouble[1]);
			InstantiateCharacter(((winnerIndex + 1) % 2) * 2, _modelLocationsDouble[2]);
			InstantiateCharacter(((winnerIndex + 1) % 2) * 2 + 1, _modelLocationsDouble[3]);

			_doubleMatchParent.gameObject.SetActive(true);
		}

		StartCoroutine(WaitBeforeGoingBackToMainMenu());
	}

	private void InstantiateCharacter(int characterIndex, Transform location)
	{
		GameObject go = Instantiate(GameParameters.Instance.PlayersCharacter[characterIndex].BasicModel, location);
		go.transform.localScale = new Vector3(20, 20, 20);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
	}

	private IEnumerator WaitBeforeGoingBackToMainMenu()
	{
		yield return new WaitForSeconds(10);
		//SceneManager.LoadScene("Clean_UI_Final");
	}
}
