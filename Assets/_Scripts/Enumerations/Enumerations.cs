using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	BEFOREGAME,
	SERVICE,
	PLAYING,
	ENDPOINT,
	ENDMATCH
}

public enum PlayerStates
{
	IDLE,
	SERVE,
	PLAY,
}

public enum Teams
{
	DEFAULT,
	TEAM1,
	TEAM2
}

public enum FieldSide
{
	FIRSTSIDE,
	SECONDSIDE
}

public enum CharacterCategory
{
	SMALL,
	MEDIUM,
	BIG
}
