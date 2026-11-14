export default {
  apiError: {
    // Unhandled Error Page
    title: 'Algo Salió Mal',
    ok: 'OK',
    serviceUnavailable: 'El servicio no está disponible.',
  },

  validation: {
    title: 'Algo Salió Mal',
    rules: {
      required: 'El Campo {key} Es Requerido',
      email: 'El Campo {key} Debe Ser Una Dirección E-mail',
      match: 'El Campo {key} Y {param} Deben Ser Idénticos',
      passwordPolicy:
        'El Campo {key} debe tener al menos 8 carácteres, tener al menos 4 carácteres únicos, tener al menos un carácter no alfanumérico, al menos una letra minúscula, al menos una letra mayúscula y al menos un dígito.',
      usernamePolicy:
        'El Campo {key} debe contener solamente A-Z 0-9 y ` _ - y tener entre 4 y 12 carácteres.',
    },
  },

  fp: {
    title: 'Inicio',
    ladder: 'Clasificación',
    community: 'Únete A Nuestro Servidor De Discord',
    login: 'Iniciar Sesión',
    playNow: 'Registrarse',
    playWith: 'Juega HoN Con {amount} Personas Ahora',
    register: 'Registrarse',
    playersOnline: 'Jugadores En Línea',
    registeredAccounts: 'Cuentas Registradas',
    matchesInProgress: 'Partidas En Progreso',
    featuredStreamer: 'Transmisión Destacada',
    showFestivities: 'Activar Tema Estacional',
    timeline: {
      title: 'Cronología de Project KONGOR',
      a: {
        date: '13 de Diciembre de 2021',
        title: 'El Fin De HoN Es Anunciado',
        text: 'Frostburn anuncia que el juego cerrará el 20 de Junio de 2022.',
      },
      b: {
        date: '13 de Diciembre de 2021',
        title: 'El Desarrollo De Project KONGOR Comienza',
        text: 'La captura de paquetes comienza y es lanzada la primera prueba de concepto. La prueba de concepto consistía en iniciar sesión y jugar Plinko.',
      },
      c: {
        date: '20 de Junio de 2022',
        title: 'Cierran Los Servidores Oficiales De HoN',
        text: 'El juego fue lanzado el 12 de Mayo de 2010, funcionando por poco más de 13 años, antes de finalmente ser cerrado.',
      },
      d: {
        date: '5 de Agosto de 2022',
        title: 'Project KONGOR Entra En Pruebas De Beta Abierta',
        text: 'Project KONGOR invita públicamente a todos los jugadores a empezar a usar el servicio.',
      },
      e: {
        date: '5 de Enero de 2023',
        title: 'Project KONGOR Alcanza Sus Primeros Mayores Logros',
        text: 'Un gran total de 1 000 000 de partidas jugadas desde Agosto de 2022.',
      },
    },
  },

  ladder: {
    title: 'Clasificación',
    gameModes: {
      caldavar: 'Caldavar',
      caldavarCasual: 'Caldavar (Casual)',
      midwars: 'Midwars',
    },
    subTitle: 'Top {x} Jugadores Clasificados Para {gameMode}',
    search: 'Buscar',
    mmr: '{rating} MMR',
    gamesPlayed: 'Partidas',
    winRate: 'Tasa de Victorias',
    KDA: 'K/D/A',
    GPM: 'GPM',
    APM: 'APM',
  },

  login: {
    // Login Page
    title: 'Inicio de Sesión',
    username: 'Usuario',
    password: 'Contraseña',
    passwordMatch: 'Repetir Contraseña',
    forgotPassword: 'Contraseña Olvidada',
    login: 'Iniciar Sesión',
    youMustBeAuthed: 'Debes Iniciar Sesión Para Ver Esta Página',
    youAreLoggedOut: 'Has Cerrado Sesión',
    emailAddress: 'Dirección Email',
    forgotPasswordSubmit: 'Enviar Email Para Cambiar Contraseña',
    changePasswordAction: 'Cambiar Contraseña',
    passwordRequestResetSuccessTitle:
      'Solicitud De Cambio De Contraseña Enviada',
    passwordRequestResetSuccessMessage:
      'Checa El Email Enviado A "{email}" Para Más Instrucciones',
    changePasswordTitle: 'Cambia Tu Contraseña',
    changePasswordSuccessTitle: 'Tu Contraseña Ha Sido Cambiada',
    changePasswordSuccessMessage: 'Tu Contraseña Ha Sido Cambiada Con Éxito',
  },

  register: {
    // Registration Page
    title: 'Registro',
    username: 'Usuario',
    password: 'Contraseña',
    passwordMatch: 'Repetir Contraseña',
    emailAddress: 'Dirección Email',
    emailAddressMatch: 'Repetir Dirección Email',
    registerAction: 'Registrarse',
    registerSuccessTitle: 'Email Registrado',
    registerSuccessfulMessage:
      'Checa El Email Enviado A "{email}" Para Más Instrucciones',
    completeRegisterAction: 'Completar Registro',
    completeYourRegistrationTitle: 'Completar Registro',
    registerCompleteSuccessTitle: 'Tu Cuenta Ha Sido Creada',
    registerCompleteSuccessMessage:
      'Si no tienes el cliente de HoN ya instalado y necesitas ayuda con eso, pregunta en la comunidad. Un buen lugar para empezar sería nuestro servidor de Discord.',
    joinOurDiscord: 'Únete A Nuestro Servidor De Discord',
  },

  dashboard: {
    // Account Page
    title: 'Cuenta',
    mySettingsBtn: 'Ajustes de Cuenta',
    logout: 'Cerrar Sesión',
    welcome: 'Bienvenido, {name}',
    menuTitles: {
      gmTools: 'Herramientas de Game Master',
      sgmTools: 'Herramientas de Senior Game Master',
      adminTools: 'Administrator tools',
    },
    youAreNotAllowedHere:
      'No puedes ver esta página. Lo siento, si crees que es una equivocación, pregúntale a un administrador.',

    mySettings: {
      // Account Settings Sub-Page
      title: 'Ajustes de Cuenta',

      changeEmail: {
        // Update Email Tab
        title: 'Cambiar Dirección Email',
        emailNotVerified: 'Dirección Email No Verificada',
        emailVerified: 'Dirección Email Verificada',
        emailAddress: 'Ingresar Nueva Dirección Email',
        emailAddressMatch: 'Repetir Nueva Dirección Email',
        changeEmailAction: 'Cambiar Dirección Email',
        password: 'Ingresar Contraseña Actual',
        changeRequestSuccessTitle:
          'Solicitud De Cambio De Dirección Email Enviada',
        changeRequestSuccessMessage:
          'Checa El Email Enviado A "{email}" Para Más Instrucciones',
        emailChangedSuccessfully:
          'Tu Dirección Email Ha Sido Cambiada Con Éxito',
      },
    },

    cai: {
      title: 'Ícono de Cuenta Personalizado',
      selectFile: 'Haz clic aquí para seleccionar un archivo y continuar.',
      instructions:
        'Usa tu rueda de desplazamiento para acercarte y alejarte, y tu ratón para mover la plantilla.',
      uploadAction: 'Se ve bien, ¡súbelo!',
      uploadedWithSuccess:
        'Ícono de cuenta personalizado agregado, es posible que necesites reiniciar tu juego antes de poder verlo en tu bóveda.',
    },

    gm: {
      suspension: {
        title: 'Suspender Un Jugador',
        lookup: 'Buscar Cuenta',
        suspensionId: 'Suspensión #{id}',
        previousSuspensions: 'Suspensiones Previas ({count})',
        noBanMessage:
          'La cuenta "{accountName}" no tiene suspensiones previas.',
        accountName: 'Nombre de cuenta',
        duration: 'Duración',
        reason: 'Razón',
        internalDetails:
          'Detalles Internos (Sólamente Visibles Para (S)GMs Y Admins)',
        submit: 'Enviar',
        suspensionSuccessful:
          'Suspensión exitosa, la cuenta "{accountName}" ha sido baneada por "{duration}" con la razón "{reason}" (detalles internos: {internalDetails}).',
        year: 'Años',
        month: 'Meses',
        week: 'Semanas',
        day: 'Días',
        codeOfConduct: {
          title: 'Selecciona un tema para continuar.',
          chatVerbalAbuse: {
            subject: 'Abuso del Chat/Verbal',
            description:
              'Insultos hacia la raza, género, sexo, orientación sexual, país de origen o estatus socioeconómico, hostigamiento, etc. (Las provocaciones ligeras, decir groserías casualmente, o llamar a la gente noobs no son baneables).',
          },
          abilityAbuse: {
            subject: 'Abuso de Habilidades',
            description:
              'Usar la habilidad de un héroe para revelar su ubicación, intención de causar muertes usando una habilidad maliciosamente, cualquier otro uso de una habilidad de un héroe de manera maliciosa, etc.',
          },
          itemAbuse: {
            subject: 'Abuso de Objetos',
            description:
              'Destruir o vender objetos maliciosamente, colocar wards en la fuente, torres o campos de creeps neutrales, comprar objetos con la intención de darlos al equipo enemigo, etc.',
          },
          staffImpersonation: {
            subject: 'Hacerse pasar por Staff',
            description:
              'Cualquier jugador que declare ser un miembro del staff (Game Master, Developer, Server Host, Community Manager, Moderator, Collaborator, etc) o actúe de una manera que cause que alguién crea que es staff cuando no lo es.',
          },
          threats: {
            subject: 'Amenazas',
            description:
              'Desearle el mal, enfermedades, la muerte a un jugador, su familia o amigos, doxxear, etc.',
          },
          hateSpeech: {
            subject: 'Discurso de Odio',
            description:
              'Discursos de odio con respecto a la raza, sexo, orientación sexual, religión, país de origen, etc.',
          },
          intentToLose: {
            subject: 'Intención de Perder',
            description:
              'Morir intencionalmente (incluso con héroes desconectados), sabotear, revelar posiciones aliadas, rehusarse a participar (cuando un jugador se rehusa intencionalmente a participar durante momentos críticos del juego con intención maliciosa, como no defender la base cuando un jugador racional lo haría, no pushear con el equipo cuando el equipo te está pidiendo pushear con ellos, etc), evitar la detección AFK, griefear (robar last hits maliciosamente, participar en la partida solamente para conseguir kills, otras acciones de mala fé o comportamientos que molestan intencionalmente a otros jugadores o que tienen un impacto negativo en el resultado del juego, etc). (Robar una línea, elegir un mal héroe, jugar mal, o robar farm no es baneable a menos que tengas una razón de creer que tengan intención maliciosa. Estar en desacuerdo con la manera de jugar de un jugador pasará, mientras no se rehusen a participar con el equipo o griefeen entonces no es una ofensa baneable).',
          },
          cheating: {
            subject: 'Trampas',
            description:
              'Compartir cuentas, abusar de bugs, hackear, usar exploits, evadir bans, etc.',
          },
          inappropriate: {
            subject: 'Nombre de Cuenta/Ícono de Cuenta Inapropiado',
            description:
              'Cualquier jugador que cree una cuenta con un nombre de cuenta o que suba un ícono de cuenta personalizado que sea ofensivo, racista, homofóbico, sexista, vulgar, repulsivo, que contenga desnudez, etc. (+ Eliminación del Ícono de Cuenta si es aplicable + Cambio de Nombre Forzoso si es aplicable).',
          },
          statManipulation: {
            subject: 'Manipulación de Estadísticas',
            description:
              'Abandonar partidas intencionalmente para bajar tu MMR o incrementar el MMR de un jugador o equipo opuesto, perder/ganar/morir intencionalmente contra otro jugador o equipo para incrementar o reducir la tasa de victorias/MMR/tasa de KD, etc. (+ Suspensión de Sub Cuenta Permanente)',
          },
          serialLeaving: {
            subject: 'Salir en Serie',
            description:
              'Si un jugador abandona 3 o más partidas en 1 día (incluyendo sub-cuentas).',
          },
        },
      },
      rapm: {
        title: 'Gestión de RAP (reservado)',
      },
    },
    staff: {
      promotions: {
        title: 'Promover/Demover Game Master',
        promoteToGameMaster: 'Promover a Game Master',
        gmRoster: 'Lista de Game Masters',
        sgmRoster: 'Lista de Senior Game Masters',
        adminRoster: 'Lista de Administradores',
        userName: 'Usuario',
        promote: 'Promover',
        demote: 'Demover',
        promoteSuccess: '{userName} Promovido Exitosamente A Game Master',
        demoteSuccess: '{userName} Demovido Exitosamente De Game Master',
      },
      dataPoints: {
        title: 'Puntos de Datos',
        firstAccountName: 'Primer Nombre de Cuenta',
        secondAccountName: 'Segundo Nombre de Cuenta',
        lookup: 'Buscar',
        result: 'Resultado',
        preDelete: 'Preparar Supresión',
        preDeleteDescription:
          'Por favor haz clic en el nombre de cuenta del que quieres eliminar el punto de datos seleccionado.',
        removeSuccessful:
          '{category} "{dataPoint}" Eliminado Con Éxito De {accountName}',
      },
      suspensions: {
        title: 'Suspensiones',
        suspensionID: 'ID de Suspensión',
        overrideSuccessful: 'Suspensión {suspensionId} Anulada Exitosamente',
        override: 'Anular Suspensión',
        newDurationSuccessful:
          'Duración De La Suspensión {suspensionId} Exitosamente Cambiada A {newDuration} ',
        newDuration: 'Nueva Duración',
        changeDuration: 'Cambiar Duración de Suspensión',
        duration: 'Duración',
        year: 'Años',
        month: 'Meses',
        week: 'Semanas',
        day: 'Días',
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
