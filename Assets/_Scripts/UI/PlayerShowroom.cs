using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Class containing every refences to set visual of player's showroom for character selections
[System.Serializable]
public class PlayerShowroom
{
	public TextMeshProUGUI PlayerInfo;
	public TextMeshProUGUI CharacterName;
    public Image NameBackground;
    public Transform ModelLocation;
    public Image Background;
    public Image CharacterEmblem;
    public GameObject Container;

    public void InitializeOnlineShowroom(CharacterData data, string username)
    {
        Container.SetActive(true);
        PlayerInfo.text = username;
        CharacterName.text = data.Name;
        NameBackground.color = data.CharacterSecondaryColor;
        GameObject model = GameObject.Instantiate(data.BasicModel, ModelLocation);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		model.transform.localScale = new Vector3(20, 20, 20);
        Background.color = data.CharacterPrimaryColor;
        CharacterEmblem.sprite = data.CharactersLogo;
	}

    public void ResetShowroom()
    {
        PlayerInfo.text = " ?? ";
        CharacterName.text = " ?? ";
        NameBackground.color = Color.black;
		if(ModelLocation.childCount > 0)
		{
			GameObject model = ModelLocation.GetChild(0).gameObject;
			model.transform.SetParent(MenuManager.Instance.CharactersModelsParent);
			model.transform.localPosition = Vector3.zero;
			model.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			model.gameObject.SetActive(false);
		}
		Background.color = Color.white;
        CharacterEmblem.sprite = null;
    }
}
