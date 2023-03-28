using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeConditions
{
    public int LOWER_LIMIT_DEATH = 8;
    public int LOWER_LIMIT_BIRTH = 11;
    public int UPPER_LIMIT_BIRTH = 16;
    public int UPPER_LIMIT_DEATH = 17;

    private int lowerLimitDeath;
    private int lowerLimitBirth;
    private int upperLimitBirth;
    private int upperLimitDeath;

    public LifeConditions()
    {
        lowerLimitDeath = LOWER_LIMIT_DEATH;
        lowerLimitBirth = LOWER_LIMIT_BIRTH;
        upperLimitBirth = UPPER_LIMIT_BIRTH;
        upperLimitDeath = UPPER_LIMIT_DEATH;
    }

    public LifeConditions(int lowerLimitDeath, int lowerLimitBirth, int upperLimitBirth, int upperLimitDeath)
    {
        this.lowerLimitDeath = lowerLimitDeath;
        this.lowerLimitBirth = lowerLimitBirth;
        this.upperLimitBirth = upperLimitBirth;
        this.upperLimitDeath = upperLimitDeath;
    }

    public int LowerLimitDeath { get => lowerLimitDeath; set => lowerLimitDeath = value; }
    public int LowerLimitBirth { get => lowerLimitBirth; set => lowerLimitBirth = value; }
    public int UpperLimitBirth { get => upperLimitBirth; set => upperLimitBirth = value; }
    public int UpperLimitDeath { get => upperLimitDeath; set => upperLimitDeath = value; }
}
