using UnityEngine;
using System.Collections;
using UnityEngine;

public class GameDataStructure
{
    public LSProperty<int> kills = new LSProperty<int>(0);
    public LSProperty<int> militaryKills = new LSProperty<int>(0);
    public LSProperty<int> queenKills = new LSProperty<int>(0);
    public LSProperty<int> droneKills = new LSProperty<int>(0);
    public LSProperty<int> deaths = new LSProperty<int>(0);
    public LSProperty<int> militaryDeaths = new LSProperty<int>(0);
    public LSProperty<int> berriesGrabbed = new LSProperty<int>(0);
    public LSProperty<int> berriesDeposited = new LSProperty<int>(0);
    public LSProperty<int> berriesKicked = new LSProperty<int>(0);
    public LSProperty<int> berriesKicked_OtherTeam = new LSProperty<int>(0);
    public LSProperty<int> snailMoved = new LSProperty<int>(0);
    public LSProperty<int> speedObtained = new LSProperty<int>(0);
    public LSProperty<int> swordObtained = new LSProperty<int>(0);
    public LSProperty<int> longestLife = new LSProperty<int>(0);
    public LSProperty<int> formGuards = new LSProperty<int>(0);
    public LSProperty<int> ledgeGuards = new LSProperty<int>(0);
    public LSProperty<int> snailGuards = new LSProperty<int>(0);
    public LSProperty<int> formFails = new LSProperty<int>(0);
    public LSProperty<int> berryFails = new LSProperty<int>(0);
    public LSProperty<int> bumpAssists = new LSProperty<int>(0);
    public LSProperty<int> snailKills = new LSProperty<int>(0);
    public LSProperty<int> snailDeaths = new LSProperty<int>(0);
    public LSProperty<int> stinches = new LSProperty<int>(0);

    public void Reset()
    {
        kills.property = militaryKills.property = queenKills.property = deaths.property = berriesGrabbed.property
            = berriesDeposited.property = berriesKicked.property = snailMoved.property = speedObtained.property =
            swordObtained.property = berriesKicked_OtherTeam.property = longestLife.property = militaryDeaths.property =
            formGuards.property = ledgeGuards.property = snailGuards.property = formFails.property = berryFails.property =
            snailKills.property = bumpAssists.property = stinches.property = droneKills.property = snailDeaths.property = 0;
    }

}

