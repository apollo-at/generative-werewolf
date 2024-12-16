
using UnityEngine;

public class GameTime
{
    public int day = 0;
    public bool is_night = false;

    ///<summary>
    ///Advance to next game phase
    ///</summary>
    public void PassTime()
    {
        if (!is_night)
        {
            day++;
        }
        is_night = !is_night;
    }

    public void ResetTime()
    {
        is_night = false;
        day = 0;
    }

    /// <summary>
    /// Copy GameTime object
    /// </summary>
    /// <returns></returns>
    public GameTime Copy()
    {
        GameTime copy = new()
        {
            day = this.day,
            is_night = this.is_night
        };
        return copy;
    }
    /// <summary>
    /// Format DayTime object to game string
    /// </summary>
    /// <returns></returns>
    public string Print()
    {
       return day > 0 ? $"{(is_night ? "Night" : "Day")} {day}" : "Start";
    }

    public int CalculatePhases() {
        int phases = (day) * 2;
        if(is_night)
        {
            return phases - 1;
        }
        return phases;
    }
}
