export default {
  apiError: {
    // Unhandled Error Page
    title: 'Something Went Wrong',
    ok: 'OK',
    serviceUnavailable: 'The service is currently unavailable.',
  },

  validation: {
    title: 'Something Went Wrong',
    rules: {
      required: 'The Field {key} Is Required',
      email: 'The Field {key} Must Be An E-mail Address',
      match: 'The Field {key} And {param} Must Be Identical',
      passwordPolicy:
        'The Field {key} must be at least 8 characters, have at least 4 unique characters, have at least one non alphanumeric character, at least one lower case alpha character, at least one upper case alpha character and at least one digit.',
      usernamePolicy:
        'The Field {key} must contain only A-Z 0-9 and ` _ - and be between 4 and 12 characters long.',
    },
  },

  fp: {
    // Home Page (TODO: Rename "fp" To "home")
    title: 'Home',
    ladder: 'Ladder',
    community: 'Join Our Discord Server',
    login: 'Log In',
    playNow: 'Register',
    playWith: 'Play HoN With {amount} Others Now',
    register: 'Register',
    playersOnline: 'Players Online',
    registeredAccounts: 'Registered Accounts',
    matchesInProgress: 'Matches In Progress',
    featuredStreamer: 'Featured Stream',
    showFestivities: 'Enable Seasonal Theme',
    timeline: {
      title: 'Project KONGOR Timeline',
      a: {
        date: 'December 13th 2021',
        title: "HoN's End-Of-Life Is Announced",
        text: 'Frostburn announces that the game will shut down on June 20th 2022.',
      },
      b: {
        date: 'December 13th 2021',
        title: 'Project KONGOR Development Begins',
        text: 'Packet capturing starts and the first proof-of-concept is released. The proof-of-concept consisted of logging in and playing Plinko.',
      },
      c: {
        date: 'June 20th 2022',
        title: 'Official HoN Servers Shut Down',
        text: 'The game first launched on May 12th 2010, running for a little over 13 years, before finally shutting down.',
      },
      d: {
        date: 'August 5th 2022',
        title: 'Project KONGOR Enters Open Beta Testing',
        text: 'Project KONGOR publicly invites all players to start using the service.',
      },
      e: {
        date: 'January 5th 2023',
        title: 'Project KONGOR Reaches First Major Milestones',
        text: 'A grand total of 1 000 000 matches played since August 2022.',
      },
    },
  },

  ladder: {
    title: 'Ladder',
    gameModes: {
      caldavar: 'Caldavar',
      caldavarCasual: 'Caldavar (Casual)',
      midwars: 'Midwars',
      warEffort: 'The War Effort (Quests)',
    },
    subTitle: 'Top {x} Rated Players For {gameMode}',
    search: 'Search',
    mmr: '{rating} MMR',
    gamesPlayed: 'Games',
    winRate: 'Win Rate',
    KDA: 'K/D/A',
    GPM: 'GPM',
    APM: 'APM',
  },

  login: {
    // Login Page
    title: 'Login',
    username: 'Username',
    password: 'Password',
    passwordMatch: 'Repeat Password',
    forgotPassword: 'Forgotten Password',
    login: 'Log In',
    youMustBeAuthed: 'You Must Be Logged In To See This Page',
    youAreLoggedOut: 'You Are Now Logged Out',
    emailAddress: 'Email Address',
    forgotPasswordSubmit: 'Send Password Update Email',
    changePasswordAction: 'Update Password',
    passwordRequestResetSuccessTitle: 'Password Update Request Sent',
    passwordRequestResetSuccessMessage:
      'Check The Email Sent To "{email}" For Further Instructions',
    changePasswordTitle: 'Update Your Password',
    changePasswordSuccessTitle: 'Your Password Has Been Updated',
    changePasswordSuccessMessage: 'Your Password Has Been Successfully Updated',
  },

  register: {
    // Registration Page
    title: 'Register',
    username: 'Username',
    password: 'Password',
    passwordMatch: 'Repeat Password',
    emailAddress: 'Email Address',
    emailAddressMatch: 'Repeat Email Address',
    registerAction: 'Register',
    registerSuccessTitle: 'Email Registered',
    registerSuccessfulMessage:
      'Check The Email Sent To "{email}" For Further Instructions',
    completeRegisterAction: 'Complete Registration',
    completeYourRegistrationTitle: 'Complete Registration',
    registerCompleteSuccessTitle: 'Your Account Has Been Created',
    registerCompleteSuccessMessage:
      "If you don't already have the HoN client installed and you need help with that, ask around the community. A good place to start would be our Discord server.",
    joinOurDiscord: 'Join Our Discord Server',
  },

  dashboard: {
    // Account Page
    title: 'Account',
    mySettingsBtn: 'Account Settings',
    logout: 'Log Out',
    welcome: 'Welcome, {name}',
    menuTitles: {
      gmTools: 'Game Master tools',
      sgmTools: 'Senior Game Master tools',
      adminTools: 'Administrator tools',
    },
    youAreNotAllowedHere:
      'You cant see this page. Sorry, if you believe this is wrong, ask a Admin',

    mySettings: {
      // Account Settings Sub-Page
      title: 'Account Settings',

      changeEmail: {
        // Update Email Tab
        title: 'Update Email Address',
        emailNotVerified: 'Email Address Not Verified',
        emailVerified: 'Email Address Verified',
        emailAddress: 'Enter New Email Address',
        emailAddressMatch: 'Repeat New Email Address',
        changeEmailAction: 'Update Email Address',
        password: 'Enter Current Password',
        changeRequestSuccessTitle: 'Email Address Update Request Sent',
        changeRequestSuccessMessage:
          'Check The Email Sent To "{email}" For Further Instructions',
        emailChangedSuccessfully:
          'Your Email Address Has Been Successfully Updated',
      },
    },

    cai: {
      title: 'Custom Account Icon',
      selectFile: 'Click here to select a file and continue.',
      instructions:
        'Use your scroll wheel to zoom in and out, and your mouse to move the stencil.',
      uploadAction: 'Looks good, upload it!',
      uploadedWithSuccess:
        'Custom account icon added, you may need to restart your game before you can see it in your vault.',
    },

    gm: {
      suspension: {
        title: 'Suspend A Player',
        lookup: 'Look Up Account',
        suspensionId: 'Suspension #{id}',
        previousSuspensions: 'Previous Suspensions ({count})',
        noBanMessage:
          'The account "{accountName}" has no previous suspensions.',
        accountName: 'Account Name',
        duration: 'Duration',
        reason: 'Reason',
        internalDetails: 'Internal Details (Only Visible To (S)GMs And Admins)',
        submit: 'Submit',
        suspensionSuccessful:
          'Suspension successful, the account "{accountName}" has been issued a ban for "{duration}" with the reason "{reason}" (internal details: {internalDetails}).',
        year: 'Years',
        month: 'Months',
        week: 'Weeks',
        day: 'Days',
        codeOfConduct: {
          title: 'Select a subject to continue.',
          chatVerbalAbuse: {
            subject: 'Chat/Verbal Abuse',
            description:
              'Insults towards ones race, gender, sex, sexual orientation, national origin or socio-economic status, harassment, etc. (light trash talk, casual swearing, calling people noobs is not bannable).',
          },
          abilityAbuse: {
            subject: 'Ability Abuse',
            description:
              'Using a hero’s ability to reveal location, intent to cause death by maliciously using an ability, any other use of a heroes ability in a malicious manner, etc.',
          },
          itemAbuse: {
            subject: 'Item Abuse',
            description:
              'Maliciously destroying or selling items, maliciously placing wards in well, tower or neutral creep spawn, buying droppable items with intent to give to the enemy team, etc.',
          },
          staffImpersonation: {
            subject: 'Staff Impersonation',
            description:
              'Any player that states they are a staff member (Game Master, Developer, Server Host, Community Manager, Moderator, Collaborator, etc) or acts in a manner that causes someone to believe they are staff when they are not.',
          },
          threats: {
            subject: 'Threats',
            description:
              'Wishing harm, illness, death towards a player, their family or friends, doxxing, etc.',
          },
          hateSpeech: {
            subject: 'Hate Speech',
            description:
              'Hate speech regarding race, sex, sexual orientation, religion, national origin, etc.',
          },
          intentToLose: {
            subject: 'Intent To Lose',
            description:
              "Intentional feeding (including disconnected heroes), sabotage, calling out teammates positioning, refusal to participate (where a player intentionally refuses to participate during critical periods of the game with malicious intent, like not defending base when a logical player would, not pushing with the team when the team is asking you to push with them, etc), avoiding AFK, griefing (maliciously stealing last hits, only participating in match to last hit hero kills, other bad-faith actions or behavior that intentionally annoys other players or has a negative impact on the outcome of the game, etc). (Stealing a lane, choosing a bad hero, poor gameplay, or stealing farm is not bannable unless you have a reason to believe they have malicious intent. Disagreeing with a player's game play will happen, as long as they are not refusing to participate with the team or griefing then it is not a bannable offense).",
          },
          cheating: {
            subject: 'Cheating',
            description:
              'Account sharing, bug abuse, hacks, exploits, ban evasion, etc.',
          },
          inappropriate: {
            subject: 'Inappropriate Account Name/Icon',
            description:
              'Any player that creates an account with an account name or uploads a custom account icon that is offensive, racist, homophobic, sexist, vulgar, repulsive, contains nudity, etc. (+ Removal of Account Icon if applicable + Forced Name Change if applicable).',
          },
          statManipulation: {
            subject: 'Stat Manipulation',
            description:
              'Intentionally leaving matches to lower your MMR or increase an opposing player or teams MMR, intentionally losing/winning/dying to another player or team to increase or decrease winrate/MMR/kdr, etc. (+ Permanent Sub Account Suspension)',
          },
          serialLeaving: {
            subject: 'Serial Leaving',
            description:
              'If a player leaves 3 or more games within 1 day (including sub-accounts).',
          },
        },
      },
      rapm: {
        title: 'RAP management (placeholder)',
      },
    },
    staff: {
      promotions: {
        title: 'Promote/Demote Game Master',
        promoteToGameMaster: 'Promote to Game Master',
        gmRoster: 'Game Master roster',
        sgmRoster: 'Senior Game Master roster',
        adminRoster: 'Administrator roster',
        userName: 'User Name',
        promote: 'Promote',
        demote: 'Demote',
        promoteSuccess: 'Successfully Promoted {userName} To Game Master',
        demoteSuccess: 'Successfully Demoted {userName} From Game Master',
      },
      dataPoints: {
        title: 'Datapoints',
        firstAccountName: 'First Account Name',
        secondAccountName: 'Second Account Name',
        lookup: 'Lookup',
        result: 'Result',
        preDelete: 'Prepare removal',
        preDeleteDescription:
          'Please click on the account name you want to delete the selected data point from',
        removeSuccessful:
          'Successfully deleted {category}: {dataPoint} from {accountName}',
      },
      suspensions: {
        title: 'Suspensions',
        suspensionID: 'Suspension ID',
        overrideSuccessful: 'Suspension {suspensionId} successfully overridden',
        override: 'Override suspension',
        newDurationSuccessful:
          'Suspension {suspensionId} duration successfully changed to {newDuration} ',
        newDuration: 'New duration',
        changeDuration: 'Change suspension duration',
        duration: 'Duration',
        year: 'Years',
        month: 'Months',
        week: 'Weeks',
        day: 'Days',
      },
      enableDisable: {
        title: 'Enable / Disable Account',
        accountName: 'Account name',
        disableAccount: 'Disable account',
        enableAccount: 'Enable account',
        lookup: 'Lookup',
        currentAccountType: 'Current account type: {accountType}',
        successfullyDisabledAccount: 'The account "{accountName}" was disabled',
        successfullyEnabledAccount: 'The account "{accountName}" was enabled',
      },
      promoteToTMMHost: {
        title: 'Promote to TMM host',
        accountNameToPromote: 'Account name',
        promote: 'Pormote',
        successfullyPromotedAccount:
          'The account "{accountName}" was promoted to TMM host',
      },
      currency: {
        title: 'Currency',
        instructions:
          'Use "add row" to add more rows. Empty User Names will be ignored',
        userName: 'User Name',
        gold: 'Gold',
        silver: 'Silver',
        tickets: 'Tickets',
        addRow: 'Add row',
        handleBatchRows: 'Handle batch rows',
        runBatchJob: 'Run batch job',
        result: 'Results',
        batchLogMessage:
          'Added {gold} gold, {silver} silver and {tickets} tickets to {userName}.',
        batchLogErrorMessage: 'Failed to add anything to {userName}.',
      },
      lookUpAccountOrUser: {
        title: 'Look Up Account Or User',
        fuzzyIdentifier: 'Enter Account Name, User Name, Or Email Address',
        lookup: 'Lookup',
        result: 'Result',
        lookupSuspensions: 'Look Up Suspensions',
        enableDisable: 'Enable/Disable',
      },
      actions: {
        title: 'Action history',
        sourceName: 'Source',
        targetName: 'Target',
        action: 'Type of action',
        details: 'Details',
        timestamp: 'Timestamp',
      },
    },
    admin: {
      upgrades: {
        title: 'Upgrades',
        selectUpgrade: 'Select one or more upgrades to add',
        selectUpgradeRemove: 'Select one or more upgrades to remove',
        addUpgrades: 'Add upgrades',
        accountName: 'Account name',
        removeUpgrades: 'Remove upgrades',
        removeSelectedUpgrades: 'Remove "{upgrades}" from "{accountName}"?',
        successfullyAddedUpgrades:
          'Successfully added "{upgrades}" upgrades to "{accountName}"',
        successfullyRemovedUpgrades:
          'Successfully removed "{upgrades}" upgrades from "{accountName}"',
      },
      renameAccount: {
        title: 'Rename account',
        rename: 'Rename',
        accountName: 'Account name',
        newAccountName: 'New account name',
        successfullyRenamedAccount:
          'The account "{accountName}" was renamed to "{newAccountName}"',
      },
    },
  },
  quests: {
    title: 'The War Effort & Quests',
    subtitle: 'Complete daily objectives and accumulate Reputation Points to unlock 14 reward satchels & chests',
    warEffortCycle: 'The War Effort — Cycle {cycle}',
    cycleEndsIn: 'Cycle ends in',
    totalReputation: 'Total Cycle Reputation',
    activeTier: 'Next Milestone',
    tierProgress: 'Current Bag Progress',
    openBag: 'Open Reward',
    opened: 'Opened',
    locked: 'Locked',
    readyToOpen: 'Ready to Open!',
    satchel: 'Supply Satchel',
    chest: 'Treasure Chest',
    finalChest: 'Treasure of Champions',
    dailySlots: 'Active Daily Objectives',
    dailyTitle: 'Daily Quests',
    weeklyTitle: 'Weekly Challenges',
    progress: 'Progress',
    completed: 'Objective Completed',
    claimed: 'Reward Claimed',
    claimReward: 'Claim Reward',
    resetsIn: 'Resets in',
    rewards: 'Rewards',
    reputationReward: '+{points} Reputation Points (RP)',
    bagOpenedTitle: 'Congratulations! Reward Unlocked',
    simulateMatch: 'Simulate Match Action (Test Quests & War Effort)',
    simulateWin: '+1 Win (Caldavar/Mid Wars)',
    simulateKill: '+1 Hero Kill',
    simulateWard: '+1 Ward Placed',
    simulateAssist: '+1 Assist',
    simulateMatchPlayed: '+1 Match Played',
  },
  minigames: {
    title: 'Lucky Wheel of Fortune',
    subtitle: 'Heroes of Newerth Lucky Wheel of Fortune (Free spin every 8 hours)',
    checkInTitle: '7-Day Check-in Streak',
    checkInSubtitle: 'Log in daily to claim escalating rewards and tickets',
    checkInButton: 'Check In Today',
    alreadyCheckedIn: 'Checked In Today',
    dayStreak: 'Day {day}',
    jackpotDay: 'Jackpot Day!',
    luckySpinTitle: 'Lucky Wheel of Fortune',
    luckySpinSubtitle: 'Spin the wheel free every 8 hours or use Plinko Tickets',
    spinFree: 'Free Spin (Every 8h)',
    spinWithTicket: 'Spin with Ticket (1 Ticket)',
    freeSpinsLeft: 'Free Spin Available!',
    ticketBalance: 'Plinko Tickets',
    spinning: 'Spinning...',
    congratulations: 'Congratulations!',
    totalSpins: 'Total Spins Played',
    nextFreeSpinIn: 'Next free spin in',
    freeSpinReady: 'Free Spin Ready!',
    cooldownWaiting: 'Free spin ready in',
  },
}
