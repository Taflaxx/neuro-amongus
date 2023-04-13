﻿using static Neuro.Recording.Map.VentData.Types;

namespace Neuro.Recording.Map;

public partial class VentData
{
    public static VentData Create(Vent vent)
    {
        VentData data = new()
        {
            Id = (uint) vent.Id,
            Position = PositionData.Create(vent, vent)
        };

        foreach (Vent nearbyVent in vent.NearbyVents)
        {
            if (!nearbyVent) continue;

            data.ConnectingVents.Add(new ConnectingVentData
            {
                Id = (uint) nearbyVent.Id,
                Position = nearbyVent.transform.position
            });
        }

        return data;
    }
}