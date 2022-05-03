﻿using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.PoliceTicket;

namespace Server.ChatCommands.Factions.Police;

internal class GivePlayerTicketCommand : ISingletonScript
{
    private readonly PoliceTicketModule _policeTicketModule;
    
    private readonly GroupFactionService _groupFactionService;

    public GivePlayerTicketCommand(
        GroupFactionService groupFactionService, 
        
        PoliceTicketModule policeTicketModule)
    {
        _groupFactionService = groupFactionService;
        
        _policeTicketModule = policeTicketModule;
    }

    [Command("giveticket", "Gebe einen anderen Charakter einen Strafzettel.", Permission.NONE, new[] { "Spieler ID", "Grund", "Kosten" })]
    public async void OnExecute(ServerPlayer player, string expectedPlayerId, string expectedReason, string expectedCosts)
    {
        if (!player.Exists)
        {
            return;
        }
        
        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            player.SendNotification("Das kann dein Charakter nicht.", NotificationType.ERROR);
            return;
        }

        var target = Alt.GetAllPlayers().GetPlayerById(player, expectedPlayerId);
        if (target == null)
        {
            return;
        }
        
        if (!int.TryParse(expectedCosts, out var costs))
        {
            player.SendNotification("Bitte gebe ein richtige Zahl für die Kosten an.", NotificationType.ERROR);
            return;
        }
        
        if (player.Position.Distance(target.Position) > 3.0f)
        {
            player.SendNotification("Du bist zu weit entfernt.", NotificationType.ERROR);
            return;
        }

        bool success = await _policeTicketModule.GivePlayerTicket(target, player.CharacterModel.Name, expectedReason, costs);
        if (success)
        {
            player.SendNotification("Du hast dem Charakter ein Ticket übergeben.", NotificationType.INFO);
        }
        else
        {
            player.SendNotification("Der Charakter hat nicht genug Platz im Inventar.", NotificationType.ERROR);
        }
    }
}