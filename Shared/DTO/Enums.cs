using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public enum ReservationStatus
    {
        CONFIRMED,
        CANCELED,
        REJECTED,
        PENDING,


    }

    public enum Role
    {
        // for Users
        USER,
        ADMIN

    }

    public enum Status
    {
        //for Resources
        ACTIVE,
        INACTIVE
    }

    public enum Theme
    {
        // for User settings (UI appearance). SYSTEM follows the OS/browser preference.
        SYSTEM,
        LIGHT,
        DARK
    }
}
