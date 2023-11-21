using System;
using System.Collections.Generic;

[Serializable]
public class NamedActions
{
    public string Name;
    public ShotParameters Parameters;

    public static ShotParameters GetActionParametersByName(List<NamedActions> namedActions, string name)
    {
        foreach(NamedActions namedAction in namedActions)
        {
            if (namedAction.Name == name)
            {
                return namedAction.Parameters;
            }
        }

        return null;
    }
}
