namespace MERRICK.DatabaseContext.Handlers;

public static class SeedDataHandlers
{
    public static async Task SeedUsers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Users: Users Have Already Been Seeded");

            return;
        }

        if (await context.Roles.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Users: Roles Have Not Yet Been Seeded");

            return;
        }

        Role roleAdministrator = await context.Roles.SingleAsync(role => role.Name.Equals(UserRoles.Administrator), cancellationToken: cancellationToken);

        User userAdministrator = new ()
        {
            EmailAddress = "project.kongor@proton.me",
            Role = roleAdministrator,
            SRPPasswordSalt = "861c37ec6d049d92cc1c67d195b414f26b572a56358272af3e9c06fcd9bfa053",
            SRPPasswordHash = "fe6f16b0ecb80f6b2bc95d68420fd13afef0c895172a81819870660208ac221a",
            PBKDF2PasswordHash = "AQAAAAIAAYagAAAAEMUkpLAr01NjkKRPaXCyTa17nlOdPKJucn5QYur+wQBTDKCpgsAcREenK+pGJPBCRw==",
            GoldCoins = 5_555_555,
            SilverCoins = 555_555_555,
            PlinkoTickets = 5_555_555,
            TotalLevel = 666,
            TotalExperience = 222_11_666,

            // TODO: Add All Upgrades (Including Missing Ones From PK Version Control); Maybe Scrape The Resources Again

            OwnedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward", "ma.Mastery Boost * 5555", "ma.Super Mastery Boost * 555" ]
        };

        await context.Users.AddAsync(userAdministrator, cancellationToken);

        Role roleGuest = await context.Roles.SingleAsync(role => role.Name.Equals(UserRoles.User), cancellationToken: cancellationToken);

        User userGuest = new ()
        {
            EmailAddress = OOTB.Accounts.GUEST.EmailAddress,
            Role = roleGuest,
            SRPPasswordSalt = "81c278a3ed03cf4549e787feac5ffe1d051029e15159b1cf0b087b4f8a85cb69",
            SRPPasswordHash = "bfad34f4dc65064dcae6e2064e12d63b299909b6c0cdb33306a91f26bb624e35",
            PBKDF2PasswordHash = "AQAAAAIAAYagAAAAECx4DCy3qYARlevpaVUJXy28QyvXDcQiJZ1KOSx547WDLBV0pdx2MX5m+Pe3xhtQAg==",
            GoldCoins = 5_555_555,
            SilverCoins = 555_555_555,
            PlinkoTickets = 5_555_555,
            TotalLevel = 0,
            TotalExperience = 0,

            // TODO: Add All Upgrades (Including Missing Ones From PK Version Control); Maybe Scrape The Resources Again

            OwnedStoreItems = ["ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward", "ma.Mastery Boost * 5555", "ma.Super Mastery Boost * 555"]
        };

        await context.Users.AddAsync(userGuest, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded Administrator And Guest Users With Email Addresses ""{AdministratorEmailAddress}"" And ""{GuestEmailAddress}""",
            userAdministrator.EmailAddress, userGuest.EmailAddress);
    }

    public static async Task SeedClans(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Clans.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Clans: Clans Have Already Been Seeded");

            return;
        }

        IEnumerable<Clan> clans =
        [
            new Clan { Name = "KONGOR", Tag = "K" },
            new Clan { Name = "Project KONGOR", Tag = "PK" },
            new Clan { Name = "Project KONGOR Open-Source", Tag = "PKOS" },
            new Clan { Name = "Heroes Of Newerth", Tag = "HON" }
        ];

        await context.Clans.AddRangeAsync(clans, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded {ClanCount} Clans: {ClanNames}",
            clans.Count(), string.Join(", ", clans.Select(clan => clan.Name)));
    }

    public static async Task SeedAccounts(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Accounts: Accounts Have Already Been Seeded");

            return;
        }

        if (await context.Users.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Accounts: Users Have Not Yet Been Seeded");

            return;
        }

        if (await context.Clans.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Accounts: Clans Have Not Yet Been Seeded");

            return;
        }

        User user = await context.Users.FirstAsync(cancellationToken);
        Clan clan = await context.Clans.FirstAsync(cancellationToken);

        Account systemAccount = new ()
        {
            Name = "KONGOR",
            User = user,
            Type = AccountType.Staff,
            IsMain = true,
            Clan = null,
            ClanTier = ClanTier.None,
            TimestampJoinedClan = null,
            AscensionLevel = 666,
            AutoConnectChatChannels = [.. ChatChannels.AllDefaultChannels],
            SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
        };

        await context.AddAsync(systemAccount, cancellationToken);

        string[] subAccountNames = [ /* [K] */ "ONGOR", "GOPO", "Xen0byte" ];

        foreach (string subAccountName in subAccountNames)
        {
            Account subAccount = new ()
            {
                Name = subAccountName,
                User = user,
                Type = AccountType.Staff,
                IsMain = false,
                Clan = clan,
                ClanTier = subAccountName is /* [K] */ "ONGOR" ? ClanTier.Leader : ClanTier.Officer,
                TimestampJoinedClan = DateTimeOffset.UtcNow,
                AscensionLevel = 666,
                AutoConnectChatChannels = [.. ChatChannels.AllDefaultChannels, clan.GetChatChannelName()],
                SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
            };

            await context.AddAsync(subAccount, cancellationToken);
        }

        string[] hostAccountNames = [ "HOST", "SERVER" ];

        foreach (string hostAccountName in hostAccountNames)
        {
            Account hostAccount = new ()
            {
                Name = hostAccountName,
                User = user,
                Type = AccountType.ServerHost,
                IsMain = false,
                Clan = null,
                ClanTier = ClanTier.None,
                TimestampJoinedClan = null,
                AscensionLevel = 666,
                AutoConnectChatChannels = [ ChatChannels.ServerHostsChannel ],
                SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
            };

            await context.AddAsync(hostAccount, cancellationToken);
        }

        User userGuest = await context.Users.Skip(1).FirstAsync(cancellationToken);
        Clan clanGuest = await context.Clans.Skip(1).FirstAsync(cancellationToken);

        for (int iterator = 0; iterator < 50 + 1; iterator++)
        {
            Account guestAccount = new ()
            {
                Name = iterator is 0 ? "MODERATOR" : $"GUEST-{iterator:D2}",
                User = userGuest,
                Type = iterator is 0 ? AccountType.MatchModerator : AccountType.Guest,
                IsMain = iterator is 0,
                Clan = clanGuest,
                ClanTier = iterator is 0 ? ClanTier.Leader : ClanTier.Member,
                TimestampJoinedClan = DateTimeOffset.UtcNow,
                AscensionLevel = 0,
                AutoConnectChatChannels = [ ChatChannels.GuestsChannel, clan.GetChatChannelName() ],
                SelectedStoreItems = [ "ai.custom_icon:1", "av.Flamboyant", "c.cat_courier", "cc.frostburnlogo", "cr.Punk Creep", "cs.frostburnlogo", "m.Super-Taunt", "sc.paragon_circle_upgrade", "t.Dumpster_Taunt", "te.Punk TP", "w.8bit_ward" ]
            };

            await context.AddAsync(guestAccount, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        int seededAccountCount = await context.Accounts.CountAsync(cancellationToken);

        logger.LogInformation(@"Seeded {AccountCount} Accounts Across System, Sub-, Host, And Guest Account Types", seededAccountCount);

        // Raise The Skill Rating Of The Non-Guest Staff Accounts Above The Default And Give Them The Highest Possible Medal So They Appear Ranked During Testing

        List<int> staffAccountIDs = await context.Accounts
            .Where(account => account.Type == AccountType.Staff).Select(account => account.ID).ToListAsync(cancellationToken);

        List<AccountStatistics> staffAccountStatistics = await context.AccountStatistics
            .Where(statistics => staffAccountIDs.Contains(statistics.AccountID)).ToListAsync(cancellationToken);

        foreach (AccountStatistics statistics in staffAccountStatistics)
        {
            statistics.SkillRating = 1850.0;
            statistics.HighestMedal = Rank.IMMORTAL;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded A Skill Rating Of {SkillRating} For {StaffAccountCount} Staff Accounts", 1850.0, staffAccountIDs.Count);

        // Also Include Friended/Ignored/Banned Peer Seeding To Reduce The Application Startup Duration

        await SeedFriendedPeers(context, cancellationToken, logger);
        await SeedIgnoredPeers(context, cancellationToken, logger);
        await SeedBannedPeers(context, cancellationToken, logger);
    }

    private static async Task SeedFriendedPeers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.NoneAsync(cancellationToken) || await context.Clans.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Friended Peers: Accounts Or Clans Have Not Yet Been Seeded");

            return;
        }

        if ((await context.Users.Include(user => user.Accounts).FirstAsync(cancellationToken)).Accounts.Any(account => account.FriendedPeers.Count > 0))
        {
            logger.LogDebug("Skipped Seeding Friended Peers: Friended Peers Have Already Been Seeded");

            return;
        }

        Account systemAccount = await context.Accounts.Include(account => account.User).FirstAsync(cancellationToken);

        List<Account> subAccounts = await context.Accounts.Include(account => account.User).Include(account => account.Clan)
            .Where(account => account.User == systemAccount.User).Where(account => account.IsMain.Equals(false))
            .Where(account => account.Type != AccountType.ServerHost).ToListAsync(cancellationToken: cancellationToken);

        List<Account> hostAccounts = await context.Accounts.Where(account => account.Type == AccountType.ServerHost).ToListAsync(cancellationToken: cancellationToken);

        foreach (Account subAccount in subAccounts)
        {
            subAccount.FriendedPeers = subAccounts.Except([subAccount]).Union([systemAccount]).Select(account => new FriendedPeer
            {
                ID = account.ID,
                Name = account.Name,
                ClanTag = account.Clan?.Tag,
                FriendGroup = "PERSONAS"
            }).ToList();
        }

        foreach (Account hostAccount in hostAccounts)
        {
            hostAccount.FriendedPeers = hostAccounts.Except([hostAccount]).Select(account => new FriendedPeer
            {
                ID = account.ID,
                Name = account.Name,
                ClanTag = account.Clan?.Tag,
                FriendGroup = "CLUSTERS"
            }).ToList();
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded Friended Peers For {AccountCount} Accounts", subAccounts.Count + hostAccounts.Count);
    }

    private static async Task SeedIgnoredPeers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Ignored Peers: Accounts Have Not Yet Been Seeded");

            return;
        }

        if ((await context.Users.Include(user => user.Accounts).FirstAsync(cancellationToken)).Accounts.Any(account => account.IgnoredPeers.Count > 0))
        {
            logger.LogDebug("Skipped Seeding Ignored Peers: Ignored Peers Have Already Been Seeded");

            return;
        }

        Account systemAccount = await context.Accounts.FirstAsync(cancellationToken);

        List<Account> guestAccounts = await context.Accounts.Include(account => account.User)
            .Where(account => account.IsMain.Equals(false)).Where(account => account.Type == AccountType.Guest).ToListAsync(cancellationToken: cancellationToken);

        foreach (Account guestAccount in guestAccounts)
        {
            systemAccount.IgnoredPeers.Add(new IgnoredPeer
            {
                ID = guestAccount.ID,
                Name = guestAccount.Name
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded {IgnoredPeerCount} Ignored Peers For Account ""{AccountName}""", guestAccounts.Count, systemAccount.Name);
    }

    private static async Task SeedBannedPeers(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.Accounts.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Banned Peers: Accounts Have Not Yet Been Seeded");

            return;
        }

        if ((await context.Users.Include(user => user.Accounts).FirstAsync(cancellationToken)).Accounts.Any(account => account.BannedPeers.Count > 0))
        {
            logger.LogDebug("Skipped Seeding Banned Peers: Banned Peers Have Already Been Seeded");

            return;
        }

        Account systemAccount = await context.Accounts.FirstAsync(cancellationToken);

        List<Account> guestAccounts = await context.Accounts.Include(account => account.User)
            .Where(account => account.IsMain.Equals(false)).Where(account => account.Type == AccountType.Guest).ToListAsync(cancellationToken: cancellationToken);

        foreach (Account guestAccount in guestAccounts)
        {
            systemAccount.BannedPeers.Add(new BannedPeer
            {
                ID = guestAccount.ID,
                Name = guestAccount.Name,
                BanReason = "Non-Main Guest Account"
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded {BannedPeerCount} Banned Peers For Account ""{AccountName}""", guestAccounts.Count, systemAccount.Name);
    }

    public static async Task SeedOperator(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        const string operatorEmailAddress = OOTB.Accounts.OPERATOR.EmailAddress;
        const string operatorAccountName = OOTB.Accounts.OPERATOR.Name;

        if (await context.Users.AnyAsync(user => user.EmailAddress.Equals(operatorEmailAddress), cancellationToken))
        {
            logger.LogDebug(@"Skipped Seeding {AccountName} Account: It Has Already Been Seeded", operatorAccountName);

            return;
        }

        if (await context.Roles.NoneAsync(cancellationToken))
        {
            logger.LogDebug(@"Skipped Seeding {AccountName} Account: Roles Have Not Yet Been Seeded", operatorAccountName);

            return;
        }

        Role roleCustodian = await context.Roles.SingleAsync(role => role.Name.Equals(UserRoles.Custodian), cancellationToken);

        User userOperator = new ()
        {
            EmailAddress = operatorEmailAddress,
            Role = roleCustodian,
            SRPPasswordSalt = "81c278a3ed03cf4549e787feac5ffe1d051029e15159b1cf0b087b4f8a85cb69",
            SRPPasswordHash = "bfad34f4dc65064dcae6e2064e12d63b299909b6c0cdb33306a91f26bb624e35",
            PBKDF2PasswordHash = "AQAAAAIAAYagAAAAECx4DCy3qYARlevpaVUJXy28QyvXDcQiJZ1KOSx547WDLBV0pdx2MX5m+Pe3xhtQAg=="
        };

        await context.Users.AddAsync(userOperator, cancellationToken);

        Account accountOperator = new ()
        {
            Name = operatorAccountName,
            User = userOperator,
            Type = AccountType.ServerHost,
            IsMain = true,
            AutoConnectChatChannels = [ ChatChannels.ServerHostsChannel ]
        };

        await context.AddAsync(accountOperator, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded {AccountName} Account With Email Address ""{UserEmailAddress}"" And Custodian Role", operatorAccountName, operatorEmailAddress);
    }

    public static async Task SeedHeroGuides(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        if (await context.HeroGuides.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Hero Guides: Hero Guides Have Already Been Seeded");

            return;
        }

        if (await context.Accounts.NoneAsync(cancellationToken))
        {
            logger.LogDebug("Skipped Seeding Hero Guides: Accounts Have Not Yet Been Seeded");

            return;
        }

        Account author = await context.Accounts.FirstAsync(cancellationToken);

        DeserializationDTOs.GuideGetDTO[] guideDTOs = JsonSerializer.Deserialize<DeserializationDTOs.GuideGetDTO[]>
            (await File.ReadAllTextAsync(DataFiles.HeroGuides, cancellationToken), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        // The Game Client Requires At Least One Featured And One Standard Guide Per Hero

        IEnumerable<HeroGuide> guides = guideDTOs.Select(guide => new HeroGuide()
        {
            Name = guide.Name,
            HeroName = guide.HeroName,
            HeroIdentifier = guide.HeroIdentifier,
            Intro = guide.Intro,
            Content = guide.Content,
            StartingItems = guide.StartingItems.ListToPipeSeparatedString(),
            EarlyGameItems = guide.EarlyGameItems.ListToPipeSeparatedString(),
            CoreItems = guide.CoreItems.ListToPipeSeparatedString(),
            LuxuryItems = guide.LuxuryItems.ListToPipeSeparatedString(),
            AbilityQueue = guide.AbilityQueue.ListToPipeSeparatedString(),
            Author = author,
            Rating = guide.Rating,
            UpVotes = guide.UpVotes,
            DownVotes = guide.DownVotes,
            Public = guide.Public,
            Featured = guide.Featured,
            TimestampCreated = DateTimeOffset.UtcNow
        });

        await context.HeroGuides.AddRangeAsync(guides, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(@"Seeded {HeroGuideCount} Hero Guides Authored By Account ""{AccountName}""", guideDTOs.Length, author.Name);
    }

    /// <summary>
    ///     Seeds default War Effort quests and reward bags if they have not yet been seeded.
    /// </summary>
    public static async Task SeedWarEffort(MerrickContext context, CancellationToken cancellationToken, ILogger logger)
    {
        bool questsExist = await context.Quests.AnyAsync(cancellationToken);
        bool bagsExist = await context.WarEffortBags.AnyAsync(cancellationToken);

        List<Quest> defaultQuests =
        [
                new ()
                {
                    QuestKey = "daily_games_won",
                    Title = "Victory March",
                    Description = "Win 1 battle in any mode",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchWin,
                    TriggerName = "achievement_value_games_won",
                    TargetCount = 1,
                    ReputationReward = 25,
                    TypeID = 1,
                    TypeKey = "games_won",
                    RewardSilver = 50,
                    RewardTickets = 1,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/games_won.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_caldavar_won",
                    Title = "Forests of Caldavar",
                    Description = "Achieve victory on Forests of Caldavar",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchWin,
                    TriggerName = "achievement_value_caldavar_won",
                    TargetCount = 1,
                    ReputationReward = 30,
                    TypeID = 2,
                    TypeKey = "caldavar_won",
                    RewardSilver = 60,
                    RewardTickets = 1,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/caldavar_won.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_mid_wars_won",
                    Title = "Mid Wars Brawler",
                    Description = "Claim victory in Mid Wars",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchWin,
                    TriggerName = "achievement_value_mid_wars_won",
                    TargetCount = 1,
                    ReputationReward = 25,
                    TypeID = 3,
                    TypeKey = "mid_wars_won",
                    RewardSilver = 50,
                    RewardTickets = 1,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/mid_wars_won.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_hero_kills",
                    Title = "Deadly Hunter",
                    Description = "Slay 5 enemy heroes in combat",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.Kills,
                    TriggerName = "achievement_value_prophet_kills",
                    TargetCount = 5,
                    ReputationReward = 25,
                    TypeID = 4,
                    TypeKey = "prophet_kills",
                    RewardSilver = 60,
                    RewardTickets = 1,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/prophet_kills.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_role_support",
                    Title = "Guardian of Newerth",
                    Description = "Play 1 match as a Support hero",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchesPlayed,
                    TriggerName = "achievement_value_role_played",
                    HeroRoleString = "support",
                    TargetCount = 1,
                    ReputationReward = 20,
                    TypeID = 5,
                    TypeKey = "role_support",
                    RewardSilver = 40,
                    RewardTickets = 0,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/role_support.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_role_carry_ganker",
                    Title = "Apex Striker",
                    Description = "Play 1 match as a Carry/Ganker hero",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchesPlayed,
                    TriggerName = "achievement_value_role_played",
                    HeroRoleString = "carry,ganker",
                    TargetCount = 1,
                    ReputationReward = 25,
                    TypeID = 6,
                    TypeKey = "role_carry_ganker",
                    RewardSilver = 50,
                    RewardTickets = 1,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/role_carry_ganker.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_legion_strength",
                    Title = "Legion Might",
                    Description = "Play 1 match as a Legion Strength hero",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchesPlayed,
                    TriggerName = "achievement_value_attribute_played",
                    TeamID = 1,
                    AttributeCode = 0,
                    TargetCount = 1,
                    ReputationReward = 25,
                    TypeID = 7,
                    TypeKey = "legion_strength",
                    RewardSilver = 50,
                    RewardTickets = 0,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/legion_strength.tga",
                    IsActive = true
                },
                new ()
                {
                    QuestKey = "daily_hellbourne_agility",
                    Title = "Hellbourne Agility",
                    Description = "Play 1 match as a Hellbourne Agility hero",
                    Category = QuestCategory.Daily,
                    TargetType = QuestTargetType.MatchesPlayed,
                    TriggerName = "achievement_value_attribute_played",
                    TeamID = 2,
                    AttributeCode = 1,
                    TargetCount = 1,
                    ReputationReward = 25,
                    TypeID = 8,
                    TypeKey = "hellbourne_agility",
                    RewardSilver = 50,
                    RewardTickets = 0,
                    ProductIconPath = "/ui/fe2/quests/quest_icons/hellbourne_agility.tga",
                    IsActive = true
                }
            ];

        List<Quest> existingQuests = await context.Quests.ToListAsync(cancellationToken);
        Dictionary<string, Quest> existingQuestsByKey = existingQuests.ToDictionary(q => q.QuestKey, StringComparer.OrdinalIgnoreCase);

        foreach (Quest defaultQuest in defaultQuests)
        {
            if (existingQuestsByKey.TryGetValue(defaultQuest.QuestKey, out Quest? existing))
            {
                existing.Title = defaultQuest.Title;
                existing.Description = defaultQuest.Description;
                existing.Category = defaultQuest.Category;
                existing.TargetType = defaultQuest.TargetType;
                existing.TriggerName = defaultQuest.TriggerName;
                existing.TargetCount = defaultQuest.TargetCount;
                existing.ReputationReward = defaultQuest.ReputationReward;
                existing.TypeID = defaultQuest.TypeID;
                existing.TypeKey = defaultQuest.TypeKey;
                existing.RewardSilver = defaultQuest.RewardSilver;
                existing.RewardTickets = defaultQuest.RewardTickets;
                existing.ProductIconPath = defaultQuest.ProductIconPath;
                existing.HeroProduct = defaultQuest.HeroProduct;
                existing.HeroRoleString = defaultQuest.HeroRoleString;
                existing.AttributeCode = defaultQuest.AttributeCode;
                existing.AttackType = defaultQuest.AttackType;
                existing.TeamID = defaultQuest.TeamID;
                existing.IsActive = defaultQuest.IsActive;
            }
            else
            {
                await context.Quests.AddAsync(defaultQuest, cancellationToken);
            }
        }

        logger.LogInformation("Synchronised {QuestCount} War Effort Quests", defaultQuests.Count);

        if (bagsExist.Equals(false))
        {
            List<WarEffortBag> defaultBags =
            [
                new () { BagNumber = 1,  Name = "Bronze Satchel I",        RequiredReputation = 50,  RewardType = "MMPOINTS", RewardSilver = 50,  RewardGold = 0,   RewardTickets = 0,  IsActive = true },
                new () { BagNumber = 2,  Name = "Bronze Satchel II",       RequiredReputation = 75,  RewardType = "MMPOINTS", RewardSilver = 75,  RewardGold = 0,   RewardTickets = 0,  IsActive = true },
                new () { BagNumber = 3,  Name = "Silver Satchel I",        RequiredReputation = 100, RewardType = "MMPOINTS", RewardSilver = 100, RewardGold = 0,   RewardTickets = 1,  IsActive = true },
                new () { BagNumber = 4,  Name = "Silver Satchel II",       RequiredReputation = 125, RewardType = "MMPOINTS", RewardSilver = 125, RewardGold = 0,   RewardTickets = 1,  IsActive = true },
                new () { BagNumber = 5,  Name = "Silver Satchel III",      RequiredReputation = 150, RewardType = "MMPOINTS", RewardSilver = 150, RewardGold = 0,   RewardTickets = 2,  IsActive = true },
                new () { BagNumber = 6,  Name = "Gold Chest I",            RequiredReputation = 175, RewardType = "POINTS",   RewardSilver = 100, RewardGold = 10,  RewardTickets = 1,  IsActive = true },
                new () { BagNumber = 7,  Name = "Gold Chest II",           RequiredReputation = 200, RewardType = "POINTS",   RewardSilver = 150, RewardGold = 20,  RewardTickets = 1,  IsActive = true },
                new () { BagNumber = 8,  Name = "Gold Chest III",          RequiredReputation = 225, RewardType = "POINTS",   RewardSilver = 200, RewardGold = 25,  RewardTickets = 2,  IsActive = true },
                new () { BagNumber = 9,  Name = "Diamond Chest I",         RequiredReputation = 250, RewardType = "POINTS",   RewardSilver = 250, RewardGold = 30,  RewardTickets = 2,  IsActive = true },
                new () { BagNumber = 10, Name = "Diamond Chest II",        RequiredReputation = 275, RewardType = "POINTS",   RewardSilver = 300, RewardGold = 40,  RewardTickets = 3,  IsActive = true },
                new () { BagNumber = 11, Name = "Diamond Chest III",       RequiredReputation = 300, RewardType = "POINTS",   RewardSilver = 350, RewardGold = 50,  RewardTickets = 3,  IsActive = true },
                new () { BagNumber = 12, Name = "Master Chest I",          RequiredReputation = 350, RewardType = "POINTS",   RewardSilver = 400, RewardGold = 75,  RewardTickets = 4,  IsActive = true },
                new () { BagNumber = 13, Name = "Master Chest II",         RequiredReputation = 400, RewardType = "POINTS",   RewardSilver = 500, RewardGold = 100, RewardTickets = 5,  IsActive = true },
                new () { BagNumber = 14, Name = "Treasure of Champions",   RequiredReputation = 500, RewardType = "POINTS",   RewardSilver = 1000, RewardGold = 250, RewardTickets = 10, IsFinalTier = true, IsActive = true }
            ];

            await context.WarEffortBags.AddRangeAsync(defaultBags, cancellationToken);
            logger.LogInformation("Seeded {BagCount} War Effort Bags", defaultBags.Count);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
