using System;

namespace LegacyApp;

public class FakeUserCredit : ICreditLimitSrevice
{
    public int GetCreditLimit(string lastName, DateTime birthday)
    {
        return 1;
    }
}