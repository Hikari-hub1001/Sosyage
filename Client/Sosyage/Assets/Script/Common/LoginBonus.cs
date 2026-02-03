using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoginBonusClaimResponse
{
    public LoginBonusPeriod period;
    public int currentDay;
    public List<LoginBonusDailyBonus> dailyBonuses;
}
[Serializable]
public class LoginBonusPeriod
{
    public string start;
    public string end;
}
[Serializable]
public class LoginBonusDailyBonus
{
    public List<LoginBonusItemBonus> bonuses;
}
[Serializable]
public class LoginBonusItemBonus
{
    public int id;
    public int quantity;
}
