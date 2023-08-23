using UnityEngine;
using System.Collections;

/*
 *     gameID: cabinet.currentGame,
    eventID: uuid.v4(),
    time: new Date(),
    type: message.type,
    values: message.values,
    cabinet: {
      sceneName: cabinet.config.sceneName,
      cabinetName: cabinet.config.cabinetName,
      token: cabinet.config.token,
    },
*/
[System.Serializable]
public class CabinetJSON
{
    public string sceneName;
    public string cabinetName;
    public string token;
}
[System.Serializable]
public class GameEventJSON
{
    public string event_type;
    public string[] values;

}


