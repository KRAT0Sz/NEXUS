export default {
  apiError: {
    // Unhandled Error Page
    title: "Quelque Chose S'est Mal Passé",
    ok: 'OK',
    serviceUnavailable: "Le service n'est pas disponible.",
  },

  validation: {
    title: "Quelque Chose S'est Mal Passé",
    rules: {
      required: 'Le Champ {key} Est Requis',
      email: 'Le Champ {key} Doit Être Une Adresse E-mail',
      match: 'Le Champ {key} Et {param} Doivent Être Identiques',
      passwordPolicy:
        'Le Champ {key} doit avoir au moins 8 caractères, avoir au moins 4 caractères uniques, avoir au moins un caractère non alphanumérique, au moins une lettre minuscule, au moins une lettre majuscule et au moins un chiffre.',
      usernamePolicy:
        'Le Champ {key} doit contenir seulement A-Z 0-9 et ` _ - et avoir entre 4 et 12 caractères.',
    },
  },

  fp: {
    title: 'Acceuil',
    ladder: 'Classement',
    community: 'Rejoignez Notre Serveur Discord',
    login: 'Connexion',
    playNow: 'Enregistrement',
    playWith: 'Jouez HoN Avec {amount} Personnes Maintenant',
    register: 'Enregistrement',
    playersOnline: 'Joueurs En Ligne',
    registeredAccounts: 'Comptes Enregistrés',
    matchesInProgress: 'Parties En Cours',
    featuredStreamer: 'Live Présenté',
    showFestivities: 'Activer Thème Saisonnier',
    timeline: {
      title: 'Chronologie de Project KONGOR',
      a: {
        date: '13 Décembre 2021',
        title: 'La Fin De HoN Est Annoncée',
        text: 'Frostburn annonce que le jeu fermera le 20 Juin 2022.',
      },
      b: {
        date: '13 Décembre 2021',
        title: 'Le Dévelopement De Project KONGOR Commence',
        text: 'La capture de paquets commence et la première preuve de concept est lancée. La preuve de concept consistait à se connecter et jouer à Plinko.',
      },
      c: {
        date: '20 Juin 2022',
        title: 'Les Serveurs Officiels De HoN Ferment',
        text: 'Le jeu fut lancé le 12 Mai 2010, fonctionant pour un peu plus de 13 ans, avant de finalement être fermé.',
      },
      d: {
        date: '5 Août 2022',
        title: 'Project KONGOR Entre En Tests De Beta Ouverte',
        text: 'Project KONGOR invite publiquement à tous les joeurs à commencer à utiliser le service.',
      },
      e: {
        date: '5 Janvier 2023',
        title: 'Project KONGOR Atteint Ses Premiers Grands Achèvements',
        text: 'Un grand total de 1 000 000 de matchs joués depuis Août 2022.',
      },
    },
  },

  ladder: {
    title: 'Classement',
    gameModes: {
      caldavar: 'Caldavar',
      caldavarCasual: 'Caldavar (Casual)',
      midwars: 'Midwars',
    },
    subTitle: 'Top {x} Joueurs Classés Pour {gameMode}',
    search: 'Rechercher',
    mmr: '{rating} MMR',
    gamesPlayed: 'Parties',
    winRate: 'Taux de Victoire',
    KDA: 'K/D/A',
    GPM: 'GPM',
    APM: 'APM',
  },

  login: {
    // Login Page
    title: 'Connexion',
    username: 'Utilisateur',
    password: 'Mot de passe',
    passwordMatch: 'Répéter Mot de passe',
    forgotPassword: 'Mot de passe Oublié',
    login: 'Se Connecter',
    youMustBeAuthed: 'Vous Devez Vous Connecter Pour Voir Cette Page',
    youAreLoggedOut: 'Vous Vous Êtes Déconnecté',
    emailAddress: 'Adresse Email',
    forgotPasswordSubmit: 'Envoyer Email Pour Changer Mot De Passe',
    changePasswordAction: 'Changer Mot De Passe',
    passwordRequestResetSuccessTitle:
      'Demande De Changement De Mot De Passe Envoyée',
    passwordRequestResetSuccessMessage:
      'Vérifiez L\'Email Envoyé À "{email}" Pour Plus D\'Instructions',
    changePasswordTitle: 'Changer Votre Mot De Passe',
    changePasswordSuccessTitle: 'Votre Mot De Passe A Été Changé',
    changePasswordSuccessMessage: 'Votre Mot De Passe A Été Changé Avec Succès',
  },

  register: {
    // Registration Page
    title: 'Enregistrement',
    username: 'Utilisateur',
    password: 'Mot de passe',
    passwordMatch: 'Répéter Mot de passe',
    emailAddress: 'Adresse Email',
    emailAddressMatch: 'Répéter Adresse Email',
    registerAction: "S'Enregistrer",
    registerSuccessTitle: 'Email Enregistré',
    registerSuccessfulMessage:
      'Vérifiez L\'Email Envoyé À "{email}" Pour Plus D\'Instructions',
    completeRegisterAction: "Compléter L'Enregistrement",
    completeYourRegistrationTitle: "Compléter L'Enregistrement",
    registerCompleteSuccessTitle: 'Votre Compte A Été Créé',
    registerCompleteSuccessMessage:
      "Si vous n'avez pas le client de HoN déjà instalé et vous avez besoin d'aide avec ça, demandez dans la communauté. Un bon endroit pour commencer serait notre serveur Discord.",
    joinOurDiscord: 'Rejoignez Notre Serveur Discord',
  },

  dashboard: {
    // Account Page
    title: 'Compte',
    mySettingsBtn: 'Paramètres de Compte',
    logout: 'Déconnexion',
    welcome: 'Bienvenue, {name}',
    menuTitles: {
      gmTools: 'Outils de Game Master',
      sgmTools: 'Outils de Senior Game Master',
      adminTools: 'Administrator tools',
    },
    youAreNotAllowedHere:
      "Vous ne pouvez pas voir cette page. Désolé, si vous croyez que c'est faux, demandez à un administrateur.",

    mySettings: {
      // Account Settings Sub-Page
      title: 'Paramètres de Compte',

      changeEmail: {
        // Update Email Tab
        title: "Changer D'Adresse Email",
        emailNotVerified: 'Adresse Email Non Vérifiée',
        emailVerified: 'Adresse Email Vérifiée',
        emailAddress: 'Introduire Nouvelle Adresse Email',
        emailAddressMatch: 'Répéter Nouvelle Adresse Email',
        changeEmailAction: "Changer D'Adresse Email",
        password: 'Introduire Mot De Passe Actuel',
        changeRequestSuccessTitle:
          "Demande De Changement D'Adresse Email Envoyée",
        changeRequestSuccessMessage:
          'Vérifiez L\'Email Envoyé À "{email}" Pour Plus D\'Instructions',
        emailChangedSuccessfully:
          'Votre Adresse Email A Été Changée Avec Succès',
      },
    },

    cai: {
      title: 'Icône de Compte Personnalisée',
      selectFile: 'Cliquez ici pour sélectionner un fichier et continuer.',
      instructions:
        'Utilisez votre roulette de défilement pour agrandir et réduire, et votre souris pour déplacer le pochoir.',
      uploadAction: 'Cela semble bon, envoyez-le !',
      uploadedWithSuccess:
        'Icône de compte personnalisée ajoutée, vous devrez peut-être redémarrer votre jeu avant de pouvoir la voir dans votre coffre.',
    },

    gm: {
      suspension: {
        title: 'Suspendre Un Joueur',
        lookup: 'Chercher Compte',
        suspensionId: 'Suspension #{id}',
        previousSuspensions: 'Suspensions Précédentes ({count})',
        noBanMessage:
          'Le compte "{accountName}" n\'a pas de suspensions précédentes.',
        accountName: 'Nom de Compte',
        duration: 'Durée',
        reason: 'Raison',
        internalDetails:
          'Détails Internes (Seulement Visibles Aux (S)GMs Et Admins)',
        submit: 'Envoyer',
        suspensionSuccessful:
          'Suspension réussite, le compte "{accountName}" a été banni pour "{duration}" avec la raison "{reason}" (détails internes: {internalDetails}).',
        year: 'Ans',
        month: 'Mois',
        week: 'Semaines',
        day: 'Jours',
        codeOfConduct: {
          title: 'Sélectionnez un sujet pour continuer.',
          chatVerbalAbuse: {
            subject: 'Abus du Chat/Verbal',
            description:
              "Insultes envers la race, genre, sexe, orientation sexuelle, pays d'origine or statut socioéconomique, harcèlement, etc. (Les provocations légères, dire des gros mots casuellement, ou appeler les gens noobs ne sont pas bannissables).",
          },
          abilityAbuse: {
            subject: 'Abus de Capacités',
            description:
              "Utiliser la capacité d'un héros pour révéler son emplacement, intention de causer des morts en utilisant une capacité malicieusement, n'importe quel otre usage d'une capacité d'un héros de manière malicieuse, etc.",
          },
          itemAbuse: {
            subject: "Abus d'Objets",
            description:
              "Détruire ou vendre des objets malicieusement, placer des wards dans la fontaine, tours ou camps de creeps neutres, acheter des objets avec l'intention de les donner à l'équipe ennemie, etc.",
          },
          staffImpersonation: {
            subject: 'Poser comme Staff',
            description:
              "N'importe quel joueur qui déclare être un membre du staff (Game Master, Developer, Server Host, Community Manager, Moderator, Collaborator, etc) ou agit d'une manière qui cause que quelqu'un croit qu'il est staff alors qu'il ne l'est pas.",
          },
          threats: {
            subject: 'Menaces',
            description:
              'Souhaiter le mal, des maladies, la mort à un autre joueur, sa famille ou ses amis, doxxer, etc.',
          },
          hateSpeech: {
            subject: 'Discours de Haine',
            description:
              "Discours de haine concernant la race, sexe, orientation sexuelle, religion, pays d'origine, etc.",
          },
          intentToLose: {
            subject: 'Intention de Perdre',
            description:
              "Mourir intentionnellement (y compris avec des héros déconnectés), saboter, révéler les positions alliées, refuser de participer (quand un joueur refuse intentionnellement de participer durant des moments critiques du jeu avec une intention malicieuse, comme ne pas défendre la base quand un joueur rationnel le ferait, ne pas push avec l'équipe quand l'équipe te demande de push avec eux, etc), éviter la détection AFK, griefer (voler des last hits malicieusement, participer dans la partie seulement pour obtenir des kills, autres actions de mauvaise foi ou comportements qui embêtent intentionnellement d'autres joueurs ou qui ont un impact négatif sur le résultat du jeu, etc). (Voler une ligne, choisir un mauvais héros, mal jouer, ou voler du farm n'est pas bannissable à moins que vous ayez una raison de croire qu'ils aient une intention malicieuse. Ne pas être d'accord avec la manière de jouer d'un joueur arrivera, tandis qu'ils ne refusent pas de participer avec l'équipe ou griefent alors ce n'est pas une offense bannissable).",
          },
          cheating: {
            subject: 'Tricheries',
            description:
              'Partager des comptes, abuser de bugs, hacker, utiliser des exploits, évader des bans, etc.',
          },
          inappropriate: {
            subject: "Nom d'Utilisateur/Icône de Compte Inappropriée",
            description:
              "N'importe quel joueur qui crée un compte avec un nom d'utilisateur ou qui envoie une icône de compte personnalisée qui est offensive, raciste, homophobique, sexiste, vulgaire, répulsive, qui contient de la nudité, etc. (+ Enlèvement de l'Icône de Compte le cas échéant + Changement Forcé de Nom d'Utilisateur le cas échéant).",
          },
          statManipulation: {
            subject: 'Manipulation de Statistiques',
            description:
              "Abandonner des parties intentionnellement pour baisser votre MMR ou augmenter le MMR d'un joueur ou une équipe adverse, perdre/gagner/mourir intentionnellement contre un autre joueur ou équipe pour augmenter ou réduire le taux de victoire/MMR/taux de KD, etc. (+ Suspension de Sous Compte Permanente)",
          },
          serialLeaving: {
            subject: 'Quitter en Série',
            description:
              'Si un joueur abandonne 3 parties ou plus en 1 jour (y compris sous-comptes).',
          },
        },
      },
      rapm: {
        title: 'Gestion de RAP (réservé)',
      },
    },
    staff: {
      promotions: {
        title: 'Promouvoir/Destituer Game Master',
        promoteToGameMaster: 'Promouvoir à Game Master',
        gmRoster: 'Liste de Game Masters',
        sgmRoster: 'Liste de Senior Game Masters',
        adminRoster: "Liste d'Administrateurs",
        userName: 'Utilisateur',
        promote: 'Promouvoir',
        demote: 'Destituer',
        promoteSuccess: '{userName} Promu À GM Avec succès',
        demoteSuccess: '{userName} Destitué De GM Avec Succès',
      },
      dataPoints: {
        title: 'Points de Données',
        firstAccountName: 'Premier Nom de Compte',
        secondAccountName: 'Second Nom de Compte',
        lookup: 'Chercher',
        result: 'Résultat',
        preDelete: 'Préparer Suppression',
        preDeleteDescription:
          "S'il vous plaît cliquez sur le nom de compte du quel vous voulez éliminer le point de données selectionné.",
        removeSuccessful:
          '{category} "{dataPoint}" Éliminé Avec Succès De {accountName}',
      },
      suspensions: {
        title: 'Suspensions',
        suspensionID: 'ID de Suspension',
        overrideSuccessful: 'Suspension {suspensionId} Annulée Avec Succès',
        override: 'Annuler Suspension',
        newDurationSuccessful:
          'Durée De La Suspension {suspensionId} Changée Avec Succès À {newDuration} ',
        newDuration: 'Nouvelle Durée',
        changeDuration: 'Changer Durée de Suspension',
        duration: 'Durée',
        year: 'Ans',
        month: 'Mois',
        week: 'Semaines',
        day: 'Jours',
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
}
