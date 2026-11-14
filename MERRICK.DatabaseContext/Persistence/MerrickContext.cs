namespace MERRICK.DatabaseContext.Persistence;

public sealed class MerrickContext : DbContext
{
    public MerrickContext(DbContextOptions options) : base(options)
    {
        if (Database.IsInMemory().Equals(false))
            Database.SetCommandTimeout(60); // 1 Minute - Helps Prevent Migrations From Timing Out When Many Records Need To Update
    }

    public const string MetadataSchema = "meta";
    public const string DefaultSchema = "data";
    public const string CoreSchema = "core";
    public const string AuthenticationSchema = "auth";
    public const string StatisticsSchema = "stat";
    public const string MiscellaneousSchema = "misc";
    public const string AdministrationSchema = "admin";
    public const string TelemetrySchema = "telemetry";
    public const string EconomySchema = "economy";

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountCompletedBag> AccountCompletedBags => Set<AccountCompletedBag>();
    public DbSet<AccountDailyCheckIn> AccountDailyCheckIns => Set<AccountDailyCheckIn>();
    public DbSet<AccountQuestProgress> AccountQuestProgresses => Set<AccountQuestProgress>();
    public DbSet<AccountStatistics> AccountStatistics => Set<AccountStatistics>();
    public DbSet<AccountWarEffort> AccountWarEfforts => Set<AccountWarEffort>();
    public DbSet<Clan> Clans => Set<Clan>();
    public DbSet<ConfigurationBackup> ConfigurationBackups => Set<ConfigurationBackup>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<DailySpecial> DailySpecials => Set<DailySpecial>();
    public DbSet<GenesisReward> GenesisRewards => Set<GenesisReward>();
    public DbSet<GeoInformation> GeoInformations => Set<GeoInformation>();
    public DbSet<GeoName> GeoNames => Set<GeoName>();
    public DbSet<HardwareSnapshot> HardwareSnapshots => Set<HardwareSnapshot>();
    public DbSet<HeroGuide> HeroGuides => Set<HeroGuide>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
    public DbSet<Mastery> Masteries => Set<Mastery>();
    public DbSet<MasteryRewards> MasteryRewards => Set<MasteryRewards>();
    public DbSet<MatchStatistics> MatchStatistics => Set<MatchStatistics>();
    public DbSet<MatchParticipantStatistics> MatchParticipantStatistics => Set<MatchParticipantStatistics>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<NicknameChange> NicknameChanges => Set<NicknameChange>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<RedeemableCode> RedeemableCodes => Set<RedeemableCode>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<StaffMemberAction> StaffMemberActions => Set<StaffMemberAction>();
    public DbSet<StatsReset> StatsResets => Set<StatsReset>();
    public DbSet<Suspension> Suspensions => Set<Suspension>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<UniqueHardware> UniqueHardwares => Set<UniqueHardware>();
    public DbSet<User> Users => Set<User>();
    public DbSet<WarEffortBag> WarEffortBags => Set<WarEffortBag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureSchemas(builder);

        ConfigureRoles(builder.Entity<Role>());
        ConfigureUsers(builder.Entity<User>());
        ConfigureAccounts(builder.Entity<Account>());
        ConfigureConfigurationBackup(builder.Entity<ConfigurationBackup>());
        ConfigureAccountStatistics(builder.Entity<AccountStatistics>());
        ConfigureMastery(builder.Entity<Mastery>());
        ConfigureMasteryRewards(builder.Entity<MasteryRewards>());
        ConfigureMatchStatistics(builder.Entity<MatchStatistics>());
        ConfigureMatchParticipantStatistics(builder.Entity<MatchParticipantStatistics>());
        ConfigureTokens(builder.Entity<Token>());
        ConfigureGeoNames(builder.Entity<GeoName>());
    }

    private static void ConfigureSchemas(ModelBuilder builder)
    {
        builder.HasDefaultSchema(DefaultSchema);

        builder.Entity<Account>().ToTable("Accounts", CoreSchema);
        builder.Entity<AccountCompletedBag>().ToTable("AccountCompletedBags", EconomySchema);
        builder.Entity<AccountDailyCheckIn>().ToTable("AccountDailyCheckIns", EconomySchema);
        builder.Entity<AccountQuestProgress>().ToTable("AccountQuestProgresses", EconomySchema);
        builder.Entity<AccountStatistics>().ToTable("AccountStatistics", StatisticsSchema);
        builder.Entity<AccountWarEffort>().ToTable("AccountWarEfforts", EconomySchema);
        builder.Entity<Clan>().ToTable("Clans", CoreSchema);
        builder.Entity<ConfigurationBackup>().ToTable("ConfigurationBackups", MiscellaneousSchema);
        builder.Entity<Coupon>().ToTable("Coupons", EconomySchema);
        builder.Entity<DailySpecial>().ToTable("DailySpecials", EconomySchema);
        builder.Entity<GenesisReward>().ToTable("GenesisRewards", EconomySchema);
        builder.Entity<GeoInformation>().ToTable("GeoInformations", TelemetrySchema);
        builder.Entity<GeoName>().ToTable("GeoNames", TelemetrySchema);
        builder.Entity<HardwareSnapshot>().ToTable("HardwareSnapshots", TelemetrySchema);
        builder.Entity<HeroGuide>().ToTable("HeroGuides", MiscellaneousSchema);
        builder.Entity<IncidentReport>().ToTable("IncidentReports", AdministrationSchema);
        builder.Entity<Mastery>().ToTable("Masteries", StatisticsSchema);
        builder.Entity<MasteryRewards>().ToTable("MasteryRewards", StatisticsSchema);
        builder.Entity<MatchParticipantStatistics>().ToTable("MatchParticipantStatistics", StatisticsSchema);
        builder.Entity<MatchStatistics>().ToTable("MatchStatistics", StatisticsSchema);
        builder.Entity<Message>().ToTable("Messages", CoreSchema);
        builder.Entity<NicknameChange>().ToTable("NicknameChanges", CoreSchema);
        builder.Entity<Quest>().ToTable("Quests", EconomySchema);
        builder.Entity<RedeemableCode>().ToTable("RedeemableCodes", MiscellaneousSchema);
        builder.Entity<Role>().ToTable("Roles", AuthenticationSchema);
        builder.Entity<StaffMemberAction>().ToTable("StaffMemberActions", AdministrationSchema);
        builder.Entity<StatsReset>().ToTable("StatsResets", EconomySchema);
        builder.Entity<Suspension>().ToTable("Suspensions", AdministrationSchema);
        builder.Entity<Token>().ToTable("Tokens", AuthenticationSchema);
        builder.Entity<UniqueHardware>().ToTable("UniqueHardwares", TelemetrySchema);
        builder.Entity<User>().ToTable("Users", CoreSchema);
        builder.Entity<WarEffortBag>().ToTable("WarEffortBags", EconomySchema);
    }

    private static void ConfigureRoles(EntityTypeBuilder<Role> builder)
    {
        builder.HasData
        (
            new Role
            {
                ID = 1,
                Name = UserRoles.Administrator
            },

            new Role
            {
                ID = 2,
                Name = UserRoles.User
            },

            new Role
            {
                ID = 3,
                Name = UserRoles.Custodian
            }
        );
    }

    private static ValueComparer<List<string>> StringListValueComparer => new
    (
        (first, second) => (first ?? new List<string>()).SequenceEqual(second ?? new List<string>()),
        collection => collection.Aggregate(0, (accumulatedHashCode, value) => HashCode.Combine(accumulatedHashCode, value.GetHashCode())),
        collection => collection.ToList()
    );

    private static ValueComparer<List<int>> IntListValueComparer => new
    (
        (first, second) => (first ?? new List<int>()).SequenceEqual(second ?? new List<int>()),
        collection => collection.Aggregate(0, (accumulatedHashCode, value) => HashCode.Combine(accumulatedHashCode, value.GetHashCode())),
        collection => collection.ToList()
    );

    private static ValueComparer<Dictionary<string, string>> DictionaryStringStringValueComparer => new
    (
        (first, second) => (first ?? new Dictionary<string, string>()).SequenceEqual(second ?? new Dictionary<string, string>()),
        dictionary => dictionary.Aggregate(0, (accumulatedHashCode, pair) => HashCode.Combine(accumulatedHashCode, pair.Key.GetHashCode(), pair.Value.GetHashCode())),
        dictionary => dictionary.ToDictionary(pair => pair.Key, pair => pair.Value)
    );

    private static void ConfigureGeoNames(EntityTypeBuilder<GeoName> builder)
    {
        builder.Property(geoName => geoName.Names).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<Dictionary<string, string>>(value, new JsonSerializerOptions()) ?? new Dictionary<string, string>(),
            DictionaryStringStringValueComparer
        );
    }

    private static void ConfigureMastery(EntityTypeBuilder<Mastery> builder)
    {
        builder.OwnsMany(mastery => mastery.HeroExperiences, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
    }

    private static void ConfigureMasteryRewards(EntityTypeBuilder<MasteryRewards> builder)
    {
        builder.Property(rewards => rewards.ClaimedLevels).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<List<int>>(value, new JsonSerializerOptions()) ?? new List<int>(),
            IntListValueComparer
        );
    }

    private static void ConfigureUsers(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.OwnedStoreItems).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<List<string>>(value, new JsonSerializerOptions()) ?? new List<string>(),
            StringListValueComparer
        );
    }

    private static void ConfigureAccounts(EntityTypeBuilder<Account> builder)
    {
        builder.Property(account => account.SelectedStoreItems).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<List<string>>(value, new JsonSerializerOptions()) ?? new List<string>(),
            StringListValueComparer
        );

        builder.OwnsMany(account => account.BannedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.FriendedPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
        builder.OwnsMany(account => account.IgnoredPeers, ownedNavigationBuilder => { ownedNavigationBuilder.ToJson(); });
    }

    private static void ConfigureConfigurationBackup(EntityTypeBuilder<ConfigurationBackup> builder)
    {
        builder.HasOne(configurationBackup => configurationBackup.Account)
            .WithOne(account => account.ConfigurationBackup)
            .HasForeignKey<ConfigurationBackup>(configurationBackup => configurationBackup.AccountID)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAccountStatistics(EntityTypeBuilder<AccountStatistics> builder)
    {
        builder.OwnsOne(statistics => statistics.HeroStatistics, ownedNavigationBuilder =>
        {
            ownedNavigationBuilder.ToJson();
            ownedNavigationBuilder.OwnsMany(summary => summary.Heroes);
        });

        builder.OwnsOne(statistics => statistics.AwardStatistics, ownedNavigationBuilder =>
        {
            ownedNavigationBuilder.ToJson();
        });
    }

    private static void ConfigureMatchParticipantStatistics(EntityTypeBuilder<MatchParticipantStatistics> builder)
    {
        builder.Property(statistics => statistics.Inventory).HasConversion
        (
            value => JsonSerializer.Serialize(value, new JsonSerializerOptions()),
            value => JsonSerializer.Deserialize<List<string>>(value, new JsonSerializerOptions()) ?? new List<string>(),
            new ValueComparer<List<string>>((first, second) => (first ?? new List<string>()).SequenceEqual(second ?? new List<string>()),
                collection => collection.Aggregate(0, (accumulatedHashCode, value) => HashCode.Combine(accumulatedHashCode, value.GetHashCode())), collection => collection.ToList())
        );

        builder.OwnsMany(statistics => statistics.ItemHistory, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
        builder.OwnsMany(statistics => statistics.AbilityHistory, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
        builder.OwnsOne(statistics => statistics.MasteryProgression, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
    }

    private static void ConfigureMatchStatistics(EntityTypeBuilder<MatchStatistics> builder)
    {
        builder.OwnsMany(statistics => statistics.FragHistory, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
    }

    private static void ConfigureTokens(EntityTypeBuilder<Token> builder)
    {
        // SQL Server's "time" Type Tops Out Below 24 Hours, So Validity Is Persisted As Tick Count (bigint).
        builder.Property(token => token.Validity).HasConversion
        (
            value => value.Ticks,
            value => TimeSpan.FromTicks(value)
        );
    }
}
